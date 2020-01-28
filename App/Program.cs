using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Azure.Sql;
using Account = Pulumi.Azure.Storage.Account;
using AccountArgs = Pulumi.Azure.Storage.AccountArgs;

namespace App
{
    public static class Program
    {
        public static Task<int> Main()
        {
            return Deployment.RunAsync(() =>
            {
                // Create an Azure Resource Group
                var resourceGroup = new ResourceGroup("resourceGroup", new ResourceGroupArgs
                {
                    Location = "westeurope"
                });

                // Create an Azure Storage Account
                var storageAccount = new Account("storageAccount", new AccountArgs
                {
                    ResourceGroupName = resourceGroup.Name,
                    AccountReplicationType = "LRS",
                    AccountTier = "Standard"
                });

                // Create an Azure Sql Server
                var sqlServer = new SqlServer("sqlServer", new SqlServerArgs
                {
                    AdministratorLogin = "AdminLoginDemo1234",
                    AdministratorLoginPassword = "AdminPasswordDemo1234",
                    ResourceGroupName = resourceGroup.Name,
                    Version = "12.0"
                });

                // Create an Azure Sql Database
                var sqlDatabase = new Database("sqlDatabase", new DatabaseArgs
                {
                    ResourceGroupName = resourceGroup.Name,
                    ServerName = sqlServer.Name,
                    Edition = "Basic"
                });

                // Create an Azure Firewall Rule
                var currentIpAddress = FindCurrentIpAddress();
                var firewallRule = new FirewallRule("firewallRule", new FirewallRuleArgs
                {
                    ResourceGroupName = resourceGroup.Name,
                    ServerName = sqlServer.Name,
                    StartIpAddress = currentIpAddress,
                    EndIpAddress = currentIpAddress
                });

                // Export the connection string for the storage account
                return new Dictionary<string, object>
                {
                    { "resourceGroupName", resourceGroup.Name },
                    { "storageAccountName", storageAccount.Name },
                    { "sqlServerName", sqlServer.Name },
                    { "sqlDatabaseName", sqlDatabase.Name },
                    { "firewallRuleName", firewallRule.Name }
                };
            });
        }

        private static string FindCurrentIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                ?.ToString();
        }
    }
}
