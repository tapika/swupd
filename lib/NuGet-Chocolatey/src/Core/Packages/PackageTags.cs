using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NuGet.Packages
{
    /// <summary>
    /// All nuget servers carries certain meta data on package. Trying to extend that meta data would require
    /// server side upgrade. This is escpecially difficult if there are multiple nuget servers in use, one for each own 
    /// purpose. One approach is to keep existing meta data as such, but try to extend file format - so
    /// extra data will be encoded in existing fields. This class will try to encode and parse to/from string
    /// in nuspec / Tags string field.
    /// 
    /// Because we want to support also spaces in value - we can use Uri encoding scheme to get %20 instead of spaces.
    /// </summary>
    public static class IPackageExtension
    {
        static Regex reSplitEqual = new Regex("([^=]*)=?(.*)");
        /// <summary>
        /// Gets key from specific package
        /// </summary>
        /// <returns></returns>
        public static string GetKey(this IPackage pack, string key)
        {
            string value;
            if (pack.TagsExtra != null)
            {
                value = pack.TagsExtra.Where(x => x.Key == key).FirstOrDefault()?.Value;
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            var lines = pack.Tags.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            value = lines.
                Select(x => reSplitEqual.Match(x)).
                Where(m => m.Success && m.Groups[1].Value == key).
                Select(m => Uri.UnescapeDataString(m.Groups[2].Value)).FirstOrDefault();

            return value;
        }

        public static string GetInstallLocation(this IPackage pack)
        {
            return GetKey(pack, "InstallLocation");
        }
    }
}
