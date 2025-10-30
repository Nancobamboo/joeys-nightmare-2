using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public sealed class PData : PureSingleton<PData>
{
	public int playerHealth { get; set; } = 30;
	public int playerMaxHealth { get; set; } = 30;
	public int playerAttack { get; set; } = 0;
	public int playerDefence { get; set; } = 0;
	public bool canOperate { get; set; } = true;

	public void SetPlayerHP(int hp)
	{
		playerHealth = hp;
		GameEvents.RaiseHPChanged(playerHealth);
	}

	public void SetPlayerAttack(int attack)
	{
		playerAttack = attack;
		GameEvents.RaiseAttackChanged(playerAttack);
	}

	public void SetPlayerDefence(int defence)
	{
		playerDefence = defence;
		GameEvents.RaiseDefenceChanged(playerDefence);
	}
}