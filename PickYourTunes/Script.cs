using GTA;
using NAudio.Wave;
using Newtonsoft.Json;
using PickYourTunes.Items;
using PickYourTunes.Streaming;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace PickYourTunes
{
    public partial class PickYourTunes : Script
    {
        /// <summary>
        /// Player for MP3 streams.
        /// </summary>
        private StreamPlayer Streaming = new StreamPlayer();
        /// <summary>
        /// List of radios added by the user.
        /// </summary>
        private List<Radio> Radios = new List<Radio>
        {
            new Radio()
            {
                Name = "Radio Off",
                Frequency = 0,
                Type = RadioType.Vanilla,
                ID = 255
            }
        };
        /// <summary>
        /// The current selected radio by the user.
        /// </summary>
        private Radio Selected = null;
        /// <summary>
        /// The output device for music files.
        /// </summary>
        private WaveOutEvent MusicOutput = new WaveOutEvent();
        /// <summary>
        /// The output device for radio announcements.
        /// </summary>
        private WaveOutEvent AdsOutput = new WaveOutEvent();
        /// <summary>
        /// The current local file.
        /// </summary>
        private WaveStream MusicFile = null;
        /// <summary>
        /// The stored progress for the radios.
        /// </summary>
        private Dictionary<Radio, TimeSpan> Progress = new Dictionary<Radio, TimeSpan>();
        /// <summary>
        /// The current song for the radios that allow it.
        /// </summary>
        private Dictionary<Radio, Song> CurrentSong = new Dictionary<Radio, Song>();
        /// <summary>
        /// A random number generator.
        /// </summary>
        private Random Randomizer = new Random();
        /// <summary>
        /// The mod configuration.
        /// </summary>
        private ScriptSettings Config = ScriptSettings.Load("scripts\\PickYourTunes.ini");
        /// <summary>
        /// The location where our data is located.
        /// Usually <GTA V>\scripts\PickYourTunes
        /// </summary>
        private string DataFolder = new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "PickYourTunes")).LocalPath;
        /// <summary>
        /// Our instance of WaveOutEvent that plays our custom files.
        /// </summary>
        private WaveOutEvent OutputDevice = new WaveOutEvent();
        /// <summary>
        /// The file that is currently playing.
        /// </summary>
        private AudioFileReader CurrentFile;
        /// <summary>
        /// The vehicle that the player was using previously.
        /// </summary>
        private int PreviousVehicle;

        /// <summary>
        /// The previous radio.
        /// </summary>
        public Radio Previous
        {
            get
            {
                // Get the index of the current radio
                int CurrentIndex = Radios.IndexOf(Selected);
                // Get the index of the previous item
                int CorrectIndex = CurrentIndex == 0 ? Radios.Count - 1 : CurrentIndex - 1;
                // Return the correct item
                return Radios[CorrectIndex];
            }
        }
        /// <summary>
        /// The next radio.
        /// </summary>
        public Radio Next
        {
            get
            {
                // Get the index of the current radio
                int CurrentIndex = Radios.IndexOf(Selected);
                // Get the index of the next item
                int CorrectIndex = CurrentIndex == Radios.Count - 1 ? 0 : CurrentIndex + 1;
                // Return the correct item
                return Radios[CorrectIndex];
            }
        }

        public PickYourTunes()
        {
            // Patch our locale so we don't have the "coma vs dot" problem
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            // Open the JSON files for reading
            foreach (string File in Directory.GetFiles("scripts\\PickYourTunes\\Radios", "*.json"))
            {
                // Open the JSON files for reading
                using (StreamReader Reader = new StreamReader(File))
                {
                    // Read the file content
                    string JSON = Reader.ReadToEnd();
                    // Parse it
                    ConfigFile Config = JsonConvert.DeserializeObject<ConfigFile>(JSON);
                    // Iterate over the list of radios
                    foreach (Radio NewRadio in Config.Radios)
                    {
                        // If the UUID is valid, add the radio
                        if (Guid.TryParse(NewRadio.UUID, out _))
                        {
                            Radios.Add(NewRadio);
                        }
                        // If not, notify the user about it
                        else
                        {
                            UI.Notify($"Warning: The radio '{NewRadio.Name}' was not added because it does not has a valid UUID.");
                        }
                    }
                    // Notify that we have loaded the file
                    UI.Notify($"List of radios loaded: {Config.Name} by {Config.Author}");
                }
            }

            // And add our events
            Tick += OnTickCheats;
            Tick += OnTickControls;
            Tick += OnTickDraw;
            MusicOutput.PlaybackStopped += OnFileStop;
            Aborted += (Sender, Args) => { Streaming.Stop(); };

            // Set the selected radio as off, just in case
            Selected = Radios[0];
            Game.RadioStation = RadioStation.RadioOff;

            // Order the radios by frequency
            Radios = Radios.OrderBy(X => X.Frequency).ToList();

            // Show the count of radios to the user
            UI.Notify($"Radios available: {Radios.Count}");
            
            // Set the volume to the configuration value
            OutputDevice.Volume = Config.GetValue("General", "Volume", 0.2f);

            // Check that the directory with our scripts exists
            // If not, create it
            if (!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }
        }
    }
}
