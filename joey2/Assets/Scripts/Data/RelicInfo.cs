public class RelicInfo
{
	public int id;
	public string cardImage;
	public string iconImage;
	public string tombstoneImage;
	public string name;
	public string description;
	public int stars;


	public RelicInfo(int id, string cardImage, string iconImage, string name, string description, int stars)
	{
		this.id = id;
		this.cardImage = cardImage;
		this.iconImage = iconImage;


		this.name = name;
		this.description = description;
		this.stars = stars;
		
		if (stars == 1)
		{
			this.tombstoneImage = "Art/Img/Relic/select_tombstone1";
		}
		else if (stars == 2)
		{
			this.tombstoneImage = "Art/Img/Relic/select_tombstone2";
		}
		else if (stars == 3)
		{
			this.tombstoneImage = "Art/Img/Relic/select_tombstone3";
		}
	}
}

