using HarmonyLib;
using JetBrains.Annotations;
using ShieldMeBruh.Components;

namespace ShieldMeBruh.Patches;

public static class DeathEvent
{
    public static bool DeathInProgress;
    
    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    private static class OnDeathEventPatch
    {
        [UsedImplicitly]
        private static void Postfix(bool __runOriginal)
        {
            if (__runOriginal)
            {
                ShieldMeBruh.AutoShield.SetShieldStatus(false);
                DeathInProgress = true;
                ShieldMeBruh.Log.Debug($"DEATH EVENT: DeathInProgress: {DeathInProgress}");
            }
                
        }
    }
    
    [HarmonyPatch(typeof(TombStone), nameof(TombStone.OnTakeAllSuccess))]
    private static class TombstoneTakeAllEventPatch
    {
        [UsedImplicitly]
        private static void Postfix(TombStone __instance, bool __runOriginal)
        {
            DeathInProgress = false;
            
            if (Game.instance == null || __instance == null)
                return;

            if (__instance.IsOwner() && Player.m_localPlayer is { } player && __runOriginal)
            {
                var name = Game.instance.GetPlayerProfile()?.GetName();

                if (name == null || __instance.m_container == null)
                    return;

                if (__instance.m_container.m_name.Equals(name))
                {
                    var savedElementVector = ShieldMeBruh.AutoShield.GetShieldSaveData().SavedElement;
                    
                    ShieldMeBruh.Log.Debug($"DEATH EVENT: SavedElement: {savedElementVector}");

                    if (savedElementVector.x >= 0 && savedElementVector.y >= 0)
                    {
                        var savedItem = player.GetInventory().GetItemAt(savedElementVector.x, savedElementVector.y);
                        
                        if (player.GetInventory() == null)
                            return;

                        InventoryGrid.Element savedElement = null;
                        
                        if (ShieldMeBruh.AutoShield.GetActiveInstance() == null)
                        {
                            if (ShieldMeBruh.AutoShield.CurrentElement != null)
                            {
                                savedElement = ShieldMeBruh.AutoShield.CurrentElement;
                                ShieldMeBruh.Log.Debug($"DEATH EVENT: CurrentElement SavedElement: {savedElement.m_pos}");
                            }
                        }
                        else
                        {
                            savedElement = ShieldMeBruh.AutoShield.GetActiveInstance().GetElement(savedElementVector.x, savedElementVector.y, player.GetInventory().m_width);
                            ShieldMeBruh.Log.Debug($"DEATH EVENT: From Inventory SavedElement: {savedElement.m_pos}");
                        }
                        
                        if (savedElement != null && savedItem != null)
                        {
                            if (savedItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
                            {
                                var shield = ShieldMe.GetShieldFromElement(savedElement.m_pos);
                                ShieldMeBruh.Log.Debug($"DEATH EVENT: Getting Shield: {shield.name}");
                                shield.ApplyShieldToElement(savedItem); 
                            }
                        }
                    }
                }
            }
        }
    }

    
}