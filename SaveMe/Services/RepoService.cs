using System.Text.Json;
using SaveMe.Models;

public class RepoService
{
    readonly List<FileInfo> trackedFiles = new List<FileInfo>();
    
    public void InitRepo()
    {
        if (IsRepoInitialized())
        {
            Console.WriteLine("Repository already exists...\n" +
                            "This will delete all existing snapshots and data in the repository.\n" +
                            "Would you like to overwrite it? (y/n)");
            if (Console.ReadKey().KeyChar == 'y')
            {
                Directory.Delete(Directory.GetCurrentDirectory() + "\\.sm", true);
                CreateRepo();
            }
            else
            {
                Console.WriteLine("\nInitialization cancelled");
                return;
            }
        }
        else
        {
            CreateRepo();
            Console.WriteLine("\nRepository created successfully.");
        }
    }
    
    public static void CreateRepo()
    {
        try
        {
            Directory.CreateDirectory(".sm");
            DirectoryInfo dir = new(Directory.GetCurrentDirectory() + "\\.sm")
            {
                Attributes = FileAttributes.Hidden
            };

            Directory.CreateDirectory(dir.FullName + "\\chunk_store");
            Directory.CreateDirectory(dir.FullName + "\\snapshots");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating repository: {ex.Message}");
        }
    }
    
    public static bool IsRepoInitialized()
    {
        DirectoryInfo info = new(Directory.GetCurrentDirectory());
        foreach (DirectoryInfo dir in info.GetDirectories())
        {
            if (dir.Name == ".sm")
            {
                return true;
            }
        }
        return false;
    }
    
    public static bool CheckRepo()
    {
        if (!IsRepoInitialized())
        {
            Console.WriteLine("Repository not initialized. Please run 'init' command first.");
            return false;
        }
        return true;
    }
    
    public void CommitChanges()
    {
        if (!CheckRepo()) return;

        trackedFiles.ForEach((file) => {
            CdcService cdc = new();
            byte[] data = File.ReadAllBytes(file.FullName);
            List<byte[]> chunks = cdc.ChunkData(data);

            chunks.ForEach((chunk) => {
                UpdateChunkStore(chunk);
            });
        });
    }

    public void GetFilesRecursively(string path){
        DirectoryInfo info = new(path);

        foreach (FileInfo file in info.GetFiles())
        {
            trackedFiles.Add(file);
        }

        foreach (DirectoryInfo dir in info.GetDirectories())
        {
            if(dir.FullName.Contains(".sm"))
            {
                continue;
            }
            GetFilesRecursively(dir.FullName);
        }
    }
    
    public void UpdateChunkStore(byte[] chunk){
        string hash = CdcService.CalculateChunkFingerprint(chunk);
        string safeHash = hash.Replace("/", "_");

        DirectoryInfo dir = new(Directory.GetCurrentDirectory() + "\\.sm\\chunk_store");
        string filePath = $"{dir.FullName}\\{safeHash}.txt";
        
        if(!File.Exists(filePath)){            
            using (FileStream fs = File.Create(filePath)){
                fs.Write(chunk, 0, chunk.Length);
            }
        }
    }

    public void CheckChanges()
    {
        if (!CheckRepo()) return;

        trackedFiles.Clear();
        GetFilesRecursively(Directory.GetCurrentDirectory());
        bool hasChanges = false;
        trackedFiles.ForEach((file) => {
            
            int numberOfChunks = 0;
            GetChunksByFile(file).ForEach((chunk) => {
                string hash = CdcService.CalculateChunkFingerprint(chunk);
                string safeHash = hash.Replace("/", "_");
                DirectoryInfo dir = new(Directory.GetCurrentDirectory() + "\\.sm\\chunk_store");
                if(!File.Exists($"{dir.FullName}\\{safeHash}.txt")) {
                    hasChanges = true;
                    numberOfChunks++;
                }
            });
            if (hasChanges)
            {
                Console.WriteLine($"Changes detected in file: {GetRelativePath(file.FullName)}, {numberOfChunks} new chunks");
            }
        });
    }

    public List<byte[]> GetChunksByFile(FileInfo file)
    {
        CdcService cdc = new();
        byte[] data = File.ReadAllBytes(file.FullName);
        return cdc.ChunkData(data);
    }

    public bool HasChanges(FileInfo file)
    {
        bool hasChanges = false;
        GetChunksByFile(file).ForEach((chunk) => {
            string hash = CdcService.CalculateChunkFingerprint(chunk);
            string safeHash = hash.Replace("/", "_");
            DirectoryInfo dir = new(Directory.GetCurrentDirectory() + "\\.sm\\chunk_store");
            if(!File.Exists($"{dir.FullName}\\{safeHash}.txt")) {
                hasChanges = true;
            }
        });
        return hasChanges;
    }

    public static string GetRelativePath(string fullPath)
    {
        string currentDir = Directory.GetCurrentDirectory();
        if (fullPath.StartsWith(currentDir))
        {
            string relativePath = fullPath.Substring(currentDir.Length).TrimStart(Path.DirectorySeparatorChar);
            return relativePath;
        }
        else
        {
            return "";
        }
    }

    public void CreateSnapshot()
    {
        if(!CheckRepo()) return;

        bool hadChanges = false;

        DirectoryInfo dir = new(Directory.GetCurrentDirectory() + "\\.sm\\snapshots");
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string snapshotId = $"snapshot_{timestamp}";
        string filePath = $"{dir.FullName}\\{snapshotId}.json";

        trackedFiles.Clear();
        GetFilesRecursively(Directory.GetCurrentDirectory());

        Snapshots snapshot = new()
        {
            Id = snapshotId,
            CommitFiles = Array.Empty<CommitFile>()
        };

        
        trackedFiles.ForEach((file) => {
            if(HasChanges(file)){
                hadChanges = true;
                CommitFile commitFile = new(GetRelativePath(file.FullName), GetChunksByFile(file));
                snapshot.CommitFiles = [.. snapshot.CommitFiles, commitFile];
            }
        });

        if(!hadChanges){
            Console.WriteLine("No changes detected. Snapshot not created.");
            return;
        }
        CommitChanges();
        
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(snapshot, options);
        File.WriteAllText(filePath, json);
    }
}