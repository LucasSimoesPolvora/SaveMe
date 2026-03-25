using System.Text.Json;
using Xunit;
using SaveMe.Models;

public class SnapshotServiceTest : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _snapshotDirectory;
    private readonly string _chunkStoreDirectory;
    private readonly string _originalWorkingDirectory;
    private static readonly object _lockObject = new();

    public SnapshotServiceTest()
    {
        lock (_lockObject)
        {
            // Store original working directory
            _originalWorkingDirectory = Directory.GetCurrentDirectory();
            
            // Create a temporary test directory
            _testDirectory = Path.Combine(Path.GetTempPath(), $"SaveMeTest_{Guid.NewGuid()}");
            _snapshotDirectory = Path.Combine(_testDirectory, ".sm", "snapshots");
            _chunkStoreDirectory = Path.Combine(_testDirectory, ".sm", "chunk_store");
            
            Directory.CreateDirectory(_snapshotDirectory);
            Directory.CreateDirectory(_chunkStoreDirectory);
            
            // Change working directory to test directory
            Directory.SetCurrentDirectory(_testDirectory);
        }
    }

    public void Dispose()
    {
        lock (_lockObject)
        {
            try
            {
                Directory.SetCurrentDirectory(_originalWorkingDirectory);
                
                Thread.Sleep(50);
            }
            catch { }
            
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch { }
            }
        }
    }

    #region ListSnapshots Tests

    [Fact]
    public void ListSnapshots_WithNoSnapshots_ShouldDisplayMessage()
    {
        // Arrange
        SnapshotService service = new();

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);
            // Act
            service.ListSnapshots();

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);
            Assert.Contains("No snapshots found", output);
        }
    }

    [Fact]
    public void ListSnapshots_WithMultipleSnapshots_ShouldDisplayAll()
    {
        // Arrange
        SnapshotService service = new();

        File.WriteAllText(Path.Combine(_snapshotDirectory, "snapshot_001.json"), "{}");
        File.WriteAllText(Path.Combine(_snapshotDirectory, "snapshot_002.json"), "{}");
        File.WriteAllText(Path.Combine(_snapshotDirectory, "snapshot_003.json"), "{}");

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.ListSnapshots();

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            Assert.Contains("Snapshots:", output);
            Assert.Contains("snapshot_001.json", output);
            Assert.Contains("snapshot_002.json", output);
            Assert.Contains("snapshot_003.json", output);
        }
    }

    [Fact]
    public void ListSnapshots_WithSnapshots_ShouldNumberThem()
    {
        // Arrange
        SnapshotService service = new SnapshotService();

        File.WriteAllText(Path.Combine(_snapshotDirectory, "snapshot_a.json"), "{}");
        File.WriteAllText(Path.Combine(_snapshotDirectory, "snapshot_b.json"), "{}");

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.ListSnapshots();

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            Assert.Contains("- 1:", output);
            Assert.Contains("- 2:", output);
        }
    }

    #endregion

    #region RestoreSnapshot Tests

    [Fact]
    public void RestoreSnapshot_WithInvalidNumber_ShouldDisplayError()
    {
        // Arrange
        SnapshotService service = new();

        File.WriteAllText(Path.Combine(_snapshotDirectory, "snapshot_001.json"), "{}");

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.RestoreSnapshot(99);

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            Assert.Contains("Invalid snapshot number", output);
        }
    }

    [Fact]
    public void RestoreSnapshot_WithNoSnapshots_ShouldDisplayError()
    {
        // Arrange
        SnapshotService service = new();

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.RestoreSnapshot(1);

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            Assert.Contains("No snapshots found", output);
        }
    }

    [Fact]
    public void RestoreSnapshot_WithValidSnapshot_ShouldRestoreFiles()
    {
        // Arrange
        SnapshotService service = new();

        Snapshots snapshot = new Snapshots
        {
            Id = "snapshot_001",
            CommitFiles =
            [
                new CommitFile("test_file.txt", ["hash1", "hash2"])
            ],
            DeletedFiles = []
        };

        string snapshotPath = Path.Combine(_snapshotDirectory, "snapshot_20260325123456.json");
        JsonContext context = new();
        string json = JsonSerializer.Serialize(snapshot, typeof(Snapshots), context);
        File.WriteAllText(snapshotPath, json);

        File.WriteAllBytes(Path.Combine(_chunkStoreDirectory, "hash1.txt"), new byte[] { 1, 2, 3 });
        File.WriteAllBytes(Path.Combine(_chunkStoreDirectory, "hash2.txt"), new byte[] { 4, 5, 6 });

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.RestoreSnapshot(1);

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            Assert.Contains("Restoring snapshot", output);
            Assert.Contains("Snapshot restore complete", output);
        }
    }

    [Fact]
    public void RestoreSnapshot_WithDeletedFiles_ShouldDeleteFilesInSnapshot()
    {
        // Arrange
        SnapshotService service = new();

        string fileToDelete = Path.Combine(_testDirectory, "to_delete.txt");
        File.WriteAllText(fileToDelete, "delete me");

        Snapshots snapshot = new()
        {
            Id = "snapshot_001",
            CommitFiles = Array.Empty<CommitFile>(),
            DeletedFiles = ["to_delete.txt"]
        };

        string snapshotPath = Path.Combine(_snapshotDirectory, "snapshot_20260325123456.json");
        JsonContext context = new();
        string json = JsonSerializer.Serialize(snapshot, typeof(Snapshots), context);
        File.WriteAllText(snapshotPath, json);

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.RestoreSnapshot(1);

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            Assert.Contains("Deleted: to_delete.txt", output);
            Assert.False(File.Exists(fileToDelete), "File should have been deleted");
        }
    }

    [Fact]
    public void RestoreSnapshot_WithMissingChunks_ShouldWarnButContinue()
    {
        // Arrange
        SnapshotService service = new();

        Snapshots snapshot = new Snapshots
        {
            Id = "snapshot_001",
            CommitFiles =
            [
                new CommitFile("test_file.txt", ["missing_hash"])
            ],
            DeletedFiles = []
        };

        string snapshotPath = Path.Combine(_snapshotDirectory, "snapshot_20260325123456.json");
        JsonContext context = new();
        string json = JsonSerializer.Serialize(snapshot, typeof(Snapshots), context);
        File.WriteAllText(snapshotPath, json);

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.RestoreSnapshot(1);

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            Assert.Contains("Warning: Chunk missing_hash not found", output);
            Assert.Contains("Snapshot restore complete", output);
        }
    }

    [Fact]
    public void RestoreSnapshot_WithNestedDirectories_ShouldCreateDirectories()
    {
        // Arrange
        SnapshotService service = new();

        Snapshots snapshot = new Snapshots
        {
            Id = "snapshot_001",
            CommitFiles =
            [
                new CommitFile("nested/path/to/file.txt", ["hash1"])
            ],
            DeletedFiles = []
        };

        string snapshotPath = Path.Combine(_snapshotDirectory, "snapshot_20260325123456.json");
        JsonContext context = new();
        string json = JsonSerializer.Serialize(snapshot, typeof(Snapshots), context);
        File.WriteAllText(snapshotPath, json);

        File.WriteAllBytes(Path.Combine(_chunkStoreDirectory, "hash1.txt"), new byte[] { 1, 2, 3 });

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.RestoreSnapshot(1);

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            string nestedPath = Path.Combine(_testDirectory, "nested", "path", "to");
            Assert.True(Directory.Exists(nestedPath), "Nested directories should be created");
            Assert.Contains("Restored: nested/path/to/file.txt", output);
        }
    }

    [Fact]
    public void RestoreSnapshot_ShouldSelectCorrectSnapshot_ByNumber()
    {
        // Arrange
        SnapshotService service = new();

        Snapshots snapshot1 = new()
        {
            Id = "snapshot_001",
            CommitFiles = [new CommitFile("file1.txt", ["hash1"])],
            DeletedFiles = Array.Empty<string>()
        };

        Snapshots snapshot2 = new()
        {
            Id = "snapshot_002",
            CommitFiles = [new CommitFile("file2.txt", ["hash2"])],
            DeletedFiles = Array.Empty<string>()
        };

        JsonContext context = new();

        File.WriteAllText(Path.Combine(_snapshotDirectory, "snapshot_001.json"), 
            JsonSerializer.Serialize(snapshot1, typeof(Snapshots), context));
        File.WriteAllText(Path.Combine(_snapshotDirectory, "snapshot_002.json"), 
            JsonSerializer.Serialize(snapshot2, typeof(Snapshots), context));

        File.WriteAllBytes(Path.Combine(_chunkStoreDirectory, "hash1.txt"), [1, 2, 3]);
        File.WriteAllBytes(Path.Combine(_chunkStoreDirectory, "hash2.txt"), [4, 5, 6]);

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.RestoreSnapshot(1);

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            Assert.Contains("Restored:", output);
        }
    }

    #endregion

    #region CompareEfficiency Tests

    [Fact]
    public void CompareEfficiency_WithNoSnapshots_ShouldReturnSilently()
    {
        // Arrange
        SnapshotService service = new();

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.CompareEfficiency();

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            Assert.Empty(output.Trim());
        }
    }

    [Fact]
    public void CompareEfficiency_WithValidSnapshot_ShouldCalculateRatio()
    {
        // Arrange
        SnapshotService service = new();
    
        string testFile = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(testFile, "test content with some data");

        Snapshots snapshot = new()
        {
            Id = "snapshot_001",
            CommitFiles =
            [
                new CommitFile("test.txt", [ "hash1", "hash2" ])
            ],
            DeletedFiles = []
        };

        string snapshotPath = Path.Combine(_snapshotDirectory, "snapshot_20260325123456.json");
        JsonContext context = new();
        string json = JsonSerializer.Serialize(snapshot, typeof(Snapshots), context);
        File.WriteAllText(snapshotPath, json);

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.CompareEfficiency();

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            Assert.Contains("Seulement", output);
            Assert.Contains("de nouvelles données écrites pour cette sauvegarde", output);
        }
    }

    [Fact]
    public void CompareEfficiency_ShouldCalculateCorrectPercentage()
    {
        // Arrange
        SnapshotService service = new();

        string testFile = Path.Combine(_testDirectory, "test_data.txt");
        File.WriteAllText(testFile, "12345");

        Snapshots snapshot = new()
        {
            Id = "snapshot_001",
            CommitFiles =
            [
                new CommitFile("test_data.txt", [ "abc", "def" ]) 
            ],
            DeletedFiles = []
        };

        string snapshotPath = Path.Combine(_snapshotDirectory, "snapshot_20260325123456.json");
        JsonContext context = new();
        string json = JsonSerializer.Serialize(snapshot, typeof(Snapshots), context);
        File.WriteAllText(snapshotPath, json);

        TextWriter originalOut = Console.Out;
        using (StringWriter writer = new())
        {
            Console.SetOut(writer);

            // Act
            service.CompareEfficiency();

            // Assert
            string output = writer.ToString();
            Console.SetOut(originalOut);

            Assert.Contains("Seulement", output);
            Assert.Contains("de nouvelles données écrites", output);
        }
    }
    #endregion

    #region Integration Tests

    [Fact]
    public void RestoreSnapshot_AfterListSnapshots_ShouldWorkCorrectly()
    {
        // Arrange
        SnapshotService service = new();

        Snapshots snapshot = new()
        {
            Id = "snapshot_001",
            CommitFiles = [new CommitFile("data.txt", ["chunk1"])],
            DeletedFiles = []
        };

        string snapshotPath = Path.Combine(_snapshotDirectory, "snapshot_20260325123456.json");
        JsonContext context = new();
        string json = JsonSerializer.Serialize(snapshot, typeof(Snapshots), context);
        File.WriteAllText(snapshotPath, json);

        File.WriteAllBytes(Path.Combine(_chunkStoreDirectory, "chunk1.txt"), [1, 2, 3]);

        TextWriter originalOut = Console.Out;

        
        using (StringWriter writer = new())
        {
            // Act - List snapshots
            Console.SetOut(writer);
            service.ListSnapshots();
            string listOutput = writer.ToString();
            Console.SetOut(originalOut);

            // Assert
            Assert.Contains("Snapshots:", listOutput);

            // Act - Restore snapshot
            Console.SetOut(writer);
            service.RestoreSnapshot(1);
            string restoreOutput = writer.ToString();
            Console.SetOut(originalOut);

            // Assert
            Assert.Contains("Restoring snapshot", restoreOutput);
            Assert.Contains("Snapshot restore complete", restoreOutput);
        }
    }

    #endregion
}
