using HarmonyLib;

namespace SmartStack
{
    public class SmartStack : Mod
    {
        internal static ModLogger ModLogger { get; private set; } = null!;

        public override void Ready()
        {
            ModLogger = Logger;

            Harmony.PatchAll(typeof(Patches.DropResourcesOnVillager));
        }
    }
}
