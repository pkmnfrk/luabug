using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeraLua;

namespace Scratch
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var lua = new Lua())
            {
                DoTest(lua);
            }

            Console.ReadKey(true);
        }

        private static int print(IntPtr state)
        {
            Lua lua = Lua.FromIntPtr(state);
            for (int i = 1; i <= lua.GetTop(); i++)
            {
                var v = lua.ToString(i);
                if (i > 1)
                    Console.Write(' ');
                Console.Write(v);

            }
            return 0;
        }

        private static void DoTest(Lua lua)
        {
            lua.Register("print", print);
            lua.DoString("test = \"foo\"");
            lua.LoadString("print(test)");

            Console.WriteLine("Stack: {0}", lua.GetTop());

            lua.DoString("return { test=\"bar\", print = \"print\" }");
            lua.SetUpValue(-2, 1);
            lua.Call(0, 0);
        }
    }
}
