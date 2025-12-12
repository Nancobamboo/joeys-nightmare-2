// Scripts/CardEffects/Effects/YBoom_OnPlay.cs
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YBoom_OnPlay : YCardEffect
{
	public int baseExtra;

	public YBoom_OnPlay(int baseExtra)
	{
		this.baseExtra = Mathf.Max(0, baseExtra);
		Id = ECardEffectId.Boom_OnPlay;
	}



	public override float OnRemoveCard()
	{
		if (CardControl != null && JoeyGameControl.Instance.HasEnemy())
		{
			JoeyGameControl.Instance.AddGlobalDelayCall(() =>
			{
				JoeyGameControl.Instance.QueueAction(EActionId.BoomEnvCard, -1, baseExtra, false);
			}, 0.1f);
		}
		return 0f;
	}
}

public partial class UIGamePhaseControl
{
	public async void BoomEnvCardRandom(int boomDamage)
	{
		int envIndex = FindRandomEnemy();
		if (envIndex == -1)
		{
			return;
		}
		BoomEnvCardAtPosition(envIndex, boomDamage, false);
	}
}

