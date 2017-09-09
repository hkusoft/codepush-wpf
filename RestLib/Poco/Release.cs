using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;

namespace RestLib.helper
{
    public class Release
    {
        public string target_binary_range { get; set; }
        public string blob_url { get; set; }
        public string description { get; set; }
        public bool is_disabled { get; set; }
        public bool is_mandatory { get; set; }
        public string label { get; set; }
        public string hash { get; set; }
        public string released_by { get; set; }
        public string release_method { get; set; }
        public int rollout { get; set; }
        public int size { get; set; }
        public long upload_time { get; set; }

        public override string ToString()
        {
            //return JsonConvert.SerializeObject(this);  --> { "is_disabled" = true, "is_mandatory" = true });
            //Below                                      --> { is_disabled = true, is_mandatory = true });
            var serializer = new JsonSerializer();
            var stringWriter = new StringWriter();
            using (var writer = new JsonTextWriter(stringWriter))
            {
                writer.QuoteName = false;
                serializer.Serialize(writer, this);
                return stringWriter.ToString();
            }
            return null;
        }

        public bool ShouldSerializetarget_binary_range() { return false; }
        public bool ShouldSerializeblob_url() { return false; }
        public bool ShouldSerializedescription() { return false; }
        public bool ShouldSerializeis_disabled() { return true; }
        public bool ShouldSerializeis_mandatory() { return true; }
        public bool ShouldSerializelabel() { return false; }
        public bool ShouldSerializehash() { return false; }
        public bool ShouldSerializereleased_by() { return false; }
        public bool ShouldSerializerelease_method() { return false; }
        public bool ShouldSerializerollout() { return false; }
        public bool ShouldSerializesize() { return false; }
        public bool ShouldSerializeupload_time() { return false; }

        [JsonIgnore]
        public bool IsEnabled
        {
            get { return !is_disabled; }
            set
            {
                is_disabled = !value;
            }
        }

        [JsonIgnore]
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

        [JsonIgnore]
        public string SizeInKb
        {
            get
            {
                return string.Format("{0} KB", size / 1024);
            }
        }

        [JsonIgnore]
        public string RolloutPercent
        {
            get
            {
                return string.Format("{0} %", rollout);
            }
        }
    }
}