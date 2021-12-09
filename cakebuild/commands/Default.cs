using Cake.Frosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace cakebuild.commands
{
    [TaskName(nameof(Default))]
    [IsDependentOn(typeof(all))]
    public class Default : FrostingTask<BuildContext>
    {
        public Default(Spectre.Console.Cli.IRemainingArguments args)
        {
        }

        public override void Run(BuildContext context)
        {
        }
    }
}
