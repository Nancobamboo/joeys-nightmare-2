// Scripts/CardEffects/Effects/YHookEquipWeaponFromDiscard_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YHookEquipWeaponFromDiscard_OnDefence : YDefaultEffect
{
	public YHookEquipWeaponFromDiscard_OnDefence()
	{
		Id = ECardEffectId.HookEquipWeaponFromDiscard_OnDefence;
	}

	//public override float UseDefence(bool isOverflow = false)
	//{
	//	if (CardControl != null && CardControl.gameObject != null)
	//	{
	//		var vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
	//		float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
	//		SFX.PlayAudio("Audio/SFX/Battle/Defence", 1.0f, 0f);
	//		return maxDelayTime > 0f ? maxDelayTime : base.UseDefence(isOverflow);
	//	}
	//	return base.UseDefence(isOverflow);
	//}

	public override float OnRemoveCard()
	{
		YActionSystem.Instance.DispatchAction(EActionId.AddCardFromDiscard, ECardType.attack);
		return 0f;
	}
}

public partial class UIGamePhaseControl
{
	public void AddCardFromDiscardByType(ECardType specialCardType)
	{
		List<Card> availableCards = new List<Card>();
		for (int i = UsedCardList.Count - 1; i >= 0; i--)
		{
			Card card = UsedCardList[i];
			ECardType cardType = (ECardType)System.Enum.Parse(typeof(ECardType), card.type);
			if (specialCardType != ECardType.other)
			{
				if (cardType == specialCardType)
				{
					availableCards.Add(card);
				}
			}
			else
			{
				if (cardType != ECardType.monster)
				{
					availableCards.Add(card);
				}
			}
		}

		if (availableCards.Count == 0)
		{
			return;
		}

		Card selectedCard = availableCards[UnityEngine.Random.Range(0, availableCards.Count)];
		UsedCardList.Remove(selectedCard);
		m_CardDict[selectedCard.UniqueId] = selectedCard;

		ECardType selectedCardType = (ECardType)System.Enum.Parse(typeof(ECardType), selectedCard.type);
		Transform parent = null;
		switch (selectedCardType)
		{
			case ECardType.attack:
				parent = m_View.AttackPanel.transform;
				break;
			case ECardType.defence:
				parent = m_View.DefencePanel.transform;
				break;
			case ECardType.skill:
				parent = m_View.SkillPanel.transform;
				break;
			case ECardType.item:
				parent = m_View.ItemPanel.transform;
				break;
			default:
				return;
		}

		UICardSimpleControl cardControl = GetCardSimple(parent, false);
		cardControl.SetData(selectedCard);
		AddBagCard(selectedCardType, cardControl);
		cardControl.PlayVFX(new List<EVFXName>(), ECardAnimName.UI_Carditem_pailai, EVFXLife.CardLife);
	}
}

