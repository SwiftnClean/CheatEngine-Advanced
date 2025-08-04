using System;
using System.Windows.Forms;
using CheatEngine.NET.GUI;

namespace CheatEngine.NET
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable visual styles for modern UI appearance
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Check for administrator privileges
            if (!Utils.SecurityUtils.IsAdministrator())
            {
                DialogResult result = MessageBox.Show(
                    "Cheat Engine works best with administrator privileges. Some features may not work without them.\n\n" +
                    "Do you want to restart the application as administrator?",
                    "Administrator Privileges Required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    // Restart as administrator
                    Utils.SecurityUtils.RestartAsAdministrator();
                    return;
                }
            }
            
            try
            {
                // Initialize core components
                Core.CheatEngineCore.Initialize();
                
                // Run the main form
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An unhandled exception occurred:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
