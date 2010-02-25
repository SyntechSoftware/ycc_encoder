// YCC_GUI V 1.0
// (c) S. Manzhulovsky KIT-24B NTU "KPI" 2010
// create   15.01.2010
// modified 15.01.2010

using System;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
