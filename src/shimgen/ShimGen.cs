﻿using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Text;
using System.Diagnostics;
using System.Drawing.IconLib;
using System.Text.RegularExpressions;
using System.Linq;
using PeNet;

namespace ConsoleApplication
{
    public class MyCmdArgs
    {
        public string path;
        public string output;
        public string iconpath;
        public bool gui;
        public bool help;
        public bool debug;
        public string temp;
        public string net = "netcoreapp3.1";
    }

    public class ShimGen
    {
        public static void Main(string[] args)
        {
            MyCmdArgs cmdArgs = CmdArgs.Parse<MyCmdArgs>(args);

            if (cmdArgs.output == null || cmdArgs.path == null || cmdArgs.help)
            {
                Console.WriteLine(
$@"Usage: shimget -output=<.exe path> -path=<in .exe path> [optional arguments]

    Arguments:
        -help               - this help
        -output <path>      - path of output shim executable
        -path <path>        - path of input executable
        -iconpath <path>    - path of executable where to get icon file
        -net <.net version> - one of: netcoreapp3.1, net5.0, net6.0, net48
        -temp <path>        - temporary directory for compilation temp (if not specified %TEMP%\shimgen_<process id> will be used

    Argument can be shorted by one letter, e.g. -h, -o, -p ...
");
                return;
            }

            string outputPath = cmdArgs.output;
            string targetExePath = cmdArgs.path;
            if (!Path.IsPathRooted(targetExePath))
            {
                targetExePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(outputPath), targetExePath));
            }

            if (!File.Exists(targetExePath))
            {
                Console.WriteLine($"Error: Executable specified by -{nameof(MyCmdArgs.path)} does not exists: '{targetExePath}'");
                return;
            }

            try {
                var file = new PeFile(targetExePath);
                if (!cmdArgs.gui && file.ImageNtHeaders.OptionalHeader.Subsystem == 2 /*PeNet.Header.Pe.SubsystemType.WindowsGui*/)
                {
                    cmdArgs.gui = true;
                }
            }
            catch (Exception ex)
            {
                if (cmdArgs.debug)
                {
                    Console.WriteLine($"Cannot load {targetExePath}: {ex.Message}");
                }
            }

            targetExePath = makeRelative(targetExePath, Path.GetDirectoryName(outputPath));

            string asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string tempDir = cmdArgs.temp;
            if (string.IsNullOrEmpty(tempDir))
            {
                tempDir = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), $"shimgen_{Process.GetCurrentProcess().Id}");
            }

            string dotnetTempDir = Path.Combine(tempDir, "temp");
            string csproj = Path.Combine(tempDir, "shim.csproj");
            string bindir = Path.Combine(tempDir, "bin");
            if (!Directory.Exists(bindir))
                Directory.CreateDirectory(bindir);

            using (XmlTextWriter xml = new XmlTextWriter(csproj, Encoding.UTF8))
            {
                xml.Formatting = Formatting.Indented;
                xml.WriteStartElement("Project");
                xml.WriteAttributeString("Sdk", "Microsoft.NET.Sdk");

                xml.WriteStartElement("PropertyGroup");
                xml.WriteElementString("OutputType", "Exe");

                xml.WriteElementString("TargetFramework", cmdArgs.net);

                xml.WriteElementString("AppendTargetFrameworkToOutputPath", "false");
                xml.WriteElementString("OutputPath", "bin");
                xml.WriteElementString("AssemblyName", Path.GetFileNameWithoutExtension(outputPath));
                xml.WriteElementString("AssemblyTitle", "ShimGen generated shim");
                xml.WriteElementString("AssemblyInformationalVersion", "1.0.0");
                xml.WriteElementString("AssemblyVersion", "1.0.0");
                xml.WriteElementString("AssemblyProduct", "ShimGen generated shim");
                xml.WriteElementString("AssemblyDesciption", "This is a shim that points to a particular file. It was generated by ShimGen (Shim Generator).");

                bool hasIcon = false;
                if (cmdArgs.iconpath != null)
                {
                    try
                    {
                        // See https://stackoverflow.com/a/74411477/2338477
                        string iconPath = Path.Combine(tempDir, "app.ico");
                        if (File.Exists(iconPath)) File.Delete(iconPath);

                        MultiIcon multiIcon = new MultiIcon();
                        multiIcon.Load(cmdArgs.iconpath);
                        multiIcon.Save(iconPath, MultiIconFormat.ICO);

                        //var peFile = new PeFile(cmdArgs.iconpath);
                        //byte[] icon = peFile.Icons().First().AsSpan().ToArray();
                        //File.WriteAllBytes(iconPath, icon);

                        hasIcon = true;

                    }
                    catch (Exception ex)
                    {
                        if (cmdArgs.debug)
                        {
                            Console.WriteLine($"Cannot extract icon: {ex.Message}");
                        }
                    }
                }
                string toShimDir = asmDir + Path.DirectorySeparatorChar;
                if (hasIcon)
                {
                    xml.WriteElementString("ApplicationIcon", "app.ico");
                    xml.WriteElementString("ApplicationManifest", $"{toShimDir}shim.manifest");
                }

                xml.WriteEndElement();

                xml.WriteStartElement("ItemGroup");

                string[] srcs = new[] {
                    "CmdArgs.cs",
                    "ShimTemplate.cs",
                };

                foreach (var src in srcs)
                {
                    xml.WriteStartElement("Compile");
                    xml.WriteAttributeString("Include", $"{toShimDir}{src}");
                    xml.WriteAttributeString("Link", src);
                    xml.WriteEndElement();
                }
                
                xml.WriteEndElement();
            }

