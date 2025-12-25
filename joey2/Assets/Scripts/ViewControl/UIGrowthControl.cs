using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGrowthControl : YViewControl
{
	private UIGrowthView m_View;
	private MonoBehaviourPool<UIDamageTextControl> m_DamageTextPool;
	private UIGrowthWindowControl m_GrowthWindow;

	private const float SlotBaseW = 150f;
	private const float SlotBaseH = 100f;
	private const bool AutoFitToScreen = false;
	private const float LayoutPadding = 260f;
	private const bool UseTreeLayout = true;
	private const bool UseTreeLinesOnly = true;
	private const bool UseGridLayout = false;
	private const float GridXSpacing = 320f;
	private const float GridYSpacing = 260f;
	// TreeLayout 横向间距：以按钮宽度为基准，尽量做到“两个按钮之间留半个按钮宽”的空隙
	// 即 center-to-center ≈ btnW * (1 + 0.5) = 1.5 * btnW
	private const float TreeXGapRatio = 0.5f;
	private const float TreeTopPadding = 180f;
	private const float TreeBottomPadding = 220f;
	private const float HorizontalMargin = 40f;
	// 旧逻辑会把树的宽度强行压缩到视口内，导致节点横向越发拥挤。
	// 现在改为允许左右拖动浏览，所以默认不再强行压缩。
	private const bool ClampTreeWidthToViewport = false;
	private readonly List<GrowthNode> m_Nodes = new List<GrowthNode>();
	private readonly Dictionary<int, GrowthNode> m_NodeById = new Dictionary<int, GrowthNode>();
	private readonly Dictionary<int, RectTransform> m_SlotById = new Dictionary<int, RectTransform>();
	private readonly Dictionary<int, UIBtnControl> m_BtnById = new Dictionary<int, UIBtnControl>();
	private readonly Dictionary<int, bool> m_IsSoldById = new Dictionary<int, bool>();
	private readonly Dictionary<int, bool> m_IsActiveById = new Dictionary<int, bool>();
	private RectTransform m_SlotsRoot;
	private static Sprite s_WhiteSprite;
	private float m_LayoutScale = 1f;
	private ScrollRect m_ScrollRect;
	private RectTransform m_ViewportRt;
	private bool m_SlotsRootRuntimeCreated;
	private readonly Dictionary<int, int> m_TreeParentByChild = new Dictionary<int, int>();
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
		m_View.BtnBack.onClick.AddListener(Close);

		RegistAction(EActionId.OnGrowthPointsChange, OnGrowthPointsChange);

		m_DamageTextPool = new MonoBehaviourPool<UIDamageTextControl>(() =>
		{
			return this.Asset.OpenUI<UIDamageTextControl>(null);
		});

		LoadCsv();
		EnsureScrollRect();
		EnsureSlotsRoot();
		InitSlotsAndButtons();
		Refresh();
	}
	public void SetData()
	{
		Refresh();
		DataGrowth dataGrowth = DataSystem.Instance.GetDataGrowth();
		m_View.TextCoins.text = dataGrowth.Points.ToString();
	}
	private void EnsureScrollRect()
	{
		if (m_ScrollRect != null) return;
		m_ScrollRect = m_View.Scroll;
		m_ViewportRt = m_View.Viewport;
		// 允许横向/纵向拖动浏览天赋树
		m_ScrollRect.horizontal = true;
		m_ScrollRect.vertical = true;
		m_ScrollRect.movementType = ScrollRect.MovementType.Clamped;
		// 不再强制把 content 的 x 归零（否则横向拖动无效）
		m_ScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
	}
	private void OnScrollValueChanged(Vector2 _)
	{
		// 兼容旧回调：保留方法避免丢失引用，但不再锁死横向位置
	}
	private void EnsureSlotsRoot()
	{
		if (m_SlotsRoot != null) return;
		m_SlotsRoot = m_View.Content;
		m_SlotsRootRuntimeCreated = false;
		m_ScrollRect.content = m_SlotsRoot;
		m_SlotsRoot.anchorMin = new Vector2(0f, 1f);
		m_SlotsRoot.anchorMax = new Vector2(1f, 1f);
		m_SlotsRoot.pivot = new Vector2(0.5f, 1f);
		m_SlotsRoot.anchoredPosition = Vector2.zero;
		m_SlotsRoot.sizeDelta = new Vector2(0f, 0f);
	}
	private Dictionary<int, Vector2> BuildAutoLayoutPositions()
	{
		const int rootId = 0;
		const float baseHexSize = 110f;
		const float maxRadiusRatio = 0.44f;
		var posById = new Dictionary<int, Vector2>();
		if (!m_NodeById.ContainsKey(rootId))
		{
		}
		posById[rootId] = Vector2.zero;
		m_LayoutScale = 1f;
		static int HexDistance(Vector2Int a, Vector2Int b)
		{
			int dx = a.x - b.x;
			int dz = a.y - b.y;
			int dy = -dx - dz;
			return (Mathf.Abs(dx) + Mathf.Abs(dy) + Mathf.Abs(dz)) / 2;
		}
		static IEnumerable<Vector2Int> HexRing(Vector2Int center, int radius)
		{
			if (radius == 0)
			{
				yield return center;
				yield break;
			}
			Vector2Int[] dirs =
			{
				new Vector2Int(1, 0),
				new Vector2Int(1, -1),
				new Vector2Int(0, -1),
				new Vector2Int(-1, 0),
				new Vector2Int(-1, 1),
				new Vector2Int(0, 1),
			};
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
					}
					int pd = GetDepth(p);
					if (pd > maxParent) maxParent = pd;
				}
				depth = maxParent + 1;
			}
			visiting.Remove(id);
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
				d += maxReachDepth + 2;
			}
			finalDepth[id] = d;
			if (d > maxDepth) maxDepth = d;
		}
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
		float hexSize = baseHexSize;
		if (AutoFitToScreen)
		{
			var hostRt = transform as RectTransform;
			if (hostRt != null && hostRt.rect.width > 1f && hostRt.rect.height > 1f)
			{
				float maxAllowedR = Mathf.Min(hostRt.rect.width, hostRt.rect.height) * maxRadiusRatio;
				float maxUnit = Mathf.Max(1f, maxDepth * 1.75f);
				hexSize = Mathf.Min(baseHexSize, maxAllowedR / maxUnit);
			}
			m_LayoutScale = Mathf.Clamp01(hexSize / baseHexSize);
		}
		else
		{
			m_LayoutScale = 1f;
		}
		var origin = Vector2Int.zero;
		var axialById = new Dictionary<int, Vector2Int> { [rootId] = origin };
		var occupied = new HashSet<Vector2Int> { origin };
		float GetDesiredAngle(int id)
		{
			if (!m_NodeById.TryGetValue(id, out var n) || n == null || n.Depends == null || n.Depends.Count == 0)
			{
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
			ids.Sort((a, b) =>
			{
				int cmp = GetDesiredAngle(a).CompareTo(GetDesiredAngle(b));
				if (cmp != 0) return cmp;
				return a.CompareTo(b);
			});
			int remaining = ids.Count;
			int start = 0;
			int ringK = d;
			while (remaining > 0)
			{
				int cap = ringK == 0 ? 1 : 6 * ringK;
				int take = Mathf.Min(remaining, cap);
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
				float avg = 0f;
				for (int i = 0; i < take; i++) avg += GetDesiredAngle(ids[start + i]);
				avg /= Mathf.Max(1, take);
				int offset = Mathf.RoundToInt((avg / (Mathf.PI * 2f)) * ringPos.Count);
				offset = ((offset % ringPos.Count) + ringPos.Count) % ringPos.Count;
				for (int i = 0; i < take; i++)
				{
					int id = ids[start + i];
					if (id == rootId) continue;
					int idx = (i * ringPos.Count) / take;
					idx = (idx + offset) % ringPos.Count;
					int guard = 0;
					while (guard < ringPos.Count && occupied.Contains(ringPos[idx]))
					{
						idx = (idx + 1) % ringPos.Count;
						guard++;
					}
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
								if (HexDistance(origin, p) < d) continue;
								chosen = p;
								goto FOUND;
							}
							extra++;
						}
					FOUND:;
					}
					else
					{
						chosen = ringPos[idx];
					}
					occupied.Add(chosen);
					axialById[id] = chosen;
					Vector2 unit = AxialToUnitXY(chosen);
					posById[id] = new Vector2(unit.x * hexSize, -unit.y * hexSize);
				}
				start += take;
				remaining -= take;
				ringK++;
			}
		}
		posById[rootId] = Vector2.zero;
		return posById;
	}
	private Dictionary<int, Vector2> BuildGridLayoutPositions()
	{
		const int rootId = 0;
		var posById = new Dictionary<int, Vector2>();
		m_LayoutScale = 1f;
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
				if (cl == pl + 1) AddAdj(parent, child);
			}
		}
		var order = new Dictionary<int, List<int>>();
		foreach (var l in layerKeys)
		{
			var list = layers[l];
			list.Sort((a, b) => a.CompareTo(b));
			order[l] = new List<int>(list);
		}
		if (order.TryGetValue(0, out var rootLayer))
		{
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
			int cross = 0;
			for (int i = 0; i < edges.Count; i++)
			{
				for (int j = i + 1; j < edges.Count; j++)
				{
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
				if (b != a + 1) continue;
				cross += CountCrossingsBetween(a, b);
			}
			return cross;
		}
		const int maxIter = 12;
		int lastCross = CountAllCrossings();
		for (int iter = 0; iter < maxIter; iter++)
		{
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
					int ia = curIndex.TryGetValue(a, out var va) ? va : 0;
					int ib = curIndex.TryGetValue(b, out var vb) ? vb : 0;
					int cc = ia.CompareTo(ib);
					return cc != 0 ? cc : a.CompareTo(b);
				});
			}
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
		for (int li = 0; li < layerKeys.Count; li++)
		{
			int l = layerKeys[li];
			var list = order[l];
			if (list == null || list.Count == 0) continue;
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
		posById[rootId] = Vector2.zero;
		return posById;
	}
	private Vector2 GetViewportSize()
	{
		if (m_View.Viewport.rect.width > 1f && m_View.Viewport.rect.height > 1f)
		{
			return m_View.Viewport.rect.size;
		}
		return Vector2.zero;
	}
	private float GetTreeXStep()
	{
		// 与 slot/button 的实际宽度保持一致（slot 在 InitSlotsAndButtons 里用 SlotBaseW * s 设置）
		float s = Mathf.Clamp(m_LayoutScale, 0.6f, 1f);
		float slotW = SlotBaseW * s;
		return slotW * (1f + TreeXGapRatio);
	}
	private Dictionary<int, Vector2> BuildTreeLayoutPositions()
	{
		const int rootId = 0;
		var posById = new Dictionary<int, Vector2>();
		m_TreeParentByChild.Clear();
		m_LayoutScale = 1f;
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
		int ChoosePrimaryParent(int childId)
		{
			if (!m_NodeById.TryGetValue(childId, out var node) || node == null) return rootId;
			if (node.Depends == null || node.Depends.Count == 0) return rootId;
			int childLevel = GetBaseLevel(childId);
			int chosen = -1;
			int chosenLevel = int.MinValue;
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
		foreach (var kv in m_NodeById)
		{
			if (!xIndexById.ContainsKey(kv.Key))
			{
				AssignX(kv.Key);
			}
		}
		float rootX = xIndexById.TryGetValue(rootId, out var rx) ? rx : 0f;
		float xStep = GetTreeXStep();
		foreach (var kv in m_NodeById)
		{
			int id = kv.Key;
			float xi = xIndexById.TryGetValue(id, out var v) ? v : 0f;
			int level = levelById.TryGetValue(id, out var l) ? l : 0;
			float x = (xi - rootX) * xStep;
			float y = -(level * GridYSpacing + TreeTopPadding);
			posById[id] = new Vector2(x, y);
		}
		if (ClampTreeWidthToViewport)
		{
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
		}
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
		float maxAbsX = Mathf.Max(Mathf.Abs(minX), Mathf.Abs(maxX));
		float maxAbsY = Mathf.Max(Mathf.Abs(minY), Mathf.Abs(maxY));
		if (m_ScrollRect != null)
		{
			m_SlotsRoot.anchorMin = new Vector2(0f, 1f);
			m_SlotsRoot.anchorMax = new Vector2(1f, 1f);
			m_SlotsRoot.pivot = new Vector2(0.5f, 1f);
			m_SlotsRoot.anchoredPosition = Vector2.zero;
			m_SlotsRoot.sizeDelta = new Vector2(0f, 0f);
			m_ScrollRect.content = m_SlotsRoot;
			Vector2 vp = GetViewportSize();
			float minVisibleH = vp.y > 1f ? vp.y : 0f;
			float bottomMost = minY - slotH * 0.5f - TreeBottomPadding;
			float needH = Mathf.Abs(bottomMost);
			if (needH < minVisibleH) needH = minVisibleH;

			// 横向：让 content 宽度覆盖所有节点范围，从而启用左右拖动
			float minVisibleW = vp.x > 1f ? vp.x : 0f;
			float wantContentW = maxAbsX * 2f + slotW + LayoutPadding;
			if (wantContentW < minVisibleW) wantContentW = minVisibleW;
			// anchorMax.x=1 时，实际宽度=viewport宽 + sizeDelta.x，所以这里只补“额外宽度”
			float extraW = minVisibleW > 0f ? Mathf.Max(0f, wantContentW - minVisibleW) : wantContentW;

			m_SlotsRoot.sizeDelta = new Vector2(extraW, needH);
			return;
		}
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
			return;
		}
		string[] lines = textAsset.text.Split('\n');
		if (lines == null || lines.Length == 0) return;
		string headerLine = lines[0].Trim();
		if (string.IsNullOrWhiteSpace(headerLine))
		{
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
			return;
		}
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
			}
			m_NodeById[node.Id] = node;
		}
	}
	private void InitSlotsAndButtons()
	{
		m_SlotById.Clear();
		m_BtnById.Clear();
		EnsureSlotsRoot();
		if (m_SlotsRoot != null)
		{
			for (int i = m_SlotsRoot.childCount - 1; i >= 0; i--)
			{
				Destroy(m_SlotsRoot.GetChild(i).gameObject);
			}
		}
		var posById = UseTreeLayout
			? BuildTreeLayoutPositions()
			: (UseGridLayout ? BuildGridLayoutPositions() : BuildAutoLayoutPositions());
		UpdateSlotsRootSizeForPositions(posById);
		foreach (var kv in m_NodeById)
		{
			int id = kv.Key;
			var node = kv.Value;
			if (node == null) continue;
			var slotGo = new GameObject($"Slot_{id}", typeof(RectTransform));
			slotGo.transform.SetParent(m_SlotsRoot, false);
			var slot = slotGo.GetComponent<RectTransform>();
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
			// Text 不再使用
			m_SlotById[id] = slot;
			m_BtnById[id] = btnCtrl;
		}

		// 重要：让“靠近根节点/层级更浅”的节点绘制在更上层，避免子节点的线段盖到父节点上（看起来像线露出来）
		ReorderSlotsByLevel();
	}

	private void ReorderSlotsByLevel()
	{
		if (m_SlotsRoot == null || m_SlotById.Count == 0 || m_NodeById.Count == 0) return;
		var list = new List<GrowthNode>(m_NodeById.Values);
		int LevelKey(GrowthNode n)
		{
			if (n == null) return int.MaxValue;
			return n.Level >= 0 ? n.Level : int.MaxValue;
		}
		list.Sort((a, b) =>
		{
			int ka = LevelKey(a);
			int kb = LevelKey(b);
			// level 越深越靠后绘制(越靠下层)，所以这里先把深层放到前面，浅层(含root)放到后面
			int c = kb.CompareTo(ka); // desc
			if (c != 0) return c;
			int ia = a != null ? a.Id : int.MaxValue;
			int ib = b != null ? b.Id : int.MaxValue;
			return ia.CompareTo(ib);
		});

		for (int i = 0; i < list.Count; i++)
		{
			var node = list[i];
			if (node == null) continue;
			if (m_SlotById.TryGetValue(node.Id, out var slot) && slot != null)
			{
				slot.SetSiblingIndex(i);
			}
		}
	}
	private void EnsureLinesRoot()
	{
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
	}
	private Vector3 GetNodeLocalCenterInLinesRoot(int id)
	{
		return Vector3.zero;
	}
	private bool TryGetSlotLocalAABBInLinesRoot(int id, out Vector2 min, out Vector2 max, out Vector2 center)
	{
		min = Vector2.zero;
		max = Vector2.zero;
		center = Vector2.zero;
		return false;
	}
	private Vector2 GetRectEdgePointTowards(int id, Vector2 targetLocal, float inset)
	{
		return Vector2.zero;
	}
	private void UpdateLineGeometry(object line, float thickness)
	{
	}
	private void UpdateAllLineGeometry()
	{
	}
	private void RefreshLinesStyle()
	{
	}
	private bool IsNodeActive(DataGrowth data, GrowthNode node)
	{
		if (data == null || node == null) return false;
		if (data.IsUnlocked(node.Id)) return false;
		if (node.Depends == null || node.Depends.Count == 0) return true;
		for (int i = 0; i < node.Depends.Count; i++)
		{
			int depId = node.Depends[i];
			if (!data.IsUnlocked(depId)) return false;
		}
		return true;
	}
	private void Refresh()
	{
		DataGrowth data = DataSystem.Instance.GetDataGrowth();
		m_IsSoldById.Clear();
		m_IsActiveById.Clear();
		
		foreach (var kv in m_BtnById)
		{
			int id = kv.Key;
			var btn = kv.Value;
			if (btn == null) continue;
			
			if (!m_NodeById.TryGetValue(id, out var node) || node == null)
			{
				btn.SetState(UIBtnControl.EBtnState.Unknow);
				btn.SetInteractable(false);
				m_IsSoldById[id] = false;
				m_IsActiveById[id] = false;
				continue;
			}

			bool isUnlocked = data.IsUnlocked(node.Id);
			bool isActive = IsNodeActive(data, node);
			
			m_IsSoldById[id] = isUnlocked;
			m_IsActiveById[id] = isActive;

			// Text 不再使用
			
			if (id == 0) // Start 节点：也需要购买激活
			{
				if (isUnlocked)
				{
					btn.SetState(UIBtnControl.EBtnState.Start);
					btn.SetInteractable(false);
				}
				else
				{
					// 未激活时显示 lock，并允许点击购买
					btn.SetState(UIBtnControl.EBtnState.Lock);
					btn.SetInteractable(true);
				}
				btn.SetLine(false, false, 0, 0);
			}
			else
			{
				if (isUnlocked)
				{
					btn.SetState(UIBtnControl.EBtnState.Unlock);
					btn.SetInteractable(false); // Already bought
				}
				else if (isActive)
				{
					btn.SetState(UIBtnControl.EBtnState.Lock);
					btn.SetInteractable(true); // Can buy
				}
				else
				{
					btn.SetState(UIBtnControl.EBtnState.Unknow);
					btn.SetInteractable(false); // Locked
				}
				
				// Draw Line to Primary Parent
				if (m_TreeParentByChild.TryGetValue(id, out int parentId) && m_SlotById.TryGetValue(parentId, out var parentRt) && m_SlotById.TryGetValue(id, out var myRt))
				{
					Vector2 dir = parentRt.anchoredPosition - myRt.anchoredPosition;
					float len = dir.magnitude;
					float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
					
					// If node is unlocked -> Unlock Line. Else -> Lock Line.
					bool useUnlockLine = isUnlocked;
					
					btn.SetLine(true, useUnlockLine, angle, len);
				}
				else
				{
					btn.SetLine(false, false, 0, 0);
				}
			}
		}
	}
	private void OnGrowthBtnClick(int id)
	{
		if (!m_NodeById.TryGetValue(id, out var node) || node == null) return;
		DataGrowth data = DataSystem.Instance.GetDataGrowth();
		if (data.IsUnlocked(node.Id)) return;
		if (!IsNodeActive(data, node)) return;

		if (m_GrowthWindow == null)
		{
			m_GrowthWindow = Asset.OpenUI<UIGrowthWindowControl>();
		}
		else
		{
			m_GrowthWindow.gameObject.SetActive(true);
		}

		m_GrowthWindow.SetData(node.Id, node.Desc, node.Price, () =>
		{
			DataSystem.Instance.GetDataGrowth().Unlock(node.Id);
			DataSystem.Instance.SaveDataGrowth();
			// 立即应用解锁到卡池/遗物池，避免必须重启或重新进局
			DataSystem.Instance.ApplyGrowthUnlocks();
			Refresh();
		});
	}

	private void OnGrowthPointsChange(object[] paraArray)
	{
		int points = (int)paraArray[0];
		int delta = paraArray.Length > 1 && paraArray[1] is int ? (int)paraArray[1] : 0;
		m_View.TextCoins.text = points.ToString();
		if (delta != 0)
		{
			UIDamageTextControl damageTextControl = m_DamageTextPool.Get();
			if (delta > 0)
			{
				damageTextControl.SetCoinData(delta, Asset.UIRoot, Vector3.zero);
			}
			else
			{
				damageTextControl.SetData(-delta, Asset.UIRoot, Vector3.zero, true);
			}
			damageTextControl.transform.position = m_View.TextCoins.transform.position - new Vector3(1f, 1f, 0f);
		}
	}

	protected override void OnClose()
	{
		if (m_ScrollRect != null)
		{
			m_ScrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
			m_ScrollRect = null;
			m_ViewportRt = null;
		}
		if (m_SlotsRoot != null)
		{
			if (m_SlotsRootRuntimeCreated)
			{
				Destroy(m_SlotsRoot.gameObject);
			}
			m_SlotsRoot = null;
			m_SlotsRootRuntimeCreated = false;
		}
		if (m_DamageTextPool != null)
		{
			m_DamageTextPool.ReleaseAll();
		}
		if (m_GrowthWindow != null)
		{
			m_GrowthWindow.Close();
			m_GrowthWindow = null;
		}
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
