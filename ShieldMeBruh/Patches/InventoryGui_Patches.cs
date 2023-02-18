using HarmonyLib;

namespace ShieldMeBruh.Patches;

public static class InventoryGui_Patches
{
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Show))]
    private static class ShowInventoryPatch
    {
        private static void Postfix(InventoryGui __instance, bool __runOriginal)
        {
            if (__runOriginal)
                ShieldMeBruh.AutoShield.SetActiveInstance(__instance.m_playerGrid);
        }
    }
}