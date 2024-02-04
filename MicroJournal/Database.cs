using Microsoft.Data.Sqlite;

namespace MicroJournal;

internal record Entry(long Id, DateTime DateTime, string Content);

internal class Database : IDisposable
{
    private SqliteConnection connection;

    public Database(string dataPath)
    {
        connection = new SqliteConnection($"Data Source={dataPath}");
        connection.Open();
    }

    public void InsertEntry(DateTime dateTime, string content)
    {
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
        command.Parameters.AddWithValue("$createdAt", dateTime);
        command.Parameters.AddWithValue("$content", content);
        command.ExecuteNonQuery();
    }

    public ICollection<Entry> GetRecentEntries(int limit)
    {
        SqliteCommand command = connection.CreateCommand();

        command.CommandText =
            @"SELECT * FROM entries
                ORDER BY createdAt DESC
                LIMIT $limit";
        command.Parameters.AddWithValue("$limit", limit);

        List<Entry> entries = new List<Entry>();
        using (SqliteDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                long id = reader.GetInt64(0);
                DateTime dateTime = reader.GetDateTime(1);
                string content = reader.GetString(2);
                entries.Add(new(id, dateTime, content));
            }
        }
        return entries;
    }

    public void Dispose()
    {
        connection.Close();
        connection.Dispose();
    }
}
