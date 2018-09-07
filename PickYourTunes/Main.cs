using GTA;
using GTA.Native;
using NAudio.Wave;
using System;
using System.IO;
using System.Reflection;

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
            // Set our events for the script and player
            Tick += OnTick;
            Tick += Cheats.OnCheat;
            OutputDevice.PlaybackStopped += OnStop;
            
            OutputDevice.Volume = 0.5f;

            // Check that the directory with our scripts exists
            // If not, create it
            if (!Directory.Exists(SongLocation))
            {
                Directory.CreateDirectory(SongLocation);
            }
        }

        private void OnTick(object Sender, EventArgs Args)
        {
            // Pause the playback if the user is not on a running vehicle
            if (Game.IsPaused || !Checks.IsEngineRunning())
            {
                OutputDevice.Pause();
            }
            // And restore it if things change
            else if (OutputDevice.PlaybackState == PlaybackState.Paused)
            {
                OutputDevice.Play();
            }

            // Now, do the real work
            // Check if the player is getting in a vehicle, if it does
            if (Game.Player.Character.IsGettingIntoAVehicle)
            {
                // Store the vehicle that the player is getting into
                Vehicle PlayerCar = Game.Player.Character.GetVehicleIsTryingToEnter();
                // Store our radio ID
                int RadioID = Config.GetValue("Vehicles", PlayerCar.Model.GetHashCode().ToString(), 256);
                // Store our radio name
                string RadioName = Function.Call<string>(Hash.GET_RADIO_STATION_NAME, RadioID);
                // Store our custom song
                string Song = Config.GetValue("Songs", PlayerCar.Model.GetHashCode().ToString(), string.Empty);
                
                // If there is a default sound file requested, play it
                if (Song != string.Empty && OutputDevice.PlaybackState == PlaybackState.Stopped)
                {
                    // Store our current file
                    CurrentFile = new AudioFileReader(Path.Combine(SongLocation, Song));
                    // Initialize it
                    OutputDevice.Init(CurrentFile);
                    // Disable the radio
                    Function.Call(Hash.SET_VEHICLE_RADIO_ENABLED, Game.Player.Character.CurrentVehicle, false);
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
            // Enable the radio
            // Function.Call(Hash.SET_VEHICLE_RADIO_ENABLED, Game.Player.Character.CurrentVehicle, true);

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
