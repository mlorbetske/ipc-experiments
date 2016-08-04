using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ipc.Memory.Components
{
	public partial class SharedMemory
	{
	    public static int Foo()
	    {
	        return 0;
	    }
	}

    public class SharedMemory<T>
        where T : class
    {
        public void Thing()
        {
            //var x = SharedMemory.ClassRef<T>(IntPtr.Zero);
        }
    }
}
