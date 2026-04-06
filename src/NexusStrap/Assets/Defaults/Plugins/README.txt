NexusStrap plugins
==================

Drop a plugin folder here containing:

  manifest.json   — metadata (see PluginManifest in the NexusStrap source)
  YourPlugin.dll  — assembly implementing IPlugin from NexusStrap.PluginSDK

The host loads *.dll from each subfolder on startup when plugins are enabled in settings.

This folder is seeded on first run so you know where plugins live. Remove this file if you like.
