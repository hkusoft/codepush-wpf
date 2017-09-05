namespace RestLib.helper
{
    public class CodePushApp
    {
        public string id { get; set; }
        public string app_secret { get; set; }
        public object description { get; set; }
        public string display_name { get; set; }
        public string name { get; set; }
        public string os { get; set; }
        public string platform { get; set; }
        public string origin { get; set; }
        public object icon_url { get; set; }
        public User owner { get; set; }
        public object azure_subscription { get; set; }

        public override string ToString()
        {
            return display_name;
        }        
    }

}
