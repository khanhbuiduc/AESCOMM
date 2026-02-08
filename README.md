# 🔐 AESCOMM - AES Encrypted File Transfer Application

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-100%25-239120?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Windows](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows)](https://www.microsoft.com/windows)

A secure, peer-to-peer file transfer application with multi-standard AES encryption built from scratch in C#. Transfer files safely over your local network with AES-128, AES-192, or AES-256 encryption.

---

## 📋 Table of Contents

- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Installation](#-installation)
- [Usage](#-usage)
- [Encryption Details](#-encryption-details)
- [Project Structure](#-project-structure)
- [Configuration](#-configuration)
- [Security Considerations](#-security-considerations)
- [Contributing](#-contributing)
- [License](#-license)

---

## ✨ Features

### 🔒 **Multi-Standard AES Encryption**
- **AES-256** (256-bit key, 14 rounds) - Maximum security
- **AES-192** (192-bit key, 12 rounds) - Balanced security
- **AES-128** (128-bit key, 10 rounds) - Fast encryption

### 📁 **Secure File Transfer**
- Real-time file encryption during transmission
- TCP/IP-based peer-to-peer communication
- Automatic device discovery and naming
- Support for any file type (binary & text)

### 💾 **File Management**
- Automatic download directory creation
- Unique filename generation (prevents overwrites)
- Encrypted file storage with metadata
- Download history tracking with timestamps

### 🔓 **Flexible Decryption**
- Immediate decryption prompt after receiving
- Manual decryption of stored encrypted files
- Support for both text-based and binary encrypted formats
- Key validation with hex format checking

### 🖥️ **User-Friendly Interface**
- Windows Forms GUI
- Device name customization
- Encryption type selection (radio buttons)
- File browser integration
- Download history viewer

### 🌐 **Network Features**
- Asynchronous TCP listener (port 5001)
- Multi-client handling capability
- Automatic server startup
- Graceful connection management

---

## 🛠️ Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Language** | C# | Latest |
| **Framework** | .NET | 8.0 (Windows) |
| **UI** | Windows Forms | Built-in |
| **Protocol** | TCP/IP | Socket-based |
| **Encryption** | Custom AES | 128/192/256-bit |
| **Encoding** | Base64 | For file transmission |

### Key Libraries
```csharp
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
```

---

## 🏗️ Architecture

### **Design Pattern**
- **Client-Server Architecture** with peer-to-peer capabilities
- **Event-driven UI** using Windows Forms
- **Asynchronous I/O** for network operations
- **Block cipher mode** - CBC (Cipher Block Chaining)

### **Encryption Flow**

```
┌─────────────┐      ┌──────────────┐      ┌─────────────┐
│   Sender    │─────▶│  Encryption  │─────▶│  Receiver   │
│  (Client)   │      │  AES + CBC   │      │  (Server)   │
└─────────────┘      └──────────────┘      └─────────────┘
       │                    │                      │
       ▼                    ▼                      ▼
  Select File          Split Blocks          Save Encrypted
  Enter Key           Apply AES              Decrypt on Demand
  Choose AES Type     Add Metadata           Verify Key
```

### **Data Format**

#### File Transfer Protocol
```
[Total Length: 4 bytes]
[IsFile Flag: 1 byte] (1 = file, 0 = message)
[AES Type: 1 byte] (1 = AES-256, 2 = AES-192, 3 = AES-128)
[FileName Length: 4 bytes]
[FileName: variable]
[Encrypted Data: variable (16-byte blocks)]
```

#### Encrypted File Storage Format
```
FILENAME:[original_name]
TIMESTAMP:[yyyy-MM-dd HH:mm:ss]
ENCRYPTED_SIZE:[bytes]
AES_TYPE:[1|2|3]
---BEGIN ENCRYPTED DATA---
[Base64 encoded encrypted blocks]
---END ENCRYPTED DATA---
```

---

## 📥 Installation

### **Prerequisites**
- Windows 10/11
- .NET 8.0 Runtime or SDK
- Visual Studio 2022 (for development)

### **Build from Source**

1. **Clone the repository**
```bash
git clone https://github.com/khanhbuiduc/AESCOMM.git
cd AESCOMM
```

2. **Open in Visual Studio**
```bash
start AES2.sln
```

3. **Build the solution**
- Press `Ctrl + Shift + B` or
- Menu: Build → Build Solution

4. **Run the application**
- Press `F5` or
- Menu: Debug → Start Debugging

### **Quick Start (Release)**
```bash
dotnet build -c Release
cd bin/Release/net8.0-windows
./AES2.exe
```

---

## 🚀 Usage

### **1. Initial Setup**

When you first launch AESCOMM:
- A random device name is generated (e.g., "Brave Lion")
- A `Downloads` folder is created in the application directory
- The TCP listener starts automatically on port **5001**

### **2. Sending Encrypted Files**

#### Step-by-Step Guide:

1. **Select AES Encryption Type**
   - Choose AES-256 (most secure), AES-192, or AES-128

2. **Enter Encryption Key**
   - **AES-256**: 64 hexadecimal characters
   - **AES-192**: 48 hexadecimal characters
   - **AES-128**: 32 hexadecimal characters
   
   Example (AES-256):
   ```
   0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF
   ```

3. **Select Target Device**
   - Enter recipient's IP address in the "Host" field
   - Default port: `5001`

4. **Choose File**
   - Click "Select File" button
   - Browse and select any file type

5. **Send**
   - Click "Send" button
   - File is encrypted and transmitted

### **3. Receiving Files**

When a file is received:
1. **Automatic prompt** appears asking to decrypt now
2. **Enter decryption key** (must match sender's key)
3. **File is decrypted** and saved to `Downloads/Decrypted/`
4. **Option to open** the file immediately

### **4. Manual Decryption**

To decrypt previously received files:

1. Click **"Select Encrypted File"** button
2. Browse to `Downloads/Encrypted/` folder
3. Select the `.encrypted` file
4. Enter the **decryption key**
5. Click **"Decrypt"** button

### **5. View Download History**

- Click on the device name label
- View complete transfer history
- Open downloads folder
- Clear history (optional)

---

## 🔐 Encryption Details

### **Custom AES Implementation**

This project implements AES encryption **from scratch** (not using .NET's built-in crypto):

#### **Core Components**

| Component | Description | Files |
|-----------|-------------|-------|
| **S-Box / Inverse S-Box** | Substitution tables for byte transformation | All Core files |
| **Key Expansion** | Generates round keys from master key | `KeyExpansion()` |
| **SubBytes** | Non-linear substitution step | `SubBytes()`, `InvSubBytes()` |
| **ShiftRows** | Row-wise permutation | `ShiftRows()`, `InvShiftRows()` |
| **MixColumns** | Column-wise mixing | `MixColumns()`, `InvMixColumns()` |
| **AddRoundKey** | XOR with round key | `AddRoundKey()` |

#### **AES Variants**

```csharp
// AES-128: 10 rounds, 4-word (128-bit) key
AES128Core.Encrypt(state, key);

// AES-192: 12 rounds, 6-word (192-bit) key
AES192Core.Encrypt(state, key);

// AES-256: 14 rounds, 8-word (256-bit) key
AES256Core.MahoaAES(state, key);
```

#### **CBC Mode (Cipher Block Chaining)**

```csharp
// Encryption: C[i] = E(P[i] ⊕ C[i-1])
uint[][] encryptedBlocks = AES256CBC.EncryptCBC(message, key, iv);

// Decryption: P[i] = D(C[i]) ⊕ C[i-1]
string decryptedMessage = AES256CBC.DecryptCBC(encryptedBlocks, key);
```

**Benefits of CBC:**
- Each block depends on all previous blocks
- Identical plaintext blocks produce different ciphertext
- IV (Initialization Vector) ensures randomness

#### **Key Formats**

| AES Type | Key Length | Hex Characters | Example |
|----------|-----------|----------------|---------|
| AES-128 | 128 bits | 32 chars | `0123456789ABCDEF0123456789ABCDEF` |
| AES-192 | 192 bits | 48 chars | `0123...CDEF` (48 chars) |
| AES-256 | 256 bits | 64 chars | `0123...CDEF` (64 chars) |

---

## 📂 Project Structure

```
AESCOMM/
│
├── 📁 AES/                          # Encryption library
│   ├── AES128Core.cs               # AES-128 implementation (10 rounds)
│   ├── AES192Core.cs               # AES-192 implementation (12 rounds)
│   ├── AES256Core.cs               # AES-256 implementation (14 rounds)
│   └── AES256CBC.cs                # CBC mode encryption/decryption
│
├── 📁 Properties/
│   └── Resources.Designer.cs       # Resource management
│
├── 📁 Resources/                    # UI resources
│
├── 📁 bin/                          # Compiled binaries
│
├── 📁 obj/                          # Build artifacts
│
├── 📄 Form1.cs                      # Main application window
├── 📄 Form1.Designer.cs             # UI designer file
├── 📄 Form1.resx                    # Form resources
│
├── 📄 HistoryForm.cs                # Download history viewer
├── 📄 HistoryForm.Designer.cs       # History UI designer
├── 📄 HistoryForm.resx              # History form resources
│
├── 📄 Program.cs                    # Application entry point
├── 📄 AES2.csproj                   # Project configuration
├── 📄 AES2.sln                      # Visual Studio solution
└── 📄 README.md                     # This file
```

### **Core Files Explained**

#### **AES/AES256Core.cs** (Main encryption engine)
```csharp
// S-Box: Substitution box for byte transformation
static readonly byte[] SBox = { ... };

// Core AES operations
public static uint[] MahoaAES(uint[] state, uint[] key);  // Encrypt
public static uint[] GiaimaAES(uint[] state, uint[] key); // Decrypt
public static uint[] KeyExpansion(uint[] key);            // Generate round keys
```

#### **AES/AES256CBC.cs** (CBC mode wrapper)
```csharp
// High-level encryption/decryption
public static uint[][] EncryptCBC(string message, uint[] key, uint[] iv);
public static string DecryptCBC(uint[][] encryptedBlocks, uint[] key);
```

#### **Form1.cs** (Main application logic)
```csharp
// Key methods
private async void BtnSend_Click();          // Send encrypted file
private async Task HandleClientAsync();       // Receive encrypted file
private async void BtnDecrypt_Click();        // Manual decryption
private uint[] ParseEncryptionKey();          // Key validation
```

---

## ⚙️ Configuration

### **Application Settings**

#### **Port Configuration**
```csharp
// Default listening port (in Form1.cs)
private const int DEFAULT_PORT = 5001;
```

To change the port, modify this constant and rebuild.

#### **Device Naming**

Create `name_random.json` in the application directory:
```json
{
  "adjectives": [
    "brave", "swift", "mighty", "clever", "silent"
  ],
  "nouns": [
    "lion", "eagle", "tiger", "wolf", "dragon"
  ]
}
```

The app generates random combinations like "Brave Lion" or "Swift Eagle".

#### **Directories**

| Folder | Purpose | Auto-created |
|--------|---------|--------------|
| `Downloads/` | Root download folder | ✅ |
| `Downloads/Encrypted/` | Encrypted files storage | ✅ |
| `Downloads/Decrypted/` | Decrypted files output | ✅ |

#### **Files Generated**

| File | Purpose |
|------|---------|
| `saved_name.txt` | Stores device name |
| `DownloadHistory.txt` | Transfer log with timestamps |

---

## 🔒 Security Considerations

### **⚠️ Important Security Notes**

1. **Custom Cryptography Implementation**
   - This is a **custom AES implementation** for educational purposes
   - Not audited or certified (not FIPS 140-2 compliant)
   - For production use, consider using .NET's `AesCryptoServiceProvider`

2. **Key Management**
   - **No automatic key exchange** - keys must be shared manually
   - Keys are entered in **plaintext** (not stored securely)
   - Consider implementing Diffie-Hellman key exchange

3. **Network Security**
   - Data is encrypted but **metadata is not**
   - No authentication mechanism
   - Vulnerable to man-in-the-middle attacks on untrusted networks
   - **Recommended**: Use on trusted local networks only

4. **File Integrity**
   - No message authentication code (MAC)
   - Cannot detect tampering or corruption
   - Consider adding HMAC for integrity verification

### **Best Practices**

✅ **DO:**
- Use strong, random encryption keys
- Share keys through secure channels (not over the same network)
- Use AES-256 for maximum security
- Verify file integrity after decryption
- Use on private, trusted networks

❌ **DON'T:**
- Use weak or predictable keys (e.g., `00000...`)
- Share keys via email or unencrypted messages
- Use on public or untrusted networks
- Assume encrypted files can't be tampered with
- Reuse the same key for all files

### **Threat Model**

| Threat | Protected | Notes |
|--------|-----------|-------|
| Network sniffing | ✅ Yes | Data is AES encrypted |
| File access by others | ✅ Yes | Files stored encrypted |
| Key interception | ❌ No | Keys must be shared manually |
| Man-in-the-middle | ⚠️ Partial | No authentication mechanism |
| Tampering detection | ❌ No | No MAC/HMAC implementation |

---

## 🎓 Educational Value

This project is excellent for learning:

### **Cryptography Concepts**
- Symmetric encryption (AES)
- Block cipher modes (CBC)
- Key expansion algorithms
- S-Box transformations
- Galois field arithmetic

### **Networking Concepts**
- TCP/IP socket programming
- Asynchronous I/O operations
- Client-server architecture
- Protocol design

### **Software Engineering**
- Windows Forms development
- Event-driven programming
- File I/O operations
- Error handling and validation

---

## 🐛 Troubleshooting

### **Common Issues**

#### **"Port 5001 already in use"**
```csharp
// Solution: Change the port in Form1.cs
private const int DEFAULT_PORT = 5002; // Use different port
```

#### **"Invalid encryption key format"**
- Ensure key contains only hex characters: `0-9, A-F, a-f`
- Verify key length matches AES type:
  - AES-128: 32 characters
  - AES-192: 48 characters
  - AES-256: 64 characters

#### **"Decryption failed"**
- Verify you're using the **exact same key** used for encryption
- Check that AES type matches (both sender and receiver)
- Ensure file hasn't been corrupted

#### **"Cannot connect to device"**
- Verify IP address is correct
- Check firewall settings (allow port 5001)
- Ensure both devices are on the same network
- Confirm receiving device has AESCOMM running

---

## 🧪 Testing

### **Unit Testing Example**

```csharp
// Test AES-256 encryption/decryption
uint[] key = AES256CBC.GenerateRandomKey();
uint[] iv = AES256CBC.GenerateIV();
string originalMessage = "Hello, AESCOMM!";

uint[][] encrypted = AES256CBC.EncryptCBC(originalMessage, key, iv);
string decrypted = AES256CBC.DecryptCBC(encrypted, key);

Debug.Assert(originalMessage == decrypted, "Encryption/Decryption failed!");
```

### **Manual Testing Checklist**

- [ ] Send text file and verify decryption
- [ ] Send binary file (image, executable)
- [ ] Test all three AES types (128, 192, 256)
- [ ] Verify incorrect key fails gracefully
- [ ] Check download history accuracy
- [ ] Test with multiple simultaneous connections
- [ ] Verify unique filename generation
- [ ] Test manual decryption of old files

---

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

### **Areas for Improvement**

1. **Security Enhancements**
   - Implement key exchange protocol (Diffie-Hellman)
   - Add HMAC for message authentication
   - Secure key storage (Windows DPAPI)

2. **Features**
   - File compression before encryption
   - Progress bars for large file transfers
   - Drag-and-drop file selection
   - Multi-file batch encryption

3. **UI/UX**
   - Dark mode theme
   - System tray integration
   - Notification system
   - Language localization

4. **Testing**
   - Unit tests for encryption functions
   - Integration tests for file transfer
   - Performance benchmarks

### **How to Contribute**

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📜 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2024 Khanh Bui Duc

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction...
```

---

## 👨‍💻 Author

**Khanh Bui Duc**
- GitHub: [@khanhbuiduc](https://github.com/khanhbuiduc)
- Repository: [AESCOMM](https://github.com/khanhbuiduc/AESCOMM)

---

## 🙏 Acknowledgments

- AES specification: [FIPS PUB 197](https://nvlpubs.nist.gov/nistpubs/FIPS/NIST.FIPS.197.pdf)
- .NET Windows Forms documentation
- Cryptography Stack Exchange community
- Open source contributors

---

## 📞 Support

If you encounter any issues or have questions:

1. Check the [Troubleshooting](#-troubleshooting) section
2. Open an [Issue](https://github.com/khanhbuiduc/AESCOMM/issues)
3. Review existing issues for solutions

---

## 🗺️ Roadmap

### **Version 2.0 (Planned)**
- [ ] RSA key exchange for secure key sharing
- [ ] Multi-threaded file transfers
- [ ] Command-line interface (CLI) version
- [ ] Cross-platform support (Linux, macOS via Avalonia UI)

### **Version 3.0 (Future)**
- [ ] End-to-end encrypted chat
- [ ] File compression (ZIP integration)
- [ ] Cloud storage integration
- [ ] Mobile app (Xamarin/MAUI)

---

<div align="center">

### ⭐ Star this repository if you find it helpful!

**Made with ❤️ and C#**

[Report Bug](https://github.com/khanhbuiduc/AESCOMM/issues) · [Request Feature](https://github.com/khanhbuiduc/AESCOMM/issues)

</div>
