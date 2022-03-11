using System.Threading;

namespace chocolatey.infrastructure.app
{
    public class ApplicationManager
    {
        static public AsyncLocal<ApplicationManager> _instance = new AsyncLocal<ApplicationManager>();

        /// <summary>
        /// Returns true if ApplicationManager.Instance was initialized.
        /// </summary>
        static public bool IsInitialized()
        {
            return _instance.Value != null;
        }

        /// <summary>
        /// Application manager instance
        /// </summary>
        static public ApplicationManager Instance
        {
            get
            {
                if (_instance.Value == null)
                {
                    _instance.Value = new ApplicationManager();
                }

                return _instance.Value;
            }

            set
            {
                _instance.Value = value;
            }
        }
        
        public ApplicationManager()
        {
            //LogService = LogService.GetInstance(!ApplicationParameters.runningUnitTesting);
            //Locations = new InstallContext();
        }

        /// <summary>
        /// In long term perspective it's planned to either remove SimpleInjector container or
        /// replace it with Caliburn.Micro.
        /// 
        /// See following articles:
        ///     https://www.palmmedia.de/Blog/2011/8/30/ioc-container-benchmark-performance-comparison
        ///     https://github.com/Caliburn-Micro/Caliburn.Micro/issues/795
        /// </summary>
        public SimpleInjector.Container Container { get; set; }

        //LogService LogService { get; set; }
        //InstallContext Locations { get; set; }
    }
}
