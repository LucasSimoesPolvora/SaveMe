/// <summary>
/// Helper class for command line interface documentation and descriptions.
/// Provides centralized command and option descriptions for the SaveMe application.
/// </summary>
public static class CommandHelper
{
    public static string GetDescription()
    {
        return "SaveMe - Repository snapshot management tool";
    }

    public static string GetCommandDescription(string commandName)
    {
        return commandName switch
        {
            "--init, -i" => "Initialize the repository for snapshot management",
            "--backup, -b" => "Create a new snapshot of current changes (use --dry-run to check without creating)",
            "--list, -l" => "List all available snapshots",
            "--restore, -r" => "Restore files from a previous snapshot",
            _ => "Unknown command"
        };
    }

    public static string GetOptionDescription(string optionName)
    {
        return optionName switch
        {
            "snapshot-number" => "The snapshot number to restore (must be greater than 0)",
            "--path, -p" => "The directory path where the .sm repository will be stored",
            "--dry-run, -d" => "Check for changes without creating a snapshot",
            _ => "Unknown option"
        };
    }

    public static string GenerateFullDocumentation()
    {
        return $@"{GetDescription()}

Usage: SaveMe <command> [options]

Commands:
  --init, -i              {GetCommandDescription("--init, -i")}
  --backup, -b            {GetCommandDescription("--backup, -b")}
  --list, -l              {GetCommandDescription("--list, -l")}
  --restore, -r           {GetCommandDescription("--restore, -r")}

Options for init:
  --path, -p <directory>  {GetOptionDescription("--path, -p")}

Options for backup:
  --dry-run, -d           {GetOptionDescription("--dry-run, -d")}

Options for restore:
  --index, -i <number>    {GetOptionDescription("snapshot-number")}

Examples:
  SaveMe --init --path C:\SaveMe              Initialize the repository with custom path
  SaveMe --backup                             Create a new snapshot
  SaveMe --backup --dry-run                   Check for changes without creating snapshot
  SaveMe --list                               Display all snapshots
  SaveMe --restore --index 1                  Restore snapshot number 1

For more information, visit: https://github.com/LucasSimoesPolvora/SaveMe
";
    }
}
