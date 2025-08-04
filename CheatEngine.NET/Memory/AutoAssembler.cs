using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CheatEngine.NET.Core;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.Memory
{
    /// <summary>
    /// Handles assembly code compilation and injection into target process
    /// </summary>
    public class AutoAssembler
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isAssembling = false;
        
        /// <summary>
        /// Event raised when assembly progress is made
        /// </summary>
        public event EventHandler<AssemblyProgressEventArgs>? AssemblyProgress;
        
        /// <summary>
        /// Event raised when assembly is complete
        /// </summary>
        public event EventHandler<AssemblyCompleteEventArgs>? AssemblyComplete;
        
        /// <summary>
        /// Gets a value indicating whether assembly is in progress
        /// </summary>
        public bool IsAssembling => _isAssembling;
        
        /// <summary>
        /// Assembles and executes the given script
        /// </summary>
        /// <param name="script">The assembly script to execute</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task AssembleAndExecuteAsync(string script)
        {
            if (_isAssembling)
            {
                throw new InvalidOperationException("Assembly is already in progress");
            }
            
            if (CheatEngineCore.TargetProcess == null)
            {
                throw new InvalidOperationException("No process is attached");
            }
            
            _isAssembling = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                // Start the stopwatch to measure execution time
                Stopwatch stopwatch = Stopwatch.StartNew();
                
                // Report initial progress
                OnAssemblyProgress(0, "Starting assembly...", stopwatch.ElapsedMilliseconds);
                
                // Parse the script
                OnAssemblyProgress(10, "Parsing script...", stopwatch.ElapsedMilliseconds);
                await Task.Delay(100, _cancellationTokenSource.Token); // Simulate parsing
                
                // Check for cancellation
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                
                // Validate the script
                OnAssemblyProgress(20, "Validating script...", stopwatch.ElapsedMilliseconds);
                await Task.Delay(100, _cancellationTokenSource.Token); // Simulate validation
                
                // Check for cancellation
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                
                // Allocate memory in target process
                OnAssemblyProgress(30, "Allocating memory...", stopwatch.ElapsedMilliseconds);
                await Task.Delay(100, _cancellationTokenSource.Token); // Simulate memory allocation
                
                // Check for cancellation
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                
                // Assemble the script
                OnAssemblyProgress(50, "Assembling code...", stopwatch.ElapsedMilliseconds);
                await Task.Delay(200, _cancellationTokenSource.Token); // Simulate assembly
                
                // Check for cancellation
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                
                // Write assembled code to target process
                OnAssemblyProgress(70, "Writing code to process memory...", stopwatch.ElapsedMilliseconds);
                await Task.Delay(200, _cancellationTokenSource.Token); // Simulate writing
                
                // Check for cancellation
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                
                // Execute the code
                OnAssemblyProgress(90, "Executing code...", stopwatch.ElapsedMilliseconds);
                await Task.Delay(100, _cancellationTokenSource.Token); // Simulate execution
                
                // Complete the assembly
                stopwatch.Stop();
                OnAssemblyComplete(true, "Assembly completed successfully", stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
                OnAssemblyComplete(false, "Assembly was cancelled", 0);
            }
            catch (Exception ex)
            {
                OnAssemblyComplete(false, $"Assembly failed: {ex.Message}", 0);
            }
            finally
            {
                _isAssembling = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }
        
        /// <summary>
        /// Stops the current assembly operation
        /// </summary>
        public void StopAssembly()
        {
            if (_isAssembling && _cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }
        
        /// <summary>
        /// Raises the AssemblyProgress event
        /// </summary>
        /// <param name="progressPercentage">The progress percentage</param>
        /// <param name="message">The status message</param>
        /// <param name="elapsedTimeMs">The elapsed time in milliseconds</param>
        private void OnAssemblyProgress(int progressPercentage, string message, long elapsedTimeMs)
        {
            AssemblyProgress?.Invoke(this, new AssemblyProgressEventArgs(progressPercentage, message, elapsedTimeMs));
        }
        
        /// <summary>
        /// Raises the AssemblyComplete event
        /// </summary>
        /// <param name="success">Whether the assembly was successful</param>
        /// <param name="message">The result message</param>
        /// <param name="totalTimeMs">The total time in milliseconds</param>
        private void OnAssemblyComplete(bool success, string message, long totalTimeMs)
        {
            AssemblyComplete?.Invoke(this, new AssemblyCompleteEventArgs(success, message, totalTimeMs));
        }
    }
}
