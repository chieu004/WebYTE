using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace WebYTE.Client.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;

    public AuthorizationMessageHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Lấy token trực tiếp từ localStorage qua JSRuntime
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken", cancellationToken);
            
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // Nếu không lấy được token, tiếp tục request không có auth
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
