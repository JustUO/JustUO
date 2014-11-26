#region Header
//   Vorspire    _,-'/-'/  MultiExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Linq;

using Server.Items;
#endregion

namespace Server
{
	public static class MultiExtUtility
	{
		#region Components
		private static readonly Dictionary<int, MultiComponentList> _ComponentsCache =
			new Dictionary<int, MultiComponentList>();

		public static Dictionary<int, MultiComponentList> ComponentsCache { get { return _ComponentsCache; } }

		public static MultiComponentList GetComponents(this BaseMulti multi)
		{
			return GetComponents(multi, multi.ItemID);
		}

		public static MultiComponentList GetComponents(this BaseMulti multi, int multiID)
		{
			multiID &= 0x3FFF;

			if (multiID <= 0)
			{
				multiID = multi.ItemID;
			}

			return GetComponents(multiID);
		}

		public static MultiComponentList GetComponents(int multiID)
		{
			multiID &= 0x3FFF;

			MultiComponentList mcl;

			if (_ComponentsCache.TryGetValue(multiID, out mcl))
			{
				return mcl;
			}

			_ComponentsCache.Add(multiID, mcl = MultiData.GetComponents(multiID));

			if (multiID == 0x1388)
			{
				// That tree...
				mcl.Remove(3405, 17, -13, 15);
				mcl.Remove(3406, 18, -14, 15);
				mcl.Remove(3393, 18, -14, 17);
			}

			return mcl;
		}
		#endregion

		#region Wireframes
		private static readonly Dictionary<int, Wireframe> _WireframeCache = new Dictionary<int, Wireframe>();

		public static Dictionary<int, Wireframe> WireframeCache { get { return _WireframeCache; } }

		public static Wireframe GetWireframe(this BaseMulti multi, IPoint3D offset)
		{
			return GetWireframe(multi, multi.ItemID, offset);
		}

		public static Wireframe GetWireframe(this BaseMulti multi, IPoint3D offset, int hOffset)
		{
			return GetWireframe(multi, multi.ItemID, offset, hOffset);
		}

		public static Wireframe GetWireframe(this BaseMulti multi, IBlock3D offset)
		{
			return GetWireframe(multi, multi.ItemID, offset);
		}

		public static Wireframe GetWireframe(this BaseMulti multi, int multiID, IPoint3D offset)
		{
			return GetWireframe(multi, multiID, offset, 0);
		}

		public static Wireframe GetWireframe(this BaseMulti multi, int multiID, IBlock3D offset)
		{
			return GetWireframe(multi, multiID, offset, offset.H);
		}

		public static Wireframe GetWireframe(this BaseMulti multi, int multiID, IPoint3D offset, int hOffset)
		{
			return
				new Wireframe(
					GetWireframe(multi, multiID).Select(box => new Block3D(box.Clone3D(offset.X, offset.Y, offset.Z), box.H + hOffset)));
		}

		public static Wireframe GetWireframe(this BaseMulti multi)
		{
			return GetWireframe(multi, multi.ItemID);
		}

		public static Wireframe GetWireframe(this BaseMulti multi, int multiID)
		{
			multiID &= 0x3FFF;

			if (multiID <= 0)
			{
				multiID = multi.ItemID;
			}

			return GetWireframe(multiID);
		}

		public static Wireframe GetWireframe(int multiID)
		{
			multiID &= 0x3FFF;

			Wireframe frame;

			if (_WireframeCache.TryGetValue(multiID, out frame))
			{
				return frame;
			}

			frame = GetWireframe(GetComponents(multiID));

			_WireframeCache.Add(multiID, frame);

			return frame;
		}

		public static Wireframe GetWireframe(this MultiComponentList mcl, IPoint3D offset)
		{
			return new Wireframe(GetWireframe(mcl).Select(box => new Block3D(box.Clone3D(offset.X, offset.Y, offset.Z), box.H)));
		}

		public static Wireframe GetWireframe(this MultiComponentList mcl, IBlock3D offset)
		{
			return
				new Wireframe(
					GetWireframe(mcl).Select(box => new Block3D(box.Clone3D(offset.X, offset.Y, offset.Z), box.H + offset.H)));
		}

		public static Wireframe GetWireframe(this MultiComponentList mcl)
		{
			if (mcl == null)
			{
				return Wireframe.Empty;
			}

			var frame = new Block3D[mcl.List.Length];

			frame.SetAll(
				i =>
				new Block3D(
					mcl.List[i].m_OffsetX,
					mcl.List[i].m_OffsetY,
					mcl.List[i].m_OffsetZ,
					TileData.ItemTable[mcl.List[i].m_ItemID].CalcHeight + 5));

			return new Wireframe(frame);
		}
		#endregion
	}
}