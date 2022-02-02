namespace chocolatey.infrastructure.filesystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using app;
    using chocolatey.infrastructure.adapters;
    using logging;
    using platforms;
    using Assembly = adapters.Assembly;
    using Environment = adapters.Environment;
#if NETFRAMEWORK
    using System_IO = Alphaleonis.Win32.Filesystem;
#else
    using System_IO = System.IO;
#endif

    /// <summary>
    ///   Base class for filesystem implementation. Only in-ram operations are listed here.
    /// </summary>
    public abstract class DotNetFileSystemBase : IFileSystem
    {
        #region Path
        private static Lazy<IEnvironment> environment_initializer = new Lazy<IEnvironment>(() => new Environment());
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void initialize_with(Lazy<IEnvironment> environment)
        {
            environment_initializer = environment;
        }

        protected static IEnvironment Environment
        {
            get { return environment_initializer.Value; }
        }

        public string combine_paths(string leftItem, params string[] rightItems)
        {
            if (leftItem == null)
            {
                var methodName = string.Empty;
                var stackFrame = new System.Diagnostics.StackFrame(1);
                if (stackFrame != null) methodName = stackFrame.GetMethod().Name;
                throw new ApplicationException("Path to combine cannot be empty. Tried to combine null with '{0}'.{1}".format_with(string.Join(",", rightItems), string.IsNullOrWhiteSpace(methodName) ? string.Empty : " Method called from '{0}'".format_with(methodName)));
            }

            var combinedPath = Platform.get_platform() == PlatformType.Windows ? leftItem : leftItem.Replace('\\', '/');
            foreach (var rightItem in rightItems)
            {
                if (rightItem.Contains(":")) throw new ApplicationException("Cannot combine a path with ':' attempted to combine '{0}' with '{1}'".format_with(rightItem, combinedPath));

                var rightSide = Platform.get_platform() == PlatformType.Windows ? rightItem : rightItem.Replace('\\', '/');
                if (rightSide.StartsWith(Path.DirectorySeparatorChar.to_string()) || rightSide.StartsWith(Path.AltDirectorySeparatorChar.to_string()))
                {
                    combinedPath = Path.Combine(combinedPath, rightSide.Substring(1));
                }
                else
                {
                    combinedPath = Path.Combine(combinedPath, rightSide);
                }
            }

            return combinedPath;
        }

        public string get_full_path(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;

            try
            {
                return Path.GetFullPath(path);
            }
            catch (IOException)
            {
                return System_IO.Path.GetFullPath(path);
            }
        }

        public string get_temp_path()
        {
            var path = Path.GetTempPath();

            if (System.Environment.UserName.contains(ApplicationParameters.Environment.SystemUserName) || path.contains("config\\systemprofile"))
            {
                path = System.Environment.ExpandEnvironmentVariables(System.Environment.GetEnvironmentVariable(ApplicationParameters.Environment.Temp, EnvironmentVariableTarget.Machine).to_string());
            }

            return path;
        }

        public char get_path_directory_separator_char()
        {
            return Path.DirectorySeparatorChar;
        }

        public char get_path_separator()
        {
            return Path.PathSeparator;
        }

        public string get_executable_path(string executableName)
        {
            if (string.IsNullOrWhiteSpace(executableName)) return string.Empty;

            var isWindows = Platform.get_platform() == PlatformType.Windows;
            IList<string> extensions = new List<string>();

            if (get_file_name_without_extension(executableName).is_equal_to(executableName) && isWindows)
            {
                var pathExtensions = Environment.GetEnvironmentVariable(ApplicationParameters.Environment.PathExtensions).to_string().Split(new[] { ApplicationParameters.Environment.EnvironmentSeparator }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var extension in pathExtensions.or_empty_list_if_null())
                {
                    extensions.Add(extension.StartsWith(".") ? extension : ".{0}".format_with(extension));
                }
            }

            // Always add empty, for when the executable name is enough.
            extensions.Add(string.Empty);

            // Gets the path to an executable based on looking in current 
            // working directory, next to the running process, then among the
            // derivatives of Path and Pathext variables, applied in order.
            var searchPaths = new List<string>();
            searchPaths.Add(get_current_directory());
            searchPaths.Add(get_directory_name(get_current_assembly_path()));
            searchPaths.AddRange(Environment.GetEnvironmentVariable(ApplicationParameters.Environment.Path).to_string().Split(new[] { get_path_separator() }, StringSplitOptions.RemoveEmptyEntries));

            foreach (var path in searchPaths.or_empty_list_if_null())
            {
                foreach (var extension in extensions.or_empty_list_if_null())
                {
                    var possiblePath = combine_paths(path, "{0}{1}".format_with(executableName, extension.to_lower()));
                    if (file_exists(possiblePath)) return possiblePath;
                }
            }

            // If not found, return the same as passed in - it may work, 
            // but possibly not.
            return executableName;
        }

        public string get_current_assembly_path()
        {
            return Assembly.GetExecutingAssembly().CodeBase.Replace(Platform.get_platform() == PlatformType.Windows ? "file:///" : "file://", string.Empty);
        }

        #endregion

        #region File

        public abstract IEnumerable<string> get_files(string directoryPath, string pattern = "*.*", SearchOption option = SearchOption.TopDirectoryOnly);
        public abstract IEnumerable<string> get_files(string directoryPath, string[] extensions, SearchOption option = SearchOption.TopDirectoryOnly);
        public abstract bool file_exists(string filePath);
        public string get_file_name(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public string get_file_name_without_extension(string filePath)
        {
            if (Platform.get_platform() == PlatformType.Windows) return Path.GetFileNameWithoutExtension(filePath);

            return Path.GetFileNameWithoutExtension(filePath.Replace('\\', '/'));
        }

        public string get_file_extension(string filePath)
        {
            if (Platform.get_platform() == PlatformType.Windows) return Path.GetExtension(filePath);

            return Path.GetExtension(filePath.Replace('\\', '/'));
        }

        public abstract dynamic get_file_info_for(string filePath);
        public abstract System.DateTime get_file_modified_date(string filePath);

        public abstract long get_file_size(string filePath);

        public abstract string get_file_version_for(string filePath);
        public abstract bool is_system_file(dynamic file);
        public abstract bool is_readonly_file(dynamic file);
        public abstract bool is_hidden_file(dynamic file);

        public bool is_encrypted_file(dynamic file)
        {
            bool isEncrypted = ((file.Attributes & FileAttributes.Encrypted) == FileAttributes.Encrypted);
            string fullName = file.FullName;
            this.Log().Debug(ChocolateyLoggers.Verbose, () => "Is file \"{0}\" an encrypted file? {1}".format_with(fullName, isEncrypted.to_string()));
            return isEncrypted;
        }

        public string get_file_date(dynamic file)
        {
            return file.CreationTime < file.LastWriteTime
                       ? file.CreationTime.Date.ToString("yyyyMMdd")
                       : file.LastWriteTime.Date.ToString("yyyyMMdd");
        }

        public abstract void move_file(string filePath, string newFilePath);
        public abstract void copy_file(string sourceFilePath, string destinationFilePath, bool overwriteExisting);
        public abstract bool copy_file_unsafe(string sourceFilePath, string destinationFilePath, bool overwriteExisting);
        public abstract void replace_file(string sourceFilePath, string destinationFilePath, string backupFilePath);
        public abstract void delete_file(string filePath);
        public abstract FileStream create_file(string filePath);
        public abstract string read_file(string filePath);
        public abstract byte[] read_file_bytes(string filePath);
        public abstract FileStream open_file_readonly(string filePath);
        public abstract FileStream open_file_exclusive(string filePath);
        public abstract void write_file(string filePath, string fileText);
        public abstract void write_file(string filePath, string fileText, Encoding encoding);
        public abstract void write_file(string filePath, Func<Stream> getStream);
        #endregion

        #region Directory

        public abstract string get_current_directory();
        public abstract IEnumerable<string> get_directories(string directoryPath);
        public abstract IEnumerable<string> get_directories(string directoryPath, string pattern, SearchOption option = SearchOption.TopDirectoryOnly);
        public abstract bool directory_exists(string directoryPath);
        
        public string get_directory_name(string filePath)
        {
            if (Platform.get_platform() != PlatformType.Windows && !string.IsNullOrWhiteSpace(filePath))
            {
                filePath = filePath.Replace('\\', '/');
            }

            try
            {
                return Path.GetDirectoryName(filePath);
            }
            catch (IOException)
            {
                return System_IO.Path.GetDirectoryName(filePath);
            }
        }

        public abstract dynamic get_directory_info_for(string directoryPath);
        public abstract dynamic get_directory_info_from_file_path(string filePath);
        public abstract void create_directory(string directoryPath);
        public abstract void move_directory(string directoryPath, string newDirectoryPath);
        public abstract void copy_directory(string sourceDirectoryPath, string destinationDirectoryPath, bool overwriteExisting);
        public abstract void create_directory_if_not_exists(string directoryPath);
        public abstract void create_directory_if_not_exists(string directoryPath, bool ignoreError);
        public abstract void delete_directory(string directoryPath, bool recursive);
        public abstract void delete_directory(string directoryPath, bool recursive, bool overrideAttributes);
        public abstract void delete_directory(string directoryPath, bool recursive, bool overrideAttributes, bool isSilent);
        public abstract void delete_directory_if_exists(string directoryPath, bool recursive);
        public abstract void delete_directory_if_exists(string directoryPath, bool recursive, bool overrideAttributes);
        public abstract void delete_directory_if_exists(string directoryPath, bool recursive, bool overrideAttributes, bool isSilent);

        #endregion

        public abstract void ensure_file_attribute_set(string path, FileAttributes attributes);
        public abstract void ensure_file_attribute_removed(string path, FileAttributes attributes);
        public abstract Encoding get_file_encoding(string filePath);
    }
}