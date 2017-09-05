using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using RestLib.helper;

namespace RestLib
{
    public partial class Http
    {
        //curl -X GET --header 'Accept: application/json' --header 'X-API-Token: 40abb7297a4f02904832decd9800aa573e4b131f' 
        //'https://api.mobile.azure.com/v0.1/user'

        const string app_token = "40abb7297a4f02904832decd9800aa573e4b131f";
        static Uri url_base = new Uri("https://api.mobile.azure.com/");


        /// <summary>
        /// This function fires an Http Get and get the raw resoonse in string
        /// </summary>
        /// <param name="base_url">e.g. new Uri("https://api.mobile.azure.com/"); </param>
        /// <param name="url_endpoint_path">e.g. "v0.1/user/whatever/endpoint"</param>
        /// <param name="requestHedaderInfo"> A dictionary of request header</param>
        /// <returns>
        ///     Tast<string> response.
        ///     Use response.Result to get the response string, note that Accessing the property's 
        ///     get accessor (i.e. response.Result) blocks the calling thread until the asynchronous 
        ///     operation is complete; it is equivalent to calling the Wait method
        ///     See also: https://msdn.microsoft.com/en-us/library/dd321468(v=vs.110).aspx
        /// </returns>
        /// Example:
        ///     var url_base = new Uri("https://api.mobile.azure.com/");
        ///     var endpoint_path = "v0.1/user";
        ///     var header = new Dictionary<string, string>()
        ///     {
        ///         { "X-API-Token", "abcdefghhijgkfdk;slkflkorwkvokrgowko" }
        ///     };
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
        public static async Task<string> HttpGet(Uri base_url, string url_endpoint_path, 
            Dictionary<string, string> requestHedaderInfo = null)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = base_url;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                foreach (var item in requestHedaderInfo)
                {
                    client.DefaultRequestHeaders.Add(item.Key, item.Value);
                }               
                
                var response = await client.GetStringAsync(url_endpoint_path); 
                return response;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"> Endpoint path, e.g. "/Membership/exists" or "/account"</param>
        /// <returns></returns>
        public static async Task<string> HttpGet(string path)
        {
            var header = new Dictionary<string, string>()
            {
                { "X-API-Token", "40abb7297a4f02904832decd9800aa573e4b131f" }
            };
            var response = await HttpGet(url_base, path, header);
            return response;            
        }

        public static async Task<User> GetLoginUser()
        {
            var json = await HttpGet("v0.1/user");
            var output = JsonConvert.DeserializeObject<User>(json);
            return output; // !=null? output.display_name: null;
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
