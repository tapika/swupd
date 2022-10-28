using Cake.Frosting;

namespace cakebuild.commands
{
    [TaskName(nameof(all))]
    [IsDependentOn(typeof(pushexe))]
    [IsDependentOn(typeof(choco_pack))]
    public class all : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
        }
    }

}

