RepoService repoService = new();
SnapshotService snapshotService = new();
ChunkService chunkService = new(repoService: repoService);

if (args.Length == 0)
{
    Console.WriteLine(CommandHelper.GenerateFullDocumentation());
    return 0;
}

var command = args[0];

switch (command)
{
    case "-i":
    case "--init":
        repoService.InitRepo();
        break;
    case "-c":
    case "--commit":
        snapshotService.CreateSnapshot();
        break;
    case "-ck":
    case "--check":
        chunkService.CheckChanges();
        break;
    case "-l":
    case "--list":
        snapshotService.ListSnapshots();
        break;
    case "-r":
    case "--restore":
        HandleRestore(args);
        break;
    case "-h":
    case "--help":
        Console.WriteLine(CommandHelper.GenerateFullDocumentation());
        break;
    default:
        Console.WriteLine($"Unknown command: {command}");
        Console.WriteLine(CommandHelper.GenerateFullDocumentation());
        return 1;
}

return 0;

void HandleRestore(string[] args)
{
    var numberIdx = Array.IndexOf(args, "--index");
    if (numberIdx < 0)
        numberIdx = Array.IndexOf(args, "-i");

    if (numberIdx >= 0 && numberIdx + 1 < args.Length && int.TryParse(args[numberIdx + 1], out int num))
    {
        if (num <= 0)
        {
            Console.WriteLine("Error: Snapshot number must be greater than 0");
            Environment.Exit(1);
        }
        snapshotService.RestoreSnapshot(num);
    }
    else
    {
        Console.WriteLine($"Usage: SaveMe restore --snapshot-number <number>");
        Console.WriteLine(CommandHelper.GetCommandDescription("restore"));
        Environment.Exit(1);
    }
}