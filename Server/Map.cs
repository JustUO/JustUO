#region Header
// **************************************\
//     _  _   _   __  ___  _   _   ___   |
//    |# |#  |#  |## |### |#  |#  |###   |
//    |# |#  |# |#    |#  |#  |# |#  |#  |
//    |# |#  |#  |#   |#  |#  |# |#  |#  |
//   _|# |#__|#  _|#  |#  |#__|# |#__|#  |
//  |##   |##   |##   |#   |##    |###   |
//        [http://www.playuo.org]        |
// **************************************/
//  [2014] Map.cs
// ************************************/
#endregion

#define NewEnumerators

#region References
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Server.Items;
using Server.Network;
using Server.Targeting;
#endregion

namespace Server
{
	[Flags]
	public enum MapRules
	{
		None = 0x0000,
		Internal = 0x0001, // Internal map (used for dragging, commodity deeds, etc)
		FreeMovement = 0x0002, // Anyone can move over anyone else without taking stamina loss
		BeneficialRestrictions = 0x0004, // Disallow performing beneficial actions on criminals/murderers
		HarmfulRestrictions = 0x0008, // Disallow performing harmful actions on innocents
		TrammelRules = FreeMovement | BeneficialRestrictions | HarmfulRestrictions,
		FeluccaRules = None
	}

	public interface IPooledEnumerable : IEnumerable
	{
		void Free();
	}

	public interface IPooledEnumerable<out T> : IPooledEnumerable, IEnumerable<T>
	{ }

	public interface IPooledEnumerator<out T> : IEnumerator<T>
	{
		void Free();
	}

	[Parsable]
	//[CustomEnum( new string[]{ "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "TerMur", "Internal" } )]
	public class Map : IComparable, IComparable<Map>
	{
		public const int SectorSize = 16;
		public const int SectorShift = 4;
		public static int SectorActiveRange = 2;

		private static readonly Map[] _Maps = new Map[0x100];

		public static Map[] Maps { get { return _Maps; } }

		public static Map Felucca { get { return _Maps[0]; } }
		public static Map Trammel { get { return _Maps[1]; } }
		public static Map Ilshenar { get { return _Maps[2]; } }
		public static Map Malas { get { return _Maps[3]; } }
		public static Map Tokuno { get { return _Maps[4]; } }
		public static Map TerMur { get { return _Maps[5]; } }

		public static Map Internal { get { return _Maps[0x7F]; } }

		private static readonly List<Map> _AllMaps = new List<Map>();

		public static List<Map> AllMaps { get { return _AllMaps; } }

		private readonly int _MapID;
		private readonly int _MapIndex;
		private readonly int _FileIndex;

		private readonly int _Width;
		private readonly int _Height;
		private readonly int _SectorsWidth;
		private readonly int _SectorsHeight;
		private readonly Dictionary<string, Region> _Regions;
		private Region _DefaultRegion;

		public int Season { get; set; }

		private readonly Sector[][] _Sectors;
		private readonly Sector _InvalidSector;

		private TileMatrix _Tiles;

		private static string[] _MapNames;
		private static Map[] _MapValues;

		public static string[] GetMapNames()
		{
			CheckNamesAndValues();
			return _MapNames;
		}

		public static Map[] GetMapValues()
		{
			CheckNamesAndValues();
			return _MapValues;
		}

		public static Map Parse(string value)
		{
			CheckNamesAndValues();

			for (int i = 0; i < _MapNames.Length; ++i)
			{
				if (Insensitive.Equals(_MapNames[i], value))
				{
					return _MapValues[i];
				}
			}

			int index;

			if (int.TryParse(value, out index))
			{
				if (index >= 0 && index < _Maps.Length && _Maps[index] != null)
				{
					return _Maps[index];
				}
			}

			//throw new ArgumentException( "Invalid map name" );
			return null;
		}

		private static void CheckNamesAndValues()
		{
			if (_MapNames != null && _MapNames.Length == _AllMaps.Count)
			{
				return;
			}

			_MapNames = new string[_AllMaps.Count];
			_MapValues = new Map[_AllMaps.Count];

			for (int i = 0; i < _AllMaps.Count; ++i)
			{
				Map map = _AllMaps[i];

				_MapNames[i] = map.Name;
				_MapValues[i] = map;
			}
		}

		public override string ToString()
		{
			return Name;
		}

		public int GetAverageZ(int x, int y)
		{
			int z = 0, avg = 0, top = 0;

			GetAverageZ(x, y, ref z, ref avg, ref top);

			return avg;
		}

		public void GetAverageZ(int x, int y, ref int z, ref int avg, ref int top)
		{
			int zTop = Tiles.GetLandTile(x, y).Z;
			int zLeft = Tiles.GetLandTile(x, y + 1).Z;
			int zRight = Tiles.GetLandTile(x + 1, y).Z;
			int zBottom = Tiles.GetLandTile(x + 1, y + 1).Z;

			z = zTop;

			if (zLeft < z)
			{
				z = zLeft;
			}

			if (zRight < z)
			{
				z = zRight;
			}

			if (zBottom < z)
			{
				z = zBottom;
			}

			top = zTop;

			if (zLeft > top)
			{
				top = zLeft;
			}

			if (zRight > top)
			{
				top = zRight;
			}

			if (zBottom > top)
			{
				top = zBottom;
			}

			avg = Math.Abs(zTop - zBottom) > Math.Abs(zLeft - zRight) ? FloorAverage(zLeft, zRight) : FloorAverage(zTop, zBottom);
		}

		private static int FloorAverage(int a, int b)
		{
			int v = a + b;

			if (v < 0)
			{
				--v;
			}

			return (v / 2);
		}

		#region Get*InRange/Bounds
		public IPooledEnumerable<IEntity> GetObjectsInRange(Point3D p)
		{
			if (this == Internal)
			{
				return NullEnumerable<IEntity>.Instance;
			}

			return
				PooledEnumerable<IEntity>.Instantiate(
					EntityEnumerator.Instantiate(this, new Rectangle2D(p.m_X - 18, p.m_Y - 18, 37, 37)));
		}

		public IPooledEnumerable<IEntity> GetObjectsInRange(Point3D p, int range)
		{
			if (this == Internal)
			{
				return NullEnumerable<IEntity>.Instance;
			}

			return
				PooledEnumerable<IEntity>.Instantiate(
					EntityEnumerator.Instantiate(this, new Rectangle2D(p.m_X - range, p.m_Y - range, range * 2 + 1, range * 2 + 1)));
		}

		public IPooledEnumerable<IEntity> GetObjectsInBounds(Rectangle2D bounds)
		{
			if (this == Internal)
			{
				return NullEnumerable<IEntity>.Instance;
			}

			return PooledEnumerable<IEntity>.Instantiate(EntityEnumerator.Instantiate(this, bounds));
		}

		public IPooledEnumerable<NetState> GetClientsInRange(Point3D p)
		{
			if (this == Internal)
			{
				return NullEnumerable<NetState>.Instance;
			}

			return
				PooledEnumerable<NetState>.Instantiate(
					ClientEnumerator.Instantiate(this, new Rectangle2D(p.m_X - 18, p.m_Y - 18, 37, 37)));
		}

