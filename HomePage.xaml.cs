using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GithubApp
{
    public sealed partial class HomePage : Page
    {
        public static string searchText, owner, name, token;

        public static List<Octokit.Repository> SearchList, SelectedList, EmptyList;

        public static GitClient client = new("GithubApp");

        private static List<string> SortList = ["Most Stars", "Fewest Stars", "Most Forks", "Fewest Forks", "Recently Updated", "Least Recently Updated"];

        public HomePage()
        {
            this.InitializeComponent();
            this.Loaded += HomePage_Loaded;

            SortBox.ItemsSource = SortList;
            if (searchText != null)
            {
                Search();
            }
        }

        private async void HomePage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (token == null)
            {
                token = await ShowContentDialog();
                client.SetToken(token);
            }
        }

        public async void Search()
        {
            Octokit.SearchRepositoriesRequest searchRequest = new(searchText)
            {
                SortField = Octokit.RepoSearchSort.Stars,
                Order = Octokit.SortDirection.Descending
            };
            var searchResults = await client.Search.SearchRepo(searchRequest);

            var languages = searchResults.Items.Select(x => x.Language).Where(x => x != string.Empty && x != null).Distinct().Take(10).ToList();
            languagesView.ItemsSource = languages;

            SearchList = searchResults.Items.ToList();
            SelectedList = SearchList;

            SortBox.SelectedIndex = 0;
            SelectNumber.Text = $"{Utils.FormatNumber(searchResults.TotalCount)} results";
        }

        private void ButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var button = sender as HyperlinkButton;
            var content = (string)button.Content;
            owner = content.Split("/")[0];
            name = content.Split("/")[1];
            (App.Window as MainWindow).Navigate(typeof(CodePage));
        }

        private void LanguageSelectChanged(object sender, SelectionChangedEventArgs e)
        {
            var language = (string)languagesView.SelectedItem;
            SelectedList = SearchList.Where(x => x.Language == language).ToList();
            SortChanged(null, null);
            SearchResultView.ItemsSource = SelectedList;
            SelectNumber.Text = $"{Utils.FormatNumber(SelectedList.Count)} results";
        }

        private void SortChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = SortBox.SelectedIndex;
            SearchResultView.ItemsSource = EmptyList;
            if (index == 0)
            {
                SelectedList.Sort((s1, s2) => { return s2.StargazersCount.CompareTo(s1.StargazersCount); });
            }
            else if (index == 1)
            {
                SelectedList.Sort((s1, s2) => { return s1.StargazersCount.CompareTo(s2.StargazersCount); });
            }
            else if (index == 2)
            {
                SelectedList.Sort((s1, s2) => { return s2.ForksCount.CompareTo(s1.ForksCount); });
            }
            else if (index == 3)
            {
                SelectedList.Sort((s1, s2) => { return s1.ForksCount.CompareTo(s2.ForksCount); });
            }
            else if (index == 4)
            {
                SelectedList.Sort((s1, s2) => { return s2.UpdatedAt.LocalDateTime.CompareTo(s1.UpdatedAt.LocalDateTime); });
            }
            else if (index == 5)
            {
                SelectedList.Sort((s1, s2) => { return s1.UpdatedAt.LocalDateTime.CompareTo(s2.UpdatedAt.LocalDateTime); });
            }
            SearchResultView.ItemsSource = SelectedList;
        }

        public async Task<string> ShowContentDialog()
        {
            TextBox box = new();
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Title = "Please Input your Github Token",
                CloseButtonText = "OK",
                Content = box
            };

            var result = await dialog.ShowAsync();
            return box.Text;
        }
    }
}