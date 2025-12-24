using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGrowthControl : YViewControl
{
	private UIGrowthView m_View;
	// 注意：不能 new MonoBehaviour（UIBtnControl 继承自 YViewControl/MonoBehaviour）
	// 这里用纯 C# 的按钮封装类来管理节点逻辑
	private List<GrowthBtn> m_BtnControls = new List<GrowthBtn>();
	private List<GrowthNode> m_Nodes = new List<GrowthNode>();

	private class GrowthBtn
	{
		private Button m_Button;
		private Image m_Image;
		private GameObject m_Sold;
		private int m_Index;
		private System.Action<int> m_OnClick;

		public void InitWithTransform(Transform trs, int index, System.Action<int> onClick, Sprite defaultSprite)
		{
			m_Index = index;
			m_OnClick = onClick;

			if (trs == null) return;

			// 这些节点在 prefab 里可能只有 RectTransform，没有任何 Graphic/按钮组件，导致“看不见”
			m_Image = trs.GetComponent<Image>();
			if (m_Image == null)
			{
				m_Image = trs.gameObject.AddComponent<Image>();
				m_Image.raycastTarget = true;
				if (defaultSprite != null) m_Image.sprite = defaultSprite;
				// 给一个非透明的默认颜色，确保可见
				m_Image.color = new Color(1f, 1f, 1f, 0.85f);
			}

			m_Button = trs.GetComponent<Button>();
			if (m_Button == null)
			{
				m_Button = trs.gameObject.AddComponent<Button>();
			}

			// sold 标记：兼容两种命名
			var soldTr = trs.Find("Sold") ?? trs.Find("sold");
			if (soldTr != null) m_Sold = soldTr.gameObject;

			if (m_Button != null)
			{
				// 确保 disabled 状态仍可见（避免被设成透明）
				var colors = m_Button.colors;
				if (colors.disabledColor.a < 0.1f)
				{
					colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
					m_Button.colors = colors;
				}

				m_Button.onClick.RemoveAllListeners();
				m_Button.onClick.AddListener(() => m_OnClick?.Invoke(m_Index));
			}
		}

		public void SetData(bool isUnlocked, bool interactable)
		{
			if (isUnlocked)
			{
				if (m_Sold != null) m_Sold.SetActive(true);
				if (m_Button != null) m_Button.interactable = false;
			}
			else
			{
				if (m_Sold != null) m_Sold.SetActive(false);
				if (m_Button != null) m_Button.interactable = interactable;
			}
		}
	}

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
		m_BtnControls.Clear();
		Sprite defaultSprite = null;
		// 优先使用 UIBtn.prefab 自己的底图（而不是返回按钮的图）
		var uiBtnPrefab = Resources.Load<GameObject>("UIPrefab/UIBtn");
		if (uiBtnPrefab != null)
		{
			var img = uiBtnPrefab.GetComponent<Image>();
			if (img != null) defaultSprite = img.sprite;
		}
		// 兜底：如果没找到 UIBtn，就退回用返回按钮的图
		if (defaultSprite == null && m_View != null && m_View.BtnSkip != null)
		{
			var img = m_View.BtnSkip.GetComponent<Image>();
			if (img != null) defaultSprite = img.sprite;
		}
		var trs = new List<RectTransform> {
			m_View.Btn, m_View.Btn1, m_View.Btn2, m_View.Btn3, m_View.Btn4,
			m_View.Btn5, m_View.Btn6, m_View.Btn7, m_View.Btn8, m_View.Btn9,
			m_View.Btn10, m_View.Btn11, m_View.Btn12, m_View.Btn13, m_View.Btn14,
			m_View.Btn15, m_View.Btn16, m_View.Btn17, m_View.Btn18, m_View.Btn19,
			m_View.Btn20
		};
		
		int nonNull = 0;
		for (int i = 0; i < trs.Count; i++)
		{
			if (trs[i] != null) nonNull++;
		}
		Debug.Log($"[UIGrowthControl] InitButtons total slots={trs.Count}, nonNull={nonNull}");
		if (nonNull == 0)
		{
			Debug.LogError("[UIGrowthControl] No growth node transforms bound. Check UIGrowth prefab: Btn/Btn1..Btn20 names or YViewReference.ViewItemList.");
		}

		for (int i = 0; i < trs.Count; i++)
		{
			if (trs[i] == null) continue;

			trs[i].gameObject.SetActive(true);

			var btnControl = new GrowthBtn();
			btnControl.InitWithTransform(trs[i], i, OnGrowthBtnClick, defaultSprite);
			m_BtnControls.Add(btnControl);
		}
	}

	public void SetData()
	{
		Refresh();
	}

	private void Refresh()
	{
		DataGrowth data = DataSystem.Instance.GetDataGrowth();

		// 刷新局外积分显示
		if (m_View != null && m_View.TextCoins != null)
		{
			m_View.TextCoins.text = data.Points.ToString();
		}
		
		for (int i = 0; i < m_BtnControls.Count; i++)
		{
			GrowthBtn btnControl = m_BtnControls[i];
			// Find node data
			GrowthNode node = m_Nodes.Find(n => n.Id == i);
			
			// Safety check if node exists
			if (node.Name == null) {
				Debug.LogWarning($"Node {i} not found in CSV data!");
				btnControl.SetData(false, false);
				continue;
			}

			bool isUnlocked = data.IsUnlocked(node.Id);
			bool interactable = true;
			
			if (!isUnlocked)
			{
				// Check dependency
				if (node.Depend != -1)
				{
					if (!data.IsUnlocked(node.Depend))
					{
						interactable = false;
					}
				}
			}
			
			Debug.Log($"Setting Node {i}: Unlocked={isUnlocked}, Interactable={interactable}");
			btnControl.SetData(isUnlocked, interactable);
		}
	}

	void OnGrowthBtnClick(int index)
	{
		GrowthNode node = m_Nodes.Find(n => n.Id == index);
		if (node.Name == null) return;

		// Double check dependency (though UI should block it)
		DataGrowth data = DataSystem.Instance.GetDataGrowth();
		if (node.Depend != -1 && !data.IsUnlocked(node.Depend)) return;
		
		// 如果已经解锁，也不处理点击（虽然 SetData 会禁用按钮）
		if (data.IsUnlocked(node.Id)) return;

		var window = Asset.OpenUI<UIGrowthWindowControl>();
		window.SetData(node.Id, node.Desc, node.Price, () =>
		{
			DataSystem.Instance.GetDataGrowth().Unlock(node.Id);
			DataSystem.Instance.SaveDataGrowth();
			Refresh();
		});
	}
}