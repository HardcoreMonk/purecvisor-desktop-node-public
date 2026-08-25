namespace DesktopNode.Delivery.Tests.Contracts;

internal sealed record LegacyPesterContract(int Ordinal, string Name);

internal static class LegacyPesterContractParser
{
    internal const string ErrorCode = "PCV_DELIVERY_LEGACY_PARSE_INVALID";

    internal static IReadOnlyList<LegacyPesterContract> Parse(
        string repositoryRelativePath,
        string source)
    {
        if (string.IsNullOrWhiteSpace(repositoryRelativePath) ||
            repositoryRelativePath.Contains('\0', StringComparison.Ordinal))
        {
            throw Invalid("repository-path");
        }

        ArgumentNullException.ThrowIfNull(source);
        var normalized = source.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var lines = normalized.Split('\n');
        var contracts = new List<LegacyPesterContract>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        var state = MultilineState.None;

        foreach (var line in lines)
        {
            if (state is MultilineState.SingleHereString or MultilineState.DoubleHereString)
            {
                var terminator = state == MultilineState.SingleHereString ? "'@" : "\"@";
                var candidate = line.TrimStart();
                if (candidate.StartsWith(terminator, StringComparison.Ordinal) &&
                    (candidate.Length == terminator.Length ||
                     char.IsWhiteSpace(candidate[terminator.Length]) ||
                     candidate[terminator.Length] is '|' or ';' or ')' or ','))
                {
                    state = MultilineState.None;
                }

                continue;
            }

            var index = 0;
            var statementPosition = true;
            if (state is MultilineState.SingleString or MultilineState.DoubleString)
            {
                var quote = state == MultilineState.SingleString ? '\'' : '"';
                statementPosition = false;
                if (!SkipSourceString(line, ref index, quote, openingConsumed: true))
                {
                    continue;
                }

                state = MultilineState.None;
            }

            while (index < line.Length)
            {
                if (state == MultilineState.BlockComment)
                {
                    var endComment = line.IndexOf("#>", index, StringComparison.Ordinal);
                    if (endComment < 0)
                    {
                        break;
                    }

                    index = endComment + 2;
                    state = MultilineState.None;
                    continue;
                }

                var current = line[index];
                if (char.IsWhiteSpace(current))
                {
                    index++;
                    continue;
                }

                if (current == '#')
                {
                    break;
                }

                if (current == '<' && index + 1 < line.Length && line[index + 1] == '#')
                {
                    state = MultilineState.BlockComment;
                    index += 2;
                    continue;
                }

                if (current == '@' && TryStartHereString(line, index, out var hereState))
                {
                    state = hereState;
                    break;
                }

                if (current is '\'' or '"')
                {
                    var quote = current;
                    if (!SkipSourceString(line, ref index, quote, openingConsumed: false))
                    {
                        state = quote == '\''
                            ? MultilineState.SingleString
                            : MultilineState.DoubleString;
                        break;
                    }

                    statementPosition = false;
                    continue;
                }

                if (current is '{' or '}' or ';')
                {
                    statementPosition = true;
                    index++;
                    continue;
                }

                if (IsTokenStart(current))
                {
                    var tokenStart = index;
                    index++;
                    while (index < line.Length && IsTokenPart(line[index]))
                    {
                        index++;
                    }

                    var token = line[tokenStart..index];
                    if (statementPosition && string.Equals(token, "It", StringComparison.OrdinalIgnoreCase))
                    {
                        ParseItDeclaration(line, ref index, contracts, names);
                    }

                    statementPosition = false;
                    continue;
                }

                statementPosition = false;
                index++;
            }
        }

        if (state == MultilineState.BlockComment)
        {
            throw Invalid("unmatched-comment");
        }

        if (state is MultilineState.SingleHereString or MultilineState.DoubleHereString)
        {
            throw Invalid("unmatched-here-string");
        }

        if (state is MultilineState.SingleString or MultilineState.DoubleString)
        {
            throw Invalid("unmatched-quote");
        }

        return contracts;
    }

    private static void ParseItDeclaration(
        string line,
        ref int index,
        ICollection<LegacyPesterContract> contracts,
        ISet<string> names)
    {
        while (index < line.Length && char.IsWhiteSpace(line[index]))
        {
            index++;
        }

        if (index >= line.Length || line[index] == '`')
        {
            throw Invalid("multiline-declaration");
        }

        if (line[index] is not ('\'' or '"'))
        {
            throw Invalid("dynamic-name");
        }

        var name = ParseLiteral(line, ref index, rejectInterpolation: true);
        if (!names.Add(name))
        {
            throw Invalid("duplicate-name");
        }

        contracts.Add(new LegacyPesterContract(contracts.Count + 1, name));
    }

    private static string ParseLiteral(string line, ref int index, bool rejectInterpolation)
    {
        var quote = line[index++];
        var value = new System.Text.StringBuilder();
        while (index < line.Length)
        {
            var current = line[index++];
            if (quote == '\'' && current == '\'')
            {
                if (index < line.Length && line[index] == '\'')
                {
                    value.Append('\'');
                    index++;
                    continue;
                }

                return value.ToString();
            }

            if (quote == '"' && current == '`')
            {
                if (index >= line.Length)
                {
                    throw Invalid(rejectInterpolation ? "multiline-declaration" : "unmatched-quote");
                }

                value.Append(DecodeBacktick(line[index++]));
                continue;
            }

            if (quote == '"' && current == '"')
            {
                return value.ToString();
            }

            if (quote == '"' && current == '$' && rejectInterpolation)
            {
                throw Invalid("dynamic-name");
            }

            value.Append(current);
        }

        throw Invalid("unmatched-quote");
    }

    private static bool SkipSourceString(
        string line,
        ref int index,
        char quote,
        bool openingConsumed)
    {
        if (!openingConsumed)
        {
            index++;
        }

        while (index < line.Length)
        {
            var current = line[index++];
            if (quote == '\'' && current == '\'')
            {
                if (index < line.Length && line[index] == '\'')
                {
                    index++;
                    continue;
                }

                return true;
            }

            if (quote == '"' && current == '`')
            {
                if (index < line.Length)
                {
                    index++;
                }

                continue;
            }

            if (quote == '"' && current == '"')
            {
                return true;
            }
        }

        return false;
    }

    private static char DecodeBacktick(char value) => value switch
    {
        '0' => '\0',
        'a' => '\a',
        'b' => '\b',
        'e' => '\u001b',
        'f' => '\f',
        'n' => '\n',
        'r' => '\r',
        't' => '\t',
        'v' => '\v',
        _ => value,
    };

    private static bool TryStartHereString(
        string line,
        int index,
        out MultilineState state)
    {
        state = MultilineState.None;
        if (index + 1 >= line.Length || line[index] != '@' || line[index + 1] is not ('\'' or '"'))
        {
            return false;
        }

        if (line[(index + 2)..].Any(value => !char.IsWhiteSpace(value)))
        {
            return false;
        }

        state = line[index + 1] == '\''
            ? MultilineState.SingleHereString
            : MultilineState.DoubleHereString;
        return true;
    }

    private static bool IsTokenStart(char value) => char.IsLetter(value) || value == '_';

    private static bool IsTokenPart(char value) =>
        char.IsLetterOrDigit(value) || value is '_' or '-';

    private static InvalidDataException Invalid(string detail) =>
        new($"{ErrorCode}|{detail}");

    private enum MultilineState
    {
        None,
        BlockComment,
        SingleString,
        DoubleString,
        SingleHereString,
        DoubleHereString,
    }
}
