using GTA;
using NAudio.Wave;
using System;
using System.IO;

namespace PickYourTunes
{
    public partial class PickYourTunes : Script
    {
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
                string SongFile = Next.Type == RadioType.SingleFile ? Path.Combine(DataFolder, "Radios", Next.Location) : Path.Combine(DataFolder, "Radios", Next.Location, CurrentSong[Next].File);
                if (!File.Exists(SongFile))
                {
                    UI.Notify($"Error: The file {SongFile} does not exists");
                    goto FinishChange;
                }
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
            FinishChange:
            Selected = Next;
        }
    }
}
