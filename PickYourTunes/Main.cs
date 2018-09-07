using GTA;
using GTA.Native;
using NAudio.Wave;
using PickYourTunes.Properties;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

namespace PickYourTunes
{
    public class PickYourTunes : Script
    {
        /// <summary>
        /// The mod configuration.
        /// </summary>
        ScriptSettings Config = ScriptSettings.Load("scripts\\PickYourTunes.ini");
        /// <summary>
        /// The location where our sounds are loaded.
        /// Usually <GTA V>\scripts\PickYourTunes
        /// </summary>
        string SongLocation = new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "PickYourTunes")).LocalPath;
        /// <summary>
        /// Our instance of WaveOutEvent that plays our custom files.
        /// </summary>
        WaveOutEvent OutputDevice = new WaveOutEvent();
        /// <summary>
        /// The file that is currently playing.
        /// </summary>
        AudioFileReader CurrentFile;

        public PickYourTunes()
        {
            // Patch our locale so we don't have the "coma vs dot" problem
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            // Set our events for the script and player
            Tick += OnTick;
            Tick += Cheats.OnCheat;
            OutputDevice.PlaybackStopped += OnStop;
            
            // Set the volume to the configuration value
            OutputDevice.Volume = Config.GetValue("General", "Volume", 0.2f);

            // Check that the directory with our scripts exists
            // If not, create it
            if (!Directory.Exists(SongLocation))
            {
                Directory.CreateDirectory(SongLocation);
            }
        }

        private void OnTick(object Sender, EventArgs Args)
        {
            // Just a hack recommended by "Slick" on the 5mods server to keep the radio disabled
            // "I made my own by setting the radio per tick, not the best way but hey it works"
            if (OutputDevice.PlaybackState == PlaybackState.Playing && Game.Player.Character.CurrentVehicle != null)
            {
                Function.Call(Hash.SET_VEH_RADIO_STATION, Game.Player.Character.CurrentVehicle, "OFF");
            }

            // If the game is paused OR the engine is not running AND the audio is not stopped
            // Pause it, because is running and we are not in a vehicle
            if ((Game.IsPaused || !Checks.IsEngineRunning()) && OutputDevice.PlaybackState != PlaybackState.Stopped)
            {
                OutputDevice.Pause();
            }
            // If the statement above didn't worked (not paused but on a running vehicle)
            // Resume the playback
            else if (OutputDevice.PlaybackState == PlaybackState.Paused)
            {
                OutputDevice.Play();
            }
            // If none of the above worked out, there is nothing playing nor loaded
            // Load the configuration value and check what is going on
            else
            {
                // Store the vehicle that the player is getting into
                Vehicle PlayerCar = Game.Player.Character.GetVehicleIsTryingToEnter();
                // Store our radio ID
                int RadioID = Config.GetValue("Vehicles", PlayerCar.Model.GetHashCode().ToString(), 256);
                // Store our radio name
                string RadioName = Function.Call<string>(Hash.GET_RADIO_STATION_NAME, RadioID);
                // Store our custom song
                string Song = Config.GetValue("Songs", PlayerCar.Model.GetHashCode().ToString(), string.Empty);
                
                // If there is a song requested and the music is stopped, play it
                if (Song != string.Empty && OutputDevice.PlaybackState == PlaybackState.Stopped)
                {
                    if (!File.Exists(Path.Combine(SongLocation, Song)))
                    {
                        UI.Notify(string.Format(Resources.FileWarning, Song));
                        return;
                    }

                    // Store our current file
                    CurrentFile = new AudioFileReader(Path.Combine(SongLocation, Song));
                    // Initialize it
                    OutputDevice.Init(CurrentFile);
                    // And play it
                    OutputDevice.Play();
                }
                // Else if our default value is not 256 (aka invalid or not added), do what we should
                else if (RadioID != 256)
                {
                    // Turn on the vehicle radio
                    Function.Call(Hash.SET_VEHICLE_RADIO_ENABLED, true);
                    // And set the requested radio
                    Function.Call(Hash.SET_VEH_RADIO_STATION, PlayerCar, RadioName);
                }
            }
        }

        private void OnStop(object Sender, StoppedEventArgs Args)
        {
            // if the current file exists
            if (CurrentFile != null)
            {
                // Dispose it
                CurrentFile.Dispose();
                // And remove it
                CurrentFile = null;
            }
        }
    }
}
