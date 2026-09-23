using HarmonyLib;

namespace ShieldMeBruh.Patches;

public static class InventoryGrid_Patches
{
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    public static class InventoryGridUpdateGuiPatch
    {
        private static bool _initializedElement;

        public static void ResetInitializedElement()
        {
            _initializedElement = false;
        }
        
        [HarmonyPriority(Priority.First)]
        private static void Prefix(InventoryGrid __instance, ref bool __state)
        {
            if (!ShieldMeBruh.AutoShield.FeatureInitialized)
                return;

            if (!__instance.name.Equals("PlayerGrid"))
                return;

            __state = false;

            int width = __instance.m_inventory.GetWidth();
            int height = __instance.m_inventory.GetHeight();

            if (__instance.m_width != width || __instance.m_height != height)
            {
                ShieldMeBruh.Log.Debug($"Width {width} doesn't match {__instance.m_width}");
                ShieldMeBruh.Log.Debug($"Height {height} doesn't match {__instance.m_height}");
                __state = true;
                _initializedElement = false;
            }
        }

        private static void Postfix(InventoryGrid __instance, ref bool __state, bool __runOriginal)
        {
            if (!__instance.name.Equals("PlayerGrid"))
                return;

            if (!__runOriginal)
                return;

            if (ShieldMeBruh.WeaponExclusion != null)
                ShieldMeBruh.WeaponExclusion.UpdateGridElements(__instance);

            if (!__state)
                return;

            ShieldMeBruh.Log.Debug("Inventory Grid needs to init.");
            
            foreach (InventoryElement element in __instance.m_elements)
            {
                if (element == null || element.gameObject == null)
                    continue;

                UIInputHandler inputHandler = element.gameObject.GetComponentInChildren<UIInputHandler>();
                if (inputHandler != null)
                {
                    inputHandler.m_onMiddleDown -= ShieldMeBruh.AutoShield.OnMiddleClick;
                    inputHandler.m_onMiddleDown += ShieldMeBruh.AutoShield.OnMiddleClick;
                }
            }

            if (!_initializedElement && Player.m_localPlayer != null && Player.m_localPlayer.m_customData.ContainsKey("vapok.mods.shieldmebruh"))
            {
                Vector2i savedElementVector = ShieldMeBruh.AutoShield.GetShieldSaveData().SavedElement;

                if (savedElementVector.x >= 0 && savedElementVector.y >= 0)
                {
                    InventoryElement savedElement =
                        __instance.GetElement(savedElementVector.x, savedElementVector.y, __instance.m_width);
                    ItemDrop.ItemData savedItem = __instance.m_inventory.GetItemAt(savedElementVector.x, savedElementVector.y);

                    if (savedElement != null && savedItem != null) ShieldMeBruh.AutoShield.ApplyShieldToElement(savedElement, savedItem);
                }

                _initializedElement = true;
            }
        }
    }
}