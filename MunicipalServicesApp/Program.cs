using System;
using System.Windows.Forms;

namespace MunicipalServicesApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Final safety net: if anything unexpected slips through, show a friendly
            // message instead of letting the application crash with no explanation.
            Application.ThreadException += (sender, e) =>
            {
                MessageBox.Show(
                    "An unexpected error occurred:" + Environment.NewLine + e.Exception.Message,
                    "Municipal Services App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.Run(new MainMenuForm());
        }
    }
}
