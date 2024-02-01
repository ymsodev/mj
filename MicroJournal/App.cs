using System.Diagnostics;
using System.Text;

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

        if (content.Length == 0)
        {
            Console.WriteLine("No content provided, exiting.");
            return;
        }

        using (Database db = new(dataPath))
        {
            db.InsertEntry(DateTime.UtcNow, content);
        }
    }

    public static void LogEntries(string dataPath, int maxNumEntries)
    {
        using (Database db = new(dataPath))
        {
            ICollection<Entry> entries = db.GetRecentEntries(maxNumEntries);
            foreach (Entry entry in entries)
            {
                Console.WriteLine(entry.DateTime.ToLocalTime());
                Console.WriteLine(entry.Content);
                Console.WriteLine();
            }
        }
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
                sw.WriteLine(@"// Don't forget to save before closing!");
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
            for (string? line = sr.ReadLine(); line != null; line = sr.ReadLine())
            {
                if (!line.StartsWith("//"))
                    stringBuilder.AppendLine(line);
            }
            string content = stringBuilder.ToString();
            return content.TrimEnd();
        }
    }
}