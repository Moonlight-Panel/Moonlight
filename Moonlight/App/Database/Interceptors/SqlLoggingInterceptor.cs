using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moonlight.App.Helpers;

namespace Moonlight.App.Database.Interceptors;

public class SqlLoggingInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        LogSql(command.CommandText);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        LogSql(command.CommandText);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        LogSql(command.CommandText);
        return base.NonQueryExecuting(command, eventData, result);
    }

    private void LogSql(string sql)
    {
        Logger.Info($"[SQL DEBUG] {sql.Replace("\n", "")}");
    }
}