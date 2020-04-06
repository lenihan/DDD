using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CppSharp.Generators.CSharp;

using System.IO;
using CppSharp;
using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Generators;

namespace CPP2CSBindings
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleDriver.Run(new DDDCppSharp());
        }
    }

    public class DDDCppSharp : ILibrary
    {


        // Setup the driver options here.
        public void Setup(Driver driver)
        {
            var options = driver.Options;
            options.GeneratorKind = GeneratorKind.CSharp;
            var module = options.AddModule("Sample");
            //module.IncludeDirs.Add(@"C:\Users\David\src\DDD\Win32-OpenGL-DLL");
            module.Headers.Add(@"C:\Users\David\src\DDD\Win32-OpenGL-DLL\Win32-OpenGL-DLL.h");
            //module.LibraryDirs.Add(@"C:\Users\David\src\DDD\x64\Debug");
            //module.Libraries.Add("Win32-OpenGL-DLL.lib");

            //module.Headers.Add(@"C:\Program Files(x86)\Windows Kits\10\Include\10.0.18362.0\um\WinUser.h");
            module.IncludeDirs.Add(@"C:\Program Files(x86)\Windows Kits\10\Include\10.0.18362.0\um");
            module.Headers.Add("WinUser.h");
        }
        // Setup your passes here.
        public void SetupPasses(Driver driver) { }
        // Do transformations that should happen before passes are processed.
        public void Preprocess(Driver driver, ASTContext ctx) { }
        // Do transformations that should happen after passes are processed.
        public void Postprocess(Driver driver, ASTContext ctx) { }
    }
}