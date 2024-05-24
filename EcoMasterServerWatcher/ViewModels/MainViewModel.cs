using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Experimental;
using DynamicData.PLinq;
using EcoMasterServerWatcher.Shared;
using EcoMasterServerWatcher.Shared.POCO;
using EcoMasterServerWatcher.Utils;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace EcoMasterServerWatcher.ViewModels;

public partial class MainViewModel : ReactiveObject, IDisposable
{
    public EMSWatcher Watcher { get; set; }

    [Reactive] public bool IsPaneOpen { get; set; }
    [Reactive] public bool IsLandspaceMode { get; set; }

    public ObservableHashSet<ServerInfo> Servers { get; init; }

    private readonly SourceCache<ServerInfo, string> _sourceCache = new(x => x.Id);
    private readonly ReadOnlyObservableCollection<ServerInfo> _testViewModels;
    public ReadOnlyObservableCollection<ServerInfo> TestViewModels => _testViewModels;

    [Reactive] public string ServersFilter { get; set; }
    [Reactive] public long TotalPlayers { get; set; }
    [Reactive] public long FiltredPlayers { get; set; }

    private readonly IDisposable _sourseSubscriber;
    private readonly IDisposable? _searchFilterToPreferencesSubscriber;

    public MainViewModel()
    {
        Servers = [];
        Servers.CollectionChanged += Servers_CollectionChanged;

        Func<ServerInfo, bool> filterDelegate(string? text) => server =>
        {
            return string.IsNullOrEmpty(text) ||
            server.Description.Contains(text, StringComparison.InvariantCultureIgnoreCase) ||
            server.DetailedDescription.Contains(text, StringComparison.InvariantCultureIgnoreCase) ||
            server.Language.Contains(text, StringComparison.InvariantCultureIgnoreCase);
        };

        ServersFilter = Preferences.Data?.SearchText ?? "";

        var filterPredicate = this.WhenAnyValue(x => x.ServersFilter)
            .Throttle(TimeSpan.FromSeconds(.25), RxApp.MainThreadScheduler)
            .DistinctUntilChanged();

        if (Preferences.Data != null)
            _searchFilterToPreferencesSubscriber = this.WhenAnyValue(x => x.ServersFilter).Subscribe((_) => Preferences.Data!.SearchText = ServersFilter);

        _sourseSubscriber = _sourceCache.Connect()
            .Filter(filterPredicate.Select(filterDelegate))
            .Bind(out _testViewModels)
            .Subscribe();

        ((INotifyCollectionChanged)TestViewModels).CollectionChanged += TestViewModels_CollectionChanged;
    }

    public void OnLoaded()
    {
        if (!OperatingSystem.IsAndroid())
            IsLandspaceMode = Design.IsDesignMode ? true : GetMainView().DesiredSize.AspectRatio >= 1;

        if (Design.IsDesignMode)
            return;

        if (OperatingSystem.IsAndroid())
            GetMainView().SizeChanged += DetermineOrientation;
        else if (true)
            GetMainView().SizeChanged += DetermineOrientation;
    }

    private Control GetMainView()
    {
        if (Application.Current!.ApplicationLifetime is ISingleViewApplicationLifetime viewLifetime)
            return viewLifetime.MainView!;
        else if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime windowLifetime)
            return windowLifetime.MainWindow!;

        throw new NotSupportedException(nameof(IControlledApplicationLifetime));
    }

    private void Servers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            foreach (ServerInfo server in e.NewItems!)
                _sourceCache.AddOrUpdate(server);
        }

        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            foreach (ServerInfo server in e.OldItems!)
                _sourceCache.Remove(server);
        }

        TotalPlayers = Servers.Sum(x => x.OnlinePlayers);
    }

    private void TestViewModels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        FiltredPlayers = TestViewModels.Sum(x => x.OnlinePlayers);
    }

    private void DetermineOrientation(object? s, SizeChangedEventArgs e) => IsLandspaceMode = e.NewSize.AspectRatio >= 1;

    public void Dispose()
    {
        _sourseSubscriber.Dispose();
        _searchFilterToPreferencesSubscriber?.Dispose();

        Servers.CollectionChanged -= Servers_CollectionChanged;
        ((INotifyCollectionChanged)TestViewModels).CollectionChanged -= TestViewModels_CollectionChanged;

        GetMainView().SizeChanged -= DetermineOrientation;

        GC.SuppressFinalize(this);
    }
}
