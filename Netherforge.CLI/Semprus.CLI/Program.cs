using Spectre.Console;
using Netherforge.CLI;

if (args.Length == 0)
{
    AnsiConsole.MarkupLine("[red]Error:[/] No command specified.");
    AnsiConsole.MarkupLine("Usage: netherforge scaffold <appname>");
    return 1;
}

var command = args[0].ToLower();

if (command == "scaffold")
{
    if (args.Length < 2)
    {
        AnsiConsole.MarkupLine("[red]Error:[/] Application name not specified.");
        AnsiConsole.MarkupLine("Usage: netherforge scaffold <appname>");
        return 1;
    }

    var appName = args[1];
    var scaffolder = new AppScaffolder();
    
    try
    {
        await scaffolder.ScaffoldAsync(appName);
        return 0;
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
        return 1;
    }
}
else
{
    AnsiConsole.MarkupLine($"[red]Error:[/] Unknown command '{command}'.");
    AnsiConsole.MarkupLine("Usage: netherforge scaffold <appname>");
    return 1;
}