		public IPooledEnumerable<NetState> GetClientsInRange(Point3D p, int range)
		{
			if (this == Internal)
			{
				return NullEnumerable<NetState>.Instance;
			}

			return
				PooledEnumerable<NetState>.Instantiate(
					ClientEnumerator.Instantiate(this, new Rectangle2D(p.m_X - range, p.m_Y - range, range * 2 + 1, range * 2 + 1)));
		}

		public IPooledEnumerable<NetState> GetClientsInBounds(Rectangle2D bounds)
		{
			if (this == Internal)
			{
				return NullEnumerable<NetState>.Instance;
			}

			return PooledEnumerable<NetState>.Instantiate(ClientEnumerator.Instantiate(this, bounds));
		}

		public IPooledEnumerable<Item> GetItemsInRange(Point3D p)
		{
			if (this == Internal)
			{
				return NullEnumerable<Item>.Instance;
			}

			return
				PooledEnumerable<Item>.Instantiate(
					ItemEnumerator.Instantiate(this, new Rectangle2D(p.m_X - 18, p.m_Y - 18, 37, 37)));
		}

		public IPooledEnumerable<Item> GetItemsInRange(Point3D p, int range)
		{
			if (this == Internal)
			{
				return NullEnumerable<Item>.Instance;
			}

			return
				PooledEnumerable<Item>.Instantiate(
					ItemEnumerator.Instantiate(this, new Rectangle2D(p.m_X - range, p.m_Y - range, range * 2 + 1, range * 2 + 1)));
		}

		public IPooledEnumerable<Item> GetItemsInBounds(Rectangle2D bounds)
		{
			if (this == Internal)
			{
				return NullEnumerable<Item>.Instance;
			}

			return PooledEnumerable<Item>.Instantiate(ItemEnumerator.Instantiate(this, bounds));
		}

		public IPooledEnumerable<Mobile> GetMobilesInRange(Point3D p)
		{
			if (this == Internal)
			{
				return NullEnumerable<Mobile>.Instance;
			}

			return
				PooledEnumerable<Mobile>.Instantiate(
					MobileEnumerator.Instantiate(this, new Rectangle2D(p.m_X - 18, p.m_Y - 18, 37, 37)));
		}

		public IPooledEnumerable<Mobile> GetMobilesInRange(Point3D p, int range)
		{
			if (this == Internal)
			{
				return NullEnumerable<Mobile>.Instance;
			}

			return
				PooledEnumerable<Mobile>.Instantiate(
					MobileEnumerator.Instantiate(this, new Rectangle2D(p.m_X - range, p.m_Y - range, range * 2 + 1, range * 2 + 1)));
		}

		public IPooledEnumerable<Mobile> GetMobilesInBounds(Rectangle2D bounds)
		{
			if (this == Internal)
			{
				return NullEnumerable<Mobile>.Instance;
			}

			return PooledEnumerable<Mobile>.Instantiate(MobileEnumerator.Instantiate(this, bounds));
		}
		#endregion

		public IPooledEnumerable<StaticTile[]> GetMultiTilesAt(int x, int y)
		{
			if (this == Internal)
			{
				return NullEnumerable<StaticTile[]>.Instance;
			}

			Sector sector = GetSector(x, y);

			if (sector.Multis.Count == 0)
			{
				return NullEnumerable<StaticTile[]>.Instance;
			}

			return PooledEnumerable<StaticTile[]>.Instantiate(MultiTileEnumerator.Instantiate(sector, new Point2D(x, y)));
		}

		#region CanFit
		public bool CanFit(Point3D p, int height, bool checkBlocksFit)
		{
			return CanFit(p.m_X, p.m_Y, p.m_Z, height, checkBlocksFit, true, true);
		}

		public bool CanFit(Point3D p, int height, bool checkBlocksFit, bool checkMobiles)
		{
			return CanFit(p.m_X, p.m_Y, p.m_Z, height, checkBlocksFit, checkMobiles, true);
		}

		public bool CanFit(Point2D p, int z, int height, bool checkBlocksFit)
		{
			return CanFit(p.m_X, p.m_Y, z, height, checkBlocksFit, true, true);
		}

		public bool CanFit(Point3D p, int height)
		{
			return CanFit(p.m_X, p.m_Y, p.m_Z, height, false, true, true);
		}

		public bool CanFit(Point2D p, int z, int height)
		{
			return CanFit(p.m_X, p.m_Y, z, height, false, true, true);
		}

		public bool CanFit(int x, int y, int z, int height)
		{
			return CanFit(x, y, z, height, false, true, true);
		}

		public bool CanFit(int x, int y, int z, int height, bool checksBlocksFit)
		{
			return CanFit(x, y, z, height, checksBlocksFit, true, true);
		}

		public bool CanFit(int x, int y, int z, int height, bool checkBlocksFit, bool checkMobiles)
		{
			return CanFit(x, y, z, height, checkBlocksFit, checkMobiles, true);
		}

		public bool CanFit(int x, int y, int z, int height, bool checkBlocksFit, bool checkMobiles, bool requireSurface)
		{
			if (this == Internal)
			{
				return false;
			}

			if (x < 0 || y < 0 || x >= _Width || y >= _Height)
			{
				return false;
			}

			bool hasSurface = false;

			LandTile lt = Tiles.GetLandTile(x, y);
			int lowZ = 0, avgZ = 0, topZ = 0;

			GetAverageZ(x, y, ref lowZ, ref avgZ, ref topZ);
			TileFlag landFlags = TileData.LandTable[lt.ID & TileData.MaxLandValue].Flags;

			if ((landFlags & TileFlag.Impassable) != 0 && avgZ > z && (z + height) > lowZ)
			{
				return false;
			}

			if ((landFlags & TileFlag.Impassable) == 0 && z == avgZ && !lt.Ignored)
			{
				hasSurface = true;
			}

			StaticTile[] staticTiles = Tiles.GetStaticTiles(x, y, true);

			bool surface, impassable;

			foreach (StaticTile t in staticTiles)
			{
				ItemData id = TileData.ItemTable[t.ID & TileData.MaxItemValue];
				surface = id.Surface;
				impassable = id.Impassable;

				if ((surface || impassable) && (t.Z + id.CalcHeight) > z && (z + height) > t.Z)
				{
					return false;
				}

				if (surface && !impassable && z == (t.Z + id.CalcHeight))
				{
					hasSurface = true;
				}
			}

			Sector sector = GetSector(x, y);
			List<Item> items = sector.Items;
			List<Mobile> mobs = sector.Mobiles;

			foreach (Item item in items)
			{
				if (item is BaseMulti || item.ItemID > TileData.MaxItemValue || !item.AtWorldPoint(x, y))
				{
					continue;
				}

				ItemData id = item.ItemData;
				surface = id.Surface;
				impassable = id.Impassable;

				if ((surface || impassable || (checkBlocksFit && item.BlocksFit)) && (item.Z + id.CalcHeight) > z &&
					(z + height) > item.Z)
				{
					return false;
				}

				if (surface && !impassable && !item.Movable && z == (item.Z + id.CalcHeight))
				{
					hasSurface = true;
				}
			}

			if (checkMobiles)
			{
				if (
					mobs.Any(
						m =>
						m.Location.m_X == x && m.Location.m_Y == y && (m.AccessLevel < AccessLevel.Counselor || !m.Hidden) &&
						((m.Z + 16) > z && (z + height) > m.Z)))
				{
					return false;
				}
			}

			return !requireSurface || hasSurface;
		}
		#endregion

