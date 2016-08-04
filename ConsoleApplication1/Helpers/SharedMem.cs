using System;
using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace ConsoleApplication1.Helpers
{
    public unsafe class SharedMem
    {
        private readonly string _mutexRoot;
        private readonly ConcurrentDictionary<string, IDisposable> _objectRegistry;
        private readonly string _objRoot;

        public SharedMem(string objRoot, string mutexRoot)
        {
            _objRoot = objRoot;
            _mutexRoot = mutexRoot;
            _objectRegistry = new ConcurrentDictionary<string, IDisposable>();
        }

        public static SharedMem Connect(string name)
        {
            string objRoot = $"{name}:objects:";
            string mutexRoot = $@"Global\{name}:Mutex:";
            return new SharedMem(objRoot, mutexRoot);
        }

        public T Object<T>(string name)
            where T : struct
        {
            IDisposable result = _objectRegistry.GetOrAdd(name, Demand<T>);
            ObjectWrapper<T> wrapper = (ObjectWrapper<T>)result;

            throw new NotImplementedException();
        }

        private IDisposable Demand<T>(string name)
            where T : struct
        {
            return ObjectWrapper<T>.Demand(_objRoot, name);
        }

        public void Release<T>(string name)
            where T : struct
        {
            IDisposable val;
            if (_objectRegistry.TryRemove(name, out val))
            {
                ObjectWrapper<T> value = val as ObjectWrapper<T>;
                value?.Dispose();
            }
        }

        private sealed class ObjectWrapper<T> : IDisposable
            where T : struct
        {
            private readonly IntPtr _data;
            private readonly MemoryMappedFile _file;
            private bool _isDisposed;
            private readonly object _sync = new object();

            private static int Size => Marshal.SizeOf<T>();

            private static void Zero(IntPtr intPtr)
            {
                byte* b = (byte*)intPtr.ToPointer();
                for (int i = 0; i < Marshal.SizeOf<T>(); ++i, ++b)
                {
                    *b = 0;
                }
            }

            public static ObjectWrapper<T> Demand(string prefix, string name)
            {
                string qualifiedName = prefix + name;
                MemoryMappedFile file;
                bool isOwner = true;

                try
                {
                    file = MemoryMappedFile.CreateNew(qualifiedName, Size, MemoryMappedFileAccess.ReadWrite);
                }
                catch
                {
                    file = MemoryMappedFile.OpenExisting(qualifiedName, MemoryMappedFileRights.ReadWrite);
                    isOwner = false;
                }

                return new ObjectWrapper<T>(file, isOwner);
            }

            private ObjectWrapper(MemoryMappedFile file, bool isOwner)
            {
                var acc = file.CreateViewAccessor();
                acc.SafeMemoryMappedViewHandle.Initialize((ulong)Size);
                var hndl = acc.SafeMemoryMappedViewHandle;
                byte* bPtr = null;
                hndl.AcquirePointer(ref bPtr);
                IntPtr ptr = new IntPtr(bPtr);
                _file = file;
                _data = ptr;

                if (isOwner)
                {
                    Zero(ptr);
                }
            }

            public T Value { get; }

            public void Dispose()
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
        }
    }
}