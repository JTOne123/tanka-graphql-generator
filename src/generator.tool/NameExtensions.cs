using System;

namespace Tanka.GraphQL.Generator.Tool
{
    public static class NameExtensions
    {
        public static string ToControllerName(this string name)
        {
            return $"{name}Controller";
        }

        public static string ToInterfaceName(this string name)
        {
            var capitalized = name.Capitalize();
            return $"I{capitalized}";
        }

        public static string Capitalize(this string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            
            return $"{name.Substring(0, 1).ToUpperInvariant()}{name.Substring(1)}";
        }
    }
}