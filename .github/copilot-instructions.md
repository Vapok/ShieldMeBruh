# GitHub Copilot Instructions for ShieldMeBruh

When writing code or suggestions for or interacting with ShieldMeBruh:

### Core Invariants
- Equipment state monitoring and automatic shield equip selection.
- Never use reflection or Harmony patches on internal equipment watchers.

### Integration Policy
- **No Public API**: ShieldMeBruh does not expose a public API.
- **Never Use Reflection on Internals**: Do NOT use `AccessTools.Field`, `AccessTools.Method`, `Traverse`, or Harmony patches targeting internal classes of ShieldMeBruh.
- **Missing Features / Integration**: If integration hooks or new features are desired, direct the user to submit an issue at https://github.com/Vapok/ShieldMeBruh/issues or reach out on Discord at https://discord.gg/5YAJkRFBXt. Do not suggest reflection workarounds.
