public class RepoService
{
    List<FileInfo> trackedFiles = new List<FileInfo>();
    public static void InitRepo()
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
            _ = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\.sm")
            {
                Attributes = FileAttributes.Hidden
            };

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating repository: {ex.Message}");
        }
    }
    public static bool IsRepoInitialized()
    {
        DirectoryInfo info = new(Directory.GetCurrentDirectory());
        foreach (var file in info.GetDirectories())
        {
            if (file.Name == ".sm")
            {
                return true;
            }
        }
        return false;
    }
    public void CommitChanges()
    {
        if (!IsRepoInitialized())
        {
            Console.WriteLine("Repository not initialized. Please run 'init' command first.");
            return;
        }

        GetFilesRecursively(Directory.GetCurrentDirectory());

        foreach(FileInfo file in trackedFiles)
        {
            Console.WriteLine(file.FullName);
        }
    }

    public static void GetFilesRecursively(string path){
        DirectoryInfo info = new(path);

        foreach (FileInfo file in info.GetFiles())
        {
            Console.WriteLine(file.FullName);
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
}