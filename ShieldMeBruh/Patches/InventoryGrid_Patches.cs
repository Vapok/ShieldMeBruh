using HarmonyLib;
using JetBrains.Annotations;
using Jotunn;
using ShieldMeBruh.Components;

namespace ShieldMeBruh.Patches;

public static class InventoryGrid_Patches
{
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    public static class InventoryGridUpdateGuiPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(InventoryGrid __instance, ref bool __state)
        {
            if (!ShieldMeBruh.AutoShield.FeatureInitialized)
                return;

            if (!__instance.name.Equals("PlayerGrid"))
                return;

            __state = false;

            var width = __instance.m_inventory.GetWidth();
            var height = __instance.m_inventory.GetHeight();

            if (__instance.m_width != width || __instance.m_height != height)
            {
                ShieldMeBruh.Log.Debug($"Width {width} doesn't match {__instance.m_width}");
                ShieldMeBruh.Log.Debug($"Height {height} doesn't match {__instance.m_height}");
                __state = true;
            }
        }

        [UsedImplicitly]
        private static void Postfix(InventoryGrid __instance, ref bool __state, bool __runOriginal)
        {
            if (!__instance.name.Equals("PlayerGrid"))
                return;

            if (!__state || !__runOriginal)
                return;

            ShieldMeBruh.Log.Debug("Inventory Grid needs to init.");
            
            foreach (var element in __instance.m_elements)
            {
                var gameObject = element.m_go;
                var shieldMe = gameObject.GetOrAddComponent<ShieldMe>();
                shieldMe.SetGrid(__instance);
                shieldMe.SetElement(element);
            }
        }
    }
}