﻿// Copyright © 2017 - 2021 Chocolatey Software, Inc
// Copyright © 2011 - 2017 RealDimensions Software, LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// 
// You may obtain a copy of the License at
// 
// 	http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace chocolatey.infrastructure.filesystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using adapters;
    using chocolatey.infrastructure.app;
    using logging;
    using platforms;
    using tolerance;
    using Environment = adapters.Environment;
#if NETFRAMEWORK
    using System_IO = Alphaleonis.Win32.Filesystem;
#else
    using System_IO = System.IO;
#endif

    /// <summary>
    ///   Implementation of IFileSystem for Dot Net
    /// </summary>
    /// <remarks>Normally we avoid regions, however this has so many methods that we are making an exception.</remarks>
    public class DotNetFileSystem : DotNetFileSystemBase
    {
        private readonly int TIMES_TO_TRY_OPERATION = 3;
        private const int MAX_PATH_FILE = 255;
        private const int MAX_PATH_DIRECTORY = 248;

        private void allow_retries(Action action, bool isSilent = false)
        {
            FaultTolerance.retry(
                TIMES_TO_TRY_OPERATION,
                action,
                waitDurationMilliseconds: 200,
                increaseRetryByMilliseconds: 100,
                isSilent: isSilent);
        }

        #region File

        public override IEnumerable<string> get_files(string directoryPath, string pattern = "*.*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) return new List<string>();
            if (!directory_exists(directoryPath))
            {
                this.Log().Warn("Directory '{0}' does not exist.".format_with(directoryPath));
                return new List<string>();
            }

            return Directory.EnumerateFiles(directoryPath, pattern, option);
        }

        public override IEnumerable<string> get_files(string directoryPath, string[] extensions, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) return new List<string>();

            return Directory.EnumerateFiles(directoryPath, "*.*", option)
                            .Where(f => extensions.Any(x => f.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
        }

        public override bool file_exists(string filePath)
        {
            try
            {
                return File.Exists(filePath);
            }
            catch (IOException)
            {
                return System_IO.File.Exists(filePath);
            }
        }

        public override dynamic get_file_info_for(string filePath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && filePath.Length >= MAX_PATH_FILE)
                {
                    return new System_IO.FileInfo(filePath);
                }

                return new FileInfo(filePath);
            }
            catch (IOException)
            {
                return new System_IO.FileInfo(filePath);
            }
        }

        public override System.DateTime get_file_modified_date(string filePath)
        {
            return new FileInfo(filePath).LastWriteTime;
        }

        public override long get_file_size(string filePath)
        {
            return new FileInfo(filePath).Length;
        }

        public override string get_file_version_for(string filePath)
        {
            return FileVersionInfo.GetVersionInfo(get_full_path(filePath)).FileVersion;
        }

        public override bool is_system_file(dynamic file)
        {
            bool isSystemFile = ((file.Attributes & FileAttributes.System) == FileAttributes.System);
            if (!isSystemFile)
            {
                //check the directory to be sure
                var directoryInfo = get_directory_info_for(file.DirectoryName);
                isSystemFile = ((directoryInfo.Attributes & FileAttributes.System) == FileAttributes.System);
            }
            else
            {
                string fullName = file.FullName;
                this.Log().Debug(ChocolateyLoggers.Verbose, () => "File \"{0}\" is a system file.".format_with(fullName));
            }

            return isSystemFile;
        }

        public override bool is_readonly_file(dynamic file)
        {
            bool isReadOnlyFile = ((file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
            if (!isReadOnlyFile)
            {
                //check the directory to be sure
                dynamic directoryInfo = get_directory_info_for(file.DirectoryName);
                isReadOnlyFile = ((directoryInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
            }
            else
            {
                string fullName = file.FullName;
                this.Log().Debug(ChocolateyLoggers.Verbose, () => "File \"{0}\" is a readonly file.".format_with(fullName));
            }

            return isReadOnlyFile;
        }

        public override bool is_hidden_file(dynamic file)
        {
            bool isHiddenFile = ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden);
            if (!isHiddenFile)
            {
                //check the directory to be sure
                var directoryInfo = get_directory_info_for(file.DirectoryName);
                isHiddenFile = ((directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden);
            }
            else
            {
                string fullName = file.FullName;
                this.Log().Debug(ChocolateyLoggers.Verbose, () => "File \"{0}\" is a hidden file.".format_with(fullName));
            }

            return isHiddenFile;
        }

        public override void move_file(string filePath, string newFilePath)
        {
            create_directory_if_not_exists(get_directory_name(newFilePath), ignoreError: true);

            allow_retries(
                () =>
                {
                    try
                    {
                        File.Move(filePath, newFilePath);
                    }
                    catch (IOException)
                    {
                        System_IO.File.Move(filePath, newFilePath);
                    }
                });
            //Thread.Sleep(10);
        }

        public override void copy_file(string sourceFilePath, string destinationFilePath, bool overwriteExisting)
        {
            this.Log().Debug(ChocolateyLoggers.Verbose, () => "Attempting to copy \"{0}\"{1} to \"{2}\".".format_with(sourceFilePath, Environment.NewLine, destinationFilePath));
            create_directory_if_not_exists(get_directory_name(destinationFilePath), ignoreError: true);

            allow_retries(
                () =>
                {
                    try
                    {
                        File.Copy(sourceFilePath, destinationFilePath, overwriteExisting);
                    }
                    catch (IOException)
                    {
                        System_IO.File.Copy(sourceFilePath, destinationFilePath, overwriteExisting);
                    }
                });
        }

        public override bool copy_file_unsafe(string sourceFilePath, string destinationFilePath, bool overwriteExisting)
        {
            if (Platform.get_platform() != PlatformType.Windows)
            {
                copy_file(sourceFilePath, destinationFilePath, overwriteExisting);
                return true;
            }

            this.Log().Debug(ChocolateyLoggers.Verbose, () => "Attempting to copy from \"{0}\" to \"{1}\".".format_with(sourceFilePath, destinationFilePath));
            create_directory_if_not_exists(get_directory_name(destinationFilePath), ignoreError: true);

            //Private Declare Function apiCopyFile Lib "kernel32" Alias "CopyFileA" _
            int success = CopyFileW(sourceFilePath, destinationFilePath, overwriteExisting ? 0 : 1);
            //if (success == 0)
            //{
            //    var error = Marshal.GetLastWin32Error();

            //}
            return success != 0;
        }

        public override void replace_file(string sourceFilePath, string destinationFilePath, string backupFilePath)
        {
            this.Log().Debug(ChocolateyLoggers.Verbose, () => "Attempting to replace \"{0}\"{1} with \"{2}\".{1} Backup placed at \"{3}\".".format_with(destinationFilePath, Environment.NewLine, sourceFilePath, backupFilePath));

            allow_retries(
                () =>
                {
                    try
                    {
                        // File.Replace is very sensitive to issues with file access
                        // the docs mention that using a backup fixes this, but this
                        // has not been the case with choco - the backup file has been
                        // the most sensitive to issues with file locking
                        //File.Replace(sourceFilePath, destinationFilePath, backupFilePath);

                        // move existing file to backup location
                        if (!string.IsNullOrEmpty(backupFilePath) && file_exists(destinationFilePath))
                        {
                            try
                            {
                                this.Log().Trace("Backing up '{0}' to '{1}'.".format_with(destinationFilePath, backupFilePath));

                                if (file_exists(backupFilePath))
                                {
                                    this.Log().Trace("Deleting existing backup file at '{0}'.".format_with(backupFilePath));
                                    delete_file(backupFilePath);
                                }
                                this.Log().Trace("Moving '{0}' to '{1}'.".format_with(destinationFilePath, backupFilePath));
                                move_file(destinationFilePath, backupFilePath);
                            }
                            catch (Exception ex)
                            {
                                this.Log().Debug("Error capturing backup of '{0}':{1} {2}".format_with(destinationFilePath, Environment.NewLine, ex.Message));
                            }
                        }

                        // copy source file to destination
                        this.Log().Trace("Copying '{0}' to '{1}'.".format_with(sourceFilePath, destinationFilePath));
                        copy_file(sourceFilePath, destinationFilePath, overwriteExisting: true);
                        
                        // delete source file
                        try
                        {
                            this.Log().Trace("Removing '{0}'".format_with(sourceFilePath));
                            delete_file(sourceFilePath);
                        }
                        catch (Exception ex)
                        {
                            this.Log().Debug("Error removing '{0}':{1} {2}".format_with(sourceFilePath, Environment.NewLine, ex.Message));
                        }
                    }
                    catch (IOException)
                    {
                        System_IO.File.Replace(sourceFilePath, destinationFilePath, backupFilePath);
                    }
                });
        }

        // ReSharper disable InconsistentNaming

        // http://msdn.microsoft.com/en-us/library/windows/desktop/aa363851.aspx
        // http://www.pinvoke.net/default.aspx/kernel32.copyfile
        /*
            BOOL WINAPI CopyFile(
              _In_  LPCTSTR lpExistingFileName,
              _In_  LPCTSTR lpNewFileName,
              _In_  BOOL bFailIfExists
            );
         */

        [DllImport("kernel32", SetLastError = true)]
        private static extern int CopyFileW(string lpExistingFileName, string lpNewFileName, int bFailIfExists);

        // ReSharper restore InconsistentNaming

        public override void delete_file(string filePath)
        {
            this.Log().Debug(ChocolateyLoggers.Verbose, () => "Attempting to delete file \"{0}\".".format_with(filePath));
            if (file_exists(filePath))
            {
                allow_retries(
                    () =>
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (IOException)
                        {
                            System_IO.File.Delete(filePath);
                        }
                    });
            }
        }

        public override FileStream create_file(string filePath)
        {
            return new FileStream(filePath, FileMode.OpenOrCreate);
        }

        public override string read_file(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath, get_file_encoding(filePath));
            }
            catch (IOException)
            {
                return System_IO.File.ReadAllText(filePath, get_file_encoding(filePath));
            }
        }

        public override byte[] read_file_bytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public override FileStream open_file_readonly(string filePath)
        {
            return File.OpenRead(filePath);
        }

        public override FileStream open_file_exclusive(string filePath)
        {
            return File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

        public override void write_file(string filePath, string fileText)
        {
            write_file(filePath, fileText, file_exists(filePath) ? get_file_encoding(filePath) : Encoding.UTF8);
        }

        public override void write_file(string filePath, string fileText, Encoding encoding)
        {
            allow_retries(() =>
                {
                    using (FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                    using (var streamWriter = new StreamWriter(fileStream, encoding))
                    {
                        streamWriter.Write(fileText);
                        streamWriter.Flush();
                        streamWriter.Close();
                        fileStream.Close();
                    }
                });
        }

        public override void write_file(string filePath, Func<Stream> getStream)
        {
            using (Stream incomingStream = getStream())
            using (Stream fileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                incomingStream.CopyTo(fileStream);
                fileStream.Close();
            }
        }

        #endregion

        #region Directory

        public override string get_current_directory()
        {
            return Directory.GetCurrentDirectory();
        }

        public override IEnumerable<string> get_directories(string directoryPath)
        {
            if (!directory_exists(directoryPath)) return new List<string>();

            return Directory.EnumerateDirectories(directoryPath);
        }

        public override IEnumerable<string> get_directories(string directoryPath, string pattern, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            if (!directory_exists(directoryPath)) return new List<string>();

            return Directory.EnumerateDirectories(directoryPath, pattern, option);
        }

        public override bool directory_exists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public override dynamic get_directory_info_for(string directoryPath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(directoryPath) && directoryPath.Length >= MAX_PATH_DIRECTORY)
                {
                    return new System_IO.DirectoryInfo(directoryPath);
                }

                return new DirectoryInfo(directoryPath);
            }
            catch (IOException)
            {
                return new System_IO.DirectoryInfo(directoryPath);
            }
        }

        public override dynamic get_directory_info_from_file_path(string filePath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath) && filePath.Length >= MAX_PATH_FILE)
                {
                    return new System_IO.DirectoryInfo(filePath).Parent;
                }

                return new DirectoryInfo(filePath).Parent;
            }
            catch (IOException)
            {
                return new System_IO.DirectoryInfo(filePath).Parent;
            }
        }

        public override void create_directory(string directoryPath)
        {
            this.Log().Debug(ChocolateyLoggers.Verbose, () => "Attempting to create directory \"{0}\".".format_with(get_full_path(directoryPath)));
            allow_retries(
                () =>
                {
                    try
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    catch (IOException)
                    {
                        System_IO.Directory.CreateDirectory(directoryPath);
                    }
                });
        }

        public override void move_directory(string directoryPath, string newDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || string.IsNullOrWhiteSpace(newDirectoryPath)) throw new ApplicationException("You must provide a directory to move from or to.");
            
            //Linux/MacOS do not have a SystemDrive environment variable, instead, everything is under "/"
            var systemDrive = Platform.get_platform() == PlatformType.Windows ? Environment.GetEnvironmentVariable("SystemDrive") : "/";
            if (combine_paths(directoryPath, "").is_equal_to(combine_paths(systemDrive, ""))) throw new ApplicationException("Cannot move or delete the root of the system drive");

            try
            {
                this.Log().Debug(ChocolateyLoggers.Verbose, "Moving '{0}'{1} to '{2}'".format_with(directoryPath, Environment.NewLine, newDirectoryPath));
                allow_retries(
                    () =>
                    {
                        try
                        {
                            Directory.Move(directoryPath, newDirectoryPath);
                        }
                        catch (IOException)
                        {
                            System_IO.Directory.Move(directoryPath, newDirectoryPath);
                        }
                    });
            }
            catch (Exception ex)
            {
                this.Log().Warn(ChocolateyLoggers.Verbose, "Move failed with message:{0} {1}{0} Attempting backup move method.".format_with(
                    Environment.NewLine, InstallContext.NormalizeMessage(ex.Message)));

                create_directory_if_not_exists(newDirectoryPath, ignoreError: true);
                foreach (var file in get_files(directoryPath, "*.*", SearchOption.AllDirectories).or_empty_list_if_null())
                {
                    var destinationFile = file.Replace(directoryPath, newDirectoryPath);
                    if (file_exists(destinationFile)) delete_file(destinationFile);

                    create_directory_if_not_exists(get_directory_name(destinationFile), ignoreError: true);
                    this.Log().Debug(ChocolateyLoggers.Verbose, "Moving '{0}'{1} to '{2}'".format_with(file, Environment.NewLine, destinationFile));
                    move_file(file, destinationFile);
                }

                Thread.Sleep(1000); // let the moving files finish up
                delete_directory_if_exists(directoryPath, recursive: true);
            }

            Thread.Sleep(2000); // sleep for enough time to allow the folder to be cleared
        }

        public override void copy_directory(string sourceDirectoryPath, string destinationDirectoryPath, bool overwriteExisting)
        {
            create_directory_if_not_exists(destinationDirectoryPath, ignoreError: true);

            foreach (var file in get_files(sourceDirectoryPath, "*.*", SearchOption.AllDirectories).or_empty_list_if_null())
            {
                var destinationFile = file.Replace(sourceDirectoryPath, destinationDirectoryPath);
                create_directory_if_not_exists(get_directory_name(destinationFile), ignoreError: true);
                //this.Log().Debug(ChocolateyLoggers.Verbose, "Copying '{0}' {1} to '{2}'".format_with(file, Environment.NewLine, destinationFile));
                copy_file(file, destinationFile, overwriteExisting);
            }

            Thread.Sleep(1500); // sleep for enough time to allow the folder to finish copying
        }

        public override void create_directory_if_not_exists(string directoryPath)
        {
            create_directory_if_not_exists(directoryPath, false);
        }

        public override void create_directory_if_not_exists(string directoryPath, bool ignoreError)
        {
            if (!directory_exists(directoryPath))
            {
                try
                {
                    create_directory(directoryPath);
                }
                catch (SystemException e)
                {
                    if (!ignoreError)
                    {
                        this.Log().Error("Cannot create directory \"{0}\". Error was:{1}{2}", get_full_path(directoryPath), Environment.NewLine, e);
                        throw;
                    }
                }
            }
        }

        public override void delete_directory(string directoryPath, bool recursive)
        {
            delete_directory(directoryPath, recursive, overrideAttributes: false, isSilent: false);
        }

        public override void delete_directory(string directoryPath, bool recursive, bool overrideAttributes)
        {
            delete_directory(directoryPath, recursive, overrideAttributes: overrideAttributes, isSilent: false);
        }

        public override void delete_directory(string directoryPath, bool recursive, bool overrideAttributes, bool isSilent)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) throw new ApplicationException("You must provide a directory to delete.");

            //Linux / MacOS do not have a SystemDrive environment variable, instead, everything is under "/"
            var systemDrive = Platform.get_platform() == PlatformType.Windows ? Environment.GetEnvironmentVariable("SystemDrive") : "/";
            if (combine_paths(directoryPath, "").is_equal_to(combine_paths(systemDrive, ""))) throw new ApplicationException("Cannot move or delete the root of the system drive");

            if (overrideAttributes)
            {
                foreach (var file in get_files(directoryPath, "*.*", SearchOption.AllDirectories))
                {
                    var filePath = get_full_path(file);
                    var fileInfo = get_file_info_for(filePath);

                    if (is_system_file(fileInfo)) ensure_file_attribute_removed(filePath, FileAttributes.System);
                    if (is_readonly_file(fileInfo)) ensure_file_attribute_removed(filePath, FileAttributes.ReadOnly);
                    if (is_hidden_file(fileInfo)) ensure_file_attribute_removed(filePath, FileAttributes.Hidden);
                }
            }

            if (!isSilent) this.Log().Debug(ChocolateyLoggers.Verbose, () => "Attempting to delete directory \"{0}\".".format_with(get_full_path(directoryPath)));
            allow_retries(
                () =>
                {
                    try
                    {
                        Directory.Delete(directoryPath, recursive);
                    }
                    catch (IOException)
                    {
                        System_IO.Directory.Delete(directoryPath, recursive);
                    }
                }, isSilent: isSilent);
        }

        public override void delete_directory_if_exists(string directoryPath, bool recursive)
        {
            delete_directory_if_exists(directoryPath, recursive, overrideAttributes: false, isSilent: false);
        }

        public override void delete_directory_if_exists(string directoryPath, bool recursive, bool overrideAttributes)
        {
            delete_directory_if_exists(directoryPath, recursive, overrideAttributes: overrideAttributes, isSilent: false);
        }

        public override void delete_directory_if_exists(string directoryPath, bool recursive, bool overrideAttributes, bool isSilent)
        {
            if (directory_exists(directoryPath))
            {
                delete_directory(directoryPath, recursive, overrideAttributes, isSilent);
            }
        }

        #endregion

        public override void ensure_file_attribute_set(string path, FileAttributes attributes)
        {
            if (directory_exists(path))
            {
                var directoryInfo = get_directory_info_for(path);
                if ((directoryInfo.Attributes & attributes) != attributes)
                {
                    this.Log().Debug(ChocolateyLoggers.Verbose, () => "Adding '{0}' attribute(s) to '{1}'.".format_with(attributes.to_string(), path));
                    directoryInfo.Attributes |= attributes;
                }
            }
            if (file_exists(path))
            {
                var fileInfo = get_file_info_for(path);
                if ((fileInfo.Attributes & attributes) != attributes)
                {
                    this.Log().Debug(ChocolateyLoggers.Verbose, () => "Adding '{0}' attribute(s) to '{1}'.".format_with(attributes.to_string(), path));
                    fileInfo.Attributes |= attributes;
                }
            }
        }

        public override void ensure_file_attribute_removed(string path, FileAttributes attributes)
        {
            if (directory_exists(path))
            {
                var directoryInfo = get_directory_info_for(path);
                if ((directoryInfo.Attributes & attributes) == attributes)
                {
                    this.Log().Debug(ChocolateyLoggers.Verbose, () => "Removing '{0}' attribute(s) from '{1}'.".format_with(attributes.to_string(), path));
                    directoryInfo.Attributes &= ~attributes;
                }
            }
            if (file_exists(path))
            {
                var fileInfo = get_file_info_for(path);
                if ((fileInfo.Attributes & attributes) == attributes)
                {
                    this.Log().Debug(ChocolateyLoggers.Verbose, () => "Removing '{0}' attribute(s) from '{1}'.".format_with(attributes.to_string(), path));
                    fileInfo.Attributes &= ~attributes;
                }
            }
        }

        /// <summary>
        ///   Takes a guess at the file encoding by looking to see if it has a BOM
        /// </summary>
        /// <param name="filePath">Path to the file name</param>
        /// <returns>A best guess at the encoding of the file</returns>
        /// <remarks>http://www.west-wind.com/WebLog/posts/197245.aspx</remarks>
        public override Encoding get_file_encoding(string filePath)
        {
            // *** Use Default of Encoding.Default (Ansi CodePage)
            Encoding enc = Encoding.Default;

            // *** Detect byte order mark if any - otherwise assume default
            var buffer = new byte[5];
            var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            file.Read(buffer, 0, 5);
            file.Close();

            if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf) enc = Encoding.UTF8;
            else if (buffer[0] == 0xfe && buffer[1] == 0xff) enc = Encoding.Unicode;
            else if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0xfe && buffer[3] == 0xff) enc = Encoding.UTF32;
            else if (buffer[0] == 0x2b && buffer[1] == 0x2f && buffer[2] == 0x76) enc = Encoding.UTF7;

            //assume xml is utf8
            //if (enc == Encoding.Default && get_file_extension(filePath).is_equal_to(".xml")) enc = Encoding.UTF8;

            return enc;
        }
    }
}