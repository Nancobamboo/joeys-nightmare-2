// Scripts/CardEffects/Effects/YThrowWeaponToStack_OnDefence.cs
using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class YThrowWeaponToStack_OnDefence : YCardEffect
{
	public YThrowWeaponToStack_OnDefence()
	{
		Id = ECardEffectId.ThrowWeaponToStack_OnDefence;
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
		YActionSystem.Instance.DispatchAction(EActionId.ThrowWeaponToEnv);
		return 0f;
	}
}

public partial class UIGamePhaseControl
{
	bool IsUseAttackFinishAnim()
	{
		UICardSimpleControl lastDefenceCard = GetLastBagCard(ECardType.defence);
		if (lastDefenceCard != null && lastDefenceCard.CardEffect != null)
		{
			return lastDefenceCard.CardEffect.Id == ECardEffectId.ThrowWeaponToStack_OnDefence;
		}
		return false;
	}

	async void ThrowWeaponToEnv()
	{
		UICardSimpleControl weaponCard = GetLastBagCard(ECardType.attack);
		if (weaponCard != null && weaponCard.CardData.id != "1016")
		{
			await AddEnvCardFromBagAsync(weaponCard);
		}
	}

	private async UniTask AddEnvCardFromBagAsync(UICardSimpleControl cardControl)
	{
		if (cardControl == null || cardControl.CardData == null)
		{
			return;
		}
		if (m_EnvPanels == null || m_EnvPanels.Count == 0)
		{
			return;
		}
		int randomIndex = Random.Range(0, m_EnvPanels.Count);
		VerticalLayoutGroup parent = m_EnvPanels[randomIndex];
		Transform effectRoot = GetEffectRoot(randomIndex);
		if (effectRoot == null)
		{
			return;
		}
		Card newCard = cardControl.CardData;
		m_CardDict[newCard.UniqueId] = newCard;
		UICardSimpleControl newCardControl = GetCardSimple(Asset.UIRoot, true);
		newCardControl.SetData(newCard, isEnv: true, envIndex: randomIndex);
		newCardControl.SetMoving(true);
		AddEnvCard(randomIndex, newCardControl);

		Vector3 startWorldPos = cardControl.CacheTrans.position;
		Vector3 startScale = cardControl.CacheTrans.localScale;
		Vector3 endWorldPos = effectRoot.position;
		Vector3 endScale = Vector3.one;
		float duration = 0.2f;

		// 使用YMoveWeaponToEnv中已定义的MoveCardToEnvAnimation方法
		await MoveCardToEnvAnimation(newCardControl, startWorldPos, endWorldPos, startScale, endScale, duration, parent);

		newCardControl.CacheTrans.SetParent(parent.transform);
		newCardControl.CacheTrans.localPosition = Vector3.zero;
		newCardControl.CacheTrans.localScale = Vector3.one;
		newCardControl.CacheTrans.localEulerAngles = Vector3.zero;
		parent.enabled = true;
		newCardControl.SetMoving(false);
	}
}