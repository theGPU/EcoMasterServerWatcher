using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using EcoMasterServerWatcher.Shared;
using EcoMasterServerWatcher.Shared.POCO;
using EcoMasterServerWatcher.Utils;
using EcoMasterServerWatcher.ViewModels;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Linq;

namespace EcoMasterServerWatcher.Views;

public partial class MainView : UserControl, IDisposable
{
    private MainViewModel _model;
    private MainViewModel Model => _model ??= (MainViewModel)this.DataContext!;

    private IDisposable _screenOrientationSubscriber;

    private bool _isSideGridResizingInProcess = false;
    private double _pointerLastHorizontalPos = -1;

    public MainView()
    {
        InitializeComponent();
        ServersGrid.SelectionChanged += OnServerSelected;
        ViewResizeHandlerRectangle.PointerPressed += SideViewGrid_PointerPressed;
        ViewResizeHandlerRectangle.PointerReleased += SideViewGrid_PointerReleased;
        ViewResizeHandlerRectangle.PointerMoved += SideViewGrid_PointerMoved;
    }

    private void SetSideViewPanelSize(double size) => SideViewSplit.OpenPaneLength = Math.Clamp(size, 300, this.DesiredSize.Width);

    private void SideViewGrid_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isSideGridResizingInProcess)
        {
            if (_pointerLastHorizontalPos == -1)
            {
                _pointerLastHorizontalPos = e.GetPosition(null).X;
                return;
            } else
            {
                var newPos = e.GetPosition(null).X;
                var diff = _pointerLastHorizontalPos - e.GetPosition(null).X;
                _pointerLastHorizontalPos = newPos;
                SetSideViewPanelSize(SideViewSplit.OpenPaneLength + diff);

                if (Preferences.Data != null)
                    Preferences.Data.SideViewPanelLength = SideViewSplit.OpenPaneLength;
            }
        }
    }

    private void SideViewGrid_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
        _isSideGridResizingInProcess = true;
        _pointerLastHorizontalPos = -1;
    }

    private void SideViewGrid_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        e.Handled = true;
        _isSideGridResizingInProcess = false;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Model.Watcher = new EMSWatcher();
        Model.Watcher.OnException += OnWatcherException;
        Model.Watcher.RunUiThreadActionRequested += (s, e) => Dispatcher.UIThread.Post(e.Action);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _screenOrientationSubscriber = Model.WhenAnyValue(x => x.IsLandspaceMode).Subscribe(OnScreenOrientationChanged);
        Model.OnLoaded();

        if (Preferences.Data != null)
        {
            SearchTextBox.Text = Preferences.Data.SearchText;
            SearchCaseSensetiveCheckBox.IsChecked = Preferences.Data.SearchCaseSensetive;
            SetSideViewPanelSize(Preferences.Data.SideViewPanelLength);
        }

        Model.Watcher.Run(Model.Servers);
    }

    private void OnServerSelected(object? sender, SelectionChangedEventArgs e)
    {
        var selectedServer = ServersGrid.SelectedItem as ServerInfo;
        ServerInfoViewSide.SetServer(selectedServer);
        ServerInfoViewBottom.SetServer(selectedServer);
    }

    private void OnWatcherException(object? sender, ExceptionThrowedEventArgs e)
    {
        Console.WriteLine(e.Exception.Message);
        Console.WriteLine(e.Exception.StackTrace);
        Debugger.Break();
    }

    private void OnScreenOrientationChanged(bool isLandspace)
    {
        SideViewGrid.IsVisible = isLandspace;
        SideViewSplit.CompactPaneLength = isLandspace ? 40 : 0;
        Model.IsPaneOpen = Model.IsPaneOpen && !isLandspace ? false : Model.IsPaneOpen;
        BottomViewSplitter.IsVisible = !isLandspace;
        ServerInfoViewBottom.IsVisible = !isLandspace;
    }

    public void Dispose()
    {
        _screenOrientationSubscriber.Dispose();
    }
}
