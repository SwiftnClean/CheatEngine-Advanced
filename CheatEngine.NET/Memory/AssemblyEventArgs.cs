using System;

namespace CheatEngine.NET.Memory
{
    /// <summary>
    /// Event arguments for assembly progress events
    /// </summary>
    public class AssemblyProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the progress percentage
        /// </summary>
        public int ProgressPercentage { get; }
        
        /// <summary>
        /// Gets the status message
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// Gets the elapsed time in milliseconds
        /// </summary>
        public long ElapsedTimeMs { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyProgressEventArgs"/> class
        /// </summary>
        /// <param name="progressPercentage">The progress percentage</param>
        /// <param name="message">The status message</param>
        /// <param name="elapsedTimeMs">The elapsed time in milliseconds</param>
        public AssemblyProgressEventArgs(int progressPercentage, string message, long elapsedTimeMs)
        {
            ProgressPercentage = progressPercentage;
            Message = message;
            ElapsedTimeMs = elapsedTimeMs;
        }
    }
    
    /// <summary>
    /// Event arguments for assembly complete events
    /// </summary>
    public class AssemblyCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether the assembly was successful
        /// </summary>
        public bool Success { get; }
        
        /// <summary>
        /// Gets the result message
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// Gets the total time in milliseconds
        /// </summary>
        public long TotalTimeMs { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyCompleteEventArgs"/> class
        /// </summary>
        /// <param name="success">Whether the assembly was successful</param>
        /// <param name="message">The result message</param>
        /// <param name="totalTimeMs">The total time in milliseconds</param>
        public AssemblyCompleteEventArgs(bool success, string message, long totalTimeMs)
        {
            Success = success;
            Message = message;
            TotalTimeMs = totalTimeMs;
        }
    }
}
