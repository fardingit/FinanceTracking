using Finance_Tracker;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

public class SQLiteHelper
{
    private string connectionString = "Data Source=finance.db;Version=3;";

    public void CreateDatabase()
    {
        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT,
                    Description TEXT,
                    Amount REAL,
                    Category TEXT
                );";
            using (SQLiteCommand cmd = new SQLiteCommand(createTableQuery, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void InsertTransaction(string date, string description, decimal amount, string category)
    {
        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string insertQuery = "INSERT INTO Transactions (Date, Description, Amount, Category) VALUES (@Date, @Description, @Amount, @Category)";
            using (SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Date", date);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@Category", category);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public DataTable LoadTransactionsAsDataTable()
    {
        DataTable dt = new DataTable();
        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string selectQuery = "SELECT * FROM Transactions";
            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(selectQuery, conn))
            {
                adapter.Fill(dt);
            }
        }
        return dt;
    }

    public List<Transaction> LoadTransactions()
    {
        List<Transaction> transactions = new List<Transaction>();
        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string selectQuery = "SELECT * FROM Transactions";
            using (SQLiteCommand cmd = new SQLiteCommand(selectQuery, conn))
            using (SQLiteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    transactions.Add(new Transaction
                    {
                        Id = reader.GetInt32(0),
                        Date = reader.GetString(1),
                        Description = reader.GetString(2),
                        Amount = reader.GetDecimal(3),
                        Category = reader.GetString(4)
                    });
                }
            }
        }
        return transactions;
    }

    public void DeleteTransaction(int transactionId)
    {
        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string deleteQuery = "DELETE FROM Transactions WHERE Id = @Id";
            using (SQLiteCommand cmd = new SQLiteCommand(deleteQuery, conn))
            {
                cmd.Parameters.AddWithValue("@Id", transactionId);
                cmd.ExecuteNonQuery();
            }
        }
    }
    public string GetTransactionSummaryForAI()
    {
        using (SQLiteConnection conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string query = @"
            SELECT Category, SUM(Amount) AS Total
            FROM Transactions
            GROUP BY Category";
            using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
            {
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    List<string> summary = new List<string>();
                    while (reader.Read())
                    {
                        string category = reader.GetString(0);
                        decimal total = reader.GetDecimal(1);
                        summary.Add($"{category}: {total:C}");
                    }
                    return string.Join("\n", summary);
                }
            }
        }
    }

}
