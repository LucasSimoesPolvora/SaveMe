using SaveMe.Services;

public class RepoService
{
    public readonly List<FileInfo> trackedFiles = new List<FileInfo>();
    private readonly AppSettingsService _appSettingsService;
    
    public RepoService(AppSettingsService? appSettingsService = null)
    {
        _appSettingsService = appSettingsService ?? new AppSettingsService();
    }
    
    private string GetRepositoryPath()
    {
        try
        {
            return _appSettingsService.GetSaveMePath();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
            Environment.Exit(1);
            return string.Empty; // Never reached, but required for compilation
        }
    }
    
    public string GetRepositoryBasePath()
    {
        return GetRepositoryPath();
    }
    
    public string GetSnapshotsPath()
    {
        return Path.Combine(GetRepositoryPath(), ".sm", "snapshots");
    }
    
    public string GetChunkStorePath()
    {
        return Path.Combine(GetRepositoryPath(), ".sm", "chunk_store");
    }
    
    public void InitRepo()
    {
        string repoPath = GetRepositoryPath();
        if (IsRepoInitialized(repoPath))
        {
            Console.WriteLine("Repository already exists...\n" +
                            "This will delete all existing snapshots and data in the repository.\n" +
                            "Would you like to overwrite it? (y/n)");
            if (Console.ReadKey().KeyChar == 'y')
            {
                Directory.Delete(Path.Combine(repoPath, ".sm"), true);
                CreateRepo(repoPath);
            }
            else
            {
                Console.WriteLine("\nInitialization cancelled");
                return;
            }
        }
        else
        {
            CreateRepo(repoPath);
            Console.WriteLine("\nRepository created successfully.");
        }
    }
    
    public static void CreateRepo(string? basePath = null)
    {
        try
        {
            basePath ??= Directory.GetCurrentDirectory();
            string smPath = Path.Combine(basePath, ".sm");
            
            Directory.CreateDirectory(smPath);
            DirectoryInfo dir = new(smPath)
            {
                Attributes = FileAttributes.Hidden
            };

            Directory.CreateDirectory(Path.Combine(dir.FullName, "chunk_store"));
            Directory.CreateDirectory(Path.Combine(dir.FullName, "snapshots"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating repository: {ex.Message}");
        }
    }
    
    public static bool IsRepoInitialized(string? basePath = null)
    {
        basePath ??= Directory.GetCurrentDirectory();
        string smPath = Path.Combine(basePath, ".sm");
        return Directory.Exists(smPath);
    }
    
    public static bool CheckRepo(string? basePath = null)
    {
        if (!IsRepoInitialized(basePath))
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
            if(trackedFiles.Find(f => f.FullName == file.FullName) == null)
            {
                trackedFiles.Add(file);
            }
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