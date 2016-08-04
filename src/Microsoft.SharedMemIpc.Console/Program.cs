using System;
using System.Runtime.InteropServices;

namespace Microsoft.SharedMemIpc.Console
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Foo
    {
        public int X;
        public int Y;
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Microsoft.SharedMemIpc.Helper.Memory.Link<Foo>(IntPtr.Zero);
        }
    }
}
