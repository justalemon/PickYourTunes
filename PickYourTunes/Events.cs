using GTA;
using GTA.Native;
using NAudio.Wave;
using PickYourTunes.Items;
using PickYourTunes.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace PickYourTunes
{
    public partial class PickYourTunes : Script
    {
        /// <summary>
        /// If the Radio system is paused.
        /// </summary>
        private bool Paused = false;
        /// <summary>
        /// The current radio being used on a vehicle.
        /// </summary>
        private Dictionary<Vehicle, Radio> CurrentRadio = new Dictionary<Vehicle, Radio>();

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
                PlayRadio(Next);
            }
        }

        private void OnTickSelect(object Sender, EventArgs Args)
        {
            // Get if the player is leaving the vehicle
            bool IsExitingVehicle = Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, Game.Player.Character, 2);
            bool IsEnteringVehicle = Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, Game.Player.Character, 160);
            Vehicle CurrentVehicle = Game.Player.Character.CurrentVehicle;
            
            // If there is a vehicle but it does not has a radio stored
            if (CurrentVehicle != null && !CurrentRadio.ContainsKey(CurrentVehicle))
            {
                // See if the User has configured a radio by trying to get the values
                Default Custom = DefaultStations.Find(X => X.Hash == CurrentVehicle.Model.Hash);

                // If there is an entry on the custom radios and is more than one
                if (Custom != null && Custom.Radios.Count < 0)
                {
                    // Try to get a custom radio
                    int RandomRadio = Randomizer.Next(Custom.Radios.Count);
                    Radio CustomRadio = Radios.Find(X => X.UUID == Custom.Radios[RandomRadio]);

                    // If the radio is not valid, play a random radio
                    if (CustomRadio == null)
                    {
                        CurrentRadio[CurrentVehicle] = GetRandomRadio();
                    }
                    // Otherwise, use the one that we have
                    else
                    {
                        CurrentRadio[CurrentVehicle] = CustomRadio;
                    }
                }
                // If there is no custom radio, information, use a random one
                else
                {
                    CurrentRadio[CurrentVehicle] = GetRandomRadio();
                }

                // Finally, play the radio
                PlayRadio(CurrentRadio[CurrentVehicle]);
            }
            // If there is a radio stored but the current radio does not match
            if (CurrentVehicle != null && CurrentRadio[CurrentVehicle] != Selected)
            {
                // Set the current radio as selected
                CurrentRadio[CurrentVehicle] = Selected;
            }
            // If the player is not on a vehicle and the selected radio is not OFF, or is leaving the vehicle, or the game is paused
            else if ((CurrentVehicle == null && Selected != Radios[0]) || IsExitingVehicle || Game.IsPaused)
            {
                // Set the radio as off but don't store it
                PlayRadio(Radios[0], false);
                // And mark the radio as paused
                Paused = true;
            }
            // If the player is on a vehicle and the radio is paused and is the player is not exiting the vehicle
            else if (CurrentVehicle != null && Paused && !IsExitingVehicle)
            {
                // If the player is on a vehicle
                PlayRadio(Selected);
                Paused = false;
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
            // If the user is not on a vehicle or the system is paused, return
            if (Game.Player.Character.CurrentVehicle == null || Paused)
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

        private void OnTickDefaultRadio(object Sender, EventArgs Args)
        {
            // Iterate over the vehicles on the map
            foreach (Vehicle WorldVehicle in World.GetAllVehicles())
            {
                // If the vehicle is not from the player
                if (Game.Player.Character.CurrentVehicle != WorldVehicle)
                {
                    // Set the radio as off
                    WorldVehicle.RadioStation = RadioStation.RadioOff;
                }
            }
        }
    }
}
