namespace MEMS.Public.Services;

public static class SafeLink
{
    // Only absolute http(s) URLs become links, so a bad stored value (e.g. "javascript:...") is never rendered as one.
    public static string? Http(string? url)
        => Uri.TryCreate(url, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps)
            ? u.AbsoluteUri : null;
}
