using GTA;
using NAudio.Wave;
using PickYourTunes.Items;
using System;
using System.IO;

namespace PickYourTunes
{
    public partial class PickYourTunes : Script
    {
        private Radio GetRandomRadio()
        {
            // Get a random radio from the list
            return Radios[Randomizer.Next(Radios.Count)];
        }

        private void PlayRadio(Radio SelectedRadio, bool Store = true, bool LoadTime = true)
        {
            // If there is a long file currently playing, store the playback status
            if (MusicOutput.PlaybackState == PlaybackState.Playing)
            {
                Progress[Selected] = MusicFile.CurrentTime;
            }

            // Stop the streaming radio and local file
            Streaming.Stop();
            MusicOutput.Stop();

            // If the chosen radio is vanilla
            if (SelectedRadio.Type == RadioType.Vanilla)
            {
                Game.RadioStation = (RadioStation)SelectedRadio.ID;
            }
            // If the radio is a single large file
            else if (SelectedRadio.Type == RadioType.SingleFile || SelectedRadio.Type == RadioType.Radio)
            {
                Game.RadioStation = RadioStation.RadioOff;
                if (MusicFile != null)
                {
                    MusicFile.Dispose();
                }
                if (SelectedRadio.Type == RadioType.Radio && !CurrentSong.ContainsKey(SelectedRadio))
                {
                    CurrentSong[SelectedRadio] = SelectedRadio.Songs[Randomizer.Next(SelectedRadio.Songs.Count)];
                }
                string SongFile = SelectedRadio.Type == RadioType.SingleFile ? Path.Combine(DataFolder, "Radios", SelectedRadio.Location) : Path.Combine(DataFolder, "Radios", SelectedRadio.Location, CurrentSong[SelectedRadio].File);
                if (!File.Exists(SongFile))
                {
                    UI.Notify($"Error: The file {SongFile} does not exists");
                    goto FinishChange;
                }
                // "The data specified for the media type is invalid, inconsistent, or not supported by this object." with MediaFoundationReader
                if (SelectedRadio.CodecFix)
                {
                    WaveFileReader TempWave = new WaveFileReader(SongFile);
                    MusicFile = WaveFormatConversionStream.CreatePcmStream(TempWave);
                }
                else
                {
                    MusicFile = new MediaFoundationReader(SongFile);
                }
                MusicOutput.Init(MusicFile);
                if (LoadTime)
                {
                    if (Progress.ContainsKey(SelectedRadio))
                    {
                        MusicFile.CurrentTime = Progress[SelectedRadio];
                    }
                    else
                    {
                        int RandomPosition = Randomizer.Next((int)MusicFile.TotalTime.TotalSeconds);
                        TimeSpan RandomTimeSpan = TimeSpan.FromSeconds(RandomPosition);
                        MusicFile.CurrentTime = RandomTimeSpan;
                    }
                }
                MusicOutput.Play();
            }
            // If the radio is a stream
            else if (SelectedRadio.Type == RadioType.Stream)
            {
                Game.RadioStation = RadioStation.RadioOff;
                Streaming.Play(SelectedRadio.Location);
            }

            // Set the next radio as the selected one if is required
            FinishChange:
            if (Store)
            {
                Selected = SelectedRadio;
            }
        }
    }
}
