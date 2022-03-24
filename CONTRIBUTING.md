# Requirements to contribute
To ensure a quick and efficient workflow, we ask for the following **before** you open up the **P**ull **R**equest.
- Intermediate C# knowledge
	- We want quality PRs which truly understand what's being done, not just copied and pasted code found on [Stack Overflow](https://stackoverflow.com).
- True understanding of how `async`/`await` works
	- Discord requires our application to handle many events at once without blocking. To do this we must prevent threadpool exhaustion, deadlocks, infinite loops, and a large part of our codebase being synchronous.
- An editor or IDE that follows the `.editorconfig` rules defined.
	- This is the easiest and most painless way to enforce our code practices. If your editor/IDE does not have the capability to follow the `.editorconfig` rules, you can alternatively run `dotnet format` on the project **before** you commit.

# Prerequisites
If you know that your Pull Request will take a long time, but you don't want someone else to interfere, we recommend opening up a Draft Pull Request. If you know your PR will contain a ton of code changes, please discuss this with us beforehand on the [Discord](https://discord.gg/dsharpplus).

# PR Requirements
When opening up the pull request, please follow the conventions we have in place:
- True understanding of [SemVer](https://semver.org/)
	- We don't want PRs that involve breaking changes, containing major rewrites or minor documentation changes. Often this is required due to Discord's lack of consistency, so please avoid them whenever possible.
- Proper titles
	- Please make them short and sweet which precisely explain what the PR is about and which sections of D#+ are modified.
	- Good titles include:
		- “GetGuildAsync: Uncomment code leftover from testing”
		- “Use `X-Audit-Log-Header` when possible”
		- “Add GetThreadMemberAsync”
	- Titles that should be avoided:
		- “Cleanup DiscordGuild.cs and add position argument to channel creation methods” – Too long
		- “Reworked TimeSpanConverter” – Not specific enough
		- “Small typo fix” – This doesn't deserve its own PR, should be opened as an issue or pointed out to us on the Discord.
- Good descriptions
	- Please entail a list of changes you've intentionally made and list the reasons as to why you chose those specific choices. If there is context, prefix your list of changes with the paragraph of context.
- Proper base
	- When opening your PR, please ensure the commit it's based on is the latest commit. This resolves hidden complications that Git may have stored for us.
