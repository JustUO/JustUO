#region Header
//   Vorspire    _,-'/-'/  ExtendedOPL.cs
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
using System.IO;

using Server;
using Server.Network;
#endregion

namespace VitaNex.Network
{
	/// <summary>
	///     Provides methods for extending Item and Mobile ObjectPropertyLists by invoking event subscriptions - No more GetProperties overrides!
	/// </summary>
	public sealed class ExtendedOPL : Queue<string>
	{
		/// <summary>
		///     Breaks EmptyClilocs every Nth entry. EG: When 5 entries have been added, use the next clilocID
		/// </summary>
		public static int ClilocBreak = 5;

		/// <summary>
		///     Breaks EmptyClilocs when the current string value length axceeds this threshold, regardless of the current ClilocBreak.
		/// </summary>
		public static int ClilocThreshold = 160;

		/// <summary>
		///     Empty clilocID list extended with ~1_VAL~ *support should be the same as ~1_NOTHING~
		///     The default settings for an ExtendedOPL instance allows for up to 65 custom cliloc entries.
		///     Capacity is equal to the number of available empty clilocs multiplied by the cliloc break value.
		///     Default: 65 == 13 * 5
		///     Clilocs with multiple argument support will be parsed accordingly.
		///     It is recommended to use clilocs that contain no characters other than the argument placeholders and whitespace.
		/// </summary>
		public static int[] EmptyClilocs = new[]
		{
			//1042971, 1070722, // ~1_NOTHING~ (Reserved by ObjectPropertyList)
			1114057, 1114778, 1114779, // ~1_val~
			1150541, // ~1_TOKEN~
			1153153, // ~1_year~

			1041522, // ~1~~2~~3~
			1060847, // ~1_val~ ~2_val~
			1116560, // ~1_val~ ~2_val~
			1116690, // ~1_val~ ~2_val~ ~3_val~
			1116691, // ~1_val~ ~2_val~ ~3_val~
			1116692, // ~1_val~ ~2_val~ ~3_val~
			1116693, // ~1_val~ ~2_val~ ~3_val~
			1116694 // ~1_val~ ~2_val~ ~3_val~
		};

		public static bool Initialized { get; private set; }

		/// <summary>
		///     Gets a value representing the parent OPL PacketHandler that was overridden, if any
		/// </summary>
		public static PacketHandler ReqOplParent { get; private set; }

		/// <summary>
		///     Gets a value representing the parent BatchOPL PacketHandler that was overridden, if any
		/// </summary>
		public static PacketHandler ReqBatchOplParent { get; private set; }

		/// <summary>
		///     Gets a value represting the handler to use when decoding OPL packet 0xD6
		/// </summary>
		public static OutgoingPacketOverrideHandler OutParent0xD6 { get; private set; }

		/// <summary>
		///     Event called when an Item based OPL is requested
		/// </summary>
		public static event Action<Item, Mobile, ExtendedOPL> OnItemOPLRequest;

		/// <summary>
		///     Event called when a Mobile based OPL is requested
		/// </summary>
		public static event Action<Mobile, Mobile, ExtendedOPL> OnMobileOPLRequest;

		/// <summary>
		///     Event called when an IEntity based OPL is requested that doesn't match an Item or Mobile
		/// </summary>
		public static event Action<IEntity, Mobile, ExtendedOPL> OnEntityOPLRequest;

		public static Dictionary<ObjectPropertyList, int> Cache { get; private set; }

		static ExtendedOPL()
		{
			Cache = new Dictionary<ObjectPropertyList, int>();
		}

		public static void Init()
		{
			if (Initialized)
			{
				return;
			}

			ReqOplParent = PacketHandlers.GetExtendedHandler(0x10);
			PacketHandlers.RegisterExtended(ReqOplParent.PacketID, ReqOplParent.Ingame, OnQueryProperties);

			ReqBatchOplParent = PacketHandlers.GetHandler(0xD6);

			bool is6017 = (PacketHandlers.Get6017Handler(0xD6) != null);
			PacketHandlers.Register(
				ReqBatchOplParent.PacketID, ReqBatchOplParent.Length, ReqBatchOplParent.Ingame, OnBatchQueryProperties);

			if (is6017)
			{
				PacketHandlers.Register6017(
					ReqBatchOplParent.PacketID, ReqBatchOplParent.Length, ReqBatchOplParent.Ingame, OnBatchQueryProperties);
			}

			OutParent0xD6 = OutgoingPacketOverrides.GetHandler(0xD6);
			OutgoingPacketOverrides.Register(0xD6, true, OnEncode0xD6);

			Initialized = true;

			PollTimer.FromSeconds(10.0, FreeCache);
		}

		private static void FreeCache()
		{
			Cache.RemoveKeyRange(
				opl =>
				opl.Entity == null || opl.Entity.Deleted || opl.Entity.Map == null || opl.Entity.Map == Map.Internal ||
				opl.UnderlyingStream == null || opl.UnderlyingStream.Position >= opl.UnderlyingStream.Length);
		}

