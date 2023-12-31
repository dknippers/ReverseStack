using HarmonyLib;
using ReverseStack.Configuration;
using ReverseStack.Extensions;
using UnityEngine;

namespace ReverseStack.Patches;

[HarmonyPatch]
public static class EnableAutoReverseStack
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.StackSend))]
    public static void WorldManager_StackSend(GameCard myCard, WorldManager __instance)
    {
        if (ReverseStackConfig.Instance is not ReverseStackConfig config ||
            !config.EnableForAutoStack ||
            config.AutoStackRange <= 0f)
        {
            return;
        }

        if (myCard is null || (myCard.BounceTarget is not null && config.AutoStackPreferNormalStack))
        {
            return;
        }

        var distanceToNormalStackTarget = (myCard.BounceTarget?.transform.position - myCard.transform.position)?.magnitude;
        var target = __instance.GetNearestCardMatchingPred(myCard, IsNearbyReverseStackTarget);

        if (target?.MyGameCard is not GameCard targetCard)
        {
            if (myCard.BounceTarget is not null)
            {
                SetBounceTarget(myCard, myCard.BounceTarget);
            }

            return;
        }

        Debug.Log($"Set target for {myCard.GetDebugName()} to {targetCard.GetDebugName()}, distance = {(targetCard.transform.position - myCard.transform.position).magnitude}");

        SetBounceTarget(myCard, targetCard);

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
                distance >= distanceToNormalStackTarget)
            {
                return false;
            }

            return true;
        }
    }

    private static void SetBounceTarget(GameCard card, GameCard target)
    {
        var finalTarget = GetFinalBounceTarget(target);

        if (finalTarget != target)
        {
            Debug.Log($"Update target for {card.GetDebugName()} from {target.GetDebugName()} to {finalTarget.GetDebugName()}");
        }

        card.BounceTarget = finalTarget;
        card.Velocity = GetVelocity(card, finalTarget);
    }

    private static GameCard GetFinalBounceTarget(GameCard firstBounceTarget)
    {
        const int MAX_ITERATIONS = 10;

        GameCard finalBounceTarget = firstBounceTarget;

        int i = 0;
        while (finalBounceTarget.BounceTarget is not null && i++ < MAX_ITERATIONS)
        {
            finalBounceTarget = finalBounceTarget.BounceTarget;
        }

        return finalBounceTarget;
    }

    /// <summary>
    /// Returns the velocity to use for <paramref name="card"/> when it has to be moved to <paramref name="other"/>.
    /// </summary>
    /// <param name="card">Card to move</param>
    /// <param name="other">Target card</param>
    /// <returns></returns>
    private static Vector3 GetVelocity(GameCard card, GameCard other)
    {
        var vector = other.transform.position - card.transform.position;
        // Velocity calculation copied from WorldManager.StackSend (line 1718),
        // they seem to always use these hardcoded values (4f / 7f / 4f).
        return new Vector3(vector.x * 4f, 7f, vector.z * 4f);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameCard), "Bounce")]
    public static void GameCard_Bounce_Prefix(GameCard __instance, out GameCard? __state)
    {
        if (ReverseStackConfig.Instance?.EnableForAutoStack == false)
        {
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
            return;
        }

        var bounceTarget = __state;

        if (__instance is null || bounceTarget is null || __instance.HasParent || __instance.Velocity is null)
        {
            return;
        }

        if (bounceTarget.IsDestroyed())
        {
            __instance.BounceTarget = null;
            Debug.Log($"{bounceTarget.GetDebugName()} is destroyed, sending {__instance.GetDebugName()} to find a new target");

            WorldManager.instance.StackSend(__instance);

            // Usually StackSend actually sends the card to its destination but
            // when the card just Bounced on another card (the child of its original target)
            // it will go into "sliding mode" and seems to forget about the BounceTarget alltogether.
            // Since it is not StackSend that takes care of the actual movement -- it only sets the direction using .Velocity --
            // we will handle the BounceTarget here if it is a Reverse Stack target and immediately do the Reverse Stack rather
            // than wait for the physics update and Bounce() calls.
            // If it is a regular stack target we will have to look if that is fixable.
            if (__instance.BounceTarget is GameCard newTarget &&
                !newTarget.IsDestroyed())
            {
                Debug.Log($"Set new bounce target to {newTarget.GetDebugName()}");
                bounceTarget = newTarget;
            }
        }

        if (__instance.CanAutoReverseStackOn(bounceTarget))
        {
            __instance.ReverseStackOn(bounceTarget);
            __instance.Velocity = null;
            __instance.BounceTarget = null;
        }
    }
}
