using System;
using System.Collections.Generic;
using System.Text;

namespace chocolatey.tests2
{
    public enum ChocoTestContext
    {
        /// <summary>
        /// Skips root folder initialization
        /// </summary>
        skipcontextinit,
        install,
        install_sxs,
        installupdate,
        hasdependency,
        exactpackage,
        badpackage,
        empty,

        packages_default,
        packages_for_dependency_testing,
        packages_for_dependency_testing2,
        packages_for_dependency_testing3,

        pack_badpackage_1_0,
        pack_hasdependency_1_0_0,
        pack_hasdependency_1_0_1,
        pack_hasdependency_1_1_0,
        pack_hasdependency_1_5_0,
        pack_hasdependency_1_6_0,
        pack_hasdependency_2_0_0,
        pack_hasdependency_2_1_0,
        pack_installpackage_1_0_0,
        pack_isdependency_1_0_0,
        pack_isdependency_1_0_1,
        pack_isdependency_1_1_0,
        pack_isdependency_2_0_0,
        pack_isdependency_2_1_0,
        pack_isexactversiondependency_1_0_0,
        pack_isexactversiondependency_1_0_1,
        pack_isexactversiondependency_1_1_0,
        pack_isexactversiondependency_2_0_0,
    };
}
