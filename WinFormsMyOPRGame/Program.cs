using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace MyOPRGame
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 form = new Form1();
            form.Icon = Properties.Resources.Icon;
            Drawer drawer = new Drawer(form);
            Game newGame = new Game(drawer);

            form.Show();
            Application.Run(form);
        }
    }
}
