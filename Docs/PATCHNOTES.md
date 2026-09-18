# 2.0.7 - Splash Window Updates & Valheim 1.0.14 Alignment
* **Splash Window Updates**:
  * Updated telemetry default to unchecked on first launch (Opt-In).
  * Added Send Error Logs toggle (Opt-Out) to capture anonymous crash diagnostics and error reports.
  * Added in-game scrollable Privacy Policy overlay with responsive mouse wheel support.
  * Added interactive tooltip data disclaimers on checkbox hover.
* **Valheim 1.0.14 Alignment**:
  * Aligned publicized game assembly and UnityEngine references to Valheim 1.0.14.
  * Updated internalized  dependency to 3.12.1014.

# 2.0.6 - Jewelcrafting Font Compatibility
* **Compatibility Fix**: Fixed issue where Jewelcrafting packages its own font which was overriding part of a vanilla font, causing the Splash screen to appear blank.
* **Vapok.Common Dependency Bump**: Updated internalized dependency to `Vapok.Valheim.Common` 3.11.1012.

# 2.0.5 - Updated README with Telemetry Information
* **Documentation Update**: Updated the README.md with Anonymous Telemetry and Privacy section per request of mod stores.
* **Vapok.Common Dependency Bump**: Updated internalized dependency to `Vapok.Valheim.Common` 3.9.1012.

# 2.0.4 - Unified Splash Screen & Telemetry Controls
* **Unified Startup Splash Screen & Telemetry**:
  * Updated `Vapok.Valheim.Common` dependency reference to `v3.5.1012`.
  * Registered mod metadata with centralized `ModSplashManager`.
  * Added `ShowSplashOnStartup` and `Enable Anonymous Telemetry` configuration bindings to `ConfigRegistry`.

# 2.0.2 - Dependency & Compatibility Maintenance
* **Runtime & Dependency Updates**:
  * Synchronized package manifest and project references with Jotunn `2.30.0` and BepInEx `5.4.2350`.
  * Verified build pipeline and ILRepack bundling with `Vapok.Valheim.Common` `3.2.1012`.
* **Compatibility & Documentation**:
  * Validated shield auto-equipping triggers and inventory icon badge renderers against current Valheim 1.0 builds.
  * Standardized mod documentation, changelog tiers, and release staging.

# 2.0.1 - Container Quick Move Mark Persistence Fix
* **Quick Move Item Deserialization Guard**:
  * Fixed an issue where using Quick Move (<kbd>Ctrl</kbd> + Left Click) to transfer items into a container inadvertently cleared custom data marks on marked shields.
  * Preserved `CustomDataManager` metadata across inventory batch operations and rapid transfer hooks.

# 2.0.0 - Valheim 1.0+ Update & Shield Marking Mechanics
* **Valheim 1.0 Compatibility**:
  * Updated assembly references for Valheim 1.0 (`1.0.12`), BepInEx 5.4.2350, and Jotunn 2.30.0.
  * Rebuilt on .NET Framework 4.8.
  * Bundled `Vapok.Valheim.Common` 3.2.1012 via ILRepack.
* **Auto-Equip Mechanics Overhaul**:
  * Refactored shield selection mechanics: marking a new shield while holding a one-handed weapon with a shield equipped now instantly auto-swaps and equips the newly marked shield.
  * Added visual badge indicator persistence on inventory item icons for marked shields.
* **Harmony Intercept Patches**:
  * Updated Harmony patches on `Humanoid.EquipItem` and `Humanoid.UnequipItem` to handle one-handed weapon detection and secondary item slot pairing safely.

# 1.1.2 - Dependency Maintenance
* Updated runtime dependencies to latest versions.

# 1.1.1 - Dedicated Server Config Syncing Fix
* Resolved regression preventing server configuration synchronization from enforcing client settings on dedicated servers.
* Added `BepInDependency` flags for graceful handling.

# 1.1.0 - Valheim 0.221.4 & Unity Engine Updates
* Updated for Valheim 0.221.4 and Unity engine runtime adjustments.

# 1.0.0 - Initial Release of Shield Me Bruh!
* Initial release of automatic shield pairing and auto-equipping when wielding one-handed weapons.
* Implemented middle-click shield marking in player inventory.
