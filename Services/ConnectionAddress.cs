namespace Smartitecture.Services;

public static class ConnectionAddress
{
    public static string Normalize(string? value)
    {
        var address = (value ?? string.Empty).Trim().TrimEnd('/');
        if (address.Length == 0) return string.Empty;
        if (!address.Contains("://", StringComparison.Ordinal))
        {
            var local = Uri.TryCreate("http://" + address, UriKind.Absolute, out var candidate) && candidate.IsLoopback;
            address = (local ? "http://" : "https://") + address;
        }
        return address;
    }

    public static bool IsValid(string address) => string.IsNullOrEmpty(address)
        || (Uri.TryCreate(address, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || (uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback))
            && string.IsNullOrEmpty(uri.UserInfo)
            && string.IsNullOrEmpty(uri.Query)
            && string.IsNullOrEmpty(uri.Fragment));
}
