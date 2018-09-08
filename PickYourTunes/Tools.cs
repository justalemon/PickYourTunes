using GTA;
using GTA.Native;

namespace PickYourTunes
{
    class Tools
    {
        /// <summary>
        /// Set the radio on the specified vehicle.
        /// </summary>
        /// <param name="Name">The radio internal name.</param>
        /// <param name="Car">The vehicle for the radio change.</param>
        public static void SetRadioByName(string Name, Vehicle Car)
        {
            // Turn on the vehicle radio
            Function.Call(Hash.SET_VEHICLE_RADIO_ENABLED, true);
            // And set the requested radio
            Function.Call(Hash.SET_VEH_RADIO_STATION, Car, Name);
        }
    }
}
