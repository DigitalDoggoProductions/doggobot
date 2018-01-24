using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

using Discord;

using DoggoBot.Core.Models.Context;

namespace DoggoBot.Core.Services.Evaluation
{
    public class Evaluation
    {
        public static async Task<string> EvalutateAsync(DoggoCommandContext context, string script)
        {
            ScriptOptions scriptOps = ScriptOptions.Default.AddReferences(obtainAssemblies()).AddReferences(EvalImports.Imports);
            EvalGlobals scriptGlb = new EvalGlobals { Client = context.Client, Context = context };

            try { var finishedEval = await CSharpScript.EvaluateAsync(script, scriptOps, scriptGlb, typeof(EvalGlobals)); return finishedEval.ToString(); }
            catch (Exception ex) { return $"```csharp\n[{ex.Source}] {ex.Message}\n\nStack Trace: {ex.StackTrace}```"; }
        }

        private static IEnumerable<Assembly> obtainAssemblies()
        {
            AssemblyName[] slnAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();

            foreach (var a in slnAssemblies)
                yield return Assembly.Load(a);

            yield return Assembly.GetEntryAssembly();
            yield return typeof(ILookup<string, string>).GetTypeInfo().Assembly;
        }

        public class EvalGlobals
        {
            public IDiscordClient Client { get; set; }

            public DoggoCommandContext Context { get; set; }

            public IGuild Guild => Context.Guild;
            public IMessageChannel Channel => Context.Channel;
            public IUserMessage Message => Context.Message;
        }

        public class EvalImports
        {
            public static List<string> Imports { get; } = new List<string>
            {
                "System",
                "System.IO",
                "System.Linq",
                "System.Text",
                "System.Reflection",
                "System.Diagnostics",
                "System.Threading.Tasks",
                "System.Collections.Generic",
                //
                "Discord",
                "Discord.Audio",
                "Discord.Commands",
                "Discord.WebSocket",
                //
                "Newtonsoft.Json"
            };
        }
    }
}
