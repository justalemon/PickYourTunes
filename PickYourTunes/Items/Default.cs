using Newtonsoft.Json;
using System.Collections.Generic;

namespace PickYourTunes.Items
{
    public class Default
    {
        /// <summary>
        /// The Model Hash for the Vehicle.
        /// </summary>
        [JsonProperty("hash")]
        public int Hash { get; set; }
        /// <summary>
        /// The UUID's for the default radios.
        /// </summary>
        [JsonProperty("radios")]
        public List<string> Radios { get; set; }
    }
}
