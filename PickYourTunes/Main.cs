using GTA;
using GTA.Native;
using System;

namespace PickYourTunes
{
    public class PickYourTunes : Script
    {
        /// <summary>
        /// The mod configuration.
        /// </summary>
        ScriptSettings Config = ScriptSettings.Load("scripts\\PickYourTunes.ini");

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
            // Now, do the real work
            if (Game.Player.Character.IsGettingIntoAVehicle)
            {
                // Store the vehicle that the player is getting into
                Vehicle PlayerCar = Game.Player.Character.GetVehicleIsTryingToEnter();
                // Store our radio ID
                int NewID = Config.GetValue("Vehicles", PlayerCar.Model.GetHashCode().ToString(), 256);
                // Store our radio name
                string RadioName = Function.Call<string>(Hash.GET_RADIO_STATION_NAME, NewID);

                // If our default value is not 256 (aka invalid or not added), do what we should
                if (NewID != 256)
                {
                    // Turn on the vehicle radio
                    Function.Call(Hash.SET_VEHICLE_RADIO_ENABLED, true);
                    // And set the requested radio
                    Function.Call(Hash.SET_VEH_RADIO_STATION, PlayerCar, RadioName);
                }
            }
        }
    }
}
