using DSharpPlus.GitHub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSharpPlus.Github.Modules
{
    class Issues
    {

        Octokit.GitHubClient ghClient;

        public Issues(Octokit.GitHubClient client)
        {
            ghClient = client;
        }


        public string GetActiveIssuesOfRepo(string user, string repo)
        {
            // var issuesForRepo = await ghclient.Issue.GetAllForRepository(owner, repo);
            var repositoryVar = ghClient.Repository.Get(user, repo);


            string returnString = $"There are {repositoryVar.Result.OpenIssuesCount.ToString()} active issues for repository {user}/{repo}";
            return returnString;

        }
    }
}
