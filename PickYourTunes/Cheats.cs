using GTA;
using GTA.Native;
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
                UI.Notify(string.Format("Your vehicle hash is: {0}", Game.Player.Character.CurrentVehicle.Model.GetHashCode()));
            }
            // Show the radio ID by using the "pyt radio" cheat
            else if (Checks.CheatHasBeenEntered("pyt radio"))
            {
                UI.Notify(string.Format("The current radio ID is: {0}", Function.Call<int>(Hash.GET_PLAYER_RADIO_STATION_INDEX)));
            }
        }
    }
}
