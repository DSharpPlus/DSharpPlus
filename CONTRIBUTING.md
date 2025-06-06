# Contributing to DSharpPlus

We're really happy to accept contributions. However we also ask that you follow several rules when doing so.

> [!NOTE]
> We do not merge pull requests for undocumented or in-PR Discord features. Before we merge support for any Discord feature, we require the relevant docs PR to be merged first.

## Proper base

When opening a PR, please make sure your branch targets the latest master branch, unless otherwise discussed. Also make sure your branch is even with the target branch, to avoid unnecessary surprises.

## Versioning

We generally attempt to follow [semantic versioning](https://semver.org/) when it comes to pushing stable releases. Ideally, this means you should only be creating PRs for `patch` and `minor` changes. If you wish to introduce a `major` (breaking) change, please discuss it beforehand so we can determine how to integrate it into our next major version. If this involves removing a public facing property/method, mark it with the `Obsolete` attribute instead on the latest release branch.

We may make exceptions to this rule to ease adoption of Discord features and/or changes. In particular, we allow minor versions to break existing code if the scope of such breaks is limited or the change is considered crucially important.

## Proper titles

When opening issues, make sure the title reflects the purpose of the issue or the pull request. Prefer past tense, and be brief. Further description belongs inside the issue or PR.

## New additions

When adding new features that do not correspond to API features, please attempt to add tests for them. Our tests follow a specific naming convention. If any changes are made to the `DSharpPlus.Commands` namespace, then the tests for those will be found in the `DSharpPlus.Tests.Commands` namespace and directory.

## Descriptive changes

We require the commits describe the change made. It can be a short description. If you fixed or resolved an open issue, please refer to it using Github's # links.

Examples of good commit messages:

* `Fixed a potential memory leak with cache entities. Fixes #142.`
* `Implemented new command extension. Resolves #169.`
* `Changed message cache behaviour. It's now global instead of per-channel.`
* `Fixed a potential NRE.`

* ```
  Changed message cache behaviour:

  - Messages are now stored globally.
  - Cache now deletes messages when they are deleted from discord.
  - Cache itself is now a ring buffer.
  ```

Examples of bad commit messages:

* `I a bad.`
* `Tit and tat.`
* `Fixed.`
* `Oops.`

## Code style

We use [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) throughout the repository, with a series of exceptions:

* Preference of `this`. While this one is not required, it's ill-advised to remove the existing instances thereof.
* In the same vein, we prefer you not use underscores for private fields in conjunction to using `this`. It is, again, ill-advised to go out of your way to change existing code.
* Do not use `var`. Always specify the type name.
* Do not use `ConfigureAwait(false)`. Other ConfigureAwait overloads may be warranted.
* Use the LINQ methods as opposed to the keyword constructs.
* Use file-scoped namespaces, and place using directives outside of them (ideally, order System.* directives first and separate groups of directives with empty lines).
* When working with async code, always await any tasks for the sake of good stack traces. For example:

  ```cs
  public async Task DoSomethingAsync()
  {
      await this.DoAnotherThingAsync();
  }

  public async Task DoAnotherThingAsync()
  {
      Console.WriteLine("42");
      await this.DoYetAnotherThingAsync(42);
  }

  public async Task DoYetAnotherThingAsync(int num)
  {
      if (num == 42)
          await SuperAwesomeMethodAsync();
  }
  ```

In addition to these, we also have several preferences:

* Use initializer syntax or collection expressions when possible:

  ```cs
  Class a = new()
  {
      StringNumber = "fourty-two",
      Number = 42
  };

  Dictionary<string, int> b = new()
  {
      ["fourty-two"] = 42,
      ["sixty-nine"] = 69
  };

  int[] c = [42, 69];
  ```

* Inline `out` declarations when possible: `SomeOutMethod(42, out string stringified);`
* Attempt to keep classes to a reasonable size, and consider partial types if a large implementation cannot be split apart.
* Members in classes should be ordered as follows (with few exceptions):
  * constants
  * private fields
  * constructors
  * public API
  * private methods
  * `Dispose()`, `DisposeAsync()`, `~TypeName()`

Use your own best judgement with regards to this ordering, and prefer intuitiveness over strict adherence.

## Code changes

One of our requirements is that all code change commits must build successfully. This is verified by our CI. When you open a pull request, Github will start an action which will perform a build and create PR artifacts. You can view its summary by visiting it from the checks section on
the PR overview page.

PRs that do not build will not be accepted.

Furthermore we require that methods you implement on Discord entities have a reflection in the Discord API, and that such entities must be documented in the currently live documentation (PR documentation does not count).

In the event your code change is a style change, XML doc change, or otherwise does not change how the code works, tag the commit with `[ci skip]`.

## Non-code changes

In the event you change something outside of code (i.e. a meta-change or documentation), you must tag your commit with `[ci skip]`.
