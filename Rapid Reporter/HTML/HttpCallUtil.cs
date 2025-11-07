using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rapid_Reporter.HTML
{
    internal static class HttpCallUtil
    {
        internal const int DefaultTimeout = 300000;
        internal const string DefaultAcceptType = "text/plain";

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(DefaultTimeout)
        };

        internal static HttpResult HttpGetCall(string url)
        {
            // Use async method synchronously for backward compatibility
            return HttpGetCallAsync(url).GetAwaiter().GetResult();
        }

        private static async Task<HttpResult> HttpGetCallAsync(string url)
        {
            var result = new HttpResult();

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Add("Accept", DefaultAcceptType);

                    var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                    
                    result.StatusCode = (int)response.StatusCode;
                    result.Status = response.ReasonPhrase ?? response.StatusCode.ToString();

                    if (response.Content != null)
                    {
                        result.Message = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (TaskCanceledException e) when (e.InnerException is TimeoutException)
            {
                result.Status = "Timeout";
                result.Message = "The request timed out.";
            }
            catch (TaskCanceledException)
            {
                result.Status = "Timeout";
                result.Message = "The request timed out.";
            }
            catch (HttpRequestException e)
            {
                result.Status = "RequestException";
                result.Message = e.Message;
            }
            catch (Exception e)
            {
                result.Status = "Error";
                result.Message = e.Message;
            }

            return result;
        }
    }
}
