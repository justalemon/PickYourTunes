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
    }
}
