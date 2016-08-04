using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ipc.Memory.Components.Test
{
    public struct Foo
    {
        public int X;
        public int Y;
    }

    class Program
    {
        static void Main(string[] args)
        {
            RefStructWrapper<Foo> wrapper = RefStructWrapper<Foo>.Connect("Foo");
            wrapper.Value = new Foo
            {
                X = 10,
                Y = 22
            };

            Console.WriteLine(wrapper.Value.X);
            Console.ReadLine();
        }
    }
}
