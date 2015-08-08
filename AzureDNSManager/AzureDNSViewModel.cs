using Microsoft.Azure.Common.Authentication;
using Microsoft.Azure.Common.Authentication.Models;
using Microsoft.Azure.Management.Dns;
using Microsoft.Azure.Management.Dns.Models;
using Microsoft.Azure.Management.Resources;
using Microsoft.Azure.Management.Resources.Models;
using Microsoft.Azure.Subscriptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AzureDNSManager
{
    class AzureDNSViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private SubscriptionClient _subscriptionClient;
        private DnsManagementClient _dnsManagementClient;
        private ResourceManagementClient _resourceManagementClient;

        private IList<Microsoft.Azure.Management.Resources.Models.ResourceGroupExtended> _resourceGroups = null;
        public IList<Microsoft.Azure.Management.Resources.Models.ResourceGroupExtended> ResourceGroups
        {
            get { return _resourceGroups; }
            set
            {
                _resourceGroups = value;
                OnPropertyChanged();
            }
        }

        private ResourceGroupExtended _activeResourceGroup;
        public ResourceGroupExtended ActiveResourceGroup
        {
            get { return _activeResourceGroup; }
            set
            {
                _activeResourceGroup = value;
                OnPropertyChanged();
                ReloadZones();
            }
        }

        private IList<Microsoft.Azure.Subscriptions.Models.Subscription> _subscriptions = null;
        public IList<Microsoft.Azure.Subscriptions.Models.Subscription> Subscriptions
        {
            get
            {
                return _subscriptions;
            }
            set
            {
                _subscriptions = value;
                OnPropertyChanged();
            }
        }

        private string _activeSubscription = "";
        public string ActiveSubscription {
            get { return _activeSubscription;  }
            set
            {
                _activeSubscription = value;
                OnPropertyChanged();

                ReloadResourceGroups();
            }
        }

        private System.Collections.ObjectModel.ObservableCollection<Zone> _zones;
        public System.Collections.ObjectModel.ObservableCollection<Zone> Zones {
            get { return _zones; }
            set
            {
                _zones = value;
                OnPropertyChanged();
            }
        }

        private Zone _activeZone;
        public Zone ActiveZone { 
            get { return _activeZone; }
            set
            {
                _activeZone = value;
                OnPropertyChanged();

                ReloadRecords();
            }
        }

        private System.Collections.ObjectModel.ObservableCollection<RecordSet> _records;
        public System.Collections.ObjectModel.ObservableCollection<RecordSet> Records
        {
            get { return _records; }
            set
            {
                _records = value;
                OnPropertyChanged();
            }
        }

        private AzureContext _azureContext;

        public AzureDNSViewModel()
        {
            try
            {
                var azureAccount = new AzureAccount()
                {
                    Id = null,
                    Type = AzureAccount.AccountType.User,
                    Properties = new Dictionary<AzureAccount.Property, string>()
                        {
                            {
                                AzureAccount.Property.Tenants, "common"
                            }
                        }
                };
                var azureSub = new AzureSubscription()
                {
                    Account = azureAccount.Id,
                    Environment = "AzureCloud",
                    Properties = new Dictionary<AzureSubscription.Property, string>()
                    { { AzureSubscription.Property.Tenants, "common" }}

                };
                var azureEnv = AzureEnvironment.PublicEnvironments["AzureCloud"];

                _azureContext = new AzureContext(azureSub, azureAccount, azureEnv);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to setup azure account credentials!");
                Debug.WriteLine(ex.Message);
                return;
            }

            try
            {
                AzureSession.AuthenticationFactory.Authenticate(_azureContext.Account, _azureContext.Environment, "common", null, Microsoft.Azure.Common.Authentication.ShowDialog.Always);
                _azureContext.Subscription.Account = _azureContext.Account.Id;
            } catch (Exception ex)
            {
                MessageBox.Show("Failed to authenticate with Azure!");
                Debug.WriteLine(ex.Message);
                return;
            }

            try
            { 
                _subscriptionClient = AzureSession.ClientFactory.CreateClient<SubscriptionClient>(_azureContext, AzureEnvironment.Endpoint.ResourceManager);
                _subscriptions = _subscriptionClient.Subscriptions.List().Subscriptions;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName]string name = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null && !string.IsNullOrWhiteSpace(name))
                handler(this, new PropertyChangedEventArgs(name));

        }

        private async void ReloadResourceGroups()
        {
            if (ActiveSubscription?.Length > 0)
            {
                _azureContext.Subscription.Id = Guid.Parse(ActiveSubscription);
                _resourceManagementClient = AzureSession.ClientFactory.CreateClient<ResourceManagementClient>(_azureContext, AzureEnvironment.Endpoint.ResourceManager);
                ResourceGroups = (await _resourceManagementClient.ResourceGroups.ListAsync(null)).ResourceGroups;
            }
        }

        private async void ReloadZones()
        {
            if (ActiveSubscription?.Length > 0 && ActiveResourceGroup != null)
            {
                _azureContext.Subscription.Id = Guid.Parse(ActiveSubscription);
                _dnsManagementClient = AzureSession.ClientFactory.CreateClient<DnsManagementClient>(_azureContext, AzureEnvironment.Endpoint.ResourceManager);
                Zones = new System.Collections.ObjectModel.ObservableCollection<Zone>((await _dnsManagementClient.Zones.ListAsync(_activeResourceGroup.Name, null)).Zones);
            }
        }

        private async void ReloadRecords()
        {
            if (ActiveZone == null)
            {
                Records = null;
            } else
            {
                Records = new System.Collections.ObjectModel.ObservableCollection<RecordSet>((await _dnsManagementClient.RecordSets.ListAllAsync(ActiveResourceGroup.Name, ActiveZone?.Name, null)).RecordSets);
            }
        }
    }
}
