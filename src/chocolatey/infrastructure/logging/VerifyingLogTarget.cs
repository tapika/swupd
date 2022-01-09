using NLog;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace chocolatey.infrastructure.logging
{
    [Target("VerifyingLogTarget")]
    public class VerifyingLogTarget : FileTarget, IDisposable
    {
        FileStream fs;
        StreamReader streamreader;
        bool verifying;
        int lineN;
        string logPath;

        void IDisposable.Dispose()
        {
            fs?.Dispose();
            streamreader?.Dispose();
        }
    
        public VerifyingLogTarget(string name, string path, bool _createNew = false): base(name)
        {
            bool allowToCreatingLog = false;

            // On official build machine this will be not allowed (just in case if developer forgets to commit his log changes)
            #if DEBUG
            allowToCreatingLog = true;
            #endif

            AutoFlush = true;
            Layout = "${message}";

            bool createNew = (!File.Exists(path) && allowToCreatingLog) || _createNew;
            verifying = !createNew;
            logPath = path;

            if (createNew)
            {
                FileName = path;
                CreateDirs = true;
                ArchiveOldFileOnStartup = true;
                MaxArchiveFiles = 1;
            }
        }

        //unlike in FileTarget - this one is without [RequiredParameter]
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
                lineN = 1;
            }

            string actualLines = base.Layout.Render(logEvent);

            foreach (var actualLine in actualLines.Replace("\r\n", "\n").Split(new char[] { '\r', '\n' }))
            {
                string expectedLine = streamreader.ReadLine();
                lineN++;

                if (actualLine != expectedLine)
                {
                    throw new Exception(
                        $"Unexpected {lineN} line {logPath}:\n" +
                        $"actual line  : '{actualLine}'\n" +
                        $"expected line: '{expectedLine}'\n"
                    );
                }
            }
        }
    }
}
