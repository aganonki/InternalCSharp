using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InjectCS
{
    public class Compiler
    {
        public Compiler(string code)
        {
            this.SourceCode = code;
        }

        public Compiler(string code, string entry)
        {
            this.SourceCode = code;
            this.EntryPoint = entry;
        }

        public void Run(string sourcecode)
        {
            ClearErrMsgs();

            string strRealSourceCode = sourcecode;

            Assembly assembly = CreateAssembly(strRealSourceCode);

            CallEntry(assembly, EntryPoint);

            DisplayErrorMsg();

        }
        public void Run()
        {
            ClearErrMsgs();

            Assembly assembly = CreateAssembly(SourceCode);

            CallEntry(assembly, EntryPoint);

            DisplayErrorMsg();

        }

        //Holds the main source code that will be dynamically compiled and executed
        internal string SourceCode = string.Empty;

        //Holds the method name that will act as the assembly entry point
        internal string EntryPoint = "Main";

        //A list of all assemblies that are referenced by the created assembly
        internal ArrayList References = new ArrayList();

        //A flag the determines if console window should remain open until user clicks enter
        internal bool WaitForUserAction = true;

        // compile the source, and create assembly in memory
        // this method code is mainly from jconwell, 
        // see http://www.codeproject.com/dotnet/DotNetScript.asp
        private Assembly CreateAssembly(string strRealSourceCode)
        {
            //Create an instance whichever code provider that is needed
            CodeDomProvider codeProvider =  new CSharpCodeProvider();

            //create the language specific code compiler
            ICodeCompiler compiler = codeProvider.CreateCompiler();

            //add compiler parameters
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/target:library";
            // you can add /optimize
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            compilerParams.IncludeDebugInformation = false;

            // add some basic references
            compilerParams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("System.Data.dll");
            compilerParams.ReferencedAssemblies.Add("System.Drawing.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xml.dll");
            compilerParams.ReferencedAssemblies.Add("System.Windows.Forms.dll");

            //actually compile the code
            CompilerResults results = compiler.CompileAssemblyFromSource(
                    compilerParams,
                    strRealSourceCode);

            //get a hold of the actual assembly that was generated
            Assembly generatedAssembly = results.CompiledAssembly;

            //return the assembly
            return generatedAssembly;
        }

        private void CallEntry(Assembly assembly, string entryPoint)
        {
            try
            {
                //Use reflection to call the static Main function
                Module[] mods = assembly.GetModules(false);
                Type[] types = mods[0].GetTypes();

                foreach (Type type in types)
                {
                    MethodInfo mi = type.GetMethod(entryPoint,
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (mi != null)
                    {
                        if (mi.GetParameters().Length == 1)
                        {
                            if (mi.GetParameters()[0].ParameterType.IsArray)
                            {
                                string[] par = new string[1]; // if Main has string [] arguments
                                mi.Invoke(null, par);
                            }
                        }
                        else
                        {
                            mi.Invoke(null, null);
                        }
                        return;
                    }

                }

                LogErrMsgs("Engine could not find the public static " + entryPoint);
            }
            catch (Exception ex)
            {
                LogErrMsgs("Error:  An exception occurred", ex);
            }

        }

        internal StringBuilder errMsg = new StringBuilder();
        internal void LogErrMsgs(string customMsg)
        {
            LogErrMsgs(customMsg, null);
        }

        internal void LogErrMsgs(string customMsg, Exception ex)
        {
            //put the error message into builder
            errMsg.Append("\r\n").Append(customMsg).Append("\r\n");

            //get all the exceptions and add their error messages
            while (ex != null)
            {
                errMsg.Append("\t").Append(ex.Message).Append("\r\n");
                ex = ex.InnerException;
            }
        }

        internal void ClearErrMsgs()
        {
            errMsg.Remove(0, errMsg.Length);
        }

        internal void DisplayErrorMsg()
        {
            if (errMsg.Length > 0)
            {
                MessageBox.Show(errMsg.ToString());
            }
        }
    }
}
