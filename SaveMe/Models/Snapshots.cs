namespace SaveMe.Models
{
    public class Snapshots
    {
        public required string Id { get; init; }
        public DateTime Timestamp { get; }

        public required CommitFile[] CommitFiles { get; set; }
        public string[] DeletedFiles { get; set; } = Array.Empty<string>();
        public Snapshots()
        {
            Timestamp = DateTime.Now;
        }
    }
}
