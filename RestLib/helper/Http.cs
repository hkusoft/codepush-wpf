using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using RestLib.helper;
using System.Text;
using RestSharp;
using System.Diagnostics;

namespace RestLib
{
    public static class Http
    {
        //curl -X GET --header 'Accept: application/json' --header 'X-API-Token: 40abb7297a4f02904832decd9800aa573e4b131f' 
        //'https://api.mobile.azure.com/v0.1/user'

        static RestClient client;
        const string app_token = "40abb7297a4f02904832decd9800aa573e4b131f";
        const string url_base_string = "https://api.mobile.azure.com";
        static Uri url_base = new Uri(url_base_string);        
        static Dictionary<string, string> header = new Dictionary<string, string>()
            {
                { "X-API-Token", app_token }
            };

        static Http()
        {
            client = new RestClient(url_base_string);            
        }

            /// <summary>
            /// This function fires an Http Get and get the raw resoonse in string
            /// </summary>
            /// <param name="base_url">e.g. new Uri("https://api.mobile.azure.com/"); </param>
            /// <param name="url_endpoint_path">e.g. "v0.1/user/whatever/endpoint"</param>
            /// <param name="requestHedaderInfo"> A dictionary of request header</param>
            /// <returns>
            ///     Task<string> response.
            ///     Use response.Result to get the response string, note that Accessing the property's 
            ///     get accessor (i.e. response.Result) blocks the calling thread until the asynchronous 
            ///     operation is complete; it is equivalent to calling the Wait method
            ///     See also: https://msdn.microsoft.com/en-us/library/dd321468(v=vs.110).aspx
            /// </returns>
            /// Example:
            ///     var url_base = new Uri("https://api.mobile.azure.com/");
            ///     var endpoint_path = "v0.1/user";
            ///                 
            ///     Task<string> response; 
            ///     //Accessing Result in another Task makes the UI responsive
            ///     Task t = Task.Run(() =>  
            ///     {
            ///         response = HttpGet(url_base, endpoint_path, header).Result;
            ///     });
            ///     //Get Result when the task is done
            ///     t.ContinueWith((t2) =>  
            ///     {               
            ///         Console.Writeline(response);
            ///     });
            public static async Task<string> HttpGet(string url_endpoint_path)            
            {
                var request = new RestRequest(url_endpoint_path, Method.GET);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("X-API-Token", app_token);
                var response = await client.ExecuteTaskAsync(request);                
                return response.Content;
            }

        public static IRestResponse Patch(string requestUri, object payload)
        {
            var client = new RestClient(url_base_string);
            var request = new RestRequest(requestUri, Method.PATCH);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("X-API-Token", app_token);

            request.RequestFormat = DataFormat.Json;

            //request.AddBody(new { is_disabled = true, is_mandatory = true });
            request.AddBody(payload);

            //var response = await client.ExecuteTaskAsync(request); //Does not work, no response!
            var response = client.Execute(request);
            return response;
        }
              

        public static Release UpdateRelease(Release r, string userName, string appName, string depolymentName)
        {

            //var uri = string.Format("https://api.mobile.azure.com/v0.1/apps/{0}/{1}/deployments/{2}/releases/{3}",
            var uri = string.Format("v0.1/apps/{0}/{1}/deployments/{2}/releases/{3}",
                            userName, appName, depolymentName, r.label);          

            var body = new { is_disabled = r.is_disabled, is_mandatory = r.is_mandatory };
            var response = Patch(uri,body);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Release>(response.Content);
            }
            return null;

           
        }


        public static async Task<User> GetLoginUser()
        {
            var json = await HttpGet("v0.1/user");
            var output = JsonConvert.DeserializeObject<User>(json);
            return output; 
        }
        
        public static async Task<List<CodePushApp>> GetAppsAsync()
        {
            var json =await HttpGet("v0.1/apps");
            var output = JsonConvert.DeserializeObject<List<CodePushApp>>(json);
            return output;
        }

        public static async Task<List<Deployment>> GetDeploymentsAsync(string owner_name, string app_name)
        {
            //v0.1/apps/cityuxykou/idemo/deployments'
            var path = string.Format("v0.1/apps/{0}/{1}/deployments", owner_name, app_name);
            var json = await HttpGet(path);
            var output = JsonConvert.DeserializeObject<List<Deployment>>(json);
            return output;
        }

        //GET /v0.1/apps/{owner_name}/{app_name}/deployments/{deployment_name}/releases
        public static async Task<List<Release>> GetReleasesAsync(string owner_name, string app_name, string deployment_name)
        {
            //v0.1/apps/cityuxykou/idemo/deployments'
            var path = string.Format("v0.1/apps/{0}/{1}/deployments/{2}/releases", owner_name, app_name, deployment_name);
            var json = await HttpGet(path);
            var output = JsonConvert.DeserializeObject<List<Release>>(json);
            return output;
        }
    }
}
