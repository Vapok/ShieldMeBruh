using System;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using ShieldMeBruh.Configuration;
using ShieldMeBruh.Extensions;
using ShieldMeBruh.Patches;
using UnityEngine;
using Vapok.Common.Managers.Configuration;
using YamlDotNet.Serialization;

namespace ShieldMeBruh.Features;

public class AutoShieldSaveData
{
    public Vector2i SavedElement { get; set; }
    public string ItemName { get; set; }
}

public class AutoShield
{
    private InventoryGrid _activeInstance;

    public Sprite Shield;
    public InventoryGrid.Element CurrentElement;
    public bool FeatureInitialized;
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
        Shield = LoadSprite($"{path}.shield.png", new Rect(0, 0, 1024, 1024));
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
                    var itemAt = player.GetInventory().GetItemAt(CurrentElement.m_pos.x, CurrentElement.m_pos.y);

                    if (itemAt != SelectedShield)
                        statusSetTo = false;
                }
            }
            if (CurrentElement != null)
            {
                CurrentElement.GetShield().enabled = statusSetTo;
            }
        }
    }
    
    public void SetEnabledStatus()
    {
        FeatureInitialized = EnableAutoShield.Value;
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
            SavedElement = new Vector2i(-1,-1)
        };
        
        if (Player.m_localPlayer.m_customData.ContainsKey(ShieldMeBruh.m_instance.PluginId))
        {
            var deserializer = new DeserializerBuilder().Build();

            var yaml = deserializer.Deserialize<AutoShieldSaveData>(Player.m_localPlayer.m_customData[ShieldMeBruh.m_instance.PluginId]);

            outputData = yaml;
        }

        return outputData;
    }
    
    public static class ResetEvent
    {
        public static void PerformReset(Player player)
        {
            if (Player.m_localPlayer == null)
                return;
            
            player.UnequipItem(player.m_rightItem, false);
            player.UnequipItem(player.m_leftItem, false);
            OnResetEvent?.Invoke(ShieldMeBruh.AutoShield, EventArgs.Empty);
        }

        public static event EventHandler OnResetEvent;
    }
}