using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace DSharpPlus.GitHub
{
    public class DSharpPlusGithubClient
    {
        public GitHubClient ghclient;

        /// <summary>
        /// Initializes the GitHub client for you
        /// </summary>
        /// <param name="client">The name of the client you will need to use</param>

        public DSharpPlusGithubClient (string client = "default-dsharp-client")
        {
            ghclient = new GitHubClient(new ProductHeaderValue(client));
        }

        /// <summary>
        /// Lists all active issues of a repo
        /// </summary>
        /// <param name="owner">The owner of the repo</param>
        /// <param name="repo">The name of the repo</param>
        public async void GetActiveIssuesOfRepo(string owner, string repo)
        {
            var issuesForRepo = await ghclient.Issue.GetAllForRepository(owner, repo);
            var active = new IssueRequest
            {
                Filter = IssueFilter.All,
                State = ItemStateFilter.Open
            };
        }

    }
    
}
