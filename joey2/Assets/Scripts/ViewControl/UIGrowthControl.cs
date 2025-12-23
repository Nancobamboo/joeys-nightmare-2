using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGrowthControl : YViewControl
{
	private UIGrowthView m_View;
	private List<UIBtnView> m_BtnViews = new List<UIBtnView>();
	private List<GrowthNode> m_Nodes = new List<GrowthNode>();

	public struct GrowthNode
	{
		public int Id;
		public string Name;
		public int Depend;
		public string Desc;
		public int Price;
	}

	public static EResType GetResType()
	{
		return EResType.UIGrowth;
	}

	protected override void OnInit()
	{
		base.OnInit();
		m_View = CreateView<UIGrowthView>();
		m_View.BtnSkip.onClick.AddListener(Close);

		LoadCsv();
		InitButtons();
	}

	private void LoadCsv()
	{
		m_Nodes.Clear();
		TextAsset textAsset = Resources.Load<TextAsset>("Data/growth");
		if (textAsset == null)
		{
			Debug.LogError("Failed to load Data/growth csv!");
			return;
		}

		string[] lines = textAsset.text.Split('\n');
		Debug.Log($"Loaded growth csv, lines: {lines.Length}");
		// Skip header (first line)
		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;
			string[] cols = line.Split(',');
			if (cols.Length < 5) continue;

			// Format: id,name,dependency,desc,price
			if (int.TryParse(cols[0], out int id))
			{
				GrowthNode node = new GrowthNode();
				node.Id = id;
				node.Name = cols[1];
				int.TryParse(cols[2], out node.Depend);
				node.Desc = cols[3];
				int.TryParse(cols[4], out node.Price);
				m_Nodes.Add(node);
			}
		}
		Debug.Log($"Parsed {m_Nodes.Count} growth nodes.");
	}

	private void InitButtons()
	{
		var trs = new List<RectTransform> {
			m_View.Btn, m_View.Btn1, m_View.Btn2, m_View.Btn3, m_View.Btn4,
			m_View.Btn5, m_View.Btn6, m_View.Btn7, m_View.Btn8, m_View.Btn9,
			m_View.Btn10, m_View.Btn11, m_View.Btn12, m_View.Btn13, m_View.Btn14,
			m_View.Btn15, m_View.Btn16, m_View.Btn17, m_View.Btn18, m_View.Btn19,
			m_View.Btn20
		};
		
		Debug.Log($"InitButtons found {trs.Count} transforms.");

		for (int i = 0; i < trs.Count; i++)
		{
			if (trs[i] == null) continue;

			var btnView = new UIBtnView();
			btnView.OnInit(trs[i]);
			
			// Fallback: manually get components if YViewReference failed
			if (btnView.UIBtn == null)
			{
				btnView.UIBtn = trs[i].GetComponent<Button>();
			}
			if (btnView.Sold == null)
			{
				Transform soldTr = trs[i].Find("Sold");
				if (soldTr != null) btnView.Sold = soldTr.gameObject;
			}

			m_BtnViews.Add(btnView);

			if (btnView.UIBtn == null)
			{
				Debug.LogWarning($"Button {i} UIBtn is null!");
				continue;
			}

			int index = i; // capture variable
			btnView.UIBtn.onClick.AddListener(() => OnGrowthBtnClick(index));
		}
	}

	public void SetData()
	{
		Refresh();
	}

	private void Refresh()
	{
		DataGrowth data = DataSystem.Instance.GetDataGrowth();
		
		// Map nodes by ID for easier lookup if needed, but here index matches ID mostly
		// Assuming m_Nodes matches m_BtnViews indices.
		// If nodes are not sorted 0..20, we should find by ID.
		
		for (int i = 0; i < m_BtnViews.Count; i++)
		{
			UIBtnView btnView = m_BtnViews[i];
			if (btnView == null || btnView.UIBtn == null) continue;

			// Find node data
			GrowthNode node = m_Nodes.Find(n => n.Id == i);
			// Safety check if node exists
			if (node.Name == null) {
				btnView.UIBtn.interactable = false;
				continue;
			}

			bool isUnlocked = data.IsUnlocked(node.Id);
			if (isUnlocked)
			{
				if (btnView.Sold != null) btnView.Sold.SetActive(true);
				btnView.UIBtn.interactable = false;
			}
			else
			{
				if (btnView.Sold != null) btnView.Sold.SetActive(false);
				
				// Check dependency
				bool interactable = true;
				if (node.Depend != -1)
				{
					if (!data.IsUnlocked(node.Depend))
					{
						interactable = false;
					}
				}
				
				btnView.UIBtn.interactable = interactable;
			}
		}
	}

	void OnGrowthBtnClick(int index)
	{
		GrowthNode node = m_Nodes.Find(n => n.Id == index);
		if (node.Name == null) return;

		// Double check dependency (though UI should block it)
		DataGrowth data = DataSystem.Instance.GetDataGrowth();
		if (node.Depend != -1 && !data.IsUnlocked(node.Depend)) return;

		var window = Asset.OpenUI<UIGrowthWindowControl>();
		window.SetData(node.Id, node.Desc, node.Price, () =>
		{
			DataSystem.Instance.GetDataGrowth().Unlock(node.Id);
			DataSystem.Instance.SaveDataGrowth();
			Refresh();
		});
	}
}