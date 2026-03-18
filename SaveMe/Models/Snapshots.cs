namespace SaveMe.Models
{
    public class Snapshots
    {
        public required string Id { get; init; }
        public DateTime Timestamp { get; }

        public required string[] FileList { get; set; }

        public Snapshots()
        {
            Timestamp = DateTime.Now;
        }
    }
}
