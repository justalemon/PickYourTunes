using GTA;
using GTA.Native;
using NAudio.Wave;
using Newtonsoft.Json;
using PickYourTunes.Items;
using PickYourTunes.Properties;
using PickYourTunes.Streaming;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace PickYourTunes
{
    public class PickYourTunes : Script
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
        /// The location where our sounds are loaded.
        /// Usually <GTA V>\scripts\PickYourTunes
        /// </summary>
        private string SongLocation = new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "PickYourTunes")).LocalPath;
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
                    // And add it onto the existing list of radios
                    Radios.AddRange(Config.Radios);
                    // Notify that we have loaded the file
                    UI.Notify($"List of radios loaded: {Config.Name} by {Config.Author}");
                }
            }

            // And add our events
            Tick += OnTickFixes;
            Tick += OnTickControls;
            Tick += OnTickDraw;
            Tick += Cheats.OnCheat;
            MusicOutput.PlaybackStopped += OnFileStop;
            Aborted += (Sender, Args) => { Streaming.Stop(); };

            // Enable the mobile phone radio
            Function.Call(Hash.SET_MOBILE_PHONE_RADIO_STATE, true);

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
            if (!Directory.Exists(SongLocation))
            {
                Directory.CreateDirectory(SongLocation);
            }
        }

        private void OnTickFixes(object Sender, EventArgs Args)
        {
            // Enable the mobile radio this tick
            Function.Call(Hash.SET_MOBILE_RADIO_ENABLED_DURING_GAMEPLAY, true);

            // Cheat for changing the song to the next one
            if (Function.Call<bool>(Hash._0x557E43C447E700A8, Game.GenerateHash("sr next")))
            {
                if (Selected.Type == RadioType.Radio)
                {
                    MusicFile.CurrentTime = MusicFile.TotalTime;
                }
            }
        }

        private void OnTickControls(object Sender, EventArgs Args)
        {
            // Disable the weapon wheel
            Game.DisableControlThisFrame(0, Control.VehicleRadioWheel);
            Game.DisableControlThisFrame(0, Control.VehicleNextRadio);
            Game.DisableControlThisFrame(0, Control.VehiclePrevRadio);

            // Check if a control has been pressed
            if (Game.IsDisabledControlJustPressed(0, Control.VehicleRadioWheel) || Game.IsDisabledControlJustPressed(0, Control.VehicleNextRadio))
            {
                NextRadio();
            }
        }

        private void OnTickDraw(object Sender, EventArgs Args)
        {
            // If the user is not on a vehicle, return
            if (Game.Player.Character.CurrentVehicle == null)
            {
                return;
            }

            // If there is a frequency, add it at the end like every normal radio ad
            string RadioName = Selected.Frequency == 0 ? Selected.Name : Selected.Name + " " + Selected.Frequency.ToString();

            // Draw the previous, current and next radio name
            UIText PreviousUI = new UIText(Previous.Name, new Point((int)(UI.WIDTH * .5f), (int)(UI.HEIGHT * .025f)), .5f, Color.LightGray, GTA.Font.ChaletLondon, true, true, false);
            PreviousUI.Draw();
            UIText CurrentUI = new UIText(RadioName, new Point((int)(UI.WIDTH * .5f), (int)(UI.HEIGHT * .055f)), .6f, Color.White, GTA.Font.ChaletLondon, true, true, false);
            CurrentUI.Draw();
            UIText NextUI = new UIText(Next.Name, new Point((int)(UI.WIDTH * .5f), (int)(UI.HEIGHT * .09f)), .5f, Color.LightGray, GTA.Font.ChaletLondon, true, true, false);
            NextUI.Draw();
        }

        private void OnFileStop(object Sender, StoppedEventArgs Args)
        {
            if (MusicFile.TotalTime == MusicFile.CurrentTime && Selected.Type == RadioType.Radio)
            {
                CurrentSong[Selected] = Selected.Songs[Randomizer.Next(Selected.Songs.Count)];
                MusicFile = new MediaFoundationReader(Selected.Location + "\\" + CurrentSong[Selected].File);
                MusicOutput.Init(MusicFile);
                MusicOutput.Play();
            }
        }

        private void NextRadio()
        {
            // If there is a long file currently playing, store the playback status
            if (MusicOutput.PlaybackState == PlaybackState.Playing)
            {
                Progress[Selected] = MusicFile.CurrentTime;
            }

            // Stop the streaming radio and local file
            Streaming.Stop();
            MusicOutput.Stop();

            // Is the next radio is vanilla
            if (Next.Type == RadioType.Vanilla)
            {
                Game.RadioStation = (RadioStation)Next.ID;
            }
            // If the radio is a single large file
            else if (Next.Type == RadioType.SingleFile || Next.Type == RadioType.Radio)
            {
                Game.RadioStation = RadioStation.RadioOff;
                if (MusicFile != null)
                {
                    MusicFile.Dispose();
                }
                if (Next.Type == RadioType.Radio && !CurrentSong.ContainsKey(Next))
                {
                    CurrentSong[Next] = Next.Songs[Randomizer.Next(Next.Songs.Count)];
                }
                string SongFile = Next.Type == RadioType.SingleFile ? Next.Location : Next.Location + "\\" + CurrentSong[Next].File;
                // "The data specified for the media type is invalid, inconsistent, or not supported by this object." with MediaFoundationReader
                if (Next.CodecFix)
                {
                    WaveFileReader TempWave = new WaveFileReader(SongFile);
                    MusicFile = WaveFormatConversionStream.CreatePcmStream(TempWave);
                }
                else
                {
                    MusicFile = new MediaFoundationReader(SongFile);
                }
                MusicOutput.Init(MusicFile);
                if (Progress.ContainsKey(Next))
                {
                    MusicFile.CurrentTime = Progress[Next];
                }
                else
                {
                    int RandomPosition = Randomizer.Next((int)MusicFile.TotalTime.TotalSeconds);
                    TimeSpan RandomTimeSpan = TimeSpan.FromSeconds(RandomPosition);
                    MusicFile.CurrentTime = RandomTimeSpan;
                }
                MusicOutput.Play();
            }
            // If the radio is a stream
            else if (Next.Type == RadioType.Stream)
            {
                Game.RadioStation = RadioStation.RadioOff;
                Streaming.Play(Next.Location);
            }

            // Set the next radio as the selected one
            Selected = Next;
        }
    }
}
