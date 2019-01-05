using GTA;
using GTA.Native;
using NAudio.Wave;
using PickYourTunes.Properties;
using System;
using System.Drawing;
using System.IO;

namespace PickYourTunes
{
    public partial class PickYourTunes : Script
    {
        private void OnTickCheats(object Sender, EventArgs Args)
        {
            // Change the song to the next one
            if (Function.Call<bool>(Hash._0x557E43C447E700A8, Game.GenerateHash("pyt next")))
            {
                if (Selected.Type == RadioType.Radio)
                {
                    MusicFile.CurrentTime = MusicFile.TotalTime;
                }
                else if (Selected.Type == RadioType.Vanilla)
                {
                    Function.Call(Hash.SKIP_RADIO_FORWARD);
                }
            }
            // Show the vehicle hash
            else if (Checks.CheatHasBeenEntered("pyt hash"))
            {
                UI.Notify(string.Format(Resources.CheatHash, Game.Player.Character.CurrentVehicle.Model.GetHashCode()));
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

        private void OnFileStop(object Sender, StoppedEventArgs Args)
        {
            if (MusicFile.TotalTime == MusicFile.CurrentTime && Selected.Type == RadioType.Radio)
            {
                CurrentSong[Selected] = Selected.Songs[Randomizer.Next(Selected.Songs.Count)];
                MusicFile = new MediaFoundationReader(Path.Combine(DataFolder, "Radios", Selected.Location, CurrentSong[Selected].File));
                MusicOutput.Init(MusicFile);
                MusicOutput.Play();
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
    }
}
