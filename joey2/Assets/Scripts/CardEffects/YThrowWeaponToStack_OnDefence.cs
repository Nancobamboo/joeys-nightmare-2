// Scripts/CardEffects/Effects/YThrowWeaponToStack_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
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
		YActionSystem.Instance.DispatchAction(EActionId.ThrowWeaponToEnv, CardControl);
		return 0f;
	}
}

public partial class UIGamePhaseControl
{
	async void ThrowWeaponToEnv(UICardSimpleControl cardControl)
	{
		// check env card count number
		int envCardCount = 0;
		foreach (var kvp in m_EnvCardDict)
		{
			envCardCount += kvp.Value.Count;
		}
		Debug.Log("ThrowWeaponToEnv env card count before: " + envCardCount);
		Debug.Log("ThrowWeaponToEnv discard card count before: " + UsedCardList.Count);
		UICardSimpleControl weaponCard = GetLastBagCard(ECardType.attack);
		// TODO judge whether the weapon card is fist
		if (weaponCard != null)
		{
			// 等待所有伤害结算完成，确保动画在正确的时机播放
			await UniTask.Yield();
			await AddEnvCardFromBagAsync(weaponCard);
			// weaponCard.Return();
		}
		envCardCount = 0;
		foreach (var kvp in m_EnvCardDict)
		{
			envCardCount += kvp.Value.Count;
		}
		Debug.Log("ThrowWeaponToEnv env card count after: " + envCardCount);
		Debug.Log("ThrowWeaponToEnv discard card count after: " + UsedCardList.Count);
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