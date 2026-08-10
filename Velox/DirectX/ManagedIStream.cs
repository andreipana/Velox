using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Velox.DirectX
{
    // A minimal native-callable IStream implementation backed by an in-memory
    // byte[], so WIC can decode images loaded as .NET embedded resources
    // without a temp file or the shlwapi SHCreateMemStream helper. Read/Seek/
    // Stat are functionally implemented (all a read-only decode needs);
    // Write/SetSize/CopyTo/Revert/LockRegion/UnlockRegion/Clone are stubbed
    // since nothing here ever writes through a stream.
    //
    // Vtable layout matches COM's IStream exactly (IUnknown[0..2] +
    // ISequentialStream Read/Write[3..4] + IStream Seek..Clone[5..13]), same
    // rationale as WicVtbl: this is called directly by native code through a
    // raw vtable pointer, no CLR COM interop involved on either side — except
    // here *we* are the COM object, so the vtable holds pointers to
    // [UnmanagedCallersOnly] methods instead of being dispatched through.
    internal static unsafe class ManagedIStream
    {
        private const int VtblSlotCount = 14;
        private const int HeaderSize = 16; // vtblPtr (8) + GCHandle (8) — x64 only

        private const int S_OK = 0;
        private const int E_NOINTERFACE = unchecked((int)0x80004002);
        private const int E_NOTIMPL = unchecked((int)0x80004001);
        private const int STG_E_INVALIDFUNCTION = unchecked((int)0x80030001);

        private static readonly IntPtr* s_vtbl = BuildVtbl();

        private sealed class State
        {
            public byte[] Data = null!;
            public int Position;
            public int RefCount;
        }

        // A blittable mirror of the native STATSTG struct (objidl.h), used only
        // to write through the caller-supplied pStatstg pointer in Stat(). Field
        // types (IntPtr, not string, for pwcsName) keep this self-adjusting to
        // the process's actual pointer size via sizeof()/struct assignment,
        // instead of hardcoding x64 struct size/offsets as raw byte math.
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeStatstg
        {
            public IntPtr pwcsName;
            public uint   type;
            public ulong  cbSize;
            public ulong  mtime;
            public ulong  ctime;
            public ulong  atime;
            public uint   grfMode;
            public uint   grfLocksSupported;
            public Guid   clsid;
            public uint   grfStateBits;
            public uint   reserved;
        }

        // Returns an IStream* (refcount 1) wrapping a copy-free view over `data`.
        // Caller must WicVtbl.Release() it once done (typically right after
        // passing it to CreateDecoderFromStream — the decoder AddRefs its own
        // reference, so releasing ours immediately afterwards is correct).
        public static IntPtr Create(byte[] data)
        {
            var state = new State { Data = data };
            IntPtr obj = Marshal.AllocHGlobal(HeaderSize);
            ((IntPtr*)obj)[0] = (IntPtr)s_vtbl;
            ((IntPtr*)obj)[1] = GCHandle.ToIntPtr(GCHandle.Alloc(state));
            state.RefCount = 1;
            return obj;
        }

        private static State GetState(IntPtr pThis) =>
            (State)GCHandle.FromIntPtr(((IntPtr*)pThis)[1]).Target!;

        private static IntPtr* BuildVtbl()
        {
            IntPtr* v = (IntPtr*)Marshal.AllocHGlobal(sizeof(IntPtr) * VtblSlotCount);
            v[0]  = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
            v[1]  = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, uint>)&AddRef;
            v[2]  = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, uint>)&Release;
            v[3]  = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, byte*, uint, uint*, int>)&Read;
            v[4]  = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, byte*, uint, uint*, int>)&Write;
            v[5]  = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, long, uint, ulong*, int>)&Seek;
            v[6]  = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, ulong, int>)&SetSize;
            v[7]  = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, IntPtr, ulong, ulong*, ulong*, int>)&CopyTo;
            v[8]  = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, uint, int>)&Commit;
            v[9]  = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, int>)&Revert;
            v[10] = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, ulong, ulong, uint, int>)&LockRegion;
            v[11] = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, ulong, ulong, uint, int>)&UnlockRegion;
            v[12] = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, IntPtr, uint, int>)&Stat;
            v[13] = (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, IntPtr*, int>)&Clone;
            return v;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int QueryInterface(IntPtr pThis, Guid* riid, IntPtr* ppvObject)
        {
            if (*riid == WicGuids.IID_IUnknown || *riid == WicGuids.IID_ISequentialStream || *riid == WicGuids.IID_IStream)
            {
                // Inlined AddRef: [UnmanagedCallersOnly] methods can't call each other directly.
                Interlocked.Increment(ref GetState(pThis).RefCount);
                *ppvObject = pThis;
                return S_OK;
            }
            *ppvObject = IntPtr.Zero;
            return E_NOINTERFACE;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static uint AddRef(IntPtr pThis) => (uint)Interlocked.Increment(ref GetState(pThis).RefCount);

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static uint Release(IntPtr pThis)
        {
            GCHandle handle = GCHandle.FromIntPtr(((IntPtr*)pThis)[1]);
            var state = (State)handle.Target!;
            int count = Interlocked.Decrement(ref state.RefCount);
            if (count == 0)
            {
                handle.Free();
                Marshal.FreeHGlobal(pThis);
            }
            return (uint)count;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int Read(IntPtr pThis, byte* buffer, uint cb, uint* pcbRead)
        {
            var s = GetState(pThis);
            int available = Math.Max(0, s.Data.Length - s.Position);
            int toRead = (int)Math.Min(cb, (uint)available);
            if (toRead > 0)
            {
                fixed (byte* src = &s.Data[s.Position])
                    Buffer.MemoryCopy(src, buffer, cb, toRead);
                s.Position += toRead;
            }
            if (pcbRead != null) *pcbRead = (uint)toRead;
            return S_OK;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int Write(IntPtr pThis, byte* buffer, uint cb, uint* pcbWritten) => STG_E_INVALIDFUNCTION;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int Seek(IntPtr pThis, long dlibMove, uint dwOrigin, ulong* plibNewPosition)
        {
            var s = GetState(pThis);
            long newPos = dwOrigin switch
            {
                0 => dlibMove,                     // STREAM_SEEK_SET
                1 => s.Position + dlibMove,         // STREAM_SEEK_CUR
                2 => s.Data.Length + dlibMove,      // STREAM_SEEK_END
                _ => -1,
            };
            // Bound to the buffer's actual size *before* the narrowing cast below —
            // checking "newPos < 0" on the pre-cast long isn't enough, since a
            // huge positive newPos (e.g. from a malformed dlibMove) can wrap to a
            // negative int once cast, silently bypassing that check. Since we're a
            // fixed-size read-only backing store, there's no reason to support
            // (and no safe way to represent) a position past the end anyway.
            if (newPos < 0 || newPos > s.Data.Length) return STG_E_INVALIDFUNCTION;
            s.Position = (int)newPos; // safe: newPos is now within [0, Data.Length], which always fits in int
            if (plibNewPosition != null) *plibNewPosition = (ulong)newPos;
            return S_OK;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int SetSize(IntPtr pThis, ulong libNewSize) => E_NOTIMPL;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int CopyTo(IntPtr pThis, IntPtr pstm, ulong cb, ulong* pcbRead, ulong* pcbWritten) => E_NOTIMPL;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int Commit(IntPtr pThis, uint grfCommitFlags) => E_NOTIMPL;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int Revert(IntPtr pThis) => E_NOTIMPL;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int LockRegion(IntPtr pThis, ulong libOffset, ulong cb, uint dwLockType) => E_NOTIMPL;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int UnlockRegion(IntPtr pThis, ulong libOffset, ulong cb, uint dwLockType) => E_NOTIMPL;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int Stat(IntPtr pThis, IntPtr pStatstg, uint grfStatFlag)
        {
            var s = GetState(pThis);
            var statstg = default(NativeStatstg);
            statstg.type   = 2; // STGTY_STREAM
            statstg.cbSize = (ulong)s.Data.Length;
            *(NativeStatstg*)pStatstg = statstg;
            return S_OK;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int Clone(IntPtr pThis, IntPtr* ppstm)
        {
            if (ppstm != null) *ppstm = IntPtr.Zero;
            return E_NOTIMPL;
        }
    }
}
