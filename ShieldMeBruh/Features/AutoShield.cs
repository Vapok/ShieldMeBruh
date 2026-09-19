using System;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using ShieldMeBruh.Configuration;
using ShieldMeBruh.Patches;
using UnityEngine;
using UnityEngine.UI;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Shared;
using YamlDotNet.Serialization;
using Object = UnityEngine.Object;

namespace ShieldMeBruh.Features;

public class AutoShieldSaveData
{
    public Vector2i SavedElement { get; set; }
    public string ItemName { get; set; }
}

public class AutoShield
{
    private InventoryGrid _activeInstance;

    private Sprite _shield;
    public Sprite Shield => _shield;
    public InventoryElement CurrentElement;
    public bool FeatureInitialized = false;
    public ItemDrop.ItemData SelectedShield;

    public ConfigEntry<bool> EnableAutoShield;
    public ConfigEntry<bool> EnableAutoUnequip;
    
    public AutoShield()
    {
        ConfigRegistry.Waiter.StatusChanged += (_, _) => RegisterConfigurationFile();
    }

    public void LoadAssets()
    {
        var path = "ShieldMeBruh.Resources";
        _shield = LoadSprite($"{path}.shield.png", new Rect(0, 0, 1024, 1024));
    }

    public void SetActiveInstance(InventoryGrid instance)
    {
        _activeInstance = instance;
    }

    public InventoryGrid GetActiveInstance()
    {
        return _activeInstance;
    }

    private void RegisterConfigurationFile()
    {
        ConfigSyncBase.UnsyncedConfig("Local Config", "Enable Auto Shield", true,
            new ConfigDescription(
                "When enabled, selected shield will automatically equip when a one handed weapon is equipped.",
                null,
                new ConfigurationManagerAttributes { Order = 1 }),ref EnableAutoShield);

        EnableAutoShield.SettingChanged += (_, _) => SetEnabledStatus();

        ConfigSyncBase.UnsyncedConfig("Local Config", "Enable Auto Unequip", true,
            new ConfigDescription(
                "When enabled, when one handed weapon is unequipped, the marked equipped shield, will also unequip.",
                null,
                new ConfigurationManagerAttributes { Order = 1 }),ref EnableAutoUnequip);
    }
    
    public static Texture2D LoadImage(byte[] bytes)
    {
        var texture = new Texture2D(2, 2);
    
        // use reflection because NetStandard 2.1 is not compatible with .NET4.8 and this function is not available at compile time.
        var loadImage = AccessTools.Method(typeof(ImageConversion), nameof(ImageConversion.LoadImage), new [] {typeof(Texture2D), typeof(byte[])});
        var isSuccess = (bool) loadImage.Invoke(null, new object[]{texture, bytes});
    
        if (!isSuccess)
            throw new Exception("Failed to load image data into texture from byte array");
    
        return texture;
    }

    public Sprite LoadSprite(string path, Rect size, Vector2? pivot = null, int units = 100)
    {
        if (pivot == null) pivot = new Vector2(0.5f, 0.5f);

        var assembly = Assembly.GetExecutingAssembly();
        var imageStream = assembly.GetManifestResourceStream(path);

        var imageData = ReadToEnd(imageStream);
        var texture = LoadImage(imageData);

        if (texture == null) ShieldMeBruh.Log.Error("Missing Embedded Resource: " + path);

        return Sprite.Create(texture, size, pivot.Value, units, 0, SpriteMeshType.Tight);
    }

