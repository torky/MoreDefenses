// MoreDefenses
// a Valheim mod skeleton using Jötunn
// 
// File:    MoreDefenses.cs
// Project: MoreDefenses

using System.IO;
using BepInEx;
using HarmonyLib;

namespace MoreDefenses
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInProcess("valheim.exe")]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class TeamsMod : BaseUnityPlugin
    {
        public const string PluginGUID = "torky.Teams";
        public const string PluginName = "Teams";
        public const string PluginVersion = "0.1.0";

        public static string ModLocation = Path.GetDirectoryName(typeof(TeamsMod).Assembly.Location);

        private readonly Harmony m_harmony = new Harmony(PluginGUID);

        public void Awake()
        {
            m_harmony.PatchAll();
        }

        // Test
        [HarmonyPatch(typeof(Player), nameof(Player.IsPVPEnabled))]
        class PlayerPvPPatch
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

       /* // Damage methods PVP OVERRIDE BABY
        [HarmonyPatch(typeof(Player), nameof(Player.RPC_Damage))]
        class PlayerDamagePatch
        {
            static void Prefix(
                ref Player __instance,
                ref ZNetView ___m_nview,
                ref bool ___m_pvp,
                ref HitData __hit)
            {
                Character attacker = __hit.GetAttacker();
                int attackerTeam = attacker.m_nview.GetZDO().GetInt("test");
                int instanceTeam = ___m_nview.GetZDO().GetInt("test");
                ___m_pvp = 
                    __instance.IsPlayer() && attacker.IsPlayer()
                    && (instanceTeam == 0 || instanceTeam != attackerTeam);
                __instance.SetPVP(___m_pvp);
            }
        }

        [HarmonyPatch(typeof(Attack), nameof(Attack.DoAreaAttack))]
        class AttackAreaDamage
        {
            static void Prefix(
                ref Humanoid ___m_character)
            {
                if (!___m_character.IsPlayer()) return;
                (___m_character as Player).m_pvp = true;
                (___m_character as Player).SetPVP(true);
            }
        }
        [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
        class AttackMeleeDamage
        {
            static void Prefix(
                ref Humanoid ___m_character)
            {
                if (!___m_character.IsPlayer()) return;
                (___m_character as Player).m_pvp = true;
                (___m_character as Player).SetPVP(true);
            }
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.IsValidTarget))]
        class ProjectileDamagePatch
        {
            static void Postfix(
                ref IDestructible __destr,
                ref Character ___m_owner,
                ref bool __hitCharacter)
            {
                Character character = __destr as Character;
                if (!(bool)character || !character.IsPlayer() || !___m_owner.IsPlayer()) return;

                __hitCharacter = true;
            }
        }*/
    }
}