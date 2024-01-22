using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GithubApp
{
    public class ReleaseInfo()
    {
        public string FormatTime { get; set; }
        public BitmapImage AuthorIcon { get; set; }
        public string AuthorName { get; set; }
        public string AuthorUri { get; set; }
        public string Tag { get; set; }
        public string TagUri { get; set; }

        public string Title { get; set; }
        public string Uri { get; set; }
        public List<AssetInfo> Assets { get; set; }
    }

    public class AssetInfo()
    {
        public string Name { get; set; }
        public string FormatSize { get; set; }
        public string FormatTime { get; set; }
        public string DownloadUri { get; set; }
    }

    public sealed partial class ReleasePage : Page
    {
        public ReleasePage()
        {
            this.InitializeComponent();
            LoadIssuesData();
        }

        public async void LoadIssuesData()
        {
            var client = HomePage.client;
            var releases = await client.GetReleasesAsync();

            List<ReleaseInfo> infos = releases.Select(x => new ReleaseInfo()
            {
                FormatTime = Utils.FormatTimeSpan((DateTime)(x.PublishedAt?.LocalDateTime)),
                AuthorIcon = new BitmapImage() { UriSource = new Uri(x.Author.AvatarUrl) },
                AuthorName = x.Author.Login,
                AuthorUri = x.Author.HtmlUrl,
                Tag = x.TagName,
                TagUri = x.HtmlUrl,

                Title = x.Name,
                Uri = x.HtmlUrl,
                Assets = x.Assets.Select(asset => new AssetInfo()
                {
                    Name = asset.Name,
                    FormatSize = Utils.FormatFileSize(asset.Size),
                    FormatTime = Utils.FormatTimeSpan(asset.CreatedAt.LocalDateTime),
                    DownloadUri = asset.BrowserDownloadUrl
                }).ToList()
            }).ToList();

            ReleaseView.ItemsSource = infos;
        }
    }
}