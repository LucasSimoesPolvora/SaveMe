namespace SaveMe.Models
{
    public class CommitFile
    {
        public string Id { get; set; } // Will be identified by the file path
    
        public List<byte[]> Chunks { get; set; }

        public CommitFile(string _id, List<byte[]> _chunks)
        {
            Id = _id;
            Chunks = _chunks;
        }
    }
}
