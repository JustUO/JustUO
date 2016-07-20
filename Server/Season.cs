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
//  [2015] Season.cs
// ************************************/
#endregion

using System;
using System.Collections.Generic;
using System.IO;

namespace Server
{
    #region Notes
    /// <summary>
	/// This class takes care of the map tile transformations in seasons.
	/// 
	/// All the maps have a base tileset. Upon this, each concrete season defines a list
	/// of tile transformations. This transformations make the environment change, for example,
	/// making shruberry and foliage in Trammel look like tombs in Felucca.
	/// 
	/// This transformations were only performed client side, but there are few of them that
	/// also changes the tile meta data, not only the graphic, i.e. height and blocking
	/// attributes. As this transformations were not performed server side, players could
	/// pass over certain tiles in a map with a modified season, when they shouldn't.
	/// 
	/// This patch applies the pertinent transformations also server side to keep in sync
	/// tile attributes and avoid movement exploits.
    /// </summary>
    #endregion
    public class Season
	{
		private static readonly int SeasonCount = 5;

		static Season()
		{
			m_TileChanges = new Dictionary<int, int>[SeasonCount];

			// 0 is default season
			RegisterSeason( 1, "spring" );
			RegisterSeason( 2, "fall" );
			RegisterSeason( 3, "winter" );
			RegisterSeason( 4, "desolation" );
		}

        public static StaticTile[] PatchTiles(StaticTile[] tiles, int season)
		{
			if ( season <= 0 || season >= SeasonCount )
				return tiles;

			var tileChanges = m_TileChanges[season];
			if ( tileChanges != null )
			{
				for ( int i = 0; i < tiles.Length; i++ )
				{
                    if (tileChanges.ContainsKey(tiles[i].ID))
                        tiles[i].ID = tileChanges[tiles[i].ID];
				}
			}

			return tiles;
		}

		private static Dictionary<int, int>[] m_TileChanges;

		private static void RegisterSeason( int seasonID, string name )
		{
			m_TileChanges[seasonID] = GetTileChanges( name );
		}

		private static Dictionary<int, int> GetTileChanges( string name )
		{
			string filename = Path.Combine( "Data/Seasons", String.Format( "{0}.cfg", name ) );

			if ( File.Exists( filename ) )
			{
				Dictionary<int, int> tileChanges = new Dictionary<int, int>();

				using ( StreamReader bin = new StreamReader( filename ) )
				{
					string line;
					while ( ( line = bin.ReadLine() ) != null )
					{
						if ( ( line.Length == 0 ) || line.StartsWith( "#" ) )
							continue;

						char[] delimiters = new char[1] { '\t' };

						string[] data = line.Split( delimiters );

						try
						{
							int defaultID = Utility.ToInt32( data[0] );
							int morphID = Utility.ToInt32( data[1] );

							tileChanges[defaultID] = morphID;

							continue;
						}
						catch
						{
							Console.WriteLine( "Warning: Invalid season entry:" );
							Console.WriteLine( line );
							continue;
						}
					}
				}

				return tileChanges;
			}
			else
			{
				Console.WriteLine( "Warning: season {0} not found.", name );
				return null;
			}
		}
	}
}