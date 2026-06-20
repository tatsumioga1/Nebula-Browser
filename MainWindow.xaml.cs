using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Nebula
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenSite(string url)
        {
            BrowserWindow browser =
                new BrowserWindow(url);

            browser.Activate();
        }

        private void OpenCurrentUrl()
        {
            string url = UrlBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(url))
                return;

            if (!url.StartsWith("http"))
            {
                url = "https://" + url;
            }

            OpenSite(url);

            UrlBox.Text = string.Empty;
        }

        private void UrlBox_KeyDown(
            object sender,
            KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                OpenCurrentUrl();
            }
        }

        private void OpenWebsite_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenCurrentUrl();
        }

        private void YouTube_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenSite("https://youtube.com");
        }

        private void Netflix_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenSite("https://netflix.com");
        }

        private void ChatGPT_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenSite("https://chatgpt.com");
        }

        private void Spotify_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenSite("https://spotify.com");
        }
    }
}