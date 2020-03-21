using System;
using System.Runtime.InteropServices;

namespace DotNetCore_OpenGL
{
    class Program
    {
        //[DllImport("Win32-OpenGL-DLL.dll")]
        [DllImport("C:/Users/David/src/DDD/x64/Debug/Win32-OpenGL-DLL.dll")]
        public static extern int main();

        static void Main(string[] args)
        {
            main();
        }
    }
}
