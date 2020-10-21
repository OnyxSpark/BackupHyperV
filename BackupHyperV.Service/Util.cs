using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace BackupHyperV.Service
{
    internal static class Util
    {
        internal static string GetDomainFQDN()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            return (properties != null && properties.DomainName != null) ? properties.DomainName : null;
        }

        internal static string GetCurrentServerFQDN()
        {
            string srv = Environment.MachineName.ToLower();
            string domain = GetDomainFQDN();

            if (string.IsNullOrWhiteSpace(domain))
                return srv;

            return $"{srv}.{domain}";
        }

        internal static List<string> GetLocalVirtualMachines()
        {
            var vmNames = new List<string>();

            var scope = WmiRoutines.GetScope(WmiRoutines.NAMESPACE_HYPER_V);
            var data = WmiRoutines.WmiQuery(scope, "SELECT * FROM MSVM_ComputerSystem WHERE Caption != 'Hosting Computer System'");

            if (data != null && data.Count > 0)
            {
                foreach (var item in data)
                {
                    string vmName = item["ElementName"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(vmName))
                    {
                        vmNames.Add(vmName);
                    }
                }
            }

            return vmNames;
        }
    }
}
