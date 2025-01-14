﻿using NLog;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace chocolatey.infrastructure.logging
{
    [Target("VerifyingLogTarget")]
    public class VerifyingLogTarget : FileTarget
    {
        FileStream fs;
        StreamReader streamreader;
        bool verifying;
        int lineN;
        string logPath;
        string lastException = null;

        protected override void CloseTarget()
        {
            base.CloseTarget();
            fs?.Dispose();
            streamreader?.Dispose();
        }

        // Not included in code coverage, because non working test also means no code coverage
        [ExcludeFromCodeCoverage]
        public VerifyingLogTarget(string name, string path, bool _createNew = false): base(name)
        {
            bool allowToCreatingLog = false;

            // On official build machine this will be not allowed (just in case if developer forgets to commit his log changes)
            #if DEBUG || RELWITHDEBINFO
            allowToCreatingLog = true;
            #endif

            AutoFlush = true;
            Layout = "${message}";

            bool createNew = (!File.Exists(path) && allowToCreatingLog) || _createNew;
            verifying = !createNew;
            logPath = path;
            lastException = null;

            if (createNew)
            {
                FileName = path;
                CreateDirs = true;
                ArchiveOldFileOnStartup = true;
                MaxArchiveFiles = 1;

                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

        //unlike in FileTarget - this one is without [RequiredParameter]
        [ExcludeFromCodeCoverage]
        new public Layout FileName
        {
            get
            {
                return base.FileName;
            }
            set
            {
                base.FileName = value;
            }
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

        [ExcludeFromCodeCoverage]
        protected override void Write(LogEventInfo logEvent)
        {
            if (!verifying)
            { 
                base.Write(logEvent);
                return;
            }

            // It's possible that functionality does not produce any logs and also does not produce
            // the log file - that why we open file here in case if there is no file.
            if (fs == null)
            {
                fs = File.OpenRead(logPath);
                streamreader = new StreamReader(fs, Encoding.UTF8, true, 4096);
                lineN = 0;
            }

            // Extra guard just in case if last failure was eaten by try-catch handling, we re-raise here 
            // exception again.
            if (lastException != null)
            {
                // NLog catches some of exceptions, but bypasses this exception always.
                throw new OutOfMemoryException(lastException);
            }

            string actualLines = base.Layout.Render(logEvent);
            List<string> lines = actualLines.Replace("\r\n", "\n").Split(new char[] { '\r', '\n' }).ToList();

            for(int i = 0; i < lines.Count; i++)
            {
                var actualLine = lines[i]; 
                string expectedLine = streamreader.ReadLine();
                lineN++;

                if (actualLine != expectedLine)
                {
                    lastException = $"Unexpected {lineN} line {logPath}:\n" +
                        $"actual line  : '{actualLine}'\n" +
                        $"expected line: '{expectedLine}'\n";

                    string rest = String.Join("\n", lines.Skip(i + 1));
                    if (!string.IsNullOrEmpty(rest))
                    {
                        lastException += $"remaining text: {rest}";
                    }

                    // Uncomment to troubleshoot multitasking problems when difficult to catch the problem.
                    //MessageBox(IntPtr.Zero, lastException, "exception halt", 0);

                    throw new Exception(lastException);
                }
            }
        }
    }
}