		#region CanSpawnMobile
		public bool CanSpawnMobile(Point3D p)
		{
			return CanSpawnMobile(p.m_X, p.m_Y, p.m_Z);
		}

		public bool CanSpawnMobile(Point2D p, int z)
		{
			return CanSpawnMobile(p.m_X, p.m_Y, z);
		}

		public bool CanSpawnMobile(int x, int y, int z)
		{
			return Region.Find(new Point3D(x, y, z), this).AllowSpawn() && CanFit(x, y, z, 16);
		}
		#endregion

		public void FixColumn(int x, int y)
		{
			LandTile landTile = Tiles.GetLandTile(x, y);

			int landZ = 0, landAvg = 0, landTop = 0;
			GetAverageZ(x, y, ref landZ, ref landAvg, ref landTop);

			StaticTile[] tiles = Tiles.GetStaticTiles(x, y, true);

			IPooledEnumerable<Item> eable = GetItemsInRange(new Point3D(x, y, 0), 0);

			List<Item> items =
				eable.Where(item => !(item is BaseMulti) && item.ItemID <= TileData.MaxItemValue)
					 .OrderBy(i => i.Z)
					 .Take(100)
					 .ToList();

			eable.Free();

			for (int i = 0; i < items.Count; ++i)
			{
				Item toFix = items[i];

				if (!toFix.Movable)
				{
					continue;
				}

				int z = int.MinValue;
				int currentZ = toFix.Z;

				if (!landTile.Ignored && landAvg <= currentZ)
				{
					z = landAvg;
				}

				foreach (StaticTile tile in tiles)
				{
					ItemData id = TileData.ItemTable[tile.ID & TileData.MaxItemValue];

					int checkZ = tile.Z;
					int checkTop = checkZ + id.CalcHeight;

					if (checkTop == checkZ && !id.Surface)
					{
						++checkTop;
					}

					if (checkTop > z && checkTop <= currentZ)
					{
						z = checkTop;
					}
				}

				for (int j = 0; j < items.Count; ++j)
				{
					if (j == i)
					{
						continue;
					}

					Item item = items[j];
					ItemData id = item.ItemData;

					int checkZ = item.Z;
					int checkTop = checkZ + id.CalcHeight;

					if (checkTop == checkZ && !id.Surface)
					{
						++checkTop;
					}

					if (checkTop > z && checkTop <= currentZ)
					{
						z = checkTop;
					}
				}

				if (z != int.MinValue)
				{
					toFix.Location = new Point3D(toFix.X, toFix.Y, z);
				}
			}

			items.Clear();
			items.TrimExcess();
		}

		/* This could be probably be re-implemented if necessary (perhaps via an ITile interface?).
		public List<Tile> GetTilesAt( Point2D p, bool items, bool land, bool statics )
		{
			List<Tile> list = new List<Tile>();

			if ( this == Map.Internal )
				return list;

			if ( land )
				list.Add( Tiles.GetLandTile( p._X, p._Y ) );

			if ( statics )
				list.AddRange( Tiles.GetStaticTiles( p._X, p._Y, true ) );

			if ( items )
			{
				Sector sector = GetSector( p );

				foreach ( Item item in sector.Items )
					if ( item.AtWorldPoint( p._X, p._Y ) )
						list.Add( new StaticTile( (ushort)item.ItemID, (sbyte) item.Z ) );
			}

			return list;
		}
		*/

		/// <summary>
		///     Gets the highest surface that is lower than <paramref name="p" />.
		/// </summary>
		/// <param name="p">The reference point.</param>
		/// <returns>
		///     A surface
		///     <typeparamref>
		///         <name>Tile</name>
		///     </typeparamref>
		///     or
		///     <typeparamref>
		///         <name>Item</name>
		///     </typeparamref>
		///     .
		/// </returns>
		public object GetTopSurface(Point3D p)
		{
			if (this == Internal)
			{
				return null;
			}

			object surface = null;
			int surfaceZ = int.MinValue;

			LandTile lt = Tiles.GetLandTile(p.X, p.Y);

			if (!lt.Ignored)
			{
				int avgZ = GetAverageZ(p.X, p.Y);

				if (avgZ <= p.Z)
				{
					surface = lt;
					surfaceZ = avgZ;

					if (surfaceZ == p.Z)
					{
						return surface;
					}
				}
			}

			StaticTile[] staticTiles = Tiles.GetStaticTiles(p.X, p.Y, true);

			foreach (StaticTile tile in staticTiles)
			{
				ItemData id = TileData.ItemTable[tile.ID & TileData.MaxItemValue];

				if (!id.Surface && (id.Flags & TileFlag.Wet) == 0)
				{
					continue;
				}

				int tileZ = tile.Z + id.CalcHeight;

				if (tileZ <= surfaceZ || tileZ > p.Z)
				{
					continue;
				}

				surface = tile;
				surfaceZ = tileZ;

				if (surfaceZ == p.Z)
				{
					return surface;
				}
			}

			Sector sector = GetSector(p.X, p.Y);

			foreach (Item item in sector.Items)
			{
				if (item is BaseMulti || item.ItemID > TileData.MaxItemValue || !item.AtWorldPoint(p.X, p.Y) || item.Movable)
				{
					continue;
				}

				ItemData id = item.ItemData;

				if (!id.Surface && (id.Flags & TileFlag.Wet) == 0)
				{
					continue;
				}

				int itemZ = item.Z + id.CalcHeight;

				if (itemZ <= surfaceZ || itemZ > p.Z)
				{
					continue;
				}

				surface = item;
				surfaceZ = itemZ;

				if (surfaceZ == p.Z)
				{
					return surface;
				}
			}

			return surface;
		}

		public void Bound(int x, int y, out int newX, out int newY)
		{
			if (x < 0)
			{
				newX = 0;
			}
			else if (x >= _Width)
			{
				newX = _Width - 1;
			}
			else
			{
				newX = x;
			}

			if (y < 0)
			{
				newY = 0;
			}
			else if (y >= _Height)
			{
				newY = _Height - 1;
			}
			else
			{
				newY = y;
			}
		}

		public Point2D Bound(Point2D p)
		{
			int x = p.m_X, y = p.m_Y;

			if (x < 0)
			{
				x = 0;
			}
			else if (x >= _Width)
			{
				x = _Width - 1;
			}

			if (y < 0)
			{
				y = 0;
			}
			else if (y >= _Height)
			{
				y = _Height - 1;
			}

			return new Point2D(x, y);
		}

		public Map(int mapID, int mapIndex, int fileIndex, int width, int height, int season, string name, MapRules rules)
		{
			_MapID = mapID;
			_MapIndex = mapIndex;
			_FileIndex = fileIndex;
			_Width = width;
			_Height = height;
			Season = season;
			Name = name;
			Rules = rules;
			_Regions = new Dictionary<string, Region>(StringComparer.OrdinalIgnoreCase);
			_InvalidSector = new Sector(0, 0, this);
			_SectorsWidth = width >> SectorShift;
			_SectorsHeight = height >> SectorShift;
			_Sectors = new Sector[_SectorsWidth][];
		}

		#region GetSector
		public Sector GetSector(Point3D p)
		{
			return InternalGetSector(p.m_X >> SectorShift, p.m_Y >> SectorShift);
		}

		public Sector GetSector(Point2D p)
		{
			return InternalGetSector(p.m_X >> SectorShift, p.m_Y >> SectorShift);
		}

