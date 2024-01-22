using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace GithubApp
{
    public class MessageInfo()
    {
        public string Title { get; set; }
        public string Info { get; set; }
        public IReadOnlyList<Octokit.Label> Labels { get; set; }
    }
    public sealed partial class MessagePage : Page
    {
        public static string Status;
        public static GitClient client = HomePage.client;
        public static List<MessageInfo> infos, EmptyList;

        public MessagePage()
        {
            this.InitializeComponent();
            MessageType.Text = Status;
            if (Status == "Issues")
            {
                LoadIssuesData();
            }
            else
            {
                LoadPullRequestsData();
            }
        }

        public async void LoadIssuesData()
        {
            var issues = await client.GetIssuesAsync();

            infos = issues.Select(x => new MessageInfo()
            {
                Title = x.Title,
                Info = $"# {x.Number} opened {Utils.FormatTimeSpan(x.CreatedAt.LocalDateTime)} by {x.User.Login}",
                Labels = x.Labels
            }).ToList();

            MessageView.ItemsSource = infos;
            SearchBox.IsEnabled = true;
        }

        public async void LoadPullRequestsData()
        {
            var pulls = await client.GetPullRequestsAsync();

            infos = pulls.Select(x => new MessageInfo()
            {
                Title= x.Title,
                Info = $"# {x.Number} opened {Utils.FormatTimeSpan(x.CreatedAt.LocalDateTime)} by {x.User.Login}",
                Labels = x.Labels
            }).ToList();

            MessageView.ItemsSource= infos;
            SearchBox.IsEnabled = true;
        }

        private void SearchMessage(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchBox.IsEnabled = false;
                MessageView.ItemsSource = EmptyList;
                if (SearchBox.Text.Trim() != string.Empty)
                {
                    var result = infos.Where(info => info.Title.Contains(SearchBox.Text.Trim()));

                    MessageView.ItemsSource = result;
                }
                else
                {
                    MessageView.ItemsSource = infos;
                }
                SearchBox.IsEnabled = true;
            }
        }
    }
}