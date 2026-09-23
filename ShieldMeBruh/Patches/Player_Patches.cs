using HarmonyLib;
using ShieldMeBruh.Features;

namespace ShieldMeBruh.Patches;

public static class Player_Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
    private static class SetLocalPlayerPatch
    {
        private static void Postfix(Player __instance)
        {
            if (Jotunn.Managers.GUIManager.IsHeadless() || __instance == null || __instance != Player.m_localPlayer)
                return;

            AutoShield.ResetEvent.PerformReset(__instance);
        }
    }

}