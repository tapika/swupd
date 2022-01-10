﻿using chocolatey.infrastructure.registration;
using NUnit.Framework;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Text;

// class being tested must reside in same namespace as [SetUpFixture] class, otherwise
// it wont get executed
namespace chocolatey.tests2
{
    [SetUpFixture]
    public class NUnitSetup: tests.integration.NUnitSetup
    {
        public override void BeforeEverything()
        {
            base.BeforeEverything(false);
        }
    }
}
