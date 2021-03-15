using BepInEx.Configuration;
using UnityEngine;

namespace SurvivorTaunts.Modules
{
    public static class Config
    {
        public static ConfigEntry<KeyCode> displayKeybind;
        public static ConfigEntry<KeyCode> poseKeybind;

        public static void ReadConfig()
        {
            displayKeybind = STPlugin.instance.Config.Bind<KeyCode>(new ConfigDefinition("Keybinds", "Display"), KeyCode.Alpha1, new ConfigDescription("Keybind used to perform the Display emote"));
            poseKeybind = STPlugin.instance.Config.Bind<KeyCode>(new ConfigDefinition("Keybinds", "Pose"), KeyCode.Alpha3, new ConfigDescription("Keybind used to perform the Pose emote"));
        }
    }
}
