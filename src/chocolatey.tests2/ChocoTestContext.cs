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
        installupdate2,
        installpackage3,
        isdependency,
        isdependency_hasdependency,
        isdependency_hasdependency_sxs,
        hasdependency,
        installed_5_packages,
        exactpackage,
        badpackage,
        upgradepackage_1_1_1_beta,
        empty,

        packages_default,
        packages_for_dependency_testing,
        packages_for_dependency_testing2,
        packages_for_dependency_testing3,
        packages_for_dependency_testing4,
        packages_for_dependency_testing5,
        packages_for_dependency_testing6,
        packages_for_dependency_testing8,
        packages_for_dependency_testing9,
        packages_for_dependency_testing10,
        packages_for_dependency_testing11,
        
        packages_for_reg_dependency_testing,
        packages_for_upgrade_testing,
        upgrade_testing_context,
        uninstall_testing_context,

        pack_badpackage_1_0,
        pack_badpackage_2_0,
        pack_hasdependency_1_0_0,
        pack_hasdependency_1_0_1,
        pack_hasdependency_1_1_0,
        pack_hasdependency_1_5_0,
        pack_hasdependency_1_6_0,
        pack_hasdependency_2_0_0,
        pack_hasdependency_2_1_0,
        pack_installpackage_1_0_0,
        pack_installpackage2_1_0_0,
        pack_installpackage2_2_3_0,
        pack_installpackage3_1_0_0,
        pack_isdependency_1_0_0,
        pack_isdependency_1_0_1,
        pack_isdependency_1_1_0,
        pack_isdependency_2_0_0,
        pack_isdependency_2_1_0,
        pack_conflictingdependency_1_0_1,
        pack_conflictingdependency_2_1_0,
        pack_isexactversiondependency_1_0_0,
        pack_isexactversiondependency_1_0_1,
        pack_isexactversiondependency_1_1_0,
        pack_isexactversiondependency_2_0_0,
        pack_upgradepackage_1_0_0,
        pack_upgradepackage_1_1_0,
        pack_upgradepackage_1_1_1_beta,
        pack_upgradepackage_1_1_1_beta2,

        pack_reghasdependency_1_0_0
    };
}

