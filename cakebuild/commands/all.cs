using Cake.Frosting;

namespace cakebuild.commands
{
    [TaskName(nameof(all))]
    [IsDependentOn(typeof(buildexe))]
    public class all : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
        }
    }

}

