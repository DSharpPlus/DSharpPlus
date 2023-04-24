namespace DSharpPlus.CH.Internals
{
    public class MessageCommandFactory
    {
        private Dictionary<string, MessageCommandMethodData> _commands = new Dictionary<string, MessageCommandMethodData>();
        internal Microsoft.Extensions.DependencyInjection.ServiceProvider _services = null!;

        internal void AddCommand(string name, MessageCommandMethodData data)
        {
            _commands.Add(name, data);
        }

        public async Task ExecuteCommand(string name, DSharpPlus.Entities.DiscordMessage message, DiscordClient client, string[]? args)
        {
            var options = new Dictionary<string, object>();
            var arguments = new Queue<string>();
            if (args is not null)
            {
                bool parsingOptions = false;
                Tuple<string, object?>? tuple = null;
                foreach (var arg in args)
                {
                    if (arg.StartsWith("--"))
                    {
                        parsingOptions = true;
                        if (tuple is null) tuple = new Tuple<string, object?>(arg.Remove(0, 2), null);
                        else
                        {
                            if (tuple.Item2 is null)
                                options.Add(tuple.Item1, true);
                            else
                                options.Add(tuple.Item1, tuple.Item2);
                            tuple = new Tuple<string, object?>(arg.Remove(0, 2), null);
                        }
                    }
                    else if (arg.StartsWith('-'))
                    {
                        parsingOptions = true;
                        if (tuple is null) tuple = new Tuple<string, object?>(arg.Remove(0, 1), null);
                        else
                        {
                            if (tuple.Item2 is null)
                                options.Add(tuple.Item1, true);
                            else
                                options.Add(tuple.Item1, tuple.Item2);

                            tuple = new Tuple<string, object?>(arg.Remove(0, 1), null);
                        }
                    }
                    else
                    {
                        if (!parsingOptions) arguments.Enqueue(arg);
                        else if (tuple is null) throw new NotImplementedException();
                        else if (tuple.Item2 is null) tuple = new Tuple<string, object?>(tuple.Item1, arg);
                        else throw new NotImplementedException();
                    }
                }
                if (tuple is not null)
                {
                    if (tuple.Item2 is null) options.Add(tuple.Item1, true);
                    else options.Add(tuple.Item1, tuple.Item2);
                }
            }

            if (_commands.TryGetValue(name, out var value))
            {

                var handler = new MessageCommandHandler();
                await handler.BuildModuleAndExecuteCommand(value, _services, message, client, options, arguments);
            }
        }
    }
}