		public Sector GetSector(IPoint2D p)
		{
			return InternalGetSector(p.X >> SectorShift, p.Y >> SectorShift);
		}

		public Sector GetSector(int x, int y)
		{
			return InternalGetSector(x >> SectorShift, y >> SectorShift);
		}

		public Sector GetRealSector(int x, int y)
		{
			return InternalGetSector(x, y);
		}

		private Sector InternalGetSector(int x, int y)
		{
			if (x >= 0 && x < _SectorsWidth && y >= 0 && y < _SectorsHeight)
			{
				Sector[] xSectors = _Sectors[x];

				if (xSectors == null)
				{
					_Sectors[x] = xSectors = new Sector[_SectorsHeight];
				}

				Sector sec = xSectors[y];

				if (sec == null)
				{
					xSectors[y] = sec = new Sector(x, y, this);
				}

				return sec;
			}

			return _InvalidSector;
		}
		#endregion

		public void ActivateSectors(int cx, int cy)
		{
			for (int x = cx - SectorActiveRange; x <= cx + SectorActiveRange; ++x)
			{
				for (int y = cy - SectorActiveRange; y <= cy + SectorActiveRange; ++y)
				{
					Sector sect = GetRealSector(x, y);

					if (sect != _InvalidSector)
					{
						sect.Activate();
					}
				}
			}
		}

		public void DeactivateSectors(int cx, int cy)
		{
			for (int x = cx - SectorActiveRange; x <= cx + SectorActiveRange; ++x)
			{
				for (int y = cy - SectorActiveRange; y <= cy + SectorActiveRange; ++y)
				{
					Sector sect = GetRealSector(x, y);

					if (sect != _InvalidSector && !PlayersInRange(sect, SectorActiveRange))
					{
						sect.Deactivate();
					}
				}
			}
		}

		private bool PlayersInRange(Sector sect, int range)
		{
			for (int x = sect.X - range; x <= sect.X + range; ++x)
			{
				for (int y = sect.Y - range; y <= sect.Y + range; ++y)
				{
					Sector check = GetRealSector(x, y);

					if (check != _InvalidSector && check.Players.Count > 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		public virtual void OnClientChange(NetState oldState, NetState newState, Mobile m)
		{
			if (this == Internal)
			{
				return;
			}

			GetSector(m).OnClientChange(oldState, newState);
		}

		public virtual void OnEnter(Mobile m)
		{
			if (this != Internal)
			{
				GetSector(m).OnEnter(m);
			}
		}

		public virtual void OnEnter(Item item)
		{
			if (this == Internal)
			{
				return;
			}

			GetSector(item).OnEnter(item);

			if (!(item is BaseMulti))
			{
				return;
			}

			var m = (BaseMulti)item;
			MultiComponentList mcl = m.Components;

			Sector start = GetMultiMinSector(item.Location, mcl);
			Sector end = GetMultiMaxSector(item.Location, mcl);

			AddMulti(m, start, end);
		}

		public virtual void OnLeave(Mobile m)
		{
			if (this != Internal)
			{
				GetSector(m).OnLeave(m);
			}
		}

		public virtual void OnLeave(Item item)
		{
			if (this == Internal)
			{
				return;
			}

			GetSector(item).OnLeave(item);

			if (!(item is BaseMulti))
			{
				return;
			}

			var m = (BaseMulti)item;
			MultiComponentList mcl = m.Components;

			Sector start = GetMultiMinSector(item.Location, mcl);
			Sector end = GetMultiMaxSector(item.Location, mcl);

			RemoveMulti(m, start, end);
		}

		public void RemoveMulti(BaseMulti m, Sector start, Sector end)
		{
			if (this == Internal)
			{
				return;
			}

			for (int x = start.X; x <= end.X; ++x)
			{
				for (int y = start.Y; y <= end.Y; ++y)
				{
					InternalGetSector(x, y).OnMultiLeave(m);
				}
			}
		}

		public void AddMulti(BaseMulti m, Sector start, Sector end)
		{
			if (this == Internal)
			{
				return;
			}

			for (int x = start.X; x <= end.X; ++x)
			{
				for (int y = start.Y; y <= end.Y; ++y)
				{
					InternalGetSector(x, y).OnMultiEnter(m);
				}
			}
		}

		public Sector GetMultiMinSector(Point3D loc, MultiComponentList mcl)
		{
			return GetSector(Bound(new Point2D(loc.m_X + mcl.Min.m_X, loc.m_Y + mcl.Min.m_Y)));
		}

		public Sector GetMultiMaxSector(Point3D loc, MultiComponentList mcl)
		{
			return GetSector(Bound(new Point2D(loc.m_X + mcl.Max.m_X, loc.m_Y + mcl.Max.m_Y)));
		}

		public virtual void OnMove(Point3D oldLocation, Mobile m)
		{
			if (this == Internal)
			{
				return;
			}

			Sector oldSector = GetSector(oldLocation);
			Sector newSector = GetSector(m.Location);

			if (oldSector == newSector)
			{
				return;
			}

			oldSector.OnLeave(m);
			newSector.OnEnter(m);
		}

		public virtual void OnMove(Point3D oldLocation, Item item)
		{
			if (this == Internal)
			{
				return;
			}

			Sector oldSector = GetSector(oldLocation);
			Sector newSector = GetSector(item.Location);

			if (oldSector != newSector)
			{
				oldSector.OnLeave(item);
				newSector.OnEnter(item);
			}

			if (!(item is BaseMulti))
			{
				return;
			}

			var m = (BaseMulti)item;
			MultiComponentList mcl = m.Components;

			Sector start = GetMultiMinSector(item.Location, mcl);
			Sector end = GetMultiMaxSector(item.Location, mcl);

			Sector oldStart = GetMultiMinSector(oldLocation, mcl);
			Sector oldEnd = GetMultiMaxSector(oldLocation, mcl);

			if (oldStart != start || oldEnd != end)
			{
				RemoveMulti(m, oldStart, oldEnd);
				AddMulti(m, start, end);
			}
		}

		private readonly object tileLock = new object();

		public TileMatrix Tiles
		{
			get
			{
				if (_Tiles == null)
				{
					lock (tileLock)
					{
						return _Tiles ?? (_Tiles = new TileMatrix(this, _FileIndex, _MapID, _Width, _Height));
					}
				}

				return _Tiles;
			}
		}

		/// <summary>
		///     The ID of the Map (file); Felucca = 0, Trammel = 1, Ilshenar = 2, Malas = 3, Tokuno = 4, TerMur = 5
		/// </summary>
		public int MapID { get { return _MapID; } }

		public int MapIndex { get { return _MapIndex; } }

		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }

		public Dictionary<string, Region> Regions { get { return _Regions; } }

		public void RegisterRegion(Region reg)
		{
			string regName = reg.Name;

			if (regName == null)
			{
				return;
			}

			if (_Regions.ContainsKey(regName))
			{
				Console.WriteLine("Warning: Duplicate region name '{0}' for map '{1}'", regName, Name);
			}
			else
			{
				_Regions[regName] = reg;
			}
		}

		public void UnregisterRegion(Region reg)
		{
			string regName = reg.Name;

			if (regName != null)
			{
				_Regions.Remove(regName);
			}
		}

		public Region DefaultRegion
		{
			//
			get { return _DefaultRegion ?? (_DefaultRegion = new Region(null, this, 0, new Rectangle3D[0])); }
			set { _DefaultRegion = value; }
		}

		public MapRules Rules { get; set; }
		public string Name { get; set; }

		public Sector InvalidSector { get { return _InvalidSector; } }

		#region Enumerables
		public class NullEnumerable<T> : IPooledEnumerable<T>
		{
			private readonly InternalEnumerator<T> m_Enumerator;

			public static readonly NullEnumerable<T> Instance = new NullEnumerable<T>();

			private NullEnumerable()
			{
				m_Enumerator = new InternalEnumerator<T>();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return m_Enumerator;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return m_Enumerator;
			}

			public void Free()
			{ }

			private class InternalEnumerator<K> : IEnumerator<K>
			{
				public void Reset()
				{ }

				object IEnumerator.Current { get { return null; } }
				public K Current { get { return default(K); } }

				public bool MoveNext()
				{
					return false;
				}

				void IDisposable.Dispose()
				{ }
			}
		}

		private class PooledEnumerable<T> : IPooledEnumerable<T>, IDisposable
		{
			private IPooledEnumerator<T> m_Enumerator;

			private static readonly Queue<PooledEnumerable<T>> m_InstancePool = new Queue<PooledEnumerable<T>>();

			public static PooledEnumerable<T> Instantiate(IPooledEnumerator<T> etor)
			{
				PooledEnumerable<T> e = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count > 0)
					{
						e = m_InstancePool.Dequeue();
						e.m_Enumerator = etor;
					}
				}

				return e ?? new PooledEnumerable<T>(etor);
			}

			private PooledEnumerable(IPooledEnumerator<T> etor)
			{
				m_Enumerator = etor;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				if (m_Enumerator == null)
				{
					throw new ObjectDisposedException("PooledEnumerable", "GetEnumerator() called after Free()");
				}

				return m_Enumerator;
			}

			public IEnumerator<T> GetEnumerator()
			{
				if (m_Enumerator == null)
				{
					throw new ObjectDisposedException("PooledEnumerable", "GetEnumerator() called after Free()");
				}

				return m_Enumerator;
			}

			public void Free()
			{
				if (m_Enumerator != null)
				{
					m_Enumerator.Free();
					m_Enumerator = null;
				}

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count < 200) // Arbitrary
					{
						m_InstancePool.Enqueue(this);
					}
				}
			}

