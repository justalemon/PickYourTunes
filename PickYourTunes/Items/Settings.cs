using Newtonsoft.Json;

namespace PickYourTunes.Items
{
    public class Settings
    {
        /// <summary>
        /// The volume used for the 
        /// </summary>
        [JsonProperty("volume")]
        public float Volume { get; set; }
    }
}
