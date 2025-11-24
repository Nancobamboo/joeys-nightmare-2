using System.Collections.Generic;

public class RoguelikeCharacter
{
	public string character;
	public int maxHealth;
	public List<string> equipmentAttack = new List<string>();
	public List<string> equipmentDefence = new List<string>();
	public List<string> equipmentItem = new List<string>();
	public List<string> equipmentSkill = new List<string>();
	public List<string> equipmentRelic = new List<string>();
	public int coins;
	public List<string> cardDeck = new List<string>();

	public RoguelikeCharacter()
	{
	}
}

