using RGiesecke.DllExport;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InjectCS
{
    public static class Main
    {
        static readonly Process ThisProcess = Process.GetCurrentProcess();

        [DllExport("DllMain", CallingConvention.Cdecl)] // Mark your members with this attribute to export them; if you don't - they won't get exported!
        public static void EntryPoint() // Note, member name does not have to match export name ("DllMain" in this case).
        {
            // Your hacks here. Note, you're inside the process now. You don't want to use WPM/RPM now, that's for external hacks. Instead use Marshal.Copy to read, and VirtualProtect(Ex) (for setting access rights, because you do not necessarily always have execute/read/write permission...; don't forget to restore old access rights after you have finished writing to a memory) + Marhsal.Copy to write.
            // You don't need to call OpenProcess or do other nasty calls, welcome to the managed world. Instead use ThisProcess.Handle ;)
            // Lets just write a text file on Desktop called "hello_from_X" (where X is the process name in which a managed DLL was injected into) for example, this will "signal" that injection went fine...
            var data = ThisProcess.Modules;
            var stringas = "";
            foreach (ProcessModule item in data)
                stringas += item.FileName+"\n";
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "hello_from_" + ThisProcess.ProcessName + ".txt"), stringas);

            MessageBox.Show(ThisProcess.ProcessName);
            Application.Run(new DynamicInput());
        }
    }
}
