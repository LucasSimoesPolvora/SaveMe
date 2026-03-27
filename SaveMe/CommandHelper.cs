/// <summary>
/// Helper class for command line interface documentation and descriptions.
/// Provides centralized command and option descriptions for the SaveMe application.
/// </summary>
public static class CommandHelper
{
    /// <summary>
    /// Gets the main application description.
    /// </summary>
    /// <returns>Description of the SaveMe application</returns>
    public static string GetDescription()
    {
        return "SaveMe - Repository snapshot management tool";
    }

    /// <summary>
    /// Gets the description for a specific command.
    /// </summary>
    /// <param name="commandName">The name of the command</param>
    /// <returns>Description of the command</returns>
    public static string GetCommandDescription(string commandName)
    {
        return commandName switch
        {
            "--init, -i" => "Initialize the repository for snapshot management",
            "--commit, -c" => "Commit and create a snapshot of current changes",
            "--check, -ch" => "Check for changes in the repository",
            "--list, -l" => "List all available snapshots",
            "--restore, -r" => "Restore files from a previous snapshot",
            _ => "Unknown command"
        };
    }

    /// <summary>
    /// Gets the description for a specific option.
    /// </summary>
    /// <param name="optionName">The name of the option</param>
    /// <returns>Description of the option</returns>
    public static string GetOptionDescription(string optionName)
    {
        return optionName switch
        {
            "snapshot-number" => "The snapshot number to restore (must be greater than 0)",
            _ => "Unknown option"
        };
    }

    /// <summary>
    /// Generates full documentation for the application.
    /// </summary>
    /// <returns>Complete help text</returns>
    public static string GenerateFullDocumentation()
    {
        return $@"{GetDescription()}

Usage: SaveMe <command> [options]

Commands:
  --init, -i              {GetCommandDescription("--init")}
  --commit, -c            {GetCommandDescription("--commit")}
  --check, -ch             {GetCommandDescription("--check")}
  --list, -l         {GetCommandDescription("--list")}
  --restore, -r           {GetCommandDescription("--restore")}
Options for restore:
  --index, -i <number>  {GetOptionDescription("--index")}

Examples:
  SaveMe --init                     Initialize the repository
  SaveMe --commit                   Create a new snapshot
  SaveMe --check                    Check for file changes
  SaveMe --list                     Display all snapshots
  SaveMe --restore --index 1        Restore snapshot number 1
For more information, visit: https://github.com/LucasSimoesPolvora/SaveMe
";
    }
}
