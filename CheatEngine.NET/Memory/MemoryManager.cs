using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CheatEngine.NET.Native;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.Memory
{
    /// <summary>
    /// Manages memory operations for the target process
    /// </summary>
    public class MemoryManager
    {
        private Process? _process;
        private IntPtr _processHandle = IntPtr.Zero;
        private bool _isInitialized = false;
        
        /// <summary>
        /// Gets a value indicating whether the memory manager is initialized
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Gets the target process
        /// </summary>
        public Process? Process => _process;
        
        /// <summary>
        /// Initializes the memory manager with a target process
        /// </summary>
        /// <param name="process">The target process</param>
        public void Initialize(Process process)
        {
            if (_isInitialized)
            {
                Cleanup();
            }
            
            _process = process;
            
            // Open process with all access rights
            _processHandle = WinAPI.OpenProcess(
                WinAPI.ProcessAccessFlags.All,
                false,
                _process.Id);
                
            if (_processHandle == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                Logger.Log($"Failed to open process handle. Error code: {error}", LogLevel.Error);
                throw new Exception($"Failed to open process handle. Error code: {error}");
            }
            
            _isInitialized = true;
            Logger.Log($"Memory manager initialized for process {_process.ProcessName} (PID: {_process.Id})");
        }
        
        /// <summary>
        /// Cleans up resources used by the memory manager
        /// </summary>
        public void Cleanup()
        {
            if (_processHandle != IntPtr.Zero)
            {
                WinAPI.CloseHandle(_processHandle);
                _processHandle = IntPtr.Zero;
            }
            
            _process = null;
            _isInitialized = false;
            
            Logger.Log("Memory manager cleaned up");
        }
        
        /// <summary>
        /// Reads memory from the target process
        /// </summary>
        /// <param name="address">The memory address to read from</param>
        /// <param name="size">The number of bytes to read</param>
        /// <returns>The bytes read from memory</returns>
        public byte[] ReadMemory(IntPtr address, int size)
        {
            CheckInitialized();
            
            byte[] buffer = new byte[size];
            int bytesRead = 0;
            
            bool success = WinAPI.ReadProcessMemory(
                _processHandle,
                address,
                buffer,
                size,
                ref bytesRead);
                
            if (!success || bytesRead != size)
            {
                int error = Marshal.GetLastWin32Error();
                Logger.Log($"Failed to read memory at 0x{address.ToInt64():X}. Error code: {error}", LogLevel.Error);
                throw new Exception($"Failed to read memory at 0x{address.ToInt64():X}. Error code: {error}");
            }
            
            return buffer;
        }
        
        /// <summary>
        /// Writes memory to the target process
        /// </summary>
        /// <param name="address">The memory address to write to</param>
        /// <param name="data">The bytes to write</param>
        /// <returns>True if the write was successful, false otherwise</returns>
        public bool WriteMemory(IntPtr address, byte[] data)
        {
            CheckInitialized();
            
            int bytesWritten = 0;
            
            bool success = WinAPI.WriteProcessMemory(
                _processHandle,
                address,
                data,
                data.Length,
                ref bytesWritten);
                
            if (!success || bytesWritten != data.Length)
            {
                int error = Marshal.GetLastWin32Error();
                Logger.Log($"Failed to write memory at 0x{address.ToInt64():X}. Error code: {error}", LogLevel.Error);
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Gets information about a memory region
        /// </summary>
        /// <param name="address">The memory address to query</param>
        /// <returns>Information about the memory region</returns>
        public WinAPI.MEMORY_BASIC_INFORMATION QueryMemory(IntPtr address)
        {
            CheckInitialized();
            
            WinAPI.MEMORY_BASIC_INFORMATION mbi = new WinAPI.MEMORY_BASIC_INFORMATION();
            int result = WinAPI.VirtualQueryEx(
                _processHandle,
                address,
                out mbi,
                Marshal.SizeOf(typeof(WinAPI.MEMORY_BASIC_INFORMATION)));
                
            if (result == 0)
            {
                int error = Marshal.GetLastWin32Error();
                Logger.Log($"Failed to query memory at 0x{address.ToInt64():X}. Error code: {error}", LogLevel.Error);
                throw new Exception($"Failed to query memory at 0x{address.ToInt64():X}. Error code: {error}");
            }
            
            return mbi;
        }
        
        /// <summary>
        /// Gets all memory regions in the target process
        /// </summary>
        /// <returns>A list of memory regions</returns>
        public List<MemoryRegion> GetMemoryRegions()
        {
            CheckInitialized();
            
            List<MemoryRegion> regions = new List<MemoryRegion>();
            IntPtr address = IntPtr.Zero;
            
            while (true)
            {
                WinAPI.MEMORY_BASIC_INFORMATION mbi;
                
                try
                {
                    mbi = QueryMemory(address);
                }
                catch
                {
                    // End of memory space
                    break;
                }
                
                // Add region to list if it's committed and not protected
                if (mbi.State == WinAPI.MemoryState.MEM_COMMIT &&
                    (mbi.Protect & WinAPI.MemoryProtection.PAGE_GUARD) == 0 &&
                    (mbi.Protect & WinAPI.MemoryProtection.PAGE_NOACCESS) == 0)
                {
                    regions.Add(new MemoryRegion
                    {
                        BaseAddress = mbi.BaseAddress,
                        Size = mbi.RegionSize.ToInt32(),
                        Protection = mbi.Protect,
                        Type = mbi.Type
                    });
                }
                
                // Move to next region
                address = new IntPtr(mbi.BaseAddress.ToInt64() + mbi.RegionSize.ToInt64());
                
                // Check if we've reached the end of the address space
                if (address.ToInt64() <= 0)
                    break;
            }
            
            return regions;
        }
        
        /// <summary>
        /// Reads a value of a specific type from memory
        /// </summary>
        /// <typeparam name="T">The type of value to read</typeparam>
        /// <param name="address">The memory address to read from</param>
        /// <returns>The value read from memory</returns>
        public T ReadValue<T>(IntPtr address) where T : struct
        {
            CheckInitialized();
            
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = ReadMemory(address, size);
            
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T))!;
            handle.Free();
            
            return result;
        }
        
        /// <summary>
        /// Writes a value of a specific type to memory
        /// </summary>
        /// <typeparam name="T">The type of value to write</typeparam>
        /// <param name="address">The memory address to write to</param>
        /// <param name="value">The value to write</param>
        /// <returns>True if the write was successful, false otherwise</returns>
        public bool WriteValue<T>(IntPtr address, T value) where T : struct
        {
            CheckInitialized();
            
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            handle.Free();
            
            return WriteMemory(address, buffer);
        }
        
        /// <summary>
        /// Reads a string from memory
        /// </summary>
        /// <param name="address">The memory address to read from</param>
        /// <param name="maxLength">The maximum length of the string</param>
        /// <param name="encoding">The encoding to use</param>
        /// <returns>The string read from memory</returns>
        public string ReadString(IntPtr address, int maxLength, Encoding encoding)
        {
            CheckInitialized();
            
            byte[] buffer = ReadMemory(address, maxLength);
            
            // Find null terminator
            int length = 0;
            while (length < buffer.Length && buffer[length] != 0)
            {
                length++;
            }
            
            return encoding.GetString(buffer, 0, length);
        }
        
        /// <summary>
        /// Writes a string to memory
        /// </summary>
        /// <param name="address">The memory address to write to</param>
        /// <param name="value">The string to write</param>
        /// <param name="encoding">The encoding to use</param>
        /// <returns>True if the write was successful, false otherwise</returns>
        public bool WriteString(IntPtr address, string value, Encoding encoding)
        {
            CheckInitialized();
            
            byte[] buffer = encoding.GetBytes(value + "\0");
            return WriteMemory(address, buffer);
        }
        
        /// <summary>
        /// Allocates memory in the target process
        /// </summary>
        /// <param name="size">The size of the memory to allocate</param>
        /// <param name="protection">The memory protection to use</param>
        /// <returns>The address of the allocated memory</returns>
        public IntPtr AllocateMemory(int size, WinAPI.MemoryProtection protection = WinAPI.MemoryProtection.PAGE_EXECUTE_READWRITE)
        {
            CheckInitialized();
            
            IntPtr address = WinAPI.VirtualAllocEx(
                _processHandle,
                IntPtr.Zero,
                (uint)size,
                WinAPI.AllocationType.MEM_COMMIT | WinAPI.AllocationType.MEM_RESERVE,
                protection);
                
            if (address == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                Logger.Log($"Failed to allocate memory. Error code: {error}", LogLevel.Error);
                throw new Exception($"Failed to allocate memory. Error code: {error}");
            }
            
            return address;
        }
        
        /// <summary>
        /// Frees memory in the target process
        /// </summary>
        /// <param name="address">The address of the memory to free</param>
        /// <returns>True if the memory was freed successfully, false otherwise</returns>
        public bool FreeMemory(IntPtr address)
        {
            CheckInitialized();
            
            bool success = WinAPI.VirtualFreeEx(
                _processHandle,
                address,
                0,
                WinAPI.FreeType.MEM_RELEASE);
                
            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                Logger.Log($"Failed to free memory at 0x{address.ToInt64():X}. Error code: {error}", LogLevel.Error);
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Checks if the memory manager is initialized
        /// </summary>
        private void CheckInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Memory manager is not initialized");
            }
        }
    }
    
    /// <summary>
    /// Represents a memory region in a process
    /// </summary>
    public class MemoryRegion
    {
        /// <summary>
        /// Gets or sets the base address of the region
        /// </summary>
        public IntPtr BaseAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the size of the region in bytes
        /// </summary>
        public int Size { get; set; }
        
        /// <summary>
        /// Gets or sets the memory protection of the region
        /// </summary>
        public WinAPI.MemoryProtection Protection { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the region
        /// </summary>
        public WinAPI.MemoryType Type { get; set; }
        
        /// <summary>
        /// Gets the end address of the region
        /// </summary>
        public IntPtr EndAddress => new IntPtr(BaseAddress.ToInt64() + Size);
        
        /// <summary>
        /// Gets a value indicating whether the region is readable
        /// </summary>
        public bool IsReadable => Protection != WinAPI.MemoryProtection.PAGE_NOACCESS &&
                                 Protection != WinAPI.MemoryProtection.PAGE_EXECUTE;
        
        /// <summary>
        /// Gets a value indicating whether the region is writable
        /// </summary>
        public bool IsWritable => (Protection & WinAPI.MemoryProtection.PAGE_READWRITE) != 0 ||
                                 (Protection & WinAPI.MemoryProtection.PAGE_WRITECOPY) != 0 ||
                                 (Protection & WinAPI.MemoryProtection.PAGE_EXECUTE_READWRITE) != 0 ||
                                 (Protection & WinAPI.MemoryProtection.PAGE_EXECUTE_WRITECOPY) != 0;
        
        /// <summary>
        /// Gets a value indicating whether the region is executable
        /// </summary>
        public bool IsExecutable => (Protection & WinAPI.MemoryProtection.PAGE_EXECUTE) != 0 ||
                                   (Protection & WinAPI.MemoryProtection.PAGE_EXECUTE_READ) != 0 ||
                                   (Protection & WinAPI.MemoryProtection.PAGE_EXECUTE_READWRITE) != 0 ||
                                   (Protection & WinAPI.MemoryProtection.PAGE_EXECUTE_WRITECOPY) != 0;
    }
}
