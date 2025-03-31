using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;

namespace Beep.DeveloperAssistant.Logic
{
    public class DeveloperWebUtilities
    {
        private readonly IDMEEditor _dmeEditor;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _rateLimiter;

        public DeveloperWebUtilities(IDMEEditor dmeEditor, int maxRequestsPerMinute = 60, bool ignoreSslErrors = false)
        {
            _dmeEditor = dmeEditor ?? throw new ArgumentNullException(nameof(dmeEditor));
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                ServerCertificateCustomValidationCallback = ignoreSslErrors ? (message, cert, chain, errors) => true : null
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            _rateLimiter = new SemaphoreSlim(maxRequestsPerMinute, maxRequestsPerMinute);
        }

        #region GET Operations

        public async Task<(string Content, HttpResponseHeaders Headers)> GetTextAsync(
            string url,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            TimeSpan? customTimeout = null,
            Dictionary<string, string> headers = null,
            (string username, string password)? basicAuth = null,
            string proxyAddress = null,
            string[] acceptedContentTypes = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!IsValidUrl(url, out Uri uri))
                    return (null, null);

                await _rateLimiter.WaitAsync(cancellationToken);
                try
                {
                    HttpClient client = ConfigureClient(proxyAddress, customTimeout);
                    var result = await FetchTextWithRetries(client, uri, maxRetries, retryDelay ?? TimeSpan.FromSeconds(2), headers, basicAuth, acceptedContentTypes, cancellationToken);
                    return (result.Content, result.Headers);
                }
                finally
                {
                    Task.Delay(TimeSpan.FromSeconds(60) / _rateLimiter.CurrentCount, cancellationToken).ContinueWith(_ => _rateLimiter.Release(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("GetTextAsync", $"Error reading from URL: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return (null, null);
            }
        }

        public async Task<List<string>> GetLinesAsync(string url, int maxRetries = 3, TimeSpan? retryDelay = null, Dictionary<string, string> headers = null, (string username, string password)? basicAuth = null, string proxyAddress = null, CancellationToken cancellationToken = default)
        {
            var (content, _) = await GetTextAsync(url, maxRetries, retryDelay, null, headers, basicAuth, proxyAddress, new[] { "text/plain", "text/csv", "application/octet-stream" }, cancellationToken);
            return content?.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList() ?? new List<string>();
        }

        public async Task ProcessLinesAsync(string url, Func<string, Task> lineProcessor, int maxRetries = 3, TimeSpan? retryDelay = null, Dictionary<string, string> headers = null, (string username, string password)? basicAuth = null, string proxyAddress = null, CancellationToken cancellationToken = default)
        {
            if (!IsValidUrl(url, out Uri uri))
                return;

            HttpClient client = ConfigureClient(proxyAddress);
            try
            {
                await _rateLimiter.WaitAsync(cancellationToken);
                try
                {
                    int attempt = 0;
                    while (true)
                    {
                        try
                        {
                            using (var request = CreateRequest(HttpMethod.Get, uri, headers, basicAuth))
                            {
                                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                                response.EnsureSuccessStatusCode();
                                ValidateContentType(response, new[] { "text/plain", "text/csv", "application/octet-stream" });

                                using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken))
                                using (var reader = new StreamReader(stream))
                                {
                                    string line;
                                    while ((line = await reader.ReadLineAsync()) != null)
                                        await lineProcessor(line);
                                }
                                return;
                            }
                        }
                        catch (HttpRequestException ex) when (IsTransientError(ex) && attempt < maxRetries)
                        {
                            attempt++;
                            await HandleRetry(attempt, maxRetries, retryDelay ?? TimeSpan.FromSeconds(2), ex.Message, "ProcessLinesAsync");
                        }
                        catch (Exception ex)
                        {
                            _dmeEditor.AddLogMessage("ProcessLinesAsync", $"Failed after {attempt + 1} attempts: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                            return;
                        }
                    }
                }
                finally
                {
                    Task.Delay(TimeSpan.FromSeconds(60) / _rateLimiter.CurrentCount, cancellationToken).ContinueWith(_ => _rateLimiter.Release(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("ProcessLinesAsync", $"Error processing URL stream: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
            finally
            {
                if (client != _httpClient) client.Dispose();
            }
        }

        public async Task<bool> DownloadFileAsync(string url, string destinationPath, Action<long, long> progressCallback = null, int maxRetries = 3, TimeSpan? retryDelay = null, Dictionary<string, string> headers = null, (string username, string password)? basicAuth = null, string proxyAddress = null, CancellationToken cancellationToken = default)
        {
            if (!IsValidUrl(url, out Uri uri))
                return false;

            HttpClient client = ConfigureClient(proxyAddress);
            try
            {
                await _rateLimiter.WaitAsync(cancellationToken);
                try
                {
                    int attempt = 0;
                    while (true)
                    {
                        try
                        {
                            using (var request = CreateRequest(HttpMethod.Get, uri, headers, basicAuth))
                            {
                                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                                response.EnsureSuccessStatusCode();

                                var totalBytes = response.Content.Headers.ContentLength ?? -1;
                                long downloadedBytes = 0;

                                using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                                using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    var buffer = new byte[8192];
                                    int bytesRead;
                                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                                    {
                                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                                        downloadedBytes += bytesRead;
                                        progressCallback?.Invoke(downloadedBytes, totalBytes);
                                    }
                                }
                                return true;
                            }
                        }
                        catch (HttpRequestException ex) when (IsTransientError(ex) && attempt < maxRetries)
                        {
                            attempt++;
                            await HandleRetry(attempt, maxRetries, retryDelay ?? TimeSpan.FromSeconds(2), ex.Message, "DownloadFileAsync");
                        }
                        catch (Exception ex)
                        {
                            _dmeEditor.AddLogMessage("DownloadFileAsync", $"Failed after {attempt + 1} attempts: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                            return false;
                        }
                    }
                }
                finally
                {
                    Task.Delay(TimeSpan.FromSeconds(60) / _rateLimiter.CurrentCount, cancellationToken).ContinueWith(_ => _rateLimiter.Release(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("DownloadFileAsync", $"Error downloading file: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
            finally
            {
                if (client != _httpClient) client.Dispose();
            }
        }

        #endregion

        #region POST, PUT, DELETE Operations

        public async Task<(string Content, HttpResponseHeaders Headers)> PostTextAsync(
            string url,
            string content,
            string contentType = "text/plain",
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            TimeSpan? customTimeout = null,
            Dictionary<string, string> headers = null,
            (string username, string password)? basicAuth = null,
            string proxyAddress = null,
            CancellationToken cancellationToken = default)
        {
            if (!IsValidUrl(url, out Uri uri))
                return (null, null);

            HttpClient client = ConfigureClient(proxyAddress, customTimeout);
            try
            {
                await _rateLimiter.WaitAsync(cancellationToken);
                try
                {
                    int attempt = 0;
                    while (true)
                    {
                        try
                        {
                            using (var request = CreateRequest(HttpMethod.Post, uri, headers, basicAuth))
                            {
                                request.Content = new StringContent(content, Encoding.UTF8, contentType);
                                var response = await client.SendAsync(request, cancellationToken);
                                response.EnsureSuccessStatusCode();
                                return (await response.Content.ReadAsStringAsync(cancellationToken), response.Headers);
                            }
                        }
                        catch (HttpRequestException ex) when (IsTransientError(ex) && attempt < maxRetries)
                        {
                            attempt++;
                            await HandleRetry(attempt, maxRetries, retryDelay ?? TimeSpan.FromSeconds(2), ex.Message, "PostTextAsync");
                        }
                        catch (Exception ex)
                        {
                            _dmeEditor.AddLogMessage("PostTextAsync", $"Failed after {attempt + 1} attempts: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                            return (null, null);
                        }
                    }
                }
                finally
                {
                    Task.Delay(TimeSpan.FromSeconds(60) / _rateLimiter.CurrentCount, cancellationToken).ContinueWith(_ => _rateLimiter.Release(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("PostTextAsync", $"Error posting to URL: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return (null, null);
            }
            finally
            {
                if (client != _httpClient) client.Dispose();
            }
        }

        public async Task<(string Content, HttpResponseHeaders Headers)> PostFormAsync(
            string url,
            Dictionary<string, string> formData,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            TimeSpan? customTimeout = null,
            Dictionary<string, string> headers = null,
            (string username, string password)? basicAuth = null,
            string proxyAddress = null,
            CancellationToken cancellationToken = default)
        {
            if (!IsValidUrl(url, out Uri uri))
                return (null, null);

            HttpClient client = ConfigureClient(proxyAddress, customTimeout);
            try
            {
                await _rateLimiter.WaitAsync(cancellationToken);
                try
                {
                    int attempt = 0;
                    while (true)
                    {
                        try
                        {
                            using (var request = CreateRequest(HttpMethod.Post, uri, headers, basicAuth))
                            {
                                request.Content = new FormUrlEncodedContent(formData);
                                var response = await client.SendAsync(request, cancellationToken);
                                response.EnsureSuccessStatusCode();
                                return (await response.Content.ReadAsStringAsync(cancellationToken), response.Headers);
                            }
                        }
                        catch (HttpRequestException ex) when (IsTransientError(ex) && attempt < maxRetries)
                        {
                            attempt++;
                            await HandleRetry(attempt, maxRetries, retryDelay ?? TimeSpan.FromSeconds(2), ex.Message, "PostFormAsync");
                        }
                        catch (Exception ex)
                        {
                            _dmeEditor.AddLogMessage("PostFormAsync", $"Failed after {attempt + 1} attempts: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                            return (null, null);
                        }
                    }
                }
                finally
                {
                    Task.Delay(TimeSpan.FromSeconds(60) / _rateLimiter.CurrentCount, cancellationToken).ContinueWith(_ => _rateLimiter.Release(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("PostFormAsync", $"Error posting form: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return (null, null);
            }
            finally
            {
                if (client != _httpClient) client.Dispose();
            }
        }

        public async Task<(string Content, HttpResponseHeaders Headers)> PostMultipartAsync(
            string url,
            Dictionary<string, string> formData,
            Dictionary<string, (string FilePath, string ContentType)> files,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            TimeSpan? customTimeout = null,
            Dictionary<string, string> headers = null,
            (string username, string password)? basicAuth = null,
            string proxyAddress = null,
            CancellationToken cancellationToken = default)
        {
            if (!IsValidUrl(url, out Uri uri))
                return (null, null);

            HttpClient client = ConfigureClient(proxyAddress, customTimeout);
            try
            {
                await _rateLimiter.WaitAsync(cancellationToken);
                try
                {
                    int attempt = 0;
                    while (true)
                    {
                        try
                        {
                            using (var request = CreateRequest(HttpMethod.Post, uri, headers, basicAuth))
                            {
                                var multipartContent = new MultipartFormDataContent();
                                foreach (var kvp in formData)
                                    multipartContent.Add(new StringContent(kvp.Value), kvp.Key);
                                foreach (var file in files)
                                {
                                    var fileContent = new StreamContent(File.OpenRead(file.Value.FilePath));
                                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.Value.ContentType);
                                    multipartContent.Add(fileContent, file.Key, Path.GetFileName(file.Value.FilePath));
                                }
                                request.Content = multipartContent;

                                var response = await client.SendAsync(request, cancellationToken);
                                response.EnsureSuccessStatusCode();
                                return (await response.Content.ReadAsStringAsync(cancellationToken), response.Headers);
                            }
                        }
                        catch (HttpRequestException ex) when (IsTransientError(ex) && attempt < maxRetries)
                        {
                            attempt++;
                            await HandleRetry(attempt, maxRetries, retryDelay ?? TimeSpan.FromSeconds(2), ex.Message, "PostMultipartAsync");
                        }
                        catch (Exception ex)
                        {
                            _dmeEditor.AddLogMessage("PostMultipartAsync", $"Failed after {attempt + 1} attempts: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                            return (null, null);
                        }
                    }
                }
                finally
                {
                    Task.Delay(TimeSpan.FromSeconds(60) / _rateLimiter.CurrentCount, cancellationToken).ContinueWith(_ => _rateLimiter.Release(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("PostMultipartAsync", $"Error posting multipart: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return (null, null);
            }
            finally
            {
                if (client != _httpClient) client.Dispose();
            }
        }

        public async Task<(string Content, HttpResponseHeaders Headers)> PutTextAsync(
            string url,
            string content,
            string contentType = "text/plain",
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            TimeSpan? customTimeout = null,
            Dictionary<string, string> headers = null,
            (string username, string password)? basicAuth = null,
            string proxyAddress = null,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync(HttpMethod.Put, url, content, contentType, maxRetries, retryDelay, customTimeout, headers, basicAuth, proxyAddress, cancellationToken);
        }

        public async Task<(string Content, HttpResponseHeaders Headers)> DeleteAsync(
            string url,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            TimeSpan? customTimeout = null,
            Dictionary<string, string> headers = null,
            (string username, string password)? basicAuth = null,
            string proxyAddress = null,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteRequestAsync(HttpMethod.Delete, url, null, null, maxRetries, retryDelay, customTimeout, headers, basicAuth, proxyAddress, cancellationToken);
        }

        #endregion

        #region JSON Operations

        public async Task<T> GetJsonAsync<T>(
            string url,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            TimeSpan? customTimeout = null,
            Dictionary<string, string> headers = null,
            (string username, string password)? basicAuth = null,
            string proxyAddress = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (json, _) = await GetTextAsync(url, maxRetries, retryDelay, customTimeout, headers, basicAuth, proxyAddress, new[] { "application/json" }, cancellationToken);
                if (string.IsNullOrEmpty(json))
                    return default;

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _dmeEditor.AddLogMessage("GetJsonAsync", $"Error parsing JSON: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return default;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("GetJsonAsync", $"Error fetching JSON: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return default;
            }
        }

        public async Task<TResponse> PostJsonAsync<TRequest, TResponse>(
            string url,
            TRequest content,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            TimeSpan? customTimeout = null,
            Dictionary<string, string> headers = null,
            (string username, string password)? basicAuth = null,
            string proxyAddress = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(content);
                var (responseText, _) = await PostTextAsync(url, jsonContent, "application/json", maxRetries, retryDelay, customTimeout, headers, basicAuth, proxyAddress, cancellationToken);
                if (string.IsNullOrEmpty(responseText))
                    return default;

                return JsonSerializer.Deserialize<TResponse>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _dmeEditor.AddLogMessage("PostJsonAsync", $"Error parsing JSON response: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return default;
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage("PostJsonAsync", $"Error posting JSON: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return default;
            }
        }

        #endregion

        #region Helper Methods

        private async Task<(string Content, HttpResponseHeaders Headers)> ExecuteRequestAsync(
            HttpMethod method,
            string url,
            string content = null,
            string contentType = null,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            TimeSpan? customTimeout = null,
            Dictionary<string, string> headers = null,
            (string username, string password)? basicAuth = null,
            string proxyAddress = null,
            CancellationToken cancellationToken = default)
        {
            if (!IsValidUrl(url, out Uri uri))
                return (null, null);

            HttpClient client = ConfigureClient(proxyAddress, customTimeout);
            try
            {
                await _rateLimiter.WaitAsync(cancellationToken);
                try
                {
                    int attempt = 0;
                    while (true)
                    {
                        try
                        {
                            using (var request = CreateRequest(method, uri, headers, basicAuth))
                            {
                                if (content != null && contentType != null)
                                    request.Content = new StringContent(content, Encoding.UTF8, contentType);
                                var response = await client.SendAsync(request, cancellationToken);
                                response.EnsureSuccessStatusCode();
                                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                                return (responseContent, response.Headers);
                            }
                        }
                        catch (HttpRequestException ex) when (IsTransientError(ex) && attempt < maxRetries)
                        {
                            attempt++;
                            await HandleRetry(attempt, maxRetries, retryDelay ?? TimeSpan.FromSeconds(2), ex.Message, $"{method.Method}RequestAsync");
                        }
                        catch (Exception ex)
                        {
                            _dmeEditor.AddLogMessage($"{method.Method}RequestAsync", $"Failed after {attempt + 1} attempts: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                            return (null, null);
                        }
                    }
                }
                finally
                {
                    Task.Delay(TimeSpan.FromSeconds(60) / _rateLimiter.CurrentCount, cancellationToken).ContinueWith(_ => _rateLimiter.Release(), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _dmeEditor.AddLogMessage($"{method.Method}RequestAsync", $"Error executing request: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return (null, null);
            }
            finally
            {
                if (client != _httpClient) client.Dispose();
            }
        }

        private HttpClient ConfigureClient(string proxyAddress = null, TimeSpan? customTimeout = null)
        {
            if (string.IsNullOrEmpty(proxyAddress) && !customTimeout.HasValue)
                return _httpClient;

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            if (!string.IsNullOrEmpty(proxyAddress))
            {
                handler.Proxy = new WebProxy(proxyAddress);
                handler.UseProxy = true;
            }
            var client = new HttpClient(handler);
            client.Timeout = customTimeout ?? _httpClient.Timeout;
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            return client;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, Uri url, Dictionary<string, string> headers, (string username, string password)? basicAuth)
        {
            var request = new HttpRequestMessage(method, url);
            if (headers != null)
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);
            if (basicAuth.HasValue)
            {
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{basicAuth.Value.username}:{basicAuth.Value.password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            }
            return request;
        }

        private bool IsValidUrl(string url, out Uri uri)
        {
            uri = null;
            if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                _dmeEditor.AddLogMessage("IsValidUrl", $"Invalid URL: {url}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
            return true;
        }

        private void ValidateContentType(HttpResponseMessage response, string[] acceptedContentTypes)
        {
            if (acceptedContentTypes == null || !acceptedContentTypes.Any())
                return;

            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (!acceptedContentTypes.Contains(contentType))
                throw new InvalidOperationException($"Unexpected content type: {contentType}. Expected: {string.Join(", ", acceptedContentTypes)}");
        }

        private bool IsTransientError(HttpRequestException ex)
        {
            if (ex.StatusCode.HasValue)
            {
                var status = (int)ex.StatusCode.Value;
                return status == 408 || status == 429 || (status >= 500 && status < 600);
            }
            return ex.InnerException is TimeoutException || ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase);
        }

        private async Task HandleRetry(int attempt, int maxRetries, TimeSpan retryDelay, string errorMessage, string methodName)
        {
            _dmeEditor.AddLogMessage(methodName, $"Transient error on attempt {attempt}/{maxRetries}: {errorMessage}. Retrying in {retryDelay.TotalSeconds} seconds.", DateTime.Now, 0, null, Errors.Warning);
            await Task.Delay(retryDelay);
        }
        private async Task<(string Content, HttpResponseHeaders Headers)> FetchTextWithRetries(
    HttpClient client,
    Uri url,
    int maxRetries,
    TimeSpan retryDelay,
    Dictionary<string, string> headers,
    (string username, string password)? basicAuth,
    string[] acceptedContentTypes,
    CancellationToken cancellationToken)
        {
            int attempt = 0;
            while (true)
            {
                try
                {
                    using (var request = CreateRequest(HttpMethod.Get, url, headers, basicAuth))
                    {
                        var response = await client.SendAsync(request, cancellationToken);
                        response.EnsureSuccessStatusCode();
                        ValidateContentType(response, acceptedContentTypes);
                        string content = await response.Content.ReadAsStringAsync(cancellationToken);
                        return (content, response.Headers);
                    }
                }
                catch (HttpRequestException ex) when (IsTransientError(ex) && attempt < maxRetries)
                {
                    attempt++;
                    await HandleRetry(attempt, maxRetries, retryDelay, ex.Message, "FetchTextWithRetries");
                }
                catch (Exception ex)
                {
                    _dmeEditor.AddLogMessage("FetchTextWithRetries", $"Failed after {attempt + 1} attempts: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                    return (null, null);
                }
            }
        }
        #endregion
    }
}