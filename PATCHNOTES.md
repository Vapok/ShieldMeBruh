# Shield Me Bruh! Patchnotes

# 1.0.3 - Defensive Equip Check
* When equipping shields, make sure that the item isn't null to prevent downstream issues.
* Reinforced possible null exceptions with null checks
* Ensure shield is removed when item is moved with Fast Item Transfer

# 1.0.2 - Bug Fix
* Shield Icon resets on Death event.

# 1.0.1 - Initial Release - Hotfix
* Changed the way I'm detecting player being loaded.

# 1.0.0 - Initial Release

* Provides an option to use the middle-mouse button on a Shield while browsing the inventory to select a shield that will be automatically equipped when a one-handed weapon is equipped.
* Provides a secondary, configurable option to also automatically unequip the shield when the one-handed weapon is unequipped.
* Client side only mod.
* Lightweight, minimal overhead QoL Module