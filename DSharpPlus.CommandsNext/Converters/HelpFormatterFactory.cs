using System;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CommandsNext.Converters
{
    internal class HelpFormatterFactory
    {
        private ObjectFactory Factory { get; set; }

        public HelpFormatterFactory() { }

        public void SetFormatterType<T>() where T : BaseHelpFormatter
        {
            this.Factory = ActivatorUtilities.CreateFactory(typeof(T), new[] { typeof(CommandsNextExtension) });
        }

        public BaseHelpFormatter Create(IServiceProvider services, CommandsNextExtension cnext)
        {
            return this.Factory(services, new object[] { cnext }) as BaseHelpFormatter;
        }
    }
}
