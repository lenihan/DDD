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
            module.IncludeDirs.Add(@"C:\Users\David\src\DDD\Win32-OpenGL-DLL");
            module.Headers.Add("Win32-OpenGL-DLL.h");
            module.LibraryDirs.Add(@"C:\Users\David\src\DDD\x64\Debug");
            module.Libraries.Add("Win32-OpenGL-DLL.lib");
        }
        // Setup your passes here.
        public void SetupPasses(Driver driver) { }
        // Do transformations that should happen before passes are processed.
        public void Preprocess(Driver driver, ASTContext ctx) { }
        // Do transformations that should happen after passes are processed.
        public void Postprocess(Driver driver, ASTContext ctx) { }
    }
}