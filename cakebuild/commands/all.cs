using Cake.Frosting;

namespace cakebuild.commands
{
    [TaskName(nameof(all))]
    public class all : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
        }
    }

}

