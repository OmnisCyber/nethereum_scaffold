using Semprus.CLI;

if (args.Length == 0)
{
    Console.WriteLine("Semprus - Scaffold censorship resistant CRUD applications");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  semprus scaffold <appname>");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  semprus scaffold MyWebApp");
    return 0;
}

if (args[0].ToLower() == "scaffold")
{
    if (args.Length < 2)
    {
        Console.WriteLine("Error: Please provide an application name.");
        Console.WriteLine("Usage: semprus scaffold <appname>");
        return 1;
    }

    string appName = args[1];
    
    // Validate app name
    if (string.IsNullOrWhiteSpace(appName) || !IsValidProjectName(appName))
    {
        Console.WriteLine($"Error: '{appName}' is not a valid application name.");
        Console.WriteLine("Application name must be a valid C# identifier.");
        return 1;
    }

    Console.WriteLine($"Scaffolding application: {appName}");
    Console.WriteLine();

    try
    {
        var scaffolder = new AppScaffolder();
        scaffolder.Scaffold(appName, Directory.GetCurrentDirectory());
        
        Console.WriteLine();
        Console.WriteLine($"✓ Application '{appName}' created successfully!");
        Console.WriteLine();
        Console.WriteLine("Next steps:");
        Console.WriteLine($"  cd {appName}");
        Console.WriteLine("  dotnet run");
        Console.WriteLine();
        
        return 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return 1;
    }
}
else
{
    Console.WriteLine($"Unknown command: {args[0]}");
    Console.WriteLine("Usage: semprus scaffold <appname>");
    return 1;
}

static bool IsValidProjectName(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        return false;
    
    // Check if it's a valid C# identifier
    if (!char.IsLetter(name[0]) && name[0] != '_')
        return false;
    
    foreach (char c in name)
    {
        if (!char.IsLetterOrDigit(c) && c != '_' && c != '.')
            return false;
    }
    
    return true;
}
