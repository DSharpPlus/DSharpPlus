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

        
    }
    
}
