// This file is part of the DSharpPlus project.
//
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2023 DSharpPlus Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DSharpPlus.Test;

internal sealed class Program
{
    public static CancellationTokenSource CancelTokenSource { get; } = new();
    private static CancellationToken CancelToken => CancelTokenSource.Token;
    private static List<TestBot> Shards { get; } = new();

    public static void Main()
        => MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();

    public static async Task MainAsync()
    {
        Console.CancelKeyPress += Console_CancelKeyPress;
        TestBotConfig? cfg = new();
        string json = string.Empty;
        if (!File.Exists("config.json"))
        {
            json = JsonConvert.SerializeObject(cfg);
            File.WriteAllText("config.json", json, new UTF8Encoding(false));
            Console.WriteLine("Config file was not found, a new one was generated. Fill it with proper values and rerun this program");
            Console.ReadKey();

            return;
        }

        json = File.ReadAllText("config.json", new UTF8Encoding(false));
        cfg = JsonConvert.DeserializeObject<TestBotConfig>(json);

        List<Task> tskl = new();
        for (int i = 0; i < cfg.ShardCount; i++)
        {
            TestBot bot = new(cfg, i);
            Shards.Add(bot);
            tskl.Add(bot.RunAsync());
            await Task.Delay(7500).ConfigureAwait(false);
        }

        await Task.WhenAll(tskl).ConfigureAwait(false);

        try
        {
            await Task.Delay(-1, CancelToken).ConfigureAwait(false);
        }
        catch (Exception) { /* shush */ }
    }

    private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;

        foreach (TestBot shard in Shards)
        {
            shard.StopAsync().GetAwaiter().GetResult(); // it dun matter
        }

        CancelTokenSource.Cancel();
    }
}
