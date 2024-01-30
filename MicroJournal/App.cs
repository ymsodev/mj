using System.Diagnostics;
using System.Text;
using Microsoft.Data.Sqlite;

namespace MicroJournal;

internal static class App
{
    private static string TEMP_ENTRY_BUFFER_FILE = "MJ_NEW_ENTRY";

    public static void CreateNewEntry(string editor, string dataPath)
    {
        string? entryBufferPath = CreateEntryBuffer();
        if (entryBufferPath == null)
            return;

        string? content = GetEntryContent(editor, entryBufferPath);
        if (content == null)
            return;

        InsertEntry(dataPath, content);
    }

    private static string? CreateEntryBuffer()
    {
        string filename = $"{TEMP_ENTRY_BUFFER_FILE}_{Guid.NewGuid()}";
        string entryBufferPath = Path.Combine(Path.GetTempPath(), filename);
        try
        {
            using (FileStream fs = File.Open(entryBufferPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (StreamWriter sw = new(fs))
            {
                sw.WriteLine();
                sw.WriteLine(@"// This is a buffer for a new micro-journal entry.");
                sw.WriteLine(@"// The lines that start with '//' will be ignored.");
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to create an entry buffer: {e.Message}");
            return null;
        }
        return entryBufferPath;
    }

    private static string? GetEntryContent(string editor, string entryBufferPath)
    {
        try
        {
            Process proc = Process.Start(editor, entryBufferPath);
            proc.WaitForExit();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to start {editor}: {e.Message}");
            return null;
        }

        using (FileStream fs = File.Open(entryBufferPath, FileMode.Open, FileAccess.Read, FileShare.None))
        using (StreamReader sr = new(fs))
        {
            StringBuilder stringBuilder = new();
            for (string? line = sr.ReadLine(); line != null && !line.StartsWith("//"); line = sr.ReadLine())
            {
                stringBuilder.AppendLine(line);
            }
            return stringBuilder.ToString();
        }
    }

    private static void InsertEntry(string dataPath, string content)
    {
        using (SqliteConnection connection = new($"Data Source={dataPath}"))
        {
            connection.Open();

            SqliteCommand command = connection.CreateCommand();

            command.CommandText =
                @"CREATE TABLE IF NOT EXISTS entries (
					id INTEGER PRIMARY KEY,
					createdAt REAL NOT NULL,
					content TEXT NOT NULL)";
            command.ExecuteNonQuery();

            command.CommandText =
                @"INSERT INTO entries (createdAt, content)
					VALUES ($createdAt, $content)";
            command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("$content", content);
            command.ExecuteNonQuery();

            connection.Close();
        }
    }
}