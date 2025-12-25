using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGrowthControl : YViewControl
{
	private UIGrowthView m_View;

	private readonly List<GrowthNode> m_Nodes = new List<GrowthNode>();
	private readonly Dictionary<int, GrowthNode> m_NodeById = new Dictionary<int, GrowthNode>();
	private readonly Dictionary<int, RectTransform> m_SlotById = new Dictionary<int, RectTransform>();
	private readonly Dictionary<int, UIBtnControl> m_BtnById = new Dictionary<int, UIBtnControl>();

	// 状态缓存，用于连线样式
	private readonly Dictionary<int, bool> m_IsSoldById = new Dictionary<int, bool>();
	private readonly Dictionary<int, bool> m_IsActiveById = new Dictionary<int, bool>();

	private RectTransform m_LinesRoot;
	private readonly List<GrowthLine> m_Lines = new List<GrowthLine>();
	private static Sprite s_WhiteSprite;

	private class GrowthLine
	{
		public int A;
		public int B;
		public RectTransform Rt;
		public Image Img;
	}

	public class GrowthNode
	{
		public int Id;
		public string Name;
		public List<int> Depends = new List<int>();
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
		if (m_View != null && m_View.BtnSkip != null) m_View.BtnSkip.onClick.AddListener(Close);

		LoadCsv();
		InitSlotsAndButtons();
		BuildLines();
		Refresh();
	}

	public void SetData()
	{
		Refresh();
	}

	private List<RectTransform> GetSlots()
	{
		return new List<RectTransform> {
			m_View.Btn, m_View.Btn1, m_View.Btn2, m_View.Btn3, m_View.Btn4,
			m_View.Btn5, m_View.Btn6, m_View.Btn7, m_View.Btn8, m_View.Btn9,
			m_View.Btn10, m_View.Btn11, m_View.Btn12, m_View.Btn13, m_View.Btn14,
			m_View.Btn15, m_View.Btn16, m_View.Btn17, m_View.Btn18, m_View.Btn19,
			m_View.Btn20
		};
	}

	private void LoadCsv()
	{
		m_Nodes.Clear();
		m_NodeById.Clear();

		TextAsset textAsset = Resources.Load<TextAsset>("Data/growth");
		if (textAsset == null)
		{
			Debug.LogError("[UIGrowthControl] Failed to load Resources/Data/growth.csv (Resources.Load TextAsset \"Data/growth\" returned null).");
			return;
		}

		string[] lines = textAsset.text.Split('\n');
		// Skip header
		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;
			line = line.Trim();
			if (string.IsNullOrEmpty(line)) continue;

			string[] cols = line.Split(',');
			if (cols.Length < 5) continue;

			if (!int.TryParse(cols[0].Trim(), out int id)) continue;

			var node = new GrowthNode();
			node.Id = id;
			node.Name = cols[1].Trim();

			// dependency 允许单个数字或用 | / ; 分隔的多个依赖
			string depStr = cols[2].Trim();
			node.Depends.Clear();
			if (!string.IsNullOrEmpty(depStr))
			{
				var parts = depStr.Split('|', ';');
				for (int p = 0; p < parts.Length; p++)
				{
					if (int.TryParse(parts[p].Trim(), out int depId))
					{
						if (depId >= 0) node.Depends.Add(depId);
					}
				}
			}

			node.Desc = cols[3].Trim();
			int.TryParse(cols[4].Trim(), out node.Price);

			m_Nodes.Add(node);
			m_NodeById[node.Id] = node;
		}
	}

	private void InitSlotsAndButtons()
	{
		m_SlotById.Clear();
		m_BtnById.Clear();

		var slots = GetSlots();
		int nonNull = 0;
		for (int i = 0; i < slots.Count; i++)
		{
			if (slots[i] != null) nonNull++;
		}
		if (nonNull == 0)
		{
			Debug.LogError("[UIGrowthControl] No slot RectTransform bound. Check UIGrowth prefab: Btn/Btn1..Btn20 names or YViewReference.ViewItemList.");
			return;
		}

		for (int id = 0; id < slots.Count; id++)
		{
			var slot = slots[id];
			if (slot == null) continue;

			slot.gameObject.SetActive(true);

			// 确保槽位下只有一个 UIBtn（每次打开 UIGrowth 都是新实例，清理是安全的）
			for (int c = slot.childCount - 1; c >= 0; c--)
			{
				Destroy(slot.GetChild(c).gameObject);
			}

			// 关键需求：把 UIBtn 创建到 21 个 RectTransform 槽位下面
			var btnCtrl = Asset.OpenUI<UIBtnControl>(slot);
			btnCtrl.Setup(id, OnGrowthBtnClick);

			// 让 UIBtn 填满槽位（忽略 prefab 自带的 anchoredPosition）
			var rt = btnCtrl.transform as RectTransform;
			if (rt != null)
			{
				rt.anchorMin = Vector2.zero;
				rt.anchorMax = Vector2.one;
				rt.pivot = new Vector2(0.5f, 0.5f);
				rt.anchoredPosition = Vector2.zero;
				rt.sizeDelta = Vector2.zero;
				rt.localScale = Vector3.one;
				rt.localRotation = Quaternion.identity;
			}
			else
			{
				btnCtrl.transform.localPosition = Vector3.zero;
				btnCtrl.transform.localScale = Vector3.one;
				btnCtrl.transform.localRotation = Quaternion.identity;
			}

			// 关键需求：UIBtn 的 text 是 growth.csv 的 name
			if (m_NodeById.TryGetValue(id, out var node))
			{
				btnCtrl.SetTitle(node.Name);
			}
			else
			{
				btnCtrl.SetTitle(string.Empty);
			}

			m_SlotById[id] = slot;
			m_BtnById[id] = btnCtrl;
		}
	}

	private void EnsureLinesRoot()
	{
		if (m_LinesRoot != null) return;

		// 放在按钮槽位同级的父节点下，并置底，确保在按钮背后
		var parent = (m_View != null && m_View.Btn != null) ? m_View.Btn.parent : transform;
		var go = new GameObject("Lines", typeof(RectTransform));
		go.transform.SetParent(parent, false);
		m_LinesRoot = go.GetComponent<RectTransform>();
		m_LinesRoot.anchorMin = Vector2.zero;
		m_LinesRoot.anchorMax = Vector2.one;
		m_LinesRoot.pivot = new Vector2(0.5f, 0.5f);
		m_LinesRoot.anchoredPosition = Vector2.zero;
		m_LinesRoot.sizeDelta = Vector2.zero;
		m_LinesRoot.SetAsFirstSibling();
	}

	private static Sprite GetWhiteSprite()
	{
		if (s_WhiteSprite == null)
		{
			s_WhiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
		}
		return s_WhiteSprite;
	}

	private void BuildLines()
	{
		EnsureLinesRoot();

		// 清理旧线（通常不会发生，但防御一下）
		for (int i = 0; i < m_Lines.Count; i++)
		{
			if (m_Lines[i] != null && m_Lines[i].Rt != null)
			{
				Destroy(m_Lines[i].Rt.gameObject);
			}
		}
		m_Lines.Clear();

		// 依赖边：node.Id <-> depId
		HashSet<string> dedup = new HashSet<string>();
		for (int i = 0; i < m_Nodes.Count; i++)
		{
			var node = m_Nodes[i];
			if (node == null || node.Depends == null) continue;

			for (int d = 0; d < node.Depends.Count; d++)
			{
				int depId = node.Depends[d];
				if (depId < 0) continue;

				int a = node.Id;
				int b = depId;
				int min = a < b ? a : b;
				int max = a < b ? b : a;
				string key = $"{min}_{max}";
				if (dedup.Contains(key)) continue;
				dedup.Add(key);

				var go = new GameObject($"Line_{a}_{b}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				go.transform.SetParent(m_LinesRoot, false);

				var img = go.GetComponent<Image>();
				img.raycastTarget = false;
				img.sprite = GetWhiteSprite();
				img.type = Image.Type.Simple;
				img.color = Color.black;

				var rt = go.GetComponent<RectTransform>();
				rt.anchorMin = new Vector2(0.5f, 0.5f);
				rt.anchorMax = new Vector2(0.5f, 0.5f);
				rt.pivot = new Vector2(0.5f, 0.5f);

				var line = new GrowthLine { A = a, B = b, Rt = rt, Img = img };
				m_Lines.Add(line);
			}
		}

		// 初次计算几何
		UpdateAllLineGeometry();
	}

	private Vector3 GetNodeLocalCenterInLinesRoot(int id)
	{
		if (m_LinesRoot == null) return Vector3.zero;
		if (!m_SlotById.TryGetValue(id, out var slot) || slot == null) return Vector3.zero;
		Vector3 world = slot.TransformPoint(slot.rect.center);
		return m_LinesRoot.InverseTransformPoint(world);
	}

	private void UpdateLineGeometry(GrowthLine line, float thickness)
	{
		if (line == null || line.Rt == null) return;
		if (!m_SlotById.ContainsKey(line.A) || !m_SlotById.ContainsKey(line.B))
		{
			line.Rt.gameObject.SetActive(false);
			return;
		}

		line.Rt.gameObject.SetActive(true);

		Vector3 a = GetNodeLocalCenterInLinesRoot(line.A);
		Vector3 b = GetNodeLocalCenterInLinesRoot(line.B);
		Vector3 mid = (a + b) * 0.5f;
		Vector3 dir = b - a;
		float len = dir.magnitude;
		if (len < 0.01f) len = 0.01f;

		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

		line.Rt.localPosition = mid;
		line.Rt.localRotation = Quaternion.Euler(0f, 0f, angle);
		line.Rt.sizeDelta = new Vector2(len, thickness);
	}

	private void UpdateAllLineGeometry()
	{
		if (m_LinesRoot == null) return;
		const float thickness = 12f;
		for (int i = 0; i < m_Lines.Count; i++)
		{
			UpdateLineGeometry(m_Lines[i], thickness);
		}
	}

	private void RefreshLinesStyle()
	{
		// 关键需求：根据 depend 两端 sold/active 调整“实/虚”
		for (int i = 0; i < m_Lines.Count; i++)
		{
			var line = m_Lines[i];
			if (line == null || line.Img == null) continue;

			bool soldA = m_IsSoldById.TryGetValue(line.A, out var sa) && sa;
			bool soldB = m_IsSoldById.TryGetValue(line.B, out var sb) && sb;
			bool activeA = m_IsActiveById.TryGetValue(line.A, out var aa) && aa;
			bool activeB = m_IsActiveById.TryGetValue(line.B, out var ab) && ab;

			float alpha;
			if (soldA && soldB)
			{
				alpha = 1f; // 实线
			}
			else if (activeA || activeB)
			{
				alpha = 0.45f; // 稍微虚一点
			}
			else
			{
				alpha = 0.2f; // 更虚一点
			}

			// 默认用深色线（避免底图是白色时看不见）
			line.Img.color = new Color(0f, 0f, 0f, alpha);
		}
	}

	private bool IsNodeActive(DataGrowth data, GrowthNode node)
	{
		if (data == null || node == null) return false;
		if (data.IsUnlocked(node.Id)) return false;
		if (node.Depends == null || node.Depends.Count == 0) return true;
		for (int i = 0; i < node.Depends.Count; i++)
		{
			if (!data.IsUnlocked(node.Depends[i])) return false;
		}
		return true;
	}

	private void Refresh()
	{
		DataGrowth data = DataSystem.Instance.GetDataGrowth();

		// 刷新局外积分显示
		if (m_View != null && m_View.TextCoins != null)
		{
			m_View.TextCoins.text = data.Points.ToString();
		}

		m_IsSoldById.Clear();
		m_IsActiveById.Clear();

		// 按 slot id 刷新按钮状态
		foreach (var kv in m_BtnById)
		{
			int id = kv.Key;
			var btn = kv.Value;
			if (btn == null) continue;

			if (!m_NodeById.TryGetValue(id, out var node) || node == null)
			{
				btn.SetTitle(string.Empty);
				btn.SetData(false, false);
				m_IsSoldById[id] = false;
				m_IsActiveById[id] = false;
				continue;
			}

			bool sold = data.IsUnlocked(node.Id);
			bool active = IsNodeActive(data, node);

			btn.SetTitle(node.Name);
			btn.SetData(sold, active);

			m_IsSoldById[id] = sold;
			m_IsActiveById[id] = active;
		}

		// 连线：位置 & 样式刷新
		UpdateAllLineGeometry();
		RefreshLinesStyle();
	}

	private void OnGrowthBtnClick(int id)
	{
		if (!m_NodeById.TryGetValue(id, out var node) || node == null) return;

		DataGrowth data = DataSystem.Instance.GetDataGrowth();
		if (data.IsUnlocked(node.Id)) return;
		if (!IsNodeActive(data, node)) return;

		var window = Asset.OpenUI<UIGrowthWindowControl>();
		window.SetData(node.Id, node.Desc, node.Price, () =>
		{
			DataSystem.Instance.GetDataGrowth().Unlock(node.Id);
			DataSystem.Instance.SaveDataGrowth();
			Refresh();
		});
	}
}