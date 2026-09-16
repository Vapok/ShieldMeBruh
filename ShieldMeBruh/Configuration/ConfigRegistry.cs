using System;
using BepInEx.Configuration;
using Vapok.Common.Abstractions;
using Vapok.Common.Managers.Configuration;

namespace ShieldMeBruh.Configuration;

public class ConfigRegistry : ConfigSyncBase
{
    //Configuration Entry Privates
    internal static ConfigEntry<bool> ShowSplashOnStartup;
    internal static ConfigEntry<bool> EnableTelemetry;

    public static Waiting Waiter;

    public ConfigRegistry(IPluginInfo mod, bool enableLockedConfigs = false) : base(mod, enableLockedConfigs)
    {
        //Waiting For Startup
        Waiter = new Waiting();

        InitializeConfigurationSettings();
    }

    public sealed override void InitializeConfigurationSettings()
    {
        if (_config == null)
            return;

        //User Configs
        UnsyncedConfig("Local Config", "Show Splash on Startup", true,
            new ConfigDescription("If enabled, displays the mod overview and links splash screen on game startup.",
                null, new ConfigurationManagerAttributes { Order = 4 }), ref ShowSplashOnStartup);

        UnsyncedConfig("Local Config", "Enable Anonymous Telemetry", true,
            new ConfigDescription("If enabled, sends anonymous mod launch and heartbeat telemetry to help improve mod stability and track active versions.",
                null, new ConfigurationManagerAttributes { Order = 5 }), ref EnableTelemetry);
    }
}

public class Waiting
{
    public void ConfigurationComplete(bool configDone)
    {
        if (configDone)
            StatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler StatusChanged;
}