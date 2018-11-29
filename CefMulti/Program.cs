using System;
using System.Windows.Forms;

namespace CefMulti
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
            Application.Run(new FrmMain(true));
            // Application.Run(new FrmSplashScreen());
            /*var splashScreen = new FrmSplashScreen();
            splashScreen.Show();*/
            // Application.Run(new FrmAdminLogin());
        }
    }
}
