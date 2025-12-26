using UnityEngine;

public class YMimicChest : YDefaultEffect
{
    public int baseExtra;
    private Card m_RealCard;
    private Card m_DisguiseCard;
    private bool m_IsRevealed = false;

    public YMimicChest(int baseExtra)
    {
        Id = ECardEffectId.MimicChest;
        this.baseExtra = baseExtra;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);

        if (CardControl != null && CardControl.CardData != null)
        {
            m_RealCard = CardControl.CardData;

            Card disguiseCard = GData.Instance.GetCardConfigById(baseExtra.ToString());
            if (disguiseCard != null)
            {
                m_DisguiseCard = disguiseCard;
                CardControl.UpdateCardDisplay(m_DisguiseCard);
            }
        }
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        // Mimic reveals itself on the first time it takes damage.
        // Important: still call base.OnTakeDamage so the hit VFX/anim/sfx are played.
        if (CardControl != null && m_RealCard != null && !m_IsRevealed)
        {
            m_IsRevealed = true;
            CardControl.UpdateCardDisplay(m_RealCard);
        }

        return base.OnTakeDamage(effectType, damage);
    }
}

