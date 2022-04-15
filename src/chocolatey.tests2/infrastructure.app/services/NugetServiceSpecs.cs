using chocolatey.infrastructure.app;
using chocolatey.infrastructure.app.configuration;
using chocolatey.infrastructure.app.domain;
using chocolatey.infrastructure.app.services;
using chocolatey.infrastructure.filesystem;
using logtesting;
using Moq;
using NuGet;
using System.Collections.Generic;
using System.IO;
using IFileSystem = chocolatey.infrastructure.filesystem.IFileSystem;

namespace chocolatey.tests2.infrastructure.app.services
{
    class NugetServiceSpecs: LogTesting
    {
        NugetService service;
        Mock<IFilesService> filesService = new Mock<IFilesService>();
        Mock<DotNetFileSystemBase> fileSystem = new Mock<DotNetFileSystemBase>().Logged(
            // Query like functions about which we are not intrested.
            nameof(IFileSystem.get_file_extension),
            nameof(IFileSystem.get_file_name),
            nameof(IFileSystem.get_files),
            nameof(IFileSystem.get_directory_name),
            nameof(IFileSystem.get_current_directory),
            nameof(IFileSystem.combine_paths),
            nameof(IFileSystem.directory_exists),
            nameof(IFileSystem.file_exists)
        );

        public NugetServiceSpecs()
        {
            service = new NugetService(
                new Mock<IRegistryService>().Object,
                fileSystem.Object,
                new Mock<IChocolateyPackageInformationService>().Object, filesService.Object, 
                new Mock<IPackageDownloader>().Object
            );
        }

        [LogTest()]
        public void TestBackupRemove()
        {
            InstallContext.Instance.RootLocation = Path.Combine("c:","choco");
            string filePath = Path.Combine("c:", "choco", "lib", "somelib", "license.txt");
            string fileConfPath = Path.Combine("c:", "choco", "lib", "somelib", "myconf.xml");
            string filePath2 = Path.Combine("c:", "choco", "lib", "somelib", "license2.txt");
            
            RegistryPackage package = new RegistryPackage();
            package.Id = "Bob";
            package.Version = new SemanticVersion(1,2,3,4);
            var packageInfo = new ChocolateyPackageInformation(package);
            var packageFile = new PackageFile { Path = filePath, Checksum = "1234" };
            var packageFiles = new PackageFiles()
            {
                Files = new List<PackageFile>
                {
                    packageFile
                }
            };
            packageInfo.FilesSnapshot = packageFiles;

            var fileSystemFiles = new [] { filePath };
            var config = new ChocolateyConfiguration();

            fileSystem.Setup(x => x.get_files(It.IsAny<string>(), It.IsAny<string>(), SearchOption.AllDirectories)).Returns(fileSystemFiles);
            filesService.Setup(x => x.capture_package_files(It.IsAny<string>(), config)).Returns(packageFiles);

            using (new LogScope("should_ignore_an_unchanged_file"))
            { 
                service.backup_changed_files(filePath, config, packageInfo);
            }

            var updatedPackageFiles = new PackageFiles()
            {
                Files = new List<PackageFile>
                    {
                        new PackageFile { Path = filePath, Checksum = "4321" }
                    }
            };

            using (new LogScope("should_backup_a_changed_file"))
            {
                filesService.Setup(x => x.capture_package_files(It.IsAny<string>(), config)).Returns(updatedPackageFiles);
                service.backup_changed_files(filePath, config, packageInfo);
            }

            using (new LogScope("if_snapshot_not_available_should_backup_files_in_folder")) //new case
            {
                var confFiles = new[] { fileConfPath };
                fileSystem.Setup(x => x.get_files(It.IsAny<string>(), It.IsAny<string[]>(), SearchOption.AllDirectories)).Returns(confFiles);
                packageInfo.FilesSnapshot = null;
                service.backup_changed_files(filePath, config, packageInfo);
            }

            using (new LogScope("should_do_nothing_if_the_directory_no_longer_exists"))
            {
                fileSystem.Setup(x => x.directory_exists(It.IsAny<string>())).Returns(false);
                filesService.Setup(x => x.get_package_file(It.IsAny<string>())).Returns(packageFile);
                service.remove_installation_files(package, packageInfo);
            }

            using (new LogScope("should_remove_an_unchanged_file"))
            {
                fileSystem.Setup(x => x.directory_exists(It.IsAny<string>())).Returns(true);
                fileSystem.Setup(x => x.get_files(It.IsAny<string>(), It.IsAny<string>(), SearchOption.AllDirectories)).Returns(fileSystemFiles);
                fileSystem.Setup(x => x.file_exists(filePath)).Returns(true);
                service.remove_installation_files(package, packageInfo);
            }

            using (new LogScope("should_not_delete_a_changed_file"))
            {
                filesService.Setup(x => x.get_package_file(It.IsAny<string>())).Returns(updatedPackageFiles.Files[0]);
                service.remove_installation_files(package, packageInfo);
            }

            using (new LogScope("should_not_delete_an_unfound_file"))
            {
                var packageFileNotInOriginal = new PackageFile { Path = filePath2, Checksum = "4321" };
                filesService.Setup(x => x.get_package_file(It.IsAny<string>())).Returns(packageFileNotInOriginal);
                service.remove_installation_files(package, packageInfo);
            }

            using (new LogScope("generated_package_should_be_in_current_directory"))
            {
                fileSystem.Setup(x => x.get_current_directory()).Returns("c:/choco");
                service.pack_noop(config);
            }

            using (new LogScope("generated_package_should_be_in_specified_directory"))
            {
                config.OutputDirectory = "c:/packages";
                service.pack_noop(config);
            }
        }
    }
}
