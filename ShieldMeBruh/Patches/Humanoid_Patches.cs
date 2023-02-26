using HarmonyLib;

namespace ShieldMeBruh.Patches;

public static class Humanoid_Patches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    private static class HumanoidEquipItemPatch
    {
        private static void Postfix(Humanoid __instance, ItemDrop.ItemData item, ref bool __result,
            ItemDrop.ItemData ___m_leftItem, bool __runOriginal)
        {
            if (__instance is not Player player || !ShieldMeBruh.AutoShield.FeatureInitialized || !__runOriginal)
                return;

            if (__result && item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon &&
                ___m_leftItem == null)
                if (ShieldMeBruh.AutoShield.SelectedShield != null)
                {
                    var equipItem = player.m_inventory.GetItemAt(ShieldMeBruh.AutoShield.CurrentElement.m_pos.x, ShieldMeBruh.AutoShield.CurrentElement.m_pos.y);
                    if (equipItem != null && equipItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
                    {
                        player.EquipItem(equipItem);                    
                    }
                }
                else
                {
                    var savedData = ShieldMeBruh.AutoShield.GetShieldSaveData();

                    if (savedData.SavedElement.x >= 0 && savedData.SavedElement.y >= 0)
                    {
                        var equipItem = player.m_inventory.GetItemAt(savedData.SavedElement.x, savedData.SavedElement.y);
                        if (equipItem != null && equipItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
                        {
                            player.EquipItem(equipItem);                    
                        }
                    }
                }
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
    private static class HumanoidUnequipItemPatch
    {
        private static void Postfix(Humanoid __instance, ItemDrop.ItemData item, ItemDrop.ItemData ___m_leftItem, bool __runOriginal)
        {
            if (__instance is not Player player || item == null || ___m_leftItem == null || !ShieldMeBruh.AutoShield.FeatureInitialized || !__runOriginal)
                return;

            if (ShieldMeBruh.AutoShield.EnableAutoUnequip.Value)
            {
                ItemDrop.ItemData equipItem = null;

                if (ShieldMeBruh.AutoShield.CurrentElement == null && ShieldMeBruh.AutoShield.SelectedShield == null)
                {
                    var savedData = ShieldMeBruh.AutoShield.GetShieldSaveData();
                    if (savedData.SavedElement.x >= 0 && savedData.SavedElement.y >= 0)
                    {
                        equipItem = player.m_inventory.GetItemAt(savedData.SavedElement.x, savedData.SavedElement.y);

                        if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon &&
                            equipItem != null && ___m_leftItem.m_shared.m_name == equipItem.m_shared.m_name &&
                            ___m_leftItem == equipItem) player.UnequipItem(equipItem);
                    }
                }
                else
                {
                    equipItem = player.m_inventory.GetItemAt(ShieldMeBruh.AutoShield.CurrentElement.m_pos.x, ShieldMeBruh.AutoShield.CurrentElement.m_pos.y);

                    if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon && ShieldMeBruh.AutoShield.SelectedShield != null &&
                        equipItem != null && ___m_leftItem.m_shared.m_name == ShieldMeBruh.AutoShield.SelectedShield.m_shared.m_name &&
                        ___m_leftItem == equipItem) player.UnequipItem(equipItem);
                }
            }
        }
    }
}