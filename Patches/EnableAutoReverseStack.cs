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

        if (target is not null && target.MyGameCard is GameCard targetCard)
        {
            Debug.Log($"Set target for {myCard.GetDebugName()} to {targetCard.GetDebugName()}");

            // If our target card itself is traveling to some other card we will set
            // that as our target too to follow the same trajectery as targetCard
            if (targetCard.BounceTarget is not null)
            {
                Debug.Log($"Update target for {myCard.GetDebugName()} from {targetCard.GetDebugName()} to {targetCard.BounceTarget.GetDebugName()}");
                targetCard = targetCard.BounceTarget;
            }

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

        Debug.Log($"Auto RS {__instance.GetDebugName()} On {bounceTarget.GetDebugName()}");

        if (__instance.CanAutoReverseStackOn(bounceTarget))
        {
            __instance.ReverseStackOn(bounceTarget);
            __instance.Velocity = null;
            __instance.BounceTarget = null;
        }
    }
}