			public void Dispose()
			{
				// Don't return disposed objects to the instance pool
				//Free();

				if (m_Enumerator == null)
				{
					return;
				}

				m_Enumerator.Free();
				m_Enumerator = null;
			}
		}
		#endregion

		#region Enumerators
		private class ClientEnumerator : IPooledEnumerator<NetState>
		{
			private Map m_Map;
			private Rectangle2D m_Bounds;

			private int m_xSector, m_ySector;
			private int m_xSectorStart, m_ySectorStart;
			private int m_xSectorEnd, m_ySectorEnd;
			private List<NetState> m_CurrentList;
			private int m_CurrentIndex;

			private static readonly Queue<ClientEnumerator> m_InstancePool = new Queue<ClientEnumerator>();

			public static ClientEnumerator Instantiate(Map map, Rectangle2D bounds)
			{
				ClientEnumerator e = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count > 0)
					{
						e = m_InstancePool.Dequeue();

						e.m_Map = map;
						e.m_Bounds = bounds;
					}
				}

				if (e == null)
				{
					e = new ClientEnumerator(map, bounds);
				}

				e.Reset();

				return e;
			}

			public void Free()
			{
				if (m_Map == null)
				{
					return;
				}

				m_Map = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count < 200) // Arbitrary
					{
						m_InstancePool.Enqueue(this);
					}
				}
			}

			private ClientEnumerator(Map map, Rectangle2D bounds)
			{
				m_Map = map;
				m_Bounds = bounds;
			}

			public NetState Current { get { return m_CurrentList[m_CurrentIndex]; } }

			object IEnumerator.Current { get { return m_CurrentList[m_CurrentIndex]; } }

			void IDisposable.Dispose()
			{ }

#if NewEnumerators
			private bool NextList()
			{
				++m_ySector;

				if (m_ySector > m_ySectorEnd)
				{
					m_ySector = m_ySectorStart;
					++m_xSector;

					if (m_xSector > m_xSectorEnd)
					{
						m_CurrentIndex = -1;
						return false;
					}
				}

				m_CurrentIndex = -1;
				m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Clients;
				return true;
			}
#endif

			public bool MoveNext()
			{
#if NewEnumerators
				NetState ns;
				Mobile m;

				while (++m_CurrentIndex <= m_CurrentList.Count)
				{
					if (m_CurrentIndex == m_CurrentList.Count)
					{
						if (!NextList())
						{
							return false;
						}

						continue;
					}

					ns = Current;

					if (ns == null)
					{
						continue;
					}

					m = ns.Mobile;

					if (m != null && !m.Deleted && m_Bounds.Contains(m.Location))
					{
						return true;
					}
				}

				return false;
#else
				while (true)
				{
					++m_CurrentIndex;

					if (m_CurrentIndex == m_CurrentList.Count)
					{
						++m_ySector;

						if (m_ySector > m_ySectorEnd)
						{
							m_ySector = m_ySectorStart;
							++m_xSector;

							if (m_xSector > m_xSectorEnd)
							{
								m_CurrentIndex = -1;
								return false;
							}
						}

						m_CurrentIndex = -1;
						m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Clients;
					}
					else
					{
						Mobile m = m_CurrentList[m_CurrentIndex].Mobile;

						if (m != null && !m.Deleted && m_Bounds.Contains(m.Location))
						{
							return true;
						}
					}
				}
#endif
			}

			public void Reset()
			{
				m_Map.Bound(m_Bounds.Start.m_X, m_Bounds.Start.m_Y, out m_xSectorStart, out m_ySectorStart);
				m_Map.Bound(m_Bounds.End.m_X - 1, m_Bounds.End.m_Y - 1, out m_xSectorEnd, out m_ySectorEnd);

				m_xSector = m_xSectorStart >>= SectorShift;
				m_ySector = m_ySectorStart >>= SectorShift;

				m_xSectorEnd >>= SectorShift;
				m_ySectorEnd >>= SectorShift;

				m_CurrentIndex = -1;
				m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Clients;
			}
		}

		private class EntityEnumerator : IPooledEnumerator<IEntity>
		{
			private Map m_Map;
			private Rectangle2D m_Bounds;

			private int m_xSector, m_ySector;
			private int m_xSectorStart, m_ySectorStart;
			private int m_xSectorEnd, m_ySectorEnd;
			private int m_Stage;
			private IList m_CurrentList;
			private int m_CurrentIndex;

			private static readonly Queue<EntityEnumerator> m_InstancePool = new Queue<EntityEnumerator>();

			public static EntityEnumerator Instantiate(Map map, Rectangle2D bounds)
			{
				EntityEnumerator e = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count > 0)
					{
						e = m_InstancePool.Dequeue();

						e.m_Map = map;
						e.m_Bounds = bounds;
					}
				}

				if (e == null)
				{
					e = new EntityEnumerator(map, bounds);
				}

				e.Reset();

				return e;
			}

			public void Free()
			{
				if (m_Map == null)
				{
					return;
				}

				m_Map = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count < 200) // Arbitrary
					{
						m_InstancePool.Enqueue(this);
					}
				}
			}

			private EntityEnumerator(Map map, Rectangle2D bounds)
			{
				m_Map = map;
				m_Bounds = bounds;
			}

			public IEntity Current { get { return (IEntity)m_CurrentList[m_CurrentIndex]; } }

			object IEnumerator.Current { get { return m_CurrentList[m_CurrentIndex]; } }

			void IDisposable.Dispose()
			{ }

