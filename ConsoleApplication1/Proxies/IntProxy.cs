using ConsoleApplication1.Contracts;

namespace ConsoleApplication1.Proxies
{
    internal unsafe class IntProxy : IPrimitiveWrapper<int>
    {
        private int* _val;

        public IntProxy(int* val)
        {
            _val = val;
        }

        public static implicit operator int(IntProxy v)
        {
            return *v._val;
        }

        public int Value
        {
            get { return *_val; }
            set { *_val = value; }
        }
    }
}