using Microsoft.Win32;

namespace chocolatey.infrastructure.app.services
{
    public class RegistryKeyInfo
    {
        public RegistryKey key;
        public RegistryHive hive;
        public RegistryView view;
        public string SubKeyName;
    }
}
