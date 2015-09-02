using System.Diagnostics;
using System.Windows.Forms;
namespace Lol
{
    public static class Injected
    {
        public static void Main()
        {
            MessageBox.Show("Vydmantas noretu sito gerio" + "\n linkejimai is" + Process.GetCurrentProcess().ProcessName);

        }
    }
}