		private static void OnEncode0xD6(NetState state, PacketReader reader, ref byte[] buffer, ref int length)
		{
			if (state == null || reader == null || buffer == null || length < 0)
			{
				return;
			}

			int pos = reader.Seek(0, SeekOrigin.Current);
			reader.Seek(5, SeekOrigin.Begin);
			Serial serial = reader.ReadInt32();
			reader.Seek(pos, SeekOrigin.Begin);

			var e = World.FindEntity(serial);

			if (e == null || e.Deleted)
			{
				return;
			}

			var opl = new ObjectPropertyList(e);

			if (e is Item)
			{
				var item = (Item)e;

				item.GetProperties(opl);
				item.AppendChildProperties(opl);
			}
			else if (e is Mobile)
			{
				var mob = (Mobile)e;

				mob.GetProperties(opl);
			}

			var eopl = new ExtendedOPL(opl);

			InvokeOPLRequest(e, state.Mobile, eopl);

			eopl.Apply();

			opl.Terminate();
			opl.SetStatic();

			buffer = opl.Compile(state.CompressionEnabled, out length);

			Cache.Remove(opl);
		}

		private static void OnBatchQueryProperties(NetState state, PacketReader pvSrc)
		{
			if (state == null || pvSrc == null || !ObjectPropertyList.Enabled)
			{
				return;
			}

			if (OnItemOPLRequest == null && OnMobileOPLRequest == null)
			{
				if (ReqBatchOplParent != null)
				{
					ReqBatchOplParent.OnReceive(state, pvSrc);
					return;
				}
			}

			Mobile from = state.Mobile;

			int length = pvSrc.Size - 3;

			if (length < 0 || (length % 4) != 0)
			{
				return;
			}

			int count = length / 4;

			for (int i = 0; i < count; ++i)
			{
				Serial s = pvSrc.ReadInt32();

				if (s.IsMobile)
				{
					Mobile m = World.FindMobile(s);

					if (m != null && from.CanSee(m) && Utility.InUpdateRange(from, m))
					{
						SendPropertiesTo(from, m);
					}
				}
				else if (s.IsItem)
				{
					Item item = World.FindItem(s);

					if (item != null && !item.Deleted && from.CanSee(item) &&
						Utility.InUpdateRange(from.Location, item.GetWorldLocation()))
					{
						SendPropertiesTo(from, item);
					}
				}
			}
		}

		private static void OnQueryProperties(NetState state, PacketReader pvSrc)
		{
			if (!ObjectPropertyList.Enabled || state == null || pvSrc == null)
			{
				return;
			}

			Mobile from = state.Mobile;
			Serial s = pvSrc.ReadInt32();

			if (s.IsMobile)
			{
				Mobile m = World.FindMobile(s);

				if (m != null && from.CanSee(m) && Utility.InUpdateRange(from, m))
				{
					SendPropertiesTo(from, m);
				}
			}
			else if (s.IsItem)
			{
				Item item = World.FindItem(s);

				if (item != null && !item.Deleted && from.CanSee(item) &&
					Utility.InUpdateRange(from.Location, item.GetWorldLocation()))
				{
					SendPropertiesTo(from, item);
				}
			}
		}

		/// <summary>
		///     Forces the comilation of a new Mobile based ObjectPropertyList and sends it to the specified Mobile
		/// </summary>
		/// <param name="to">Mobile viewer, the Mobile viewing the OPL</param>
		/// <param name="m">Mobile owner, the Mobile which owns the OPL</param>
		public static void SendPropertiesTo(Mobile to, Mobile m)
		{
			if (to == null || m == null)
			{
				return;
			}

			var opl = new ObjectPropertyList(m);

			m.GetProperties(opl);

			var eopl = new ExtendedOPL(opl);

			InvokeOPLRequest(m, to, eopl);

			eopl.Apply();

			opl = eopl.Opl;

			opl.Terminate();
			opl.SetStatic();

			to.Send(opl);

			Cache.Remove(opl);
		}

		/// <summary>
		///     Forces the comilation of a new Item based ObjectPropertyList and sends it to the specified Mobile
		/// </summary>
		/// <param name="to">Mobile viewer, the Mobile viewing the OPL</param>
		/// <param name="item"></param>
		public static void SendPropertiesTo(Mobile to, Item item)
		{
			if (to == null || item == null)
			{
				return;
			}

			var opl = new ObjectPropertyList(item);

			item.GetProperties(opl);
			item.AppendChildProperties(opl);

			var eopl = new ExtendedOPL(opl);

			InvokeOPLRequest(item, to, eopl);

			eopl.Apply();

			opl = eopl.Opl;

			opl.Terminate();
			opl.SetStatic();

			to.Send(opl);

			Cache.Remove(opl);
		}

