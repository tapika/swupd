using chocolatey.infrastructure.app.domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace NuGet
{
    public class RegistryPackage : IPackage
    {
        public RegistryPackage()
        {
            Listed = true;
        }

        public string Id
        {
            get;
            set;
        }

        public SemanticVersion Version
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public IEnumerable<string> Authors
        {
            get;
            set;
        }

        public IEnumerable<string> Owners
        {
            get;
            set;
        }

        public Uri IconUrl
        {
            get;
            set;
        }

        public Uri LicenseUrl
        {
            get;
            set;
        }

        public Uri ProjectUrl
        {
            get;
            set;
        }
        
        public bool RequireLicenseAcceptance
        {
            get;
            set;
        }

        public bool DevelopmentDependency
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public string Summary
        {
            get;
            set;
        }

        public string ReleaseNotes
        {
            get;
            set;
        }

        public string Language
        {
            get;
            set;
        }

        public string Tags
        {
            get;
            set;
        }

        public List<Authoring.Tag> TagsExtra
        {
            get;
            set;
        }

        public Uri ProjectSourceUrl { get; set; }
        public Uri PackageSourceUrl { get; set; }
        public Uri DocsUrl { get; set; }
        public Uri WikiUrl { get; set; }
        public Uri MailingListUrl { get; set; }
        public Uri BugTrackerUrl { get; set; }
        public IEnumerable<string> Replaces { get; set; }
        public IEnumerable<string> Provides { get; set; }
        public IEnumerable<string> Conflicts { get; set; }

        public string SoftwareDisplayName { get; set; }
        public string SoftwareDisplayVersion { get; set; }

        public Version MinClientVersion
        {
            get;
            private set;
        }

        public bool IsAbsoluteLatestVersion
        {
            get
            {
                return true;
            }
        }

        public bool IsLatestVersion
        {
            get
            {
                return this.IsReleaseVersion();
            }
        }

        public bool Listed
        {
            get;
            set;
        }

        public DateTimeOffset? Published
        {
            get;
            set;
        }

        public string Copyright
        {
            get;
            set;
        }

        public IEnumerable<PackageDependencySet> DependencySets
        {
            get;
            set;
        }

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies
        {
            get;
            set;
        }

        public IEnumerable<IPackageAssemblyReference> AssemblyReferences
        {
            get
            {
                return Enumerable.Empty<IPackageAssemblyReference>();
            }
        }

        public ICollection<PackageReferenceSet> PackageAssemblyReferences
        {
            get;
            private set;
        }

        #region Server Metadata Only
        public string PackageHash
        {
            get
            {
                return string.Empty;
            }
        }

        public string PackageHashAlgorithm
        {
            get
            {
                return string.Empty;
            }
        }

        public long PackageSize
        {
            get
            {
                return -1;
            }
        }

        public Uri ReportAbuseUrl
        {
            get
            {
                return null;
            }
        }

        public int DownloadCount
        {
            get
            {
                return -1;
            }
        }

        public int VersionDownloadCount
        {
            get
            {
                return -1;
            }
        }

        public bool IsApproved
        {
            get
            {
                return false;
            }
        }

        public string PackageStatus
        {
            get
            {
                return "Unknown";
            }
        }

        public string PackageSubmittedStatus
        {
            get
            {
                return "Unknown";
            }
        }

        public string PackageTestResultStatus
        {
            get
            {
                return "Unknown";
            }
        }

        public DateTime? PackageTestResultStatusDate
        {
            get
            {
                return null;
            }
        }

        public string PackageValidationResultStatus
        {
            get
            {
                return "Unknown";
            }
        }

        public DateTime? PackageValidationResultDate
        {
            get
            {
                return null;
            }
        }

        public DateTime? PackageCleanupResultDate
        {
            get
            {
                return null;
            }
        }

        public DateTime? PackageReviewedDate
        {
            get
            {
                return null;
            }
        }

        public DateTime? PackageApprovedDate
        {
            get
            {
                return null;
            }
        }

        public string PackageReviewer
        {
            get
            {
                return string.Empty;
            }
        }

        public bool IsDownloadCacheAvailable
        {
            get
            {
                return false;
            }
        }

        public DateTime? DownloadCacheDate
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<DownloadCache> DownloadCache
        {
            get
            {
                return Enumerable.Empty<DownloadCache>();
            }
        }
        #endregion

        public bool IsPinned { get; set; }
        public RegistryApplicationKey RegistryKey { get; set; }

        public virtual IEnumerable<FrameworkName> GetSupportedFrameworks()
        {
            return FrameworkAssemblies.SelectMany(f => f.SupportedFrameworks).Distinct();
        }

        public IEnumerable<IPackageFile> GetFiles()
        {
            return Enumerable.Empty<IPackageFile>();
        }

        public void OverrideOriginalVersion(SemanticVersion version)
        {
            if (version != null) Version = version;
        }

        public Stream GetStream()
        {
            throw new NotImplementedException();
        }

        public void ExtractContents(IFileSystem fileSystem, string extractPath)
        {
            throw new NotImplementedException();
        }
    }
}