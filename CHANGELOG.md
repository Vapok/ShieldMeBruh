# 2.1.0 - 1-Handed Weapon Exclusion & Red X Badges
* **1-Handed Weapon Exclusion**: Added ability to middle-click any one-handed weapon (such as woodcutting axes) in your inventory to mark it as excluded from automatic shield deployment.
* **Red X Visual Indicator**: Excluded weapons display a crisp red X badge in the inventory grid (matching the shield badge styling) that seamlessly follows the item across slot moves and swaps.
* **Multi-Weapon Support**: Multiple one-handed weapons can be excluded simultaneously while retaining your single designated auto-equip shield.
* **Configurable**: Added `Enable Weapon Exclusion` setting under `Local Config` to toggle the feature on or off as desired.

<details>
<summary><b>2.0 Changelog History (Valheim Release)</b> (<i>click to expand</i>)</summary>

### 2.0.8 - Dedicated Server AutoShield Reset Fix & Valheim 1.0.15 Alignment
* **Dedicated Server AutoShield Reset Fix**: Added null-conditional invocation for `OnResetEvent` during `Player.SetLocalPlayer`, eliminating `NullReferenceException` crashes on dedicated servers when no client UI handlers are registered.
* **Valheim 1.0.15 Alignment**: Updated all game assembly references and internalized `Vapok.Valheim.Common` 3.13.1015.

### 2.0.7 - Splash Window Updates & Valheim 1.0.14 Alignment
* **Splash Window Updates**:
  * Telemetry is now unchecked when first loaded (Opt-In visibility)
  * Added Send Error Logs (Opt-Out)
  * Privacy Policy is now available directly in-game
  * Added Data Disclaimers on hover over checkboxes for transparency on what data is sent
* **Valheim 1.0.14 Alignment**: Updated game assembly references and internalized  3.12.1014.


### 2.0.6 - Jewelcrafting Font Compatibility
* Fixed: Jewelcrafting packages it's own font which was overriding part of a vanilla font, causing the Splash screen to appear blank.
### 2.0.5 - Updated README with Telemetry Information
* Updated the README.md with Anonymous Telemetry information per request of mod stores.

### 2.0.4 - Unified Splash Screen & Telemetry Controls
* **Unified Startup Splash Screen**: Integrated with a centralized startup splash screen.
  * Added configurable `Show on Game Startup` which can be enabled or disabled in the configuration file.
* **Anonymous Telemetry**: 
  * Added configurable `Enable Anonymous Telemetry` configuration which can be enabled or disabled in the configuration file.
    * Defaults to enabled with auto-opt-in on launch. Uncheck to Opt-Out
    * ANONYMOUS DATA ONLY - I track version number and usage data. No personal data is ever collected. For more information, see the [Privacy Policy](https://vapok.io/privacy-policy/).

### 2.0.2 - Dependency & Compatibility Maintenance
* **Dependency Updates**: Updated Jotunn and BepInEx runtime package bindings.
* **Compatibility Maintenance**: Verified compatibility against the latest Valheim 1.0 release.
* **Documentation Improvements**: Standardized README, user guides, and technical patch documentation.

### 2.0.1 - Container Quick Move Mark Persistence Fix
* Fixed: Resolved an issue where using Quick Move (<kbd>Ctrl</kbd> + Left Click) on items into a container would inadvertently clear the mark from a marked shield.

### 2.0.0 - Valheim 1.0+ Update & Shield Marking Mechanics
* Updated codebase for Valheim 1.0.
* Adjusted shield marking mechanics: marking a new shield while holding a one-handed weapon now instantly auto-equips the newly marked shield.
* Added visual icon badges on marked shields in player inventory.

</details>

<details>
<summary><b>1.0 Changelog History (Valheim Early Access)</b> (<i>click to expand</i>)</summary>

### 1.1.2 - Dependency Maintenance
* Updated all dependencies to latest versions.

### 1.1.1 - Dedicated Server Config Syncing Fix
* Resolved an issue preventing dedicated servers from properly enforcing configuration settings on connected clients.
* Added graceful dependency handling and notifications.

### 1.1.0 - Valheim 0.221.4 & Unity Engine Updates
* Updated for Valheim 0.221.4 and Unity runtime adjustments.

### 1.0.0 - Initial Release of Shield Me Bruh!
* Initial release of automatic shield pairing and auto-equipping when wielding one-handed weapons.
* Implemented middle-click shield marking in player inventory.

</details>
