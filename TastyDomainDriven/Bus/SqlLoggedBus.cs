namespace TastyDomainDriven.Bus
{
	using System;
	using System.Collections.Generic;
	using System.Data.SqlClient;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization.Formatters.Binary;

	public abstract class SqlLoggedBus : IBus
    {
        private readonly IBus bus;

        private readonly string connection;

        private readonly string tablename;

        public SqlLoggedBus(IBus bus, string connection, string tablename = "commandlogs")
        {
            this.bus = bus;
            this.connection = connection;
            this.tablename = tablename;
        }

        public void Dispatch(ICommand cmd)
        {
            try
            {
                this.bus.Dispatch(cmd);
                this.Log(cmd.GetType().FullName, this.ToJson(cmd), this.SerializeObject(cmd), true);
            }
            catch (Exception ex)
            {
                var str = new string(ex.ToString().Take(499).ToArray());
                this.Log(cmd.GetType().FullName, str, this.SerializeObject(cmd), false);
                throw;
            }
        }

        public class CommandEntry
        {
            public ICommand Command { get; set; }

            public DateTime Timestamp { get; set; }

            public string Details { get; set; }

            public bool Success { get; set; }

            public int EntryId { get; set; }
        }

        public IEnumerable<CommandEntry> GetLogs(DateTime? from = null, DateTime? to = null)
        {
            using (var conn = new SqlConnection(this.connection))
            {
                conn.Open();

                string sql =
                    string.Format(@"SELECT [CommandId], [Json], [data], [success], [Timestamp] FROM {0}
                        WHERE [Timestamp] > @from and [Timestamp] < @to
                        ORDER BY [Timestamp]", this.tablename);
                
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@from", from ?? DateTime.MinValue);
                    cmd.Parameters.AddWithValue("@to", to ?? DateTime.MaxValue);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = (byte[])reader["Data"];
                            using (var memStream = new MemoryStream(data))
                            {                                
                                yield return
                                    new CommandEntry
                                    {
                                        EntryId = (int)reader["CommandId"],
                                        Command = (ICommand)this.formatter.Deserialize(memStream),
                                        Timestamp = (DateTime)reader["timestamp"],
                                        Details = reader["Json"].ToString(),
                                        Success = (bool)reader["success"]
                                    };
                            }
                        }
                    }
                }
            } 
        }

        private void Log(string name, string json, byte[] data, bool success)
        {
            using (var conn = new SqlConnection(this.connection))
            {
                conn.Open();

                
                using (var tx = conn.BeginTransaction())
                {
                    string txt = @"INSERT INTO " + this.tablename + @" (commandtype, json, success, data) 
                                VALUES(@commandtype, @message, @success, @data)";
                    
                    using (var cmd = new SqlCommand(txt, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@commandtype", name);
                        cmd.Parameters.AddWithValue("@message", json);
                        cmd.Parameters.AddWithValue("@success", success);
                        cmd.Parameters.AddWithValue("@data", data);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }

        readonly BinaryFormatter formatter = new BinaryFormatter();

        byte[] SerializeObject(ICommand cmd)
        {
            using (var mem = new MemoryStream())
            {
                this.formatter.Serialize(mem, cmd);
                return mem.ToArray();
            }
        }

        /// <summary>
        /// Create the table structure
        /// </summary>
        public void Initialize()
        {
            using (var conn = new SqlConnection(this.connection))
            {
                var create = new SqlCommand(@"CREATE TABLE [dbo].[CommandLog](
	[CommandId] [int] IDENTITY(1,1) NOT NULL,
	[CommandType] [nvarchar](200) NOT NULL,
	[Json] [nvarchar](500) NULL,
	[Serialized] [varbinary](max) NULL,
	[Timestamp] [datetime] NOT NULL,
 CONSTRAINT [PK_CommandLog] PRIMARY KEY CLUSTERED 
(
	[CommandId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[CommandLog] ADD  CONSTRAINT [DF_CommandLog_Timestamp]  DEFAULT (getdate()) FOR [Timestamp]
GO", conn);

                create.ExecuteNonQuery();
            }
        }

        public abstract string ToJson(object cmd);

    }
}