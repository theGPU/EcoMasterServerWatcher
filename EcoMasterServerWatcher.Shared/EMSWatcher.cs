using EcoMasterServerWatcher.Shared.POCO;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EcoMasterServerWatcher.Shared
{
    public class ExceptionThrowedEventArgs(Exception exception) : EventArgs
    {
        public Exception Exception { get; private set; } = exception;
    }

    public class ServerInfoUpdatedEventArgs(string serverId) : EventArgs
    {
        public string ServerId { get; private set; } = serverId;
    }

    public class RunUiThreadActionRequestedEventArgs(Action action) : EventArgs
    {
        public Action Action { get; private set; } = action;
    }

    public class EMSWatcher : INotifyPropertyChanged
    {
        private static readonly JsonSerializerOptions _serializerOptions = new() { PropertyNameCaseInsensitive = true };
        private static readonly Uri _masterServerFetchUrl = new("https://masterserver.eco.strangeloopgames.com/api/v1/ServerListing");

        private CancellationTokenSource _cts = null!;
        private CancellationToken _mainCt;
        private HttpClient _httpClient = null!;

        public int UpdateInterval { get; set; } = 5000;
        public Task MainTask { get; set; } = null!;
        public Exception? Exception { get; set; }
        public event EventHandler<RunUiThreadActionRequestedEventArgs>? RunUiThreadActionRequested;

        private int _skippedServers = 0;
        public int SkippedServers { get => _skippedServers; set { if (value != _skippedServers) { _skippedServers = value; NotifyPropertyChanged(); } } }
        public ObservableHashSet<ServerInfo> ServerInfos { get; set; } = [];

        public event EventHandler<ExceptionThrowedEventArgs>? OnException;
        public event EventHandler<ServerInfoUpdatedEventArgs>? OnServerUpdated;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public EMSWatcher()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
                {
                    return true;
                }
            };

            _httpClient = new HttpClient(handler);
        }

        public Task Run(ObservableHashSet<ServerInfo>? serverHashset = null, CancellationToken ct = default)
        {
            if (serverHashset != null)
                ServerInfos = serverHashset;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _mainCt = _cts.Token;
            MainTask = Task.Run(Worker, _mainCt).ContinueWith((t) => { if (t.IsFaulted) { Exception = t.Exception; OnException?.Invoke(this, new(Exception)); }  }, _mainCt);
            return MainTask;
        }

        public Task Stop()
        {
            _cts.Cancel();
            return MainTask;
        }

        private async Task Worker()
        {
            while (!_mainCt.IsCancellationRequested)
            {
                try
                {
                    var servers = await FetchMasterServer();
                    RunUiThreadActionRequested?.Invoke(this, new(() => UpdateServerList(servers)));
                    await Task.Delay(UpdateInterval, _mainCt);
                }
                catch (OperationCanceledException) { }
                catch { throw; }
            }
        }

        public async Task<HashSet<ServerInfo>> FetchMasterServer()
        {
            var resp = await _httpClient.GetStringAsync(_masterServerFetchUrl, _mainCt);
            var serversUnparsed = JsonSerializer.Deserialize<HashSet<object>>(resp, _serializerOptions)!;
            var serversParsed = new HashSet<ServerInfo>();

            SkippedServers = 0;
            foreach (var server in serversUnparsed)
            {
                try
                {
                    serversParsed.Add(JsonSerializer.Deserialize<ServerInfo>(JsonSerializer.Serialize(server), _serializerOptions)!);
                } catch (Exception ex)
                {
                    SkippedServers++;
                    Trace.TraceError(ex.ToString());
                    //Debugger.Break();
                }
            }

            return serversParsed;
        }

        public void UpdateServerList(HashSet<ServerInfo> fetchedServers)
        {
            foreach (var server in ServerInfos)
                if (!fetchedServers.Contains(server))
                    ServerInfos.Remove(server);

            foreach (var server in fetchedServers)
                if (!ServerInfos.Contains(server))
                    ServerInfos.Add(server);

            foreach (var server in ServerInfos)
            {
                fetchedServers.TryGetValue(server, out var fetchedServer);
                if (server.UpdateData(fetchedServer!))
                    OnServerUpdated?.Invoke(this, new(server.Id));
            }
        }
    }
}
