using GTA;
using GTA.Native;

namespace PickYourTunes
{
    class Tools
    {
        /// <summary>
        /// Set the radio on the specified vehicle.
        /// </summary>
        /// <param name="ID">The radio ID.</param>
        /// <param name="Car">The vehicle for the radio change.</param>
        public static void SetRadioInVehicle(int ID, Vehicle Car)
        {
            // Get the radio name from the ID
            string RadioName = Function.Call<string>(Hash.GET_RADIO_STATION_NAME, ID);
            // And do the change
            SetRadioInVehicle(RadioName, Car);
        }

        /// <summary>
        /// Set the radio on the specified vehicle.
        /// </summary>
        /// <param name="Name">The radio internal name.</param>
        /// <param name="Car">The vehicle for the radio change.</param>
        public static void SetRadioInVehicle(string Name, Vehicle Car)
        {
            // Turn on the vehicle radio
            Function.Call(Hash.SET_VEHICLE_RADIO_ENABLED, true);
            // And set the requested radio
            Function.Call(Hash.SET_VEH_RADIO_STATION, Car, Name);
        }
    }
}