		private static void InvokeOPLRequest(IEntity entity, Mobile viewer, ExtendedOPL eopl)
		{
			if (entity == null || entity.Deleted || eopl == null)
			{
				return;
			}

			if (entity is Mobile && OnMobileOPLRequest != null)
			{
				OnMobileOPLRequest((Mobile)entity, viewer, eopl);
			}

			if (entity is Item && OnItemOPLRequest != null)
			{
				OnItemOPLRequest((Item)entity, viewer, eopl);
			}

			if (OnEntityOPLRequest != null)
			{
				OnEntityOPLRequest(entity, viewer, eopl);
			}
		}

		public static void AddTo(ObjectPropertyList opl, string line, params object[] args)
		{
			if (args != null)
			{
				line = String.Format(line, args);
			}

			AddTo(opl, new[] {line});
		}

		public static void AddTo(ObjectPropertyList opl, string[] lines)
		{
			if (opl != null)
			{
				new ExtendedOPL(opl, lines).Apply();
			}
		}

		public static ClilocInfo Lookup(int index)
		{
			return EmptyClilocs.InBounds(index) ? ClilocLNG.NULL.Lookup(EmptyClilocs[index]) : null;
		}

		private static readonly object _SyncLock = new object();

		/// <summary>
		///     Gets or sets the underlying ObjectPropertyList
		/// </summary>
		public ObjectPropertyList Opl { get; set; }

		public int Index
		{
			get
			{
				if (Opl == null)
				{
					return -1;
				}

				int index;

				lock (_SyncLock)
				{
					if (!Cache.TryGetValue(Opl, out index))
					{
						Cache.Add(Opl, index = 0);
					}
				}

				return Math.Max(0, Math.Min(EmptyClilocs.Length, index));
			}
			set
			{
				if (Opl == null)
				{
					return;
				}

				lock (_SyncLock)
				{
					Cache.AddOrReplace(Opl, Math.Max(0, Math.Min(EmptyClilocs.Length, value)));
				}
			}
		}

		public int LineBreak { get; set; }

		/// <summary>
		///     Create with pre-defined OPL
		/// </summary>
		/// <param name="opl">ObjectPropertyList object to wrap and extend</param>
		public ExtendedOPL(ObjectPropertyList opl)
		{
			Opl = opl;
			LineBreak = ClilocBreak;
		}

		/// <summary>
		///     Create with pre-defined OPL
		/// </summary>
		/// <param name="opl">ObjectPropertyList object to wrap and extend</param>
		/// <param name="capacity">Capacity of the extension</param>
		public ExtendedOPL(ObjectPropertyList opl, int capacity)
			: base(capacity)
		{
			Opl = opl;
			LineBreak = ClilocBreak;
		}

		/// <summary>
		///     Create with pre-defined OPL
		/// </summary>
		/// <param name="opl">ObjectPropertyList object to wrap and extend</param>
		/// <param name="list">Pre-defined list to append to the specified OPL</param>
		public ExtendedOPL(ObjectPropertyList opl, IEnumerable<string> list)
			: base(list)
		{
			Opl = opl;
			LineBreak = ClilocBreak;
		}

		~ExtendedOPL()
		{
			Opl = null;

			Clear();
			TrimExcess();
		}

		/// <summary>
		///     Applies all changes to the underlying ObjectPropertyList
		/// </summary>
		public void Apply()
		{
			if (Opl == null || Count == 0)
			{
				Clear();
				TrimExcess();
				return;
			}

			while (Count > 0)
			{
				if (!EmptyClilocs.InBounds(Index))
				{
					break;
				}

				var info = Lookup(Index++);

				if (info == null || !info.HasArgs)
				{
					continue;
				}

				int r = 0, l = 0;

				foreach (var entry in this)
				{
					if (!String.IsNullOrEmpty(entry))
					{
						if (r > 0 && l + entry.Length >= ClilocThreshold)
						{
							break;
						}

						l += entry.Length;
					}

					if (++r >= LineBreak || l >= ClilocThreshold)
					{
						break;
					}
				}

				if (r <= 0)
				{
					break;
				}

				if (r == 1)
				{
					Opl.Add(info.Index, info.ToString(Dequeue()));
					continue;
				}

				var range = new string[Math.Min(r, Count)];

				this.DequeueRange(ref range);

				Opl.Add(info.Index, info.ToString(String.Join("\n", range)));
			}

			Clear();
			TrimExcess();
		}

		public void Add(string format, params object[] args)
		{
			if (Opl == null)
			{
				return;
			}

			if (format == null)
			{
				format = String.Empty;
			}

			if (args == null || args.Length == 0)
			{
				Enqueue(format);
				return;
			}

			Enqueue(String.Format(format, args));
		}

		public void Add(int number, params object[] args)
		{
			if (Opl == null)
			{
				return;
			}

			var info = ClilocLNG.NULL.Lookup(number);

			if (info != null)
			{
				Enqueue(info.ToString(args));
			}
		}
	}
}