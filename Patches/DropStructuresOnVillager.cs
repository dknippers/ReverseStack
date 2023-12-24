using HarmonyLib;
using SmartStack.Utils;

namespace SmartStack.Patches;

[HarmonyPatch]
public static class DropStructuresOnVillager
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CheckIfCanAddOnStack))]
    public static void WorldManager_CheckIfCanAddOnStack(GameCard topCard, ref bool __result)
    {
#if DEBUG
        SmartStack.ModLogger.Log($"{nameof(WorldManager_CheckIfCanAddOnStack)} | Stack: {DebugDisplay.Stack(topCard)}");
#endif

        if (topCard is null) return;

        var stack = topCard.GetAllCardsInStack().ToArray();

        if (stack.Any(card => card.CardData.MyCardType != CardType.Structures))
        {
            return;
        }

        if (__result)
        {
#if DEBUG
            // Game already found a suitable stack.
            SmartStack.ModLogger.Log($"SmartStack.CheckIfCanAddOnStack | Game put card in a stack: ${DebugDisplay.Stack(topCard)}");
#endif
            return;
        }

        var leaf = topCard.GetLeafCard();
        var villagerRoot = topCard.GetOverlappingCards()
            .FirstOrDefault(c => c != topCard && !c.IsChildOf(topCard) && c.GetLeafCard() is GameCard v && v.CardData is BaseVillager && leaf.CardData.CanHaveCardOnTop(v.GetRootCard().CardData))
            ?.GetRootCard();

        if (villagerRoot is null)
        {
            return;
        }

        GameCard parent = villagerRoot.Parent;

#if DEBUG
        SmartStack.ModLogger.Log($"SmartStack | Adding {DebugDisplay.Stack(topCard)} to {DebugDisplay.Stack(villagerRoot)}");
        SmartStack.ModLogger.Log($"SmartStack | VillagerRoot {DebugDisplay.Card(villagerRoot)}");
#endif

        villagerRoot.SetParent(leaf);

#if DEBUG
        SmartStack.ModLogger.Log($"SmartStack | Final stack: {DebugDisplay.Stack(villagerRoot)}");
#endif

        __result = true;
    }
}