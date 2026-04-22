# SaveMe

> A smart version control system using Content Defined Chunking (CDC) for efficient file change detection and management.

## 📖 Table of Contents

- [Project Overview](#-project-overview)
- [Main Features](#-main-features)
- [Project Structure](#-project-structure)
- [Technologies Used](#-technologies-used)
- [Installation & Setup](#-installation--setup)
- [Usage Guide](#-usage-guide)
- [How It Works: CDC](#-how-it-works-content-defined-chunking-cdc)
- [Testing](#-testing)
- [Data Models](#-data-models)
- [Educational Value](#-educational-value)
- [Future Improvements & Roadmap](#-future-improvements--roadmap)
- [Known Issues & Limitations](#-known-issues--limitations)
- [Author](#-author)
- [Documentation References](#-documentation-references)
- [Contributing](#-contributing)
- [FAQ](#-faq)
- [Reporting Issues](#-reporting-issues)

---

## 📚 Project Overview

**SaveMe** is an educational school project developed to prepare for exams, implementing an intelligent file versioning system. It demonstrates advanced concepts in version control, data chunking, and file management through a clean CLI interface.

The core innovation is the use of **Content Defined Chunking (CDC)**, a technique that divides files based on content boundaries rather than fixed sizes. This enables efficient detection of changes, smart deduplication, and minimal storage overhead.

### Key Concept
SaveMe tracks file changes at the chunk level rather than the entire file. When you modify a file, only the changed chunks are detected and stored, making it exceptionally efficient for:
- Detecting granular changes
- Minimizing storage space through chunk deduplication  
- Creating lightweight snapshots of file states
- Efficiently restoring previous versions

---

## 🎯 Main Features

### ✅ Repository Management
- Initialize a new SaveMe repository in any directory
- Automatic `.sm/` hidden folder structure for storing chunks and snapshots
- Comprehensive change tracking across all files
- **Support for multiple repository paths** in configuration

### ✅ Smart Change Detection
- Content-based chunking algorithm automatically detects what changed
- Efficient comparison of file versions at the chunk level
- Real-time reporting of detected changes

### ✅ Snapshot Creation (Commit)
- Create named snapshots of your repository state
- Each snapshot captures the exact state of all files with timestamps
- Automatic tracking of deleted files

### ✅ Version History
- List all available snapshots with dates and identifiers
- Quick access to previous versions
- Detailed snapshot metadata

### ✅ File Restoration
- Restore your workspace to any previous snapshot
- Automatic handling of deleted files
- Warning system for incomplete or corrupted snapshots

---

## 📁 Project Structure

```
SaveMe/
├── SaveMe/                          # Main application folder
│   ├── Program.cs                   # CLI entry point and command routing
│   ├── SaveMe.csproj                # Project configuration (.NET 9.0)
│   │
│   ├── Models/                      # Data models
│   │   ├── CommitFile.cs            # Represents a file in a snapshot
│   │   ├── Snapshots.cs             # Snapshot metadata structure
│   │   └── JsonContext.cs           # JSON serialization context
│   │
│   ├── Services/                    # Core business logic
│   │   ├── RepoService.cs           # Repository initialization and management
│   │   ├── SnapshotService.cs       # Snapshot creation and restoration
│   │   ├── ChunkService.cs          # Chunk detection and comparison
│   │   ├── CdcService.cs            # Content Defined Chunking algorithm
│   │   └── AppSettingsService.cs    # Application settings and path management
│   │
│   ├── Tests/                       # Unit tests
│   │   ├── SnapshotServiceTest.cs   # SnapshotService test suite (20+ tests)
│   │   ├── CdcServiceTest.cs        # CdcService test suite
│   │   └── Models/                  # Data model classes
│   │       ├── AppSettings.cs       # Application settings with multiple paths
│   │       ├── CommitFile.cs        # Represents a file in a snapshot
│   │       └── Snapshots.cs         # Snapshot metadata structure
│   │
│   └── Repositories/                # Data persistence layer
│
├── SCHEMA_FONCTIONNEL.md            # Detailed functional design document
├── TESTING_GUIDE.md                 # Comprehensive testing documentation
├── TESTS_README.md                  # Quick test reference
└── README.md                        # This file
```

### Repository Structure After Initialization

After running `init`, SaveMe creates a hidden `.sm/` folder:

```
.sm/ (hidden)
├── chunk_store/                     # Storage for file chunks
│   ├── abc123def456...txt          # Chunk files named by content hash
│   └── xyz789uvw012...txt
│
└── snapshots/                       # Snapshot history
    ├── snapshot_20260323143022.json
    ├── snapshot_20260323150100.json
    └── snapshot_20260324090000.json
```

---

## 🛠️ Technologies Used

- **Language**: C# (.NET 9.0)
- **Framework**: .NET Console Application
- **Testing**: xUnit framework
- **Mocking**: Moq library
- **Serialization**: System.Text.Json with POCO source generator
- **Platform**: Windows x64 (self-contained executable)

### Project Configuration
- `ImplicitUsings`: Enabled for cleaner code
- `Nullable`: Enabled for type safety
- `PublishSingleFile`: Single executable output
- `PublishTrimmed`: Optimized binary size

---

## 🚀 Installation & Setup

### Prerequisites
- **.NET 9.0 SDK** or higher
- **Windows 10/11** (x64)
- PowerShell or Command Prompt

### Clone the Repository

```bash
git clone https://github.com/LucasSimoesPolvora/SaveMe.git
cd SaveMe/SaveMe
```

### Build from Source

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Build in Release mode (optimized)
dotnet build -c Release
```

### Publish as Standalone Executable

```bash
# Create a self-contained executable
dotnet publish -c Release

# The executable will be in: bin/Release/net9.0/win-x64/publish/SaveMe.exe
```

### Run Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test -v detailed

# Run specific test class
dotnet test --filter "SnapshotServiceTest"
```

---

## ⚙️ Configuration

### AppSettings File

SaveMe stores configuration in `appsettings.json` located at:
```
%APPDATA%\SaveMe\appsettings.json
```

### Configuration Structure

The configuration file stores multiple backup paths:

```json
{
  "saveMePaths": [
    "C:\\MyProject",
    "C:\\Documents",
    "D:\\Work\\Data"
  ]
}
```

### Managing Paths

**Initialize a new path:**
```bash
SaveMe --init C:\\MyProject
```

**View all configured paths:**
The paths are stored in the `appsettings.json` file. Each path is a separate repository with its own `.sm/` folder.

---

### Getting Started

SaveMe is a **CLI (Command Line Interface)** application. All commands are run from the command line:

```bash
SaveMe.exe --[COMMAND] [OPTIONS]
```

### Available Commands

#### 1. **--help** / **-h** - Display Help
Shows all available commands and their descriptions.

```bash
SaveMe --help
SaveMe -h
```

#### 2. **--init** / **-i** - Initialize Repository
Creates a new SaveMe repository with specified path. Creates the `.sm/` hidden folder structure.

```bash
SaveMe --init <path>
SaveMe -i C:\MyProject
```

**Output Example**:
```
Repository initialized successfully!
.sm/ folder created with chunk_store and snapshots directories.
```

#### 3. **--commit** / **-c** - Create Snapshot
Analyzes all files in the repository, detects changes using CDC, and creates a timestamped snapshot.

```bash
SaveMe --commit
SaveMe -c
```

**Output Example**:
```
Analyzing files...
Snapshot created: snapshot_20260325143022.json
- Files tracked: 15
- New chunks: 12
- Unchanged chunks: 8
```

#### 4. **--backup** - Backup Management
Manage repository backups with various options.

##### Backup Operations:
```bash
# Create a backup (same as --commit)
SaveMe --backup

# Dry run - check changes without creating backup
SaveMe --backup --dry-run
```

#### 5. **--snapshots** - List Snapshots
Displays all available snapshots in the repository with their identifiers.

```bash
SaveMe --snapshots
```

**Output Example**:
```
Available snapshots:
 - 1: snapshot_20260323143022
 - 2: snapshot_20260323150100
 - 3: snapshot_20260324090000
```

#### 6. **--restore** - Restore Snapshot
Restores your workspace to a previous snapshot state. Requires snapshot number.

```bash
SaveMe --restore <snapshot_number>
SaveMe --restore 2
```

**Interactive Example**:
```
Enter the snapshot number to restore:
2
Restoring snapshot_20260323150100...
Restored 12 files
Deleted 1 files
Restore completed successfully!
```

---

## 💡 How It Works: Content Defined Chunking (CDC)

SaveMe's innovation is the **Content Defined Chunking** algorithm:

### Traditional Approach (Size-based)
```
File: [=================== 100 KB ===================]
Split into: [   25 KB   |   25 KB   |   25 KB   |   25 KB   ]
If 1 byte changes at position 26 KB, all 4 chunks are re-stored!
```

### SaveMe's Approach (Content-based)
```
File: [=================== 100 KB ===================]
Content-based split:
[  15 KB  |   28 KB   |   17 KB   |   21 KB   |   19 KB  ]
  hash1    hash2      hash3      hash4      hash5

If 1 byte changes at position 26 KB (in chunk 2):
Only chunk 2 gets a new hash → Much more efficient!
```

### Benefits
- **Smart Detection**: Identifies exact changes down to the chunk level
- **Deduplication**: Identical chunks are stored only once
- **Efficiency**: Minimal storage for large file modifications
- **Quick Comparison**: Fast change detection without comparing full files

---

## 🧪 Testing

SaveMe includes a comprehensive unit test suite using **xUnit** framework with **extensive test coverage**.

### Test Coverage

| Service | Tests | Coverage |
|---------|-------|----------|
| **SnapshotService** | 20+ | List, Restore, Efficiency comparison, Deleted files |
| **CdcService** | 20+ | Chunking, Fingerprinting, Content deduplication |
| **ChunkService** | 20+ | File chunking, Change detection, Reconstruction |
| **RepoService** | 20+ | Initialization, File enumeration, Verification |

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test -v detailed

# Run specific test class
dotnet test --filter "SnapshotServiceTest"

# Run with code coverage
dotnet test /p:CollectCoverage=true
```

### Test Structure
Tests follow the **Arrange-Act-Assert (AAA)** pattern:
- **Arrange**: Set up test data and mocks
- **Act**: Execute the method being tested
- **Assert**: Verify the expected outcomes

See `TESTS_README.md` for quick reference and `TESTING_GUIDE.md` for detailed documentation.

---

## 📊 Data Models

### AppSettings
Application configuration for managing multiple backup paths:
```json
{
  "saveMePaths": [
    "C:\\MyProject",
    "C:\\Documents",
    "D:\\Work\\Data"
  ]
}
```

### CommitFile
Represents a file within a snapshot:
```json
{
  "relativePath": "src/main.cs",
  "chunkFingerprints": ["hash1", "hash2", "hash3"]
}
```

### Snapshots
Complete snapshot structure:
```json
{
  "id": "snapshot_20260323143022",
  "commitFiles": [
    { "relativePath": "src/main.cs", "chunkFingerprints": [...] },
    { "relativePath": "README.md", "chunkFingerprints": [...] }
  ],
  "deletedFiles": ["old_file.txt"]
}
```

---

## 🚀 Future Improvements & Roadmap

### Planned Features
- [ ] **Compression Support**: Add GZIP/Brotli compression for chunks
- [ ] **Encryption**: Implement AES-256 encryption for sensitive data
- [ ] **Cloud Integration**: Support for AWS S3, Azure Blob, Google Cloud Storage
- [ ] **Incremental Restore**: Restore only specific files instead of entire snapshot
- [ ] **Differential Snapshots**: Store only changes between snapshots
- [ ] **Cross-Platform Support**: Build for Linux and macOS
- [ ] **GUI Application**: Develop a Windows Forms or WPF interface
- [ ] **Concurrent Operations**: Support parallel chunk processing
- [ ] **Snapshot Branching**: Create branches from snapshots
- [ ] **Merge Operations**: Merge snapshots from different branches

### Short Term (Next Release)
- [ ] Add `--compression` flag for backup operations
- [ ] Implement snapshot export/import functionality
- [ ] Add progress indicators for long-running operations
- [ ] Support for symbolic links

### Medium Term
- [ ] Web interface for remote management
- [ ] Database backend alternative to JSON
- [ ] Automated backup scheduling

---

## 🤝 Contributing

We welcome contributions! Here's how you can help:

### Getting Started
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests for new functionality
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Development Guidelines
- Follow C# naming conventions (PascalCase for classes, camelCase for variables)
- Write unit tests for all new features
- Ensure all tests pass before submitting PR
- Update documentation for API changes
- Keep commits atomic and descriptive

### Code Style
- Use 4 spaces for indentation
- Max line length: 120 characters
- Add XML documentation comments for public methods
- Use meaningful variable names

---

## ⚠️ Known Issues & Limitations
- **Windows Only**: Currently compiled for Windows x64 only
- **No Compression**: Chunks are stored uncompressed
- **No Encryption**: Snapshots are stored in plain JSON
- **Single Machine**: No cloud sync or multi-device support
- **Manual Restoration**: No automatic conflict resolution
- **Large Files**: No built-in support for very large files (>1GB)

### Workarounds
- For cross-platform support: Rebuild with `dotnet publish -c Release` on target OS
- For security: Manually encrypt the `.sm/` folder
- For cloud backup: Use external tools to sync `.sm/` folder

---

## 👤 Author

**Lucas Simões Póvora**  
GitHub: [@LucasSimoesPolvora](https://github.com/LucasSimoesPolvora)

---

## 📚 Documentation References

- **Functional Design**: See `SCHEMA_FONCTIONNEL.md` for detailed architecture and data flows
- **Testing Guide**: See `TESTING_GUIDE.md` for comprehensive test documentation
- **Quick Test Reference**: See `TESTS_README.md` for test suite overview

---

## 🐛 Reporting Issues

If you find a bug or have a suggestion:
1. Check existing issues on GitHub
2. Provide clear steps to reproduce
3. Include your environment details (.NET version, OS)
4. Open an issue with the label `bug` or `enhancement`

---

**Last Updated**: April 22, 2026  
**Status**: Educational Project | Active Development  
**.NET Version**: 9.0