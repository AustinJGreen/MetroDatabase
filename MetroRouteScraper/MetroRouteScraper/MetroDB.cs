using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;

namespace MetroRouteScraper
{
    public class MetroDB : IDisposable
    {
        private bool disposed = false;
        private MySqlConnection conn;
        private readonly string host;
        private readonly int port;
        private readonly string db;
        private readonly string user;
        private readonly string pass;

        public MetroDB(string user, string pass)
        {
            host = "metrodb.c6yfhb0actbf.us-west-2.rds.amazonaws.com";
            port = 1997;
            db = "metrodb";
            this.user = user;
            this.pass = pass;
        }
        
        private string GetConnectionString()
        {
            return $"Server={host};Port={port};Database={db};Uid={user};Pwd={pass};";
        }

        public bool Connect()
        {
            if (disposed)
            {
                return false;
            }
            else if (conn == null)
            {
                conn = new MySqlConnection(GetConnectionString());
            }

            try
            {
                conn.Open();
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (MySqlException)
            {
                return false;
            }

            return conn.State == System.Data.ConnectionState.Open;
        }

        public List<string>[] GetTable(string table)
        {
            if (disposed || conn == null || conn.State != System.Data.ConnectionState.Open)
            {
                return null;
            }

            using (MySqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM {table};";
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    List<string>[] columnData = new List<string>[rdr.FieldCount];
                    while (rdr.Read())
                    {
                        for (int o = 0; o < rdr.FieldCount; o++)
                        {
                            if (columnData[o] == null)
                            {
                                columnData[o] = new List<string>();
                            }
                            using (TextReader txt = rdr.GetTextReader(o))
                            {
                                string line = null;
                                while ((line = txt.ReadLine()) != null)
                                {
                                    columnData[o].Add(line);
                                }
                            }
                        }
                    }
                    return columnData;
                }
            }
        }

        public void Dispose()
        {
            if (!disposed && conn != null)
            {
                conn.Dispose();
                conn = null;           
            }
            disposed = true;
        }
    }
}
