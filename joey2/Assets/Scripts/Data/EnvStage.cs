using System.Collections.Generic;

public enum ETheme
{
	monkey,
	turkey,
	donkey,
	deadkey,
	tutorial
}

public class EnvStage
{
	public int level;
	public List<string> monsterIds = new List<string>();
	public EStageType type;
	public ETheme theme;

	public EnvStage()
	{
	}
}

