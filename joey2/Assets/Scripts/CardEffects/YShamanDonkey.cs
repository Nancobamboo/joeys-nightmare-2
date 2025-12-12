using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YShamanDonkey : YDefaultEffect
{
    // Donkey类怪物的card id列表
    private static readonly HashSet<string> DonkeyCardIds = new HashSet<string>
    {
        "5010", // 普通驴
        "5011", // 骷髅驴
        "5012", // 萨满驴(自己)
        "5013", // 笨驴
        "5014"  // 死灵驴
    };

    public const int ATTACK_BOOST = 3;

    public YShamanDonkey()
    {
        Id = ECardEffectId.ShamanDonkey;
    }

    public override float OnTakeDamage(EEffectType effectType = EEffectType.Damage, int damage = 0)
    {
        if (CardControl != null && CardControl.CardData != null)
        {
            // 受到攻击后，给环境牌中最外层的Donkey类怪物加攻击
            YActionSystem.Instance.DispatchAction(EActionId.ShamanDonkeyBuff, CardControl);
        }
        return base.OnTakeDamage(effectType, damage);
    }

    public static bool IsDonkeyCard(string cardId)
    {
        return DonkeyCardIds.Contains(cardId);
    }
}

public partial class UIGamePhaseControl
{
    void ShamanDonkeyBuff(object[] paraArray)
    {
        UICardSimpleControl sourceCardControl = paraArray.Length > 0 ? (UICardSimpleControl)paraArray[0] : null;
        OnShamanDonkeyBuff(sourceCardControl);
    }

    public void OnShamanDonkeyBuff(UICardSimpleControl sourceCardControl)
    {
        // 遍历所有环境牌列，找到最外层的Donkey类怪物
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            UICardSimpleControl lastCard = GetLastEnvCard(i);
            if (lastCard != null && lastCard.gameObject.activeSelf && 
                lastCard.CardType == ECardType.monster && 
                lastCard.CardData != null &&
                YShamanDonkey.IsDonkeyCard(lastCard.CardData.id))
            {
                // 跳过自己（触发效果的萨满驴）
                if (sourceCardControl != null && lastCard == sourceCardControl)
                {
                    continue;
                }

                // 给这个Donkey怪物加攻击
                Card cardData = lastCard.CardData;
                cardData.SetAttack(cardData.currentAttack + YShamanDonkey.ATTACK_BOOST);
                // 刷新卡牌显示
                lastCard.RefreshCard();
                Debug.Log($"ShamanDonkey: Buffed {cardData.cardName} attack by {YShamanDonkey.ATTACK_BOOST}, new attack: {cardData.currentAttack}");
            }
        }
    }
}
