using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CheatEngine.NET.Core;
using CheatEngine.NET.Scanner;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.Memory
{
    /// <summary>
    /// Manages the list of memory addresses and their values
    /// </summary>
    public class AddressListManager
    {
        private readonly List<AddressEntry> _entries = new List<AddressEntry>();
        private readonly BindingList<AddressEntry> _bindingList;
        private System.Threading.Timer? _updateTimer;
        private bool _autoUpdate = true;
        private int _updateInterval = 1000; // milliseconds
        
        /// <summary>
        /// Event raised when the address list is changed
        /// </summary>
        public event EventHandler? AddressListChanged;
        
        /// <summary>
        /// Gets the binding list of address entries
        /// </summary>
        public BindingList<AddressEntry> Entries => _bindingList;
        
        /// <summary>
        /// Gets or sets a value indicating whether values should be automatically updated
        /// </summary>
        public bool AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                _autoUpdate = value;
                UpdateTimerState();
            }
        }
        
        /// <summary>
        /// Gets or sets the update interval in milliseconds
        /// </summary>
        public int UpdateInterval
        {
            get => _updateInterval;
            set
            {
                _updateInterval = value;
                UpdateTimerState();
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressListManager"/> class
        /// </summary>
        public AddressListManager()
        {
            _bindingList = new BindingList<AddressEntry>(_entries);
            _bindingList.ListChanged += (sender, e) => OnAddressListChanged();
            
            // Create update timer
            _updateTimer = new System.Threading.Timer(UpdateValues, null, Timeout.Infinite, Timeout.Infinite);
            UpdateTimerState();
        }
        
        /// <summary>
        /// Adds an address entry to the list
        /// </summary>
        /// <param name="address">The memory address</param>
        /// <param name="description">The description</param>
        /// <param name="valueType">The type of value</param>
        /// <returns>The added entry</returns>
        public AddressEntry AddEntry(IntPtr address, string description, ScanType valueType)
        {
            // Check if entry already exists
            foreach (var entry in _entries)
            {
                if (entry.Address == address && entry.ValueType == valueType)
                {
                    throw new Exception($"Address entry already exists at 0x{address.ToInt64():X}");
                }
            }
            
            // Create entry
            AddressEntry newEntry = new AddressEntry
            {
                Address = address,
                Description = description,
                ValueType = valueType,
                IsActive = true
            };
            
            // Read initial value
            UpdateEntryValue(newEntry);
            
            // Add to list
            _entries.Add(newEntry);
            
            Logger.Log($"Address entry added: 0x{address.ToInt64():X} ({description})");
            OnAddressListChanged();
            
            return newEntry;
        }
        
        /// <summary>
        /// Adds multiple address entries from scan results
        /// </summary>
        /// <param name="results">The scan results</param>
        public void AddEntries(IEnumerable<ScanResult> results)
        {
            foreach (var result in results)
            {
                try
                {
                    // Skip if entry already exists
                    if (_entries.Any(e => e.Address == result.Address && e.ValueType == result.ValueType))
                    {
                        continue;
                    }
                    
                    // Create entry
                    AddressEntry newEntry = new AddressEntry
                    {
                        Address = result.Address,
                        Description = result.Description,
                        ValueType = result.ValueType,
                        Value = result.Value,
                        IsActive = true
                    };
                    
                    // Add to list
                    _entries.Add(newEntry);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error adding address entry: {ex.Message}", LogLevel.Warning);
                }
            }
            
            Logger.Log($"Added {results.Count()} address entries from scan results");
            OnAddressListChanged();
        }
        
        /// <summary>
        /// Removes an address entry from the list
        /// </summary>
        /// <param name="entry">The entry to remove</param>
        public void RemoveEntry(AddressEntry entry)
        {
            _entries.Remove(entry);
            
            Logger.Log($"Address entry removed: 0x{entry.Address.ToInt64():X} ({entry.Description})");
            OnAddressListChanged();
        }
        
        /// <summary>
        /// Clears all address entries
        /// </summary>
        public void ClearEntries()
        {
            _entries.Clear();
            
            Logger.Log("Address list cleared");
            OnAddressListChanged();
        }
        
        /// <summary>
        /// Updates the address list UI with current values
        /// </summary>
        public void UpdateAddressList()
        {
            // Refresh all values and notify UI of changes
            UpdateAllValues();
            OnAddressListChanged();
        }
        
        /// <summary>
        /// Updates the address list with scan results
        /// </summary>
        /// <param name="results">The scan results</param>
        public void UpdateAddressList(IEnumerable<ScanResult> results)
        {
            if (results == null)
            {
                return;
            }
            
            // Clear existing entries
            _entries.Clear();
            
            // Add new entries from scan results
            foreach (var result in results)
            {
                AddEntry(result.Address, result.Description, result.ValueType);
            }
            
            // Update values and notify UI
            UpdateAllValues();
            OnAddressListChanged();
            
            Logger.Log($"Address list updated with {results.Count()} scan results");
        }
        
        /// <summary>
        /// Updates the values of all active entries
        /// </summary>
        public void UpdateAllValues()
        {
            if (CheatEngineCore.MemoryManager == null || CheatEngineCore.TargetProcess == null)
            {
                return;
            }
            
            foreach (var entry in _entries)
            {
                if (entry.IsActive)
                {
                    UpdateEntryValue(entry);
                }
            }
        }
        
        /// <summary>
        /// Updates the value of an entry
        /// </summary>
        /// <param name="entry">The entry to update</param>
        public void UpdateEntryValue(AddressEntry entry)
        {
            if (CheatEngineCore.MemoryManager == null || CheatEngineCore.TargetProcess == null)
            {
                return;
            }
            
            try
            {
                // Read value from memory
                string value = ReadValueAsString(entry.Address, entry.ValueType);
                
                // Update entry
                entry.Value = value;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating value for address 0x{entry.Address.ToInt64():X}: {ex.Message}", LogLevel.Warning);
                entry.Value = "Error";
            }
        }
        
        /// <summary>
        /// Writes a value to memory
        /// </summary>
        /// <param name="entry">The address entry</param>
        /// <param name="value">The value to write</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool WriteValue(AddressEntry entry, string value)
        {
            if (CheatEngineCore.MemoryManager == null || CheatEngineCore.TargetProcess == null)
            {
                return false;
            }
            
            try
            {
                // Write value to memory
                bool success = WriteValueFromString(entry.Address, value, entry.ValueType);
                
                if (success)
                {
                    // Update entry
                    entry.Value = value;
                    Logger.Log($"Value written to address 0x{entry.Address.ToInt64():X}: {value}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error writing value to address 0x{entry.Address.ToInt64():X}: {ex.Message}", LogLevel.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Saves the address list to a file
        /// </summary>
        /// <param name="filePath">The file path</param>
        public void SaveToFile(string filePath)
        {
            try
            {
                // Create serializable list
                List<SerializableAddressEntry> serializableEntries = _entries.Select(e => new SerializableAddressEntry
                {
                    AddressString = e.Address.ToInt64().ToString("X"),
                    Description = e.Description,
                    ValueType = e.ValueType,
                    Value = e.Value,
                    IsActive = e.IsActive
                }).ToList();
                
                // Serialize to XML
                XmlSerializer serializer = new XmlSerializer(typeof(List<SerializableAddressEntry>));
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, serializableEntries);
                }
                
                Logger.Log($"Address list saved to {filePath}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error saving address list: {ex.Message}", LogLevel.Error);
                throw;
            }
        }
        
        /// <summary>
        /// Loads the address list from a file
        /// </summary>
        /// <param name="filePath">The file path</param>
        public void LoadFromFile(string filePath)
        {
            try
            {
                // Deserialize from XML
                XmlSerializer serializer = new XmlSerializer(typeof(List<SerializableAddressEntry>));
                List<SerializableAddressEntry> serializableEntries;
                
                using (StreamReader reader = new StreamReader(filePath))
                {
                    serializableEntries = (List<SerializableAddressEntry>)serializer.Deserialize(reader);
                }
                
                // Clear current entries
                _entries.Clear();
                
                // Add deserialized entries
                foreach (var serializableEntry in serializableEntries)
                {
                    try
                    {
                        // Parse address
                        long addressValue = long.Parse(serializableEntry.AddressString, System.Globalization.NumberStyles.HexNumber);
                        IntPtr address = new IntPtr(addressValue);
                        
                        // Create entry
                        AddressEntry entry = new AddressEntry
                        {
                            Address = address,
                            Description = serializableEntry.Description,
                            ValueType = serializableEntry.ValueType,
                            Value = serializableEntry.Value,
                            IsActive = serializableEntry.IsActive
                        };
                        
                        // Add to list
                        _entries.Add(entry);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error loading address entry: {ex.Message}", LogLevel.Warning);
                    }
                }
                
                Logger.Log($"Address list loaded from {filePath}");
                OnAddressListChanged();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading address list: {ex.Message}", LogLevel.Error);
                throw;
            }
        }
        
        /// <summary>
        /// Updates the timer state
        /// </summary>
        private void UpdateTimerState()
        {
            if (_updateTimer == null)
                return;
                
            if (_autoUpdate && CheatEngineCore.TargetProcess != null)
            {
                _updateTimer.Change(0, _updateInterval);
            }
            else
            {
                _updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }
        
        /// <summary>
        /// Timer callback to update values
        /// </summary>
        /// <param name="state">State object</param>
        private void UpdateValues(object? state)
        {
            try
            {
                UpdateAllValues();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error updating values: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Reads a value from memory as a string
        /// </summary>
        /// <param name="address">The memory address</param>
        /// <param name="valueType">The type of value</param>
        /// <returns>The value as a string</returns>
        private string ReadValueAsString(IntPtr address, ScanType valueType)
        {
            // Get memory manager
            var memoryManager = CheatEngineCore.MemoryManager!;
            
            try
            {
                // Read value from memory based on value type
                switch (valueType)
                {
                    case ScanType.Byte:
                        byte byteValue = memoryManager.ReadValue<byte>(address);
                        return byteValue.ToString();
                        
                    case ScanType.Short:
                        short shortValue = memoryManager.ReadValue<short>(address);
                        return shortValue.ToString();
                        
                    case ScanType.Integer:
                        int intValue = memoryManager.ReadValue<int>(address);
                        return intValue.ToString();
                        
                    case ScanType.Long:
                        long longValue = memoryManager.ReadValue<long>(address);
                        return longValue.ToString();
                        
                    case ScanType.Float:
                        float floatValue = memoryManager.ReadValue<float>(address);
                        return floatValue.ToString("F6");
                        
                    case ScanType.Double:
                        double doubleValue = memoryManager.ReadValue<double>(address);
                        return doubleValue.ToString("F6");
                        
                    case ScanType.String:
                        // Read a string (limited to 100 characters)
                        return memoryManager.ReadString(address, 100, Encoding.ASCII);
                        
                    default:
                        return "Unknown";
                }
            }
            catch
            {
                return "Error";
            }
        }
        
        /// <summary>
        /// Writes a value to memory from a string
        /// </summary>
        /// <param name="address">The memory address</param>
        /// <param name="value">The value as a string</param>
        /// <param name="valueType">The type of value</param>
        /// <returns>True if successful, false otherwise</returns>
        private bool WriteValueFromString(IntPtr address, string value, ScanType valueType)
        {
            // Get memory manager
            var memoryManager = CheatEngineCore.MemoryManager!;
            
            try
            {
                // Write value to memory based on value type
                switch (valueType)
                {
                    case ScanType.Byte:
                        byte byteValue = byte.Parse(value);
                        return memoryManager.WriteValue(address, byteValue);
                        
                    case ScanType.Short:
                        short shortValue = short.Parse(value);
                        return memoryManager.WriteValue(address, shortValue);
                        
                    case ScanType.Integer:
                        int intValue = int.Parse(value);
                        return memoryManager.WriteValue(address, intValue);
                        
                    case ScanType.Long:
                        long longValue = long.Parse(value);
                        return memoryManager.WriteValue(address, longValue);
                        
                    case ScanType.Float:
                        float floatValue = float.Parse(value);
                        return memoryManager.WriteValue(address, floatValue);
                        
                    case ScanType.Double:
                        double doubleValue = double.Parse(value);
                        return memoryManager.WriteValue(address, doubleValue);
                        
                    case ScanType.String:
                        return memoryManager.WriteString(address, value, Encoding.ASCII);
                        
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Raises the AddressListChanged event
        /// </summary>
        protected virtual void OnAddressListChanged()
        {
            AddressListChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Represents an entry in the address list
    /// </summary>
    public class AddressEntry
    {
        /// <summary>
        /// Gets or sets the memory address
        /// </summary>
        public IntPtr Address { get; set; }
        
        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the type of value
        /// </summary>
        public ScanType ValueType { get; set; }
        
        /// <summary>
        /// Gets or sets the value as a string
        /// </summary>
        public string Value { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets a value indicating whether the entry is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Gets the address as a hexadecimal string
        /// </summary>
        public string AddressString => $"0x{Address.ToInt64():X}";
    }
    
    /// <summary>
    /// Serializable version of an address entry
    /// </summary>
    [Serializable]
    public class SerializableAddressEntry
    {
        /// <summary>
        /// Gets or sets the memory address as a hexadecimal string
        /// </summary>
        public string AddressString { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the type of value
        /// </summary>
        public ScanType ValueType { get; set; }
        
        /// <summary>
        /// Gets or sets the value as a string
        /// </summary>
        public string Value { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets a value indicating whether the entry is active
        /// </summary>
        public bool IsActive { get; set; }
    }
}
