#region Header
//   Vorspire    _,-'/-'/  RegionExt.cs
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

using VitaNex.Crypto;
using VitaNex.Items;
using VitaNex.Reflection;
#endregion

namespace Server
{
	public static class RegionExtUtility
	{
		public class RegionSerial : CryptoHashCode
		{
			public RegionSerial(Region r)
				: base(CryptoHashType.MD5, r.Map + r.GetType().FullName)
			{ }

			public RegionSerial(GenericReader reader)
				: base(reader)
			{ }
		}

		public static RegionSerial GetSerial(this Region r)
		{
			return r != null ? new RegionSerial(r) : null;
		}

		private static readonly Dictionary<RegionSerial, Dictionary<Point2D, BoundsPreviewTile>> _PreviewTiles =
			new Dictionary<RegionSerial, Dictionary<Point2D, BoundsPreviewTile>>();

		public static void DisplayPreview(this Region region, int hueBase = 11, int hueJump = 11)
		{
			var s = GetSerial(region);

			if (region == null || s == null)
			{
				return;
			}

			if (!_PreviewTiles.ContainsKey(s))
			{
				_PreviewTiles.Add(s, new Dictionary<Point2D, BoundsPreviewTile>());
			}
			else
			{
				ClearPreview(region);
			}

			//region.Area.For((index, r) => Console.WriteLine("Region Area #{0}: ({1})", index, r));

			region.Area.For(
				(index, r) =>
				{
					int spacing = Math.Max(1, Math.Min(10, (r.Width * r.Height) / 100));
					double decay = Math.Sqrt(r.Width * r.Height);

					r.ToRectangle2D().ForEach(
						p =>
						{
							if (p.X == r.Start.X || p.Y == r.Start.Y || p.X == r.End.X - 1 || p.Y == r.End.Y - 1 ||
								(p.X % spacing == 0 && p.Y % spacing == 0))
							{
								AddPreviewTile(region, hueBase, hueJump, index, p);
							}
						});

					Timer.DelayCall(
						TimeSpan.FromSeconds((decay < 60) ? 60 : (decay > 300) ? 300 : decay), () => ClearPreview(region, index));
				});
		}

		private static void AddPreviewTile(Region region, int hueBase, int hueJump, int index, Point2D p)
		{
			var s = GetSerial(region);

			if (region == null || s == null)
			{
				return;
			}

			BoundsPreviewTile tile;

			if (!_PreviewTiles[s].ContainsKey(p))
			{
				tile = new BoundsPreviewTile(region.Name, hueBase + (hueJump * index));
				_PreviewTiles[s].Add(p, tile);
			}
			else
			{
				tile = _PreviewTiles[s][p];
				tile.Hue = hueBase + (hueJump * index);
			}

			tile.MoveToWorld(p.GetWorldTop(region.Map), region.Map);
		}

		public static void ClearPreview(this Region region)
		{
			var s = GetSerial(region);

			if (region == null || s == null || !_PreviewTiles.ContainsKey(s))
			{
				return;
			}

			for (int index = 0; index < region.Area.Length; index++)
			{
				ClearPreview(region, index);
			}
		}

		public static void ClearPreview(this Region region, int index)
		{
			var s = GetSerial(region);

			if (region == null || s == null || !_PreviewTiles.ContainsKey(s) || index < 0 || index >= _PreviewTiles[s].Count)
			{
				return;
			}

			region.Area[index].ToRectangle2D().ForEach(
				p =>
				{
					if (!_PreviewTiles[s].ContainsKey(p))
					{
						return;
					}

					_PreviewTiles[s][p].Delete();
					_PreviewTiles[s].Remove(p);
				});
		}

		public static bool Contains(this Sector s, Point3D p)
		{
			return s.RegionRects.Contains(p);
		}

		public static bool Contains(this Region r, Point3D p)
		{
			return r.Area.Contains(p);
		}

		public static bool Contains(this RegionRect r, Point3D p)
		{
			return r.Rect.Contains(p);
		}

		public static TRegion Create<TRegion>(params object[] args) where TRegion : Region
		{
			return Create(typeof(TRegion), args) as TRegion;
		}

		public static Region Create(Type type, params object[] args)
		{
			return Activator.CreateInstance(type, args) as Region;
		}

		public static Region Clone(this Region region, params object[] args)
		{
			return Clone<Region>(region, args);
		}

		public static TRegion Clone<TRegion>(this Region region, params object[] args) where TRegion : Region
		{
			return CloneGeneric(region, args) as TRegion;
		}

		public static TRegion CloneGeneric<TRegion>(TRegion region, params object[] args) where TRegion : Region
		{
			if (region == null)
			{
				return null;
			}

			var fields = GetFields(region);

			fields.Remove("m_Name");
			fields.Remove("m_Map");
			fields.Remove("m_Parent");
			fields.Remove("m_Area");
			fields.Remove("m_Sectors");
			fields.Remove("m_ChildLevel");
			fields.Remove("m_Registered");

			#region Remove Fields that apply to global standards (if any)
			fields.Remove("_Name");
			fields.Remove("_Map");
			fields.Remove("_Parent");
			fields.Remove("_Area");
			fields.Remove("_Sectors");
			fields.Remove("_ChildLevel");
			fields.Remove("_Registered");
			#endregion

			region.Unregister();

			var reg = Create(region.GetType(), args) as TRegion;

			if (reg != null)
			{
				fields.Serialize(reg);
				reg.Register();
			}

			return reg;
		}

		public static FieldList<TRegion> GetFields<TRegion>(TRegion region) where TRegion : Region
		{
			return new FieldList<TRegion>(region, true);
		}

		public static bool IsPartOf<TRegion>(this Region region) where TRegion : Region
		{
			return region != null && region.IsPartOf(typeof(TRegion));
		}

		public static TRegion GetRegion<TRegion>(this Region region) where TRegion : Region
		{
			return region != null ? region.GetRegion(typeof(TRegion)) as TRegion : null;
		}

		public static TRegion GetRegion<TRegion>(this Mobile m) where TRegion : Region
		{
			return GetRegion<TRegion>(m.Region);
		}
	}
}