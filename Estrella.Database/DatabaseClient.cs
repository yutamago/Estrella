using System;
using System.Collections.Generic;
using System.Data;
using Estrella.Database.Storage;
using Estrella.Util;
using MySql.Data.MySqlClient;

namespace Estrella.Database
{
    public sealed class DatabaseClient : IDisposable
    {
        private MySqlCommand Command;
        public int CommandCacheCount;
        public PriorityQueue<MySqlCommand> Commands = new PriorityQueue<MySqlCommand>();

        private MySqlConnection Connection;
        private uint Handle;
        public bool IsBusy;

        private DateTime LastActivity;

        private DatabaseManager Manager;

        public DatabaseClient(uint mHandle, DatabaseManager pManager)
        {
            if (pManager == null)
                throw new ArgumentNullException("pManager");

            Handle = mHandle;
            Manager = pManager;

            Connection = new MySqlConnection(Manager.CreateConnectionString());
            Command = Connection.CreateCommand();

            UpdateLastActivity();
        }


        public bool IsAnonymous
        {
            get { return Handle == 0; }
        }

        public int InactiveTime
        {
            get { return (int) (DateTime.Now - LastActivity).TotalSeconds; }
        }

        public uint mHandle
        {
            get { return Handle; }
        }

        public ConnectionState State
        {
            get { return Connection != null ? Connection.State : ConnectionState.Broken; }
        }

        public void Dispose()
        {
            if (!IsAnonymous) // No disposing for this client yet! Return to the manager!
            {
                IsBusy = false;
                // Reset this!
                // mCommand.CommandText = null;
                Command.Parameters.Clear();

                Manager.ReleaseClient(Handle);
            }
            else // Anonymous client, dispose this right away!
            {
                Destroy();
            }
        }

        public MySqlConnection GetConnection()
        {
            return Connection;
        }

        public ConnectionState Connect()
        {
            if (Connection == null && Connection.ConnectionString == null)
            {
                // Connection.Open();
                new DatabaseException("Connection instance of database client " + Handle + " holds no value.");
                return ConnectionState.Broken;
            }

            if (Connection.State != ConnectionState.Closed)
            {
                new DatabaseException("Connection instance of database client " + Handle +
                                      " requires to be closed before it can open again.");
                return ConnectionState.Open;
            }

            try
            {
                Connection.Open();
            }
            catch (MySqlException mex)
            {
                new DatabaseException("Failed to open connection for database client " + Handle +
                                      ", exception message: " + mex.Message);
                return ConnectionState.Closed;
            }

            return ConnectionState.Connecting;
        }

        public void Disconnect()
        {
            try
            {
                Connection.Close();
            }
            catch
            {
            }
        }

        public void Destroy()
        {
            Disconnect();

            Connection.Dispose();
            Connection = null;

            Command.Dispose();
            Command = null;

            Manager = null;
        }

        public void UpdateLastActivity()
        {
            LastActivity = DateTime.Now;
        }

        public void AddParamWithValue(string sParam, object val)
        {
            Command.Parameters.AddWithValue(sParam, val);
        }

        private void AddParameters(MySqlCommand command, IEnumerable<MySqlParameter> pParams)
        {
            lock (command)
            {
                foreach (var parameter in pParams)
                {
                    command.Parameters.Add(parameter);
                }
            }
        }

        public void ExecuteQuery(string sQuery)
        {
            try
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    Command.CommandText = sQuery;
                    PushCommand(Command);
                }
                else
                {
                    IsBusy = true;
                    Command.CommandText = sQuery;
                    Command.Connection = Connection;
                    PushCommand(Command);
                    for (var i = 0; i < Commands.Count; i++)
                    {
                        var cmd = Commands.Dequeue();
                        cmd.Connection = Command.Connection;
                        cmd.ExecuteScalar();
                        CommandCacheCount--;
                        Console.WriteLine("Ramm Kacke..");
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(LogLevel.Error, e + "\n (" + sQuery + ")");
            }
        }

        public void PushCommand(MySqlCommand command)
        {
            lock (command)
            {
                CommandCacheCount++;
                Commands.Enqueue(command, CommandCacheCount);
            }
        }

