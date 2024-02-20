using LiveChartsCore.SkiaSharpView;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;


namespace GithubApp
{
    public class FileInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string Glyph { get; set; }
    }

    public class CloneUriInfo
    {
        public string Method { get; set; }
        public string Uri { get; set; }
    }

    public sealed partial class CodePage : Page
    {
        public GitClient client;

        public IReadOnlyList<Octokit.Branch> branches;
        public IReadOnlyList<Octokit.RepositoryTag> tags;
        public IReadOnlyList<Octokit.Release> releases;
        public IReadOnlyList<Octokit.RepositoryLanguage> languages;
        IReadOnlyList<Octokit.GitHubCommit> commits;
        public Octokit.GitHubCommit latestCommit;

        public CodePage()
        {
            this.InitializeComponent();
            client = HomePage.client;
            LoadRepoData();
        }

        private async void LoadRepoData()
        {
            Octokit.Repository repo = await client.SetRepoInfo(HomePage.owner, HomePage.name);

            branches = await client.GetBranchesAsync();

            // 仓库名称，公开状态
            repoImage.Source = new BitmapImage()
            {
                UriSource = new Uri(repo.Owner.AvatarUrl)
            };
            repoNameButton.Content = repo.Name;
            HyperButtonBindingUri(repoNameButton, repo.HtmlUrl);

            repoVisualStateBlock.Text = repo.Visibility.ToString();

            // 仓库观察 (Watch), 分叉 (Fork), 星星 (Star) 数量
            repoWatchButton.Content = $"Watch {Utils.FormatNumber(repo.SubscribersCount)}";
            repoForkButton.Content = $"Fork {Utils.FormatNumber(repo.ForksCount)}";
            repoStarButton.Content = $"Star {Utils.FormatNumber(repo.StargazersCount)}";

            latestCommit = await client.GetLatestCommitAsync();
            ShowLatestCommitInfo();

            ShowRepoFile();

            itemsView.ItemsSource = new List<CloneUriInfo>()
            {
                new()
                {
                    Method = "HTTPS",
                    Uri = repo.CloneUrl,
                },
                new()
                {
                    Method = "SSH",
                    Uri = repo.SshUrl
                },
                new()
                {
                    Method = "Github CLI",
                    Uri = $"gh repo clone {repo.Owner.Login} / {repo.Name}"
                }
            };

            repoDescription.Text = repo.Description;

            // Topics
            foreach (string topic in repo.Topics)
            {
                HyperlinkButton button = new()
                {
                    Content = topic,
                    CornerRadius = new CornerRadius(15),
                    Background = new SolidColorBrush(Colors.LightSkyBlue),
                    Margin = new Thickness(2)
                };
                HyperButtonBindingUri(button, $"https://github.com/topics/{topic}");
                topicPanel.Children.Add(button);
            }

            // 分支 (Branch)，标签 (Tag) 数量
            repoDefaultBranchButton.Content = repo.DefaultBranch;

            releases = await client.GetReleasesAsync();
            repoBranchButton.Content = $"{Utils.FormatNumber(branches.Count)} Branches";
            HyperButtonBindingUri(repoBranchButton, client.GetGithubBaseUri("branches"));

            tags = await client.GetRepoTagsAsync();
            repoTagButton.Content = $"{Utils.FormatNumber(tags.Count)} tags";
            HyperButtonBindingUri(repoTagButton, client.GetGithubBaseUri("tags"));

            // 仓库介绍 和仓库信息
            markdown.Text = (await client.GetReadmeAsync()).Content;

            Dictionary<string, string> repoInfoDict = new()
            {
                { "Readme", client.GetGithubBaseUri() + "#readme" },
                { "License", repo.License.Url },
                { "Activity", client.GetGithubBaseUri() + "/activity" },
                { $"{Utils.FormatNumber(repo.StargazersCount)} stars", client.GetGithubBaseUri() + "/stargazers" },
                { $"{Utils.FormatNumber(repo.SubscribersCount)} watching", client.GetGithubBaseUri() + "/watchers" },
                { $"{Utils.FormatNumber(repo.ForksCount)} forks", client.GetGithubBaseUri() + "/forks" }
            };
            repoInfoView.ItemsSource = repoInfoDict;

            ShowReleaseInfo();

            languages = await client.GetAllLanguagesAsync();

            long sum = languages.Sum(language => language.NumberOfBytes);

            List<PieSeries<double>> series = languages
                .Where(language => language.NumberOfBytes > sum * 0.001)
                .Select(language => new PieSeries<double>()
                {
                    Name = language.Name,
                    Values = [Math.Round((double)language.NumberOfBytes / sum, 3)],
                    ToolTipLabelFormatter = (value) => $"{value.Label} {value.Coordinate.PrimaryValue:P1}"
                })
                .ToList();

            chart.Series = series;

            contributorView.ItemsSource = (await client.GetAllContributorsAsync()).Take(14);

            // 长耗时
            // Commits 总数量
            commits = await client.GetGitHubCommitsAsync();
            repoCommitsButton.Content = $"{commits.Count} Commits";
        }

        public void ShowLatestCommitInfo()
        {
            // 最后一次提交信息
            latestCommitAuthor.Content = latestCommit.Author.Login;
            latestCommitMessage.Content = latestCommit.Commit.Message.Split("\n")[0];
            HyperButtonBindingUri(latestCommitMessage, latestCommit.HtmlUrl);

            latestCommitId.Content = latestCommit.Sha[0..7];
            HyperButtonBindingUri(latestCommitId, latestCommit.HtmlUrl);

            latestCommitTime.Text = Utils.FormatTimeSpan(latestCommit.Commit.Author.Date.LocalDateTime);
        }

        public async void ShowRepoFile()
        {
            List<Octokit.RepositoryContent> contents = [.. await client.GetAllContentsAsync()];
            contents.Sort((x, y) =>
            {
                int result = x.Type.StringValue.CompareTo(y.Type.StringValue);
                if (result != 0) return result;
                else return x.Name.CompareTo(y.Name);
            });

            List<FileInfo> fileInfos = contents.Select(content => new FileInfo()
                {
                    Name = content.Name,
                    Type = content.Type.StringValue,
                    Path = content.Path,
                    Glyph = content.Type.StringValue == "file" ? "\uE8B7" : "\uE8D5"
                }).ToList();

            FileListView.ItemsSource = fileInfos;
        }

        public void ShowReleaseInfo()
        {
            releaseButton.Content = $"Releases {releases.Count}";

            var latestRelease = releases.Where(release => !release.Prerelease).First();

            latestReleaseButton.Content = $"{latestRelease.Name}\n{Utils.FormatTimeSpan((DateTime)latestRelease.PublishedAt?.LocalDateTime)}";

            otherReleaseButton.Content = $"+ {releases.Count - 1} releases";
        }

        private void JumpToReleasePage(object sender, RoutedEventArgs e)
        {
            (App.Window as MainWindow).Navigate(typeof(ReleasePage));
        }

        private void HyperButtonBindingUri(HyperlinkButton button, string uri)
        {
            button.NavigateUri = new Uri(uri);
            ToolTipService.SetToolTip(button, uri);
        }

        private async void ContributorNaviate(object sender, ItemClickEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri((e.ClickedItem as Octokit.RepositoryContributor).HtmlUrl));
        }

        private void MarkdownImageResolving(object sender, CommunityToolkit.WinUI.UI.Controls.ImageResolvingEventArgs e)
        {
            e.Image = new BitmapImage(new Uri($"https://raw.githubusercontent.com/{client.owner}/{client.name}/main/" + e.Url));
            e.Handled = true;
        }

        private async void MarkdownLinkClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        private void CommandExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
        {
            foreach (CloneUriInfo info in itemsView.ItemsSource as List<CloneUriInfo>)
            {
                if (info.Uri == (string)args.Parameter)
                {
                    DataPackage package = new();
                    package.SetText(info.Uri);
                    Clipboard.SetContent(package);
                }
            }
        }
    }
} 
