﻿using BepInEx.Configuration;
using UnityEngine;

namespace SurvivorTaunts.Modules
{
    public static class Config
    {
        public static ConfigEntry<KeyCode> displayKeybind;
        public static ConfigEntry<KeyCode> poseKeybind;
        public static ConfigEntry<KeyCode> disablePoseKeybind;

        public static void ReadConfig()
        {
            displayKeybind = STPlugin.instance.Config.Bind<KeyCode>(new ConfigDefinition("Keybinds", "Display"), KeyCode.Alpha1, new ConfigDescription("Keybind used to perform the Display emote"));
            poseKeybind = STPlugin.instance.Config.Bind<KeyCode>(new ConfigDefinition("Keybinds", "Pose"), KeyCode.Alpha2, new ConfigDescription("Keybind used to perform the Pose emote"));
            disablePoseKeybind = STPlugin.instance.Config.Bind("Keybinds", "Toggle", KeyCode.Alpha0, "Toggle the displays on/off to allow certain modded characters who have existing taunts to still taunt.");
        }
    }
}
