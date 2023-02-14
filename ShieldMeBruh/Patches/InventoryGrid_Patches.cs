using HarmonyLib;
using ShieldMeBruh.Features;
using YamlDotNet.Serialization;

namespace ShieldMeBruh.Patches;

public static class InventoryGrid_Patches
{
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    private static class InventoryGridUpdateGuiPatch
    {
        private static bool _initializedElement;

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
                _initializedElement = false;
            }
        }

        private static void Postfix(InventoryGrid __instance, ref bool __state)
        {
            if (!__instance.name.Equals("PlayerGrid"))
                return;

            if (!__state)
                return;

            ShieldMeBruh.Log.Debug("Inventory Grid needs to init.");
            
            foreach (var element in __instance.m_elements)
            {
                var gameObject = element.m_go;
                var inputHandler = gameObject.GetComponentInChildren<UIInputHandler>();
                inputHandler.m_onMiddleDown += ShieldMeBruh.AutoShield.OnMiddleClick;
                ShieldMeBruh.Log.Debug($"Adding to element: X: {element.m_pos.x}  Y: {element.m_pos.y}");
            }

            if (!_initializedElement && Player.m_localPlayer.m_customData.ContainsKey("vapok.mods.shieldmebruh"))
            {
                
                var savedElementVector = ShieldMeBruh.AutoShield.GetShieldSaveData().SavedElement;

                if (savedElementVector.x >= 0 && savedElementVector.y >= 0)
                {
                    var savedElement =
                        __instance.GetElement(savedElementVector.x, savedElementVector.y, __instance.m_width);
                    var savedItem = __instance.m_inventory.GetItemAt(savedElementVector.x, savedElementVector.y);

                    if (savedElement != null && savedItem != null) ShieldMeBruh.AutoShield.ApplyShieldToElement(savedElement, savedItem);
                }

                _initializedElement = true;
            }
        }
    }
}