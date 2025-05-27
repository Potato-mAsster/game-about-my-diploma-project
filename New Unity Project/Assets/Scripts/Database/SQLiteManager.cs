using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;

public class SQLiteManager : MonoBehaviour
{
    private IDbConnection dbConnection;
    private string databaseName = "HypersomniaDB.db";

    void Awake()
    {
        OpenDb();
        CreateTable();
    }

    void OnDestroy()
    {
        CloseDb();
    }

    private void OpenDb()
    {
        string connectionString = "URI=file:" + Application.persistentDataPath + "/" + databaseName;
        dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();
        Debug.Log("Database opened: " + connectionString);
    }

    private void CloseDb()
    {
        if (dbConnection != null && dbConnection.State == ConnectionState.Open)
        {
            dbConnection.Close();
            dbConnection = null;
            Debug.Log("Database closed.");
        }
    }

    private void CreateTable()
    {
        IDbCommand dbcmd = dbConnection.CreateCommand();
        string sql = "CREATE TABLE IF NOT EXISTS PlayerData (id INTEGER PRIMARY KEY, playerName TEXT, score INTEGER, level INTEGER)";
        dbcmd.CommandText = sql;
        dbcmd.ExecuteReader();
        Debug.Log("Table 'PlayerData' created or already exists.");
        dbcmd.Dispose();
    }

    public void SavePlayerData(string playerName, int score, int level)
    {
        IDbCommand dbcmd = dbConnection.CreateCommand();
        string sql = "INSERT OR REPLACE INTO PlayerData (id, playerName, score, level) VALUES (1, @playerName, @score, @level)";
        dbcmd.CommandText = sql;

        IDbDataParameter paramName = dbcmd.CreateParameter();
        paramName.ParameterName = "@playerName";
        paramName.Value = playerName;
        dbcmd.Parameters.Add(paramName);

        IDbDataParameter paramScore = dbcmd.CreateParameter();
        paramScore.ParameterName = "@score";
        paramScore.Value = score;
        dbcmd.Parameters.Add(paramScore);

        IDbDataParameter paramLevel = dbcmd.CreateParameter();
        paramLevel.ParameterName = "@level";
        paramLevel.Value = level;
        dbcmd.Parameters.Add(paramLevel);

        dbcmd.ExecuteNonQuery();
        Debug.Log("Player data saved.");
        dbcmd.Dispose();
    }

    public PlayerData LoadPlayerData()
    {
        IDbCommand dbcmd = dbConnection.CreateCommand();
        string sql = "SELECT playerName, score, level FROM PlayerData WHERE id = 1";
        dbcmd.CommandText = sql;
        IDataReader reader = dbcmd.ExecuteReader();

        PlayerData playerData = null;
        if (reader.Read())
        {
            playerData = new PlayerData
            {
                playerName = reader.GetString(0),
                score = reader.GetInt32(1),
                level = reader.GetInt32(2)
            };
            Debug.Log("Player data loaded: " + playerData.playerName + ", " + playerData.score + ", " + playerData.level);
        }
        else
        {
            Debug.LogWarning("No player data found.");
        }

        reader.Close();
        reader.Dispose();
        dbcmd.Dispose();
        return playerData;
    }

    public class PlayerData
    {
        public string playerName;
        public int score;
        public int level;
    }
}