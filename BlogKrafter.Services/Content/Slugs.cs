using System.Globalization;
using System.Text;

namespace BlogKrafter.Content;

public static class Slugs
{
    /// <summary>
    /// Lower-case ASCII letters and digits separated by single hyphens. Accents are folded
    /// (<c>Café</c> becomes <c>cafe</c>); anything else that is not a letter or digit becomes a separator.
    /// Returns empty when nothing usable is left, such as for a title written only in symbols.
    /// </summary>
    public static string Generate(string? text, int maxLength = Constants.MaxLengths.Slug)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string decomposed = text.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        bool pendingSeparator = false;

        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            char lower = char.ToLowerInvariant(c);
            if (lower is (>= 'a' and <= 'z') or (>= '0' and <= '9'))
            {
                if (pendingSeparator && builder.Length > 0)
                {
                    builder.Append('-');
                }

                builder.Append(lower);
                pendingSeparator = false;
            }
            else if (lower != '\'')
            {
                // Apostrophes join rather than split, so "Don't" reads "dont" instead of "don-t".
                pendingSeparator = true;
            }
        }

        string slug = builder.ToString();
        return slug.Length <= maxLength ? slug : slug[..maxLength].TrimEnd('-');
    }

    /// <summary>Appends <c>-2</c>, <c>-3</c> and so on, keeping the result inside the column length.</summary>
    public static string WithSuffix(string slug, int number)
    {
        string suffix = "-" + number.ToString(CultureInfo.InvariantCulture);
        int room = Constants.MaxLengths.Slug - suffix.Length;
        return (slug.Length <= room ? slug : slug[..room].TrimEnd('-')) + suffix;
    }
}