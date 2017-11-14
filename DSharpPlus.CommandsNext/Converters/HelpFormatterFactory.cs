using System;
using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.CommandsNext.Converters
{
    internal class HelpFormatterFactory
    {
        private ObjectFactory Factory { get; set; }

        public HelpFormatterFactory() { }

        public void SetFormatterType<T>() where T : class, IHelpFormatter
        {
            this.Factory = ActivatorUtilities.CreateFactory(typeof(T), new Type[0]);
        }

        public IHelpFormatter Create(IServiceProvider services)
        {
            return this.Factory(services, null) as IHelpFormatter;
        }
    }
}
