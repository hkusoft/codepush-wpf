using RestLib;
using RestLib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace codepush_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<CodePushApp> apps = new List<CodePushApp>();
        User user = new User();

        //The current selected App is the AppList
        CodePushApp CurrentApp;
        Deployment CurrentDeployment;        


        //Key is the App, each app has N deployments such as Staging and Production        
        Dictionary<CodePushApp, List<Deployment>> all_deployments = new Dictionary<CodePushApp, List<Deployment>>();
        Dictionary<Deployment, List<Release>> all_releases = new Dictionary<Deployment, List<Release>>();
        Dictionary<Deployment, List<ReleaseMetric>> all_release_metrics = new Dictionary<Deployment, List<ReleaseMetric>>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            RefreshAppList();
        }

       
        async void RefreshAppList()
        {
            AppList.IsEnabled = false;
            DeploymentList.IsEnabled = false;
            ReleaseList.IsEnabled = false;

            UpdateStatus("Login ... ");
            user = await Http.GetLoginUser();            

            UserLabel.Content = "User Name:" + user.name;

            UpdateStatus("Get all apps ... ");
            apps = await Http.GetAppsAsync();

            AppList.ItemsSource = apps;
            if(apps == null)
            {
                SetStatus("Failed to get any apps from your account. Make sure you have sufficient access rights.");
                return;
            }
            foreach (var app in apps)
            {
                //all_apps[app.name] = app;
                var deploys = await RefreshDepolymentListAsync(app.owner.name, app);
                if (deploys == null)
                {
                    SetStatus("Failed to get any deployment for your app: " + app.name);
                    return;
                }
            }

            UpdateStatus("Successfully fetched all app info.");

            AppList.IsEnabled = true;
            DeploymentList.IsEnabled = true;
            ReleaseList.IsEnabled = true;
            if (AppList.Items.Count > 0)
                AppList.SelectedIndex = 0;


        }


        private async Task<List<Deployment>> RefreshDepolymentListAsync(string owner_name, CodePushApp app)
        {
            List<Deployment> deploys = new List<Deployment>();
            deploys = await Http.GetDeploymentsAsync(app.owner.name, app.name);
            if(deploys == null)
            {
                SetStatus(string.Format("Cannot get any deployment for the app {0}", app.name));
                return null;
            }

            all_deployments[app] = deploys;
            SetStatus("Get all deployment for App --> " + app.display_name);
            foreach (var deployment in deploys)
            {
                var metricList = await ScanReleaseMetricAsync(app, deployment);
                if (metricList == null)
                {
                    SetStatus(string.Format("Cannot get any release metric for the deployment {0}", deployment.name));
                    return null;
                }

                var releaseList = await RefreshReleaseList(app, deployment);
                if(releaseList == null)
                {
                    SetStatus(string.Format("Cannot get any releases for the deployment {0}", deployment.name));
                    return null;
                }
                
                
            }
            return deploys;
        }

        private async Task<List<ReleaseMetric>> ScanReleaseMetricAsync(CodePushApp app, Deployment deployment)
        {            
            UpdateStatus("Get all releases metrics for deployment --> " + deployment.name);
            var metrics = await Http.GetReleaseMetricAsync(app.owner.name, app.name, deployment.name);
            if(metrics == null)
            {
                SetStatus(string.Format("Cannot get any release metric for the deployment {0}", deployment.name));
                return null;
            }
            all_release_metrics[deployment] = metrics;
            return metrics;
        }

        private async Task<List<Release>> RefreshReleaseList(CodePushApp app, Deployment deployment)
        {
            List<Release> releases = new List<Release>();
            UpdateStatus("Get all releases for deployment --> " + deployment.name);
            releases = await Http.GetReleasesAsync(app.owner.name, app.name, deployment.name);
            if(releases == null)
            {
                SetStatus("Failed to get any releases for deployment: " + deployment.name);
                return null;
            }
            releases = releases.OrderByDescending(item => item.upload_time).ToList();
            all_releases[deployment] = releases;

            RefreshReleaseMetrics(deployment, all_releases[deployment]);
            return releases;
        }

        private void RefreshReleaseMetrics(Deployment d, List<Release> releases)
        {
            if (!all_release_metrics.ContainsKey(d))
            {
                SetStatus("Failed to get any releases metric for deployment: " + d.name);
                return;
            }
            var release_metrics = all_release_metrics[d];
            foreach (var item in releases)
            {
                var metric = release_metrics.FirstOrDefault(rm => rm.label == item.label);
                if(metric ==null)
                {
                    SetStatus("Failed to get any releases metric for deployment: " + item.label);
                    return;
                }
                item.Metric = metric;
            }
        }

        private void AppList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppList.SelectedItems.Count > 0)
            {
                CurrentApp = AppList.SelectedItems[0] as CodePushApp;
                if (all_deployments.Count > 0 && all_deployments.ContainsKey(CurrentApp))
                {
                    //var selected_app = all_apps[selected_item];
                    DeploymentList.ItemsSource = all_deployments[CurrentApp];
                }
            }
        }
               
        private void DeploymentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeploymentList.SelectedItems.Count > 0)
            {
                CurrentDeployment = DeploymentList.SelectedItems[0] as Deployment;
                if (CurrentDeployment != null && all_releases.Count > 0 && all_releases.ContainsKey(CurrentDeployment))
                {
                    var selected_releases = all_releases[CurrentDeployment];
                    ReleaseList.ItemsSource = selected_releases;                   
                }
            }
        }

        //When the release enable | mandatory toggle button changed
        private void OnReleaseToggleChanged(object sender, RoutedEventArgs e)
        {
            var tb = (ToggleButton)sender;
            if (tb == null)
                return;

            var CurrentRelease = tb.DataContext as Release;
            if (CurrentApp != null && CurrentDeployment != null && CurrentRelease != null)
            {
                //Task.Run(() =>
                //{                   
                this.Cursor = Cursors.Wait;
                    var UpdatedRelease = Http.UpdateRelease(CurrentRelease, CurrentApp.owner.name, CurrentApp.name, CurrentDeployment.name);
                    if (UpdatedRelease == null)
                    {
                        SetStatus(string.Format("Failed to update the Mandatory Status for {0}", CurrentRelease.label));
                        return;
                    }

                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        SetStatus(tb.Name == "EnableToggleButton" ?
                        string.Format("Success! Release {0} now {1}", UpdatedRelease.label, UpdatedRelease.is_disabled ? "DISABLED" : "ENABLED"):
                        string.Format("Success! Release {0} now is {1}", UpdatedRelease.label, UpdatedRelease.is_mandatory ? "MANDATORY" : "NON-MANDATORY") 
                        );
                    }));
                this.Cursor = Cursors.Arrow;

                //});

                
            }
        }

        private void SetStatus(string info)
        {
            Dispatcher.BeginInvoke((Action)(() => {
                InfoView.Content = info;
            }));
        }
        private void UpdateStatus(string info)
        {
            Dispatcher.BeginInvoke((Action)(() => {
                InfoView.Content = info;
            }));
        }

   
    }
}
