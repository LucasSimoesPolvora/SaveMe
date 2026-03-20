public class ChunkService
{
    readonly RepoService repoService;

    public ChunkService(RepoService repoService)
    {
        this.repoService = repoService;
    }

    public void UpdateChunkStore(byte[] chunk)
    {
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

    public static List<byte[]> GetChunksByFile(FileInfo file)
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

    public void CommitChunks()
    {
        repoService.trackedFiles.ForEach((file) => {
            CdcService cdc = new();
            byte[] data = File.ReadAllBytes(file.FullName);
            List<byte[]> chunks = cdc.ChunkData(data);

            chunks.ForEach((chunk) => {
                UpdateChunkStore(chunk);
            });
        });
    }

    public void CheckChanges()
    {
        bool hasChanges = false;
        repoService.trackedFiles.ForEach((file) => {
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
                Console.WriteLine($"Changes detected in file: {RepoService.GetRelativePath(file.FullName)}, {numberOfChunks} new chunks");
            }
            hasChanges = false;
        });
    }
}