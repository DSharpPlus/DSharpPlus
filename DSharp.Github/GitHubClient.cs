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
        // Public Stuff
        /// <summary>
        /// The GitHub Client
        /// </summary>
        public GitHubClient ghclient;

        // Private Stuff
        bool isUnauthenticated;
        
        /// <summary>
        /// GitHub client (Octokit)
        /// </summary>
        /// <param name="unauthenticated">If true, use unauthClientName instead of user/pass</param>
        /// <param name="unauthClientName">The name of the unauthenticated client</param>
        /// <param name="user">The authenticated user's name, leave null if unauthenticated</param>
        /// <param name="pass">The authenticated user's password, leave null if unauthenticated</param>

        public DSharpPlusGithubClient (bool unauthenticated = true, string unauthClientName = "default-dsharp-client", string user = null, string pass = null)
        {
            isUnauthenticated = unauthenticated;
            if (isUnauthenticated)
            {
                ghclient = new GitHubClient(new ProductHeaderValue(unauthClientName));
            } else
            {
                var basicAuth = new Credentials(user, pass); // NOTE: not real credentials
            }
        }

        /// <summary>
        /// INDEV: Lists all active issues of a repo
        /// </summary>
        /// <param name="user">The owner of the repo</param>
        /// <param name="repo">The name of the repo</param>
        public async Task<string> GetActiveIssuesOfRepo(string user, string repo)
        {
            // var issuesForRepo = await ghclient.Issue.GetAllForRepository(owner, repo);
            var repositoryVar = await ghclient.Repository.Get(user, repo);

            string returnString = $"There are {repositoryVar.OpenIssuesCount.ToString()} active issues for repository {user}/{repo}";
            return returnString;

        }

        public async void GetLastCommitOfRepo(string user, string repo)
        {
            var repositoryVar = await ghclient.Repository.Get(user, repo);


        }

    }
    
}
