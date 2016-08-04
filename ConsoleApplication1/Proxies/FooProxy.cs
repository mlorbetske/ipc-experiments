using ConsoleApplication1.Contracts;
using ConsoleApplication1.Data;

namespace ConsoleApplication1.Proxies
{
    internal unsafe class FooProxy : IFoo
    {
        private readonly Foo* _val;

        public FooProxy(Foo* val)
        {
            _val = val;
        }

        public long X
        {
            get { return _val->X; }
            set { _val->X = value; }
        }

        public long Y
        {
            get { return _val->Y; }
            set { _val->Y = value; }
        }

        public override string ToString()
        {
            return _val->ToString();
        }
    }
}