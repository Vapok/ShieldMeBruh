/* ShieldMeBruh by Vapok */

using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using ShieldMeBruh.Configuration;
using ShieldMeBruh.Features;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers;
using Vapok.Common.Managers.Configuration;
using Vapok.Common.Managers.LocalizationManager;

namespace ShieldMeBruh;

[BepInPlugin(_pluginId, _displayName, _version)]
public class ShieldMeBruh : BaseUnityPlugin, IPluginInfo
{
    //Module Constants
    private const string _pluginId = "vapok.mods.shieldmebruh";
    private const string _displayName = "Shield Me Bruh!";
    private const string _version = "1.0.8";
    public static bool ValheimAwake;
    public static Waiting Waiter;

    //Class Privates
    private static ShieldMeBruh _instance;
    private static ConfigSyncBase _config;
    private static ILogIt _log;
    private Harmony _harmony;

    //Class Properties
    public static ILogIt Log => _log;
    public static AutoShield AutoShield { get; private set; }

    [UsedImplicitly]
    // This the main function of the mod. BepInEx will call this.
    private void Awake()
    {
        //I'm awake!
        _instance = this;

        //Waiting For Startup
        Waiter = new Waiting();

        //Initialize Managers
        Localizer.Init();

        //Register Configuration Settings
        _config = new ConfigRegistry(_instance);

        //Register Logger
        LogManager.Init(PluginId, out _log);

        Localizer.Waiter.StatusChanged += InitializeModule;

        //Register Features
        AutoShield = new AutoShield();
        AutoShield.FeatureInitialized = true;
        AutoShield.ResetEvent.OnResetEvent += (_, _) => ResetAutoSheild();

        //Patch Harmony
        _harmony = new Harmony(Info.Metadata.GUID);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());

        //???

        //Profit
    }

    private void Start()
    {
        AutoShield.LoadAssets();
    }

    private void Update()
    {
        if (!Player.m_localPlayer || !ZNetScene.instance)
            return;
    }

    private void ResetAutoSheild()
    {
        AutoShield.ResetAutoShieldOnPlayerAwake();
    }
    
    private void OnDestroy()
    {
        _instance = null;
        AutoShield = null;
        _harmony?.UnpatchSelf();
    }

    //Interface Properties
    public string PluginId => _pluginId;
    public string DisplayName => _displayName;
    public string Version => _version;
    public BaseUnityPlugin Instance => _instance;

    public void InitializeModule(object send, EventArgs args)
    {
        if (ValheimAwake)
            return;

        ConfigRegistry.Waiter.ConfigurationComplete(true);

        ValheimAwake = true;
    }

    public class Waiting
    {
        public void ValheimIsAwake(bool awakeFlag)
        {
            if (awakeFlag)
                StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler StatusChanged;
    }
}