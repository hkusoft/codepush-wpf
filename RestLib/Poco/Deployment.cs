namespace RestLib.helper
{
    public class Deployment
    {
        public string name { get; set; }
        public string key { get; set; }
        public Release latest_release { get; set; }

        public override string ToString()
        {
            return name;
        }
    }
}