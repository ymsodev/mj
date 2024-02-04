using System.CommandLine;

namespace MicroJournal;

internal static class Program
{
    private const string EditorEnvVar = "MJ_EDITOR";
    private const string DataEnvVar = "MJ_DATA";
    private const string DefaultDbFilename = ".mj.db";
    private const string WindowsDefaultEditor = "notepad.exe";

    private static async Task<int> Main(string[] args)
    {
        Option<string> dataOption = new(
            aliases: ["-d", "--data"],
            description: "Path to which entry data is stored",
            getDefaultValue: () =>
            {
                string? dataPath = Environment.GetEnvironmentVariable(DataEnvVar);
                if (dataPath != null)
                    return dataPath;
                string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Path.Combine(homePath, DefaultDbFilename);
            });

        RootCommand rootCommand = new("Command-line app for micro journaling");
        rootCommand.AddGlobalOption(dataOption);

        Option<string> editorOption = new(
            aliases: ["-e", "--editor"],
            description: "Path to the text editor to be used",
            getDefaultValue: () =>
            {
                string? editorPath = Environment.GetEnvironmentVariable(EditorEnvVar);
                if (editorPath != null)
                    return editorPath;
                return WindowsDefaultEditor;
            });

        Command newEntryCommand = new("new", "Create a new entry");
        newEntryCommand.AddOption(editorOption);
        newEntryCommand.SetHandler(App.CreateNewEntry, editorOption, dataOption);
        rootCommand.AddCommand(newEntryCommand);

        Option<int> numEntriesOption = new(
            aliases: ["-n", "--num-entries"],
            description: "Maximum number of entries to show",
            getDefaultValue: () => 10);

        Command logEntriesCommand = new("log", "Show entry logs");
        logEntriesCommand.AddOption(numEntriesOption);
        logEntriesCommand.SetHandler(App.LogEntries, dataOption, numEntriesOption);
        rootCommand.AddCommand(logEntriesCommand);

        return await rootCommand.InvokeAsync(args);
    }
}