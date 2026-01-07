// Scripts/CardEffects/Effects/YThrowWeaponToStack_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public class YAddKnifeToEnv_OnDefense : YCardEffect
{
	public YAddKnifeToEnv_OnDefense()
	{
		Id = ECardEffectId.AddKnifeToEnv_OnDefense;
	}

	public override float UseDefence(bool isOverflow = false)
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			var vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
			SFX.PlayAudio("Audio/SFX/Battle/Defence", 1.0f, 0f);
			return maxDelayTime > 0f ? maxDelayTime : base.UseDefence(isOverflow);
		}
		return base.UseDefence(isOverflow);
	}

    public override float OnRemoveCard()
	{
		YActionSystem.Instance.DispatchAction(EActionId.AddCardToEnv, CardControl, "1018");
		YActionSystem.Instance.DispatchAction(EActionId.AddCardToEnv, CardControl, "1018");
		return 0f;
	}
}

public partial class UIGamePhaseControl
{
	public void AddCardToEnv(UICardSimpleControl cardControl, string cardId, int envIndex = -1)
	{
        if (cardControl == null || cardControl.CardData == null || string.IsNullOrEmpty(cardId))
        {
            return;
        }		
        if (m_EnvPanels == null || m_EnvPanels.Count == 0)
        {
            return;
        }
        // TODO fix card id to knife card id
        Card knifeCard = CreateCard(cardId);
        
        // Apply difficulty effects to monsters in Env mode
        if (JoeyGameControl.Instance != null && JoeyGameControl.Instance.GameMode == EGameMode.Env && knifeCard.GetCardType() == ECardType.monster)
        {
            ApplyEnvDifficultyToMonster(knifeCard);
        }
        
        int randomIndex = envIndex == -1 ? Random.Range(0, m_EnvPanels.Count) : envIndex;
        VerticalLayoutGroup parent = m_EnvPanels[randomIndex];
        m_CardDict[knifeCard.UniqueId] = knifeCard;
        UICardSimpleControl newCardControl = GetCardSimple(parent.transform, true);
        newCardControl.SetData(knifeCard, isEnv: true, envIndex: randomIndex);
        AddEnvCard(randomIndex, newCardControl);
        newCardControl.PlayVFX(new List<EVFXName>(), ECardAnimName.UI_Carditem_pailai, EVFXLife.CardLife);

		// Update monster buffs after new card is added (for BadMonkey/MonkeyKing attack updates)
		for (int i = 0; i < m_EnvPanels.Count; i++)
		{
			UICardSimpleControl lastCard = GetLastEnvCard(i);
			if (lastCard != null && lastCard.CardType == ECardType.monster)
			{
				// Only update cards with UpdateAttack buff to avoid affecting Counter-based effects
				if (lastCard.GetBuffValue(EBuffType.UpdateAttack) > 0)
				{
					lastCard.UpdateBuffValue();
				}
			}
		}
	}
}