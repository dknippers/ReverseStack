using HarmonyLib;

namespace StackToBottom.Patches;

/// <summary>
/// The game never calls <see cref="CardData.UpdateCard"/> from <see cref="Worker.UpdateCard"/> outside of Cities.
/// As a result Worker cards will never disable the highlight we enable in <see cref="HighlightStackToBottomTargets"/>,
/// unlike all other cards in the game.
/// We patch that behavior here by setting <see cref="GameCard.HighlightActive"/> to <c>false</c> like <see cref="CardData.UpdateCard"/>
/// does for every other card.
/// </summary>
[HarmonyPatch]
public static class FixStacklands2000HighlightOutsideOfCities
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Worker), "UpdateCard")]
    public static void Worker_UpdateCard(Worker __instance)
    {
        if (WorldManager.instance.CurrentBoard?.Location != Location.Cities &&
            __instance.MyGameCard is GameCard card)
        {
            card.HighlightActive = false;
        };
    }
}
