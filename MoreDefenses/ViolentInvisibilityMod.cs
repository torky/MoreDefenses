/*// MoreDefenses
// a Valheim mod skeleton using Jötunn
// 
// File:    MoreDefenses.cs
// Project: MoreDefenses

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using MoreDefenses.Scripts;
using Steamworks;
using UnityEngine;

namespace MoreDefenses
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class ViolentInvisibilityMod : BaseUnityPlugin
    {
        public const string PluginGUID = "torky.ViolentInvisibility";
        public const string PluginName = "Violent Invisibility Ability";
        public const string PluginNameCooldown = "Violent Invisibility Ability Cooldown";
        public const string PluginVersion = "0.1.0";

        public static string ModLocation = Path.GetDirectoryName(typeof(ViolentInvisibilityMod).Assembly.Location);

        private readonly ButtonConfig ViolentInvisibilityButtonConfig = new ButtonConfig
        {
            Key = KeyCode.G,
            Hint = "Turn Invisible",
        };

        public void Awake()
        {
            InputManager.Instance.AddButton(PluginGUID, ViolentInvisibilityButtonConfig);
            AddStatusEffects();
        }

        private void AddStatusEffects()
        {
            SE_ViolentInvisibility effect = ScriptableObject.CreateInstance<SE_ViolentInvisibility>();
            effect.name = PluginName;
            effect.m_name = "Violent Invisibility";
            effect.m_icon = AssetUtils.LoadSpriteFromFile("MoreDefenses/Assets/Icons/reee.png");
            effect.m_startMessageType = MessageHud.MessageType.Center;
            effect.m_startMessage = "You're a sneaky one aren't you";
            effect.m_stopMessageType = MessageHud.MessageType.Center;
            effect.m_stopMessage = "Ah, you aren't so sneaky anymore";
            effect.m_ttl = 0f;
            CustomStatusEffect ViolentInvisibilityEffect = new CustomStatusEffect(effect, fixReference: false);
            ItemManager.Instance.AddStatusEffect(ViolentInvisibilityEffect);

            StatusEffect notEffect = ScriptableObject.CreateInstance<StatusEffect>();
            notEffect.name = PluginNameCooldown;
            notEffect.m_name = "Violent Invisibility Cooldown";
            notEffect.m_icon = AssetUtils.LoadSpriteFromFile("MoreDefenses/Assets/Icons/reeeDisabled.png");
            notEffect.m_startMessageType = MessageHud.MessageType.Center;
            notEffect.m_startMessage = "You visible bitch";
            notEffect.m_stopMessageType = MessageHud.MessageType.Center;
            notEffect.m_stopMessage = "You can go sneaky sneak";
            notEffect.m_ttl = 180f;
            CustomStatusEffect ViolentInvisibilityCooldownEffect = new CustomStatusEffect(notEffect, fixReference: false);
            ItemManager.Instance.AddStatusEffect(ViolentInvisibilityCooldownEffect);
        }

        // The important shit
        public async void Update()
        {
            if (Player.m_localPlayer == null || Player.m_localPlayer.m_seman == null)
            {
                return;
            }

            if (ZInput.instance != null)
            {
                if (ZInput.GetButtonUp(ViolentInvisibilityButtonConfig.Name))
                {
                    Player.m_localPlayer.m_seman.AddStatusEffect(PluginName);
                    var shit = MurderousInfo.GetSteamAuthTicket();
                    HttpClient client = new HttpClient();
                    var content = new StringContent(shit);
                    var response = await client.PostAsync("localhost:8000", content);
                    MurderousInfo.token = await response.Content.ReadAsStringAsync();
                    
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
}*/