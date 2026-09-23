using System.Collections.Generic;
using BepInEx.Configuration;
using ShieldMeBruh.Configuration;
using UnityEngine;
using UnityEngine.UI;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Shared;
using Object = UnityEngine.Object;

namespace ShieldMeBruh.Features;

public class WeaponExclusion
{
    public const string CustomDataKey = "vapok.mods.shieldmebruh.excluded";

    private Sprite _excluded;
    public Sprite Excluded => _excluded;

    public ConfigEntry<bool> EnableWeaponExclusion;

    public WeaponExclusion()
    {
        ConfigRegistry.Waiter.StatusChanged += (_, _) => RegisterConfigurationFile();
    }

    public void LoadAssets()
    {
        string path = "ShieldMeBruh.Resources";
        _excluded = ShieldMeBruh.AutoShield.LoadSprite($"{path}.excluded.png", new Rect(0, 0, 1024, 1024));
    }

    private void RegisterConfigurationFile()
    {
        ConfigSyncBase.UnsyncedConfig("Local Config", "Enable Weapon Exclusion", true,
            new ConfigDescription(
                "When enabled, middle-clicking a one-handed weapon marks it with a red X to exclude it from auto-equipping the shield.",
                null,
                new ConfigurationManagerAttributes { Order = 2 }), ref EnableWeaponExclusion);
    }

    public bool IsExcluded(ItemDrop.ItemData item)
    {
        if (item == null || EnableWeaponExclusion == null || !EnableWeaponExclusion.Value)
            return false;

        if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.OneHandedWeapon)
            return false;

        return item.m_customData != null && item.m_customData.ContainsKey(CustomDataKey);
    }

    public void ToggleExclusion(ItemDrop.ItemData item, InventoryElement element = null)
    {
        if (item == null || item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.OneHandedWeapon)
            return;

        if (item.m_customData == null)
            item.m_customData = new Dictionary<string, string>();

        bool isExcluded = item.m_customData.ContainsKey(CustomDataKey);
        if (isExcluded)
        {
            item.m_customData.Remove(CustomDataKey);
            ShieldMeBruh.Log.Debug($"Removed exclusion from {item.m_shared.m_name}");
        }
        else
        {
            item.m_customData[CustomDataKey] = "true";
            ShieldMeBruh.Log.Debug($"Added exclusion to {item.m_shared.m_name}");
        }

        if (element != null)
        {
            Image img = GetExcludedImage(element);
            if (img != null)
                img.enabled = !isExcluded;
        }
    }

    private Image CreateExcludedImage(Image baseImg, Image noTeleport)
    {
        Image obj = Object.Instantiate(baseImg, baseImg.transform.parent);
        Transform transform = obj.transform;
        transform.name = "excluded";

        obj.sprite = _excluded;
        obj.name = "excluded";
        obj.color = noTeleport.color;
        obj.type = noTeleport.type;

        return obj;
    }

    public Image GetExcludedImage(InventoryElement element)
    {
        if (element == null || element.gameObject == null)
        {
            ShieldMeBruh.Log.Error("Element gameObject is null");
            return null;
        }

        ShieldMeElementData data = element.GetComponent<ShieldMeElementData>();
        if (data == null)
        {
            data = element.gameObject.AddComponent<ShieldMeElementData>();
        }

        if (data.ExcludedImage == null)
        {
            data.ExcludedImage = CreateExcludedImage(element.m_icon, element.m_noteleport);
            data.ExcludedImage.enabled = false;
        }

        return data.ExcludedImage;
    }

    public void UpdateGridElements(InventoryGrid grid)
    {
        if (grid == null || grid.m_inventory == null || grid.m_elements == null)
            return;

        foreach (InventoryElement element in grid.m_elements)
        {
            if (element == null || element.gameObject == null)
                continue;

            ItemDrop.ItemData item = grid.m_inventory.GetItemAt(element.Position.x, element.Position.y);
            bool shouldShow = IsExcluded(item);

            Image img = GetExcludedImage(element);
            if (img != null && img.enabled != shouldShow)
            {
                img.enabled = shouldShow;
            }
        }
    }
}
