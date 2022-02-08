using System;
using System.Collections.Generic;
using System.Text;

namespace chocolatey.tests2
{
    public enum ChocoTestContext
    {
        install,
        install_sxs,
        installupdate,
        exactpackage,
        badpackage,
        empty
    };
}
