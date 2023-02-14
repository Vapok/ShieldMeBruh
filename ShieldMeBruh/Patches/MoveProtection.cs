using HarmonyLib;

namespace ShieldMeBruh.Patches;

public static class MoveProtection
{
    private static bool _reEnableShield;
    private static bool _movingWithMoveItemToThis;
    private static bool _movingWithDropItem;
    private static bool _reEnableShieldOnDropItem;
    private static InventoryGrid.Element _futureElement;
    private static InventoryGrid.Element _oldElement;

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveItemToThis), typeof(Inventory), typeof(ItemDrop.ItemData),
        typeof(int), typeof(int), typeof(int))]
    private static class MoveItemToThisPatch
    {
        private static void Prefix(Inventory __instance, Inventory fromInventory, ItemDrop.ItemData item, int x, int y)
        {
            if (_movingWithDropItem)
                return;

            if (ShieldMeBruh.AutoShield.CurrentElement == null && ShieldMeBruh.AutoShield.SelectedShield == null)
                return;

            /* Two Scenarios:
             * 1) SelectedShield is moving to another item. In this case, "item" is selected sheild, and pos is position of other item moving to.
             * 2) another item, or shield, is moving to a position where SelectedItem is shield, which means it will move.
             *
             * Work: Detect both in this method.
             */

            if (__instance.m_name.Equals("Inventory"))
            {
                //Scenario 2:
                if (item != ShieldMeBruh.AutoShield.SelectedShield)
                {
                    //Peer into the next item
                    var targetElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(x, y, __instance.m_width);
                    var sourceElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(item.m_gridPos.x, item.m_gridPos.y, __instance.m_width);
                    var itemAt = __instance.GetItemAt(x, y);

                    if (itemAt != ShieldMeBruh.AutoShield.SelectedShield)
                        return;

                    _futureElement = sourceElement;
                    _oldElement = targetElement;
                }
                else
                {
                    //Scenario 1:
                    var targetElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(x, y, __instance.m_width);
                    var sourceElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(item.m_gridPos.x, item.m_gridPos.y, __instance.m_width);

                    _futureElement = targetElement;
                    _oldElement = sourceElement;
                }

                _reEnableShield = true;
                _movingWithMoveItemToThis = true;
            }
        }

        private static void Finalizer(Inventory __instance, Inventory fromInventory, ItemDrop.ItemData item, int x,
            int y)
        {
            if (_movingWithDropItem)
                return;

            if (item == null)
                return;

            if (_reEnableShield)
            {
                var newItem = __instance.GetItemAt(_futureElement.m_pos.x, _futureElement.m_pos.y);

                ShieldMeBruh.AutoShield.ResetCurrentSheildElement(_oldElement);
                ShieldMeBruh.AutoShield.ApplyShieldToElement(_futureElement, newItem);

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
        private static void Postfix(Inventory __instance, ItemDrop.ItemData item)
        {
            if (item == null || __instance == null ||
                ShieldMeBruh.AutoShield.CurrentElement == null || ShieldMeBruh.AutoShield.SelectedShield == null ||
                _movingWithDropItem || _movingWithMoveItemToThis)
                return;

            if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield)
                return;

            //if item.pos of item being removed equal CurrentElement.pos then reset.
            if (item.m_gridPos == ShieldMeBruh.AutoShield.CurrentElement.m_pos)
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
            if (ShieldMeBruh.AutoShield.CurrentElement == null && ShieldMeBruh.AutoShield.SelectedShield == null)
                return;

            /* Two Scenarios:
             * 1) SelectedShield is moving to another item. In this case, "item" is selected sheild, and pos is position of other item moving to.
             * 2) another item, or shield, is moving to a position where SelectedItem is shield, which means it will move.
             *
             * Work: Detect both in this method.
             */

            if (__instance.name.Equals("PlayerGrid"))
            {
                //Scenario 2:
                if (item != ShieldMeBruh.AutoShield.SelectedShield)
                {
                    //Peer into the next item
                    var targetElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(pos.x, pos.y, __instance.m_width);
                    var sourceElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(item.m_gridPos.x, item.m_gridPos.y, __instance.m_width);
                    var itemAt = __instance.m_inventory.GetItemAt(pos.x, pos.y);

                    if (itemAt != ShieldMeBruh.AutoShield.SelectedShield)
                        return;

                    _futureElement = sourceElement;
                    _oldElement = targetElement;
                }
                else
                {
                    //Scenario 1:
                    var targetElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(pos.x, pos.y, __instance.m_width);
                    var sourceElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(item.m_gridPos.x, item.m_gridPos.y, __instance.m_width);

                    _futureElement = targetElement;
                    _oldElement = sourceElement;
                }

                _reEnableShieldOnDropItem = true;
                _movingWithDropItem = true;
            }
        }

        private static void Finalizer(InventoryGrid __instance, Inventory fromInventory, ItemDrop.ItemData item,
            int amount, Vector2i pos, ref bool __result)
        {
            if (!__result)
                return;

            if (_reEnableShieldOnDropItem)
            {
                var newItem = __instance.m_inventory.GetItemAt(_futureElement.m_pos.x, _futureElement.m_pos.y);

                ShieldMeBruh.AutoShield.ResetCurrentSheildElement(_oldElement);
                ShieldMeBruh.AutoShield.ApplyShieldToElement(_futureElement, newItem);

                _reEnableShieldOnDropItem = false;
            }

            _oldElement = null;
            _futureElement = null;
            _movingWithDropItem = false;
        }
    }
}