    private byte[] ReadToEnd(Stream stream)
    {
        var originalPosition = stream.Position;
        stream.Position = 0;

        try
        {
            var readBuffer = new byte[4096];

            var totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;

                if (totalBytesRead == readBuffer.Length)
                {
                    var nextByte = stream.ReadByte();
                    if (nextByte != -1)
                    {
                        var temp = new byte[readBuffer.Length * 2];
                        Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                        Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }

            var buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead)
            {
                buffer = new byte[totalBytesRead];
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }

            return buffer;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private Image CreateShieldedImage(Image baseImg, Image noTeleport)
    {
        // set m_queued parent as parent first, so the position is correct
        var obj = Object.Instantiate(baseImg, baseImg.transform.parent);
        // change the parent to the m_queued image so we can access the new image without a loop
        var transform = obj.transform;
        //transform.SetParent(baseImg.transform);
        transform.name = "shield";
        //transform.SetAsLastSibling();

        // set the new shield image
        obj.sprite = _shield;
        obj.name = "shield";
        obj.color = noTeleport.color;
        obj.type = noTeleport.type;

        return obj;
    }

    public void OnMiddleClick(UIInputHandler middleClick)
    {
        if (!FeatureInitialized || Player.m_localPlayer == null || _activeInstance == null)
            return;

        if (middleClick == null || middleClick.gameObject == null) return;

        var player = Player.m_localPlayer;

        var buttonPos = _activeInstance.GetButtonPos(middleClick.gameObject);
        ShieldMeBruh.Log.Debug($"Button Pressed on {buttonPos.x},{buttonPos.y}");

        var itemAt = _activeInstance.m_inventory.GetItemAt(buttonPos.x, buttonPos.y);

        if (itemAt == null) return;

        ShieldMeBruh.Log.Debug($"Item Name {itemAt.m_shared.m_name} of type {itemAt.m_shared.m_itemType}");

        if (itemAt.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield) return;

        var shieldEquipped = player.m_leftItem != null && player.m_leftItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield;
        var currentEquippedShield = shieldEquipped ? player.m_leftItem : null;

        var targetVector = new Vector2i(buttonPos.x, buttonPos.y);
        var selectedElement = _activeInstance.GetElement(buttonPos.x, buttonPos.y, _activeInstance.m_width);

        if (CurrentElement == null)
        {
            ApplyShieldToElement(selectedElement, itemAt, true);

            if (shieldEquipped && currentEquippedShield != itemAt)
            {
                player.UnequipItem(currentEquippedShield);
                player.EquipItem(itemAt);
            }
        }
        else if (CurrentElement.Position == targetVector)
        {
            ResetCurrentSheildElement();

            if (shieldEquipped)
            {
                player.UnequipItem(currentEquippedShield);
            }
        }
        else if (CurrentElement.Position != targetVector)
        {
            ResetCurrentSheildElement();
            ApplyShieldToElement(selectedElement, itemAt, true);

            if (shieldEquipped && currentEquippedShield != itemAt)
            {
                player.UnequipItem(currentEquippedShield);
                player.EquipItem(itemAt);
            }
        }
    }

    public void SetShieldStatus(bool statusSetTo)
    {
        FeatureInitialized = EnableAutoShield.Value;
        
        if (EnableAutoShield.Value)
        {
            if (statusSetTo)
            {
                if (Player.m_localPlayer is { } player && CurrentElement != null && SelectedShield != null)
                {
                    //Validate Location and Item
                    var itemAt = player.GetInventory().GetItemAt(CurrentElement.Position.x, CurrentElement.Position.y);

                    if (itemAt != SelectedShield)
                        statusSetTo = false;
                }
            }
            if (CurrentElement != null)
            {
                GetShield(CurrentElement).enabled = statusSetTo;
            }
        }
    }
    
    public void SetEnabledStatus()
    {
        FeatureInitialized = EnableAutoShield.Value;
        
        if (EnableAutoShield.Value)
        {
            if (CurrentElement != null)
            {
                GetShield(CurrentElement).enabled = true;
            }
            return;
        }

        if (CurrentElement != null)
            GetShield(CurrentElement).enabled = false;
    }

    public void ResetCurrentSheildElement(InventoryElement selectedElement = null)
    {
        if (CurrentElement != null && selectedElement == null) GetShield(CurrentElement).enabled = false;

        if (selectedElement != null)
            GetShield(selectedElement).enabled = false;

        CurrentElement = null;
        SelectedShield = null;

        var savedData = new AutoShieldSaveData
        {
            SavedElement = new Vector2i(-1, -1)
        };
        SaveShieldSaveData(savedData);
    }

    public void ApplyShieldToElement(InventoryElement selectedElement, ItemDrop.ItemData itemAt, bool allowReset = false)
    {
        if (itemAt.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield)
            return;
        
        var img = GetShield(selectedElement);

        img.enabled = true;

        if (CurrentElement == null)
        {
            CurrentElement = selectedElement;
            SelectedShield = itemAt;
        }
        else
        {
            if ((CurrentElement.Position == selectedElement.Position && allowReset) || selectedElement.Position.x < 0 ||
                selectedElement.Position.y < 0)
            {
                GetShield(CurrentElement).enabled = false;
                CurrentElement = null;
                SelectedShield = null;
            }
            else
            {
                CurrentElement = selectedElement;
                SelectedShield = itemAt;
            }
        }

        var saveVector = new Vector2i(-1, -1);

        if (CurrentElement != null) saveVector = new Vector2i(CurrentElement.Position.x, CurrentElement.Position.y);

        var savedData = new AutoShieldSaveData();
        savedData.SavedElement = saveVector;

        SaveShieldSaveData(savedData);
        
        SetEnabledStatus();
    }

    private Image GetShield(InventoryElement element)
    {
        Image img = null;

        if (element.gameObject == null)
        {
            ShieldMeBruh.Log.Error("Element gameObject is null");
            return null;
        }

        if (element.transform.childCount > 0)
        {
            for (var i = 0; i < element.transform.childCount; i++)
            {
                var childTransform = element.transform.GetChild(i);
                var childImage = childTransform.GetComponent<Image>();
                
                if (childImage != null)
                {
                    if (childImage.transform.name == "shield")
                        img = childImage;
                }
            }
        }

        if (img == null)
        {
            ShieldMeBruh.Log.Debug($"Image Null: {element.name}");
            img = CreateShieldedImage(element.m_icon, element.m_noteleport);
        }

        img.enabled = false;
        return img;
    }

    public void ResetAutoShieldOnPlayerAwake()
    {
        if (DeathEvent.DeathInProgress)
            return;
        
        ShieldMeBruh.Log.Debug($"Resetting Player Context");
        _activeInstance = null;
        CurrentElement = null;
        SelectedShield = null;
    }

    public AutoShieldSaveData GetShieldSaveData()
    {
        var outputData = new AutoShieldSaveData()
        {
            SavedElement = new Vector2i(-1, -1)
        };

        if (Player.m_localPlayer != null && Player.m_localPlayer.m_customData.ContainsKey("vapok.mods.shieldmebruh"))
        {
            var deserializer = new DeserializerBuilder().Build();

            var yaml = deserializer.Deserialize<AutoShieldSaveData>(
                Player.m_localPlayer.m_customData["vapok.mods.shieldmebruh"]);

            outputData = yaml;
        }

        return outputData;
    }

    public void SaveShieldSaveData(AutoShieldSaveData savedData)
    {
        if (Player.m_localPlayer == null)
            return;

        var serializer = new SerializerBuilder().Build();

        var yaml = serializer.Serialize(savedData);

        if (Player.m_localPlayer.m_customData.ContainsKey("vapok.mods.shieldmebruh"))
            Player.m_localPlayer.m_customData["vapok.mods.shieldmebruh"] = yaml;
        else
            Player.m_localPlayer.m_customData.Add("vapok.mods.shieldmebruh", yaml);
    }
    
    public static class ResetEvent
    {
        public static void PerformReset(Player player)
        {
            if (Player.m_localPlayer == null || player == null)
                return;
            
            player.UnequipItem(player.m_rightItem, false);
            player.UnequipItem(player.m_leftItem, false);
            OnResetEvent?.Invoke(ShieldMeBruh.AutoShield, EventArgs.Empty);
        }

        public static event EventHandler OnResetEvent;
    }
}