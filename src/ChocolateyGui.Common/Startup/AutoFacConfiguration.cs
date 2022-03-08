// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Chocolatey" file="AutoFacConfiguration.cs">
//   Copyright 2017 - Present Chocolatey Software, LLC
//   Copyright 2014 - 2017 Rob Reynolds, the maintainers of Chocolatey, and RealDimensions Software, LLC
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Autofac;
using chocolatey.infrastructure.information;
using chocolatey.infrastructure.registration;

namespace ChocolateyGui.Common.Startup
{
    public static class AutoFacConfiguration
    {
        [SuppressMessage(
            "Microsoft.Maintainability",
            "CA1506:AvoidExcessiveClassCoupling",
            Justification = "This is really a requirement due to required registrations.")]
        public static IContainer RegisterAutoFac(string chocolateyGuiAssemblySimpleName, string licensedGuiAssemblyLocation)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(System.Reflection.Assembly.GetCallingAssembly());
            return builder.Build();
        }
    }
}