            using (StreamWriter text = new StreamWriter(Path.Combine(tempDir, "shim.cs")))
            {
                text.WriteLine(
$@"using System;

class Program
{{
	static int Main(string[] args)
	{{
        ShimArgs shimargs = new ShimArgs(){{
            shimgen_exepath = @""{targetExePath}"",
            shimgen_gui = {cmdArgs.gui.ToString().ToLower()}
        }};

        return ShimTemplate.ShimMain(args, shimargs);
	}}
}}
	");
            }

            Environment.CurrentDirectory = tempDir;

            if (File.Exists(outputPath)) File.Delete(outputPath);

            string dotnetPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\dotnet\dotnet.exe");
            if (!File.Exists(dotnetPath))
            {
                Console.WriteLine($"Error: dotnet executable does not exists: {dotnetPath}");
                return;
            }

            //dotnet build generates a lot of temporary files, we will clean them up by redirecting temp directory
            Environment.SetEnvironmentVariable("TEMP", dotnetTempDir);
            Environment.SetEnvironmentVariable("TMP", dotnetTempDir);
            runDotNet(dotnetPath, "build shim.csproj --verbosity quiet --nologo -consoleLoggerParameters:NoSummary");

            // We could run a publish here, but it uses subfolder. Easier just to copy whatever we have.
            if (Directory.Exists(bindir))
            {
                string outdir = Path.GetDirectoryName(outputPath);
                var files2copy = Directory.GetFiles(bindir, "*");
                foreach (var file in files2copy)
                {
                    if (Path.GetExtension(file).ToLower() == ".pdb") continue;  // no need for debug symbols

                    string targetFile = Path.Combine(outdir, Path.GetFileName(file));
                    File.Copy(file, targetFile, true);
                }
            }

            if (File.Exists(outputPath))
            {
                Console.WriteLine($" ShimGen has successfully created a shim for {Path.GetFileName(outputPath)}\n");
            }
            Environment.CurrentDirectory = asmDir;

            if (!cmdArgs.debug)
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch (IOException)
                { 
                    //maybe running in parallel with dotnet command - cannot delete directory sometimes
                }
            }
        }

        private static void runDotNet(string dotnetPath, string arguments)
        {
            Process p = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = dotnetPath,
                    UseShellExecute = false,
                    Arguments = arguments
                }
            };
            if (!p.Start()) throw new Exception($"Process {p.StartInfo.FileName} cannot be started");
            p.WaitForExit();
            //.net core 3.1 or higher
            //Process.Start("dotnet", "build shim.csproj --verbosity quiet --nologo -consoleLoggerParameters:NoSummary").WaitForExit();
        }

        /// <summary>
        /// Rebases file with path fromPath to folder with baseDir.
        /// </summary>
        /// <param name="_fromPath">Full file path (absolute)</param>
        /// <param name="_baseDir">Full base directory path (absolute)</param>
        /// <returns>Relative path to file in respect of baseDir</returns>
        static public String makeRelative(String _fromPath, String _baseDir)
        {
            String pathSep = "\\";
            String fromPath = Path.GetFullPath(_fromPath);
            String baseDir;

            if (!string.IsNullOrEmpty(_baseDir))
            {
                baseDir = Path.GetFullPath(_baseDir);            // If folder contains upper folder references, they gets lost here. "c:\test\..\test2" => "c:\test2"
            }
            else
            {
                // Path not specified, assuming current directory
                baseDir = Environment.CurrentDirectory;
            }


            String[] p1 = Regex.Split(fromPath, "[\\\\/]").Where(x => x.Length != 0).ToArray();
            String[] p2 = Regex.Split(baseDir, "[\\\\/]").Where(x => x.Length != 0).ToArray();
            int i = 0;

            for (; i < p1.Length && i < p2.Length; i++)
                if (String.Compare(p1[i], p2[i], true) != 0)    // Case insensitive match
                    break;

            if (i == 0)     // Cannot make relative path, for example if resides on different drive
                return fromPath;
                
            String r = String.Join(pathSep, Enumerable.Repeat("..", p2.Length - i).Concat(p1.Skip(i).Take(p1.Length - i)));
            return r;


        }

    }
}
