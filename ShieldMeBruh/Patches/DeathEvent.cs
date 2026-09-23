using System.Threading;
using HarmonyLib;
using ShieldMeBruh.Features;

namespace ShieldMeBruh.Patches;

public static class DeathEvent
{
    public static bool DeathInProgress = false;
    
    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    private static class OnDeathEventPatch
    {
        private static void Postfix(Player __instance, bool __runOriginal)
        {
            if (!__runOriginal || __instance == null || __instance != Player.m_localPlayer || ShieldMeBruh.AutoShield == null)
                return;

            ShieldMeBruh.AutoShield.SetShieldStatus(false);
            DeathInProgress = true;
        }
    }
    
    [HarmonyPatch(typeof(TombStone), nameof(TombStone.OnTakeAllSuccess))]
    private static class TombstoneTakeAllEventPatch
    {
        private static void Postfix(TombStone __instance, bool __runOriginal)
        {
            if (!__runOriginal || Game.instance == null || __instance == null || Player.m_localPlayer == null || ShieldMeBruh.AutoShield == null)
                return;

            if (!__instance.IsOwner())
                return;

            Player player = Player.m_localPlayer;
            PlayerProfile profile = Game.instance.GetPlayerProfile();
            if (profile == null)
                return;

            string name = profile.GetName();
            if (string.IsNullOrEmpty(name) || __instance.m_container == null)
                return;

            if (!string.Equals(__instance.m_container.m_name, name))
                return;

            DeathInProgress = false;

            AutoShieldSaveData shieldSaveData = ShieldMeBruh.AutoShield.GetShieldSaveData();
            if (shieldSaveData == null)
                return;

            Vector2i savedElementVector = shieldSaveData.SavedElement;
            if (savedElementVector.x < 0 || savedElementVector.y < 0)
                return;

            if (player.GetInventory() == null)
                return;

            ItemDrop.ItemData savedItem = player.GetInventory().GetItemAt(savedElementVector.x, savedElementVector.y);
            InventoryElement savedElement = null;

            if (ShieldMeBruh.AutoShield.GetActiveInstance() == null)
            {
                if (ShieldMeBruh.AutoShield.CurrentElement != null)
                {
                    savedElement = ShieldMeBruh.AutoShield.CurrentElement;
                }
            }
            else
            {
                savedElement = ShieldMeBruh.AutoShield.GetActiveInstance().GetElement(savedElementVector.x, savedElementVector.y, player.GetInventory().m_width);
            }

            if (savedElement != null && savedItem != null && savedItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                ShieldMeBruh.AutoShield.ApplyShieldToElement(savedElement, savedItem);
            }
        }
    }

    
}