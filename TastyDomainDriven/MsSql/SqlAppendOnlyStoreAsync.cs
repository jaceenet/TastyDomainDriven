 

using System;

namespace TastyDomainDriven.MsSql
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    using AsyncImpl;

    public sealed class SqlAppendOnlyStoreAsync : IAppendOnlyAsync, IDisposable
    {
        readonly SqlConnection connection;

        private readonly string tableName;

        public SqlAppendOnlyStoreAsync(string connectionString, string tableName = "events")
        {
            this.connection = new SqlConnection(connectionString);
        }

        public SqlAppendOnlyStoreAsync(SqlConnection connection, string tableName = "events")
        {
            this.connection = connection;
            this.tableName = "[" + tableName + "]";
        }

        private async Task OpenConnection(int retry = 0, int max = 3)
        {
            try
            {
                await this.connection.OpenAsync();
            }
            catch (SqlException e)
            {
                if (e.Number == -2 && retry < max) //connection failed
                {
                    await this.OpenConnection(retry + 1, max);
                    return;
                }
                
                throw;
            }
            
        }

        public async Task Initialize()
        {
            await this.OpenConnection();
            //using (var conn = new SqlConnection(this.connection))
            {
                //await conn.OpenAsync();

                string txt = @"IF NOT EXISTS 
                        (SELECT * FROM sys.objects 
                            WHERE object_id = OBJECT_ID(N'[dbo]."+ this.tableName + @"') 
                            AND type in (N'U'))

                        CREATE TABLE [dbo]."+tableName+@"(
                            [Id] [int] PRIMARY KEY IDENTITY,
	                        [Name] [nvarchar](50) NOT NULL,
	                        [Version] [int] NOT NULL,
	                        [Data] [varbinary](max) NOT NULL
                        ) ON [PRIMARY]
";
                using (var cmd = new SqlCommand(txt, this.connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task Append(string name, byte[] data, long expectedVersion)
        {
            await this.OpenConnection();
            //using (var conn = new SqlConnection(this.connection))
            {
                //await conn.OpenAsync();

                using (var tx = this.connection.BeginTransaction())
                {
                    string sql =
                        @"SELECT ISNULL(MAX(Version),0) 
                            FROM " + tableName + @"
                            WHERE Name=@name";

                    int version;
                    using (var cmd = new SqlCommand(sql, this.connection, tx))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        version = (int)cmd.ExecuteScalar();
                        if (expectedVersion >= 0)
                        {
                            if (version != expectedVersion)
                            {
                                throw new AppendOnlyStoreConcurrencyException(version, expectedVersion, name);
                            }
                        }
                    }
                    string txt = @"INSERT INTO " + this.tableName + @" (Name,Version,Data) 
                                VALUES(@name,@version,@data)";

                    using (var cmd = new SqlCommand(txt, this.connection, tx))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@version", version + 1);
                        cmd.Parameters.AddWithValue("@data", data);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }

        public async Task<DataWithVersion[]> ReadRecords(string name, long afterVersion, int maxCount)
        {
            await this.OpenConnection();
            //using (var conn = new SqlConnection(this.connection))
            {
              //  await conn.OpenAsync();

                string sql =
                    string.Format(@"SELECT TOP (@take) Data, Version FROM {0}
                        WHERE Name = @p1 AND Version > @skip
                        ORDER BY Version", tableName);
                using (var cmd = new SqlCommand(sql, this.connection))
                {
                    cmd.Parameters.AddWithValue("@p1", name);
                    cmd.Parameters.AddWithValue("@take", maxCount);
                    cmd.Parameters.AddWithValue("@skip", afterVersion);

                    var list = new List<DataWithVersion>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = (byte[])reader["Data"];
                            var version = (int)reader["Version"];
                            list.Add(new DataWithVersion(version, data));
                        }
                    }

                    return list.ToArray();
                }
            }
        }

        public async Task<DataWithName[]> ReadRecords(long afterVersion, int maxCount)
        {
            await this.OpenConnection();
            //using (var conn = new SqlConnection(this.connection))
            {
//                await conn.OpenAsync();

                string sql = string.Format(@"SELECT TOP (@take) Data, Name FROM {0}
                        WHERE Id > @skip
                        ORDER BY Id", tableName);
                using (var cmd = new SqlCommand(sql, this.connection))
                {

                    cmd.Parameters.AddWithValue("@take", maxCount);
                    cmd.Parameters.AddWithValue("@skip", afterVersion);

                    var list = new List<DataWithName>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = (byte[])reader["Data"];
                            var name = (string)reader["Name"];
                            list.Add(new DataWithName(name, data));
                        }
                    }

                    return list.ToArray();
                }
            }
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}