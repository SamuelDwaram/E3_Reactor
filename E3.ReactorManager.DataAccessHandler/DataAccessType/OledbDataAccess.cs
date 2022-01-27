using E3.ReactorManager.Interfaces.DataAccessHandler;
using System.Data;
using System.Data.OleDb;

namespace E3.ReactorManager.DataAccessHandler.DataAccessType
{
    public class OledbDataAccess : IDatabaseHandler
    {
        private string ConnectionString { get; set; }

        public OledbDataAccess(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new OleDbConnection(ConnectionString);
        }

        public void CloseConnection(IDbConnection connection)
        {
            var oleDbConnection = (OleDbConnection)connection;
            oleDbConnection.Close();
            oleDbConnection.Dispose();
        }

        public IDbCommand CreateCommand(string commandText, CommandType commandType, IDbConnection connection)
        {
            return new OleDbCommand
            {
                CommandText = commandText,
                Connection = (OleDbConnection)connection,
                CommandType = commandType
            };
        }

        public IDataAdapter CreateAdapter(IDbCommand command)
        {
            return new OleDbDataAdapter((OleDbCommand)command);
        }

        public IDbDataParameter CreateParameter(IDbCommand command)
        {
            OleDbCommand SQLcommand = (OleDbCommand)command;
            return SQLcommand.CreateParameter();
        }
    }
}
