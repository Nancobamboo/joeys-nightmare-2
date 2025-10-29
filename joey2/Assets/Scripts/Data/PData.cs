using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public sealed class PData : PureSingleton<PData>
{
	public int playerHealth { get; set; } = 30;
	public int playerMaxHealth { get; set; } = 30;
	public bool canOperate { get; set; } = true;

	public void SetPlayerHP(int hp)
	{
		if (playerHealth == hp)
		{
			return;
		}
		playerHealth = hp;
		GameEvents.RaiseHPChanged(playerHealth);
	}
}