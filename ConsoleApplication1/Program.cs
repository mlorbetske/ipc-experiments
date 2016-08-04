using System;
using System.Diagnostics;
using ConsoleApplication1.Contracts;
using ConsoleApplication1.Helpers;

namespace ConsoleApplication1
{
    public class Program
    {
        private static IFoo _foo;

        public static void Main(string[] args)
        {
            Console.Title = Process.GetCurrentProcess().Id.ToString();
            Ipc scope = Ipc.Scope("Demo");
            Guid allBinding = scope.Bind("MyEvent", DoThing);
            string targettedEvent = "MyEvent" + Process.GetCurrentProcess().Id;
            scope.Bind("cls" + Process.GetCurrentProcess().Id, Console.Clear);
            Guid binding = scope.Bind(targettedEvent, DoThing);
            IFoo foo = scope.GetFoo("Foo");
            _foo = foo;

            while (true)
            {
                string cmd = Console.ReadLine();

                string v;
                switch (cmd)
                {
                    case "r":
                        scope.Call("MyEvent");
                        break;
                    case "d":
                        scope.Unbind(allBinding);
                        break;
                    case "rr":
                        allBinding = scope.Bind("MyEvent", DoThing);
                        break;
                    case "x":
                        v = Console.ReadLine();

                        if (v == null)
                        {
                            break;
                        }

                        foo.X = uint.Parse(v);
                        break;
                    case "y":
                        v = Console.ReadLine();

                        if (v == null)
                        {
                            break;
                        }

                        foo.Y = uint.Parse(v);
                        break;
                    case "q":
                        scope.Unbind(allBinding);
                        scope.Unbind(binding);
                        Environment.Exit(0);
                        return;
                    default:
                        if (cmd != null && cmd.StartsWith("msg:"))
                        {
                            scope.Call(cmd.Substring(4));
                        }
                        break;
                }
            }
        }

        private static void DoThing()
        {
            Console.WriteLine(_foo);
        }
    }
}
