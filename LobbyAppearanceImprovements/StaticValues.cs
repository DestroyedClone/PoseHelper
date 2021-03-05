using BepInEx;
using R2API.Utils;
using RoR2;
using BepInEx.Configuration;
using UnityEngine;
using System.Security;
using System.Security.Permissions;
using static UnityEngine.ColorUtility;
using static LobbyAppearanceImprovements.Helpers;
using System.Collections;
using System.Collections.ObjectModel;
using R2API;
using UnityEngine.Networking;
using System.Reflection;
using Path = System.IO.Path;
using R2API.Networking;
using UnityEngine.Playables;
using System;
using static UnityEngine.ScriptableObject;
using System.Linq;
using System.Collections.Generic;
using EntityStates;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using RoR2.Projectile;
using static UnityEngine.Animator;
using LeTai.Asset.TranslucentImage;

namespace LobbyAppearanceImprovements
{
    public static class StaticValues
    {
        // You may ask, why not combine the two?
        // That is because, my code sucks ass.

        public static Dictionary<string, float[]> textCameraSettings = new Dictionary<string, float[]>
        {
            {"Commando", new float[]{ 20, 2, 24 } },
            {"Huntress", new float[]{ 9, -3, 18 } },
            {"Toolbot", new float[]{ 10, -1, -1 } },
            {"Engi", new float[]{ 6, 1, -7.5f } },
            {"Mage", new float[]{ 8, -1, 13 } },
            {"Merc", new float[]{ 5, -8.5f, -3 } },
            {"Treebot", new float[]{ 6, 0.7f, -15.5f } },
            {"Loader", new float[]{ 11, -2, 20 } },
            {"Croco", new float[]{ 8, -8.5f, 13 } },
            {"Captain", new float[]{ 8, -1, 7 } },
            {"SniperClassic", new float[]{ 6, 0.5f, -12.5f } },
            {"Enforcer", new float[]{ 11, -1, 10 } },
            {"NemesisEnforcer", new float[]{ 10, -7.5f, 8 } },
            {"BanditReloaded", new float[]{ 20, 1, -30 } },
            {"HANDOverclocked", new float[]{ 8, -2, -4 } },
            {"Miner", new float[]{ 17, 1, -26 } },
            {"RobPaladin", new float[]{ 9, -1, -10 } },
            {"CHEF", new float[]{ 5, -8.5f, 3 } },
            {"RobHenry", new float[]{ 12, -7, -27 } },
            {"Wyatt", new float[]{ 12, -2, -22 } },
            {"Custodian", new float[]{ 12, -2, -22 } },
            {"Executioner", new float[]{ 10, -1, 5 } },
        };

        // BodyName + Position + Rotation
        public static Dictionary<string, Vector3[]> characterDisplaySettings = new Dictionary<string, Vector3[]>()
        {
            { "Commando", new [] {new Vector3(2.65f, 0.01f, 6.00f), new Vector3(0f, 240f, 0f) } },
            { "Huntress", new [] {new Vector3(4.8f, 1.43f, 15.36f), new Vector3(0f, 170f, 0f) } },
            { "Toolbot", new [] {new Vector3(-0.21f, 0.15f, 20.84f), new Vector3(0f, 170f, 0f) } },
            { "Engi", new [] {new Vector3(-2.58f, -0.01f, 19f), new Vector3(0f, 150f, 0f) } },
            { "Mage", new [] {new Vector3(3.35f, 0.21f, 14.73f), new Vector3(0f, 220f, 0f) } },
            { "Merc", new [] {new Vector3(-1.32f, 3.65f, 22.28f), new Vector3(0f, 180f, 0f) } },
            { "Treebot", new [] {new Vector3(-6.51f, -0.11f, 22.93f), new Vector3(0f, 140f, 0f) } },
            { "Loader", new [] {new Vector3(5.04f, 0, 14.26f), new Vector3(0f, 220f, 0f) } },
            { "Croco", new [] {new Vector3(5f, 3.59f, 22f), new Vector3(0f, 210f, 0f) } },
            { "Captain", new [] {new Vector3(2.21f, 0.01f, 19.40f), new Vector3(0f, 190f, 0f) } },
            //
            { "Enforcer", new [] {new Vector3(3.2f, 0f, 18.74f), new Vector3(0f, 220f, 0f) } },
            { "NemesisEnforcer", new [] {new Vector3(3f, 2.28f, 21f), new Vector3(0f, 200f, 0f) } },

            { "SniperClassic", new [] { new Vector3(-5f, 0.03f, 22f), new Vector3(0f, 180f, 0f) } },
            { "BanditReloaded", new [] {new Vector3(-3.5f, -0.06f, 5.85f), new Vector3(0f, 154f, 0f) } },
            { "HANDOverclocked", new [] { new Vector3(-1.57f, -0.038f, 20.48f), new Vector3(0f, 154f, 0f) } },
            { "Miner", new [] {new Vector3(-3.3f, 0.04f, 6.69f), new Vector3(0f, 140f, 0f) } },
            { "RobPaladin", new [] {new Vector3(-4f, 0.01f, 22f), new Vector3(0f, 160f, 0f) } },
            { "CHEF", new [] {new Vector3(1.63f, 3.4f, 23.2f), new Vector3(0f, 270f, 0f) } },
            { "RobHenry", new [] {new Vector3(-4.5f, 1.22f, 8.81f), new Vector3(0f, 128f, 0f) } },
            { "Wyatt", new [] {new Vector3(-3.92f, 0.1f, 9.62f), new Vector3(0f, 138f, 0f) } },
            { "Custodian", new [] {new Vector3(-3.92f, 0.1f, 9.62f), new Vector3(0f, 138f, 0f) } },
            { "Executioner", new [] {new Vector3(1.19f, 0f, 19.74f), new Vector3(0f, 192f, 0f) } },
        };

        public enum LobbyViewType
        {
            Default,
            Hide,
            Zoom
        }
    }
}
