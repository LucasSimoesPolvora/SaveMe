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
        case "i":
            repoService.InitRepo();
            break;
        case "commit":
        case "c":
            snapshotService.CreateSnapshot();
            break;
        case "check":
        case "ch":
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
    Console.WriteLine("  help, h        Show this help message");
    Console.WriteLine("  init, i        Initialize the repository");
    Console.WriteLine("  commit, c      Commit changes to the repository");
    Console.WriteLine("  check, ch      Check for changes in the repository");
}