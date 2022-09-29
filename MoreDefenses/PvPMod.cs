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
    internal class PvPMod : BaseUnityPlugin
    {
        public const string PluginGUID = "torky.PvP";
        public const string PluginName = "PvP";
        public const string PluginVersion = "0.2.0";

        public static string ModLocation = Path.GetDirectoryName(typeof(PvPMod).Assembly.Location);

        private readonly Harmony m_harmony = new Harmony(PluginGUID);

        public void Awake()
        {
            m_harmony.PatchAll();
        }

        // Test
        [HarmonyPatch(typeof(Player), nameof(Player.OnDamaged))]
        class PlayerHudFlash
        {
            static void Postfix()
            {
                Hud.instance.DamageFlash();
            }
        }

        // Test
        [HarmonyPatch(typeof(Player), nameof(Player.IsPVPEnabled))]
        class PVPOn
        {
            static void Prefix(ref bool ___m_pvp)
            {
                ___m_pvp = true;
            }

            static void Postfix(ref bool __result)
            {
                __result = true;
            }
        }

        // Testing
        [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
        class PlayerMeleeDamage
        {
            static void Prefix(ref Player __instance, HitData hit)
            {
                if (
                    hit == null 
                    || __instance == null
                    || hit.GetAttacker() == null
                    || !hit.GetAttacker().IsPlayer()
                    || !__instance.IsPlayer())
                {
                    return;
                }

                Player victim = __instance;
                Player attacker = hit.GetAttacker() as Player;
                bool cantDamage = Teams.IsSameTeam(victim.GetPlayerName(), attacker.GetPlayerName());

                if (!hit.m_ranged)
                {
                    hit.ApplyModifier(1.4f);
                }

                if (cantDamage)
                {
                    // hit.ApplyModifier(0);
                    Jotunn.Logger.LogInfo("Friendly Fire!");
                }
            }

            static void Postfix(HitData hit)
            {
                Jotunn.Logger.LogDebug("TotalDmg:" + hit.GetTotalDamage() + ", TotalPhys:" + hit.GetTotalPhysicalDamage() + ", TotalBlockable: " + hit.GetTotalBlockableDamage());
            }
        }
    }
}