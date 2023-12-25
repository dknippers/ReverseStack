using HarmonyLib;

namespace ReverseStack;

public class ReverseStack : Mod
{
    internal static ModLogger ModLogger { get; private set; } = null!;

    public override void Ready()
    {
        ModLogger = Logger;

        Harmony.PatchAll();
    }
}
