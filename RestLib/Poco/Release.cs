using System;
using System.ComponentModel;

namespace RestLib.helper
{
    public class Release
    {
        //[DisplayName("Test Property")]
        //[Description("This is the description that shows up")]
        [DisplayName("Release Binary Size")]         
        public string target_binary_range { get; set; }

        [DisplayName("Release URL")]
        public string blob_url { get; set; }

        [DisplayName("Description of the release")]
        public string description { get; set; }

        [DisplayName("Enabled Status")]
        public bool is_disabled { get; set; }

        [DisplayName("Mandatory")]
        public bool is_mandatory { get; set; }

        [DisplayName("Release Label")]
        public string label { get; set; }

        [DisplayName("Release Hash")]
        public string hash { get; set; }

        [DisplayName("Released By")]
        public string released_by { get; set; }

        [DisplayName("Release Method")]
        public string release_method { get; set; }

        [DisplayName("Number of Rollout Users")]
        public int rollout { get; set; }

        [DisplayName("Release Size")]
        public int size { get; set; }

        [DisplayName("Update Time")]
        public long upload_time { get; set; }

        public override string ToString()
        {
            return label;
        }

        public bool IsEnabled
        {
            get { return !is_disabled; }
            set
            {
                is_disabled = !value;
            }
        }

        public string UploadTime
        {
            get
            {                
                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime date = start.AddMilliseconds(upload_time).ToLocalTime();
                return String.Format("{0:g}", date);
                //return date.ToString(@"h \\h m \\m");
            }
        }
    }
}