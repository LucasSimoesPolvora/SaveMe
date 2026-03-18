foreach (var arg in args)
{
    switch (arg)
    {
        case "help":
        case "h":
            ShowHelp();
            break;
        case "init":
            RepoService.InitRepo();
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