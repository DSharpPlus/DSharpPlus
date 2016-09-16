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



    }
}
