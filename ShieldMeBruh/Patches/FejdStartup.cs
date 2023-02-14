using HarmonyLib;

namespace ShieldMeBruh.Patches;

public class FejdStartupPatches
{
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
    [HarmonyAfter("org.bepinex.helpers.LocalizationManager")]
    [HarmonyBefore("org.bepinex.helpers.ItemManager")]
    public static class FejdStartupAwakePatch
    {
        private static void Prefix()
        {
            ShieldMeBruh.Waiter.ValheimIsAwake(true);
        }
    }
}