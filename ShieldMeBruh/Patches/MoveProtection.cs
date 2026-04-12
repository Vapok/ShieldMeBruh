using HarmonyLib;
using JetBrains.Annotations;
using ShieldMeBruh.Components;
using ShieldMeBruh.Extensions;

namespace ShieldMeBruh.Patches;

public static class MoveProtection
{
    private static bool _reEnableShield;
    private static bool _movingWithMoveItemToThis;
    private static bool _movingWithDropItem;
    private static bool _reEnableShieldOnDropItem;
    private static bool _doingCrafting;
    private static InventoryGrid.Element _futureElement;
    private static InventoryGrid.Element _oldElement;

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.DoCrafting), typeof(Player))]
    private static class EnsureCraftingPatches
    {
        [UsedImplicitly]
        private static void Prefix(InventoryGui __instance)
        {
            _doingCrafting = true;
        }

        [UsedImplicitly]
        private static void Postfix(InventoryGui __instance)
        {
            _doingCrafting = false;

            var originalItem = __instance.m_craftUpgradeItem;

            if (originalItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield) return;
            
            var pos = originalItem.m_gridPos;
            var currentShield = ShieldMe.Elements[pos];
            var shieldItem = currentShield.GetItem();
            
            if (shieldItem != null && shieldItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield &&
                currentShield.ShieldIsActive())
                return;
                
            currentShield.ResetCurrentSheildElement();
        }
    }
    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveItemToThis), typeof(Inventory), typeof(ItemDrop.ItemData))]
    private static class MoveItemPatch
    {
        [UsedImplicitly]
        private static void Prefix(Inventory __instance, Inventory fromInventory, ItemDrop.ItemData item)
        {
            if (_movingWithDropItem)
                return;

            if (ShieldMeBruh.AutoShield.CurrentElement == null && ShieldMeBruh.AutoShield.SelectedShield == null)
                return;

            if (__instance == null || fromInventory == null || item == null)
                return;

            if (!__instance.m_name.Equals("Inventory"))
            {
                if (item.m_gridPos == ShieldMeBruh.AutoShield.CurrentElement?.m_pos)
                {
                    var oldShield = ShieldMe.Elements[ShieldMeBruh.AutoShield.CurrentElement.m_pos];
                    oldShield.ResetCurrentSheildElement();
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveItemToThis), typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int))]
    private static class MoveItemToThisPatch
    {
        [UsedImplicitly]
        private static void Prefix(Inventory __instance, Inventory fromInventory, ItemDrop.ItemData item, int x, int y)
        {
            if (_movingWithDropItem)
                return;

            if (ShieldMeBruh.AutoShield.CurrentElement == null && ShieldMeBruh.AutoShield.SelectedShield == null)
                return;

            if (__instance == null || fromInventory == null || item == null)
                return;

            /* Two Scenarios:
             * 1) SelectedShield is moving to another item. In this case, "item" is selected shield, and pos is position of other item moving to.
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
                    var targetElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(x, y, __instance.m_width);
                    var sourceElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(item.m_gridPos.x, item.m_gridPos.y, __instance.m_width);

                    if (targetElement == null || sourceElement == null)
                        return;

                    _futureElement = targetElement;
                    _oldElement = sourceElement;
                }

                _reEnableShield = true;
                _movingWithMoveItemToThis = true;
            }
        }

        [UsedImplicitly]
        private static void Postfix(Inventory __instance, Inventory fromInventory, ItemDrop.ItemData item, int x,
            int y, bool __runOriginal )
        {
            if (_movingWithDropItem || !__runOriginal)
                return;

            if (item == null)
                return;

            if (_reEnableShield)
            {
                var newItem = __instance.GetItemAt(_futureElement.m_pos.x, _futureElement.m_pos.y);

                if (newItem != null && _oldElement != null && _futureElement != null)
                {
                    var oldShield = ShieldMe.Elements[_oldElement.m_pos];
                    oldShield.ResetCurrentSheildElement();
                    
                    var shield = ShieldMe.Elements[_futureElement.m_pos];
                    shield.ApplyShieldToElement(newItem);
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
        [UsedImplicitly]
        private static void Postfix(Inventory __instance, ItemDrop.ItemData item, bool __runOriginal)
        {
            if (item == null || !__runOriginal || __instance == null ||
                ShieldMeBruh.AutoShield.CurrentElement == null || ShieldMeBruh.AutoShield.SelectedShield == null ||
                _movingWithDropItem || _movingWithMoveItemToThis || _doingCrafting)
                return;

            if (DeathEvent.DeathInProgress)
            {
                ShieldMeBruh.Log.Debug($"RemoveItemPatch: DeathInProgress: {DeathEvent.DeathInProgress}");
                return;
            }
            
            if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield)
                return;
            
            //if item.pos of item being removed equal CurrentElement.pos then reset.
            if (item.m_gridPos != ShieldMeBruh.AutoShield.CurrentElement.m_pos) return;
            
            ShieldMeBruh.Log.Debug($"RemoveItemPatch: Removing an Item Start");
            
            var oldShield = ShieldMe.Elements[ShieldMeBruh.AutoShield.CurrentElement.m_pos];
            oldShield.ResetCurrentSheildElement();
            
            ShieldMeBruh.Log.Debug($"RemoveItemPatch: Removing an Item Finished");
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.DropItem), typeof(Inventory), typeof(ItemDrop.ItemData),
        typeof(int), typeof(Vector2i))]
    private static class DropItemPatch
    {
        [UsedImplicitly]
        private static void Prefix(InventoryGrid __instance, Inventory fromInventory, ItemDrop.ItemData item,
            int amount, Vector2i pos)
        {
            if (item == null || __instance == null)
                return;
            
            if (ShieldMeBruh.AutoShield.SelectedShield == null || ShieldMeBruh.AutoShield.GetActiveInstance() == null)
                return;

            /* Two Scenarios:
             * 1) SelectedShield is moving to another item. In this case, "item" is selected shield, and pos is position of other item moving to.
             * 2) another item, or shield, is moving to a position where SelectedItem is shield, which means it will move.
             *
             * Work: Detect both in this method.
             */

            if (__instance.name.Equals("PlayerGrid"))
            {
                ShieldMeBruh.Log.Debug($"DropItemPatch: Dropping an Item Start");
                //Scenario 2:
                if (item != ShieldMeBruh.AutoShield.SelectedShield)
                {
                    //Peer into the next item
                    var targetElement = ShieldMeBruh.AutoShield.GetActiveInstance().GetElement(pos.x, pos.y, __instance.m_width);
                    var sourceElement = ShieldMeBruh.AutoShield.GetActiveInstance().GetElement(item.m_gridPos.x, item.m_gridPos.y, __instance.m_width);
                    var itemAt = __instance.m_inventory.GetItemAt(pos.x, pos.y);

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
                    var targetElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(pos.x, pos.y, __instance.m_width);
                    var sourceElement = ShieldMeBruh.AutoShield.GetActiveInstance()
                        .GetElement(item.m_gridPos.x, item.m_gridPos.y, __instance.m_width);

                    if (targetElement == null || sourceElement == null)
                        return;

                    _futureElement = targetElement;
                    _oldElement = sourceElement;
                }

                _reEnableShieldOnDropItem = true;
                _movingWithDropItem = true;
                ShieldMeBruh.Log.Debug($"DropItemPatch: Dropping an Item Finished");
            }
        }

        [UsedImplicitly]
        private static void Postfix(InventoryGrid __instance, Inventory fromInventory, ItemDrop.ItemData item,
            int amount, Vector2i pos, ref bool __result, bool __runOriginal)
        {
            if (!__result || !__runOriginal)
            {
                _reEnableShieldOnDropItem = false;
                _oldElement = null;
                _futureElement = null;
                _movingWithDropItem = false;
                return;
            }
                

            if (_reEnableShieldOnDropItem)
            {
                var newItem = __instance.m_inventory.GetItemAt(_futureElement.m_pos.x, _futureElement.m_pos.y);

                if (newItem != null && _oldElement != null && _futureElement != null)
                {
                    var oldShield = ShieldMe.Elements[ShieldMeBruh.AutoShield.CurrentElement.m_pos];
                    oldShield.ResetCurrentSheildElement();

                    var shield = ShieldMe.Elements[_futureElement.m_pos];
                    shield.ApplyShieldToElement(newItem);
                }
                _reEnableShieldOnDropItem = false;
            }

            _oldElement = null;
            _futureElement = null;
            _movingWithDropItem = false;
        }
    }
}