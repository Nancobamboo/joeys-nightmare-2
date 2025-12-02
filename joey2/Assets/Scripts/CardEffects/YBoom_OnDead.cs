// Scripts/CardEffects/Effects/YBoom_OnKill.cs
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class YBoom_OnDead : YCardEffect
{
	public int baseExtra;

	int m_EnvIndex = -1;

	public YBoom_OnDead(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.Boom_OnDead;
	}

	public override float OnBeDying()
	{
		if (CardControl != null && CardControl.gameObject != null)
		{
			m_EnvIndex = CardControl.EnvIndex;
			var vfxNames = new List<EVFXName> { };
			float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_feitian, EVFXLife.SelfLife);
			return maxDelayTime > 0f ? maxDelayTime : base.OnBeDying();
		}
		return base.OnBeDying();
	}

	public override float OnDead()
	{
		YActionSystem.Instance.DispatchAction(EActionId.BoomEnvCard, m_EnvIndex, baseExtra, true);
		return 0f;
	}



	public override int GetEffectValue(EEffectType effectType)
	{
		if (effectType == EEffectType.Damage)
		{
			return baseExtra;
		}
		return base.GetEffectValue(effectType);
	}
}

public partial class UIGamePhaseControl
{
	public async UniTask BoomEnvCardAtPosition(int envIndex, int boomDamage, bool isExcludeSelf)
	{
		Debug.Log("BoomEnvCard: envIndex = " + envIndex);
		foreach (KeyValuePair<int, List<UICardSimpleControl>> kvp in m_EnvCardDict)
		{
			Debug.Log("BoomEnvCard: envIndex = " + kvp.Key + " " + kvp.Value.Count);
		}

		int[] indices;
		if (isExcludeSelf)
		{
			indices = new int[] { envIndex - 1, envIndex + 1 };
		}
		else
		{
			indices = new int[] { envIndex - 1, envIndex, envIndex + 1 };
		}

		for (int i = 0; i < indices.Length; i++)
		{
			int index = indices[i];
			if (m_EnvCardDict.TryGetValue(index, out List<UICardSimpleControl> cardList))
			{
				if (cardList != null && cardList.Count > 0)
				{
					UICardSimpleControl lastCard = cardList[cardList.Count - 1];
					CancellationToken token = GetOrCreateCardToken(lastCard);
					await DealDamageToEnvCard(lastCard, boomDamage, index, EEffectType.Boom, token);
					RemoveCardCts(lastCard);
				}
			}
		}
	}
}

