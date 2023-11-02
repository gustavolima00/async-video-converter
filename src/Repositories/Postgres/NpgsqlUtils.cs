using Npgsql;
using NpgsqlTypes;

namespace Repositories.Postgres;

public static class NpgsqlUtils
{
    public static void AddParameter<T>(this NpgsqlCommand command, string parameterName, T? value, NpgsqlDbType? dbType = null)
    {
        if (value == null)
        {
            command.Parameters.AddWithValue(parameterName, DBNull.Value);
        }
        else
        {
            var param = new NpgsqlParameter(parameterName, value);
            if (dbType.HasValue)
            {
                param.NpgsqlDbType = dbType.Value;
            }
            command.Parameters.Add(param);
        }
    }
}