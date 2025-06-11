using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TestApp.Service
{
    public class dbServices
    {
        private readonly string? _connectionString;

        public dbServices(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? configuration["db:connStrPrimary"];
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Database connection string is not configured.");
            }
        }

        public List<Dictionary<string, object>[]> ExecuteSQLName(string sql, MySqlParameter[]? parameters = null)
        {
            var allTables = new List<Dictionary<string, object>[]>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    using (var transaction = conn.BeginTransaction())
                    {
                        cmd.Transaction = transaction;
                        try
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                do
                                {
                                    var tableRows = new List<Dictionary<string, object>>();
                                    while (reader.Read())
                                    {
                                        var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            row[reader.GetName(i)] = reader.GetValue(i);
                                        }
                                        tableRows.Add(row);
                                    }
                                    allTables.Add(tableRows.ToArray());
                                } while (reader.NextResult());
                            }
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Console.WriteLine($"Database transaction failed: {ex.Message}");
                            return new List<Dictionary<string, object>[]>();
                        }
                    }
                }
            }
            return allTables;
        }
    }
}
