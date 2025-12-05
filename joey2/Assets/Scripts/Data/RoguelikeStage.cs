using System.Collections.Generic;

public enum EStageType
{
	normal,
	elite,
	boss,
	final
}

public class RoguelikeStage
{
	public string stages;
	public List<string> level = new List<string>();
	public EStageType type;

	public RoguelikeStage()
	{
	}
}

