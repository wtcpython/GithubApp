using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Graphics;


namespace GithubApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            AppTitleBar.Loaded += (sender, e) => SetRegionsForCustomTitleBar();
            AppTitleBar.SizeChanged += (sender, e) => SetRegionsForCustomTitleBar();

            AppTitle.Text = Title = AppInfo.Current.DisplayInfo.DisplayName;

            this.SystemBackdrop = new MicaBackdrop();

            navigation.SelectedItem = navigation.MenuItems.OfType<NavigationViewItem>().First(); 
        }

        private void SetRegionsForCustomTitleBar()
        {
            double scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;

            GeneralTransform transform = SearchBox.TransformToVisual(null);
            Rect bounds = transform.TransformBounds(new Rect(0, 0,
                                                             SearchBox.ActualWidth,
                                                             SearchBox.ActualHeight));
            RectInt32 SearchBoxRect = GetRect(bounds, scaleAdjustment);


            var Rects = new RectInt32[] { SearchBoxRect };

            InputNonClientPointerSource nonClientInputSrc =
                InputNonClientPointerSource.GetForWindowId(this.AppWindow.Id);
            nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, Rects);
        }

        private static RectInt32 GetRect(Rect bounds, double scale)
        {
            return new RectInt32(
                _X: (int)Math.Round(bounds.X * scale),
                _Y: (int)Math.Round(bounds.Y * scale),
                _Width: (int)Math.Round(bounds.Width * scale),
                _Height: (int)Math.Round(bounds.Height * scale)
            );
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            string tag = (string)(args.SelectedItem as NavigationViewItem).Tag;
            if (tag == "HomePage") ContentFrame.Navigate(typeof(HomePage));
            else if (tag == "CodePage") ContentFrame.Navigate(typeof(CodePage));
            else if (tag == "MessagePage")
            {
                MessagePage.Status = (string)(args.SelectedItem as NavigationViewItem).Content;
                ContentFrame.Navigate(typeof(MessagePage));
            }
            else if (tag == "ReleasePage") ContentFrame.Navigate(typeof(ReleasePage));
        }

        private void StartSearch(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (SearchBox.Text != string.Empty)
                {
                    HomePage.searchText = SearchBox.Text;
                    ContentFrame.Navigate(typeof(HomePage));
                }
            }
        }

        public void Navigate(Type type)
        {
            ContentFrame.Navigate(type);
        }

        private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (ContentFrame.SourcePageType != null && ContentFrame.SourcePageType != typeof(MessagePage))
            {
                navigation.SelectedItem = navigation.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n =>
                    {
                        return (string)n.Tag == ContentFrame.SourcePageType.Name;
                    });
            }
        }
    }
}
