using GTA;
using GTA.Native;
using System;

namespace PickYourTunes
{
    public class PickYourTunes : Script
    {
        public PickYourTunes()
        {
            Tick += OnTick;
        }

        private void OnTick(object Sender, EventArgs Args)
        {
            // Show the vehicle hash by using the cheat "pyt hash"
            if (Function.Call<bool>(Hash._0x557E43C447E700A8, Game.GenerateHash("pyt hash")))
            {
                if (Game.Player.Character.CurrentVehicle == null)
                {
                    UI.Notify("You are not in a vehicle");
                }
                else
                {
                    UI.Notify(string.Format("Your vehicle hash is: {0}", Game.Player.Character.CurrentVehicle.Model.GetHashCode()));
                }
            }
            // Show the radio ID by using the "pyt radio" cheat
            if (Function.Call<bool>(Hash._0x557E43C447E700A8, Game.GenerateHash("pyt radio")))
            {
                if (Game.Player.Character.CurrentVehicle == null)
                {
                    UI.Notify("You are not in a vehicle");
                }
                else
                {
                    UI.Notify(string.Format("The current radio ID is: {0}", Function.Call<int>(Hash.GET_PLAYER_RADIO_STATION_INDEX)));
                }
            }
        }
    }
}
