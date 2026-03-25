namespace SaveMe.Models
{
    public class CommitFile
    {
        public string Id { get; set; } // Will be identified by the file path
    
        public List<string> Chunks { get; set; }

        public CommitFile(string id, List<string> chunks)
        {
            Id = id;
            Chunks = chunks;
        }
    }
}
