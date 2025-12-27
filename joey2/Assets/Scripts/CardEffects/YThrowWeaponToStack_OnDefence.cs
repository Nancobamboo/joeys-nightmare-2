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
			// 先从背包移除原卡，避免异步操作期间重复获取同一张卡
			int cardTypeInt = (int)ECardType.attack;
			if (m_BagCardDict.TryGetValue(cardTypeInt, out List<UICardSimpleControl> cardList))
			{
				cardList.Remove(weaponCard);
			}
			
			// 然后执行添加到Env的异步动画
			await AddEnvCardFromBagAsync(weaponCard);
			
			// 动画完成后归还原卡的CardControl
			weaponCard.Return();
			
			// 注意：这里不触发新栈顶卡的OnBecomeTopOfPile
			// 因为ThrowWeaponToEnv是从SingleDelayAction的回调中执行的，
			// 如果在此触发OnBecomeTopOfPile，会导致AddGlobalDelayCall立即执行缓存的action，
			// 造成连锁反应把多张卡都扔到Env
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