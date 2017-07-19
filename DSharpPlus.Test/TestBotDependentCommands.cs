using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace DSharpPlus.Test
{
    public class TestBotDependentCommands
    {
        private string DependencyString { get; }
        private TestDependency Dependency { get; }

        public TestBotDependentCommands(string dep1, TestDependency dep2)
        {
            this.DependencyString = dep1;
            this.Dependency = dep2;
        }

        [Command("showdep"), Description("Displays a string from the dependency collection.")]
        public async Task ShowDep(CommandContext ctx)
        {
            await ctx.RespondAsync(Formatter.BlockCode(this.DependencyString));
        }

        [Command("showdep2"), Description("Displays a string from the dependency collection.")]
        public async Task ShowDep2(CommandContext ctx)
        {
            await ctx.RespondAsync(Formatter.BlockCode(this.Dependency.Dependency));
        }

        [Group("depnest")]
        public class TestBotNestedDependentCommands
        {
            private TestDependency Dependency { get; }

            public TestBotNestedDependentCommands(TestDependency dep1)
            {
                this.Dependency = dep1;
            }

            [Command("test")]
            public async Task NestTest1(CommandContext ctx)
            {
                await ctx.RespondAsync(Formatter.BlockCode(this.Dependency.Dependency));
            }
        }
    }

    public class TestDependency
    {
        public string Dependency { get; }

        public TestDependency(string depstr)
        {
            this.Dependency = depstr;
        }
    }
}
