using System.Collections.Generic;

public class GrowthInfo
{
	public int id;
	public string name;
	// 兼容字段：单依赖时使用（取 depends 的第一个；无依赖时为 -1）
	public int depend;
	// 新字段：支持多依赖（growth.csv 的 dependency 列支持用 ';' 或 '|' 分隔）
	public List<int> depends = new List<int>();
	public string desc;
	public int price;

	public GrowthInfo(int id, string name, List<int> depends, string desc, int price)
	{
		this.id = id;
		this.name = name;
		this.depends = depends ?? new List<int>();
		this.depend = this.depends.Count > 0 ? this.depends[0] : -1;
		this.desc = desc;
		this.price = price;
	}
}

