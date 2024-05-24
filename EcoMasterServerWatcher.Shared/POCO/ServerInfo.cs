using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EcoMasterServerWatcher.Shared.POCO
{
    public class ServerInfo : INotifyPropertyChanged, IDisposable
    {
        private static readonly Regex _tagsRemoverRegex = new("<.*?>", RegexOptions.Compiled);

        public required string Id { get; set; }
        public required bool External { get; set; }
        public required int GamePort { get; set; }
        public required int WebPort { get; set; }
        public required bool IsLAN { get; set; }
        public required string Description { get; set; }
        public required string DetailedDescription { get; set; }
        public required int Category { get; set; }
        public required int OnlinePlayers { get; set; }
        public required int TotalPlayers { get; set; }
        public List<string>? OnlinePlayersNames { get; set; }
        public required bool AdminOnline { get; set; }
        public required double TimeSinceStart { get; set; }
        public required double TimeLeft { get; set; }
        public required int Animals { get; set; }
        public required int Plants { get; set; }
        public required int Laws { get; set; }
        public required string WorldSize { get; set; }
        public required string Version { get; set; }
        public required string EconomyDesc { get; set; }
        public required int SkillSpecializationSetting { get; set; }
        public required string Language { get; set; }
        public required bool HasPassword { get; set; }
        public required bool HasMeteor { get; set; }
        public string? DistributionStationItems { get; set; }
        public string? Playtimes { get; set; }
        public string? DiscordAddress { get; set; }
        public bool? IsPaused { get; set; }
        public int? ActiveAndOnlinePlayers { get; set; }
        public int? PeakActivePlayers { get; set; }
        public int? MaxActivePlayers { get; set; }
        public float? ShelfLifeMultiplier { get; set; }
        public float? ExhaustionAfterHours { get; set; }
        public bool? ExhaustionActive { get; set; }
        public bool? ExhaustionAllowPlaytimeSaving { get; set; }
        public int? ExhaustionMaxSavedHours { get; set; }
        public float? ExhaustionBonusHoursOnExhaustionEnabled { get; set; }
        public bool? ExhaustionBonusRetroactiveHoursAfterStart { get; set; }
        public Dictionary<string, int>? ExhaustionHoursGainPerWeekday { get; set; }
        public Dictionary<string, string>? ServerAchievementsDict { get; set; }
        public string? RelayAddress { get; set; }
        public string? Access { get; set; }
        public string? JoinUrl { get; set; }
        public required string InternalEndpoint { get; set; }
        public required string Address { get; set; }

        public string ClearDescription => RemoteTags(Description);
        public string ClearDetailedDescription => RemoteTags(DetailedDescription).Replace(@"\n", "\n");
        public string ShortLanguage => GetShortLanguage();

        public string OpTp => $"{OnlinePlayers}/{TotalPlayers}";
        public int CurrentDay => TimeSinceStart >= 315360000 ? -1 : TimeSpan.FromSeconds(TimeSinceStart).Days;
        public int DaysLeft => TimeLeft >= 315360000 || TimeLeft < 0 ? -1 : TimeSpan.FromSeconds(TimeLeft).Days;
        public int BuildId
        {
            get
            {
                var versionBuildString = new string(Version.TakeLast(3).Where(char.IsDigit).ToArray());
                if (int.TryParse(versionBuildString, out var value))
                    return value;
                return -1;
            }
        }

        public string SafeAccess => Access ?? "Unknown";

        public event PropertyChangedEventHandler? PropertyChanged;

        private static Dictionary<string, Tuple<MethodInfo, MethodInfo>> UpdatableProperties = 
            typeof(ServerInfo)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.CanWrite)
            .ToDictionary(x => x.Name, x => Tuple.Create(x.GetGetMethod()!, x.GetSetMethod()!));

        public ServerInfo()
        {
            RegisterUpdateWatcher();
        }

        private void RegisterUpdateWatcher()
        {
            this.PropertyChanged += OnPropertyUpdated;
        }

        private void UnregisterUpdateWatcher()
        {
            this.PropertyChanged -= OnPropertyUpdated;
        }

        private void OnPropertyUpdated(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Description))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClearDescription)));
            else if (e.PropertyName == nameof(DetailedDescription))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClearDetailedDescription)));
            else if (e.PropertyName == nameof(OnlinePlayers) || e.PropertyName == nameof(TotalPlayers))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OpTp)));
            else if (e.PropertyName == nameof(TimeSinceStart))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentDay)));
            else if (e.PropertyName == nameof(TimeLeft))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DaysLeft)));
            else if (e.PropertyName == nameof(Access))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SafeAccess)));
            else if (e.PropertyName == nameof(Version))
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BuildId)));
        }

        public bool UpdateData(ServerInfo fetchedServer)
        {
            var updated = false;
            foreach (var prop in UpdatableProperties)
            {
                var currentValue = prop.Value.Item1.Invoke(this, null)!;
                var newValue = prop.Value.Item1.Invoke(fetchedServer, null)!;

                if (newValue is List<object> newValueList && !((List<object>)currentValue).SequenceEqual(newValueList))
                {
                    updated = true;
                    prop.Value.Item2.Invoke(this, [newValue]);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop.Key));
                }
                else if (newValue is Dictionary<object, object> newValueDict && (((Dictionary<object, object>)currentValue).Count != newValueDict.Count || ((Dictionary<object, object>)currentValue).Except(newValueDict).Any()))
                {
                    updated = true;
                    prop.Value.Item2.Invoke(this, [newValue]);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop.Key));
                }
                else if (currentValue != newValue)
                {
                    updated = true;
                    prop.Value.Item2.Invoke(this, [newValue]);

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop.Key));
                }
            }

            return updated;
        }

        private static string RemoteTags(string text) => _tagsRemoverRegex.Replace(text ?? "", string.Empty);

        private string GetShortLanguage() => Language == "SimplifedChinese" ? "SimplifedChinese" : Language == "BrazilianPortuguese" ? "Portuguese" : Language;

        public override bool Equals(object? obj)
        {
            return obj is ServerInfo info &&
                   Id == info.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(ServerInfo? left, ServerInfo? right)
        {
            return EqualityComparer<ServerInfo>.Default.Equals(left, right);
        }

        public static bool operator !=(ServerInfo? left, ServerInfo? right)
        {
            return !(left == right);
        }

        public void Dispose()
        {
            UnregisterUpdateWatcher();
            GC.SuppressFinalize(this);
        }
    }
}
