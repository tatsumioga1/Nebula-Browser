using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using Nebula.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using WinRT.Interop;

namespace Nebula
{
    public sealed partial class BrowserWindow : Window
    {
        public string CurrentUrl { get; private set; }

        private readonly string HomeUrl;

        private bool IsFullscreen = false;

        private bool DockVisible = true;

        private readonly DispatcherTimer HideTimer;

        public BrowserWindow(string url)
        {
            InitializeComponent();

            IntPtr hwnd =
                WindowNative.GetWindowHandle(this);

            WindowId windowId =
                Microsoft.UI.Win32Interop
                    .GetWindowIdFromWindow(hwnd);

            AppWindow appWindow =
                AppWindow.GetFromWindowId(windowId);

            this.Title = "";

            appWindow.TitleBar.BackgroundColor =
                Windows.UI.Color.FromArgb(
                    255, 32, 32, 32);

            appWindow.TitleBar.ForegroundColor =
                Colors.White;

            appWindow.TitleBar.ButtonBackgroundColor =
                Windows.UI.Color.FromArgb(
                    255, 32, 32, 32);

            appWindow.TitleBar.ButtonForegroundColor =
                Colors.White;

            appWindow.TitleBar.ButtonHoverBackgroundColor =
                Windows.UI.Color.FromArgb(
                    255, 48, 48, 48);

            appWindow.TitleBar.ButtonHoverForegroundColor =
                Colors.White;

            this.Title = "";

            HomeUrl = url;
            CurrentUrl = url;

            BrowserView.Source = new Uri(url);

            WindowManager.OpenWindows.Add(this);

            Closed += BrowserWindow_Closed;

            BrowserView.NavigationCompleted +=
                BrowserView_NavigationCompleted;

            BrowserView.CoreWebView2Initialized +=
                BrowserView_CoreWebView2Initialized;

            HideTimer = new DispatcherTimer();

            HideTimer.Interval =
                TimeSpan.FromSeconds(3);

            HideTimer.Tick += HideTimer_Tick;

            HideTimer.Start();
        }

        private void BrowserView_CoreWebView2Initialized(
            WebView2 sender,
            CoreWebView2InitializedEventArgs args)
        {
            BrowserView.CoreWebView2.DocumentTitleChanged +=
                CoreWebView2_DocumentTitleChanged;

            BrowserView.CoreWebView2.SourceChanged +=
                CoreWebView2_SourceChanged;

            BrowserView.CoreWebView2.NewWindowRequested +=
                CoreWebView2_NewWindowRequested;
        }

        private void CoreWebView2_NewWindowRequested(
            CoreWebView2 sender,
            CoreWebView2NewWindowRequestedEventArgs args)
        {
            args.Handled = true;

            DispatcherQueue.TryEnqueue(() =>
            {
                BrowserWindow window =
                    new BrowserWindow(args.Uri);

                window.Activate();
            });
        }

        private void CoreWebView2_DocumentTitleChanged(
            CoreWebView2 sender,
            object args)
        {
            string title =
                sender.DocumentTitle;

            if (!string.IsNullOrWhiteSpace(title))
            {
                this.Title =
                    $"Nebula — {title}";
            }
        }

        private void CoreWebView2_SourceChanged(
            CoreWebView2 sender,
            CoreWebView2SourceChangedEventArgs args)
        {
            CurrentUrl =
                sender.Source;
        }

        private void RootGrid_PointerMoved(
            object sender,
            PointerRoutedEventArgs e)
        {
            ShowDock();

            HideTimer.Stop();
            HideTimer.Start();
        }

        private void HideTimer_Tick(
            object? sender,
            object e)
        {
            HideTimer.Stop();

            HideDock();
        }

        private void ShowDock()
        {
            if (DockVisible)
                return;

            DockVisible = true;

            var fade = new DoubleAnimation
            {
                To = 1,
                Duration =
                    TimeSpan.FromMilliseconds(180)
            };

            var slide = new DoubleAnimation
            {
                To = 0,
                Duration =
                    TimeSpan.FromMilliseconds(180)
            };

            Storyboard fadeBoard = new();
            fadeBoard.Children.Add(fade);

            Storyboard.SetTarget(fade, Dock);
            Storyboard.SetTargetProperty(
                fade,
                "Opacity");

            fadeBoard.Begin();

            Storyboard slideBoard = new();
            slideBoard.Children.Add(slide);

            Storyboard.SetTarget(
                slide,
                DockTransform);

            Storyboard.SetTargetProperty(
                slide,
                "Y");

            slideBoard.Begin();

        }

