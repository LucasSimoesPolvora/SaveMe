using System.Text.Json;
using SaveMe.Models;
public class SnapshotService
{
    readonly RepoService repoService = new();
    readonly ChunkService chunkService;

    public SnapshotService()
    {
        chunkService = new(repoService);
    }
    public void CreateSnapshot()
    {
        if(!RepoService.CheckRepo()) return;

        bool hadChanges = false;

        DirectoryInfo dir = new(Directory.GetCurrentDirectory() + "\\.sm\\snapshots");
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
            
            // Check if file has changes (modified)
            if(chunkService.HasChanges(file)){
                hadChanges = true;
                CommitFile commitFile = new(relativePath, ChunkService.GetChunksByFile(file));
                snapshot.CommitFiles = [.. snapshot.CommitFiles, commitFile];
            }
            // Check if file was previously deleted but now exists (resurrected)
            else if (WasFileDeleted(relativePath))
            {
                hadChanges = true;
                CommitFile commitFile = new(relativePath, ChunkService.GetChunksByFile(file));
                snapshot.CommitFiles = [.. snapshot.CommitFiles, commitFile];
                Console.WriteLine($"File resurrected: {relativePath}");
            }
        });

        // Check for deleted files
        string[] deletedFiles = GetDeletedFiles();
        if (deletedFiles.Length > 0)
        {
            Console.WriteLine("There was a deleted file");
            hadChanges = true;
            snapshot.DeletedFiles = deletedFiles;
        }

        if(!hadChanges){
            Console.WriteLine("No changes detected. Snapshot not created.");
            return;
        }
        chunkService.CommitChunks();
        
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(snapshot, options);
        File.WriteAllText(filePath, json);
    }

    public void ListSnapshots()
    {
        DirectoryInfo dir = new(Directory.GetCurrentDirectory() + "\\.sm\\snapshots");
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
        DirectoryInfo snapshotDir = new(Directory.GetCurrentDirectory() + "\\.sm\\snapshots");
        FileInfo[] snapshotFiles = snapshotDir.GetFiles("*.json");
        
        // If this is the first snapshot, no files can be deleted
        if (snapshotFiles.Length == 0)
        {
            return Array.Empty<string>();
        }

        // Get the most recent snapshot
        FileInfo lastSnapshotFile = snapshotFiles.OrderByDescending(f => f.Name).First();
        string lastSnapshotJson = File.ReadAllText(lastSnapshotFile.FullName);
        Snapshots? lastSnapshot = JsonSerializer.Deserialize<Snapshots>(lastSnapshotJson);

        if (lastSnapshot == null)
        {
            return Array.Empty<string>();
        }

        // Get current file paths
        HashSet<string> currentFiles = new(repoService.trackedFiles.Select(f => RepoService.GetRelativePath(f.FullName)));

        // Find deleted files
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
        DirectoryInfo snapshotDir = new(Directory.GetCurrentDirectory() + "\\.sm\\snapshots");
        FileInfo[] snapshotFiles = snapshotDir.GetFiles("*.json");
        
        if (snapshotFiles.Length == 0)
        {
            return false;
        }

        // Get the most recent snapshot
        FileInfo lastSnapshotFile = snapshotFiles.OrderByDescending(f => f.Name).First();
        string lastSnapshotJson = File.ReadAllText(lastSnapshotFile.FullName);
        Snapshots? lastSnapshot = JsonSerializer.Deserialize<Snapshots>(lastSnapshotJson);

        if (lastSnapshot?.DeletedFiles == null)
        {
            return false;
        }

        // Check if this file is in the deleted files list
        return lastSnapshot.DeletedFiles.Contains(filePath);
    }
}