using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YJokerNightmare : YDefaultEffect
{
    public YJokerNightmare()
    {
        Id = ECardEffectId.JokerNightmare;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.Counter, 3);
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.Counter)
        {
            int envIndex = CardControl.EnvIndex;
            if (JoeyGameControl.Instance.IsCardOnTop(CardControl, envIndex))
            {
                value--;
                if (value == 0)
                {
                    YActionSystem.Instance.DispatchAction(EActionId.JokerNightmareCurse, CardControl);
                    value = 3;
                }
            }
        }
        return value;
    }
}

public partial class UIGamePhaseControl
{
    void JokerNightmareCurse(object[] paraArray)
    {
        UICardSimpleControl cardControl = (UICardSimpleControl)paraArray[0];
        JokerNightmareCurseAsync(cardControl).Forget();
    }

    async UniTask JokerNightmareCurseAsync(UICardSimpleControl cardControl)
    {
        if (cardControl == null || !cardControl.gameObject.activeSelf || cardControl.CardData.currentHealth <= 0)
        {
            return;
        }

        UICardSimpleControl weaponCard = GetLastBagCard(ECardType.attack);
        UICardSimpleControl defenceCard = GetLastBagCard(ECardType.defence);

        bool canCurseWeapon = weaponCard != null && weaponCard != m_FistCardCache;
        bool canCurseDefence = defenceCard != null;

        if (!canCurseWeapon && !canCurseDefence)
        {
            return;
        }

        if (cardControl.CacheTrans != null)
        {
            JoeyGameControl.Instance.PlayVFX(EVFXName.VFX_Shihun, cardControl.CacheTrans, 1f);
        }
        await UniTask.WaitForSeconds(0.5f);

        List<UICardSimpleControl> weaponList = GetBagCardList(ECardType.attack);
        List<UICardSimpleControl> defenceList = GetBagCardList(ECardType.defence);
        int weaponCount = 0;
        int defenceCount = 0;

        if (weaponList != null)
        {
            for (int i = 0; i < weaponList.Count; i++)
            {
                if (weaponList[i] != m_FistCardCache)
                {
                    weaponCount++;
                }
            }
        }
        if (defenceList != null)
        {
            defenceCount = defenceList.Count;
        }

        bool curseWeapon = false;
        if (canCurseWeapon && canCurseDefence)
        {
            if (weaponCount > defenceCount)
            {
                curseWeapon = true;
            }
            else if (weaponCount < defenceCount)
            {
                curseWeapon = false;
            }
            else
            {
                curseWeapon = Random.Range(0, 2) == 0;
            }
        }
        else if (canCurseWeapon)
        {
            curseWeapon = true;
        }
        else
        {
            curseWeapon = false;
        }

        if (curseWeapon)
        {
            CurseCardWithTemplate(weaponCard, "1025");
        }
        else
        {
            CurseCardWithTemplate(defenceCard, "2016");
        }

        await UniTask.WaitForSeconds(0.3f);
    }

    void CurseCardWithTemplate(UICardSimpleControl cardControl, string templateCardId)
    {
        Card template = GData.Instance.GetCardConfigById(templateCardId);
        Card cardData = cardControl.CardData;
        cardData.cardImage = template.cardImage;
        cardData.cardBackground = template.cardBackground;
        cardData.cardName = template.cardName;
        cardData.description = template.description;
        cardData.id = template.id;
        cardData.SetAttack(template.currentAttack);
        cardData.SetDefence(template.currentDefence);
        cardData.effectId = template.effectId;
        cardControl.SetData(cardData);
    }

    public bool IsJokerNightmareAlive()
    {
        UICardSimpleControl jokerCard = FindEnvCardByEffectId(ECardEffectId.JokerNightmare);
        return jokerCard != null && jokerCard.gameObject.activeSelf && jokerCard.CardData.currentHealth > 0;
    }

    public async UniTask TriggerJokerNightmareAttackedEffects(UICardSimpleControl attackedCard)
    {
        UICardSimpleControl turkeyCard = FindEnvCardByEffectId(ECardEffectId.TurkeyJack);
        if (turkeyCard != null && turkeyCard.gameObject.activeSelf && turkeyCard.CardData.currentHealth > 0)
        {
            await TurkeyJackExtraCounterAsync();
        }

        UICardSimpleControl queenCard = FindEnvCardByEffectId(ECardEffectId.DonkeyQueen);
        if (queenCard != null && queenCard.gameObject.activeSelf && queenCard.CardData.currentHealth > 0)
        {
            await UniTask.WaitForSeconds(0.5f);
            await DonkeyQueenHealTargetAsync(attackedCard);
        }

        UICardSimpleControl kingCard = FindEnvCardByEffectId(ECardEffectId.MonkeyKing);
        if (kingCard != null && kingCard.gameObject.activeSelf && kingCard.CardData.currentHealth > 0)
        {
            await MonkeyKingRemoveDefenceAsync();
        }
    }
}

