using System;
using System.Diagnostics;
using System.Security.Principal;

namespace CheatEngine.NET.Utils
{
    /// <summary>
    /// Provides security-related utility functions
    /// </summary>
    public static class SecurityUtils
    {
        /// <summary>
        /// Checks if the current process is running with administrator privileges
        /// </summary>
        /// <returns>True if running as administrator, false otherwise</returns>
        public static bool IsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error checking administrator privileges: {ex.Message}", LogLevel.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Restarts the application with administrator privileges
        /// </summary>
        public static void RestartAsAdministrator()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                startInfo.Verb = "runas"; // Request elevation
                
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error restarting as administrator: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
