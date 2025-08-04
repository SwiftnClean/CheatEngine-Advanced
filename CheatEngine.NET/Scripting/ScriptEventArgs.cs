using System;

namespace CheatEngine.NET.Scripting
{
    /// <summary>
    /// Event arguments for script output events
    /// </summary>
    public class ScriptOutputEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the output message
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// Gets the output type
        /// </summary>
        public ScriptOutputType OutputType { get; }
        
        /// <summary>
        /// Gets the output text
        /// </summary>
        public string Output => Message;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptOutputEventArgs"/> class
        /// </summary>
        /// <param name="message">The output message</param>
        /// <param name="outputType">The output type</param>
        public ScriptOutputEventArgs(string message, ScriptOutputType outputType)
        {
            Message = message;
            OutputType = outputType;
        }
    }
    
    /// <summary>
    /// Event arguments for script error events
    /// </summary>
    public class ScriptErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the error message
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// Gets the error source
        /// </summary>
        public string Source { get; }
        
        /// <summary>
        /// Gets the line number where the error occurred
        /// </summary>
        public int LineNumber { get; }
        
        /// <summary>
        /// Gets the error text
        /// </summary>
        public string Error => $"{Message} at line {LineNumber} in {Source}";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptErrorEventArgs"/> class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="source">The error source</param>
        /// <param name="lineNumber">The line number where the error occurred</param>
        public ScriptErrorEventArgs(string message, string source, int lineNumber)
        {
            Message = message;
            Source = source;
            LineNumber = lineNumber;
        }
    }
    
    /// <summary>
    /// Event arguments for script execution completed events
    /// </summary>
    public class ScriptExecutionCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether the script execution was successful
        /// </summary>
        public bool Success { get; }
        
        /// <summary>
        /// Gets the execution time in milliseconds
        /// </summary>
        public long ExecutionTimeMs { get; }
        
        /// <summary>
        /// Gets the error message if execution failed
        /// </summary>
        public string ErrorMessage { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptExecutionCompletedEventArgs"/> class
        /// </summary>
        /// <param name="success">Whether the script execution was successful</param>
        /// <param name="executionTimeMs">The execution time in milliseconds</param>
        /// <param name="errorMessage">The error message if execution failed</param>
        public ScriptExecutionCompletedEventArgs(bool success, long executionTimeMs, string errorMessage = null)
        {
            Success = success;
            ExecutionTimeMs = executionTimeMs;
            ErrorMessage = errorMessage ?? string.Empty;
        }
    }
    
    /// <summary>
    /// Script output type
    /// </summary>
    public enum ScriptOutputType
    {
        /// <summary>
        /// Standard output
        /// </summary>
        Standard,
        
        /// <summary>
        /// Error output
        /// </summary>
        Error,
        
        /// <summary>
        /// Debug output
        /// </summary>
        Debug
    }
}
