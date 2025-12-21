using System.Reflection;

namespace Core.Auth;

public static class RolesConstants
{
    public const string Operator = "Operator";
    public const string Analyst = "Analyst";
    public const string Admin = "Admin";

    public static IReadOnlyCollection<string> GetRoles()
    {
        return typeof(RolesConstants)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToArray();
    }
}