using System.Globalization;
using System.Text;

namespace BlogKrafter.Content;

/// <summary>Turns titles and typed addresses into the slugs stored on posts, categories, and tags.</summary>
public static class Slugs
{
    /// <summary>
    /// Lower-case ASCII letters and digits separated by single hyphens. Accents are folded
    /// (<c>Café</c> becomes <c>cafe</c>); anything else that is not a letter or digit becomes a separator.
    /// Returns empty when nothing usable is left, such as for a title written only in symbols.
    /// </summary>
    /// <param name="text">The title or typed address. <see langword="null"/> and blank return an empty string.</param>
    /// <param name="maxLength">The longest result. The default is the slug column length. A cut ends on a hyphen being removed.</param>
    /// <returns>The slug, or an empty string when nothing usable remains.</returns>
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
    /// <param name="slug">The slug to extend. It is shortened when the suffix would pass the column length.</param>
    /// <param name="number">The suffix, written with invariant digits.</param>
    /// <returns>The slug with the suffix.</returns>
    public static string WithSuffix(string slug, int number)
    {
        string suffix = "-" + number.ToString(CultureInfo.InvariantCulture);
        int room = Constants.MaxLengths.Slug - suffix.Length;
        return (slug.Length <= room ? slug : slug[..room].TrimEnd('-')) + suffix;
    }
}