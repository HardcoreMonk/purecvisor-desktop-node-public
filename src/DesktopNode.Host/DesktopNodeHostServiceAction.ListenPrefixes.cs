namespace DesktopNode.Host;

public static partial class DesktopNodeHostServiceAction
{
    internal readonly record struct DesktopNodeHostServiceListenPrefixes(
        string Prefix,
        string WebPrefix,
        bool EmitAllowLan);

    internal static DesktopNodeHostServiceListenPrefixes ResolveListenPrefixes(DesktopNodeHostOptions options)
    {
        const string defaultPrefix = "http://127.0.0.1:7777/";
        const string defaultWebPrefix = "http://127.0.0.1:80/";
        if (options.ServiceAction is not ("configure-installed" or "repair-installed"))
        {
            return new DesktopNodeHostServiceListenPrefixes(defaultPrefix, defaultWebPrefix, false);
        }

        var prefix = string.IsNullOrWhiteSpace(options.Prefix) ? defaultPrefix : options.Prefix;
        var webPrefix = options.WebPrefix is null ? defaultWebPrefix : options.WebPrefix;
        var api = ParseConfigurePrefix(prefix);
        var web = ParseConfigurePrefix(webPrefix);
        if (!string.Equals(api.Host, web.Host, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"PCV_HOST_PREFIX_HOST_MISMATCH|The API prefix host and the Web prefix host must match.|API host '{api.Host}' and Web host '{web.Host}'.");
        }

        var loopback = IsConfigureLoopbackHost(api.Host);
        if (!loopback && !options.AllowLan)
        {
            throw new ArgumentException(
                $"PCV_HOST_PREFIX_NOT_LOOPBACK|The .NET service host prefix must stay on loopback unless LAN mode is explicit.|Rejected host '{api.Host}'.");
        }

        if (!loopback && api.Port != 7777)
        {
            throw new ArgumentException(
                $"PCV_HOST_PREFIX_API_PORT|LAN mode API prefix port must be 7777.|Rejected port '{api.Port}'.");
        }

        return new DesktopNodeHostServiceListenPrefixes(prefix, webPrefix, !loopback);
    }

    private readonly record struct ParsedConfigurePrefix(string Host, int Port);

    private static ParsedConfigurePrefix ParseConfigurePrefix(string prefix)
    {
        if (IsRawWildcardHost(prefix))
        {
            throw Wildcard(prefix);
        }

        Uri uri;
        try
        {
            uri = new Uri(prefix);
        }
        catch (UriFormatException)
        {
            throw Scheme(prefix);
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            throw Scheme(prefix);
        }

        var host = uri.DnsSafeHost.Trim().Trim('[', ']').ToLowerInvariant();
        if (host is "+" or "*" or "0.0.0.0" or "::" or "")
        {
            throw Wildcard(prefix);
        }

        return new ParsedConfigurePrefix(host, uri.Port);
    }

    private static bool IsRawWildcardHost(string prefix)
    {
        var schemeSplit = prefix.IndexOf("://", StringComparison.Ordinal);
        if (schemeSplit < 0)
        {
            return false;
        }

        var authority = prefix[(schemeSplit + 3)..];
        var slash = authority.IndexOf('/');
        if (slash >= 0)
        {
            authority = authority[..slash];
        }

        var at = authority.LastIndexOf('@');
        if (at >= 0)
        {
            authority = authority[(at + 1)..];
        }

        if (authority.StartsWith('['))
        {
            var end = authority.IndexOf(']');
            return end > 0 && string.Equals(authority[..(end + 1)], "[::]", StringComparison.OrdinalIgnoreCase);
        }

        var colon = authority.LastIndexOf(':');
        if (colon >= 0)
        {
            authority = authority[..colon];
        }

        return authority is "+" or "*";
    }

    private static bool IsConfigureLoopbackHost(string host)
    {
        return host is "127.0.0.1" or "localhost" or "::1";
    }

    private static void InsertConfigureAllowLan(IList<string> arguments, bool emitAllowLan)
    {
        if (!emitAllowLan || arguments.Count == 0)
        {
            return;
        }

        var webPrefixFlag = arguments.IndexOf("--web-prefix");
        if (webPrefixFlag >= 0 && webPrefixFlag + 2 <= arguments.Count)
        {
            arguments.Insert(webPrefixFlag + 2, "--allow-lan");
        }
    }

    private static ArgumentException Wildcard(string prefix)
    {
        return new ArgumentException(
            $"PCV_HOST_PREFIX_WILDCARD|Wildcard service prefixes are rejected.|Rejected prefix '{prefix}'.");
    }

    private static ArgumentException Scheme(string prefix)
    {
        return new ArgumentException(
            $"PCV_HOST_PREFIX_SCHEME|The service prefix must use the http scheme.|Rejected prefix '{prefix}'.");
    }
}
