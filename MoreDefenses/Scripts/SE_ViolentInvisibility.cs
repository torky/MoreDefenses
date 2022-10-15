/*namespace MoreDefenses.Scripts
{
    public class SE_ViolentInvisibility : StatusEffect
    {
        Player m_player;

        public override void Setup(Character character)
        {
            if (character.IsPlayer())
            {
                m_player = character as Player;
                if (!string.IsNullOrEmpty(m_startMessage))
                {
                    m_player.Message(m_startMessageType, m_startMessage);
                }
            }
        }

        // Why does it break on stop?
        public override void UpdateStatusEffect(float dt)
        {
            base.UpdateStatusEffect(dt);
            if (
                m_player != null
                && m_time <= m_ttl
                && ZNet.instance != null
                && ZNet.instance.isActiveAndEnabled)
            {
                ZNet.instance.SetPublicReferencePosition(false);
            }
            else if (m_player != null && m_player.m_seman != null)
            {
                m_player.m_seman.AddStatusEffect(ViolentInvisibilityMod.PluginNameCooldown);
                m_player.m_seman.RemoveStatusEffect(ViolentInvisibilityMod.PluginName);
            }
        }

        public override void Stop()
        {
            // This works, keep
            return;
        }

        public override bool IsDone()
        {
            return false;
        }

        public override bool CanAdd(Character character)
        {
            return 
                character != null
                && character.m_seman != null
                && !character.m_seman.HaveStatusEffect(ViolentInvisibilityMod.PluginNameCooldown) 
                && !character.m_seman.HaveStatusEffect(ViolentInvisibilityMod.PluginName) 
                && character.IsPlayer();
        }
    }
}
*/