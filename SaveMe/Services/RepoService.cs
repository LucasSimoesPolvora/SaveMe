using System.Text.Json;
using SaveMe.Models;

public class RepoService
{
    public readonly List<FileInfo> trackedFiles = new List<FileInfo>();
    readonly ChunkService chunkService;

    public RepoService()
    {
        chunkService = new ChunkService(this);
    }
    
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

    public static string GetRelativePath(string fullPath)
    {
        string currentDir = Directory.GetCurrentDirectory();
        if (fullPath.StartsWith(currentDir))
        {
            string relativePath = fullPath[currentDir.Length..].TrimStart(Path.DirectorySeparatorChar);
            return relativePath;
        }
        else
        {
            return "";
        }
    }
}