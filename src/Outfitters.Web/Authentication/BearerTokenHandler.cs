using System.Net.Http.Headers;

namespace Outfitters.Web.Authentication;

public sealed class BearerTokenHandler : DelegatingHandler
{
    private readonly AuthSession _session;

    public BearerTokenHandler(AuthSession session) => _session = session;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
