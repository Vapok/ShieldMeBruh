using HarmonyLib;
using ShieldMeBruh.Features;

namespace ShieldMeBruh.Patches;

public static class Player_Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
    private static class HumanoidEquipItemPatch
    {
        static void Postfix(Player __instance)
        {
            AutoShield.ResetEvent.PerformReset(__instance);
        }
    }

}