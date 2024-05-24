using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData.Binding;
using EcoMasterServerWatcher.Shared.POCO;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoMasterServerWatcher.ViewModels
{
    public partial class ServerInfoViewModel : ReactiveObject, IDisposable
    {
        private IDisposable _serverInfoChangedSubscribe;
        private List<IDisposable>? _serverInfoPropSubscribes;

        [Reactive] public ServerInfo? ServerInfo { get; set; }
        public string ServerStats => GenerateServerStats();

        public ServerInfoViewModel()
        {
            _serverInfoChangedSubscribe = this.WhenAnyValue(x => x.ServerInfo).Subscribe(_ => OnServerInfoChanged());
        }

        public void OnServerInfoChanged()
        {
            DisposeSubscribes();
            if (ServerInfo != null)
            {
                _serverInfoPropSubscribes = [];
                var statsSubscriber = this.WhenAnyValue(
                    x => x.ServerInfo!.OnlinePlayers, x => x.ServerInfo!.TotalPlayers, x => x.ServerInfo!.AdminOnline,
                    x => x.ServerInfo!.Animals, x => x.ServerInfo!.Plants, x => x.ServerInfo!.EconomyDesc,
                    x => x.ServerInfo!.ActiveAndOnlinePlayers, x => x.ServerInfo!.PeakActivePlayers, x => x.ServerInfo!.MaxActivePlayers,
                    (_, _, _, _, _, _, _, _, _) => new object())
                    .Subscribe(_ => this.RaisePropertyChanged(nameof(ServerStats)));
                _serverInfoPropSubscribes.Add(statsSubscriber);
            }
        }

        public void NotifyUpdateAllInfo()
        {

        }

        public string GenerateServerStats()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"OnlinePlayers: {ServerInfo?.OnlinePlayers.ToString() ?? "-"}");
            sb.AppendLine($"TotalPlayers: {ServerInfo?.TotalPlayers.ToString() ?? "-"}");
            sb.AppendLine($"AdminOnline: {ServerInfo?.AdminOnline.ToString() ?? "-"}");
            sb.AppendLine($"Animals: {ServerInfo?.Animals.ToString() ?? "-"}");
            sb.AppendLine($"Plants: {ServerInfo?.Plants.ToString() ?? "-"}");
            sb.AppendLine($"EconomyDesc: {ServerInfo?.EconomyDesc.ToString() ?? "-"}");
            sb.AppendLine($"ActiveAndOnlinePlayers: {ServerInfo?.ActiveAndOnlinePlayers.ToString() ?? "-"}");
            sb.AppendLine($"PeakActivePlayers: {ServerInfo?.PeakActivePlayers.ToString() ?? "-"}");
            sb.AppendLine($"MaxActivePlayers: {ServerInfo?.MaxActivePlayers.ToString() ?? "-"}");
            return sb.ToString().Trim();
        }

        private void DisposeSubscribes() => _serverInfoPropSubscribes?.ForEach(x => x.Dispose());

        public void Dispose()
        {
            _serverInfoChangedSubscribe.Dispose();
            _serverInfoPropSubscribes?.ForEach(x => x.Dispose());
            GC.SuppressFinalize(this);
        }
    }
}
