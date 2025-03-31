using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor; // for IDMEEditor

namespace Beep.DeveloperAssistant.Logic
{
    /// <summary>
    /// Provides extended network utilities including basic connectivity (ping, port checking),
    /// TCP/UDP communication, HTTP file downloads with progress, traceroute functionality,
    /// and retrieval of local network interface information.
    /// This version includes refined error handling, detailed logging, and cancellation support.
    /// </summary>
    public class DeveloperNetworkUtilities
    {
        private readonly IDMEEditor _dmeEditor;
        private static readonly HttpClient _httpClient = new HttpClient();

        public DeveloperNetworkUtilities(IDMEEditor dmeEditor = null)
        {
            _dmeEditor = dmeEditor;
        }

        #region Basic Network Operations

        public async Task<long?> PingHostAsync(string host, int timeout = 4000, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(host, timeout);
                    cancellationToken.ThrowIfCancellationRequested();

                    if (reply.Status == IPStatus.Success)
                    {
                        return reply.RoundtripTime;
                    }
                    else
                    {
                        _dmeEditor?.AddLogMessage("PingHostAsync", $"Ping to {host} failed with status: {reply.Status}", DateTime.Now, 0, null, Errors.Failed);
                        return null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _dmeEditor?.AddLogMessage("PingHostAsync", "Ping operation canceled.", DateTime.Now, 0, null, Errors.Ok);
                return null;
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("PingHostAsync", $"Error pinging host {host}: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public async Task<bool> IsPortOpenAsync(string host, int port, int timeout = 3000, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (TcpClient tcpClient = new TcpClient())
                {
                    var connectTask = tcpClient.ConnectAsync(host, port);
                    if (await Task.WhenAny(connectTask, Task.Delay(timeout, cancellationToken)) == connectTask)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return tcpClient.Connected;
                    }
                    else
                    {
                        _dmeEditor?.AddLogMessage("IsPortOpenAsync", $"Connection to {host}:{port} timed out.", DateTime.Now, 0, null, Errors.Failed);
                        return false;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _dmeEditor?.AddLogMessage("IsPortOpenAsync", "Port check canceled.", DateTime.Now, 0, null, Errors.Ok);
                return false;
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("IsPortOpenAsync", $"Error checking port {port} on {host}: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        public async Task<string> HttpGetAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                string content = await response.Content.ReadAsStringAsync();
                cancellationToken.ThrowIfCancellationRequested();
                return content;
            }
            catch (OperationCanceledException)
            {
                _dmeEditor?.AddLogMessage("HttpGetAsync", "HTTP GET operation canceled.", DateTime.Now, 0, null, Errors.Ok);
                return null;
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("HttpGetAsync", $"HTTP GET error for {url}: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public async Task<string> HttpPostAsync(string url, HttpContent content, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                HttpResponseMessage response = await _httpClient.PostAsync(url, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                cancellationToken.ThrowIfCancellationRequested();
                return result;
            }
            catch (OperationCanceledException)
            {
                _dmeEditor?.AddLogMessage("HttpPostAsync", "HTTP POST operation canceled.", DateTime.Now, 0, null, Errors.Ok);
                return null;
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("HttpPostAsync", $"HTTP POST error for {url}: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public List<IPAddress> ResolveHostName(string host)
        {
            try
            {
                return Dns.GetHostAddresses(host).ToList();
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("ResolveHostName", $"Error resolving host {host}: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return new List<IPAddress>();
            }
        }

        public List<IPAddress> GetLocalIPv4Addresses()
        {
            List<IPAddress> ipList = new List<IPAddress>();
            try
            {
                foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        ipList.Add(ip);
                }
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("GetLocalIPv4Addresses", $"Error retrieving local IPv4 addresses: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
            return ipList;
        }

        #endregion

        #region Additional Network Utilities

        public async Task<string> TcpSendReceiveAsync(string host, int port, string message, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(host, port);
                    cancellationToken.ThrowIfCancellationRequested();

                    NetworkStream stream = client.GetStream();

                    // Send the message
                    byte[] sendBuffer = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(sendBuffer, 0, sendBuffer.Length, cancellationToken);

                    // Read the response
                    byte[] receiveBuffer = new byte[4096];
                    int bytesRead = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length, cancellationToken);
                    return Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead);
                }
            }
            catch (OperationCanceledException)
            {
                _dmeEditor?.AddLogMessage("TcpSendReceiveAsync", "TCP send/receive canceled.", DateTime.Now, 0, null, Errors.Ok);
                return null;
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("TcpSendReceiveAsync", $"Error in TCP send/receive: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public async Task<string> UdpSendReceiveAsync(string host, int port, string message, CancellationToken cancellationToken = default)
        {
            try
            {
                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.Connect(host, port);
                    byte[] sendBytes = Encoding.UTF8.GetBytes(message);
                    await udpClient.SendAsync(sendBytes, sendBytes.Length);

                    var receiveTask = udpClient.ReceiveAsync();
                    if (await Task.WhenAny(receiveTask, Task.Delay(3000, cancellationToken)) == receiveTask)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        UdpReceiveResult result = receiveTask.Result;
                        return Encoding.UTF8.GetString(result.Buffer);
                    }
                    else
                    {
                        _dmeEditor?.AddLogMessage("UdpSendReceiveAsync", "UDP receive timed out.", DateTime.Now, 0, null, Errors.Failed);
                        return null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _dmeEditor?.AddLogMessage("UdpSendReceiveAsync", "UDP send/receive canceled.", DateTime.Now, 0, null, Errors.Ok);
                return null;
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("UdpSendReceiveAsync", $"Error in UDP send/receive: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return null;
            }
        }

        public async Task<List<int>> ScanPortsAsync(string host, int startPort, int endPort, CancellationToken cancellationToken = default)
        {
            List<int> openPorts = new List<int>();
            List<Task> tasks = new List<Task>();

            for (int port = startPort; port <= endPort; port++)
            {
                int currentPort = port;
                tasks.Add(Task.Run(async () =>
                {
                    if (await IsPortOpenAsync(host, currentPort, 1000, cancellationToken))
                    {
                        lock (openPorts)
                        {
                            openPorts.Add(currentPort);
                        }
                    }
                }, cancellationToken));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                _dmeEditor?.AddLogMessage("ScanPortsAsync", "Port scanning canceled.", DateTime.Now, 0, null, Errors.Ok);
            }
            return openPorts;
        }

        public async Task<bool> DownloadFileAsync(string url, string destinationPath, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {
            try
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    long? totalBytes = response.Content.Headers.ContentLength;
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                    {
                        byte[] buffer = new byte[81920];
                        long totalRead = 0;
                        int bytesRead;
                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                            totalRead += bytesRead;
                            progress?.Report(totalRead);
                        }
                    }
                }
                return true;
            }
            catch (OperationCanceledException)
            {
                _dmeEditor?.AddLogMessage("DownloadFileAsync", "File download canceled.", DateTime.Now, 0, null, Errors.Ok);
                return false;
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("DownloadFileAsync", $"Error downloading file: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
                return false;
            }
        }

        public async Task<List<TracerouteHop>> TracerouteAsync(string host, int maxHops = 30, CancellationToken cancellationToken = default)
        {
            List<TracerouteHop> hops = new List<TracerouteHop>();
            try
            {
                using (Ping ping = new Ping())
                {
                    for (int ttl = 1; ttl <= maxHops; ttl++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        PingOptions options = new PingOptions(ttl, true);
                        byte[] buffer = Encoding.ASCII.GetBytes("Traceroute");
                        int timeout = 3000;
                        PingReply reply = await ping.SendPingAsync(host, timeout, buffer, options);
                        cancellationToken.ThrowIfCancellationRequested();

                        hops.Add(new TracerouteHop
                        {
                            Hop = ttl,
                            Address = reply.Address?.ToString(),
                            RoundtripTime = reply.RoundtripTime,
                            Status = reply.Status.ToString()
                        });

                        if (reply.Status == IPStatus.Success)
                        {
                            break;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _dmeEditor?.AddLogMessage("TracerouteAsync", "Traceroute operation canceled.", DateTime.Now, 0, null, Errors.Ok);
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("TracerouteAsync", $"Error performing traceroute: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
            return hops;
        }

        public class TracerouteHop
        {
            public int Hop { get; set; }
            public string Address { get; set; }
            public long RoundtripTime { get; set; }
            public string Status { get; set; }
        }

        public List<NetworkInterfaceInfo> GetNetworkInterfaceInfo()
        {
            List<NetworkInterfaceInfo> interfaces = new List<NetworkInterfaceInfo>();
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        var ipProps = ni.GetIPProperties();
                        var ipv4Addresses = ipProps.UnicastAddresses
                            .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
                            .Select(addr => addr.Address.ToString())
                            .ToList();

                        interfaces.Add(new NetworkInterfaceInfo
                        {
                            Name = ni.Name,
                            Description = ni.Description,
                            IPv4Addresses = ipv4Addresses
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _dmeEditor?.AddLogMessage("GetNetworkInterfaceInfo", $"Error retrieving network interfaces: {ex.Message}", DateTime.Now, 0, null, Errors.Failed);
            }
            return interfaces;
        }

        public class NetworkInterfaceInfo
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public List<string> IPv4Addresses { get; set; }
        }

        #endregion
    }
}
