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

namespace chocolatey.infrastructure.registration
{
    using System;
    using app;
    using app.registration;
    using SimpleInjector;

    /// <summary>
    ///   The inversion container
    /// </summary>
    public static class SimpleInjectorContainer
    {
        private static readonly Lazy<Container> _container = new Lazy<Container>(initialize);

#if DEBUG
        private static bool _verifyContainer = true;
#else
        private static bool _verifyContainer = false;
#endif
        
        public static bool VerifyContainer { 
            get { return _verifyContainer; }
            set { _verifyContainer = value; } 
        }

        /// <summary>
        ///   Gets the container.
        /// </summary>
        public static Container Container { get { return _container.Value; } }

        /// <summary>
        ///   Initializes the container
        /// </summary>
        private static Container initialize()
        {
            var container = new Container();
            ApplicationManager.Instance.Container = container;
            container.Options.AllowOverridingRegistrations = true;
            var originalConstructorResolutionBehavior = container.Options.ConstructorResolutionBehavior;
            container.Options.ConstructorResolutionBehavior = new SimpleInjectorContainerResolutionBehavior(originalConstructorResolutionBehavior);

            var binding = new ContainerBinding();
            binding.RegisterComponents(container);

            if (_verifyContainer) container.Verify();
            return container;
        }
    }
}
