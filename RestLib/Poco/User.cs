namespace RestLib.helper
{
    public class User
    {
        public string id { get; set; }
        public string display_name { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public object avatar_url { get; set; }
        public bool can_change_password { get; set; }
        public string created_at { get; set; }
        public string origin { get; set; }
    }
}