#if NewEnumerators
			private bool NextList()
			{
				++m_ySector;

				if (m_ySector > m_ySectorEnd)
				{
					m_ySector = m_ySectorStart;
					++m_xSector;

					if (m_xSector > m_xSectorEnd)
					{
						if (m_Stage > 0)
						{
							m_CurrentIndex = -1;
							return false;
						}

						++m_Stage;
						m_xSector = m_xSectorStart >>= SectorShift;
						m_ySector = m_ySectorStart >>= SectorShift;
					}
				}

				m_CurrentIndex = -1;

				if (m_Stage == 0)
				{
					m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Items;
				}
				else
				{
					m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Mobiles;
				}

				return true;
			}
#endif

			public bool MoveNext()
			{
#if NewEnumerators
				IEntity e;
				Item item;
				Mobile m;

				while (++m_CurrentIndex <= m_CurrentList.Count)
				{
					if (m_CurrentIndex == m_CurrentList.Count)
					{
						if (!NextList())
						{
							return false;
						}

						continue;
					}

					e = Current;

					if (e == null)
					{
						continue;
					}

					if (e is Item)
					{
						item = (Item)e;

						if (!item.Deleted && item.Parent == null && m_Bounds.Contains(e.Location))
						{
							return true;
						}
					}
					else if (e is Mobile)
					{
						m = (Mobile)e;

						if (!m.Deleted && m_Bounds.Contains(e.Location))
						{
							return true;
						}
					}
				}

				return false;
#else
				while (true)
				{
					++m_CurrentIndex;

					if (m_CurrentIndex == m_CurrentList.Count)
					{
						++m_ySector;

						if (m_ySector > m_ySectorEnd)
						{
							m_ySector = m_ySectorStart;
							++m_xSector;

							if (m_xSector > m_xSectorEnd)
							{
								if (m_Stage > 0)
								{
									m_CurrentIndex = -1;
									return false;
								}
								++m_Stage;
								m_xSector = m_xSectorStart >>= SectorShift;
								m_ySector = m_ySectorStart >>= SectorShift;
							}
						}

						m_CurrentIndex = -1;

						if (m_Stage == 0)
						{
							m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Items;
						}
						else
						{
							m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Mobiles;
						}
					}
					else
					{
						IEntity e = (IEntity)m_CurrentList[m_CurrentIndex];

						if (e is Item)
						{
							Item item = (Item)e;

							if (!item.Deleted && item.Parent == null && m_Bounds.Contains(e.Location))
							{
								return true;
							}
						}
						else if (e is Mobile)
						{
							Mobile m = (Mobile)e;
							
							if (!m.Deleted && m_Bounds.Contains(e.Location))
							{
								return true;
							}
						}
					}
				}
#endif
			}

			public void Reset()
			{
				m_Map.Bound(m_Bounds.Start.m_X, m_Bounds.Start.m_Y, out m_xSectorStart, out m_ySectorStart);
				m_Map.Bound(m_Bounds.End.m_X - 1, m_Bounds.End.m_Y - 1, out m_xSectorEnd, out m_ySectorEnd);

				m_xSector = m_xSectorStart >>= SectorShift;
				m_ySector = m_ySectorStart >>= SectorShift;

				m_xSectorEnd >>= SectorShift;
				m_ySectorEnd >>= SectorShift;

				m_CurrentIndex = -1;
				m_Stage = 0;
				m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Items;
			}
		}

		private class ItemEnumerator : IPooledEnumerator<Item>
		{
			private Map m_Map;
			private Rectangle2D m_Bounds;

			private int m_xSector, m_ySector;
			private int m_xSectorStart, m_ySectorStart;
			private int m_xSectorEnd, m_ySectorEnd;
			private List<Item> m_CurrentList;
			private int m_CurrentIndex;

			private static readonly Queue<ItemEnumerator> m_InstancePool = new Queue<ItemEnumerator>();

			public static ItemEnumerator Instantiate(Map map, Rectangle2D bounds)
			{
				ItemEnumerator e = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count > 0)
					{
						e = m_InstancePool.Dequeue();

						e.m_Map = map;
						e.m_Bounds = bounds;
					}
				}

				if (e == null)
				{
					e = new ItemEnumerator(map, bounds);
				}

				e.Reset();

				return e;
			}

			public void Free()
			{
				if (m_Map == null)
				{
					return;
				}

				m_Map = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count < 200) // Arbitrary
					{
						m_InstancePool.Enqueue(this);
					}
				}
			}

			private ItemEnumerator(Map map, Rectangle2D bounds)
			{
				m_Map = map;
				m_Bounds = bounds;
			}

			public Item Current { get { return m_CurrentList[m_CurrentIndex]; } }

			object IEnumerator.Current { get { return m_CurrentList[m_CurrentIndex]; } }

			void IDisposable.Dispose()
			{ }

#if NewEnumerators
			private bool NextList()
			{
				++m_ySector;

				if (m_ySector > m_ySectorEnd)
				{
					m_ySector = m_ySectorStart;
					++m_xSector;

					if (m_xSector > m_xSectorEnd)
					{
						m_CurrentIndex = -1;
						return false;
					}
				}

				m_CurrentIndex = -1;
				m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Items;
				return true;
			}
#endif

			public bool MoveNext()
			{
#if NewEnumerators
				Item item;

				while (++m_CurrentIndex <= m_CurrentList.Count)
				{
					if (m_CurrentIndex == m_CurrentList.Count)
					{
						if (!NextList())
						{
							return false;
						}

						continue;
					}

					item = Current;

					if (item != null && !item.Deleted && item.Parent == null && m_Bounds.Contains(item.Location))
					{
						return true;
					}
				}

				return false;
#else
				while (true)
				{
					++m_CurrentIndex;

					if (m_CurrentIndex == m_CurrentList.Count)
					{
						++m_ySector;

						if (m_ySector > m_ySectorEnd)
						{
							m_ySector = m_ySectorStart;
							++m_xSector;

							if (m_xSector > m_xSectorEnd)
							{
								m_CurrentIndex = -1;
								return false;
							}
						}

						m_CurrentIndex = -1;
						m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Items;
					}
					else
					{
						Item item = m_CurrentList[m_CurrentIndex];

						if (!item.Deleted && item.Parent == null && m_Bounds.Contains(item.Location))
						{
							return true;
						}
					}
				}
#endif
			}

			public void Reset()
			{
				m_Map.Bound(m_Bounds.Start.m_X, m_Bounds.Start.m_Y, out m_xSectorStart, out m_ySectorStart);
				m_Map.Bound(m_Bounds.End.m_X - 1, m_Bounds.End.m_Y - 1, out m_xSectorEnd, out m_ySectorEnd);

				m_xSector = m_xSectorStart >>= SectorShift;
				m_ySector = m_ySectorStart >>= SectorShift;

				m_xSectorEnd >>= SectorShift;
				m_ySectorEnd >>= SectorShift;

				m_CurrentIndex = -1;
				m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Items;
			}
		}

		private class MobileEnumerator : IPooledEnumerator<Mobile>
		{
			private Map m_Map;
			private Rectangle2D m_Bounds;

			private int m_xSector, m_ySector;
			private int m_xSectorStart, m_ySectorStart;
			private int m_xSectorEnd, m_ySectorEnd;
			private List<Mobile> m_CurrentList;
			private int m_CurrentIndex;

			private static readonly Queue<MobileEnumerator> m_InstancePool = new Queue<MobileEnumerator>();

			public static MobileEnumerator Instantiate(Map map, Rectangle2D bounds)
			{
				MobileEnumerator e = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count > 0)
					{
						e = m_InstancePool.Dequeue();

						e.m_Map = map;
						e.m_Bounds = bounds;
					}
				}

				if (e == null)
				{
					e = new MobileEnumerator(map, bounds);
				}

				e.Reset();

				return e;
			}

			public void Free()
			{
				if (m_Map == null)
				{
					return;
				}

				m_Map = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count < 200) // Arbitrary
					{
						m_InstancePool.Enqueue(this);
					}
				}
			}

			private MobileEnumerator(Map map, Rectangle2D bounds)
			{
				m_Map = map;
				m_Bounds = bounds;
			}

			public Mobile Current { get { return m_CurrentList[m_CurrentIndex]; } }

			object IEnumerator.Current { get { return m_CurrentList[m_CurrentIndex]; } }

			void IDisposable.Dispose()
			{ }

