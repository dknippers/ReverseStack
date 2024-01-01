using HarmonyLib;
using StackToBottom.Configuration;
using StackToBottom.Extensions;
using UnityEngine;

namespace StackToBottom.Patches;

[HarmonyPatch]
public static class EnableAutoStackToBottom
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.StackSend))]
    public static void WorldManager_StackSend(GameCard myCard, WorldManager __instance)
    {
        if (StackToBottomConfig.Instance is not StackToBottomConfig config ||
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
        var target = __instance.GetNearestCardMatchingPred(myCard, IsNearbyStackToBottomTarget);

        if (target?.MyGameCard is not GameCard targetCard)
        {
            if (myCard.BounceTarget is not null)
            {
                SetBounceTarget(myCard, myCard.BounceTarget);
            }

            return;
        }

        SetBounceTarget(myCard, targetCard);

        bool IsNearbyStackToBottomTarget(GameCard other)
        {
            if (!other.IsRoot() ||
                !myCard.CanAutoStackToBottomOf(other))
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
        if (StackToBottomConfig.Instance?.EnableForAutoStack == false)
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
        if (StackToBottomConfig.Instance?.EnableForAutoStack == false)
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
            __instance.Velocity = null;
            WorldManager.instance.StackSend(__instance);
            return;
        }

        if (__instance.CanAutoStackToBottomOf(bounceTarget))
        {
            __instance.StackToBottomOf(bounceTarget);
            __instance.Velocity = null;
            __instance.BounceTarget = null;
        }
    }
}
