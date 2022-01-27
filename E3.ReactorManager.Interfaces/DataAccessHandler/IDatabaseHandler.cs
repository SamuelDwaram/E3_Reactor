using System.Data;

namespace E3.ReactorManager.Interfaces.DataAccessHandler
{
    public interface IDatabaseHandler
    {
        IDbConnection CreateConnection();

        void CloseConnection(IDbConnection connection);

        IDbCommand CreateCommand(string commandText, CommandType commandType, IDbConnection connection);

        IDataAdapter CreateAdapter(IDbCommand command);

        IDbDataParameter CreateParameter(IDbCommand command);
    }
}
