using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KeraLua;

namespace Scratch
{
    class Program
    {
        [DllImport("lua53", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern IntPtr lua_setupvalue(IntPtr luaState, int funcIndex, int n);

        private static List<Tuple<IntPtr, int, bool>> allocatedMemory = new List<Tuple<IntPtr, int, bool>>();

        static IntPtr MyAlloc(IntPtr ud, IntPtr ptr, UIntPtr oSize, UIntPtr nSize)
        {
            //Console.WriteLine("Entering MyAlloc");
            try
            {
                if (nSize == UIntPtr.Zero)
                {
                    //Console.WriteLine("Freeing {0:X}", (int)ptr);
                    allocatedMemory.RemoveAll(p => p.Item1 == ptr);
                    allocatedMemory.Add(Tuple.Create(ptr, (int)nSize, false));
                    Marshal.FreeHGlobal(ptr);
                    return IntPtr.Zero;
                }
                else if (ptr == IntPtr.Zero)
                {
                    //Console.WriteLine("Allocating {0} bytes", nSize);
                    var ret = Marshal.AllocHGlobal((int)nSize);
                    //Console.WriteLine("New block {0:X}", (int)ret);
                    allocatedMemory.Add(Tuple.Create(ret, (int)nSize, true));
                    return ret;
                }
                else
                {
                    unsafe
                    {
                        //Console.WriteLine("Resizing {2:X} from {0} to {1} bytes", oSize, nSize, (int)ptr);
                        var ret = Marshal.ReAllocHGlobal(ptr, new IntPtr(nSize.ToPointer()));
                        allocatedMemory.RemoveAll(p => p.Item1 == ptr);
                        allocatedMemory.Add(Tuple.Create(ret, (int)nSize, true));
                        //Console.WriteLine("New block {0:X}", (int)ret);
                        return ret;
                    }

                }
            } finally
            {
                //Console.WriteLine("Exiting MyAlloc");
            }
        }

        static void DumpAllocations()
        {
            foreach(var p in allocatedMemory.OrderBy(p => (int)p.Item1))
            {
                Console.WriteLine("{0:x}: {1} bytes (dead? {2})", (int)p.Item1, p.Item2, p.Item3);
            }
        }

        static void Main(string[] args)
        {
            using (var lua = new Lua())
            {
                var ptr = IntPtr.Zero;
                lua.SetAllocFunction(MyAlloc, ref ptr);
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
            Console.WriteLine();
            return 0;
        }

        private static void DoTest(Lua lua)
        {
            lua.Register("print", print);
            lua.DoString("test = \"foo\"");
            lua.LoadString("print(test)");

            Console.WriteLine("Stack: {0}", lua.GetTop());

            lua.DoString("return { test=\"bar\", print = print }");

            DumpAllocations();
            //lua.SetUpValue(-2, 1);
            var ret = lua_setupvalue(lua.Handle, -2, 1);

            if(ret != IntPtr.Zero)
            {
                Console.WriteLine("Got a valid pointer back! {0:x}", (int)ret);
                Console.WriteLine("About to try and marshall back into a string... wish me luck!");

                string res = Marshal.PtrToStringAuto(ret);

                Console.WriteLine("Success! {0}", res);
            }
            else
            {
                Console.WriteLine("Uh oh, ret is null...... this is about to be bad");
            }
            lua.Call(0, 0);

        }
    }
}
