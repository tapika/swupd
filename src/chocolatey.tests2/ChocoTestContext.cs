﻿using System;
using System.Collections.Generic;
using System.Text;

namespace chocolatey.tests2
{
    public enum ChocoTestContext
    {
        install,
        installupdate,
        exactpackage,
        badpackage,
        empty
    };
}