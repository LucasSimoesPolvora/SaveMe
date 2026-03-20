using System.Text.Json;
using SaveMe.Models;
public class SnapshotService
{
    readonly RepoService repoService = new();
    readonly ChunkService chunkService = new();
    public void CreateSnapshot()
    {
        if(!RepoService.CheckRepo()) return;

        bool hadChanges = false;

        DirectoryInfo dir = new(Directory.GetCurrentDirectory() + "\\.sm\\snapshots");
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string snapshotId = $"snapshot_{timestamp}";
        string filePath = $"{dir.FullName}\\{snapshotId}.json";

        repoService.trackedFiles.Clear();
        repoService.GetFilesRecursively(Directory.GetCurrentDirectory());

        Snapshots snapshot = new()
        {
            Id = snapshotId,
            CommitFiles = Array.Empty<CommitFile>()
        };

        
        repoService.trackedFiles.ForEach((file) => {
            if(chunkService.HasChanges(file)){
                hadChanges = true;
                CommitFile commitFile = new(RepoService.GetRelativePath(file.FullName), ChunkService.GetChunksByFile(file));
                snapshot.CommitFiles = [.. snapshot.CommitFiles, commitFile];
            }
        });

        if(!hadChanges){
            Console.WriteLine("No changes detected. Snapshot not created.");
            return;
        }
        chunkService.CommitChunks();
        
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(snapshot, options);
        File.WriteAllText(filePath, json);
    }
}