using System.Text.RegularExpressions;

namespace DesktopNode.Delivery.Tests.Infrastructure;

internal sealed partial class MarkdownContract
{
    private readonly string owner;
    private readonly string[] lines;

    private MarkdownContract(string owner, string source)
    {
        this.owner = owner;
        lines = source.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n');
    }

    internal static MarkdownContract Parse(string owner, string source) =>
        new(DeliveryContractError.RequireOwner(owner), source);

    internal void RequireHeadingOrder(params string[] headings)
    {
        var actual = lines
            .Select(line => HeadingRegex().Match(line))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value.Trim())
            .ToArray();
        RequireOrdered(actual, headings);
    }

    internal void RequireTableRowOrder(params string[] firstCells)
    {
        var actual = lines
            .Where(line => line.TrimStart().StartsWith('|'))
            .Select(ParseCells)
            .Where(cells => cells.Length > 0 && cells.Any(cell => cell.Any(character => character != '-')))
            .Select(cells => cells[0])
            .ToArray();
        RequireOrdered(actual, firstCells);
    }

    internal string RequireSingleKeyValue(string key)
    {
        var matches = lines
            .Select(line => KeyValueRegex().Match(line))
            .Where(match => match.Success && match.Groups[1].Value == key)
            .Select(match => match.Groups[2].Value.Trim())
            .ToArray();
        return matches.Length == 1
            ? matches[0]
            : throw DeliveryContractError.Invalid(owner, "markdown-order");
    }

    private void RequireOrdered(IReadOnlyList<string> actual, IReadOnlyList<string> expected)
    {
        var lastIndex = -1;
        foreach (var value in expected)
        {
            var index = -1;
            for (var candidate = lastIndex + 1; candidate < actual.Count; candidate++)
            {
                if (actual[candidate] == value)
                {
                    index = candidate;
                    break;
                }
            }

            if (index < 0)
            {
                throw DeliveryContractError.Invalid(owner, "markdown-order");
            }

            lastIndex = index;
        }
    }

    private static string[] ParseCells(string line) => line.Trim().Trim('|')
        .Split('|')
        .Select(cell => cell.Trim())
        .ToArray();

    [GeneratedRegex(@"^#{1,6}\s+(.+?)\s*#*\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex HeadingRegex();

    [GeneratedRegex(@"^\s*([A-Za-z0-9_.-]+)\s*[:=]\s*(.*?)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex KeyValueRegex();
}
