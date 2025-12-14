using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YDonkeyMajor : YDefaultEffect
{
    public int baseExtra;

    public YDonkeyMajor(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
        Id = ECardEffectId.DonkeyMajor;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        Debug.Log("YDonkeyMajor OnTakeDamage: " + baseExtra);
        // 每次受到攻击时，增加 baseExtra 点攻击力
        if (CardControl != null && CardControl.CardData != null)
        {
            Debug.Log("YDonkeyMajor OnTakeDamage Before: " + CardControl.CardData.currentAttack);
            CardControl.CardData.SetAttack(CardControl.CardData.currentAttack + baseExtra);
            Debug.Log("YDonkeyMajor OnTakeDamage After: " + CardControl.CardData.currentAttack);
            CardControl.RefreshCard();
            
            // 播放增益特效
            if (CardControl.CacheTrans != null)
            {
                JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shihun, CardControl.CacheTrans, 1f);
            }
        }
        
        // 调用基类方法播放受击动画和音效
        return base.OnTakeDamage(effectType, damage);
    }
}

