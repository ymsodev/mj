using System.CommandLine;

namespace MicroJournal;

internal class Program
{
	private static string EDITOR_ENV_VAR = "MJ_EDITOR";
	private static string DATA_ENV_VAR = "MJ_DATA";
	private static string DEFAULT_DB_FILENAME = ".mj.db";
	private static string WINDOWS_DEFAULT_EDITOR = "notepad.exe";
	
    private static async Task<int> Main(string[] args)
    {
		Option<string> editorOption = new(
			aliases: new string[]{"-e", "--editor"},
            description: "Path to the text editor to be used",
			getDefaultValue: () =>
			{
				string? editorPath = Environment.GetEnvironmentVariable(EDITOR_ENV_VAR);
				if (editorPath != null)
					return editorPath;
				return WINDOWS_DEFAULT_EDITOR;
			});
		
		Option<string> dataOption = new(
			aliases: new string[]{"-d", "--data"},
			description: "Path to which entry data is stored",
			getDefaultValue: () =>
			{
				string? dataPath = Environment.GetEnvironmentVariable(DATA_ENV_VAR);
				if (dataPath != null)
					return dataPath;
				string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				return Path.Combine(homePath, DEFAULT_DB_FILENAME);
			});
		
        RootCommand rootCommand = new("Command-line app for micro journaling");
		rootCommand.AddGlobalOption(editorOption);
		rootCommand.AddGlobalOption(dataOption);
		
		Command newEntryCommand = new("new", "Create a new entry");
		newEntryCommand.SetHandler(App.CreateNewEntry, editorOption, dataOption);
		rootCommand.AddCommand(newEntryCommand);
		
		return await rootCommand.InvokeAsync(args);
    }
}