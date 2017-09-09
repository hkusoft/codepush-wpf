namespace RestLib
{
    public class ReleaseMetric
    {
        public int active { get; set; }
        public int installed { get; set; }
        public int downloaded { get; set; }
        public int failed { get; set; }
        public string label { get; set; }

    }
}