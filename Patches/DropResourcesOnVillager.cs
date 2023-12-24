using HarmonyLib;

namespace SmartStack.Patches;

public static class DropResourcesOnVillager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CheckIfCanAddOnStack))]
    public static void WorldManager_CheckIfCanAddOnStack()
    {
        SmartStack.ModLogger.Log("SmartStack.CheckIfCanAddOnStack");
    }
}