#region Header
//   Vorspire    _,-'/-'/  ExportBounds.cs
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
using System.IO;

using Server;
using Server.Commands;
using Server.Mobiles;

using VitaNex.IO;
#endregion

namespace VitaNex.Commands
{
	public static class ExportBoundsCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register(
				"ExportBounds2D",
				AccessLevel.GameMaster,
				e =>
				{
					if (e != null && e.Mobile != null)
					{
						OnExportBounds2D(e.Mobile, e.ArgString);
					}
				});

			CommandSystem.Register(
				"ExportBounds3D",
				AccessLevel.GameMaster,
				e =>
				{
					if (e != null && e.Mobile != null)
					{
						OnExportBounds3D(e.Mobile, e.ArgString);
					}
				});
		}

		public static void OnExportBounds2D(Mobile m, string speech)
		{
			if (m == null || m.Deleted || !(m is PlayerMobile))
			{
				return;
			}

			if (String.IsNullOrWhiteSpace(speech))
			{
				speech = TimeStamp.UtcNow.ToString();
			}

			BoundingBoxPicker.Begin(
				m,
				(from, map, start, end, state) =>
				{
					var r = new Rectangle2D(start, end.Clone3D(1, 1));

					using (
						StreamWriter w =
							IOUtility.EnsureFile(
								VitaNexCore.DataDirectory + "/Exported Bounds/2D/" + IOUtility.GetSafeFileName(speech) + ".txt").AppendText())
					{
						w.WriteLine("new Rectangle2D({0}, {1}, {2}, {3}),", r.Start.X, r.Start.Y, r.Width, r.Height);
						w.Close();
					}
				},
				null);
		}

		public static void OnExportBounds3D(Mobile m, string speech)
		{
			if (m == null || m.Deleted || !(m is PlayerMobile))
			{
				return;
			}

			if (String.IsNullOrWhiteSpace(speech))
			{
				speech = TimeStamp.UtcNow.ToString();
			}

			BoundingBoxPicker.Begin(
				m,
				(from, map, start, end, state) =>
				{
					var r = new Rectangle3D(start, end.Clone3D(1, 1));

					using (
						StreamWriter w =
							IOUtility.EnsureFile(
								VitaNexCore.DataDirectory + "/Exported Bounds/3D/" + IOUtility.GetSafeFileName(speech) + ".txt").AppendText())
					{
						w.WriteLine(
							"new Rectangle3D({0}, {1}, {2}, {3}, {4}, {5}),", r.Start.X, r.Start.Y, r.Start.Z, r.Width, r.Height, r.Depth);
						w.Close();
					}
				},
				null);
		}
	}
}