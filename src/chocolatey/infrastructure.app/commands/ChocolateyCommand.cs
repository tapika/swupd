using System.Collections.Generic;
using chocolatey.infrastructure.commandline;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.commands;

namespace chocolatey.infrastructure.app.commands
{
    /// <summary>
    /// Dummy empty command implementation.
    /// </summary>
    public class ChocolateyCommand : ICommand
    {
        public virtual void configure_argument_parser(OptionSet optionSet, ChocolateyConfiguration config)
        {
        }

        public virtual void handle_additional_argument_parsing(IList<string> unparsedArguments, ChocolateyConfiguration configuration)
        {
        }

        public virtual void handle_validation(ChocolateyConfiguration configuration)
        {
        }

        public virtual void help_message(ChocolateyConfiguration configuration)
        {
        }

        public virtual void noop(ChocolateyConfiguration configuration)
        {
        }

        public virtual void run(ChocolateyConfiguration configuration)
        {
        }

        public virtual bool may_require_admin_access()
        {
            return false;
        }
    }
}

