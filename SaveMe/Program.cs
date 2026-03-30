using SaveMe.Services;

RepoService repoService;
SnapshotService snapshotService;
ChunkService chunkService;
AppSettingsService appSettingsService = new();

try
{
    repoService = new(appSettingsService);
    snapshotService = new(repoService);
    chunkService = new(repoService: repoService);
}
catch
{
    repoService = new();
    snapshotService = new(repoService);
    chunkService = new(repoService: repoService);
}

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
        HandleInit(args, appSettingsService, repoService);
        break;
    case "-b":
    case "--backup":
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

void HandleInit(string[] args, AppSettingsService settingsService, RepoService repo)
{
    int pathIdx = Array.IndexOf(args, "--path");
    if (pathIdx < 0)
        pathIdx = Array.IndexOf(args, "-p");

    if (pathIdx >= 0 && pathIdx + 1 < args.Length)
    {
        string saveMePath = args[pathIdx + 1];
        
        try
        {
            settingsService.SetSaveMePath(saveMePath);
            Console.WriteLine($"SaveMe path configured: {saveMePath}");
            
            if (Directory.Exists(saveMePath) && Directory.GetFiles(saveMePath).Length > 0)
            {
                Console.WriteLine("Directory is not empty. Initialize anyway? (y/n)");
                if (Console.ReadKey().KeyChar == 'y')
                {
                    repo.InitRepo();
                }
                else
                {
                    Console.WriteLine("\nInitialization cancelled");
                }
            }
            else
            {
                repo.InitRepo();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing repository: {ex.Message}");
            Environment.Exit(1);
        }
    }
    else
    {
        Console.WriteLine("Usage: SaveMe init --path <directory>");
        Console.WriteLine(CommandHelper.GetCommandDescription("--init"));
        Environment.Exit(1);
    }
}

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