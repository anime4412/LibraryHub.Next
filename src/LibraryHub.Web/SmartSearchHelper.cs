using System.Globalization;
using System.Text;
using LibraryHub.Domain;

namespace LibraryHub.Web;

public static class SmartSearchHelper
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        var lastWasSpace = true;

        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;
            var mapped = ch switch
            {
                'đ' or 'Đ' => 'd',
                _ => char.ToLowerInvariant(ch)
            };
            if (char.IsLetterOrDigit(mapped))
            {
                sb.Append(mapped);
                lastWasSpace = false;
            }
            else if (!lastWasSpace)
            {
                sb.Append(' ');
                lastWasSpace = true;
            }
        }
        return sb.ToString().Trim();
    }

    public static string[] Tokenize(string? value) =>
        Normalize(value).Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    public static int ScoreBook(Book book, string? rawQuery)
    {
        var normQuery = Normalize(rawQuery);
        if (string.IsNullOrEmpty(normQuery)) return 0;

        var title = Normalize(book.Title);
        var author = Normalize(book.Author?.Name);
        var category = Normalize(book.Category?.Name);
        var keywords = Normalize(book.Keywords);
        var isbn = Normalize(book.Isbn);
        var description = Normalize(book.Description);

        var score = 0;

        if (title == normQuery || isbn == normQuery) score += 200;
        else if (title.StartsWith(normQuery, StringComparison.Ordinal)) score += 130;
        else if (title.Contains(normQuery, StringComparison.Ordinal)) score += 95;

        if (!string.IsNullOrEmpty(author) && author.Contains(normQuery, StringComparison.Ordinal)) score += 70;
        if (!string.IsNullOrEmpty(category) && category.Contains(normQuery, StringComparison.Ordinal)) score += 65;
        if (!string.IsNullOrEmpty(keywords) && keywords.Contains(normQuery, StringComparison.Ordinal)) score += 50;
        if (!string.IsNullOrEmpty(isbn) && isbn.Contains(normQuery, StringComparison.Ordinal)) score += 90;
        if (!string.IsNullOrEmpty(description) && description.Contains(normQuery, StringComparison.Ordinal)) score += 25;

        var queryTokens = normQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (queryTokens.Length > 0)
        {
            var matchedTokens = 0;
            var titleWords = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var metaWords = $"{author} {category} {keywords}".Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in queryTokens)
            {
                var tokenScore = 0;
                if (title.Contains(token, StringComparison.Ordinal))
                    tokenScore += 36;
                else if (HasFuzzyWordMatch(token, titleWords))
                    tokenScore += 22;

                if (author.Contains(token, StringComparison.Ordinal)
                    || category.Contains(token, StringComparison.Ordinal)
                    || keywords.Contains(token, StringComparison.Ordinal)
                    || isbn.Contains(token, StringComparison.Ordinal))
                    tokenScore += 24;
                else if (HasFuzzyWordMatch(token, metaWords))
                    tokenScore += 14;
                else if (description.Contains(token, StringComparison.Ordinal))
                    tokenScore += 10;

                if (tokenScore > 0)
                {
                    matchedTokens++;
                    score += tokenScore;
                }
            }

            if (matchedTokens == queryTokens.Length && queryTokens.Length > 1)
                score += 40;
        }

        if (score > 0 && book.AvailableQuantity > 0) score += 4;
        return score;
    }

    public static int ScoreCategory(Category category, string? rawQuery)
    {
        var normQuery = Normalize(rawQuery);
        if (string.IsNullOrEmpty(normQuery)) return 0;

        var name = Normalize(category.Name);
        var desc = Normalize(category.Description);
        var score = 0;

        if (name == normQuery) score += 160;
        else if (name.StartsWith(normQuery, StringComparison.Ordinal)) score += 110;
        else if (name.Contains(normQuery, StringComparison.Ordinal)) score += 80;
        else if (desc.Contains(normQuery, StringComparison.Ordinal)) score += 30;
        else
        {
            var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in normQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (name.Contains(token, StringComparison.Ordinal)) score += 35;
                else if (HasFuzzyWordMatch(token, words)) score += 20;
            }
        }
        return score;
    }

    private static bool HasFuzzyWordMatch(string token, string[] words)
    {
        if (token.Length < 4) return false;
        var maxEdits = token.Length >= 7 ? 2 : 1;
        foreach (var word in words)
        {
            if (Math.Abs(word.Length - token.Length) > maxEdits) continue;
            if (word.StartsWith(token[..Math.Min(3, token.Length)], StringComparison.Ordinal)
                && Levenshtein(token, word) <= maxEdits)
                return true;
            if (Levenshtein(token, word) <= 1)
                return true;
        }
        return false;
    }

    private static int Levenshtein(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;
        Span<int> prev = stackalloc int[b.Length + 1];
        Span<int> curr = stackalloc int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++) prev[j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(Math.Min(curr[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
            }
            curr.CopyTo(prev);
        }
        return prev[b.Length];
    }
}
