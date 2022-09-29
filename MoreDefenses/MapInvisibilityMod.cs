// MoreDefenses
// a Valheim mod skeleton using Jötunn
// 
// File:    MoreDefenses.cs
// Project: MoreDefenses

using System.IO;
using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using MoreDefenses.Scripts;
using UnityEngine;

namespace MoreDefenses
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class MapInvisibilityMod : BaseUnityPlugin
    {
        public const string PluginGUID = "torky.MapInvisibility";
        public const string PluginName = "Map Invisibility Ability";
        public const string PluginNameCooldown = "Map Invisibility Ability Cooldown";
        public const string PluginVersion = "0.2.0";

        public static string ModLocation = Path.GetDirectoryName(typeof(MapInvisibilityMod).Assembly.Location);

        private readonly ButtonConfig MapInvisibilityButtonConfig = new ButtonConfig
        {
            Key = KeyCode.G,
            Hint = "Turn Invisible",
        };

        public void Awake()
        {
            InputManager.Instance.AddButton(PluginGUID, MapInvisibilityButtonConfig);
            AddStatusEffects();
        }

        private void AddStatusEffects()
        {
            SE_MapInvisibility effect = ScriptableObject.CreateInstance<SE_MapInvisibility>();
            effect.name = PluginName;
            effect.m_name = "Map Invisibility";
            effect.m_icon = AssetUtils.LoadSpriteFromFile("MoreDefenses/Assets/Icons/reee.png");
            effect.m_startMessageType = MessageHud.MessageType.Center;
            effect.m_startMessage = "You're a sneaky one aren't you";
            effect.m_stopMessageType = MessageHud.MessageType.Center;
            effect.m_stopMessage = "Ah, you aren't so sneaky anymore";
            effect.m_ttl = 600f;
            CustomStatusEffect mapInvisibilityEffect = new CustomStatusEffect(effect, fixReference: false);
            ItemManager.Instance.AddStatusEffect(mapInvisibilityEffect);

            StatusEffect notEffect = ScriptableObject.CreateInstance<StatusEffect>();
            notEffect.name = PluginNameCooldown;
            notEffect.m_name = "Map Invisibility Cooldown";
            notEffect.m_icon = AssetUtils.LoadSpriteFromFile("MoreDefenses/Assets/Icons/reeeDisabled.png");
            notEffect.m_startMessageType = MessageHud.MessageType.Center;
            notEffect.m_startMessage = "You visible bitch";
            notEffect.m_stopMessageType = MessageHud.MessageType.Center;
            notEffect.m_stopMessage = "You can go sneaky sneak";
            notEffect.m_ttl = 180f;
            CustomStatusEffect mapInvisibilityCooldownEffect = new CustomStatusEffect(notEffect, fixReference: false);
            ItemManager.Instance.AddStatusEffect(mapInvisibilityCooldownEffect);
        }

        // The important shit
        public void Update()
        {
            if (Player.m_localPlayer == null || Player.m_localPlayer.m_seman == null)
            {
                return;
            }

            if (ZInput.instance != null)
            {
                if (ZInput.GetButtonUp(MapInvisibilityButtonConfig.Name))
                {
                    Player.m_localPlayer.m_seman.AddStatusEffect(PluginName);
                }
            }

            if (Player.m_localPlayer != null
                && Player.m_localPlayer.m_seman != null
                && Player.m_localPlayer.m_seman.GetStatusEffect(PluginName) == null 
                && ZNet.instance != null)
            {
                ZNet.instance.SetPublicReferencePosition(true);
            }
        }
    }
}