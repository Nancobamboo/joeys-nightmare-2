using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGrowthControl : YViewControl
{
	private UIGrowthView m_View;

	private const float SlotBaseW = 150f;
	private const float SlotBaseH = 100f;
	// 布局策略：
	// - 你希望“按 growth.csv 的 level 分散”，并且允许超出一屏（后续做滚动）
	// - 所以默认不再把整体强行压缩到屏幕内（AutoFitToScreen=false）
	private const bool AutoFitToScreen = false;
	private const float LayoutPadding = 260f; // 内容包围盒额外留白，避免边缘节点贴边
	// 新需求：
	// 1) 用“成长路线（树）”来画线，保证不交叉
	// 2) 用 ScrollRect 只能上下滚动（左右锁死），滚动范围等于树的高度
	private const bool UseTreeLayout = true;
	private const bool UseTreeLinesOnly = true;
	private const bool UseGridLayout = false;  // 旧网格（DAG）回退方案
	private const float GridXSpacing = 320f; // 水平间距（越大越松）
	private const float GridYSpacing = 260f; // 垂直层距（越大越松）
	private const float TreeTopPadding = 180f;
	private const float TreeBottomPadding = 220f;
	private const float HorizontalMargin = 40f; // 禁止横向滚动时，留给两侧的安全边距

	private readonly List<GrowthNode> m_Nodes = new List<GrowthNode>();
	private readonly Dictionary<int, GrowthNode> m_NodeById = new Dictionary<int, GrowthNode>();
	private readonly Dictionary<int, RectTransform> m_SlotById = new Dictionary<int, RectTransform>();
	private readonly Dictionary<int, UIBtnControl> m_BtnById = new Dictionary<int, UIBtnControl>();

	// 状态缓存，用于连线样式
	private readonly Dictionary<int, bool> m_IsSoldById = new Dictionary<int, bool>();
	private readonly Dictionary<int, bool> m_IsActiveById = new Dictionary<int, bool>();

	private RectTransform m_LinesRoot;
	private RectTransform m_SlotsRoot;
	private readonly List<GrowthLine> m_Lines = new List<GrowthLine>();
	private static Sprite s_WhiteSprite;
	private float m_LayoutScale = 1f;
	private ScrollRect m_ScrollRect;
	private RectTransform m_ViewportRt;
	private bool m_SlotsRootRuntimeCreated;
	private readonly Dictionary<int, int> m_TreeParentByChild = new Dictionary<int, int>(); // child -> parent（成长路线）

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
		public int Level = -1;
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
		EnsureScrollRect();
		EnsureSlotsRoot();
		InitSlotsAndButtons();
		BuildLines();
		Refresh();
	}

	public void SetData()
	{
		Refresh();
	}

	private void EnsureScrollRect()
	{
		if (m_ScrollRect != null) return;

		// 你已经在 prefab 里加了 ScrollRect：这里做运行时兜底配置（只上下滚动、左右锁死）
		m_ScrollRect = GetComponentInChildren<ScrollRect>(true);
		if (m_ScrollRect == null) return;

		m_ViewportRt = m_ScrollRect.viewport != null ? m_ScrollRect.viewport : (m_ScrollRect.transform as RectTransform);
		m_ScrollRect.horizontal = false;
		m_ScrollRect.vertical = true;
		m_ScrollRect.movementType = ScrollRect.MovementType.Clamped;

		// 防止拖拽时出现微小的 x 漂移
		m_ScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
		m_ScrollRect.onValueChanged.AddListener(OnScrollValueChanged);
	}

	private void OnScrollValueChanged(Vector2 _)
	{
		if (m_ScrollRect == null || m_SlotsRoot == null) return;
		var p = m_SlotsRoot.anchoredPosition;
		if (Mathf.Abs(p.x) > 0.01f)
		{
			m_SlotsRoot.anchoredPosition = new Vector2(0f, p.y);
		}
	}

	private void EnsureSlotsRoot()
	{
		if (m_SlotsRoot != null) return;

		// 旧版 prefab 里有一个名为 "GameObject" 的容器，里面手工摆了 Btn/Btn1..Btn20。
		// 新需求：不再依赖这些固定槽位，运行时隐藏旧容器，改用纯代码生成。
		var legacy = transform.Find("GameObject");
		if (legacy != null) legacy.gameObject.SetActive(false);

		// 如果有 ScrollRect：优先复用其 content，并把 GrowthSlotsRoot 作为 content（只上下滚动）
		if (m_ScrollRect != null)
		{
			if (m_ScrollRect.content != null)
			{
				m_SlotsRoot = m_ScrollRect.content;
				m_SlotsRootRuntimeCreated = false;
			}
			else
			{
				var go = new GameObject("GrowthSlotsRoot", typeof(RectTransform));
				var parent = m_ViewportRt != null ? m_ViewportRt : (m_ScrollRect.transform as RectTransform);
				go.transform.SetParent(parent, false);
				m_SlotsRoot = go.GetComponent<RectTransform>();
				m_ScrollRect.content = m_SlotsRoot;
				m_SlotsRootRuntimeCreated = true;
			}

			// 作为 ScrollRect Content：顶部对齐、水平拉伸，禁止水平滚动（宽度=Viewport）
			m_SlotsRoot.anchorMin = new Vector2(0f, 1f);
			m_SlotsRoot.anchorMax = new Vector2(1f, 1f);
			m_SlotsRoot.pivot = new Vector2(0.5f, 1f);
			m_SlotsRoot.anchoredPosition = Vector2.zero;
			m_SlotsRoot.sizeDelta = new Vector2(0f, 0f); // y 会在 UpdateSlotsRootSizeForPositions 里算出来
			return;
		}

		// 无 ScrollRect：仍用旧方式放在本 UI 根节点下（可自由居中）
		{
			var go = new GameObject("GrowthSlotsRoot", typeof(RectTransform));
			go.transform.SetParent(transform, false);
			m_SlotsRoot = go.GetComponent<RectTransform>();
			m_SlotsRootRuntimeCreated = true;

			m_SlotsRoot.anchorMin = new Vector2(0.5f, 0.5f);
			m_SlotsRoot.anchorMax = new Vector2(0.5f, 0.5f);
			m_SlotsRoot.pivot = new Vector2(0.5f, 0.5f);
			m_SlotsRoot.anchoredPosition = Vector2.zero;

			var hostRt = transform as RectTransform;
			m_SlotsRoot.sizeDelta = (hostRt != null && hostRt.rect.width > 1f && hostRt.rect.height > 1f)
				? hostRt.rect.size
				: Vector2.zero;

			// 放在背景 Image 之上、但在顶部按钮/金币之下
			var bg = transform.Find("Image");
			if (bg != null)
			{
				int idx = bg.GetSiblingIndex() + 1;
				if (idx < 0) idx = 0;
				if (idx > transform.childCount - 1) idx = transform.childCount - 1;
				m_SlotsRoot.SetSiblingIndex(idx);
			}
			else
			{
				m_SlotsRoot.SetAsFirstSibling();
			}
		}
	}

	private Dictionary<int, Vector2> BuildAutoLayoutPositions()
	{
		// 更规整的“网状”布局：六边形蜂窝网格（hex grid）。
		// - id=0 永远在中心
		// - 按 dependency 的 depth 向外扩散（depth 越大，离中心的 hex 距离越大）
		// - 每层落在规则 hex 环上，且根据父节点方向做排序/旋转，尽量减少交叉并避免重叠
		// - 无随机：每次打开都一致
		const int rootId = 0;
		const float baseHexSize = 110f;     // 调整网格疏密（越大越松）
		const float maxRadiusRatio = 0.44f; // 最大半径占 UI 根 Rect 的比例（取 min(width,height)）
		// slot 尺寸由 InitSlotsAndButtons 用 SlotBaseW/SlotBaseH + m_LayoutScale 控制

		var posById = new Dictionary<int, Vector2>();
		if (!m_NodeById.ContainsKey(rootId))
		{
			Debug.LogError("[UIGrowthControl] growth.csv missing root node id=0. Layout will still generate but center may be empty.");
		}

		// 先放 root（就算 root 不存在，也固定中心点坐标为 0）
		posById[rootId] = Vector2.zero;
		m_LayoutScale = 1f;

		// --- hex 坐标工具（axial: q,r） ---
		static int HexDistance(Vector2Int a, Vector2Int b)
		{
			// axial -> cube: (x=q, z=r, y=-x-z)
			int dx = a.x - b.x;
			int dz = a.y - b.y;
			int dy = -dx - dz;
			return (Mathf.Abs(dx) + Mathf.Abs(dy) + Mathf.Abs(dz)) / 2;
		}

		static IEnumerable<Vector2Int> HexRing(Vector2Int center, int radius)
		{
			// 返回距离 center 为 radius 的一圈 hex（顺时针）
			if (radius == 0)
			{
				yield return center;
				yield break;
			}

			// axial 方向（pointy top）
			Vector2Int[] dirs =
			{
				new Vector2Int(1, 0),
				new Vector2Int(1, -1),
				new Vector2Int(0, -1),
				new Vector2Int(-1, 0),
				new Vector2Int(-1, 1),
				new Vector2Int(0, 1),
			};

			// 从 “西北” 起点开始绕一圈（center + dir4*radius）
			Vector2Int hex = center + dirs[4] * radius;
			for (int side = 0; side < 6; side++)
			{
				for (int step = 0; step < radius; step++)
				{
					yield return hex;
					hex += dirs[side];
				}
			}
		}

		static Vector2 AxialToUnitXY(Vector2Int a)
		{
			// pointy-top axial -> pixel units（hexSize=1）
			// x = sqrt(3) * (q + r/2)
			// y = 3/2 * r
			float x = 1.7320508f * (a.x + a.y * 0.5f);
			float y = 1.5f * a.y;
			return new Vector2(x, y);
		}

		static float NormalizeAngle01(float angleRad)
		{
			float a = angleRad % (Mathf.PI * 2f);
			if (a < 0f) a += Mathf.PI * 2f;
			return a;
		}

		// 构建 children 方便做 root 可达性分析
		var childrenById = new Dictionary<int, List<int>>();
		for (int i = 0; i < m_Nodes.Count; i++)
		{
			var n = m_Nodes[i];
			if (n == null || n.Depends == null) continue;
			for (int d = 0; d < n.Depends.Count; d++)
			{
				int parent = n.Depends[d];
				if (parent < 0) continue;
				if (!childrenById.TryGetValue(parent, out var list))
				{
					list = new List<int>();
					childrenById[parent] = list;
				}
				if (!list.Contains(n.Id)) list.Add(n.Id);
			}
		}

		// root 可达集合（用于把“孤岛节点”整体推到更外圈，避免挤在中心）
		var reachable = new HashSet<int>();
		var q = new Queue<int>();
		reachable.Add(rootId);
		q.Enqueue(rootId);
		while (q.Count > 0)
		{
			int cur = q.Dequeue();
			if (!childrenById.TryGetValue(cur, out var childs) || childs == null) continue;
			for (int i = 0; i < childs.Count; i++)
			{
				int c = childs[i];
				if (reachable.Add(c)) q.Enqueue(c);
			}
		}

		// depth（使用依赖的最长链，保证子节点一定在父节点“更深层”；遇到环做保护）
		var depthMemo = new Dictionary<int, int>();
		var visiting = new HashSet<int>();
		int GetDepth(int id)
		{
			if (id == rootId) return 0;
			if (depthMemo.TryGetValue(id, out var cached)) return cached;
			if (!m_NodeById.TryGetValue(id, out var node) || node == null)
			{
				depthMemo[id] = 0;
				return 0;
			}
			if (visiting.Contains(id))
			{
				Debug.LogWarning($"[UIGrowthControl] Cycle detected in growth dependency graph at id={id}. Depth fallback to 1.");
				depthMemo[id] = 1;
				return 1;
			}

			visiting.Add(id);
			int depth = 1;
			if (node.Depends != null && node.Depends.Count > 0)
			{
				int maxParent = 0;
				for (int i = 0; i < node.Depends.Count; i++)
				{
					int p = node.Depends[i];
					if (p < 0) continue;
					if (!m_NodeById.ContainsKey(p))
					{
						Debug.LogWarning($"[UIGrowthControl] Node id={id} depends on missing parent id={p} (csv).");
					}
					int pd = GetDepth(p);
					if (pd > maxParent) maxParent = pd;
				}
				depth = maxParent + 1;
			}
			visiting.Remove(id);
			// 如果 csv 有 level，则把它作为“最低层级”约束（防止加列后想固定层级）
			if (node.Level >= 0) depth = Mathf.Max(depth, node.Level);
			depthMemo[id] = depth;
			return depth;
		}

		int maxReachDepth = 0;
		foreach (var kv in m_NodeById)
		{
			int id = kv.Key;
			if (id == rootId) continue;
			if (!reachable.Contains(id)) continue;
			int d = GetDepth(id);
			if (d > maxReachDepth) maxReachDepth = d;
		}

		var finalDepth = new Dictionary<int, int>();
		int maxDepth = 0;
		foreach (var kv in m_NodeById)
		{
			int id = kv.Key;
			if (id == rootId)
			{
				finalDepth[id] = 0;
				continue;
			}
			int d = Mathf.Max(1, GetDepth(id));
			if (!reachable.Contains(id))
			{
				// 孤岛节点整体下移（仍会按它自身依赖层级再细分）
				d += maxReachDepth + 2;
			}
			finalDepth[id] = d;
			if (d > maxDepth) maxDepth = d;
		}

		// 分层收集
		var layers = new Dictionary<int, List<int>>();
		foreach (var kv in finalDepth)
		{
			int id = kv.Key;
			int d = kv.Value;
			if (!layers.TryGetValue(d, out var list))
			{
				list = new List<int>();
				layers[d] = list;
			}
			list.Add(id);
		}

		// hexSize：默认不再按屏幕压缩（便于“按 level 拉开”且允许超出一屏）
		float hexSize = baseHexSize;
		if (AutoFitToScreen)
		{
			var hostRt = transform as RectTransform;
			if (hostRt != null && hostRt.rect.width > 1f && hostRt.rect.height > 1f)
			{
				// 粗略把 depth 当作 hex 距离上界，估算最大“半径”单位（hexSize=1 时的 max |pos|）
				// maxUnit ≈ depth * 1.5（y 方向更紧一点），这里用 1.75 提前留边
				float maxAllowedR = Mathf.Min(hostRt.rect.width, hostRt.rect.height) * maxRadiusRatio;
				float maxUnit = Mathf.Max(1f, maxDepth * 1.75f);
				hexSize = Mathf.Min(baseHexSize, maxAllowedR / maxUnit);
			}
			// 和 slot 尺寸联动：如果网格被压缩，就同步缩小 slot，避免重叠
			m_LayoutScale = Mathf.Clamp01(hexSize / baseHexSize);
		}
		else
		{
			// 不压缩：slot 用原尺寸，节点间距由 baseHexSize 决定
			m_LayoutScale = 1f;
		}

		// 放置：axial 坐标
		var origin = Vector2Int.zero;
		var axialById = new Dictionary<int, Vector2Int> { [rootId] = origin };
		var occupied = new HashSet<Vector2Int> { origin };

		float GetDesiredAngle(int id)
		{
			if (!m_NodeById.TryGetValue(id, out var n) || n == null || n.Depends == null || n.Depends.Count == 0)
			{
				// 没有父节点：按 id 稳定分散到 6 个方向
				int dir = Mathf.Abs(id) % 6;
				return dir * (Mathf.PI * 2f / 6f);
			}

			float sumX = 0f, sumY = 0f;
			int cnt = 0;
			for (int i = 0; i < n.Depends.Count; i++)
			{
				int p = n.Depends[i];
				if (p < 0) continue;
				if (axialById.TryGetValue(p, out var pa))
				{
					Vector2 u = AxialToUnitXY(pa);
					sumX += u.x;
					sumY += u.y;
					cnt++;
				}
			}
			if (cnt == 0)
			{
				int dir = Mathf.Abs(id) % 6;
				return dir * (Mathf.PI * 2f / 6f);
			}
			return NormalizeAngle01(Mathf.Atan2(sumY, sumX));
		}

		for (int d = 1; d <= maxDepth; d++)
		{
			if (!layers.TryGetValue(d, out var ids) || ids == null || ids.Count == 0) continue;

			// 先按“父节点方向”排序（稳定）
			ids.Sort((a, b) =>
			{
				int cmp = GetDesiredAngle(a).CompareTo(GetDesiredAngle(b));
				if (cmp != 0) return cmp;
				return a.CompareTo(b);
			});

			int remaining = ids.Count;
			int start = 0;
			int ringK = d;

			// 如果同一层节点太多，自动往更外的 hex 环“溢出”，确保不重叠
			while (remaining > 0)
			{
				int cap = ringK == 0 ? 1 : 6 * ringK;
				int take = Mathf.Min(remaining, cap);

				// ring positions（按角度顺序）
				var ringPos = new List<Vector2Int>(cap);
				foreach (var p in HexRing(origin, ringK)) ringPos.Add(p);
				ringPos.Sort((p1, p2) =>
				{
					float a1 = NormalizeAngle01(Mathf.Atan2(AxialToUnitXY(p1).y, AxialToUnitXY(p1).x));
					float a2 = NormalizeAngle01(Mathf.Atan2(AxialToUnitXY(p2).y, AxialToUnitXY(p2).x));
					int cmp = a1.CompareTo(a2);
					if (cmp != 0) return cmp;
					if (p1.x != p2.x) return p1.x.CompareTo(p2.x);
					return p1.y.CompareTo(p2.y);
				});

				// 根据本批次节点的平均期望角度，旋转 ring 起点（避免多圈都卡在水平轴上）
				float avg = 0f;
				for (int i = 0; i < take; i++) avg += GetDesiredAngle(ids[start + i]);
				avg /= Mathf.Max(1, take);
				int offset = Mathf.RoundToInt((avg / (Mathf.PI * 2f)) * ringPos.Count);
				offset = ((offset % ringPos.Count) + ringPos.Count) % ringPos.Count;

				for (int i = 0; i < take; i++)
				{
					int id = ids[start + i];
					if (id == rootId) continue;

					// 均匀采样 ring 位置（cap>=take 时不重复）
					int idx = (i * ringPos.Count) / take;
					idx = (idx + offset) % ringPos.Count;

					// 如果这个格子被占了（跨层/溢出情况），就沿 ring 顺时针找下一个空位
					int guard = 0;
					while (guard < ringPos.Count && occupied.Contains(ringPos[idx]))
					{
						idx = (idx + 1) % ringPos.Count;
						guard++;
					}

					// 兜底：万一整圈都满了，就继续往外扩一圈找最近空位
					Vector2Int chosen;
					if (guard >= ringPos.Count)
					{
						int extra = 1;
						chosen = origin;
						while (extra < 64)
						{
							foreach (var p in HexRing(origin, ringK + extra))
							{
								if (occupied.Contains(p)) continue;
								if (HexDistance(origin, p) < d) continue; // 不要跑到比 depth 更近的位置
								chosen = p;
								goto FOUND;
							}
							extra++;
						}
					FOUND: ;
					}
					else
					{
						chosen = ringPos[idx];
					}

					occupied.Add(chosen);
					axialById[id] = chosen;

					// 转像素坐标
					Vector2 unit = AxialToUnitXY(chosen);
					// UI 坐标：y 方向翻转一下更符合直觉（上为正）
					posById[id] = new Vector2(unit.x * hexSize, -unit.y * hexSize);
				}

				start += take;
				remaining -= take;
				ringK++;
			}
		}

		// 根节点强制回到中心（保险）
		posById[rootId] = Vector2.zero;

		return posById;
	}

	private Dictionary<int, Vector2> BuildGridLayoutPositions()
	{
		// 规整网格分层布局（Sugiyama 简化版）：
		// - 以 csv 的 level 为“层”（y 轴），同层节点水平排开（x 轴）
		// - 只对“相邻层”边做重心排序迭代，目标是把交叉降到 0（如果你的设计保证可平面嵌入，这里就能做到完全无交叉）
		const int rootId = 0;

		var posById = new Dictionary<int, Vector2>();
		m_LayoutScale = 1f;

		// --- children 及可达集合（用于孤岛节点下移） ---
		var childrenById = new Dictionary<int, List<int>>();
		for (int i = 0; i < m_Nodes.Count; i++)
		{
			var n = m_Nodes[i];
			if (n == null || n.Depends == null) continue;
			for (int d = 0; d < n.Depends.Count; d++)
			{
				int parent = n.Depends[d];
				if (parent < 0) continue;
				if (!childrenById.TryGetValue(parent, out var list))
				{
					list = new List<int>();
					childrenById[parent] = list;
				}
				if (!list.Contains(n.Id)) list.Add(n.Id);
			}
		}

		var reachable = new HashSet<int>();
		var q = new Queue<int>();
		reachable.Add(rootId);
		q.Enqueue(rootId);
		while (q.Count > 0)
		{
			int cur = q.Dequeue();
			if (!childrenById.TryGetValue(cur, out var childs) || childs == null) continue;
			for (int i = 0; i < childs.Count; i++)
			{
				int c = childs[i];
				if (reachable.Add(c)) q.Enqueue(c);
			}
		}

		// --- 计算 layer（优先用 csv 的 Level；缺失则用依赖深度兜底） ---
		var layerMemo = new Dictionary<int, int>();
		var visiting = new HashSet<int>();

		int GetLayer(int id)
		{
			if (id == rootId) return 0;
			if (layerMemo.TryGetValue(id, out var cached)) return cached;
			if (!m_NodeById.TryGetValue(id, out var node) || node == null)
			{
				layerMemo[id] = 0;
				return 0;
			}
			if (visiting.Contains(id))
			{
				Debug.LogWarning($"[UIGrowthControl] Cycle detected in growth dependency graph at id={id}. Layer fallback to 1.");
				layerMemo[id] = 1;
				return 1;
			}

			visiting.Add(id);

			int layer;
			if (node.Level >= 0)
			{
				layer = node.Level;
			}
			else
			{
				int maxParent = 0;
				if (node.Depends != null)
				{
					for (int i = 0; i < node.Depends.Count; i++)
					{
						int p = node.Depends[i];
						if (p < 0) continue;
						int pl = GetLayer(p);
						if (pl > maxParent) maxParent = pl;
					}
				}
				layer = maxParent + 1;
			}

			visiting.Remove(id);
			layerMemo[id] = layer;
			return layer;
		}

		int maxReachLayer = 0;
		foreach (var kv in m_NodeById)
		{
			int id = kv.Key;
			if (id == rootId) continue;
			if (!reachable.Contains(id)) continue;
			int l = GetLayer(id);
			if (l > maxReachLayer) maxReachLayer = l;
		}

		// 最终 layer（把孤岛整体下移，避免混在主图里）
		var finalLayer = new Dictionary<int, int>();
		int maxLayer = 0;
		foreach (var kv in m_NodeById)
		{
			int id = kv.Key;
			int l = GetLayer(id);
			if (id != rootId && !reachable.Contains(id))
			{
				l += maxReachLayer + 2;
			}
			finalLayer[id] = l;
			if (l > maxLayer) maxLayer = l;
		}

		// 按层收集
		var layers = new Dictionary<int, List<int>>();
		foreach (var kv in finalLayer)
		{
			if (!layers.TryGetValue(kv.Value, out var list))
			{
				list = new List<int>();
				layers[kv.Value] = list;
			}
			list.Add(kv.Key);
		}

		var layerKeys = new List<int>(layers.Keys);
		layerKeys.Sort();
		if (layerKeys.Count == 0) return posById;

		// --- 构建“相邻层”边（用于交叉最小化） ---
		var parentsPrevLayer = new Dictionary<int, List<int>>();
		var childrenNextLayer = new Dictionary<int, List<int>>();

		void AddAdj(int parent, int child)
		{
			if (!childrenNextLayer.TryGetValue(parent, out var cList))
			{
				cList = new List<int>();
				childrenNextLayer[parent] = cList;
			}
			if (!cList.Contains(child)) cList.Add(child);

			if (!parentsPrevLayer.TryGetValue(child, out var pList))
			{
				pList = new List<int>();
				parentsPrevLayer[child] = pList;
			}
			if (!pList.Contains(parent)) pList.Add(parent);
		}

		for (int i = 0; i < m_Nodes.Count; i++)
		{
			var n = m_Nodes[i];
			if (n == null || n.Depends == null) continue;
			int child = n.Id;
			if (!finalLayer.TryGetValue(child, out var cl)) continue;
			for (int d = 0; d < n.Depends.Count; d++)
			{
				int parent = n.Depends[d];
				if (parent < 0) continue;
				if (!finalLayer.TryGetValue(parent, out var pl)) continue;
				// 只拿相邻层边（保证“直线连接两层”时的平面性判断）
				if (cl == pl + 1) AddAdj(parent, child);
			}
		}

		// --- 初始化每层顺序（稳定：root 层固定） ---
		var order = new Dictionary<int, List<int>>();
		foreach (var l in layerKeys)
		{
			var list = layers[l];
			list.Sort((a, b) => a.CompareTo(b));
			order[l] = new List<int>(list);
		}
		if (order.TryGetValue(0, out var rootLayer))
		{
			// 如果 root 层有多个，仍保持 id=0 在最中间/最前
			rootLayer.Sort((a, b) =>
			{
				if (a == rootId && b != rootId) return -1;
				if (b == rootId && a != rootId) return 1;
				return a.CompareTo(b);
			});
		}

		static Dictionary<int, int> BuildIndexMap(List<int> ids)
		{
			var m = new Dictionary<int, int>(ids.Count);
			for (int i = 0; i < ids.Count; i++) m[ids[i]] = i;
			return m;
		}

		float GetBarycenterOfParents(int nodeId, Dictionary<int, int> prevIndex)
		{
			if (!parentsPrevLayer.TryGetValue(nodeId, out var ps) || ps == null || ps.Count == 0) return float.NaN;
			float sum = 0f;
			int cnt = 0;
			for (int i = 0; i < ps.Count; i++)
			{
				if (prevIndex.TryGetValue(ps[i], out var idx))
				{
					sum += idx;
					cnt++;
				}
			}
			return cnt > 0 ? sum / cnt : float.NaN;
		}

		float GetBarycenterOfChildren(int nodeId, Dictionary<int, int> nextIndex)
		{
			if (!childrenNextLayer.TryGetValue(nodeId, out var cs) || cs == null || cs.Count == 0) return float.NaN;
			float sum = 0f;
			int cnt = 0;
			for (int i = 0; i < cs.Count; i++)
			{
				if (nextIndex.TryGetValue(cs[i], out var idx))
				{
					sum += idx;
					cnt++;
				}
			}
			return cnt > 0 ? sum / cnt : float.NaN;
		}

		int CountCrossingsBetween(int layerA, int layerB)
		{
			// layerB 必须是 layerA+1（相邻层）
			if (!order.TryGetValue(layerA, out var aList) || !order.TryGetValue(layerB, out var bList)) return 0;
			var aIndex = BuildIndexMap(aList);
			var bIndex = BuildIndexMap(bList);

			var edges = new List<(int a, int b)>();
			for (int i = 0; i < bList.Count; i++)
			{
				int child = bList[i];
				if (!parentsPrevLayer.TryGetValue(child, out var ps) || ps == null) continue;
				for (int p = 0; p < ps.Count; p++)
				{
					int parent = ps[p];
					if (!aIndex.TryGetValue(parent, out var ai)) continue;
					if (!bIndex.TryGetValue(child, out var bi)) continue;
					edges.Add((ai, bi));
				}
			}

			edges.Sort((e1, e2) =>
			{
				int c = e1.a.CompareTo(e2.a);
				if (c != 0) return c;
				return e1.b.CompareTo(e2.b);
			});

			// O(E^2) 统计 inversions（规模很小，够用且更稳）
			int cross = 0;
			for (int i = 0; i < edges.Count; i++)
			{
				for (int j = i + 1; j < edges.Count; j++)
				{
					// 同一 a 不算交叉（从同一父节点发散的线共享起点是允许的）
					if (edges[i].a == edges[j].a) continue;
					if (edges[i].b > edges[j].b) cross++;
				}
			}
			return cross;
		}

		int CountAllCrossings()
		{
			int cross = 0;
			for (int i = 0; i < layerKeys.Count - 1; i++)
			{
				int a = layerKeys[i];
				int b = layerKeys[i + 1];
				if (b != a + 1) continue; // 只统计相邻层
				cross += CountCrossingsBetween(a, b);
			}
			return cross;
		}

		// --- 迭代重心排序，目标：交叉为 0 ---
		const int maxIter = 12;
		int lastCross = CountAllCrossings();
		for (int iter = 0; iter < maxIter; iter++)
		{
			// downward sweep：按父节点重心排序
			for (int li = 1; li < layerKeys.Count; li++)
			{
				int l = layerKeys[li];
				int prevL = layerKeys[li - 1];
				if (l != prevL + 1) continue;

				var prevIndex = BuildIndexMap(order[prevL]);
				var cur = order[l];
				var curIndex = BuildIndexMap(cur);
				cur.Sort((a, b) =>
				{
					float ba = GetBarycenterOfParents(a, prevIndex);
					float bb = GetBarycenterOfParents(b, prevIndex);
					bool na = float.IsNaN(ba);
					bool nb = float.IsNaN(bb);
					if (!na && !nb)
					{
						int c = ba.CompareTo(bb);
						if (c != 0) return c;
					}
					else if (!na && nb) return -1;
					else if (na && !nb) return 1;
					// tie：保持稳定（按原索引再按 id）
					int ia = curIndex.TryGetValue(a, out var va) ? va : 0;
					int ib = curIndex.TryGetValue(b, out var vb) ? vb : 0;
					int cc = ia.CompareTo(ib);
					return cc != 0 ? cc : a.CompareTo(b);
				});
			}

			// upward sweep：按子节点重心排序（root 层固定不动）
			for (int li = layerKeys.Count - 2; li >= 1; li--)
			{
				int l = layerKeys[li];
				int nextL = layerKeys[li + 1];
				if (nextL != l + 1) continue;

				var nextIndex = BuildIndexMap(order[nextL]);
				var cur = order[l];
				var curIndex = BuildIndexMap(cur);
				cur.Sort((a, b) =>
				{
					float ba = GetBarycenterOfChildren(a, nextIndex);
					float bb = GetBarycenterOfChildren(b, nextIndex);
					bool na = float.IsNaN(ba);
					bool nb = float.IsNaN(bb);
					if (!na && !nb)
					{
						int c = ba.CompareTo(bb);
						if (c != 0) return c;
					}
					else if (!na && nb) return -1;
					else if (na && !nb) return 1;
					int ia = curIndex.TryGetValue(a, out var va) ? va : 0;
					int ib = curIndex.TryGetValue(b, out var vb) ? vb : 0;
					int cc = ia.CompareTo(ib);
					return cc != 0 ? cc : a.CompareTo(b);
				});
			}

			int now = CountAllCrossings();
			if (now == 0) break;
			if (now >= lastCross) break;
			lastCross = now;
		}

		// --- 生成坐标 ---
		for (int li = 0; li < layerKeys.Count; li++)
		{
			int l = layerKeys[li];
			var list = order[l];
			if (list == null || list.Count == 0) continue;

			// root 节点固定在 x=0；其余同层居中排列
			if (l == 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					int id = list[i];
					posById[id] = (id == rootId) ? Vector2.zero : new Vector2((i - (list.Count - 1) * 0.5f) * GridXSpacing, 0f);
				}
				continue;
			}

			float cx = (list.Count - 1) * 0.5f;
			float y = -l * GridYSpacing;
			for (int i = 0; i < list.Count; i++)
			{
				int id = list[i];
				float x = (i - cx) * GridXSpacing;
				posById[id] = new Vector2(x, y);
			}
		}

		// 保险：root 回到中心
		posById[rootId] = Vector2.zero;
		return posById;
	}

	private Vector2 GetViewportSize()
	{
		if (m_ViewportRt != null && m_ViewportRt.rect.width > 1f && m_ViewportRt.rect.height > 1f)
		{
			return m_ViewportRt.rect.size;
		}
		var hostRt = transform as RectTransform;
		if (hostRt != null && hostRt.rect.width > 1f && hostRt.rect.height > 1f)
		{
			return hostRt.rect.size;
		}
		return Vector2.zero;
	}

	private Dictionary<int, Vector2> BuildTreeLayoutPositions()
	{
		// 树布局（成长路线）：
		// - 每个节点只选一个“主父节点”（成长路线），形成树 => 直线连接天然不交叉
		// - y 用 csv 的 level（并保证父层 < 子层），x 用“叶子顺序/子树中心”分配（tidy tree 简化版）
		const int rootId = 0;

		var posById = new Dictionary<int, Vector2>();
		m_TreeParentByChild.Clear();
		m_LayoutScale = 1f;

		// --- base level（优先 csv Level；缺失则用依赖深度兜底） ---
		var baseLevelMemo = new Dictionary<int, int>();
		var visiting = new HashSet<int>();
		int GetBaseLevel(int id)
		{
			if (id == rootId) return 0;
			if (baseLevelMemo.TryGetValue(id, out var cached)) return cached;
			if (!m_NodeById.TryGetValue(id, out var node) || node == null)
			{
				baseLevelMemo[id] = 0;
				return 0;
			}
			if (visiting.Contains(id))
			{
				Debug.LogWarning($"[UIGrowthControl] Cycle detected in growth dependency graph at id={id}. BaseLevel fallback to 1.");
				baseLevelMemo[id] = 1;
				return 1;
			}

			visiting.Add(id);
			int level;
			if (node.Level >= 0)
			{
				level = node.Level;
			}
			else
			{
				int maxParent = 0;
				if (node.Depends != null)
				{
					for (int i = 0; i < node.Depends.Count; i++)
					{
						int p = node.Depends[i];
						if (p < 0) continue;
						int pl = GetBaseLevel(p);
						if (pl > maxParent) maxParent = pl;
					}
				}
				level = maxParent + 1;
			}
			visiting.Remove(id);
			baseLevelMemo[id] = level;
			return level;
		}

		// --- 选择“成长路线”的主父节点（只用于画线/布局，不影响解锁条件） ---
		int ChoosePrimaryParent(int childId)
		{
			if (!m_NodeById.TryGetValue(childId, out var node) || node == null) return rootId;
			if (node.Depends == null || node.Depends.Count == 0) return rootId;

			int childLevel = GetBaseLevel(childId);
			int chosen = -1;
			int chosenLevel = int.MinValue;

			// 优先：父 level < 子 level 且尽量接近（最大化父 level）
			for (int i = 0; i < node.Depends.Count; i++)
			{
				int p = node.Depends[i];
				if (p < 0) continue;
				if (!m_NodeById.ContainsKey(p)) continue;
				int pl = GetBaseLevel(p);
				if (pl >= childLevel) continue;
				if (pl > chosenLevel || (pl == chosenLevel && p < chosen))
				{
					chosen = p;
					chosenLevel = pl;
				}
			}

			if (chosen >= 0) return chosen;

			// 兜底：选 id 最小的合法依赖
			for (int i = 0; i < node.Depends.Count; i++)
			{
				int p = node.Depends[i];
				if (p < 0) continue;
				if (!m_NodeById.ContainsKey(p)) continue;
				if (chosen < 0 || p < chosen) chosen = p;
			}
			return chosen >= 0 ? chosen : rootId;
		}

		foreach (var kv in m_NodeById)
		{
			int id = kv.Key;
			if (id == rootId) continue;
			int parent = ChoosePrimaryParent(id);
			if (parent < 0) parent = rootId;
			m_TreeParentByChild[id] = parent;
		}

		// --- children adjacency ---
		var childrenById = new Dictionary<int, List<int>>();
		void AddChild(int parent, int child)
		{
			if (!childrenById.TryGetValue(parent, out var list))
			{
				list = new List<int>();
				childrenById[parent] = list;
			}
			if (!list.Contains(child)) list.Add(child);
		}
		foreach (var kv in m_TreeParentByChild)
		{
			AddChild(kv.Value, kv.Key);
		}

		// --- enforce level monotonic along the chosen route ---
		var levelById = new Dictionary<int, int>();
		foreach (var kv in m_NodeById)
		{
			levelById[kv.Key] = Mathf.Max(0, GetBaseLevel(kv.Key));
		}
		levelById[rootId] = 0;

		var levelVisited = new HashSet<int>();
		void EnforceLevel(int id)
		{
			if (!levelVisited.Add(id)) return;
			if (!childrenById.TryGetValue(id, out var cs) || cs == null) return;
			for (int i = 0; i < cs.Count; i++)
			{
				int c = cs[i];
				levelById[c] = Mathf.Max(levelById[c], levelById[id] + 1);
				EnforceLevel(c);
			}
		}
		EnforceLevel(rootId);

		// children 排序（稳定 + 更符合 level 从左到右的直觉）
		foreach (var kv in childrenById)
		{
			kv.Value.Sort((a, b) =>
			{
				int la = levelById.TryGetValue(a, out var va) ? va : 0;
				int lb = levelById.TryGetValue(b, out var vb) ? vb : 0;
				int c = la.CompareTo(lb);
				return c != 0 ? c : a.CompareTo(b);
			});
		}

		// --- tidy tree：给每个节点一个 xIndex（叶子从左到右递增，父节点取子节点平均） ---
		var xIndexById = new Dictionary<int, float>();
		var xVisited = new HashSet<int>();
		int nextLeaf = 0;
		void AssignX(int id)
		{
			if (!xVisited.Add(id)) return;
			if (!childrenById.TryGetValue(id, out var cs) || cs == null || cs.Count == 0)
			{
				xIndexById[id] = nextLeaf;
				nextLeaf++;
				return;
			}
			for (int i = 0; i < cs.Count; i++) AssignX(cs[i]);
			float sum = 0f;
			for (int i = 0; i < cs.Count; i++) sum += xIndexById[cs[i]];
			xIndexById[id] = sum / Mathf.Max(1, cs.Count);
		}
		AssignX(rootId);

		// 兜底：如果有节点没被 root 覆盖（比如 root 缺失/断链），也给它们分配位置（放到右侧，不影响主树）
		foreach (var kv in m_NodeById)
		{
			if (!xIndexById.ContainsKey(kv.Key))
			{
				AssignX(kv.Key);
			}
		}

		float rootX = xIndexById.TryGetValue(rootId, out var rx) ? rx : 0f;
		foreach (var kv in m_NodeById)
		{
			int id = kv.Key;
			float xi = xIndexById.TryGetValue(id, out var v) ? v : 0f;
			int level = levelById.TryGetValue(id, out var l) ? l : 0;
			float x = (xi - rootX) * GridXSpacing;
			float y = -(level * GridYSpacing + TreeTopPadding);
			posById[id] = new Vector2(x, y);
		}

		// 禁止横向滚动：如果超出 viewport，则按比例压缩 x（不改变左右顺序 => 不会产生交叉）
		Vector2 vp = GetViewportSize();
		if (vp.x > 1f)
		{
			float maxAbsX = 0f;
			foreach (var kv in posById)
			{
				float ax = Mathf.Abs(kv.Value.x);
				if (ax > maxAbsX) maxAbsX = ax;
			}
			float allowed = vp.x * 0.5f - SlotBaseW * 0.5f - HorizontalMargin;
			if (allowed > 10f && maxAbsX > allowed)
			{
				float scale = allowed / maxAbsX;
				var keys = new List<int>(posById.Keys);
				for (int i = 0; i < keys.Count; i++)
				{
					int id = keys[i];
					var p = posById[id];
					posById[id] = new Vector2(p.x * scale, p.y);
				}
			}
		}

		// root 保险
		posById[rootId] = new Vector2(0f, -(TreeTopPadding));
		return posById;
	}

	private void UpdateSlotsRootSizeForPositions(Dictionary<int, Vector2> posById)
	{
		if (m_SlotsRoot == null || posById == null || posById.Count == 0) return;

		float minX = float.PositiveInfinity;
		float maxX = float.NegativeInfinity;
		float minY = float.PositiveInfinity;
		float maxY = float.NegativeInfinity;

		foreach (var kv in posById)
		{
			Vector2 p = kv.Value;
			if (p.x < minX) minX = p.x;
			if (p.x > maxX) maxX = p.x;
			if (p.y < minY) minY = p.y;
			if (p.y > maxY) maxY = p.y;
		}

		if (float.IsInfinity(minX) || float.IsInfinity(minY)) return;

		float s = Mathf.Clamp(m_LayoutScale, 0.6f, 1f);
		float slotW = SlotBaseW * s;
		float slotH = SlotBaseH * s;

		// 由于布局中心固定在 (0,0)，用 maxAbs 确保 sizeDelta 覆盖两侧
		float maxAbsX = Mathf.Max(Mathf.Abs(minX), Mathf.Abs(maxX));
		float maxAbsY = Mathf.Max(Mathf.Abs(minY), Mathf.Abs(maxY));

		// ScrollRect 模式：只允许上下滚动 => content 宽度锁定为 viewport，height = 树的高度
		if (m_ScrollRect != null)
		{
			// 顶部对齐 + 水平拉伸（宽度=viewport）
			m_SlotsRoot.anchorMin = new Vector2(0f, 1f);
			m_SlotsRoot.anchorMax = new Vector2(1f, 1f);
			m_SlotsRoot.pivot = new Vector2(0.5f, 1f);
			m_SlotsRoot.anchoredPosition = Vector2.zero;
			m_SlotsRoot.sizeDelta = new Vector2(0f, 0f);
			m_ScrollRect.content = m_SlotsRoot;

			Vector2 vp = GetViewportSize();
			float minVisibleH = vp.y > 1f ? vp.y : 0f;

			// y 是从顶部往下为负数：取最底部（最小 y）决定 content 高度
			float bottomMost = minY - slotH * 0.5f - TreeBottomPadding;
			float needH = Mathf.Abs(bottomMost);
			if (needH < minVisibleH) needH = minVisibleH;
			m_SlotsRoot.sizeDelta = new Vector2(0f, needH);
			return;
		}

		// 非 ScrollRect：仍按包围盒扩展到足够大（允许四向）
		float wantW = maxAbsX * 2f + slotW + LayoutPadding;
		float wantH = maxAbsY * 2f + slotH + LayoutPadding;

		var hostRt = transform as RectTransform;
		if (hostRt != null && hostRt.rect.width > 1f && hostRt.rect.height > 1f)
		{
			wantW = Mathf.Max(wantW, hostRt.rect.width);
			wantH = Mathf.Max(wantH, hostRt.rect.height);
		}

		m_SlotsRoot.sizeDelta = new Vector2(wantW, wantH);
	}

	private static string[] ParseCSVLine(string line)
	{
		// 简易 CSV 解析：支持双引号包裹字段，支持字段内逗号与 "" 转义
		if (line == null) return Array.Empty<string>();
		List<string> values = new List<string>();
		bool inQuotes = false;
		string current = "";

		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			if (c == '"')
			{
				if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
				{
					current += '"';
					i++;
				}
				else
				{
					inQuotes = !inQuotes;
				}
			}
			else if (c == ',' && !inQuotes)
			{
				values.Add(current);
				current = "";
			}
			else if (c != '\r')
			{
				current += c;
			}
		}
		values.Add(current);
		return values.ToArray();
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
		if (lines == null || lines.Length == 0) return;

		// header：按列名映射，支持新增/调整列顺序（比如你现在加了 level）
		string headerLine = lines[0].Trim();
		if (string.IsNullOrWhiteSpace(headerLine))
		{
			Debug.LogError("[UIGrowthControl] growth.csv header is empty.");
			return;
		}

		string[] header = ParseCSVLine(headerLine);
		var idx = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < header.Length; i++)
		{
			string key = (header[i] ?? string.Empty).Trim();
			if (!string.IsNullOrEmpty(key) && !idx.ContainsKey(key)) idx[key] = i;
		}

		int GetIdx(params string[] keys)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				if (idx.TryGetValue(keys[i], out int v)) return v;
			}
			return -1;
		}

		int idIdx = GetIdx("id", "ID", "Id");
		int levelIdx = GetIdx("level", "Level", "lv", "Lv");
		int nameIdx = GetIdx("name", "Name", "title", "Title");
		int depIdx = GetIdx("dependency", "depends", "depend", "dep");
		int descIdx = GetIdx("desc", "Desc", "description", "Description");
		int priceIdx = GetIdx("price", "Price", "cost", "Cost");

		if (idIdx < 0 || nameIdx < 0)
		{
			Debug.LogError("[UIGrowthControl] growth.csv missing required columns: id/name");
			return;
		}

		// Skip header
		for (int i = 1; i < lines.Length; i++)
		{
			string line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;
			line = line.Trim();
			if (string.IsNullOrEmpty(line)) continue;

			string[] cols = ParseCSVLine(line);
			if (cols == null || cols.Length == 0) continue;

			string Get(int index)
			{
				if (index < 0 || index >= cols.Length) return string.Empty;
				return (cols[index] ?? string.Empty).Trim();
			}

			if (!int.TryParse(Get(idIdx), out int id)) continue;

			var node = new GrowthNode();
			node.Id = id;
			node.Name = Get(nameIdx);

			if (levelIdx >= 0) int.TryParse(Get(levelIdx), out node.Level);

			// dependency 允许单个数字或用 | / ; 分隔的多个依赖
			string depStr = Get(depIdx);
			node.Depends.Clear();
			if (!string.IsNullOrEmpty(depStr))
			{
				var parts = depStr.Split(new char[] { '|', ';' }, StringSplitOptions.RemoveEmptyEntries);
				for (int p = 0; p < parts.Length; p++)
				{
					if (int.TryParse(parts[p].Trim(), out int depId))
					{
						if (depId >= 0) node.Depends.Add(depId);
					}
				}
			}

			node.Desc = Get(descIdx);
			if (priceIdx >= 0) int.TryParse(Get(priceIdx), out node.Price);

			m_Nodes.Add(node);
			if (m_NodeById.ContainsKey(node.Id))
			{
				Debug.LogWarning($"[UIGrowthControl] Duplicate growth id={node.Id} in csv. Last one wins (line {i + 1}).");
			}
			m_NodeById[node.Id] = node;
		}
	}

	private void InitSlotsAndButtons()
	{
		m_SlotById.Clear();
		m_BtnById.Clear();
		EnsureSlotsRoot();

		// 清空旧生成的 slot（防御：理论上每次打开都是新实例，但避免复用时残留）
		if (m_SlotsRoot != null)
		{
			for (int i = m_SlotsRoot.childCount - 1; i >= 0; i--)
			{
				Destroy(m_SlotsRoot.GetChild(i).gameObject);
			}
		}

		// 按 CSV 自动生成位置
		var posById = UseTreeLayout
			? BuildTreeLayoutPositions()
			: (UseGridLayout ? BuildGridLayoutPositions() : BuildAutoLayoutPositions());
		UpdateSlotsRootSizeForPositions(posById);

		// 生成每一个节点的 slot + UIBtn
		foreach (var kv in m_NodeById)
		{
			int id = kv.Key;
			var node = kv.Value;
			if (node == null) continue;

			var slotGo = new GameObject($"Slot_{id}", typeof(RectTransform));
			slotGo.transform.SetParent(m_SlotsRoot, false);
			var slot = slotGo.GetComponent<RectTransform>();
			// ScrollRect content：以顶部为锚点更自然（y 负方向向下）；非 ScrollRect：居中
			if (m_ScrollRect != null)
			{
				slot.anchorMin = new Vector2(0.5f, 1f);
				slot.anchorMax = new Vector2(0.5f, 1f);
			}
			else
			{
				slot.anchorMin = new Vector2(0.5f, 0.5f);
				slot.anchorMax = new Vector2(0.5f, 0.5f);
			}
			slot.pivot = new Vector2(0.5f, 0.5f);
			float s = Mathf.Clamp(m_LayoutScale, 0.6f, 1f);
			slot.sizeDelta = new Vector2(SlotBaseW * s, SlotBaseH * s);
			slot.anchoredPosition = posById.TryGetValue(id, out var p) ? p : Vector2.zero;

			var btnCtrl = Asset.OpenUI<UIBtnControl>(slot);
			btnCtrl.Setup(id, OnGrowthBtnClick);

			// 让 UIBtn 填满槽位
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

			btnCtrl.SetTitle(node.Name);

			m_SlotById[id] = slot;
			m_BtnById[id] = btnCtrl;
		}
	}

	private void EnsureLinesRoot()
	{
		if (m_LinesRoot != null) return;

		// 放在按钮槽位同级的父节点下，并置底，确保在按钮背后
		var parent = (m_SlotsRoot != null) ? m_SlotsRoot : transform;
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

		void CreateLine(int a, int b)
		{
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

		// 成长路线（树）：只画主父子边 => 直线天然不交叉
		if (UseTreeLinesOnly && m_TreeParentByChild != null && m_TreeParentByChild.Count > 0)
		{
			foreach (var kv in m_TreeParentByChild)
			{
				int child = kv.Key;
				int parent = kv.Value;
				if (parent < 0) continue;
				CreateLine(child, parent);
			}
		}
		else
		{
			// 回退：画所有依赖边（可能交叉）
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
					CreateLine(a, b);
				}
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

	private bool TryGetSlotLocalAABBInLinesRoot(int id, out Vector2 min, out Vector2 max, out Vector2 center)
	{
		min = Vector2.zero;
		max = Vector2.zero;
		center = Vector2.zero;

		if (m_LinesRoot == null) return false;
		if (!m_SlotById.TryGetValue(id, out var slot) || slot == null) return false;

		Vector3[] corners = new Vector3[4];
		slot.GetWorldCorners(corners);

		float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
		float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
		for (int i = 0; i < 4; i++)
		{
			var local = (Vector2)m_LinesRoot.InverseTransformPoint(corners[i]);
			if (local.x < minX) minX = local.x;
			if (local.y < minY) minY = local.y;
			if (local.x > maxX) maxX = local.x;
			if (local.y > maxY) maxY = local.y;
		}

		min = new Vector2(minX, minY);
		max = new Vector2(maxX, maxY);
		center = (min + max) * 0.5f;
		return true;
	}

	// 从 rect 的中心朝 target 方向，求与 rect 边界的交点（在 LinesRoot 的 local 坐标）
	private Vector2 GetRectEdgePointTowards(int id, Vector2 targetLocal, float inset)
	{
		if (!TryGetSlotLocalAABBInLinesRoot(id, out var min, out var max, out var center))
		{
			return GetNodeLocalCenterInLinesRoot(id);
		}

		Vector2 dir = targetLocal - center;
		if (dir.sqrMagnitude < 0.0001f) return center;

		float dx = dir.x;
		float dy = dir.y;

		// 计算射线 center + t*dir 与 AABB 边界的最近正交点 t
		float tX = float.PositiveInfinity;
		if (Mathf.Abs(dx) > 0.0001f)
		{
			float boundX = dx > 0 ? max.x : min.x;
			tX = (boundX - center.x) / dx;
		}

		float tY = float.PositiveInfinity;
		if (Mathf.Abs(dy) > 0.0001f)
		{
			float boundY = dy > 0 ? max.y : min.y;
			tY = (boundY - center.y) / dy;
		}

		float t = Mathf.Min(tX, tY);
		if (float.IsInfinity(t) || t <= 0f) t = 1f;

		Vector2 edge = center + dir * t;

		// 留一点点间距，避免线压到按钮边缘（inset>0 会把点往中心缩）
		Vector2 n = dir.normalized;
		edge -= n * inset;

		return edge;
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

		// 关键：线不穿过按钮本体，只连到按钮边缘
		Vector2 centerA = (Vector2)GetNodeLocalCenterInLinesRoot(line.A);
		Vector2 centerB = (Vector2)GetNodeLocalCenterInLinesRoot(line.B);
		const float inset = 6f;
		Vector2 a2 = GetRectEdgePointTowards(line.A, centerB, inset);
		Vector2 b2 = GetRectEdgePointTowards(line.B, centerA, inset);

		Vector3 a = new Vector3(a2.x, a2.y, 0f);
		Vector3 b = new Vector3(b2.x, b2.y, 0f);
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

	protected override void OnClose()
	{
		// 这个 UI 关闭时 GameObject 会被销毁，这里只做引用清理与额外节点销毁（防御/避免复用时残留）
		if (m_ScrollRect != null)
		{
			m_ScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
			m_ScrollRect = null;
			m_ViewportRt = null;
		}
		if (m_LinesRoot != null)
		{
			Destroy(m_LinesRoot.gameObject);
			m_LinesRoot = null;
		}
		if (m_SlotsRoot != null)
		{
			// 如果复用了 ScrollRect prefab 自带的 content，这里不要额外 Destroy（避免影响别的层级）
			if (m_SlotsRootRuntimeCreated)
			{
				Destroy(m_SlotsRoot.gameObject);
			}
			m_SlotsRoot = null;
			m_SlotsRootRuntimeCreated = false;
		}
		m_Lines.Clear();
		m_BtnById.Clear();
		m_SlotById.Clear();
		m_TreeParentByChild.Clear();
		m_NodeById.Clear();
		m_Nodes.Clear();
		base.OnClose();
	}

	protected override void OnReturn()
	{
		base.OnReturn();
	}
}