        public void ExecuteQueryWithParameters(MySqlCommand query, params MySqlParameter[] pParams)
        {
            try
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    Command = query;
                    PushCommand(Command);
                }
                else
                {
                    IsBusy = true;
                    Command = query;
                    Command.Connection = Connection;
                    PushCommand(Command);
                    for (var i = 0; i < Commands.Count; i++)
                    {
                        var cmd = Commands.Dequeue();
                        cmd.Connection = Command.Connection;
                        cmd.ExecuteScalar();
                        CommandCacheCount--;
                        Console.WriteLine("Ramm Kacke..");
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteLine(LogLevel.Error, e + "\n (" + Command.CommandText + ")");
            }
        }

        public bool FindsResult(string sQuery)
        {
            var found = false;
            IsBusy = true;
            try
            {
                Command.CommandText = sQuery;
                var dReader = Command.ExecuteReader();
                found = dReader.HasRows;
                dReader.Close();
            }
            catch (Exception e)
            {
                Log.WriteLine(LogLevel.Error, e + "\n (" + sQuery + ")");
            }

            return found;
        }

        public DataSet ReadDataSet(string query)
        {
            try
            {
                IsBusy = true;
                var dataSet = new DataSet();
                Command.CommandText = query;

                using (var adapter = new MySqlDataAdapter(Command))
                {
                    adapter.Fill(dataSet);
                }

                // Command.CommandText = null;
                return dataSet;
            }
            catch (DatabaseException ex)
            {
                Log.WriteLine(LogLevel.Error, ex.ToString());
                return null;
            }
        }

        public DataTable ReadDataTable(string query)
        {
            try
            {
                IsBusy = true;
                var dataTable = new DataTable();
                Command.CommandText = query;

                using (var adapter = new MySqlDataAdapter(Command))
                {
                    adapter.Fill(dataTable);
                }

                //  Command.CommandText = null;
                return dataTable;
            }
            catch (DatabaseException ex)
            {
                Log.WriteLine(LogLevel.Error, ex.ToString());
                return null;
            }
        }

        public DataRow ReadDataRow(string query)
        {
            try
            {
                IsBusy = true;
                var dataTable = ReadDataTable(query);

                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    return dataTable.Rows[0];
                }
            }
            catch (DatabaseException ex)
            {
                Log.WriteLine(LogLevel.Error, ex.ToString());
                return null;
            }

            return null;
        }

        public string ReadString(string query)
        {
            try
            {
                IsBusy = true;
                Command.CommandText = query;
                var result = Command.ExecuteScalar().ToString();
                // Command.CommandText = null;
                return result;
            }
            catch (DatabaseException ex)
            {
                Log.WriteLine(LogLevel.Error, ex.ToString());
                return null;
            }
        }

        public uint InsertAndIdentify(string query)
        {
            IsBusy = true;
            var command = Connection.CreateCommand();
            command.CommandText = query;
            return InsertAndIdentifyPublic(command);
        }

        public uint InsertAndIdentifyPublic(MySqlCommand pCommand)
        {
            IsBusy = true;
            pCommand.Prepare();
            pCommand.ExecuteNonQuery();
            pCommand.CommandText = "SELECT LAST_INSERT_ID()";
            pCommand.Parameters.Clear();
            return (uint) (long) pCommand.ExecuteScalar();
        }

        #region ReadMethods

        public uint ReadUInt(string query)
        {
            IsBusy = true;
            Command.CommandText = query;
            var result = uint.Parse(Command.ExecuteScalar().ToString());
            //  Command.CommandText = null;
            return result;
        }

        public int ReadInt32(string query)
        {
            IsBusy = true;
            Command.CommandText = query;
            var result = Convert.ToInt32(Command.ExecuteScalar());
            // Command.CommandText = null;
            return result;
        }

        public byte[] GetBlob(MySqlCommand pCommand)
        {
            byte[] ret;
            try
            {
                IsBusy = true;
                ret = (byte[]) pCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading BLOB: {0} && {1}", ex.Message, ex.StackTrace);
                return null;
            }

            return ret;
        }

        #endregion
    }
}