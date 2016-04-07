using DiscordSharp;
using DiscordSharp.Commands;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luigibot.Modules
{
    /// <summary>
    /// Used for evaluating code.
    /// </summary>
    class EvalModules : IModule
    {
        private string[] EvalNamespaces = new string[]
        {
            "DiscordSharp",
            "System.Threading",
            "DiscordSharp.Objects",
            "System.Data.Linq",
            "System.Collections.Generic"
        };

        private string CustomLuaFunctions = @"--- Returns HEX representation of num
function num2hex(num)
    local hexstr = '0123456789abcdef'
    local s = ''
    while num > 0 do
        local mod = math.fmod(num, 16)
        s = string.sub(hexstr, mod+1, mod+1) .. s
        num = math.floor(num / 16)
    end
    if s == '' then s = '0' end
    return s
end

--- Returns HEX representation of str
function str2hex(str)
    local hex = ''
    while #str > 0 do
        local hb = num2hex(string.byte(str, 1, 1))
        if #hb < 2 then hb = '0' .. hb end
        hex = hex..hb
        str = string.sub(str, 2)
    end
    return hex
end";

        bool runningOnMono = false;
        public EvalModules()
        {
            Name = "eval";
            Description = "Contains commands for evaluating code.";
            runningOnMono = Type.GetType("Mono.Runtime") != null;
        }

        public override void Install(CommandsManager manager)
        {
            manager.AddCommand(new CommandStub("eval", "Evaluates real-time C# code. Be careful with this",
                "Evaluates C# code that is dynamically compiled.\n\nThe following namespaces are available for use:\n * DiscordSharp\n * System.Threading\n * DiscordSharp.Objects\n\n\nMake sure your function returns a string value.\nYou can reference the DiscordSharp client by using `discordClient`.", PermissionType.User, 1, e =>
                {
                    bool canExec = false;
                    if (manager.HasPermission(e.Author, PermissionType.Admin))
                        canExec = true;
                    if (!canExec)
                    {
                        e.Channel.SendMessage("kek");
                        return;
                    }

                    string whatToEval = e.Args[0];
                    if (whatToEval.StartsWith("`") && whatToEval.EndsWith("`"))
                        whatToEval = whatToEval.Trim('`');
                    try
                    {
                        var eval = EvalProvider.CreateEvalMethod<DiscordClient, string>(whatToEval, EvalNamespaces, new string[] { "DiscordSharp.dll", "System.Data.Linq.dll" });
                        string res = "";
                        Thread.Sleep(1000);
                        Thread executionThread = null;
                        Task evalTask = new Task(() =>
                        {
                            executionThread = Thread.CurrentThread;
                            if (eval != null)
                            {
                                try
                                {
                                    res = eval(manager.Client);
                                }
                                catch (Exception ex) { res = "Exception occurred while running: " + ex.Message; }
                            }
                            else
                            {
                                string errors = "Errors While Compiling: \n";
                                if (EvalProvider.errors != null)
                                {
                                    if (EvalProvider.errors.Count > 0)
                                    {
                                        foreach (var error in EvalProvider.errors)
                                        {
                                            errors += $"{error.ToString()}\n\n";
                                        }
                                    }
                                    e.Channel.SendMessage($"```\n{errors}\n```");
                                }
                                else
                                    e.Channel.SendMessage("Errors!");
                            }

                        });
                        evalTask.Start();
                        evalTask.Wait(10 * 1000);
                        if (!runningOnMono) //causes exceptions apparently >.>
                            if (executionThread != null)
                                executionThread.Abort();
                        if (res == null || res == "")
                            e.Channel.SendMessage("Terminated after 10 second timeout.");
                        else
                            e.Channel.SendMessage($"**Result**\n```\n{res}\n```");
                    }
                    catch (Exception ex)
                    {
                        string errors = "Errors While Compiling: \n";
                        if (EvalProvider.errors != null)
                        {
                            if (EvalProvider.errors.Count > 0)
                            {
                                foreach (var error in EvalProvider.errors)
                                {
                                    errors += $"{error.ToString()}\n\n";
                                }
                            }
                            else
                                errors += ex.Message;
                            e.Channel.SendMessage($"```\n{errors}\n```");
                        }
                        else
                            e.Channel.SendMessage($"Errors! {ex.Message}");
                    }
                }), this);

            manager.AddCommand(new CommandStub("lua", "Evals Lua code.", "WIP.", PermissionType.Admin, 1, cmdArgs =>
            {
                string whatToEval = cmdArgs.Args[0];
                if (whatToEval.StartsWith("`") && whatToEval.EndsWith("`"))
                {
                    whatToEval = whatToEval.Trim('`');
                    if (whatToEval.StartsWith("\n"))
                        whatToEval = whatToEval.Trim('\n');
                }

                Lua state = new Lua();

                bool isAdmin = false;
                if (manager.HasPermission(cmdArgs.Author, PermissionType.Admin))
                {
                    state["discordClient"] = manager.Client;
                    state.LoadCLRPackage();
                    string importStatements = "";
                    foreach (var use in EvalNamespaces)
                        importStatements += $"import('{use}')\n";
                    state.DoString(importStatements);
                    isAdmin = true;
                }
                else
                {
                    //state.DoString("import = function () end");
                }
                state.DoString(CustomLuaFunctions);

                string prefix = isAdmin ? $"{whatToEval}" : $"return run({whatToEval});";
                var res = state.DoString(prefix);
                string resultMessage = $"**Result: {res.Length}**\n```";
                foreach (var obj in res)
                {
                    resultMessage += $"\n{obj.ToString()}";
                }
                resultMessage += "\n```";

                if (res != null)
                    cmdArgs.Channel.SendMessage($"{resultMessage}");
                else
                    cmdArgs.Channel.SendMessage($"No result given.");
            }), this);
        }
    }
}
