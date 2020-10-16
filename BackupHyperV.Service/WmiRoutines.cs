using System.Management;

namespace BackupHyperV.Service
{
    internal static class WmiRoutines
    {
        internal static readonly string NAMESPACE_CIMV2 = @"root\cimv2";
        internal static readonly string NAMESPACE_HYPER_V = @"root\virtualization\v2";

        internal static ManagementScope GetScope(string wmiNamespace)
        {
            return new ManagementScope($"\\\\localhost\\{wmiNamespace}");
        }

        internal static ManagementObjectCollection WmiQuery(ManagementScope scope, string query)
        {
            ManagementObjectCollection result = null;
            ObjectQuery objQuery = new ObjectQuery(query);

            scope.Connect();

            using (var searcher = new ManagementObjectSearcher(scope, objQuery))
            {
                result = searcher.Get();
            }

            return result;
        }

        internal static bool IsFeatureInstalled(string featureName)
        {
            string query = $"select * from Win32_ServerFeature where Name = '{featureName}'";

            var scope = GetScope(NAMESPACE_CIMV2);
            var data = WmiQuery(scope, query);

            return (data != null && data.Count > 0);
        }

        internal static ManagementObject GetServiceObject(ManagementScope scope, string serviceName)
        {
            scope.Connect();

            var wmiPath = new ManagementPath(serviceName);
            var serviceClass = new ManagementClass(scope, wmiPath, null);
            var services = serviceClass.GetInstances();

            return GetFirstObjectFromCollection(services);
        }

        internal static ManagementObject GetFirstObjectFromCollection(ManagementObjectCollection collection)
        {
            if (collection == null || collection.Count == 0)
                return null;

            foreach (ManagementObject managementObject in collection)
                return managementObject;

            return null;
        }
    }
}
