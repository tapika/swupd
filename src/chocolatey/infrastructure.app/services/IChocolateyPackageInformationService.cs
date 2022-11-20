// Copyright © 2017 - 2021 Chocolatey Software, Inc
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

namespace chocolatey.infrastructure.app.services
{
    using domain;
    using NuGet;

    public interface IChocolateyPackageInformationService
    {
        /// <summary>
        /// Gets package information
        /// </summary>
        /// <param name="package">package to get information on</param>
        /// <param name="installDir">where package was installed or updated, null otherwise</param>
        ChocolateyPackageInformation get_package_information(IPackage package, string installDir);
        ChocolateyPackageInformation get_package_information(IPackage package);

        /// <summary>
        /// Saves package information into .install_info folder.
        /// </summary>
        /// <param name="installDir">can specify install path if package was just installed. Provide null otherwise.</param>
        void save_package_information(ChocolateyPackageInformation packageInformation, string installDir);
        void remove_package_information(IPackage package);
    }
}
