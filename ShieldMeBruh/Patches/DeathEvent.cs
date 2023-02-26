using System.Threading;
using HarmonyLib;

namespace ShieldMeBruh.Patches;

public static class DeathEvent
{
    public static bool DeathInProgress = false;
    
    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    private static class OnDeathEventPatch
    {
        private static void Postfix(bool __runOriginal)
        {
            if (__runOriginal)
            {
                ShieldMeBruh.AutoShield.SetShieldStatus(false);
                DeathInProgress = true;
            }
                
        }
    }
    
    [HarmonyPatch(typeof(TombStone), nameof(TombStone.OnTakeAllSuccess))]
    private static class TombstoneTakeAllEventPatch
    {
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
                    var savedElementVector = ShieldMeBruh.AutoShield.GetShieldSaveData()?.SavedElement;
                    
                    if (savedElementVector == null)
                        return;

                    if (savedElementVector.Value.x >= 0 && savedElementVector.Value.y >= 0)
                    {
                        var savedItem = player.GetInventory().GetItemAt(savedElementVector.Value.x, savedElementVector.Value.y);
                        
                        if (player.GetInventory() == null)
                            return;

                        InventoryGrid.Element savedElement = null;
                        
                        if (ShieldMeBruh.AutoShield.GetActiveInstance() == null)
                        {
                            if (ShieldMeBruh.AutoShield.CurrentElement != null)
                            {
                                savedElement = ShieldMeBruh.AutoShield.CurrentElement;
                            }
                        }
                        else
                        {
                            savedElement = ShieldMeBruh.AutoShield.GetActiveInstance().GetElement(savedElementVector.Value.x, savedElementVector.Value.y, player.GetInventory().m_width);                            
                        }
                        
                        if (savedElement != null && savedItem != null)
                        {
                            if (savedItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
                            {
                                ShieldMeBruh.AutoShield.ApplyShieldToElement(savedElement, savedItem);
                            }
                        }
                    }
                }
            }
        }
    }

    
}