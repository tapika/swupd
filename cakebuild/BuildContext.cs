using Cake.Core;
using Cake.Frosting;

namespace cakebuild
{
    public class BuildContext : FrostingContext
    {
        public BuildContext(ICakeContext context) : base(context)
        {
        }
    }
}

