// Scripts/CardEffects/Effects/YThrowWeaponToStack_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YThrowWeaponDefenceToStack_UseSkill : YCardEffect
{
    public YThrowWeaponDefenceToStack_UseSkill()
    {
        Id = ECardEffectId.ThrowWeaponDefenceToStack_UseSkill;
    }

	public override float UseSkill()
	{

        if (CardControl != null && CardControl.gameObject != null)
        {
            var vfxNames = new List<EVFXName> { };
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
            return 0.3f;
        }
		return base.UseSkill();
	}


    public override float OnRemoveCard()
    {
        YActionSystem.Instance.DispatchAction(EActionId.ThrowWeaponDefenceToEnv, CardControl);
        return 0f;
    }
}

public partial class UIGamePhaseControl
{
    public void ThrowWeaponDefenceToEnv(UICardSimpleControl cardControl)
    {

        UICardSimpleControl weaponCard = GetLastBagCard(ECardType.attack);
        // TODO judge whether the weapon card is fist
        if (weaponCard != null)
        {
            AddEnvCardFromBag(weaponCard);
            RemoveBagCardInstant(ECardType.attack, weaponCard);
        }

        UICardSimpleControl defenceCard = GetLastBagCard(ECardType.defence);
        // TODO judge whether the weapon card is fist
        if (defenceCard != null)
        {
            AddEnvCardFromBag(defenceCard);
            RemoveBagCardInstant(ECardType.defence, defenceCard);
        }
    }

    private void RemoveBagCardInstant(ECardType cardType, UICardSimpleControl cardControl)
    {
        if (cardControl == null)
        {
            return;
        }

        int cardTypeInt = (int)cardType;
        if (m_BagCardDict.TryGetValue(cardTypeInt, out List<UICardSimpleControl> cardList))
        {
            cardList.Remove(cardControl);
        }

        RemoveCardData(cardControl.CardData.UniqueId);
        cardControl.Return();

        UICardSimpleControl newLastBagCard = GetLastBagCard(cardType);
        if (newLastBagCard != null)
        {
            newLastBagCard.CardEffect?.OnBecomeTopOfPile();
        }
        // If no weapon card left and we removed an attack card, trigger bare hands OnBecomeTopOfPile
        else if (cardType == ECardType.attack && m_FistCardCache != null)
        {
            m_FistCardCache.CardEffect?.OnBecomeTopOfPile();
        }
    }


}