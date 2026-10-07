using System.Text;

namespace SuccinctLinq.Analyzers.Extensions;

internal static class StringExtensions
{
    extension(string value)
    {
        public string CollapseWhitespace()
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var builder = new StringBuilder(value.Length);
            var previousWasWhitespace = false;

            foreach (var c in value)
            {
                if (char.IsWhiteSpace(c))
                {
                    previousWasWhitespace = true;
                    continue;
                }

                if (previousWasWhitespace && builder.Length > 0)
                    builder.Append(' ');

                builder.Append(c);
                previousWasWhitespace = false;
            }

            return builder.ToString();
        }
    }
}
