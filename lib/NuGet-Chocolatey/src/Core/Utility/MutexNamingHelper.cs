using System;
using System.Collections.Generic;
using System.Text;

namespace NuGet.Utility
{
    public class MutexNamingHelper
    {
        public static string GenerateUniqueToken(string fileName)
        {
            // new Mutex in Linux does not like forward slashes.
            return EncryptionUtility.GenerateUniqueToken(fileName).Replace("/", "");
        }
    }
}
