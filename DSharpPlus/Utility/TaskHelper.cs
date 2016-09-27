//Thanks to Discord.NET for this class.
//https://github.com/RogueException/Discord.Net/blob/a357a06d33c5631f5fb7181e7e231d960bf7a3f3/src/Discord.Net.Shared/TaskHelper.cs
//https://github.com/RogueException/Discord.Net/
using System;
using System.Threading.Tasks;

namespace DSharpPlus.Utility
{
    public static class TaskHelper
    {
        public static Task CompletedTask => Task.Delay(0);

        public static Func<Task> ToAsync(Action action)
        {
            return () =>
            {
                action(); return CompletedTask;
            };
        }

        public static Func<T, Task> ToAsync<T>(Action<T> action)
        {
            return x =>
            {
                action(x); return CompletedTask;
            };
        }
    }
}
