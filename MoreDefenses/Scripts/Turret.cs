using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MoreDefenses;
using MoreDefenses.Models;
using MoreDefenses.Scripts;
using UnityEngine;

public class Turret : MonoBehaviour, Hoverable, Interactable
{
    public float Range = 20f;
    public float FireInterval = 0.5f;
    public float Damage = 0f;
    public float PierceDamage = 0f;
    public float FireDamage = 0f;
    public float FrostDamage = 0f;
    public float LightningDamage = 0f;
    public float PoisonDamage = 0f;
    public float SpiritDamage = 0f;
    public float DamageRadius = 0f;
    public bool CanShootFlying = true;
    public bool IsContinuous = false;
    public TurretType Type = TurretType.Gun;
    public enum TurretType
    {
        Gun,
        Cannon,
        Projectile,
        Flamethrower
    }

    private HitData m_hitData;

    private readonly float m_targetUpdateInterval = 0.5f;

    private readonly int m_viewBlockMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "terrain", "viewblock", "vehicle");
    private readonly int m_rayMaskSolids = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "terrain", "character", "character_net", "character_ghost", "hitbox", "character_noenv", "vehicle");
    private Character m_target;
    private float m_updateTargetTimer;
    private float m_shootTimer;
    private bool m_isFiring;

    private AudioSource m_audioSource;
    private ParticleSystem m_outputParticleSystem;
    private ParticleSystem m_impactParticleSystem;
    private ParticleSystem m_projectileParticleSystem;
    private Bounds m_bounds;

    private ZNetView m_nview;
    private Piece m_piece;
    private string m_name = "Turret";

    // Debug targeting
    //private LineRenderer m_lineRenderer;

    public void Awake()
    {
        // Debug targeting
        //m_lineRenderer = gameObject.AddComponent<LineRenderer>();
        //m_lineRenderer.startWidth = 0.1f;
        //m_lineRenderer.endWidth = 0.1f;

        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.outputAudioMixerGroup = AudioMan.instance.m_ambientMixer;
        m_audioSource.spatialBlend = 1f;
        m_audioSource.rolloffMode = AudioRolloffMode.Linear;
        m_audioSource.maxDistance = 100f;
        SetVolume();
        Mod.TurretVolume.SettingChanged += SetVolume;


        m_bounds = GetComponent<BoxCollider>().bounds;

        m_nview = GetComponent<ZNetView>();
        m_nview.Register<Vector3>("Fire", RPC_Fire);
        m_nview.Register("StopFire", RPC_StopFire);
        m_nview.Register<long>("ToggleEnabled", RPC_ToggleEnabled);
        m_nview.Register<long, string>("TogglePermitted", RPC_TogglePermitted);

        m_piece = GetComponent<Piece>();

        m_hitData = new HitData
        {
            m_damage = new HitData.DamageTypes
            {
                m_damage = Damage,
                m_pierce = PierceDamage,
                m_fire = FireDamage,
                m_frost = FrostDamage,
                m_lightning = LightningDamage,
                m_poison = PoisonDamage,
                m_spirit = SpiritDamage,
            },
            m_blockable = true,
        };

        

        m_outputParticleSystem = transform.Find("OutputParticleSystem")?.GetComponent<ParticleSystem>();
        if (m_outputParticleSystem == null) m_outputParticleSystem = transform.Find("Particle System")?.GetComponent<ParticleSystem>();

        m_impactParticleSystem = transform.Find("ImpactParticleSystem")?.GetComponent<ParticleSystem>();

        m_projectileParticleSystem = transform.Find("ProjectileParticleSystem")?.GetComponent<ParticleSystem>();
        if (m_projectileParticleSystem != null)
        {
            var projectileParticle = m_projectileParticleSystem.gameObject.AddComponent<ProjectileParticle>();
            projectileParticle.HitData = m_hitData;
            projectileParticle.SourceTurret = this;
        }
    }

    public void Start()
    {
        if (Game.instance.GetPlayerProfile().GetPlayerID() == m_piece.GetCreator())
        {
            Setup(Game.instance.GetPlayerProfile().m_playerName);
        }
    }

    public void Initialize(TurretConfig turretConfig)
    {
        Type = (TurretType)Enum.Parse(typeof(TurretType), turretConfig.type, true);
        Range = turretConfig.range;
        Damage = turretConfig.damage;
        PierceDamage = turretConfig.pierceDamage;
        FireDamage = turretConfig.fireDamage;
        FrostDamage = turretConfig.frostDamage;
        LightningDamage = turretConfig.lightningDamage;
        PoisonDamage = turretConfig.poisonDamage;
        SpiritDamage = turretConfig.spiritDamage;
        FireInterval = turretConfig.fireInterval;
        DamageRadius = turretConfig.damageRadius;
        CanShootFlying = turretConfig.canShootFlying;
        IsContinuous = turretConfig.isContinuous;
        m_name = turretConfig.name;
    }

    public bool IsOwner()
    {
        if (m_nview == null || !m_nview.IsOwner())
        {
            return false;
        }

        return true;
    }

    private void SetVolume(object sender, EventArgs e)
    {
        SetVolume();
    }

    private void SetVolume()
    {
        //Jotunn.Logger.LogDebug(Mod.TurretVolume.Value);
        m_audioSource.volume = Mod.TurretVolume.Value * 0.005f;
    }

    public void OnDestroy()
    {
        Mod.TurretVolume.SettingChanged -= SetVolume;
    }

    public void Update()
    {
        if (m_nview == null || !m_nview.IsOwner())
        {
            return;
        }

        if (!IsEnabled())
        {
            m_target = null;
            if (IsContinuous && m_isFiring) m_nview.InvokeRPC(ZNetView.Everybody, "StopFire");
            return;
        }

        if (m_target == null)
        {
            if (IsContinuous && m_isFiring) m_nview.InvokeRPC(ZNetView.Everybody, "StopFire");
            if (m_updateTargetTimer < 0)
            {
                StartCoroutine(FindTarget());
                m_updateTargetTimer = m_targetUpdateInterval;
            }
        } 
        
        if (m_target != null)
        {
            if (m_target.IsDead() || !IsCharacterInRange(m_target) || !CanSeeCharacter(m_target))
            {
                m_target = null;
                if (IsContinuous) m_nview.InvokeRPC(ZNetView.Everybody, "StopFire");
                //Jotunn.Logger.LogDebug("Target lost");
            }
            else
            {
                transform.LookAt(m_target.transform);

                if (m_shootTimer < 0)
                {
                    if (!IsContinuous) m_nview.InvokeRPC(ZNetView.Everybody, "Fire", m_target.transform.position);
                    switch (Type)
                    {
                        case TurretType.Gun:
                            m_target.Damage(m_hitData);
                            break;
                        case TurretType.Cannon:
                            DamageAreaTargets(m_target.transform.position);
                            break;
                        case TurretType.Flamethrower:
                            DamageFlamethrowerTargets();
                            break;
                        default:
                            break;
                    }

                    // Debug targeting
                    //m_lineRenderer.SetPosition(0, Bounds.center);
                    //m_lineRenderer.SetPosition(1, Target.GetCenterPoint());

                    //Jotunn.Logger.LogDebug("Fire");

                    m_shootTimer = FireInterval;
                }
            }
        }

        m_updateTargetTimer -= Time.deltaTime;
        m_shootTimer -= Time.deltaTime;
    }

    private IEnumerator FindTarget()
    {
        List<Character> allCharacters = Character.GetAllCharacters();
        foreach (Character character in allCharacters)
        {
            if (character == null) continue;
            var isPlayer = character.m_faction == Character.Faction.Players;
            var isViablePlayerTarget = false;
            if (isPlayer)
            {
                // Jotunn.Logger.LogDebug($"Player?! {character.m_name}");
                Player player = character as Player;
                Player creator = Player.GetPlayer(m_piece.GetCreator());

                bool isPermitted = IsPermitted(player.GetPlayerID());
                bool isCreator = player.GetPlayerID() == m_piece.GetCreator();
                bool isSameTeam = Teams.IsSameTeam(player.GetPlayerName(), creator.GetPlayerName());
                isViablePlayerTarget = !isPermitted && !isCreator && !isSameTeam;
            }
            if (
                (!isPlayer || isViablePlayerTarget)
                && (CanShootFlying || !character.IsFlying())
                && !character.IsTamed()
                && !character.IsDead()
                && IsCharacterInRange(character)
                && CanSeeCharacter(character)
                && IsEnabled()
            ) {
                Jotunn.Logger.LogDebug($"Target changed to {character.m_name}");
                m_target = character;
                if (IsContinuous) m_nview.InvokeRPC(ZNetView.Everybody, "Fire", m_target.transform.position);
                yield break;
            }
        }
    }

    private bool IsCharacterInRange(Character character)
    {
        var zDiff = character.transform.position.z - transform.position.z;
        var xDiff = character.transform.position.x - transform.position.x;
        return (zDiff * zDiff) + (xDiff * xDiff) <= Range * Range;
    }

    private bool CanSeeCharacter(Character character)
    {
        var vector = character.GetCenterPoint() - m_bounds.center;
        return !Physics.Raycast(m_bounds.center, vector.normalized, vector.magnitude, m_viewBlockMask);
    }

    private void RPC_Fire(long sender, Vector3 impactPosition)
    {
        m_audioSource.Play();
        if (m_outputParticleSystem != null) m_outputParticleSystem.Play();
        if (m_projectileParticleSystem != null) m_projectileParticleSystem.Play();
        if (m_impactParticleSystem != null)
        {
            m_impactParticleSystem.transform.position = impactPosition;
            m_impactParticleSystem.Play();
        }

        if (IsContinuous)
        {
            m_isFiring = true;
        }
    }

    private void RPC_StopFire(long sender)
    {
        m_audioSource.Stop();
        if (m_outputParticleSystem != null && !m_outputParticleSystem.isStopped) m_outputParticleSystem.Stop();
        if (m_projectileParticleSystem != null && !m_projectileParticleSystem.isStopped) m_projectileParticleSystem.Stop();

        m_isFiring = false;
    }

    public void SetVolume(float volume)
    {
        m_audioSource.volume = volume / 100;
    }

    private void DamageAreaTargets(Vector3 position)
    {
        var hits = Physics.OverlapSphere(position, DamageRadius, m_rayMaskSolids);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Character character) && !character.IsTamed() && !character.IsDead())
            {
                character.Damage(m_hitData);
            }
        }
    }

    private void DamageFlamethrowerTargets()
    {
        var hits = Physics.OverlapCapsule(transform.position, transform.position + transform.forward * Range, 1, m_rayMaskSolids);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Character character) && !character.IsTamed() && !character.IsDead())
            {
                character.Damage(m_hitData);
            }
        }
    }

    public string GetHoverText()
    {
        if (!m_nview.IsValid())
        {
            return "";
        }
        if (Player.m_localPlayer == null)
        {
            return "";
        }
        StringBuilder stringBuilder = new StringBuilder(256);
        string ownerText = "\n$piece_guardstone_owner:" + GetCreatorName() + " -> Team " + Teams.GetPlayerTeam(GetCreatorName());
        if (m_piece.IsCreator())
        {
            if (IsEnabled())
            {
                stringBuilder.Append(m_name + " ( $piece_guardstone_active )");
                stringBuilder.Append(ownerText);
                stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_deactivate");
            }
            else
            {
                stringBuilder.Append(m_name + " ( $piece_guardstone_inactive )");
                stringBuilder.Append(ownerText);
                stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_activate");
            }
        }
        else if (IsEnabled())
        {
            stringBuilder.Append(m_name + " ( $piece_guardstone_active )");
            stringBuilder.Append(ownerText);
        }
        else
        {
            stringBuilder.Append(m_name + " ( $piece_guardstone_inactive )");
            stringBuilder.Append(ownerText);
            if (IsPermitted(Player.m_localPlayer.GetPlayerID()))
            {
                stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_remove");
            }
            else
            {
                stringBuilder.Append("\n[<color=yellow><b>$KEY_Use</b></color>] $piece_guardstone_add");
            }
        }
        AddUserList(stringBuilder);
        return Localization.instance.Localize(stringBuilder.ToString());
    }

    public void Setup(string name)
    {
        m_nview.GetZDO().Set("creatorName", name);
    }

    private string GetCreatorName()
    {
        return m_nview.GetZDO().GetString("creatorName");
    }

    private void AddUserList(StringBuilder text)
    {
        List<KeyValuePair<long, string>> permittedPlayers = GetPermittedPlayers();
        text.Append("\n$piece_guardstone_additional: ");
        for (int i = 0; i < permittedPlayers.Count; i++)
        {
            text.Append(permittedPlayers[i].Value);
            if (i != permittedPlayers.Count - 1)
            {
                text.Append(", ");
            }
        }
    }

    public string GetHoverName()
    {
        throw new NotImplementedException();
    }

    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        Player player = user as Player;
        if (m_piece.IsCreator())
        {
            m_nview.InvokeRPC("ToggleEnabled", player.GetPlayerID());
            if (m_name == string.Empty)
            {
                Setup(player.GetPlayerName());
            }
            return true;
        }
        if (IsEnabled())
        {
            return false;
        }
        m_nview.InvokeRPC("TogglePermitted", player.GetPlayerID(), player.GetPlayerName());
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item)
    {
        throw new NotImplementedException();
    }

    private void RPC_TogglePermitted(long uid, long playerID, string name)
    {
        if (m_nview.IsOwner() && !IsEnabled())
        {
            if (IsPermitted(playerID))
            {
                RemovePermitted(playerID);
            }
            else
            {
                AddPermitted(playerID, name);
            }
        }
    }

    private void RPC_ToggleEnabled(long uid, long playerID)
    {
        ZLog.Log("Toggle enabled from " + playerID + "  creator is " + m_piece.GetCreator());
        if (m_nview.IsOwner() && m_piece.GetCreator() == playerID)
        {
            SetEnabled(!IsEnabled());
        }
    }

    public bool IsEnabled()
    {
        if (!m_nview.IsValid())
        {
            return false;
        }
        return m_nview.GetZDO().GetBool("enabled");
    }

    private void SetEnabled(bool enabled)
    {
        m_nview.GetZDO().Set("enabled", enabled);
    }

    private void RemovePermitted(long playerID)
    {
        List<KeyValuePair<long, string>> permittedPlayers = GetPermittedPlayers();
        if (permittedPlayers.RemoveAll((KeyValuePair<long, string> x) => x.Key == playerID) > 0)
        {
            SetPermittedPlayers(permittedPlayers);
        }
    }

    private bool IsPermitted(long playerID)
    {
        foreach (KeyValuePair<long, string> permittedPlayer in GetPermittedPlayers())
        {
            if (permittedPlayer.Key == playerID)
            {
                return true;
            }
        }
        return false;
    }

    private void AddPermitted(long playerID, string playerName)
    {
        List<KeyValuePair<long, string>> permittedPlayers = GetPermittedPlayers();
        foreach (KeyValuePair<long, string> item in permittedPlayers)
        {
            if (item.Key == playerID)
            {
                return;
            }
        }
        permittedPlayers.Add(new KeyValuePair<long, string>(playerID, playerName));
        SetPermittedPlayers(permittedPlayers);
    }

    private void SetPermittedPlayers(List<KeyValuePair<long, string>> users)
    {
        m_nview.GetZDO().Set("permitted", users.Count);
        for (int i = 0; i < users.Count; i++)
        {
            KeyValuePair<long, string> keyValuePair = users[i];
            m_nview.GetZDO().Set("pu_id" + i, keyValuePair.Key);
            m_nview.GetZDO().Set("pu_name" + i, keyValuePair.Value);
        }
    }

    private List<KeyValuePair<long, string>> GetPermittedPlayers()
    {
        List<KeyValuePair<long, string>> list = new List<KeyValuePair<long, string>>();
        int @int = m_nview.GetZDO().GetInt("permitted");
        for (int i = 0; i < @int; i++)
        {
            long @long = m_nview.GetZDO().GetLong("pu_id" + i, 0L);
            string @string = m_nview.GetZDO().GetString("pu_name" + i);
            if (@long != 0L)
            {
                list.Add(new KeyValuePair<long, string>(@long, @string));
            }
        }
        return list;
    }
}
