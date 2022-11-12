using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

    
/// <summary>
/// Tiny command line arguments parser
/// </summary>
public class CmdArgs
{
    /// <summary>
    /// Parses command line arguments into cmdArgs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args">Arguments to parse</param>
    /// <param name="cmdArgs">structure to fill in</param>
    /// <param name="allowShortcuts">allow to shorten command line arguments (-help => -h)</param>
    /// <param name="remainingArgs">Remaining (unparsed arguments), is arguments are parsed in multiple turns</param>
    /// <returns>cmdArgs</returns>
    public static T Parse<T>(string[] args, bool allowShortcuts = true, T cmdArgs = null, List<string> remainingArgs = null) where T: class, new()
    {
        if (cmdArgs == null) cmdArgs = new T();
        // matches: --arg=value -arg=value /arg:value ...    
        var reKeyArg = new Regex("^[-/]{1,2}([^=:]+)[=:](.*)$");
        // matches: --arg -arg /arg
        var reKey = new Regex("^[-/]{1,2}(.+)$");
        Match m;
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            string key;
            object value = null;
            bool isKeyValuePair = (m = reKeyArg.Match(arg)).Success;

            if (isKeyValuePair)
            {
                key = m.Groups[1].ToString();
                value = m.Groups[2].ToString();
            }
            else
            {
                if (!(m = reKey.Match(arg)).Success)
                {
                    if (remainingArgs != null) remainingArgs.Add(arg);
                    continue;
                }
                key = m.Groups[1].ToString();
            }

            key = key.Replace("-", "_");        //E.g. shimgen-help => shimgen_help
            if (allowShortcuts && key == "?") key = "help";

            var f = cmdArgs.GetType().GetFields().Where(x => x.Name == key || 
                (allowShortcuts && key.Length == 1 && key[0] == x.Name[0])).FirstOrDefault();
            if (f == null)
            { 
                if (remainingArgs != null) remainingArgs.Add(arg);
                continue;
            }

            if (!isKeyValuePair)
            {
                if (f.FieldType == typeof(bool))
                {
                    value = true;
                }
                else
                {
                    i++;
                    if (i == args.Length)
                    {
                        if (remainingArgs != null) remainingArgs.Add(arg);
                        continue;
                    }
                    value = args[i];
                }
            }

            f.SetValue(cmdArgs, value);
        }

        return cmdArgs;
    }
}

