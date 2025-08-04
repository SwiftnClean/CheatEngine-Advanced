using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CheatEngine.NET.Utils
{
    /// <summary>
    /// Log level enumeration
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug level for detailed information
        /// </summary>
        Debug,
        
        /// <summary>
        /// Info level for general information
        /// </summary>
        Info,
        
        /// <summary>
        /// Warning level for potential issues
        /// </summary>
        Warning,
        
        /// <summary>
        /// Error level for errors
        /// </summary>
        Error,
        
        /// <summary>
        /// Critical level for critical errors
        /// </summary>
        Critical
    }
    
    /// <summary>
    /// Provides logging functionality for the application
    /// </summary>
    public static class Logger
    {
        private static readonly object _lockObject = new object();
        private static readonly List<LogEntry> _logEntries = new List<LogEntry>();
        private static readonly List<ILogListener> _listeners = new List<ILogListener>();
        private static string _logFilePath = string.Empty;
        private static bool _isInitialized = false;
        private static LogLevel _minimumLogLevel = LogLevel.Info;
        
        /// <summary>
        /// Gets the log entries
        /// </summary>
        public static IReadOnlyList<LogEntry> LogEntries => _logEntries.AsReadOnly();
        
        /// <summary>
        /// Gets or sets the minimum log level
        /// </summary>
        public static LogLevel MinimumLogLevel
        {
            get => _minimumLogLevel;
            set => _minimumLogLevel = value;
        }
        
        /// <summary>
        /// Initializes the logger
        /// </summary>
        /// <param name="logFilePath">The path to the log file</param>
        /// <param name="minimumLogLevel">The minimum log level</param>
        public static void Initialize(string logFilePath, LogLevel minimumLogLevel = LogLevel.Info)
        {
            lock (_lockObject)
            {
                _logFilePath = logFilePath;
                _minimumLogLevel = minimumLogLevel;
                
                // Create log directory if it doesn't exist
                string logDirectory = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                // Add file logger
                AddListener(new FileLogListener(logFilePath));
                
                _isInitialized = true;
                
                Log("Logger initialized", LogLevel.Info);
            }
        }
        
        /// <summary>
        /// Adds a log listener
        /// </summary>
        /// <param name="listener">The listener to add</param>
        public static void AddListener(ILogListener listener)
        {
            lock (_lockObject)
            {
                if (!_listeners.Contains(listener))
                {
                    _listeners.Add(listener);
                }
            }
        }
        
        /// <summary>
        /// Removes a log listener
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        public static void RemoveListener(ILogListener listener)
        {
            lock (_lockObject)
            {
                _listeners.Remove(listener);
            }
        }
        
        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="level">The log level</param>
        public static void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (level < _minimumLogLevel)
            {
                return;
            }
            
            LogEntry entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };
            
            lock (_lockObject)
            {
                // Add to internal list
                _logEntries.Add(entry);
                
                // Notify listeners
                foreach (var listener in _listeners)
                {
                    try
                    {
                        listener.LogMessage(entry);
                    }
                    catch
                    {
                        // Ignore errors in listeners
                    }
                }
            }
        }
        
        /// <summary>
        /// Logs an exception
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">An optional message</param>
        /// <param name="level">The log level</param>
        public static void LogException(Exception exception, string message = "", LogLevel level = LogLevel.Error)
        {
            if (level < _minimumLogLevel)
            {
                return;
            }
            
            StringBuilder sb = new StringBuilder();
            
            if (!string.IsNullOrEmpty(message))
            {
                sb.AppendLine(message);
            }
            
            sb.AppendLine($"Exception: {exception.GetType().Name}");
            sb.AppendLine($"Message: {exception.Message}");
            sb.AppendLine($"Stack Trace: {exception.StackTrace}");
            
            if (exception.InnerException != null)
            {
                sb.AppendLine("Inner Exception:");
                sb.AppendLine($"Type: {exception.InnerException.GetType().Name}");
                sb.AppendLine($"Message: {exception.InnerException.Message}");
                sb.AppendLine($"Stack Trace: {exception.InnerException.StackTrace}");
            }
            
            Log(sb.ToString(), level);
        }
        
        /// <summary>
        /// Clears all log entries
        /// </summary>
        public static void Clear()
        {
            lock (_lockObject)
            {
                _logEntries.Clear();
            }
        }
    }
    
    /// <summary>
    /// Represents a log entry
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Gets or sets the timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Gets or sets the log level
        /// </summary>
        public LogLevel Level { get; set; }
        
        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the thread ID
        /// </summary>
        public int ThreadId { get; set; }
        
        /// <summary>
        /// Gets the formatted log message
        /// </summary>
        public string FormattedMessage => $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level}] [Thread {ThreadId}] {Message}";
    }
    
    /// <summary>
    /// Interface for log listeners
    /// </summary>
    public interface ILogListener
    {
        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="entry">The log entry</param>
        void LogMessage(LogEntry entry);
    }
    
    /// <summary>
    /// File log listener
    /// </summary>
    public class FileLogListener : ILogListener
    {
        private readonly string _filePath;
        private readonly object _lockObject = new object();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogListener"/> class
        /// </summary>
        /// <param name="filePath">The path to the log file</param>
        public FileLogListener(string filePath)
        {
            _filePath = filePath;
        }
        
        /// <summary>
        /// Logs a message to a file
        /// </summary>
        /// <param name="entry">The log entry</param>
        public void LogMessage(LogEntry entry)
        {
            lock (_lockObject)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(_filePath, true))
                    {
                        writer.WriteLine(entry.FormattedMessage);
                    }
                }
                catch
                {
                    // Ignore errors when writing to log file
                }
            }
        }
    }
    
    /// <summary>
    /// Console log listener
    /// </summary>
    public class ConsoleLogListener : ILogListener
    {
        /// <summary>
        /// Logs a message to the console
        /// </summary>
        /// <param name="entry">The log entry</param>
        public void LogMessage(LogEntry entry)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            
            // Set color based on log level
            switch (entry.Level)
            {
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }
            
            Console.WriteLine(entry.FormattedMessage);
            
            // Restore original color
            Console.ForegroundColor = originalColor;
        }
    }
}
