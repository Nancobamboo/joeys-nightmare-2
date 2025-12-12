// Scripts/CardEffects/Effects/YDoubleAttack_OnPlay.cs
using System.Collections;
using UnityEngine;

public class YDoubleAttack_OnPlay : YCardEffect
{
	int m_ExtraAttackCnt = 1;
	public YDoubleAttack_OnPlay()
	{
		Id = ECardEffectId.DoubleAttack_OnPlay;

	}

	public override float OnRemoveCard()
	{
		YActionSystem.Instance.DispatchAction(EActionId.AddEffectValueToBagCard, ECardType.attack, EEffectType.ExtraAttackCnt, m_ExtraAttackCnt);
		return 0f;
	}
}

public partial class UIGamePhaseControl
{
	public void AddEffectValueToBagCard(ECardType targetCardType, EEffectType effectType, int value)
	{
		UICardSimpleControl lastBagCard = GetLastBagCard(targetCardType);

		if (lastBagCard == null && targetCardType == ECardType.attack)
		{
			lastBagCard = GetFistCard();

		}


		if (lastBagCard != null)
		{
			lastBagCard.AddEffectValue(effectType, value);
		}

	}
}

