using System;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using ShieldMeBruh.Configuration;
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
    public InventoryGrid.Element CurrentElement;
    public bool FeatureInitialized = false;
    public ItemDrop.ItemData SelectedShield;

    public ConfigEntry<bool> EnableAutoShield { get; private set; }
    public ConfigEntry<bool> EnableAutoUnequip { get; private set; }
    
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
        EnableAutoShield = ConfigSyncBase.UnsyncedConfig("Local Config", "Enable Auto Shield", true,
            new ConfigDescription(
                "When enabled, selected shield will automatically equip when a one handed weapon is equipped.",
                null,
                new ConfigurationManagerAttributes { Order = 1 }));

        EnableAutoShield.SettingChanged += (_, _) => SetEnabledStatus();

        EnableAutoUnequip = ConfigSyncBase.UnsyncedConfig("Local Config", "Enable Auto Unequip", true,
            new ConfigDescription(
                "When enabled, when one handed weapon is unequipped, the marked equipped shield, will also unequip.",
                null,
                new ConfigurationManagerAttributes { Order = 1 }));
    }

    public Sprite LoadSprite(string path, Rect size, Vector2? pivot = null, int units = 100)
    {
        if (pivot == null) pivot = new Vector2(0.5f, 0.5f);


        var assembly = Assembly.GetExecutingAssembly();
        var imageStream = assembly.GetManifestResourceStream(path);

        var texture = new Texture2D((int)size.width, (int)size.height, TextureFormat.RGBAFloat, false);
        texture.LoadImage(ReadToEnd(imageStream));

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

        var targetVector = new Vector2i(buttonPos.x, buttonPos.y);
        var selectedElement = _activeInstance.GetElement(buttonPos.x, buttonPos.y, _activeInstance.m_width);

        if (CurrentElement == null)
        {
            ApplyShieldToElement(selectedElement, itemAt, true);
        }
        else if (CurrentElement.m_pos == targetVector)
        {
            ResetCurrentSheildElement();
        }
        else if (CurrentElement.m_pos != targetVector)
        {
            var oldShield = _activeInstance.GetInventory().GetItemAt(CurrentElement.m_pos.x, CurrentElement.m_pos.y);
            var newShield = itemAt;

            ResetCurrentSheildElement();
            ApplyShieldToElement(selectedElement, itemAt, true);

            if (oldShield != null && oldShield.m_equiped) player.EquipItem(newShield);
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

    public void ResetCurrentSheildElement(InventoryGrid.Element selectedElement = null)
    {
        if (CurrentElement != null && selectedElement == null) GetShield(CurrentElement).enabled = false;

        if (selectedElement != null)
            GetShield(selectedElement).enabled = false;

        CurrentElement = null;
        SelectedShield = null;
    }

    public void ApplyShieldToElement(InventoryGrid.Element selectedElement, ItemDrop.ItemData itemAt,
        bool allowReset = false)
    {
        var img = GetShield(selectedElement);

        img.enabled = true;

        if (CurrentElement == null)
        {
            CurrentElement = selectedElement;
            SelectedShield = itemAt;
        }
        else
        {
            if ((CurrentElement.m_pos == selectedElement.m_pos && allowReset) || selectedElement.m_pos.x < 0 ||
                selectedElement.m_pos.y < 0)
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

        if (CurrentElement != null) saveVector = new Vector2i(CurrentElement.m_pos.x, CurrentElement.m_pos.y);

        var savedData = new AutoShieldSaveData();
        savedData.SavedElement = saveVector;

        SaveShieldSaveData(savedData);
        
        SetEnabledStatus();
    }

    private Image GetShield(InventoryGrid.Element element)
    {
        Image img = null;

        if (element == null)
        {
            ShieldMeBruh.Log.Debug("Element is null");
            return null;
        }

        if (element.m_icon == null)
        {
            ShieldMeBruh.Log.Debug("Element.m_icon is null");
        }
        else
        {
            if (element.m_icon.transform == null)
            {
                ShieldMeBruh.Log.Debug("Element.m_icon.transform is null");
            }
            else
            {
                if (element.m_icon.transform.parent == null)
                    ShieldMeBruh.Log.Debug("Element.m_icon.transform.parent is null");
            }
        }


        
        if (element.m_go == null) ShieldMeBruh.Log.Debug("Element.m_go is null");


        if (element.m_go.transform.childCount > 0)
        {
            ShieldMeBruh.Log.Debug($"Parent Transform Name: {element.m_icon.transform.parent.name}");
            for (var i = 0; i < element.m_go.transform.childCount; i++)
            {
                var childTransform = element.m_go.transform.GetChild(i);
                ShieldMeBruh.Log.Debug($"Transform Name: {childTransform.name}");

                var childImage = childTransform.GetComponent<Image>();
                if (childImage != null)
                {
                    ShieldMeBruh.Log.Debug($"Transform Name: {childTransform.name} - Image Name: {childImage.name}");
                    if (childImage.transform.name == "shield")
                        img = childImage;
                }
            }
        }

        if (img == null)
        {
            ShieldMeBruh.Log.Debug($"Image Null: {element.m_go.transform.name}");
            img = CreateShieldedImage(element.m_icon, element.m_noteleport);
        }

        img.enabled = false;
        return img;
    }

    public void ResetAutoShieldOnPlayerAwake()
    {
        ShieldMeBruh.Log.Warning($"Resetting Player Context");
        _activeInstance = null;
        CurrentElement = null;
        SelectedShield = null;
    }

    public AutoShieldSaveData GetShieldSaveData()
    {
        var outputData = new AutoShieldSaveData()
        {
            SavedElement = new Vector2i(-1,-1)
        };

        if (Player.m_localPlayer.m_customData.ContainsKey("vapok.mods.shieldmebruh"))
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
            if (Player.m_localPlayer == null)
                return;
            
            player.UnequipItem(player.m_rightItem, false);
            player.UnequipItem(player.m_leftItem, false);
            OnResetEvent.Invoke(ShieldMeBruh.AutoShield, EventArgs.Empty);
        }

        public static event EventHandler OnResetEvent;
    }
}