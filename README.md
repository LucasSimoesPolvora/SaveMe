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
- [License](#-license)
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
│   │   └── CdcService.cs            # Content Defined Chunking algorithm
│   │
│   ├── Tests/                       # Unit tests
│   │   ├── SnapshotServiceTest.cs   # SnapshotService test suite (16 tests)
│   │   └── CdcServiceTest.cs        # CdcService test suite
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

## 📖 Usage Guide

### Getting Started

SaveMe is a **CLI (Command Line Interface)** application. All commands are run from the command line:

```bash
SaveMe.exe [COMMAND] [OPTIONS]
```

### Available Commands

#### 1. **help** / **h** - Display Help
Shows all available commands and their descriptions.

```bash
SaveMe help
SaveMe h
```

#### 2. **init** / **i** - Initialize Repository
Creates a new SaveMe repository in the current directory. Creates the `.sm/` hidden folder structure.

```bash
SaveMe init
SaveMe i
```

**Output Example**:
```
Repository initialized successfully!
.sm/ folder created with chunk_store and snapshots directories.
```

#### 3. **commit** / **c** - Create Snapshot
Analyzes all files in the repository, detects changes using CDC, and creates a timestamped snapshot.

```bash
SaveMe commit
SaveMe c
```

**Output Example**:
```
Analyzing files...
Snapshot created: snapshot_20260325143022.json
- Files tracked: 15
- New chunks: 12
- Unchanged chunks: 8
```

#### 4. **check** / **ch** - Check for Changes
Scans the repository for file modifications and displays detected changes.

```bash
SaveMe check
SaveMe ch
```

**Output Example**:
```
Checking for changes...
Changes detected in file: src/main.cs (3 new chunks)
Changes detected in file: README.md (1 new chunk)
No changes: data.json
```

#### 5. **snapshots** / **s** - List Snapshots
Displays all available snapshots in the repository with their identifiers.

```bash
SaveMe snapshots
SaveMe s
```

**Output Example**:
```
Available snapshots:
 - 1: snapshot_20260323143022
 - 2: snapshot_20260323150100
 - 3: snapshot_20260324090000
```

#### 6. **restore** / **r** - Restore Snapshot
Restores your workspace to a previous snapshot state. You'll be prompted to enter the snapshot number.

```bash
SaveMe restore
SaveMe r
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

SaveMe includes a comprehensive unit test suite using **xUnit** framework with **76 total tests**.

### Test Coverage

| Service | Tests | Coverage |
|---------|-------|----------|
| **SnapshotService** | 16 | List, Restore, Efficiency comparison |
| **CdcService** | 20 | Chunking, Fingerprinting, Content deduplication |
| **ChunkService** | 20 | File chunking, Change detection, Reconstruction |
| **RepoService** | 20 | Initialization, File enumeration, Verification |

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

## 🎓 Educational Value

This project demonstrates:

### Software Engineering Concepts
- **Service-Oriented Architecture**: Separation of concerns (Services)
- **Data Models**: POCO (Plain Old C# Object) design
- **Unit Testing**: xUnit framework with mocking
- **JSON Serialization**: Modern .NET approaches

### Algorithms & Data Structures
- **Content Defined Chunking**: Advanced algorithm for change detection
- **Hash-based Deduplication**: Efficient storage management
- **File System Traversal**: Recursive directory scanning

### .NET/C# Best Practices
- Nullable reference types for safety
- Implicit usings for cleaner code
- JSON context source generators
- CLI argument handling

---

## 🚧 Future Improvements & Roadmap

### Version 2.0 Enhancements
- [ ] **Compression**: GZIP compression for stored chunks
- [ ] **Encryption**: AES-256 encryption for sensitive snapshots
- [ ] **Diff Viewing**: Command to view differences between snapshots
- [ ] **Tagging**: Add descriptive tags to snapshots
- [ ] **Branching**: Support multiple snapshot branches
- [ ] **Sync**: Backup snapshots to cloud storage
- [ ] **GUI**: Windows Forms or WPF interface
- [ ] **Cross-platform**: Support for macOS and Linux

### Performance Optimizations
- [ ] Parallel chunk processing for large files
- [ ] Incremental snapshots (delta storage)
- [ ] Indexed snapshot search
- [ ] Memory-mapped files for large files

### Developer Experience
- [ ] Configuration file for custom chunk boundaries
- [ ] Verbose logging levels
- [ ] Snapshot comparison and merge tools
- [ ] Integration with Git for hybrid version control

---

## ⚠️ Known Issues & Limitations

### Current Limitations
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

## 🤝 Contributing

This is an educational project. Contributions for educational purposes are welcome:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Please ensure:
- Tests pass (`dotnet test`)
- Code follows C# conventions
- New features include unit tests
- Documentation is updated

---

## ❓ FAQ

### Q: What's the difference between "commit" and "check"?
**A**: `check` only detects changes but doesn't save them. `commit` detects changes AND creates a snapshot that you can later restore.

### Q: Can I restore to the middle of a snapshot's history?
**A**: Yes! Use `snapshots` to list all snapshots, then `restore` to choose any snapshot by number.

### Q: Is my data safe?
**A**: SaveMe stores snapshots in JSON format. Always maintain backups of important data in multiple locations.

### Q: How much space does SaveMe use?
**A**: It depends on your files and the CDC chunking. Due to deduplication, identical chunks are stored only once, making it very space-efficient compared to full file backups.

### Q: Can I use SaveMe for production?
**A**: SaveMe is an educational project. For production version control, use Git or professional VCS solutions.

---

## 🐛 Reporting Issues

If you find a bug or have a suggestion:
1. Check existing issues on GitHub
2. Provide clear steps to reproduce
3. Include your environment details (.NET version, OS)
4. Open an issue with the label `bug` or `enhancement`

---

**Last Updated**: March 25, 2026  
**Status**: Educational Project | Stable Release  
**.NET Version**: 9.0