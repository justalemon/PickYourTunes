using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace PickYourTunes.Items
{
    /// <summary>
    /// Configuration data for the radios.
    /// </summary>
    public class ConfigFile
    {
        /// <summary>
        /// The name of the configuration file.
        /// </summary>
        [DefaultValue("Unknown")]
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Name { get; set; }
        /// <summary>
        /// The author of the config.
        /// </summary>
        [DefaultValue("Unknown")]
        [JsonProperty("author", DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Author { get; set; }
        /// <summary>
        /// The list of radios added by the config.
        /// </summary>
        [JsonProperty("radios")]
        public List<Radio> Radios { get; set; }
    }
}
