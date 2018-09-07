using GTA;
using GTA.Native;
using PickYourTunes.Properties;
using System;

namespace PickYourTunes
{
    public class Cheats
    {
        public static void OnCheat(object Sender, EventArgs Args)
        {
            // if the user is not on a vehicle, return
            if (Game.Player.Character.CurrentVehicle == null)
            {
                return;
            }

            // Show the vehicle hash by using the cheat "pyt hash"
            if (Checks.CheatHasBeenEntered("pyt hash"))
            {
                UI.Notify(string.Format(Resources.CheatHash, Game.Player.Character.CurrentVehicle.Model.GetHashCode()));
            }
            // Show the radio ID by using the "pyt radio" cheat
            else if (Checks.CheatHasBeenEntered("pyt radio"))
            {
                UI.Notify(string.Format(Resources.CheatRadio, Function.Call<int>(Hash.GET_PLAYER_RADIO_STATION_INDEX)));
            }
        }
    }
}
