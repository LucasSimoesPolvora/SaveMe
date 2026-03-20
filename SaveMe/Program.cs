RepoService repoService = new RepoService();
SnapshotService snapshotService = new SnapshotService();
ChunkService chunkService = new ChunkService();
foreach (var arg in args)
{
    switch (arg)
    {
        case "help":
        case "h":
            ShowHelp();
            break;
        case "init":
            repoService.InitRepo();
            break;
        case "commit":
            snapshotService.CreateSnapshot();
            break;
        case "check":
            chunkService.CheckChanges();
            break;
        default:
            ShowHelp();
            break;
    }
}

void ShowHelp()
{
    Console.WriteLine("Usage: SaveMe [options]");
    Console.WriteLine("Options:");
    Console.WriteLine("  help, h     Show this help message");
    Console.WriteLine("  init        Initialize the repository");
    Console.WriteLine("  commit      Commit changes to the repository");
    Console.WriteLine("  check       Check for changes in the repository");
}