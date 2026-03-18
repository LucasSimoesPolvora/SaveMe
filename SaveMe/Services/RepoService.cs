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
            DirectoryInfo dir = new(Directory.GetCurrentDirectory() + "\\.sm")
            {
                Attributes = FileAttributes.Hidden
            };

            Directory.CreateDirectory(dir.FullName + "\\chunk_store");
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
    public void CommitChanges()
    {
        if (!IsRepoInitialized())
        {
            Console.WriteLine("Repository not initialized. Please run 'init' command first.");
            return;
        }

        GetFilesRecursively(Directory.GetCurrentDirectory());

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
    public static void UpdateChunkStore(byte[] chunk){
        string hash = CdcService.CalculateChunkFingerprint(chunk);

        DirectoryInfo dir = new(Directory.GetCurrentDirectory() + "\\.sm\\chunk_store");
        
        using (FileStream fs = File.Create($"{dir.FullName}\\{hash}.txt")){
            fs.Write(chunk, 0, chunk.Length);
        }

        
    }
}