using System.Collections.Generic;
using System.IO;
using UnityEngine;


public sealed class PData : PureSingleton<PData>
{
	public int playerHealth { get; set; } = 100;
	public int playerMaxHealth { get; set; } = 100;
	public bool canOperate { get; set; } = true;

}