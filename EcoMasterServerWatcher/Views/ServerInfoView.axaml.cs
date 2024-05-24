using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EcoMasterServerWatcher.Shared.POCO;
using EcoMasterServerWatcher.ViewModels;
using System;
using System.Diagnostics;

namespace EcoMasterServerWatcher.Views;

public partial class ServerInfoView : UserControl
{
    private ServerInfoViewModel _model;
    private ServerInfoViewModel Model => _model ??= (ServerInfoViewModel)this.DataContext!;

    public ServerInfoView()
    {
        this.DataContext = new ServerInfoViewModel();
        InitializeComponent();
    }

    public void SetServer(ServerInfo? server) => Model.ServerInfo = server;

    private void OpenWebButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            OpenUrl($"http://{Model.ServerInfo!.Address}:{Model.ServerInfo.WebPort}");
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
        }
    }

    private void DiscordButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            OpenUrl(Model.ServerInfo!.DiscordAddress!);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
        }
    }

    private void JoinButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            OpenUrl(Model.ServerInfo!.JoinUrl!);
        } catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
        }
    }

    private void OpenUrl(string url)
    {
        if (OperatingSystem.IsWindows())
        {
            using var proc = new Process { StartInfo = { UseShellExecute = true, FileName = url } };
            proc.Start();
        } 
        else if (OperatingSystem.IsAndroid())
        {

        } 
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", url);
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start("x-www-browser", url);
        }
        else if (OperatingSystem.IsBrowser())
        {
#if BROWSER

#endif
        }
    }
}