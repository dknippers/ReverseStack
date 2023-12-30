using HarmonyLib;
using ReverseStack.Configuration;
using ReverseStack.Extensions;
using UnityEngine;

namespace ReverseStack.Patches;

/// <summary>
/// When a stack X cannot be stacked on top of a stack Y this patch will attempt the reverse: stack Y on top of X.
///
/// This is especially useful when stack Y contains a Villager harvesting cards such as Apple Trees and does not allow any
/// cards being stacked on top of it, but it can itself be stacked on top of another stack.
/// 
/// For example, consider stack X being:
///
/// <code>
/// [ Apple Tree ]
///
/// and stack Y being:
/// 
/// [ Apple Tree ] &lt;- bottom of stack
/// [ Apple Tree ]
/// [ Villager   ] &lt;- top of stack
/// </code>
/// 
/// Stack X is not allowed on top of stack Y because the game does not allow anything to be added on top of
/// a stack that has any status cards (cards that have a timer running). However, it is perfectly valid to put stack Y on top of stack X
/// even if Y contains status cards.
///
/// When dragging stack X on top of stack Y and failing to stop on top this patch will put stack Y on top of stack X instead
/// which is what the user would want in this case.
/// </summary>
[HarmonyPatch]
public static class AttemptReverseStack
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.StackSend))]
    public static void WorldManager_StackSend(GameCard myCard, WorldManager __instance)
    {
        if (ReverseStackConfig.Instance is not ReverseStackConfig config ||
            !config.EnableForAutoStack ||
            config.AutoStackRange <= 0f)
        {
            Debug.Log("[WorldManager_StackSend] Enable for auto stack is OFF, skipping");
            return;
        }

        if (myCard is null || (myCard.BounceTarget is not null && config.AutoStackPreferNormalStack))
        {
            if (myCard?.BounceTarget is not null && config.AutoStackPreferNormalStack)
            {
                Debug.Log("[WorldManager_StackSend] Game found normal stack target and we prefer that so SKIP");
            }

            return;
        }

        var distanceToNormalStackTarget = (myCard.BounceTarget?.transform.position - myCard.transform.position)?.magnitude;
        var target = __instance.GetNearestCardMatchingPred(myCard, IsNearbyReverseStackTarget);

        if (target is not null && target.MyGameCard is GameCard targetCard)
        {
            // Velocity calculation copied from WorldManager.StackSend (line 1718)
            var vector = targetCard.transform.position - myCard.transform.position;
            var velocity = new Vector3(vector.x * 4f, 7f, vector.z * 4f);

            myCard.BounceTarget = targetCard;
            myCard.Velocity = velocity;
        }

        bool IsNearbyReverseStackTarget(GameCard other)
        {
            if (!other.IsRoot() ||
                !myCard.CanAutoReverseStackOn(other))
            {
                return false;
            }

            var vector = other.transform.position - myCard.transform.position;
            var distance = vector.magnitude;

            if (distance > config.AutoStackRange ||
                distance > distanceToNormalStackTarget)
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameCard), "Bounce")]
    public static void GameCard_Bounce_Prefix(GameCard __instance, out GameCard? __state)
    {
        if (ReverseStackConfig.Instance?.EnableForAutoStack == false)
        {
            Debug.Log("[GameCard_Bounce_Prefix] Enable for auto stack is OFF, skipping");
            __state = null;
            return;
        }

        __state = __instance?.BounceTarget;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameCard), "Bounce")]
    public static void GameCard_Bounce(GameCard __instance, GameCard? __state)
    {
        if (ReverseStackConfig.Instance?.EnableForAutoStack == false)
        {
            Debug.Log("[GameCard_Bounce] Enable for auto stack is OFF, skipping");
            return;
        }

        var bounceTarget = __state;

        if (__instance is null || bounceTarget is null || __instance.HasParent || __instance.Velocity is null)
        {
            return;
        }

        if (__instance.CanAutoReverseStackOn(bounceTarget))
        {
            ReverseStackOn(__instance, bounceTarget);
            __instance.Velocity = null;
            __instance.BounceTarget = null;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.CheckIfCanAddOnStack))]
    public static void WorldManager_CheckIfCanAddOnStack(GameCard topCard, ref bool __result)
    {
        if (__result || topCard?.CardData is null)
        {
            return;
        }

        var targetRoot = topCard
            .GetOverlappingCards()
            .Select(c => c.GetRootCard())
            .FirstOrDefault(topCard.CanReverseStackOn);

        if (targetRoot is null)
        {
            return;
        }

        ReverseStackOn(topCard, targetRoot);

        __result = true;
    }

    private static void ReverseStackOn(GameCard card, GameCard target)
    {
        // We do not want to move the target stack that we are Reverse Stacking onto.
        // By default when adding a card onto a stack on the board the stack that was being dragged will snap onto
        // the stack that is on the board.
        // Technically since we perform a Reverse Stack operation the card we are dragging is considered
        // the stack on the board and the stack that was actually on the board will be moved to snap onto the stack being dragged.
        // To fix this issue we first update the position of the stack being dragged to the position of the stack we drag onto.
        card.SetPosition(target.transform.position);
        target.SetParent(card.GetLeafCard());

        PlayDropOnStackSound();
    }

    private static void PlayDropOnStackSound()
    {
        AudioManager.me.PlaySound2D(AudioManager.me.DropOnStack, UnityEngine.Random.Range(0.8f, 1.2f), 0.3f);
    }
}