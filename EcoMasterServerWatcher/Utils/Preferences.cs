using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Text.Json;

namespace EcoMasterServerWatcher.Utils
{
    public class PreferencesData : ReactiveObject
    {
        [Reactive] public string SearchText { get; set; } = null!;
        [Reactive] public bool SearchCaseSensetive { get; set; } = false;
        [Reactive] public double SideViewPanelLength { get; set; } = 0;
    }

    public static class Preferences
    {
        const string DataFileName = "Preferences.json";

        public static PreferencesData? Data { get; private set; }
        private static IDisposable _dataSubscriber;

        static Preferences()
        {
            if (OperatingSystem.IsBrowser())
            {
                return;
            }
            else if (OperatingSystem.IsAndroid())
            {
                return;
            }
            else if (OperatingSystem.IsIOS())
            {
                return;
            }
            else if (OperatingSystem.IsWindows())
            {
                LoadData();
            }

            if (Data != null)
            {
                _dataSubscriber = Data.WhenAnyPropertyChanged().Throttle(TimeSpan.FromSeconds(.25), RxApp.TaskpoolScheduler).Subscribe((_) => SaveData());
            }
        }

        private static void LoadData()
        {
            try
            {
                Data = JsonSerializer.Deserialize<PreferencesData>(File.ReadAllText(DataFileName))!;
            } catch
            {
                Data = new PreferencesData();
                SaveData();
            }
        }

        private static void SaveData()
        {
            File.WriteAllText(DataFileName, JsonSerializer.Serialize(Data));
        }
    }
}
