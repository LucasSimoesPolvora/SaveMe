using System.Text.Json;
using SaveMe.Models;
public class SnapshotService
{
    readonly RepoService repoService;
    readonly ChunkService chunkService;

    public SnapshotService(RepoService? repoService = null)
    {
        this.repoService = repoService ?? new RepoService();
        chunkService = new(this.repoService);
    }
    public void CreateSnapshot()
    {
        if(!RepoService.CheckRepo(repoService.GetRepositoryBasePath())) return;

        DirectoryInfo dir = new(repoService.GetSnapshotsPath());
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string snapshotId = $"snapshot_{timestamp}";
        string filePath = $"{dir.FullName}\\{snapshotId}.json";

        repoService.GetFilesRecursively(Directory.GetCurrentDirectory());

        Snapshots snapshot = new()
        {
            Id = snapshotId,
            CommitFiles = Array.Empty<CommitFile>(),
            DeletedFiles = Array.Empty<string>()
        };

        
        repoService.trackedFiles.ForEach((file) => {
            string relativePath = RepoService.GetRelativePath(file.FullName);
            
            if(chunkService.HasChanges(file)){
                List<string> chunkFingerprints = ChunkService.GetChunkFingerprintsByFile(file);
                CommitFile commitFile = new(relativePath, chunkFingerprints);
                snapshot.CommitFiles = [.. snapshot.CommitFiles, commitFile];
            }
            else if (WasFileDeleted(relativePath))
            {
                List<string> chunkFingerprints = ChunkService.GetChunkFingerprintsByFile(file);
                CommitFile commitFile = new(relativePath, chunkFingerprints);
                snapshot.CommitFiles = [.. snapshot.CommitFiles, commitFile];
                Console.WriteLine($"File resurrected: {relativePath}");
            }
        });

        string[] deletedFiles = GetDeletedFiles();
        if (deletedFiles.Length > 0)
        {
            snapshot.DeletedFiles = deletedFiles;
        }

        chunkService.CommitChunks();
        
        JsonSerializerOptions options = new() { WriteIndented = true };
        JsonContext context = new();
        string json = JsonSerializer.Serialize(snapshot, typeof(Snapshots), context);

        CompareEfficiency();
        
        File.WriteAllText(filePath, json);
    }

    public void ListSnapshots()
    {
        DirectoryInfo dir = new(repoService.GetSnapshotsPath());
        FileInfo[] snapshotFiles = dir.GetFiles("*.json");
        if (!dir.Exists || snapshotFiles.Length == 0)
        {
            Console.WriteLine("No snapshots found.");
            return;
        }

        Console.WriteLine("Snapshots:");
        int i = 0;
        foreach (FileInfo file in snapshotFiles)
        {
            Console.WriteLine($"- {++i}: {file.Name}");
        }
    }

    private string[] GetDeletedFiles()
    {
        DirectoryInfo snapshotDir = new(repoService.GetSnapshotsPath());
        FileInfo[] snapshotFiles = snapshotDir.GetFiles("*.json");
        
        if (snapshotFiles.Length == 0)
        {
            return [];
        }

        FileInfo lastSnapshotFile = snapshotFiles.OrderByDescending(f => f.Name).First();
        string lastSnapshotJson = File.ReadAllText(lastSnapshotFile.FullName);
        JsonContext context = new();
        Snapshots? lastSnapshot = JsonSerializer.Deserialize<Snapshots>(lastSnapshotJson, context.Snapshots);


        if (lastSnapshot == null)
        {
            return Array.Empty<string>();
        }

        HashSet<string> currentFiles = [.. repoService.trackedFiles.Select(f => RepoService.GetRelativePath(f.FullName))];

        List<string> deletedFiles = new();
        foreach (CommitFile commitFile in lastSnapshot.CommitFiles)
        {
            if (!currentFiles.Contains(commitFile.Id))
            {
                deletedFiles.Add(commitFile.Id);
                Console.WriteLine($"File deleted: {commitFile.Id}");
            }
        }

        return deletedFiles.ToArray();
    }