        private void HideDock()
        {
            if (!DockVisible)
                return;

            if (Omnibox.Visibility ==
                Visibility.Visible)
                return;

            DockVisible = false;

            var fade = new DoubleAnimation
            {
                To = 0,
                Duration =
                    TimeSpan.FromMilliseconds(180)
            };

            var slide = new DoubleAnimation
            {
                To = 40,
                Duration =
                    TimeSpan.FromMilliseconds(180)
            };

            Storyboard fadeBoard = new();
            fadeBoard.Children.Add(fade);

            Storyboard.SetTarget(fade, Dock);
            Storyboard.SetTargetProperty(
                fade,
                "Opacity");

            fadeBoard.Begin();

            Storyboard slideBoard = new();
            slideBoard.Children.Add(slide);

            Storyboard.SetTarget(
                slide,
                DockTransform);

            Storyboard.SetTargetProperty(
                slide,
                "Y");

            slideBoard.Begin();

        }

        private async void DisplayMode_Tapped(
            object sender,
            TappedRoutedEventArgs e)
        {
            DisplayMode.Visibility =
                Visibility.Collapsed;

            Omnibox.Visibility =
                Visibility.Visible;

            for (double width = 160;
                 width <= 500;
                 width += 20)
            {
                OmniboxBorder.Width = width;
                await Task.Delay(5);
            }

            OmniboxBorder.Width = 500;

            Omnibox.Text =
                BrowserView.CoreWebView2?.Source
                ?? CurrentUrl;

            Omnibox.Focus(
                FocusState.Programmatic);

            Omnibox.SelectAll();
        }

        private async void BrowserView_NavigationCompleted(
            WebView2 sender,
            CoreWebView2NavigationCompletedEventArgs args)
        {
            if (BrowserView.Source == null)
                return;

            CurrentUrl =
                BrowserView.Source.ToString();

            SiteTitle.Text =
                GetDisplayName(CurrentUrl);

            this.Title =
                $"Nebula — {GetDisplayName(CurrentUrl)}";

            try
            {
                string faviconUrl =
                    $"https://www.google.com/s2/favicons?domain={BrowserView.Source.Host}&sz=64";

                SiteIcon.Source =
                    new BitmapImage(
                        new Uri(faviconUrl));
            }
            catch
            {
                SiteIcon.Source = null;
            }
        }

        private string GetDisplayName(
            string url)
        {
            try
            {
                Uri uri = new(url);

                string host =
                    uri.Host.Replace("www.", "");

                string name =
                    host.Split('.')[0];

                return
                    char.ToUpper(name[0]) +
                    name.Substring(1);
            }
            catch
            {
                return url;
            }
        }

        private string ResolveInput(
            string input)
        {
            input = input.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return "";

            if (input.Contains("."))
            {
                if (!input.StartsWith("http"))
                {
                    return "https://" + input;
                }

                return input;
            }

            return
                "https://www.google.com/search?q=" +
                Uri.EscapeDataString(input);
        }

        private void Omnibox_GotFocus(
            object sender,
            RoutedEventArgs e)
        {
            ShowDock();
        }

        private async void Omnibox_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            Omnibox.Visibility =
                Visibility.Collapsed;

            DisplayMode.Visibility =
                Visibility.Visible;

            for (double width = 500;
                 width >= 160;
                 width -= 20)
            {
                OmniboxBorder.Width = width;
                await Task.Delay(5);
            }

            OmniboxBorder.Width = 160;

            HideTimer.Stop();
            HideTimer.Start();
        }

        private async void Omnibox_KeyDown(
            object sender,
            KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter)
                return;

            string url =
                ResolveInput(Omnibox.Text);

            if (string.IsNullOrWhiteSpace(url))
                return;

            CurrentUrl = url;

            BrowserView.Source =
                new Uri(url);

            Omnibox.Visibility =
                Visibility.Collapsed;

            DisplayMode.Visibility =
                Visibility.Visible;

            for (double width = 500;
                 width >= 160;
                 width -= 20)
            {
                OmniboxBorder.Width = width;
                await Task.Delay(5);
            }

            OmniboxBorder.Width = 160;
        }

        private void Back_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (BrowserView.CanGoBack)
                BrowserView.GoBack();
        }

        private void Forward_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (BrowserView.CanGoForward)
                BrowserView.GoForward();
        }

        private void Refresh_Click(
            object sender,
            RoutedEventArgs e)
        {
            BrowserView.Reload();
        }

        private void Home_Click(
            object sender,
            RoutedEventArgs e)
        {
            BrowserView.Source =
                new Uri(HomeUrl);
        }

        private void CopyUrl_Click(
            object sender,
            RoutedEventArgs e)
        {
            DataPackage package = new();

            package.SetText(CurrentUrl);

            Clipboard.SetContent(package);
        }

        private void Fullscreen_Click(
            object sender,
            RoutedEventArgs e)
        {
            IsFullscreen =
                !IsFullscreen;
        }

        private void BrowserWindow_Closed(
            object sender,
            WindowEventArgs args)
        {
            HideTimer.Stop();

            WindowManager.OpenWindows.Remove(this);
        }
    }
}