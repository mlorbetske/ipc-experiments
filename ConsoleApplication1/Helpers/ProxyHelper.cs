using ConsoleApplication1.Contracts;
using ConsoleApplication1.Data;
using ConsoleApplication1.Proxies;
using Microsoft.SharedMemIpc.Helper;

namespace ConsoleApplication1.Helpers
{
    public static unsafe class ProxyHelper
    {
        public static IFoo GetFoo(this Ipc ipc, string name)
        {
            Ipc<Foo> wrapper = ipc.GetObject<Foo>(name);
            return wrapper.GetProxy();
        }

        public static IFoo GetProxy(this Ipc<Foo> wrapper)
        {
            Foo* linked = Memory.Link<Foo>(wrapper.Handle);
            return new FooProxy(linked);
        }

        public static IPrimitiveWrapper<int> GetInt32(this Ipc ipc, string name)
        {
            Ipc<int> wrapper = ipc.GetObject<int>(name);
            return wrapper.GetProxy();
        }

        public static IPrimitiveWrapper<int> GetProxy(this Ipc<int> wrapper)
        {
            int* linked = Memory.Link<int>(wrapper.Handle);
            return new IntProxy(linked);
        }
    }
}