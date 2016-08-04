using System.Runtime.InteropServices;
using ConsoleApplication1.Contracts;

namespace ConsoleApplication1.Data
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Foo : IFoo
    {
        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }

        public long X { get; set; }

        public long Y { get; set; }
    }
}