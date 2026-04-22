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
        HandleBackup(args, snapshotService);
        break;
    case "-l":
    case "--list":
        snapshotService.ListSnapshots();
        break;
    case "-r":
    case "--restore":
        HandleRestore(args, appSettingsService);
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
        Console.WriteLine("Usage: SaveMe --init --path <directory>");
        Console.WriteLine(CommandHelper.GetCommandDescription("--init"));
        Environment.Exit(1);
    }
}

void HandleBackup(string[] args, SnapshotService snapshotService)
{
    bool isDryRun = args.Contains("--dry-run") || args.Contains("-d");
    
    if (isDryRun)
    {
        Console.WriteLine("[DRY RUN] Checking for changes...");
        chunkService.CheckChanges();
        Console.WriteLine("[DRY RUN] Complete. No snapshot created.");
    }
    else
    {
        snapshotService.CreateSnapshot();
    }
}

void HandleRestore(string[] args, AppSettingsService settingsService)
{
    int numberIdx = Array.IndexOf(args, "--index");
    int numberPath = Array.IndexOf(args, "--path");
    if (numberIdx < 0)
        numberIdx = Array.IndexOf(args, "-i");
    if (numberPath < 0)
        numberPath = Array.IndexOf(args, "-p");

    if (numberIdx >= 0 && 
    numberIdx + 1 < args.Length && 
    int.TryParse(args[numberIdx + 1], out int num) && 
    numberPath >= 0 && 
    numberPath + 1 < args.Length &&
    Directory.Exists(args[numberPath + 1]))
    {
        if (num <= 0)
        {
            Console.WriteLine("Error: Snapshot number must be greater than 0");
            Environment.Exit(1);
        }
        if(settingsService.GetSaveMePaths().Count == 0)
        {
            Console.WriteLine("Error: No SaveMe paths configured. Please run 'SaveMe --init <path>' to initialize.");
            Environment.Exit(1);
        }
         else if(!settingsService.GetSaveMePaths().Contains(args[numberPath + 1]))
        {
            Console.WriteLine("Error: The specified path is not configured for SaveMe. Please run 'SaveMe --init --path <path>' to initialize.");
            Environment.Exit(1);
        }

        snapshotService.RestoreSnapshot(num, args[numberPath + 1]);
    }
    else
    {
        Console.WriteLine($"Usage: SaveMe --restore --path <directory> --index <number>");
        Console.WriteLine(CommandHelper.GetCommandDescription("restore"));
        Environment.Exit(1);
    }
}