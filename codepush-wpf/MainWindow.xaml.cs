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

        //const string app_token = "40abb7297a4f02904832decd9800aa573e4b131f";
        //const string app_token = "18f73dde601c4fa3538c309941438461672d02df";
        //string app_token = "a6d65eed1a887f86d525137de0f6c9581bfc9fda";    //dch.it.pro
        string app_token;

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
            Init();
        }

        void Init(bool IsLoginInProgress = true)
        {
          

            UserLabel.Content = "";
            AppList.ItemsSource = null;
            DeploymentList.ItemsSource = null;
            ReleaseList.ItemsSource = null;

            RefreshReleasesButton.IsEnabled = false;

            app_token = Properties.Settings.Default.AppToken;
            if (Properties.Settings.Default.RememberMe && !string.IsNullOrEmpty(app_token))
            {
                if (IsLoginInProgress)
                    UpdateStatus("Login now ...");

                Http.Init(app_token);
                RefreshAll(IsLoginInProgress);
            }            
        }

        
        private async Task GetAppsAsync()
        {            
            UpdateStatus("Get all apps ... ");
            apps = await Http.GetAppsAsync();

            AppList.ItemsSource = apps;
            if (apps == null)
            {
                UpdateStatus("No apps found for your account. Make sure you have sufficient access rights.");
                return;
            }
        }

        private async Task GetActiveUser()
        {
            AppList.IsEnabled = false;
            DeploymentList.IsEnabled = false;
            ReleaseList.IsEnabled = false;

            //UpdateStatus("Login ... ");
            user = await Http.GetLoginUser();
            UserLabel.Content = user.name;

        }

        private async Task<List<Deployment>> GetDeploymentsAsync(CodePushApp app)
        {
            UpdateStatus("Get all deployment for App --> " + app.display_name);
            var output = await Http.GetDeploymentsAsync(app.owner.name, app.name);
            if (output == null)
            {
                UpdateStatus(string.Format("Cannot get any deployment for the app {0}", app.name));
                return null;
            }
            all_deployments[app] = output;           

            return output;
        }
     
        private async void RefreshReleases_Click(object sender, RoutedEventArgs e)
        {
            var app = AppList.SelectedItem as CodePushApp;
            if(app == null)
            {
                UpdateStatus("Select an App first");
                return;
            }
            var d = DeploymentList.SelectedItem as Deployment;
            if(d == null)
            {

                UpdateStatus("Select an deployment first");
                return;
            }

            ReleaseList.ItemsSource = null;
                        
            await GetReleaseListAsync(app, d);
            await GetReleaseMetricAsync(app, d);

            ReleaseList.ItemsSource = all_releases[d];
        }

        async void RefreshAll(bool IsLoginInProgress = true)
        {
            await GetActiveUser();            
            await GetAppsAsync();
            if (apps == null)
            {
                UpdateStatus(IsLoginInProgress? "Incorrect login credentials. Contact admin." : "Logout success");
                return;
            }
            foreach (var app in apps)
            {
                var deploys = await GetDeploymentsAsync(app);                
                foreach (Deployment d in deploys)
                {
                    await GetReleaseListAsync(app, d);
                    await GetReleaseMetricAsync(app, d);
                }                
            }
            UpdateStatus("Successfully fetched all app info.");

            AppList.IsEnabled = true;
            DeploymentList.IsEnabled = true;
            ReleaseList.IsEnabled = true;
            RefreshReleasesButton.IsEnabled = true;

            if (AppList.Items.Count > 0)
                AppList.SelectedIndex = 0;

        }   
        
        private async Task<List<ReleaseMetric>> GetReleaseMetricAsync(CodePushApp app, Deployment deployment)
        {
            UpdateStatus("Get all releases metrics for deployment --> " + deployment.name);
            var metricList = await Http.GetReleaseMetricAsync(app.owner.name, app.name, deployment.name);
            if (metricList == null)
            {
                UpdateStatus(string.Format("Cannot get any release metric for the deployment {0}", deployment.name));
                return null;
            }
            all_release_metrics[deployment] = metricList;
            
            if (metricList == null)
                UpdateStatus(string.Format("Cannot get any release metric for the deployment {0}", deployment.name));

            RefreshReleaseMetrics(deployment, all_releases[deployment]);

            return metricList;
        }

        private async Task<List<Release>> GetReleaseListAsync(CodePushApp app, Deployment deployment)
        {            
            UpdateStatus("Get all releases for deployment --> " + deployment.name);
            var releaseList = await Http.GetReleasesAsync(app.owner.name, app.name, deployment.name);
            if (releaseList == null)
            {
                UpdateStatus("Failed to get any releases for deployment: " + deployment.name);
                return null;
            }
            releaseList = releaseList.OrderByDescending(item => item.upload_time).ToList();
            all_releases[deployment] = releaseList;

                   
            return releaseList;
        }


        private void RefreshReleaseMetrics(Deployment d, List<Release> releases)
        {
            if (!all_release_metrics.ContainsKey(d))
            {
                UpdateStatus("Failed to get any releases metric for deployment: " + d.name);
                return;
            }
            var release_metrics = all_release_metrics[d];
            foreach (var item in releases)
            {
                var metric = release_metrics.FirstOrDefault(rm => rm.label == item.label);
                if (metric == null)
                {
                    UpdateStatus("Failed to get any releases metric for deployment: " + item.label);
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
                this.Cursor = Cursors.Wait;
                var UpdatedRelease = Http.UpdateRelease(CurrentRelease, CurrentApp.owner.name, CurrentApp.name, CurrentDeployment.name);
                if (UpdatedRelease == null)
                {
                    UpdateStatus(string.Format("Failed to update the Mandatory Status for {0}", CurrentRelease.label));
                    return;
                }

                Dispatcher.BeginInvoke((Action)(() =>
                {
                    UpdateStatus(tb.Name == "EnableToggleButton" ?
                    string.Format("Success! Release {0} now {1}", UpdatedRelease.label, UpdatedRelease.is_disabled ? "DISABLED" : "ENABLED") :
                    string.Format("Success! Release {0} now is {1}", UpdatedRelease.label, UpdatedRelease.is_mandatory ? "MANDATORY" : "NON-MANDATORY")
                    );
                }));
                this.Cursor = Cursors.Arrow;
            }
        }
        
        DateTime lastModifiedTime;
        private void UpdateStatus(string info)
        {
            lastModifiedTime = DateTime.Now;
            Dispatcher.BeginInvoke((Action)(async () =>
            {
                InfoView.Content = info;
                await Task.Delay(TimeSpan.FromSeconds(3));
                var span = DateTime.Now - lastModifiedTime;
                if(span.TotalSeconds >=3)
                    InfoView.Content = "";
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginOutContextMenu.PlacementTarget = sender as Button;
            LoginOutContextMenu.IsOpen = true;

        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(UserLabel.Content as string))
            {
                Properties.Settings.Default.AppToken = "";
                Properties.Settings.Default.Save();
                Init(false); //Retry login to clear everything
            }
            else
                UpdateStatus("Already logout!");
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UserLabel.Content as string))
            {
                UserInfo window = new UserInfo();
                window.Owner = this;
                if (window.ShowDialog() == true)
                {
                    Init();
                }
            }
            else
                UpdateStatus("Already logined!");
        }      
    }
}
