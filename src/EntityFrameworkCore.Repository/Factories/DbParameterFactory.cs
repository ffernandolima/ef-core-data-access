using System;
using System.Data;
using System.Data.Common;

namespace EntityFrameworkCore.Repository.Factories
{
    public static class DbParameterFactory
    {
        public static DbParameter CreateDbParameter<TParameter>(string parameterName, object parameterValue, ParameterDirection direction = ParameterDirection.Input) where TParameter : DbParameter
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException($"{nameof(parameterName)} cannot be null or white-space.", nameof(parameterName));
            }

            var parameter = Activator.CreateInstance<TParameter>();
            {
                parameter.ParameterName = parameterName;
                parameter.Value = parameterValue ?? DBNull.Value;
                parameter.Direction = direction;
            }

            return parameter;
        }
    }
}
