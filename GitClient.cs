using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GithubApp
{
    public sealed partial class GitClient
    {
        private GitHubClient client;

        public string owner;

        public string name;

        public ISearchClient Search => client.Search;

        public GitClient(string productHeaderValue)
        {
            client = new GitHubClient(new ProductHeaderValue(productHeaderValue));
        }

        public void SetToken(string token)
        {
            Credentials credentials = new(token);
            client.Credentials = credentials;
        }

        public async Task<Repository> SetRepoInfo(string owner, string name)
        {
            this.owner = owner;
            this.name = name;
            return await client.Repository.Get(owner, name);
        }

        public async Task<IReadOnlyList<Branch>> GetBranchesAsync()
        {
            return await client.Repository.Branch.GetAll(owner, name);
        }

        public async Task<IReadOnlyList<GitHubCommit>> GetGitHubCommitsAsync()
        {
            return await client.Repository.Commit.GetAll(owner, name);
        }

        public async Task<GitHubCommit> GetLatestCommitAsync()
        {
            ApiOptions option = new()
            {
                PageCount = 1,
                PageSize = 1,
            };
            var _ = await client.Repository.Commit.GetAll(owner, name, option);
            return _[0];
        }

        public async Task<IReadOnlyList<RepositoryTag>> GetRepoTagsAsync()
        {
            return await client.Repository.GetAllTags(owner, name);
        }

        public async Task<Readme> GetReadmeAsync()
        {
            return await client.Repository.Content.GetReadme(owner, name);
        }

        public async Task<string> GetReadmeHtmlAsync()
        {
            return await client.Repository.Content.GetReadmeHtml(owner, name);
        }

        public async Task<IReadOnlyList<RepositoryContent>> GetAllContentsAsync()
        {
            return await client.Repository.Content.GetAllContents(owner, name);
        }

        public async Task<IReadOnlyList<RepositoryLanguage>> GetAllLanguagesAsync()
        {
            return await client.Repository.GetAllLanguages(owner, name);
        }

        public async Task<IReadOnlyList<Release>> GetReleasesAsync()
        {
            return await client.Repository.Release.GetAll(owner, name);
        }

        public async Task<IReadOnlyList<Issue>> GetIssuesAsync()
        {
            return await client.Issue.GetAllForRepository(owner, name);
        }

        public async Task<IReadOnlyList<PullRequest>> GetPullRequestsAsync()
        {
            return await client.PullRequest.GetAllForRepository(owner, name);
        }

        public async Task<IReadOnlyList<RepositoryContributor>> GetAllContributorsAsync()
        {
            return await client.Repository.GetAllContributors(owner, name);
        }

        public string GetGithubBaseUri(string detail = "")
        {
            return $"https://github.com/{owner}/{name}/{detail}";
        }
    }
}