namespace chocolatey.infrastructure.app.commands
{
    using System.Collections.Generic;
    using commandline;
    using configuration;
    using infrastructure.commands;

    /// <summary>
    /// Artificial command used to initialize application startup parameters.
    /// </summary>
    public class ChocolateyStartupCommand : ChocolateyCommand
    {
        override public void configure_argument_parser(OptionSet optionSet, ChocolateyConfiguration config)
        {
            configureStartupOptions(optionSet);
        }

        public static void configureStartupOptions(OptionSet optionSet)
        {
            // For some reason not empty when called for second time via Lets.GetChocolatey()
            if (!optionSet.Contains("root"))
            {
                optionSet
                    .Add("root=", "Sets root location",
                        option => InstallContext.Instance.RootLocation = option.remove_surrounding_quotes())
                    .Add("clearlogs|clearlog", "Removes logs if any is present",
                        option => ApplicationParameters.ClearLogFiles = option != null)
                ;
            }
        }
    }
}
