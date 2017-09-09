using RestLib;
using RestLib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace codepush_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<CodePushApp> apps = new List<CodePushApp>();
        User user = new User();

        //Key is the App, each app has N deployments such as Staging and Production        
        Dictionary<CodePushApp, List<Deployment>> all_deployments = new Dictionary<CodePushApp, List<Deployment>>();
        Dictionary<Deployment, List<Release>> all_releases = new Dictionary<Deployment, List<Release>>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            RefreshAppList();
        }

        private void UpdateStatus(string info)
        {
            Dispatcher.BeginInvoke((Action)(() => {
                InfoView.Content = info;
            }));
        }
        async void RefreshAppList()
        {
            UpdateStatus("Login ... ");
            user = await Http.GetLoginUser();
            UserLabel.Content = user.display_name;

            UpdateStatus("Get all apps ... ");
            apps = await Http.GetAppsAsync();

            AppList.ItemsSource = apps;

            foreach (var app in apps)
            {
                //all_apps[app.name] = app;
                await RefreshDepolymentListAsync(user.display_name, app);
            }
        }

        private void SetStatus(string info)
        {
            Dispatcher.BeginInvoke((Action)(() => {
                InfoView.Content = info;
            }));
        }
        private async Task RefreshDepolymentListAsync(string owner_name, CodePushApp app)
        {
            List<Deployment> deploys = new List<Deployment>();
            deploys = await Http.GetDeploymentsAsync(owner_name, app.name);

            all_deployments[app] = deploys;
            SetStatus("Get all deployment for App " + app.display_name);
            foreach (var deployment in deploys)
                RefreshReleaseList(owner_name, app, deployment);
        }

        private async void RefreshReleaseList(string owner_name, CodePushApp app, Deployment deployment)
        {
            List<Release> releases = new List<Release>();
            UpdateStatus("Get all releases for deployment " + deployment.name);
            releases = await Http.GetReleasesAsync(owner_name, app.name, deployment.name);
            releases = releases.OrderByDescending(item => item.upload_time).ToList();
            all_releases[deployment] = releases;
        }

     

        private void AppList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppList.SelectedItems.Count > 0)
            {
                var app = AppList.SelectedItems[0] as CodePushApp;
                if (all_deployments.Count > 0 && all_deployments.ContainsKey(app))
                {
                    //var selected_app = all_apps[selected_item];
                    DeploymentList.ItemsSource = all_deployments[app];
                }
            }
        }

        void GetSelectedReleaseInfo(out Deployment d, out Release r, out CodePushApp a)
        {
            d = null;
            r = null;
            a = null;
            if (DeploymentList.SelectedItem !=null && ReleaseList.SelectedItem != null)
            {
                d = DeploymentList.SelectedItem as Deployment;
                r = ReleaseList.SelectedItem as Release;
                a = AppList.SelectedItem as CodePushApp;
            }
        }
        private void DeploymentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeploymentList.SelectedItems.Count > 0)
            {
                var selected_deployment = DeploymentList.SelectedItems[0] as Deployment;
                if (selected_deployment != null && all_releases.Count > 0 && all_releases.ContainsKey(selected_deployment))
                {
                    var selected_releases = all_releases[selected_deployment];
                    ReleaseList.ItemsSource = selected_releases;
                }
            }
        }
             
        private void UpdateReleaseButton_Click(object sender, RoutedEventArgs e)
        {
            Deployment d;
            Release r;
            CodePushApp a;
            GetSelectedReleaseInfo(out d, out r, out a);
            bool bSuccess = Http.UpdateRelease(r, user.name, a.name, d.name);
            SetStatus(bSuccess? "Success!" : "Update Failure!");
        }
    }
}
