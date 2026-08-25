namespace DesktopNode.Delivery.Tests.Infrastructure;

internal static class SourceContract
{
    internal static void RequireExecutableToken(string owner, string source, string token)
    {
        var normalizedOwner = DeliveryContractError.RequireOwner(owner);
        if (string.IsNullOrEmpty(token) ||
            !StripCommentsAndStrings(source).Contains(token, StringComparison.Ordinal))
        {
            throw DeliveryContractError.Invalid(normalizedOwner, "source-token-context");
        }
    }

    internal static void RequireNoExecutableToken(string owner, string source, string token)
    {
        var normalizedOwner = DeliveryContractError.RequireOwner(owner);
        if (string.IsNullOrEmpty(token) ||
            StripCommentsAndStrings(source).Contains(token, StringComparison.Ordinal))
        {
            throw DeliveryContractError.Invalid(normalizedOwner, "source-token-context");
        }
    }

    internal static void RequireLiteral(string owner, string source, string literal)
    {
        var normalizedOwner = DeliveryContractError.RequireOwner(owner);
        if (string.IsNullOrEmpty(literal) || !source.Contains(literal, StringComparison.Ordinal))
        {
            throw DeliveryContractError.Invalid(normalizedOwner, "source-token-context");
        }
    }

    private static string StripCommentsAndStrings(string source)
    {
        var result = source.ToCharArray();
        var state = LexicalState.Code;
        for (var index = 0; index < source.Length; index++)
        {
            var current = source[index];
            var next = index + 1 < source.Length ? source[index + 1] : '\0';
            switch (state)
            {
                case LexicalState.Code:
                    if (current == '#')
                    {
                        result[index] = ' ';
                        state = LexicalState.LineComment;
                    }
                    else if (current == '/' && next == '/')
                    {
                        result[index] = result[index + 1] = ' ';
                        index++;
                        state = LexicalState.LineComment;
                    }
                    else if (current == '/' && next == '*')
                    {
                        result[index] = result[index + 1] = ' ';
                        index++;
                        state = LexicalState.BlockComment;
                    }
                    else if (current == '\'')
                    {
                        result[index] = ' ';
                        state = LexicalState.SingleQuote;
                    }
                    else if (current == '"')
                    {
                        result[index] = ' ';
                        state = LexicalState.DoubleQuote;
                    }
                    break;

                case LexicalState.LineComment:
                    if (current is '\r' or '\n')
                    {
                        state = LexicalState.Code;
                    }
                    else
                    {
                        result[index] = ' ';
                    }
                    break;

                case LexicalState.BlockComment:
                    result[index] = ' ';
                    if (current == '*' && next == '/')
                    {
                        result[index + 1] = ' ';
                        index++;
                        state = LexicalState.Code;
                    }
                    break;

                case LexicalState.SingleQuote:
                    result[index] = ' ';
                    if (current == '\'' && next == '\'')
                    {
                        result[index + 1] = ' ';
                        index++;
                    }
                    else if (current == '\'')
                    {
                        state = LexicalState.Code;
                    }
                    break;

                case LexicalState.DoubleQuote:
                    result[index] = ' ';
                    if (current == '`' && next != '\0')
                    {
                        result[index + 1] = ' ';
                        index++;
                    }
                    else if (current == '\\' && next != '\0')
                    {
                        result[index + 1] = ' ';
                        index++;
                    }
                    else if (current == '"')
                    {
                        state = LexicalState.Code;
                    }
                    break;
            }
        }

        return new string(result);
    }

    private enum LexicalState
    {
        Code,
        LineComment,
        BlockComment,
        SingleQuote,
        DoubleQuote,
    }
}
