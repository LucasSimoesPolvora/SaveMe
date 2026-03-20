namespace SaveMe.Models
{
    public class CommitFile
    {
        public string Id { get; set; } // Will be identified by the file path
    
        public List<byte[]> Chunks { get; set; }

        public CommitFile(string id, List<byte[]> chunks)
        {
            Id = id;
            Chunks = chunks;
        }
    }
}
