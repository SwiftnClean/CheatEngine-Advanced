using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CheatEngine.NET.Core;
using CheatEngine.NET.Memory;
using CheatEngine.NET.Utils;

namespace CheatEngine.NET.Disassembler
{
    /// <summary>
    /// Provides functionality for disassembling machine code
    /// </summary>
    public class Disassembler
    {
        private IntPtr _baseAddress;
        private int _instructionCount = 20;
        private List<DisassembledInstruction> _instructions = new List<DisassembledInstruction>();
        private bool _showBytes = true;
        private bool _showAddresses = true;
        
        /// <summary>
        /// Event raised when disassembly is refreshed
        /// </summary>
        public event EventHandler? DisassemblyRefreshed;
        
        /// <summary>
        /// Gets or sets the base address
        /// </summary>
        public IntPtr BaseAddress
        {
            get => _baseAddress;
            set
            {
                _baseAddress = value;
                RefreshDisassembly();
            }
        }
        
        /// <summary>
        /// Gets or sets the number of instructions to disassemble
        /// </summary>
        public int InstructionCount
        {
            get => _instructionCount;
            set
            {
                _instructionCount = value;
                RefreshDisassembly();
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether to show instruction bytes
        /// </summary>
        public bool ShowBytes
        {
            get => _showBytes;
            set => _showBytes = value;
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
        /// Gets the disassembled instructions
        /// </summary>
        public IReadOnlyList<DisassembledInstruction> Instructions => _instructions.AsReadOnly();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Disassembler"/> class
        /// </summary>
        public Disassembler()
        {
            _baseAddress = IntPtr.Zero;
        }
        
        /// <summary>
        /// Refreshes the disassembly
        /// </summary>
        public void RefreshDisassembly()
        {
            if (CheatEngineCore.MemoryManager == null || CheatEngineCore.TargetProcess == null)
            {
                return;
            }
            
            try
            {
                // Clear current instructions
                _instructions.Clear();
                
                // Read memory block
                byte[] memoryBlock;
                try
                {
                    // Read a larger block to ensure we have enough bytes for all instructions
                    memoryBlock = CheatEngineCore.MemoryManager.ReadMemory(_baseAddress, _instructionCount * 15);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error reading memory for disassembly: {ex.Message}", LogLevel.Error);
                    return;
                }
                
                // Disassemble instructions
                IntPtr currentAddress = _baseAddress;
                int bytesProcessed = 0;
                
                for (int i = 0; i < _instructionCount; i++)
                {
                    // Check if we have enough bytes left
                    if (bytesProcessed >= memoryBlock.Length)
                    {
                        break;
                    }
                    
                    // Disassemble one instruction
                    DisassembledInstruction instruction = DisassembleInstruction(memoryBlock, bytesProcessed, currentAddress);
                    
                    // Add to list
                    _instructions.Add(instruction);
                    
                    // Move to next instruction
                    bytesProcessed += instruction.Size;
                    currentAddress = new IntPtr(currentAddress.ToInt64() + instruction.Size);
                }
                
                // Raise event
                OnDisassemblyRefreshed();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error refreshing disassembly: {ex.Message}", LogLevel.Error);
            }
        }
        
        /// <summary>
        /// Disassembles a single instruction
        /// </summary>
        /// <param name="memoryBlock">The memory block</param>
        /// <param name="offset">The offset within the memory block</param>
        /// <param name="address">The address of the instruction</param>
        /// <returns>The disassembled instruction</returns>
        private DisassembledInstruction DisassembleInstruction(byte[] memoryBlock, int offset, IntPtr address)
        {
            // This is a simplified disassembler implementation
            // In a real implementation, we would use a proper disassembly library like SharpDisasm or Iced
            
            // For now, we'll implement a very basic x86 disassembler for common instructions
            
            // Check if we have at least one byte
            if (offset >= memoryBlock.Length)
            {
                return new DisassembledInstruction
                {
                    Address = address,
                    Size = 1,
                    Bytes = new byte[] { 0 },
                    Mnemonic = "???",
                    Operands = ""
                };
            }
            
            byte opcode = memoryBlock[offset];
            
            // Handle some common instructions
            switch (opcode)
            {
                case 0x90: // NOP
                    return new DisassembledInstruction
                    {
                        Address = address,
                        Size = 1,
                        Bytes = new byte[] { opcode },
                        Mnemonic = "NOP",
                        Operands = ""
                    };
                
                case 0xC3: // RET
                    return new DisassembledInstruction
                    {
                        Address = address,
                        Size = 1,
                        Bytes = new byte[] { opcode },
                        Mnemonic = "RET",
                        Operands = ""
                    };
                
                case 0x50: case 0x51: case 0x52: case 0x53: // PUSH reg
                case 0x54: case 0x55: case 0x56: case 0x57:
                    string reg = GetRegisterName(opcode - 0x50);
                    return new DisassembledInstruction
                    {
                        Address = address,
                        Size = 1,
                        Bytes = new byte[] { opcode },
                        Mnemonic = "PUSH",
                        Operands = reg
                    };
                
                case 0x58: case 0x59: case 0x5A: case 0x5B: // POP reg
                case 0x5C: case 0x5D: case 0x5E: case 0x5F:
                    reg = GetRegisterName(opcode - 0x58);
                    return new DisassembledInstruction
                    {
                        Address = address,
                        Size = 1,
                        Bytes = new byte[] { opcode },
                        Mnemonic = "POP",
                        Operands = reg
                    };
                
                case 0xB8: case 0xB9: case 0xBA: case 0xBB: // MOV reg, imm32
                case 0xBC: case 0xBD: case 0xBE: case 0xBF:
                    if (offset + 5 > memoryBlock.Length)
                    {
                        return CreateInvalidInstruction(address, memoryBlock, offset);
                    }
                    
                    reg = GetRegisterName(opcode - 0xB8);
                    int imm32 = BitConverter.ToInt32(memoryBlock, offset + 1);
                    
                    byte[] bytes = new byte[5];
                    Array.Copy(memoryBlock, offset, bytes, 0, 5);
                    
                    return new DisassembledInstruction
                    {
                        Address = address,
                        Size = 5,
                        Bytes = bytes,
                        Mnemonic = "MOV",
                        Operands = $"{reg}, 0x{imm32:X8}"
                    };
                
                case 0x89: // MOV r/m32, r32
                    if (offset + 2 > memoryBlock.Length)
                    {
                        return CreateInvalidInstruction(address, memoryBlock, offset);
                    }
                    
                    byte modrm89 = memoryBlock[offset + 1];
                    int mod89 = (modrm89 >> 6) & 0x3;
                    int reg89 = (modrm89 >> 3) & 0x7;
                    int rm89 = modrm89 & 0x7;
                    
                    string regName89 = GetRegisterName(reg89);
                    string rmName89 = GetRegisterName(rm89);
                    
                    if (mod89 == 3) // Direct register addressing
                    {
                        return new DisassembledInstruction
                        {
                            Address = address,
                            Size = 2,
                            Bytes = new byte[] { opcode, modrm89 },
                            Mnemonic = "MOV",
                            Operands = $"{rmName89}, {regName89}"
                        };
                    }
                    else
                    {
                        // More complex addressing modes would be handled here
                        return new DisassembledInstruction
                        {
                            Address = address,
                            Size = 2,
                            Bytes = new byte[] { opcode, modrm89 },
                            Mnemonic = "MOV",
                            Operands = $"[{rmName89}], {regName89}"
                        };
                    }
                
                case 0x8B: // MOV r32, r/m32
                    if (offset + 2 > memoryBlock.Length)
                    {
                        return CreateInvalidInstruction(address, memoryBlock, offset);
                    }
                    
                    byte modrm8B = memoryBlock[offset + 1];
                    int mod8B = (modrm8B >> 6) & 0x3;
                    int reg8B = (modrm8B >> 3) & 0x7;
                    int rm8B = modrm8B & 0x7;
                    
                    string regName8B = GetRegisterName(reg8B);
                    string rmName8B = GetRegisterName(rm8B);
                    
                    if (mod8B == 3) // Direct register addressing
                    {
                        return new DisassembledInstruction
                        {
                            Address = address,
                            Size = 2,
                            Bytes = new byte[] { opcode, modrm8B },
                            Mnemonic = "MOV",
                            Operands = $"{regName8B}, {rmName8B}"
                        };
                    }
                    else
                    {
                        // More complex addressing modes would be handled here
                        return new DisassembledInstruction
                        {
                            Address = address,
                            Size = 2,
                            Bytes = new byte[] { opcode, modrm8B },
                            Mnemonic = "MOV",
                            Operands = $"{regName8B}, [{rmName8B}]"
                        };
                    }
                
                case 0xE8: // CALL rel32
                    if (offset + 5 > memoryBlock.Length)
                    {
                        return CreateInvalidInstruction(address, memoryBlock, offset);
                    }
                    
                    int rel32 = BitConverter.ToInt32(memoryBlock, offset + 1);
                    long targetAddress = address.ToInt64() + 5 + rel32;
                    
                    bytes = new byte[5];
                    Array.Copy(memoryBlock, offset, bytes, 0, 5);
                    
                    return new DisassembledInstruction
                    {
                        Address = address,
                        Size = 5,
                        Bytes = bytes,
                        Mnemonic = "CALL",
                        Operands = $"0x{targetAddress:X8}"
                    };
                
                case 0xCC: // INT3 (breakpoint)
                    return new DisassembledInstruction
                    {
                        Address = address,
                        Size = 1,
                        Bytes = new byte[] { opcode },
                        Mnemonic = "INT3",
                        Operands = ""
                    };
                
                default:
                    // For unknown opcodes, return a generic instruction
                    return new DisassembledInstruction
                    {
                        Address = address,
                        Size = 1,
                        Bytes = new byte[] { opcode },
                        Mnemonic = "DB",
                        Operands = $"0x{opcode:X2}"
                    };
            }
        }
        
        /// <summary>
        /// Creates an invalid instruction
        /// </summary>
        /// <param name="address">The address</param>
        /// <param name="memoryBlock">The memory block</param>
        /// <param name="offset">The offset</param>
        /// <returns>The invalid instruction</returns>
        private DisassembledInstruction CreateInvalidInstruction(IntPtr address, byte[] memoryBlock, int offset)
        {
            byte[] bytes = new byte[1];
            bytes[0] = memoryBlock[offset];
            
            return new DisassembledInstruction
            {
                Address = address,
                Size = 1,
                Bytes = bytes,
                Mnemonic = "DB",
                Operands = $"0x{bytes[0]:X2}"
            };
        }
        
        /// <summary>
        /// Gets the register name for a register index
        /// </summary>
        /// <param name="index">The register index</param>
        /// <returns>The register name</returns>
        private string GetRegisterName(int index)
        {
            switch (index)
            {
                case 0: return "EAX";
                case 1: return "ECX";
                case 2: return "EDX";
                case 3: return "EBX";
                case 4: return "ESP";
                case 5: return "EBP";
                case 6: return "ESI";
                case 7: return "EDI";
                default: return $"R{index}";
            }
        }
        
        /// <summary>
        /// Navigates to a specific address
        /// </summary>
        /// <param name="address">The address to navigate to</param>
        public void NavigateTo(IntPtr address)
        {
            _baseAddress = address;
            RefreshDisassembly();
        }
        
        /// <summary>
        /// Navigates to the next instruction
        /// </summary>
        public void NavigateNext()
        {
            if (_instructions.Count > 0)
            {
                var lastInstruction = _instructions.Last();
                _baseAddress = new IntPtr(lastInstruction.Address.ToInt64() + lastInstruction.Size);
                RefreshDisassembly();
            }
        }
        
        /// <summary>
        /// Navigates to the previous instruction
        /// </summary>
        public void NavigatePrevious()
        {
            // This is a bit tricky since instructions have variable length
            // For simplicity, we'll just go back a fixed amount and then disassemble forward
            _baseAddress = new IntPtr(_baseAddress.ToInt64() - 16);
            RefreshDisassembly();
        }
        
        /// <summary>
        /// Gets the formatted instruction as a string
        /// </summary>
        /// <param name="instruction">The instruction</param>
        /// <returns>The formatted instruction</returns>
        public string GetFormattedInstruction(DisassembledInstruction instruction)
        {
            StringBuilder sb = new StringBuilder();
            
            // Add address
            if (_showAddresses)
            {
                sb.Append($"{instruction.Address.ToInt64():X8}: ");
            }
            
            // Add bytes
            if (_showBytes)
            {
                foreach (byte b in instruction.Bytes)
                {
                    sb.Append($"{b:X2} ");
                }
                
                // Pad to a fixed width
                int bytesWidth = 20;
                int bytesLength = instruction.Bytes.Length * 3;
                if (bytesLength < bytesWidth)
                {
                    sb.Append(new string(' ', bytesWidth - bytesLength));
                }
            }
            
            // Add mnemonic and operands
            sb.Append(instruction.Mnemonic);
            
            if (!string.IsNullOrEmpty(instruction.Operands))
            {
                sb.Append(" ");
                sb.Append(instruction.Operands);
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Gets all formatted instructions as a string
        /// </summary>
        /// <returns>The formatted disassembly</returns>
        public string GetFormattedDisassembly()
        {
            StringBuilder sb = new StringBuilder();
            
            foreach (var instruction in _instructions)
            {
                sb.AppendLine(GetFormattedInstruction(instruction));
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Raises the DisassemblyRefreshed event
        /// </summary>
        protected virtual void OnDisassemblyRefreshed()
        {
            DisassemblyRefreshed?.Invoke(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Represents a disassembled instruction
    /// </summary>
    public class DisassembledInstruction
    {
        /// <summary>
        /// Gets or sets the address of the instruction
        /// </summary>
        public IntPtr Address { get; set; }
        
        /// <summary>
        /// Gets or sets the size of the instruction in bytes
        /// </summary>
        public int Size { get; set; }
        
        /// <summary>
        /// Gets or sets the raw bytes of the instruction
        /// </summary>
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
        
        /// <summary>
        /// Gets or sets the mnemonic of the instruction
        /// </summary>
        public string Mnemonic { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the operands of the instruction
        /// </summary>
        public string Operands { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets the address as a hexadecimal string
        /// </summary>
        public string AddressString => $"0x{Address.ToInt64():X8}";
        
        /// <summary>
        /// Gets the bytes as a hexadecimal string
        /// </summary>
        public string BytesString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in Bytes)
                {
                    sb.Append($"{b:X2} ");
                }
                return sb.ToString().TrimEnd();
            }
        }
    }
}
