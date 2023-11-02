using System.Linq.Expressions;
using System.Reflection;

namespace Moonlight.App.Extensions;

public static class TypeExtensions
{
    public static PropertyInfo? GetProperty<T, TValue>(this T type, Expression<Func<T, TValue>> selector)
        where T : class
    {
        Expression expression = selector.Body;

        return expression.NodeType == ExpressionType.MemberAccess
            ? (PropertyInfo) ((MemberExpression) expression).Member
            : null;
    }
}