#if NewEnumerators
			private bool NextList()
			{
				++m_ySector;

				if (m_ySector > m_ySectorEnd)
				{
					m_ySector = m_ySectorStart;
					++m_xSector;

					if (m_xSector > m_xSectorEnd)
					{
						m_CurrentIndex = -1;
						return false;
					}
				}

				m_CurrentIndex = -1;
				m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Mobiles;
				return true;
			}
#endif

			public bool MoveNext()
			{
#if NewEnumerators
				Mobile m;

				while (++m_CurrentIndex <= m_CurrentList.Count)
				{
					if (m_CurrentIndex == m_CurrentList.Count)
					{
						if (!NextList())
						{
							return false;
						}

						continue;
					}

					m = Current;

					if (!m.Deleted && m_Bounds.Contains(m.Location))
					{
						return true;
					}
				}

				return false;
#else
				while (true)
				{
					++m_CurrentIndex;

					if (m_CurrentIndex == m_CurrentList.Count)
					{
						++m_ySector;

						if (m_ySector > m_ySectorEnd)
						{
							m_ySector = m_ySectorStart;
							++m_xSector;

							if (m_xSector > m_xSectorEnd)
							{
								m_CurrentIndex = -1;
								return false;
							}
						}

						m_CurrentIndex = -1;
						m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Mobiles;
					}
					else
					{
						Mobile m = m_CurrentList[m_CurrentIndex];

						if (!m.Deleted && m_Bounds.Contains(m.Location))
						{
							return true;
						}
					}
				}
#endif
			}

			public void Reset()
			{
				m_Map.Bound(m_Bounds.Start.m_X, m_Bounds.Start.m_Y, out m_xSectorStart, out m_ySectorStart);
				m_Map.Bound(m_Bounds.End.m_X - 1, m_Bounds.End.m_Y - 1, out m_xSectorEnd, out m_ySectorEnd);

				m_xSector = m_xSectorStart >>= SectorShift;
				m_ySector = m_ySectorStart >>= SectorShift;

				m_xSectorEnd >>= SectorShift;
				m_ySectorEnd >>= SectorShift;

				m_CurrentIndex = -1;
				m_CurrentList = m_Map.InternalGetSector(m_xSector, m_ySector).Mobiles;
			}
		}

		private class MultiTileEnumerator : IPooledEnumerator<StaticTile[]>
		{
			private List<BaseMulti> m_List;
			private Point2D m_Location;
			private StaticTile[] m_Current;
			private int m_Index;

			private static readonly Queue<MultiTileEnumerator> m_InstancePool = new Queue<MultiTileEnumerator>();

			public static MultiTileEnumerator Instantiate(Sector sector, Point2D loc)
			{
				MultiTileEnumerator e = null;

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count > 0)
					{
						e = m_InstancePool.Dequeue();

						e.m_List = sector.Multis;
						e.m_Location = loc;
					}
				}

				if (e == null)
				{
					e = new MultiTileEnumerator(sector, loc);
				}

				e.Reset();

				return e;
			}

			private MultiTileEnumerator(Sector sector, Point2D loc)
			{
				m_List = sector.Multis;
				m_Location = loc;
			}

			public StaticTile[] Current { get { return m_Current; } }

			object IEnumerator.Current { get { return m_Current; } }

			void IDisposable.Dispose()
			{ }

			public bool MoveNext()
			{
				while (++m_Index < m_List.Count)
				{
					BaseMulti m = m_List[m_Index];

					if (m == null || m.Deleted)
					{
						continue;
					}

					MultiComponentList list = m.Components;

					int xOffset = m_Location.m_X - (m.Location.m_X + list.Min.m_X);
					int yOffset = m_Location.m_Y - (m.Location.m_Y + list.Min.m_Y);

					if (xOffset < 0 || xOffset >= list.Width || yOffset < 0 || yOffset >= list.Height)
					{
						continue;
					}

					StaticTile[] tiles = list.Tiles[xOffset][yOffset];

					if (tiles.Length <= 0)
					{
						continue;
					}

					/*#if NewEnumerators
					for (int i = 0; i < tiles.Length; i++)
					{
						tiles[i].Z += m.Z;
					}

					m_Current = tiles;
					#else*/
					// TODO: How to avoid this copy?
					var copy = new StaticTile[tiles.Length];

					for (int i = 0; i < copy.Length; ++i)
					{
						copy[i] = tiles[i];
						copy[i].Z += m.Z;
					}

					m_Current = copy;
					//#endif
					return true;
				}

				return false;
			}

			public void Free()
			{
				if (m_List == null)
				{
					return;
				}

				lock (m_InstancePool)
				{
					if (m_InstancePool.Count < 200) // Arbitrary
					{
						m_InstancePool.Enqueue(this);
					}

					m_List = null;
				}
			}

			public void Reset()
			{
				m_Current = null;
				m_Index = -1;
			}
		}
		#endregion

		public Point3D GetPoint(object o, bool eye)
		{
			Point3D p;

			if (o is Mobile)
			{
				p = ((Mobile)o).Location;
				p.Z += 14;
			}
			else if (o is Item)
			{
				p = ((Item)o).GetWorldLocation();
				p.Z += (((Item)o).ItemData.Height / 2) + 1;
			}
			else if (o is Point3D)
			{
				p = (Point3D)o;
			}
			else if (o is LandTarget)
			{
				p = ((LandTarget)o).Location;

				int low = 0, avg = 0, top = 0;
				GetAverageZ(p.X, p.Y, ref low, ref avg, ref top);

				p.Z = top + 1;
			}
			else if (o is StaticTarget)
			{
				var st = (StaticTarget)o;
				ItemData id = TileData.ItemTable[st.ItemID & TileData.MaxItemValue];

				p = new Point3D(st.X, st.Y, st.Z - id.CalcHeight + (id.Height / 2) + 1);
			}
			else if (o is IPoint3D)
			{
				p = new Point3D((IPoint3D)o);
			}
			else
			{
				Console.WriteLine("Warning: Invalid object ({0}) in line of sight", o);
				p = Point3D.Zero;
			}

			return p;
		}

		#region Line Of Sight
		private static int _MaxLOSDistance = 25;

		public static int MaxLOSDistance { get { return _MaxLOSDistance; } set { _MaxLOSDistance = value; } }

		public bool LineOfSight(Point3D org, Point3D dest)
		{
			if (this == Internal)
			{
				return false;
			}

			if (!Utility.InRange(org, dest, _MaxLOSDistance))
			{
				return false;
			}

			Point3D end = dest;

			if (org.X > dest.X || (org.X == dest.X && org.Y > dest.Y) || (org.X == dest.X && org.Y == dest.Y && org.Z > dest.Z))
			{
				Point3D swap = org;
				org = dest;
				dest = swap;
			}

			if (org == dest)
			{
				return true;
			}

			int ix, iy, iz;
			int height;
			bool found;
			Point3D p;
			TileFlag flags;
			var path = new Point3DList();

			int xd = dest.m_X - org.m_X;
			int yd = dest.m_Y - org.m_Y;
			int zd = dest.m_Z - org.m_Z;

			double zslp = Math.Sqrt(xd * xd + yd * yd);
			double sq3d = zd != 0 ? Math.Sqrt(zslp * zslp + zd * zd) : zslp;

			double rise = (yd) / sq3d;
			double run = (xd) / sq3d;

			zslp = (zd) / sq3d;

			double y = org.m_Y;
			double z = org.m_Z;
			double x = org.m_X;

			while (Utility.NumberBetween(x, dest.m_X, org.m_X, 0.5) && Utility.NumberBetween(y, dest.m_Y, org.m_Y, 0.5) &&
				   Utility.NumberBetween(z, dest.m_Z, org.m_Z, 0.5))
			{
				ix = (int)Math.Round(x);
				iy = (int)Math.Round(y);
				iz = (int)Math.Round(z);

				if (path.Count > 0)
				{
					p = path.Last;

					if (p.m_X != ix || p.m_Y != iy || p.m_Z != iz)
					{
						path.Add(ix, iy, iz);
					}
				}
				else
				{
					path.Add(ix, iy, iz);
				}

				x += run;
				y += rise;
				z += zslp;
			}

			if (path.Count == 0)
			{
				return true;
			}

			p = path.Last;

			if (p != dest)
			{
				path.Add(dest);
			}

			Point3D pTop = org, pBottom = dest;
			Utility.FixPoints(ref pTop, ref pBottom);

			int pathCount = path.Count;
			int endTop = end.m_Z + 1;

			for (int i = 0; i < pathCount; ++i)
			{
				Point3D point = path[i];
				int pointTop = point.m_Z + 1;

				LandTile landTile = Tiles.GetLandTile(point.X, point.Y);
				int landZ = 0, landAvg = 0, landTop = 0;

				GetAverageZ(point.m_X, point.m_Y, ref landZ, ref landAvg, ref landTop);

				if (landZ <= pointTop && landTop >= point.m_Z &&
					(point.m_X != end.m_X || point.m_Y != end.m_Y || landZ > endTop || landTop < end.m_Z) && !landTile.Ignored)
				{
					return false;
				}

				StaticTile[] statics = Tiles.GetStaticTiles(point.m_X, point.m_Y, true);

				bool contains = false;
				int ltID = landTile.ID;

				for (int j = 0; !contains && j < _InvalidLandTiles.Length; ++j)
				{
					contains = (ltID == _InvalidLandTiles[j]);
				}

				if (contains && statics.Length == 0)
				{
					IPooledEnumerable<Item> eable = GetItemsInRange(point, 0);

					foreach (Item item in eable)
					{
						if (item.Visible)
						{
							contains = false;
						}

						if (!contains)
						{
							break;
						}
					}

					eable.Free();

					if (contains)
					{
						return false;
					}
				}

				foreach (StaticTile t in statics)
				{
					ItemData id = TileData.ItemTable[t.ID & TileData.MaxItemValue];

					flags = id.Flags;
					height = id.CalcHeight;

					if (t.Z > pointTop || t.Z + height < point.Z || (flags & (TileFlag.Window | TileFlag.NoShoot)) == 0)
					{
						continue;
					}

					if (point.m_X == end.m_X && point.m_Y == end.m_Y && t.Z <= endTop && t.Z + height >= end.m_Z)
					{
						continue;
					}

					return false;
				}
			}

			var rect = new Rectangle2D(pTop.m_X, pTop.m_Y, (pBottom.m_X - pTop.m_X) + 1, (pBottom.m_Y - pTop.m_Y) + 1);

			IPooledEnumerable<Item> area = GetItemsInBounds(rect);

			foreach (Item i in area)
			{
				if (!i.Visible)
				{
					continue;
				}

				if (i is BaseMulti || i.ItemID > TileData.MaxItemValue)
				{
					continue;
				}

				ItemData id = i.ItemData;
				flags = id.Flags;

				if ((flags & (TileFlag.Window | TileFlag.NoShoot)) == 0)
				{
					continue;
				}

				height = id.CalcHeight;

				found = false;

				int count = path.Count;

				for (int j = 0; j < count; ++j)
				{
					Point3D point = path[j];
					int pointTop = point.m_Z + 1;
					Point3D loc = i.Location;

					if (loc.m_X != point.m_X || loc.m_Y != point.m_Y || loc.m_Z > pointTop || loc.m_Z + height < point.m_Z)
					{
						continue;
					}

					if (loc.m_X == end.m_X && loc.m_Y == end.m_Y && loc.m_Z <= endTop && loc.m_Z + height >= end.m_Z)
					{
						continue;
					}

					found = true;
					break;
				}

				if (!found)
				{
					continue;
				}

				area.Free();
				return false;
			}

			area.Free();

			return true;
		}

		public bool LineOfSight(object from, object dest)
		{
			if (from == dest || (from is Mobile && ((Mobile)from).AccessLevel >= AccessLevel.Counselor))
			{
				return true;
			}

			if (dest is Item && from is Mobile && ((Item)dest).RootParent == from)
			{
				return true;
			}

			return LineOfSight(GetPoint(from, true), GetPoint(dest, false));
		}

		public bool LineOfSight(Mobile from, Point3D target)
		{
			if (from.AccessLevel >= AccessLevel.Counselor)
			{
				return true;
			}

			Point3D eye = from.Location;

			eye.Z += 14;

			return LineOfSight(eye, target);
		}

		public bool LineOfSight(Mobile from, Mobile to)
		{
			if (from == to || from.AccessLevel >= AccessLevel.Counselor)
			{
				return true;
			}

			Point3D eye = from.Location;
			Point3D target = to.Location;

			eye.Z += 14;
			target.Z += 14; //10;

			return LineOfSight(eye, target);
		}
		#endregion

		private static int[] _InvalidLandTiles = new[] {0x244};

		public static int[] InvalidLandTiles { get { return _InvalidLandTiles; } set { _InvalidLandTiles = value; } }

		public virtual int CompareTo(Map other)
		{
			if (other == null)
			{
				return -1;
			}

			return _MapID.CompareTo(other._MapID);
		}

		public virtual int CompareTo(object other)
		{
			if (other == null || other is Map)
			{
				return CompareTo(other);
			}

			throw new ArgumentException();
		}
	}
}