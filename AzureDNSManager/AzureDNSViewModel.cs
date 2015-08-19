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
using System.Windows.Input;

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

        public ICommand AddZoneCommand { get; set; }
        public ICommand AddRecordCommand { get; set; }
        public ICommand DeleteRecordCommand { get; set; }
        public ICommand CommitRecordCommand { get; set; }
        public ICommand AddRecordEntryCommand { get; set; }


        public AzureDNSViewModel()
        {
            AddZoneCommand = new Command(this.AddZone);
            AddRecordCommand = new Command(this.AddRecord);
            DeleteRecordCommand = new Command(this.DeleteRecord);
            CommitRecordCommand = new Command(this.CommitRecord);
            AddRecordEntryCommand = new Command(this.AddRecordEntry);

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
                IAccessToken token = AzureSession.AuthenticationFactory.Authenticate(_azureContext.Account, _azureContext.Environment, "common", null, Microsoft.Azure.Common.Authentication.ShowDialog.Auto);
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
                if (_subscriptions?.Count == 1)
                {
                    ActiveSubscription = _subscriptions[0].SubscriptionId;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        protected async void AddRecord(object param)
        {
            RecordSet rs = new RecordSet("global");
            string typeString = param.ToString();
            InputDialog dlg;
            switch (typeString)
            {
                case "A":
                    dlg = new InputDialog("Add A record", "Enter the 'name' of the record", "@");
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Name = dlg.Value;
                    } else
                    {
                        return;
                    }
                    dlg = new InputDialog("Add A record", "Enter the 'target' for the record", "127.0.0.1");
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Properties = new RecordSetProperties(600);
                        rs.Properties.ARecords = new List<ARecord>();
                        rs.Properties.ARecords.Add(new ARecord(dlg.Value));
                    }
                    else
                    {
                        return;
                    }
                    rs.Type = "Microsoft.Network/dnszones/A";
                    break;
                case "AAAA":
                    dlg = new InputDialog("Add AAAA record", "Enter the 'name' of the record", "@");
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Name = dlg.Value;
                    }
                    else
                    {
                        return;
                    }
                    dlg = new InputDialog("Add AAAA record", "Enter the 'target' for the record", "::1");
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Properties = new RecordSetProperties(600);
                        rs.Properties.AaaaRecords = new List<AaaaRecord>();
                        rs.Properties.AaaaRecords.Add(new AaaaRecord(dlg.Value));
                    }
                    else
                    {
                        return;
                    }
                    rs.Type = "Microsoft.Network/dnszones/AAAA";
                    break;
                case "CNAME":
                    dlg = new InputDialog("Add CNAME record", "Enter the 'name' of the record", "@");
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Name = dlg.Value;
                    }
                    else
                    {
                        return;
                    }
                    dlg = new InputDialog("Add CNAME record", "Enter the 'target' for the record", ActiveZone.Name);
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Properties = new RecordSetProperties(600);
                        rs.Properties.CnameRecord = new CnameRecord(dlg.Value);
                    }
                    else
                    {
                        return;
                    }
                    rs.Type = "Microsoft.Network/dnszones/CNAME";
                    break;
                case "MX":
                    rs.Name = "@";
                    dlg = new InputDialog("Add MX record", "Enter the 'preference / priority' of the record", "10");
                    rs.Properties = new RecordSetProperties(600);
                    rs.Properties.MxRecords = new List<MxRecord>();
                    rs.Properties.MxRecords.Add(new MxRecord());
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Properties.MxRecords[0].Preference = ushort.Parse(dlg.Value);
                    }
                    else
                    {
                        return;
                    }
                    dlg = new InputDialog("Add MX record", "Enter the 'target / exchange' for the record", "mx1." + ActiveZone.Name);
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Properties.MxRecords[0].Exchange = dlg.Value;
                    }
                    else
                    {
                        return;
                    }
                    rs.Type = "Microsoft.Network/dnszones/MX";
                    break;
                case "SRV":
                    dlg = new InputDialog("Add SRV record", "Enter the 'name/origin' of the record", "@");
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Name = dlg.Value;
                    }
                    else
                    {
                        return;
                    }
                    rs.Properties = new RecordSetProperties(600);
                    rs.Properties.SrvRecords = new List<SrvRecord>();
                    rs.Properties.SrvRecords.Add(new SrvRecord());

                    dlg = new InputDialog("Add SRV record", "Enter the 'priority' for the record", "100");
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Properties.SrvRecords[0].Priority = ushort.Parse(dlg.Value);
                    }
                    else
                    {
                        return;
                    }

                    dlg = new InputDialog("Add SRV record", "Enter the 'weight' for the record", "1");
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Properties.SrvRecords[0].Weight = ushort.Parse(dlg.Value);
                    }
                    else
                    {
                        return;
                    }

                    dlg = new InputDialog("Add SRV record", "Enter the 'port' for the record", "443");
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Properties.SrvRecords[0].Port = ushort.Parse(dlg.Value);
                    }
                    else
                    {
                        return;
                    }
                    dlg = new InputDialog("Add SRV record", "Enter the 'target' for the record", ActiveZone.Name);
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Properties.SrvRecords[0].Target = dlg.Value;
                    }
                    else
                    {
                        return;
                    }
                    rs.Type = "Microsoft.Network/dnszones/SRV";
                    break;
                case "TXT":
                    dlg = new InputDialog("Add TXT record", "Enter the 'name/origin' of the record", "@");
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Name = dlg.Value;
                    }
                    else
                    {
                        return;
                    }
                    dlg = new InputDialog("Add TXT record", "Enter the 'target/value' for the record", ActiveZone.Name);
                    if (dlg.ShowDialog() == true)
                    {
                        rs.Properties = new RecordSetProperties(600);
                        rs.Properties.TxtRecords = new List<TxtRecord>();
                        rs.Properties.TxtRecords.Add(new TxtRecord(dlg.Value));
                    }
                    else
                    {
                        return;
                    }
                    rs.Type = "Microsoft.Network/dnszones/TXT";
                    break;
                default:
                    return;
            }
            try
            {
                await _dnsManagementClient.RecordSets.CreateOrUpdateAsync(ActiveResourceGroup.Name, ActiveZone.Name, rs.Name, GetRecordType(rs.Type), new RecordSetCreateOrUpdateParameters(rs));
                ReloadRecords();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to add new record. " + ex.Message);
            }
        }

        protected void AddRecordEntry(object param)
        {
            RecordSet rs = param as RecordSet;
            if (rs != null)
            {
                switch (GetRecordType(rs.Type))
                {
                    case RecordType.A:
                        rs.Properties?.ARecords.Add(new ARecord());
                        break;
                    case RecordType.AAAA:
                        rs.Properties?.AaaaRecords.Add(new AaaaRecord());
                        break;
                    case RecordType.MX:
                        rs.Properties?.MxRecords.Add(new MxRecord());
                        break;
                    case RecordType.TXT:
                        rs.Properties?.TxtRecords.Add(new TxtRecord());
                        break;
                    case RecordType.SRV:
                        rs.Properties?.SrvRecords.Add(new SrvRecord());
                        break;
                }
            }
            var r = new List<RecordSet>(Records);
            Records.Clear();
            Records = new System.Collections.ObjectModel.ObservableCollection<RecordSet>(r);
        }

        protected async void CommitRecord(object param)
        {
            RecordSet rs = param as RecordSet;
            if (rs != null)
            {
                await _dnsManagementClient.RecordSets.CreateOrUpdateAsync(ActiveResourceGroup.Name, ActiveZone.Name, rs.Name, GetRecordType(rs.Type), new RecordSetCreateOrUpdateParameters(rs));
                ReloadRecords();
            }
        }

        protected  RecordType GetRecordType(string type)
        {
            switch(type)
            {
                case "Microsoft.Network/dnszones/A":
                    return RecordType.A;
                case "Microsoft.Network/dnszones/AAAA":
                    return RecordType.AAAA;
                case "Microsoft.Network/dnszones/CNAME":
                    return RecordType.CNAME;
                case "Microsoft.Network/dnszones/MX":
                    return RecordType.MX;
                case "Microsoft.Network/dnszones/PTR":
                    return RecordType.PTR;
                case "Microsoft.Network/dnszones/SRV":
                    return RecordType.SRV;
                case "Microsoft.Network/dnszones/TXT":
                    return RecordType.TXT;
            }
            throw new ArgumentException("Not a valid record type to be updated!");
        }
        protected async void DeleteRecord(object param)
        {

        }

        protected async void AddZone()
        {
            InputDialog input = new InputDialog("Enter zone name", "Enter the name of the DNS zone to add", "");
            if (ActiveResourceGroup != null && input.ShowDialog() == true)
            {
                ZoneCreateOrUpdateParameters p = new ZoneCreateOrUpdateParameters();
                p.Zone = new Zone("global");
                p.Zone.Properties = new ZoneProperties();
                try
                {
                    ZoneCreateOrUpdateResponse responseCreateZone = await _dnsManagementClient.Zones.CreateOrUpdateAsync(ActiveResourceGroup.Name, input.Value, p);
                    ReloadZones();
                } catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    MessageBox.Show("Failed to add zone name!");
                }
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
                if (ResourceGroups?.Count == 1)
                {
                    ActiveResourceGroup = ResourceGroups[0];
                }
            }
        }

        private async void ReloadZones()
        {
            if (ActiveSubscription?.Length > 0 && ActiveResourceGroup != null)
            {
                _azureContext.Subscription.Id = Guid.Parse(ActiveSubscription);
                _dnsManagementClient = AzureSession.ClientFactory.CreateClient<DnsManagementClient>(_azureContext, AzureEnvironment.Endpoint.ResourceManager);
                Zones = new System.Collections.ObjectModel.ObservableCollection<Zone>((await _dnsManagementClient.Zones.ListAsync(_activeResourceGroup.Name, null)).Zones);
                if (Zones?.Count > 0)
                {
                    this.ActiveZone = Zones[0];
                }
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
