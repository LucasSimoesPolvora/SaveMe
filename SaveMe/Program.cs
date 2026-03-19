RepoService repoService = new RepoService();
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
            repoService.CommitChanges();
            break;
        case "check":
            repoService.CheckChanges();
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
}