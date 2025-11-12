// Scripts/CardEffects/Effects/YDoubleAttack_OnPlay.cs
using System.Collections;
using UnityEngine;

public class YDoubleAttack_OnPlay : YCardEffect
{
	public YDoubleAttack_OnPlay()
	{
		Id = ECardEffectId.DoubleAttack_OnPlay;
	}

	public override float UseSkill()
	{
		return base.UseSkill();
	}
}

