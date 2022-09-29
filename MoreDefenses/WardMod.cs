// MoreDefenses
// a Valheim mod skeleton using Jötunn
// 
// File:    MoreDefenses.cs
// Project: MoreDefenses

using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Managers;
using Jotunn.Utils;
using MoreDefenses.Scripts;

namespace MoreDefenses
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class WardMod : BaseUnityPlugin
    {
        public const string PluginGUID = "torky.Ward";
        public const string PluginName = "Ward";
        public const string PluginVersion = "0.2.0";

        public static string ModLocation = Path.GetDirectoryName(typeof(WardMod).Assembly.Location);

        private readonly Harmony m_harmony = new Harmony(PluginGUID);

        public void Awake()
        {
            m_harmony.PatchAll();
        }

        // Test
        [HarmonyPatch(typeof(PrivateArea), nameof(PrivateArea.IsPermitted))]
        class PrivateAreaTeam
        {
            static void Postfix(long playerID, ref bool __result, ref PrivateArea __instance)
            {
                if (__result) return;
                string creatorName = __instance.GetCreatorName();
                string playerName = Player.GetPlayer(playerID).GetPlayerName();
                __result = Teams.IsSameTeam(creatorName, playerName);
            }
        }
    }
}