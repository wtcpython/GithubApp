using LiveChartsCore.SkiaSharpView;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        public RoutedEventHandler CopyToClip { get; set; }
    }

    public sealed partial class CodePage : Page
    {
        public GitClient client;

        public IReadOnlyList<Octokit.Branch> branches;
        public IReadOnlyList<Octokit.RepositoryTag> tags;
        public IReadOnlyList<Octokit.Release> releases;
        public IReadOnlyList<Octokit.RepositoryLanguage> languages;
        public Octokit.Readme readmeContent;
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
            await WebView.EnsureCoreWebView2Async();

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

            // https URL
            httpsUri.Text = repo.CloneUrl;
            httpsButton.Click += (sender, e) =>
            {
                DataPackage package = new();
                package.SetText(repo.CloneUrl);
                Clipboard.SetContent(package);
            };

            // SSH URL
            sshUri.Text = repo.SshUrl;
            sshButton.Click += (sender, e) =>
            {
                DataPackage package = new();
                package.SetText(repo.SshUrl);
                Clipboard.SetContent(package);
            };

            // Github Cli URL
            cliUri.Text = $"gh repo clone {repo.Owner.Login} / {repo.Name}";
            cliButton.Click += (sender, e) =>
            {
                DataPackage package = new();
                package.SetText(cliUri.Text);
                Clipboard.SetContent(package);
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

            // 仓库介绍
            readmeContent = await client.GetReadmeAsync();
            string readMeContent = await client.GetReadmeHtmlAsync();
            WebView.NavigateToString(readMeContent);

            // 仓库其他信息
            licenseButton.Content = repo.License.Name;
            starButton.Content = $"{Utils.FormatNumber(repo.StargazersCount)} stars";
            watchingButton.Content = $"{Utils.FormatNumber(repo.SubscribersCount)} watching";
            forkButton.Content = $"{Utils.FormatNumber(repo.ForksCount)} forks";

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

            List<Octokit.RepositoryContributor> contributors = [.. await client.GetAllContributorsAsync()];

            foreach (var contributor in contributors[..14])
            {
                HyperlinkButton button = new()
                {
                    Content = new ImageIcon()
                    {
                        Source = new BitmapImage()
                        {
                            UriSource = new Uri(contributor.AvatarUrl),
                        },
                        Width = 32,
                        Height = 32,
                    }
                };
                HyperButtonBindingUri(button, contributor.HtmlUrl);
                contributorPanel.Children.Add(button);
            }
            

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
    }
} 