using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern T* Link<T>(IntPtr val);
    }
}
