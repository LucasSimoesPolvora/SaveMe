using System;
using System.Text.Json;
using Xunit;
using SaveMe.Models;

public class CdcServiceTest : IDisposable
{
    private string _originalWorkingDirectory;
    private string _testDirectory;
    private static readonly object _lockObject = new();

    public CdcServiceTest()
    {
        lock (_lockObject)
        {
            _originalWorkingDirectory = Directory.GetCurrentDirectory();
            _testDirectory = Path.Combine(Path.GetTempPath(), $"CdcServiceTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
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

    #region ChunkData Tests

    [Fact]
    public void ChunkData_WithEmptyData_ShouldReturnEmptyList()
    {
        // Arrange
        CdcService service = new();
        byte[] emptyData = [];

        // Act
        List<byte[]> chunks = service.ChunkData(emptyData);

        // Assert
        Assert.Empty(chunks);
    }

    [Fact]
    public void ChunkData_WithSmallData_ShouldReturnSingleChunk()
    {
        // Arrange
        CdcService service = new();
        byte[] smallData = [1, 2, 3, 4, 5];

        // Act
        List<byte[]> chunks = service.ChunkData(smallData);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.Single(chunks);
        Assert.Equal(smallData, chunks[0]);
    }

    [Fact]
    public void ChunkData_WithLargeData_ShouldReturnMultipleChunks()
    {
        // Arrange
        CdcService service = new();

        byte[] largeData = new byte[20000];
        for (int i = 0; i < largeData.Length; i++)
        {
            largeData[i] = (byte)(i % 256);
        }

        // Act
        List<byte[]> chunks = service.ChunkData(largeData);

        // Assert
        Assert.NotEmpty(chunks);
        Assert.True(chunks.Count > 1, "Large data should be split into multiple chunks");
        
        int totalSize = chunks.Sum(c => c.Length);
        Assert.Equal(largeData.Length, totalSize);
    }

    [Fact]
    public void ChunkData_ShouldRespectMaxChunkSize()
    {
        // Arrange
        CdcService service = new();
        byte[] largeData = new byte[15000];
        new Random(42).NextBytes(largeData);

        // Act
        List<byte[]> chunks = service.ChunkData(largeData);

        // Assert
        const int maxChunkSize = 4096 * 2;
        foreach (byte[] chunk in chunks)
        {
            Assert.True(chunk.Length <= maxChunkSize, $"Chunk size {chunk.Length} exceeds max {maxChunkSize}");
        }
    }

    [Fact]
    public void ChunkData_ShouldRespectMinChunkSize()
    {
        // Arrange
        CdcService service = new CdcService();
        byte[] largeData = new byte[20000];
        new Random(42).NextBytes(largeData);

        // Act
        List<byte[]> chunks = service.ChunkData(largeData);

        // Assert
        const int minChunkSize = 4096 / 2;
        for (int i = 0; i < chunks.Count - 1; i++)
        {
            Assert.True(chunks[i].Length >= minChunkSize, 
                $"Chunk {i} size {chunks[i].Length} is below min {minChunkSize}");
        }
    }

    [Fact]
    public void ChunkData_WithConsistentInput_ShouldProduceConsistentOutput()
    {
        // Arrange
        CdcService service = new CdcService();
        byte[] data = new byte[50000];
        new Random(12345).NextBytes(data);

        // Act
        List<byte[]> chunks1 = service.ChunkData(data);
        List<byte[]> chunks2 = service.ChunkData(data);

        // Assert
        Assert.Equal(chunks1.Count, chunks2.Count);
        for (int i = 0; i < chunks1.Count; i++)
        {
            Assert.Equal(chunks1[i], chunks2[i]);
        }
    }

    [Fact]
    public void ChunkData_ShouldNotProduceEmptyChunks()
    {
        // Arrange
        CdcService service = new CdcService();
        byte[] data = new byte[100000];
        new Random(99).NextBytes(data);

        // Act
        List<byte[]> chunks = service.ChunkData(data);

        // Assert
        foreach (byte[] chunk in chunks)
        {
            Assert.NotEmpty(chunk);
        }
    }

    [Fact]
    public void ChunkData_WithBinaryData_ShouldChunkCorrectly()
    {
        // Arrange
        CdcService service = new CdcService();
        byte[] binaryData = new byte[30000];
        for (int i = 0; i < binaryData.Length; i++)
        {
            binaryData[i] = (byte)(i % 256);
        }

        // Act
        List<byte[]> chunks = service.ChunkData(binaryData);

        // Assert
        Assert.NotEmpty(chunks);
        int totalSize = chunks.Sum(c => c.Length);
        Assert.Equal(binaryData.Length, totalSize);
    }

    [Fact]
    public void ChunkData_WithRepeatingPattern_ShouldChunkCorrectly()
    {
        // Arrange
        CdcService service = new CdcService();
        byte[] pattern = System.Text.Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOP");
        byte[] repeatingData = new byte[50000];
        for (int i = 0; i < repeatingData.Length; i++)
        {
            repeatingData[i] = pattern[i % pattern.Length];
        }

        // Act
        List<byte[]> chunks = service.ChunkData(repeatingData);

        // Assert
        Assert.NotEmpty(chunks);
        byte[] reconstructed = chunks.SelectMany(c => c).ToArray();
        Assert.Equal(repeatingData, reconstructed);
    }

    [Fact]
    public void ChunkData_WithSingleByteChunks_ShouldCreateValidChunks()
    {
        // Arrange
        CdcService service = new CdcService();
        byte[] singleByte = [42];

        // Act
        List<byte[]> chunks = service.ChunkData(singleByte);

        // Assert
        Assert.Single(chunks);
        Assert.Equal([42], chunks[0]);
    }

    #endregion

    #region CalculateChunkFingerprint Tests

    [Fact]
    public void CalculateChunkFingerprint_WithValidData_ShouldReturnBase64String()
    {
        // Arrange
        byte[] data = [1, 2, 3, 4, 5];

        // Act
        string fingerprint = CdcService.CalculateChunkFingerprint(data);

        // Assert
        Assert.NotEmpty(fingerprint);
        Assert.True(fingerprint.Length % 4 == 0);
        byte[] decoded = Convert.FromBase64String(fingerprint);
        Assert.NotEmpty(decoded);
    }

    [Fact]
    public void CalculateChunkFingerprint_WithIdenticalData_ShouldReturnIdenticalFingerprint()
    {
        // Arrange
        byte[] data = [10, 20, 30, 40, 50];

        // Act
        string fingerprint1 = CdcService.CalculateChunkFingerprint(data);
        string fingerprint2 = CdcService.CalculateChunkFingerprint(data);

        // Assert
        Assert.Equal(fingerprint1, fingerprint2);
    }

    [Fact]
    public void CalculateChunkFingerprint_WithDifferentData_ShouldReturnDifferentFingerprint()
    {
        // Arrange
        byte[] data1 = [1, 2, 3, 4, 5];
        byte[] data2 = [1, 2, 3, 4, 6];

        // Act
        string fingerprint1 = CdcService.CalculateChunkFingerprint(data1);
        string fingerprint2 = CdcService.CalculateChunkFingerprint(data2);

        // Assert
        Assert.NotEqual(fingerprint1, fingerprint2);
    }

    [Fact]
    public void CalculateChunkFingerprint_WithLargeData_ShouldReturnValidFingerprint()
    {
        // Arrange
        byte[] largeData = new byte[1000000];
        new Random(42).NextBytes(largeData);

        // Act
        string fingerprint = CdcService.CalculateChunkFingerprint(largeData);

        // Assert
        Assert.NotEmpty(fingerprint);
        Assert.Equal(44, fingerprint.Length);
    }

    [Fact]
    public void CalculateChunkFingerprint_WithEmptyData_ShouldReturnFingerprint()
    {
        // Arrange
        byte[] emptyData = Array.Empty<byte>();

        // Act
        string fingerprint = CdcService.CalculateChunkFingerprint(emptyData);

        // Assert
        Assert.NotEmpty(fingerprint);
        Assert.Equal(44, fingerprint.Length);
    }

    [Fact]
    public void CalculateChunkFingerprint_WithLongString_ShouldReturnConsistentFingerprint()
    {
        // Arrange
        byte[] data1 = System.Text.Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
        byte[] data2 = System.Text.Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");

        // Act
        string fp1 = CdcService.CalculateChunkFingerprint(data1);
        string fp2 = CdcService.CalculateChunkFingerprint(data2);

        // Assert
        Assert.Equal(fp1, fp2);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ChunkData_AndCalculateFingerprintForEachChunk_ShouldProduceDifferentFingerprints()
    {
        // Arrange
        CdcService service = new CdcService();
        byte[] data = new byte[100000];
        new Random(777).NextBytes(data);

        // Act
        List<byte[]> chunks = service.ChunkData(data);
        List<string> fingerprints = chunks.Select(CdcService.CalculateChunkFingerprint).ToList();

        // Assert
        Assert.NotEmpty(chunks);
        Assert.Equal(fingerprints.Count, fingerprints.Distinct().Count());
    }

    [Fact]
    public void ChunkData_WithModifiedData_ShouldProduceDifferentFingerprints()
    {
        // Arrange
        CdcService service = new CdcService();
        byte[] originalData = new byte[50000];
        new Random(888).NextBytes(originalData);

        byte[] modifiedData = (byte[])originalData.Clone();
        modifiedData[25000] ^= 0xFF;

        // Act
        List<byte[]> originalChunks = service.ChunkData(originalData);
        List<byte[]> modifiedChunks = service.ChunkData(modifiedData);

        List<string> originalFingerprints = originalChunks.Select(CdcService.CalculateChunkFingerprint).ToList();
        List<string> modifiedFingerprints = modifiedChunks.Select(CdcService.CalculateChunkFingerprint).ToList();

        // Assert
        Assert.NotEqual(originalFingerprints, modifiedFingerprints);
    }

    [Fact]
    public void ContentAddressableStorage_ShouldDetectDuplicateChunks()
    {
        // Arrange
        CdcService service = new();
        byte[] data1 = new byte[50000];
        byte[] data2 = new byte[50000];
        
        new Random(555).NextBytes(data1);
        Array.Copy(data1, data2, data1.Length);

        // Act
        var chunks1 = service.ChunkData(data1);
        var chunks2 = service.ChunkData(data2);

        var fingerprints1 = chunks1.Select(CdcService.CalculateChunkFingerprint).ToList();
        var fingerprints2 = chunks2.Select(CdcService.CalculateChunkFingerprint).ToList();

        // Assert
        Assert.Equal(fingerprints1, fingerprints2);
    }

    [Fact]
    public void ContentAddressableStorage_WithPartiallyModifiedData_ShouldReuseUnchangedChunks()
    {
        // Arrange
        CdcService service = new CdcService();
        byte[] originalData = new byte[100000];
        new Random(666).NextBytes(originalData);

        byte[] modifiedData = (byte[])originalData.Clone();
        for (int i = 90000; i < modifiedData.Length; i++)
        {
            modifiedData[i] ^= 0xAA;
        }

        // Act
        List<byte[]> originalChunks = service.ChunkData(originalData);
        List<byte[]> modifiedChunks = service.ChunkData(modifiedData);

        HashSet<string> originalFingerprints = originalChunks.Select(CdcService.CalculateChunkFingerprint).ToHashSet();
        List<string> modifiedFingerprints = modifiedChunks.Select(CdcService.CalculateChunkFingerprint).ToList();

        // Assert
        int reuseableChunks = modifiedFingerprints.Where(fp => originalFingerprints.Contains(fp)).Count();
        Assert.True(reuseableChunks > 0, "Should reuse some chunks from original data");
    }

    #endregion
}
