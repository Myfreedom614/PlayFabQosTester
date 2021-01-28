using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace PlayFabQosTester
{
    /// <summary>
    /// Used to create and run the root command for VmAgent.  
    /// </summary>
    public static class RootCommandConfiguration
    {
        public static RootCommand GenerateCommand(Func<string, string, bool, bool, Task> onInvoke)
        {
            var rootCommand = new RootCommand()
            {
                new Option("--titleId",
                    "Your PlayFab titleId (Hex)")
                {
                    Argument = new Argument<string>(),
                    Required = true
                },
                new Option("--playerId",
                    "Optional player id, if not specified a GUID will be used")
                {
                    Argument = new Argument<string>(defaultValue: () => Guid.NewGuid().ToString())
                },
                new Option("--listqosfortitle",
                    "Use ListQosServersForTitle instead")
                {
                    Argument = new Argument<bool>(),
                    Required = false
                },
                new Option("--verbose",
                    "When passed, print verbose results")
                {
                    Argument = new Argument<bool>(),
                    Required = false
                },
            };

            rootCommand.Handler = CommandHandler.Create(onInvoke);
            
            return rootCommand;
        }
    }
}