using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CommandsNext.Converters
{
    internal class HelpFormatterFactory
    {
        private ObjectFactory Factory { get; set; }

        public HelpFormatterFactory() { }

        public void SetFormatterType<T>() where T : BaseHelpFormatter
        {
            this.Factory = ActivatorUtilities.CreateFactory(typeof(T), new[] { typeof(CommandContext) });
        }

        public BaseHelpFormatter Create(CommandContext ctx)
        {
            return this.Factory(ctx.Services, new object[] { ctx }) as BaseHelpFormatter;
        }
    }
}
