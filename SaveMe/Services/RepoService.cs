using System.Text.Json;
using SaveMe.Models;

public class RepoService
{
    readonly List<FileInfo> trackedFiles = new List<FileInfo>();
    bool wasUpdated = false;
    
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

        GetFilesRecursively(Directory.GetCurrentDirectory());

        trackedFiles.ForEach((file) => {
            CdcService cdc = new();
            byte[] data = File.ReadAllBytes(file.FullName);
            List<byte[]> chunks = cdc.ChunkData(data);

            chunks.ForEach((chunk) => {
                UpdateChunkStore(chunk);
            });
        });

        if(!wasUpdated){
            Console.WriteLine($"No changes detected");
        }
    }

    public void GetFilesRecursively(string path){
        trackedFiles.Clear();
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
                wasUpdated = true;
                fs.Write(chunk, 0, chunk.Length);
            }
        }
    }

    public void CheckChanges()
    {
        if (!CheckRepo()) return;

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
            Console.WriteLine($"Chunk hash: {hash}");
            string safeHash = hash.Replace("/", "_");
            DirectoryInfo dir = new(Directory.GetCurrentDirectory() + "\\.sm\\chunk_store");
            if(!File.Exists($"{dir.FullName}\\{safeHash}.txt")) {
                Console.WriteLine($"New chunk detected: {safeHash}");
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

        GetFilesRecursively(Directory.GetCurrentDirectory());

        Snapshots snapshot = new()
        {
            Id = snapshotId,
            CommitFiles = Array.Empty<CommitFile>()
        };
        
        // Check for changes BEFORE committing
        trackedFiles.ForEach((file) => {
            Console.WriteLine($"Checking file: {GetRelativePath(file.FullName)}");
            Console.WriteLine($"Has changes: {HasChanges(file)}");
            if(HasChanges(file)){
                hadChanges = true;
                CommitFile commitFile = new(GetRelativePath(file.FullName), GetChunksByFile(file));        
                Console.WriteLine($"Changes detected in file: {GetRelativePath(file.FullName)}");

                snapshot.CommitFiles = [.. snapshot.CommitFiles, commitFile];
            }
        });

        if(!hadChanges){
            Console.WriteLine("No changes detected. Snapshot not created.");
            return;
        }

        // Commit changes AFTER detecting them
        CommitChanges();
        
        string json = JsonSerializer.Serialize(snapshot);
        File.WriteAllText(filePath, json);
    }
}