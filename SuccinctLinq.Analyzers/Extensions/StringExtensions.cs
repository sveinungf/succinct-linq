using System.Text;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class StringExtensions
{
    extension(string value)
    {
        public string RemoveWhitespace()
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var builder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (!char.IsWhiteSpace(c))
                    builder.Append(c);
            }
            return builder.ToString();
        }
    }
}
