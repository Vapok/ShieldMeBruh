using System;
using HarmonyLib;
using ShieldMeBruh.Features;

namespace ShieldMeBruh.Patches;

public static class MoveProtection
{
    private static bool _reEnableShield;
    private static bool _movingWithMoveItemToThis;
    private static bool _movingWithDropItem;
    private static bool _reEnableShieldOnDropItem;
    private static InventoryElement _futureElement;
    private static InventoryElement _oldElement;

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveItemToThis), typeof(Inventory), typeof(ItemDrop.ItemData))]
    private static class MoveItemPatch
    {
        private static void Prefix(Inventory __instance, Inventory fromInventory, ItemDrop.ItemData item)
        {
            if (Jotunn.Managers.GUIManager.IsHeadless() || _movingWithDropItem)
                return;

            if (__instance == null || fromInventory == null || item == null || string.IsNullOrEmpty(__instance.m_name))
                return;

            if (ShieldMeBruh.AutoShield == null || (ShieldMeBruh.AutoShield.CurrentElement == null && ShieldMeBruh.AutoShield.SelectedShield == null))
                return;

            if (!string.Equals(__instance.m_name, "Inventory", StringComparison.Ordinal))
            {
                if (item == ShieldMeBruh.AutoShield.SelectedShield ||
                    (ShieldMeBruh.AutoShield.CurrentElement != null && item.m_gridPos == ShieldMeBruh.AutoShield.CurrentElement.Position))
                {
                    ShieldMeBruh.AutoShield.ResetCurrentSheildElement();
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveItemToThis), typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int))]
    private static class MoveItemToThisPatch
    {
        private static void Prefix(Inventory __instance, Inventory fromInventory, ItemDrop.ItemData item, int x, int y)
        {
            if (Jotunn.Managers.GUIManager.IsHeadless() || _movingWithDropItem)
                return;

            if (__instance == null || fromInventory == null || item == null || string.IsNullOrEmpty(__instance.m_name))
                return;

            if (ShieldMeBruh.AutoShield == null || (ShieldMeBruh.AutoShield.CurrentElement == null && ShieldMeBruh.AutoShield.SelectedShield == null))
                return;

            /* Two Scenarios:
             * 1) SelectedShield is moving to another item. In this case, "item" is selected sheild, and pos is position of other item moving to.
             * 2) another item, or shield, is moving to a position where SelectedItem is shield, which means it will move.
             *
             * Work: Detect both in this method.
             */

            if (string.Equals(__instance.m_name, "Inventory", StringComparison.Ordinal))
            {
                InventoryGrid activeGrid = ShieldMeBruh.AutoShield.GetActiveInstance();
                if (activeGrid == null)
                    return;

                //Scenario 2:
                if (item != ShieldMeBruh.AutoShield.SelectedShield)
                {
                    //Peer into the next item
                    InventoryElement targetElement = activeGrid.GetElement(x, y, __instance.m_width);
                    InventoryElement sourceElement = activeGrid.GetElement(item.m_gridPos.x, item.m_gridPos.y, __instance.m_width);
                    ItemDrop.ItemData itemAt = __instance.GetItemAt(x, y);

                    if (targetElement == null || sourceElement == null || itemAt == null)
                        return;

                    if (itemAt != ShieldMeBruh.AutoShield.SelectedShield)
                        return;

                    _futureElement = sourceElement;
                    _oldElement = targetElement;
                }
                else
                {
                    //Scenario 1:
                    InventoryElement targetElement = activeGrid.GetElement(x, y, __instance.m_width);
                    InventoryElement sourceElement = activeGrid.GetElement(item.m_gridPos.x, item.m_gridPos.y, __instance.m_width);

                    if (targetElement == null || sourceElement == null)
                        return;

                    _futureElement = targetElement;
                    _oldElement = sourceElement;
                }

                _reEnableShield = true;
                _movingWithMoveItemToThis = true;
            }
            else
            {
                if (item == ShieldMeBruh.AutoShield.SelectedShield ||
                    (ShieldMeBruh.AutoShield.CurrentElement != null && item.m_gridPos == ShieldMeBruh.AutoShield.CurrentElement.Position))
                {
                    ShieldMeBruh.AutoShield.ResetCurrentSheildElement();
                }
            }
        }

        private static void Postfix(Inventory __instance, Inventory fromInventory, ItemDrop.ItemData item, int x,
            int y, bool __runOriginal )
        {
            if (Jotunn.Managers.GUIManager.IsHeadless() || _movingWithDropItem || !__runOriginal)
                return;

            if (item == null || __instance == null)
                return;

            if (_reEnableShield)
            {
                ItemDrop.ItemData newItem = __instance.GetItemAt(_futureElement.Position.x, _futureElement.Position.y);

                if (newItem != null && _oldElement != null && _futureElement != null && ShieldMeBruh.AutoShield != null) 
                {
                    ShieldMeBruh.AutoShield.ResetCurrentSheildElement(_oldElement);
                    ShieldMeBruh.AutoShield.ApplyShieldToElement(_futureElement, newItem);
                }
                _reEnableShield = false;
            }

            _oldElement = null;
            _futureElement = null;


            _movingWithMoveItemToThis = false;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.RemoveItem), typeof(ItemDrop.ItemData))]
    private static class RemoveItemPatch
    {
        private static void Postfix(Inventory __instance, ItemDrop.ItemData item, bool __runOriginal)
        {
            if (Jotunn.Managers.GUIManager.IsHeadless() || item == null || !__runOriginal || __instance == null || string.IsNullOrEmpty(__instance.m_name) ||
                ShieldMeBruh.AutoShield == null || ShieldMeBruh.AutoShield.CurrentElement == null || ShieldMeBruh.AutoShield.SelectedShield == null ||
                _movingWithDropItem || _movingWithMoveItemToThis)
                return;

            if (!string.Equals(__instance.m_name, "Inventory", StringComparison.Ordinal))
                return;

            if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield)
                return;

            //if item.pos of item being removed equal CurrentElement.pos then reset.
            if (item == ShieldMeBruh.AutoShield.SelectedShield || item.m_gridPos == ShieldMeBruh.AutoShield.CurrentElement.Position)
                ShieldMeBruh.AutoShield.ResetCurrentSheildElement();
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.DropItem), typeof(Inventory), typeof(ItemDrop.ItemData),
        typeof(int), typeof(Vector2i))]
    private static class DropItemPatch
    {
        private static void Prefix(InventoryGrid __instance, Inventory fromInventory, ItemDrop.ItemData item,
            int amount, Vector2i pos)
        {
            if (Jotunn.Managers.GUIManager.IsHeadless() || item == null || __instance == null || ShieldMeBruh.AutoShield == null)
                return;
            
            InventoryGrid activeGrid = ShieldMeBruh.AutoShield.GetActiveInstance();
            if (ShieldMeBruh.AutoShield.SelectedShield == null || activeGrid == null)
                return;

            /* Two Scenarios:
             * 1) SelectedShield is moving to another item. In this case, "item" is selected sheild, and pos is position of other item moving to.
             * 2) another item, or shield, is moving to a position where SelectedItem is shield, which means it will move.
             *
             * Work: Detect both in this method.
             */

            if (string.Equals(__instance.name, "PlayerGrid", StringComparison.Ordinal))
            {
                //Scenario 2:
                if (item != ShieldMeBruh.AutoShield.SelectedShield)
                {
                    //Peer into the next item
                    InventoryElement targetElement = activeGrid.GetElement(pos.x, pos.y, __instance.m_width);
                    InventoryElement sourceElement = activeGrid.GetElement(item.m_gridPos.x, item.m_gridPos.y, __instance.m_width);
                    ItemDrop.ItemData itemAt = __instance.m_inventory.GetItemAt(pos.x, pos.y);

                    if (targetElement == null || sourceElement == null || itemAt == null)
                        return;

                    if (itemAt != ShieldMeBruh.AutoShield.SelectedShield)
                        return;

                    _futureElement = sourceElement;
                    _oldElement = targetElement;
                }
                else
                {
                    //Scenario 1:
                    InventoryElement targetElement = activeGrid.GetElement(pos.x, pos.y, __instance.m_width);
                    InventoryElement sourceElement = activeGrid.GetElement(item.m_gridPos.x, item.m_gridPos.y, __instance.m_width);

                    if (targetElement == null || sourceElement == null)
                        return;

                    _futureElement = targetElement;
                    _oldElement = sourceElement;
                }

                _reEnableShieldOnDropItem = true;
                _movingWithDropItem = true;
            }
        }

        private static void Postfix(InventoryGrid __instance, Inventory fromInventory, ItemDrop.ItemData item,
            int amount, Vector2i pos, ref bool __result, bool __runOriginal)
        {
            if (Jotunn.Managers.GUIManager.IsHeadless() || !__result || !__runOriginal || __instance == null || __instance.m_inventory == null)
            {
                _reEnableShieldOnDropItem = false;
                _oldElement = null;
                _futureElement = null;
                _movingWithDropItem = false;
                return;
            }

            if (_reEnableShieldOnDropItem)
            {
                ItemDrop.ItemData newItem = __instance.m_inventory.GetItemAt(_futureElement.Position.x, _futureElement.Position.y);

                if (newItem != null && _oldElement != null && _futureElement != null && ShieldMeBruh.AutoShield != null)
                {
                    ShieldMeBruh.AutoShield.ResetCurrentSheildElement(_oldElement);
                    ShieldMeBruh.AutoShield.ApplyShieldToElement(_futureElement, newItem);
                }
                _reEnableShieldOnDropItem = false;
            }

            _oldElement = null;
            _futureElement = null;
            _movingWithDropItem = false;
        }
    }
}