    private bool WasFileDeleted(string filePath)
    {
        DirectoryInfo snapshotDir = new(repoService.GetSnapshotsPath());
        FileInfo[] snapshotFiles = snapshotDir.GetFiles("*.json");
        
        if (snapshotFiles.Length == 0)
        {
            return false;
        }

        FileInfo lastSnapshotFile = snapshotFiles.OrderByDescending(f => f.Name).First();
        string lastSnapshotJson = File.ReadAllText(lastSnapshotFile.FullName);
        JsonContext context = new();
        Snapshots? lastSnapshot = JsonSerializer.Deserialize<Snapshots>(lastSnapshotJson, context.Snapshots);


        if (lastSnapshot?.DeletedFiles == null)
        {
            return false;
        }

        return lastSnapshot.DeletedFiles.Contains(filePath);
    }

    public void RestoreSnapshot(int snapshotNumber, string restorePath)
    {
        DirectoryInfo dir = new(Path.Combine(restorePath, ".sm", "snapshots"));
        FileInfo[] snapshotFiles = dir.GetFiles("*.json");
        
        if (!dir.Exists || snapshotFiles.Length == 0)
        {
            Console.WriteLine("No snapshots found.");
            return;
        }

        if (snapshotNumber < 1 || snapshotNumber > snapshotFiles.Length)
        {
            Console.WriteLine($"Invalid snapshot number. Please choose a number between 1 and {snapshotFiles.Length}.");
            return;
        }

        snapshotFiles = snapshotFiles.OrderByDescending(f => f.Name).ToArray();
        FileInfo selectedSnapshotFile = snapshotFiles[snapshotNumber - 1];
        
        string snapshotJson = File.ReadAllText(selectedSnapshotFile.FullName);
        JsonContext context = new();
        Snapshots? snapshot = JsonSerializer.Deserialize<Snapshots>(snapshotJson, context.Snapshots);

        if (snapshot == null)
        {
            Console.WriteLine("Failed to deserialize snapshot.");
            return;
        }

        Console.WriteLine($"Restoring snapshot: {selectedSnapshotFile.Name}");
        
        string currentDir = Directory.GetCurrentDirectory();
        DirectoryInfo chunkStoreDir = new(repoService.GetChunkStorePath());

        foreach (CommitFile commitFile in snapshot.CommitFiles)
        {
            string filePath = Path.Combine(currentDir, commitFile.Id);
            string? fileDir = Path.GetDirectoryName(filePath);

            if (fileDir != null && !Directory.Exists(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }

            File.Create(filePath).Close();

            foreach (string fingerprint in commitFile.Chunks)
            {
                string safeHash = fingerprint.Replace("/", "_");
                string chunkFilePath = $"{chunkStoreDir.FullName}\\{safeHash}.txt";

                if (File.Exists(chunkFilePath))
                {
                    byte[] chunkData = File.ReadAllBytes(chunkFilePath);
                    chunkService.WriteChunkToFile(chunkData, filePath);
                }
                else
                {
                    Console.WriteLine($"Warning: Chunk {safeHash} not found in chunk store for file {commitFile.Id}");
                }
            }

            Console.WriteLine($"Restored: {commitFile.Id}");
        }

        if (snapshot.DeletedFiles != null && snapshot.DeletedFiles.Length > 0)
        {
            foreach (string deletedFile in snapshot.DeletedFiles)
            {
                string filePath = Path.Combine(currentDir, deletedFile);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"Deleted: {deletedFile}");
                }
            }
        }

        Console.WriteLine("Snapshot restore complete.");
    }

    public void CompareEfficiency(){
        DirectoryInfo dir = new(repoService.GetSnapshotsPath());
        FileInfo[] snapshotFiles = dir.GetFiles("*.json");
        
        if (!dir.Exists || snapshotFiles.Length == 0)
        {
            return;
        }
    
        FileInfo lastSnapshotFile = snapshotFiles.OrderByDescending(f => f.Name).First();
        string lastSnapshotJson = File.ReadAllText(lastSnapshotFile.FullName);
        JsonContext context = new();
        Snapshots? lastSnapshot = JsonSerializer.Deserialize<Snapshots>(lastSnapshotJson, context.Snapshots);
    
        if (lastSnapshot == null)
        {
            Console.WriteLine("Failed to deserialize snapshot.");
            return;
        }

        long totalChunkSize = lastSnapshot.CommitFiles.Sum(cf => cf.Chunks.Sum(chunk => chunk.Length));
        long totalFileSize = repoService.trackedFiles.Sum(f => f.Length);
        Console.WriteLine($"Seulement {(double)totalChunkSize / totalFileSize:P2} de nouvelles données écrites pour cette sauvegarde");
    }
}