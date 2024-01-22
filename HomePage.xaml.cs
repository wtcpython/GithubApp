using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace GithubApp
{
    public class SearchResult
    {
        public string RepoName { get; set; }
        public BitmapImage OwnerIcon { get; set; }
        public string Description { get; set; }
        public List<string> Topics { get; set; }
        public string Language { get; set; }
        public int Stars { get; set; }
        public string FormatStar { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string FormatUpdatedTime { get; set; }
        public int Forks { get; set; }
    }

    public sealed partial class HomePage : Page
    {
        public static string searchText, owner, name, token;

        public static List<SearchResult> SearchList, SelectedList, EmptyList;

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

            SearchList = searchResults.Items.Select(x => new SearchResult
            {
                RepoName = x.Owner.Login + "/" + x.Name,
                OwnerIcon = new BitmapImage(new Uri(x.Owner.AvatarUrl)),
                Description = x.Description,
                Topics = x.Topics.Take(5).ToList(),
                Language = x.Language,
                Stars = x.StargazersCount,
                FormatStar = $"Stars: {Utils.FormatNumber(x.StargazersCount)}",
                UpdatedTime = x.UpdatedAt.LocalDateTime,
                FormatUpdatedTime = $"Updated {Utils.FormatTimeSpan(x.UpdatedAt.LocalDateTime)}",
                Forks = x.ForksCount,
            }).ToList();
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
                SelectedList.Sort((s1, s2) => { return s2.Stars.CompareTo(s1.Stars); });
            }
            else if (index == 1)
            {
                SelectedList.Sort((s1, s2) => { return s1.Stars.CompareTo(s2.Stars); });
            }
            else if (index == 2)
            {
                SelectedList.Sort((s1, s2) => { return s2.Forks.CompareTo(s1.Forks); });
            }
            else if (index == 3)
            {
                SelectedList.Sort((s1, s2) => { return s1.Forks.CompareTo(s2.Forks); });
            }
            else if (index == 4)
            {
                SelectedList.Sort((s1, s2) => { return s2.UpdatedTime.CompareTo(s1.UpdatedTime); });
            }
            else if (index == 5)
            {
                SelectedList.Sort((s1, s2) => { return s1.UpdatedTime.CompareTo(s2.UpdatedTime); });
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