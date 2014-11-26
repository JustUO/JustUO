#region Header
//   Vorspire    _,-'/-'/  CastBars.cs
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
using System.Drawing;

using Server;
using Server.Mobiles;

using VitaNex.IO;
using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.CastBars
{
	public static partial class SpellCastBars
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		private static CastBarsOptions _CMOptions = new CastBarsOptions();

		private static readonly BinaryDataStore<PlayerMobile, Tuple<bool, Point>> _States =
			new BinaryDataStore<PlayerMobile, Tuple<bool, Point>>(VitaNexCore.SavesDirectory + "/SpellCastBars", "States");

		private static readonly Dictionary<PlayerMobile, SpellCastBar> _Instances =
			new Dictionary<PlayerMobile, SpellCastBar>();

		private static PollTimer _InternalTimer;

		private static readonly Queue<PlayerMobile> _CastBarQueue = new Queue<PlayerMobile>();

		public static CastBarsOptions CMOptions { get { return _CMOptions ?? (_CMOptions = new CastBarsOptions()); } }
		public static BinaryDataStore<PlayerMobile, Tuple<bool, Point>> States { get { return _States; } }
		public static Dictionary<PlayerMobile, SpellCastBar> Instances { get { return _Instances; } }

		public static event Action<CastBarRequestEventArgs> OnCastBarRequest;

		private static void OnSpellRequest(CastSpellRequestEventArgs e)
		{
			if (!CMOptions.ModuleEnabled || !(e.Mobile is PlayerMobile))
			{
				return;
			}

			PlayerMobile user = (PlayerMobile)e.Mobile;

			if (!States.ContainsKey(user) || States[user].Item1)
			{
				_CastBarQueue.Enqueue(user);
			}
		}

		private static void PollCastBarQueue()
		{
			if (!CMOptions.ModuleEnabled)
			{
				return;
			}

			while (_CastBarQueue.Count > 0)
			{
				SendCastBarGump(_CastBarQueue.Dequeue());
			}
		}

		public static void SendCastBarGump(PlayerMobile user)
		{
			if (user == null || user.Deleted || user.NetState == null)
			{
				return;
			}

			var e = new CastBarRequestEventArgs(user, GetOffset(user));

			if (OnCastBarRequest != null)
			{
				OnCastBarRequest(e);
			}

			if (e.Gump == null)
			{
				CastBarRequestHandler(e);
			}

			if (e.Gump != null)
			{
				e.Gump.Refresh(true);
			}
		}

		private static void CastBarRequestHandler(CastBarRequestEventArgs e)
		{
			if (e.User == null || e.User.Deleted || e.User.NetState == null || e.Gump != null)
			{
				return;
			}

			SpellCastBar cb;

			if (Instances.TryGetValue(e.User, out cb) && !cb.IsDisposed)
			{
				cb.X = e.Location.X;
				cb.Y = e.Location.Y;
			}
			else
			{
				cb = new SpellCastBar(e.User, e.Location.X, e.Location.Y);

				if (Instances.ContainsKey(e.User))
				{
					Instances[e.User] = cb;
				}
				else
				{
					Instances.Add(e.User, cb);
				}
			}

			cb.Preview = false;

			e.Gump = cb;

			if (CMOptions.ModuleDebug)
			{
				CMOptions.ToConsole(
					"Request: {0} casting {1}, using {2} ({3}) at {4}",
					e.User,
					e.User.Spell,
					cb,
					cb.Preview ? "Prv" : "Std",
					e.Location);
			}
		}

		public static void HandleToggleCommand(PlayerMobile user)
		{
			if (user == null || user.Deleted || user.NetState == null || !CMOptions.ModuleEnabled)
			{
				return;
			}

			bool t = GetToggle(user);

			SetToggle(user, !t);

			user.SendMessage(t ? 0x22 : 0x55, "Cast-Bar has been {0}.", t ? "disabled" : "enabled");
		}

		public static void HandlePositionCommand(PlayerMobile user)
		{
			if (user == null || user.Deleted || user.NetState == null || !CMOptions.ModuleEnabled)
			{
				return;
			}

			var e = new CastBarRequestEventArgs(user, GetOffset(user));

			if (OnCastBarRequest != null)
			{
				OnCastBarRequest(e);
			}

			if (e.Gump == null)
			{
				CastBarRequestHandler(e);
			}

			if (e.Gump == null)
			{
				return;
			}

			e.Gump.Preview = true;

			SuperGump.Send(
				new OffsetSelectorGump(
					user,
					e.Gump.Refresh(true),
					e.Location,
					(self, oldValue) =>
					{
						SetOffset(self.User, self.Value);
						self.User.SendMessage(0x55, "Cast-Bar position set to X({0:#,0}), Y({1:#,0}).", self.Value.X, self.Value.Y);
						e.Gump.X = self.Value.X;
						e.Gump.Y = self.Value.Y;
						e.Gump.Refresh(true);
					}));
		}

		public static bool GetToggle(PlayerMobile user)
		{
			if (user == null || user.Deleted)
			{
				return false;
			}

			bool toggle = false;

			if (States.ContainsKey(user))
			{
				toggle = States[user].Item1;
			}
			else
			{
				States.Add(user, new Tuple<bool, Point>(false, new Point(200, 200)));
			}

			return toggle;
		}

		public static void SetToggle(PlayerMobile user, bool toggle)
		{
			if (user == null || user.Deleted)
			{
				return;
			}

			if (States.ContainsKey(user))
			{
				States[user] = new Tuple<bool, Point>(toggle, States[user].Item2);
			}
			else
			{
				States.Add(user, new Tuple<bool, Point>(toggle, new Point(200, 200)));
			}

			if (!toggle)
			{
				Instances.Remove(user);
			}
		}

		public static Point GetOffset(PlayerMobile user)
		{
			Point loc = new Point(200, 200);

			if (user == null || user.Deleted)
			{
				return loc;
			}

			if (States.ContainsKey(user))
			{
				loc = States[user].Item2;
			}
			else
			{
				States.Add(user, new Tuple<bool, Point>(true, loc));
			}

			return loc;
		}

		public static void SetOffset(PlayerMobile user, Point loc)
		{
			if (user == null || user.Deleted)
			{
				return;
			}

			if (States.ContainsKey(user))
			{
				States[user] = new Tuple<bool, Point>(States[user].Item1, loc);
			}
			else
			{
				States.Add(user, new Tuple<bool, Point>(true, loc));
			}
		}
	}
}