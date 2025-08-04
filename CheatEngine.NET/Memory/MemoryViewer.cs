using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using CheatEngine.NET.Core;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.Memory
{
    /// <summary>
    /// Provides functionality for viewing and editing memory in a hexadecimal format
    /// </summary>
    public class MemoryViewer
    {
        private IntPtr _baseAddress;
        private int _bytesPerRow = 16;
        private int _totalRows = 16;
        private byte[][] _memoryData;
        private bool _showText = true;
        private bool _showAddresses = true;
        
        /// <summary>
        /// Event raised when memory data is refreshed
        /// </summary>
        public event EventHandler? MemoryRefreshed;
        
        /// <summary>
        /// Gets or sets the base address
        /// </summary>
        public IntPtr BaseAddress
        {
            get => _baseAddress;
            set
            {
                _baseAddress = value;
                RefreshMemory();
            }
        }
        
        /// <summary>
        /// Gets or sets the number of bytes per row
        /// </summary>
        public int BytesPerRow
        {
            get => _bytesPerRow;
            set
            {
                _bytesPerRow = value;
                RefreshMemory();
            }
        }
        
        /// <summary>
        /// Gets or sets the total number of rows
        /// </summary>
        public int TotalRows
        {
            get => _totalRows;
            set
            {
                _totalRows = value;
                RefreshMemory();
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether to show text representation
        /// </summary>
        public bool ShowText
        {
            get => _showText;
            set => _showText = value;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether to show addresses
        /// </summary>
        public bool ShowAddresses
        {
            get => _showAddresses;
            set => _showAddresses = value;
        }
        
        /// <summary>
        /// Gets the memory data
        /// </summary>
        public byte[][] MemoryData => _memoryData;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryViewer"/> class
        /// </summary>
        public MemoryViewer()
        {
            _baseAddress = IntPtr.Zero;
            _memoryData = new byte[_totalRows][];
            
            for (int i = 0; i < _totalRows; i++)
            {
                _memoryData[i] = new byte[_bytesPerRow];
            }
        }
        
        /// <summary>
        /// Refreshes the memory data
        /// </summary>
        public void RefreshMemory()
        {
            if (CheatEngineCore.MemoryManager == null || CheatEngineCore.TargetProcess == null)
            {
                return;
            }
            
            try
            {
                // Resize memory data array if needed
                if (_memoryData.Length != _totalRows || _memoryData[0].Length != _bytesPerRow)
                {
                    _memoryData = new byte[_totalRows][];
                    
                    for (int i = 0; i < _totalRows; i++)
                    {
                        _memoryData[i] = new byte[_bytesPerRow];
                    }
                }
                
                // Read memory for each row
                for (int row = 0; row < _totalRows; row++)
                {
                    IntPtr rowAddress = new IntPtr(_baseAddress.ToInt64() + row * _bytesPerRow);
                    
                    try
                    {
                        _memoryData[row] = CheatEngineCore.MemoryManager.ReadMemory(rowAddress, _bytesPerRow);
                    }
                    catch
                    {
                        // If memory can't be read, fill with zeros
                        for (int i = 0; i < _bytesPerRow; i++)
                        {
                            _memoryData[row][i] = 0;
                        }
                    }
                }
                
                // Raise event
                OnMemoryRefreshed();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error refreshing memory: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Navigates to a specific address
        /// </summary>
        /// <param name="address">The address to navigate to</param>
        public void NavigateTo(IntPtr address)
        {
            _baseAddress = address;
            RefreshMemory();
        }
        
        /// <summary>
        /// Navigates to the next page
        /// </summary>
        public void NavigateNext()
        {
            _baseAddress = new IntPtr(_baseAddress.ToInt64() + _totalRows * _bytesPerRow);
            RefreshMemory();
        }
        
        /// <summary>
        /// Navigates to the previous page
        /// </summary>
        public void NavigatePrevious()
        {
            _baseAddress = new IntPtr(_baseAddress.ToInt64() - _totalRows * _bytesPerRow);
            RefreshMemory();
        }
        
        /// <summary>
        /// Writes a byte value to memory
        /// </summary>
        /// <param name="row">The row index</param>
        /// <param name="column">The column index</param>
        /// <param name="value">The byte value to write</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool WriteByte(int row, int column, byte value)
        {
            if (CheatEngineCore.MemoryManager == null || CheatEngineCore.TargetProcess == null)
            {
                return false;
            }
            
            if (row < 0 || row >= _totalRows || column < 0 || column >= _bytesPerRow)
            {
                return false;
            }
            
            try
            {
                // Calculate address
                IntPtr address = new IntPtr(_baseAddress.ToInt64() + row * _bytesPerRow + column);
                
                // Write value
                byte[] data = new byte[] { value };
                bool success = CheatEngineCore.MemoryManager.WriteMemory(address, data);
                
                if (success)
                {
                    // Update memory data
                    _memoryData[row][column] = value;
                    Logger.Log($"Wrote byte value 0x{value:X2} to address 0x{address.ToInt64():X}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error writing byte value: {ex.Message}", LogLevel.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Gets the address for a specific row
        /// </summary>
        /// <param name="row">The row index</param>
        /// <returns>The address</returns>
        public IntPtr GetAddressForRow(int row)
        {
            return new IntPtr(_baseAddress.ToInt64() + row * _bytesPerRow);
        }
        
        /// <summary>
        /// Gets the address for a specific row and column
        /// </summary>
        /// <param name="row">The row index</param>
        /// <param name="column">The column index</param>
        /// <returns>The address</returns>
        public IntPtr GetAddressForCell(int row, int column)
        {
            return new IntPtr(_baseAddress.ToInt64() + row * _bytesPerRow + column);
        }
        
        /// <summary>
        /// Gets the text representation of a byte
        /// </summary>
        /// <param name="b">The byte value</param>
        /// <returns>The text representation</returns>
        public static char GetTextChar(byte b)
        {
            if (b >= 32 && b <= 126)
            {
                return (char)b;
            }
            else
            {
                return '.';
            }
        }
        
        /// <summary>
        /// Gets the formatted row as a string
        /// </summary>
        /// <param name="row">The row index</param>
        /// <returns>The formatted row</returns>
        public string GetFormattedRow(int row)
        {
            if (row < 0 || row >= _totalRows)
            {
                return string.Empty;
            }
            
            StringBuilder sb = new StringBuilder();
            
            // Add address
            if (_showAddresses)
            {
                IntPtr address = GetAddressForRow(row);
                sb.Append($"{address.ToInt64():X8}: ");
            }
            
            // Add hex values
            for (int i = 0; i < _bytesPerRow; i++)
            {
                sb.Append($"{_memoryData[row][i]:X2} ");
            }
            
            // Add text representation
            if (_showText)
            {
                sb.Append("  ");
                
                for (int i = 0; i < _bytesPerRow; i++)
                {
                    sb.Append(GetTextChar(_memoryData[row][i]));
                }
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Gets all formatted rows as a string
        /// </summary>
        /// <returns>The formatted memory view</returns>
        public string GetFormattedMemory()
        {
            StringBuilder sb = new StringBuilder();
            
            for (int row = 0; row < _totalRows; row++)
            {
                sb.AppendLine(GetFormattedRow(row));
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Raises the MemoryRefreshed event
        /// </summary>
        protected virtual void OnMemoryRefreshed()
        {
            MemoryRefreshed?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Provides rendering functionality for memory viewer
    /// </summary>
    public class MemoryViewerRenderer
    {
        private readonly MemoryViewer _memoryViewer;
        private readonly Font _font;
        private readonly Brush _textBrush;
        private readonly Brush _addressBrush;
        private readonly Brush _selectedBrush;
        private readonly Brush _modifiedBrush;
        private int _selectedRow = -1;
        private int _selectedColumn = -1;
        private Dictionary<IntPtr, byte> _modifiedBytes = new Dictionary<IntPtr, byte>();
        
        /// <summary>
        /// Gets or sets the selected row
        /// </summary>
        public int SelectedRow
        {
            get => _selectedRow;
            set => _selectedRow = value;
        }
        
        /// <summary>
        /// Gets or sets the selected column
        /// </summary>
        public int SelectedColumn
        {
            get => _selectedColumn;
            set => _selectedColumn = value;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryViewerRenderer"/> class
        /// </summary>
        /// <param name="memoryViewer">The memory viewer</param>
        public MemoryViewerRenderer(MemoryViewer memoryViewer)
        {
            _memoryViewer = memoryViewer;
            _font = new Font("Consolas", 10);
            _textBrush = Brushes.Black;
            _addressBrush = Brushes.Blue;
            _selectedBrush = Brushes.LightBlue;
            _modifiedBrush = Brushes.Red;
        }
        
        /// <summary>
        /// Renders the memory viewer
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="bounds">The bounds rectangle</param>
        public void Render(Graphics g, Rectangle bounds)
        {
            g.Clear(Color.White);
            
            // Calculate metrics
            float charWidth = g.MeasureString("0", _font).Width;
            float charHeight = g.MeasureString("0", _font).Height;
            
            // Calculate offsets
            float addressWidth = _memoryViewer.ShowAddresses ? 10 * charWidth : 0;
            float hexOffset = addressWidth;
            float textOffset = hexOffset + _memoryViewer.BytesPerRow * 3 * charWidth;
            
            // Draw rows
            for (int row = 0; row < _memoryViewer.TotalRows; row++)
            {
                float y = row * charHeight;
                
                // Draw address
                if (_memoryViewer.ShowAddresses)
                {
                    IntPtr address = _memoryViewer.GetAddressForRow(row);
                    g.DrawString($"{address.ToInt64():X8}:", _font, _addressBrush, 0, y);
                }
                
                // Draw hex values
                for (int col = 0; col < _memoryViewer.BytesPerRow; col++)
                {
                    float x = hexOffset + col * 3 * charWidth;
                    byte value = _memoryViewer.MemoryData[row][col];
                    
                    // Check if cell is selected
                    if (row == _selectedRow && col == _selectedColumn)
                    {
                        g.FillRectangle(_selectedBrush, x, y, 2 * charWidth, charHeight);
                    }
                    
                    // Check if byte is modified
                    IntPtr address = _memoryViewer.GetAddressForCell(row, col);
                    Brush brush = _modifiedBytes.ContainsKey(address) ? _modifiedBrush : _textBrush;
                    
                    g.DrawString($"{value:X2}", _font, brush, x, y);
                }
                
                // Draw text representation
                if (_memoryViewer.ShowText)
                {
                    for (int col = 0; col < _memoryViewer.BytesPerRow; col++)
                    {
                        float x = textOffset + col * charWidth;
                        byte value = _memoryViewer.MemoryData[row][col];
                        char c = MemoryViewer.GetTextChar(value);
                        
                        g.DrawString(c.ToString(), _font, _textBrush, x, y);
                    }
                }
            }
        }
        
        /// <summary>
        /// Marks a byte as modified
        /// </summary>
        /// <param name="address">The address</param>
        /// <param name="value">The new value</param>
        public void MarkModified(IntPtr address, byte value)
        {
            _modifiedBytes[address] = value;
        }
        
        /// <summary>
        /// Clears the modified bytes
        /// </summary>
        public void ClearModified()
        {
            _modifiedBytes.Clear();
        }
        
        /// <summary>
        /// Gets the row and column at a point
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="g">The graphics object</param>
        /// <param name="row">The output row</param>
        /// <param name="column">The output column</param>
        /// <param name="isText">Whether the point is in the text area</param>
        /// <returns>True if a cell was hit, false otherwise</returns>
        public bool GetCellAt(Point point, Graphics g, out int row, out int column, out bool isText)
        {
            row = -1;
            column = -1;
            isText = false;
            
            // Calculate metrics
            float charWidth = g.MeasureString("0", _font).Width;
            float charHeight = g.MeasureString("0", _font).Height;
            
            // Calculate offsets
            float addressWidth = _memoryViewer.ShowAddresses ? 10 * charWidth : 0;
            float hexOffset = addressWidth;
            float textOffset = hexOffset + _memoryViewer.BytesPerRow * 3 * charWidth;
            
            // Calculate row
            row = (int)(point.Y / charHeight);
            if (row < 0 || row >= _memoryViewer.TotalRows)
            {
                return false;
            }
            
            // Check if in hex area
            if (point.X >= hexOffset && point.X < textOffset)
            {
                column = (int)((point.X - hexOffset) / (3 * charWidth));
                if (column < 0 || column >= _memoryViewer.BytesPerRow)
                {
                    return false;
                }
                
                isText = false;
                return true;
            }
            
            // Check if in text area
            if (_memoryViewer.ShowText && point.X >= textOffset)
            {
                column = (int)((point.X - textOffset) / charWidth);
                if (column < 0 || column >= _memoryViewer.BytesPerRow)
                {
                    return false;
                }
                
                isText = true;
                return true;
            }
            
            return false;
        }
    }
}
