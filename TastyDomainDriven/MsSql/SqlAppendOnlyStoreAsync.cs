

namespace TastyDomainDriven.MsSql
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    using AsyncImpl;

    public sealed class SqlAppendOnlyStoreAsync : IAppendOnlyAsync
    {
        readonly string connectionString;

        private readonly string tableName;

        public SqlAppendOnlyStoreAsync(string connectionString, string tableName = "events")
        {
            this.connectionString = connectionString;
            this.tableName = "[" + tableName + "]";
        }

        public async Task Initialize()
        {
            using (var conn = new SqlConnection(this.connectionString))
            {
                await conn.OpenAsync();

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
                using (var cmd = new SqlCommand(txt, conn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task Append(string name, byte[] data, long expectedVersion)
        {
            using (var conn = new SqlConnection(this.connectionString))
            {
                await conn.OpenAsync();

                using (var tx = conn.BeginTransaction())
                {
                    string sql =
                        @"SELECT ISNULL(MAX(Version),0) 
                            FROM " + tableName + @"
                            WHERE Name=@name";

                    int version;
                    using (var cmd = new SqlCommand(sql, conn, tx))
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

                    using (var cmd = new SqlCommand(txt, conn, tx))
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
            using (var conn = new SqlConnection(this.connectionString))
            {
                await conn.OpenAsync();

                string sql =
                    string.Format(@"SELECT TOP (@take) Data, Version FROM {0}
                        WHERE Name = @p1 AND Version > @skip
                        ORDER BY Version", tableName);
                using (var cmd = new SqlCommand(sql, conn))
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
            using (var conn = new SqlConnection(this.connectionString))
            {
                await conn.OpenAsync();

                string sql = string.Format(@"SELECT TOP (@take) Data, Name FROM {0}
                        WHERE Id > @skip
                        ORDER BY Id", tableName);
                using (var cmd = new SqlCommand(sql, conn))
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
    }
}