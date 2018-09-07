using GTA;
using GTA.Native;

namespace PickYourTunes
{
    class Checks
    {
        /// <summary>
        /// Checks if certain cheat has been entered.
        /// </summary>
        /// <param name="Cheat">The readeable cheat.</param>
        /// <returns>True if the cheat has been entered, false otherwise.</returns>
        public static bool CheatHasBeenEntered(string Cheat)
        {
            return Function.Call<bool>(Hash._0x557E43C447E700A8, Game.GenerateHash(Cheat));
        }

        /// <summary>
        /// Checks if the player got out of a vehicle recently.
        /// </summary>
        /// <returns>True if the player got out of a car, False otherwise.</returns>
        public static bool PlayerIsOutOfVehicle()
        {
            // In order:
            // Player is not on a vehicle
            // Player has been on a car recently
            // Player is not trying to enter a car
            return Game.Player.Character.CurrentVehicle == null && Game.Player.Character.LastVehicle != null && !Game.Player.Character.IsGettingIntoAVehicle;
        }

        /// <summary>
        /// Checks if the vehicle that the player is using has the engine running.
        /// </summary>
        /// <returns>True if is running, false if is off or the player is not in a car.</returns>
        public static bool IsEngineRunning()
        {
            if (Game.Player.Character.CurrentVehicle == null)
            {
                return false;
            }
            else
            {
                return Game.Player.Character.CurrentVehicle.EngineRunning;
            }
        }
    }
}
