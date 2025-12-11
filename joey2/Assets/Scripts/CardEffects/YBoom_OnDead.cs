// Scripts/CardEffects/Effects/YBoom_OnKill.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
		JoeyGameControl.Instance.QueueAction(EActionId.BoomEnvCard, m_EnvIndex, baseExtra, true, CardControl);
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
	public void BoomEnvCardAtPosition(int envIndex, int boomDamage, bool isExcludeSelf)
	{
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
			if (index < 0 || index >= m_EnvPanels.Count)
			{
				continue;
			}
			UICardSimpleControl lastCard = GetLastEnvCard(index);
			if (lastCard != null && lastCard.gameObject.activeSelf && lastCard.CardType == ECardType.monster)
			{
				DealDamageToEnvCard(lastCard, boomDamage, index, EEffectType.Boom, CancellationToken.None).Forget();
			}
		}
	}
}

