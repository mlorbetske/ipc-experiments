using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using Microsoft.SharedMemIpc.Helper;

namespace ConsoleApplication1
{
    public class Ipc
    {
        private readonly string _name;
        private Dictionary<Guid, CancellationTokenSource> _eventMappings = new Dictionary<Guid, CancellationTokenSource>();

        public Ipc(string name)
        {
            _name = name;
        }

        public string Name => _name;

        public static Ipc Scope(string name)
        {
            return new Ipc(name);
        }

        public Ipc<T> GetObject<T>(string name)
            where T : struct
        {
            return Ipc<T>.CreateOrAttach($"{_name}:obj:{name}");
        }

        public Mutex GetMutex(string name)
        {
            string qualifiedName = $@"Global\{_name}:mutex:{name}";
            bool created;
            return new Mutex(false, qualifiedName, out created);
        }

        public IDisposable WaitOn(string name)
        {
            return new MutexWaiter(GetMutex(name));
        }

        public IDisposable WaitOn(string name, int timeout)
        {
            return new MutexWaiter(GetMutex(name), timeout);
        }

        private sealed class MutexWaiter : IDisposable
        {
            private readonly Mutex _mutex;

            public MutexWaiter(Mutex mutex)
            {
                _mutex = mutex;
                if (!_mutex.WaitOne())
                {
                    throw new TimeoutException();
                }
            }

            public MutexWaiter(Mutex mutex, int timeout)
            {
                _mutex = mutex;

                if (!_mutex.WaitOne(timeout))
                {
                    throw new TimeoutException();
                }
            }

            public void Dispose()
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
        }

        public void Call(string name)
        {
            string qualifiedName = $@"Global\{_name}:events:{name}";
            bool created;
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset, qualifiedName, out created);
            handle.Set();
            handle.Reset();
        }

        public Guid Bind(string name, Action run)
        {
            string qualifiedName = $@"Global\{_name}:events:{name}";
            bool created;
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset, qualifiedName, out created);
            Guid evt = Guid.NewGuid();
            CancellationTokenSource cts = new CancellationTokenSource();
            _eventMappings[evt] = cts;
            CancellationToken token = cts.Token;

            new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    handle.WaitOne();

                    if (!token.IsCancellationRequested)
                    {
                        run();
                    }
                }
            }).Start();

            return evt;
        }

        public void Unbind(Guid eventHandle)
        {
            CancellationTokenSource cancellationTokenSource;
            if (_eventMappings.TryGetValue(eventHandle, out cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
            }
        }
    }

    public unsafe class Ipc<T> : IDisposable
        where T : struct
    {
        private readonly T _x;
        private bool _isDisposed;
        private readonly object _sync = new object();
        public readonly IntPtr Handle;
        private readonly MemoryMappedFile _file;

        internal Ipc(IntPtr start, MemoryMappedFile file)
        {
            _file = file;
            Handle = start;
        }

        public static Ipc<T> CreateOrAttach(string source)
        {
            int sz = Marshal.SizeOf<T>();
            MemoryMappedFile file;
            bool isOwner = true;

            try
            {
                file = MemoryMappedFile.CreateNew(source, sz, MemoryMappedFileAccess.ReadWrite);
            }
            catch
            {
                file = MemoryMappedFile.OpenExisting(source, MemoryMappedFileRights.ReadWrite);
                isOwner = false;
            }

            var acc = file.CreateViewAccessor();
            acc.SafeMemoryMappedViewHandle.Initialize((ulong)sz);
            var hndl = acc.SafeMemoryMappedViewHandle;
            byte* bPtr = null;
            hndl.AcquirePointer(ref bPtr);
            IntPtr ptr = new IntPtr(bPtr);

            if (isOwner)
            {
                Zero(ptr);
            }

            return new Ipc<T>(ptr, file);
        }

        public static Ipc<T> Attach(string source)
        {
            int sz = Marshal.SizeOf<T>();
            MemoryMappedFile file = MemoryMappedFile.OpenExisting(source, MemoryMappedFileRights.ReadWrite);
            var acc = file.CreateViewAccessor();
            acc.SafeMemoryMappedViewHandle.Initialize((ulong)sz);
            var hndl = acc.SafeMemoryMappedViewHandle;
            byte* bPtr = null;
            hndl.AcquirePointer(ref bPtr);
            IntPtr ptr = new IntPtr(bPtr);
            return new Ipc<T>(ptr, file);
        }

        public static Ipc<T> Create(string source)
        {
            int sz = Marshal.SizeOf<T>();
            MemoryMappedFile file = MemoryMappedFile.CreateOrOpen(source, sz, MemoryMappedFileAccess.ReadWrite);
            var acc = file.CreateViewAccessor();
            acc.SafeMemoryMappedViewHandle.Initialize((ulong)sz);
            var hndl = acc.SafeMemoryMappedViewHandle;
            byte* bPtr = null;
            hndl.AcquirePointer(ref bPtr);
            IntPtr ptr = new IntPtr(bPtr);
            Zero(ptr);
            return new Ipc<T>(ptr, file);
        }

        private static void Zero(IntPtr intPtr)
        {
            byte* b = (byte*)intPtr.ToPointer();
            for (int i = 0; i < Marshal.SizeOf<T>(); ++i, ++b)
            {
                *b = 0;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                lock (_sync)
                {
                    if (!_isDisposed)
                    {
                        _isDisposed = true;
                        _file.Dispose();
                    }
                }
            }
        }

        ~Ipc()
        {
            Dispose(false);
        }

        public T Value => *Memory.Link<T>(Handle);
    }
}
