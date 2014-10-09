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
	public sealed class ExtendedOPL : List<string>
	{
		/// <summary>
		///     Breaks EmptyClilocs every Nth entry. EG: When 10 entries have been added, use the next clilocID
		/// </summary>
		public static int ClilocBreak = 5;

		/// <summary>
		///     Empty clilocID list extended with ~1_VAL~ *support should be the same as ~1_NOTHING~
		/// </summary>
		public static int[] EmptyClilocs = new[]
		{
			//1042971, 1070722, // ~1_NOTHING~ (Reserved by ObjectPropertyList)
			1114057, 1114778, 1114779, // ~1_val~
			1150541, // ~1_TOKEN~
			1153153, // ~1_year~
		};

		public static int[,] EmptyMultiClilocs = new[,]
		{
			{1041522, 3}, // ~1~~2~~3~
			{1060847, 2}, // ~1_val~ ~2_val~
			{1116560, 2}, // ~1_val~ ~2_val~
			{1116690, 3}, // ~1_val~ ~2_val~ ~3_val~
			{1116691, 3}, // ~1_val~ ~2_val~ ~3_val~
			{1116692, 3}, // ~1_val~ ~2_val~ ~3_val~
			{1116693, 3}, // ~1_val~ ~2_val~ ~3_val~
			{1116694, 3}, // ~1_val~ ~2_val~ ~3_val~
		};

		private int _Index;

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
		///     Gets the underlying ObjectPropertyList
		/// </summary>
		public ObjectPropertyList Opl { get; private set; }

		/// <summary>
		///     Event called when an Item based OPL is requested
		/// </summary>
		public static event Action<Item, Mobile, ExtendedOPL> OnItemOPLRequest;

		/// <summary>
		///     Event called when a Mobile based OPL is requested
		/// </summary>
		public static event Action<Mobile, Mobile, ExtendedOPL> OnMobileOPLRequest;

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

			if (serial.IsItem)
			{
				Item item = World.FindItem(serial);

				if (item == null || item.Deleted)
				{
					return;
				}

				var list = new ObjectPropertyList(item);

				item.GetProperties(list);
				item.AppendChildProperties(list);

				InvokeItemOPLRequest(item, state.Mobile, list);

				list.Terminate();
				list.SetStatic();

				buffer = list.Compile(state.CompressionEnabled, out length);
			}
			else if (serial.IsMobile)
			{
				Mobile mobile = World.FindMobile(serial);

				if (mobile == null || mobile.Deleted)
				{
					return;
				}

				var list = new ObjectPropertyList(mobile);

				mobile.GetProperties(list);

				InvokeMobileOPLRequest(mobile, state.Mobile, list);

				list.Terminate();
				list.SetStatic();

				buffer = list.Compile(state.CompressionEnabled, out length);
			}
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

			var list = new ObjectPropertyList(m);

			m.GetProperties(list);

			InvokeMobileOPLRequest(m, to, list);

			list.Terminate();
			list.SetStatic();

			to.Send(list);
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

			var list = new ObjectPropertyList(item);

			item.GetProperties(list);
			item.AppendChildProperties(list);

			InvokeItemOPLRequest(item, to, list);

			list.Terminate();
			list.SetStatic();

			to.Send(list);
		}

		private static void InvokeMobileOPLRequest(Mobile mobile, Mobile viewer, ObjectPropertyList list)
		{
			if (mobile == null || list == null)
			{
				return;
			}

			if (OnMobileOPLRequest == null)
			{
				return;
			}

			var eList = new ExtendedOPL(list);
			OnMobileOPLRequest(mobile, viewer, eList);
			eList.Apply();
		}

		private static void InvokeItemOPLRequest(Item item, Mobile viewer, ObjectPropertyList list)
		{
			if (item == null || list == null)
			{
				return;
			}

			if (OnItemOPLRequest == null)
			{
				return;
			}

			var eList = new ExtendedOPL(list);
			OnItemOPLRequest(item, viewer, eList);
			eList.Apply();
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

		/// <summary>
		///     Applies all changes to the underlying ObjectPropertyList
		/// </summary>
		public void Apply()
		{
			if (Opl == null || Count == 0)
			{
				return;
			}

			for (; _Index < EmptyClilocs.Length + EmptyMultiClilocs.Length; _Index++)
			{
				var range = GetCurrentRange();

				if (range.Count == 0)
				{
					break;
				}

				if (_Index < EmptyClilocs.Length)
				{
					Opl.Add(EmptyClilocs[_Index], String.Join("\n", range));
				}
				else
				{
					int index = _Index - EmptyClilocs.Length;
					var args = new object[EmptyMultiClilocs[index, 1]];
					var format = new string[args.Length];

					args.For(
						i =>
						{
							args[i] = i == 0 ? String.Join("\n", range) : String.Empty;
							format[i] = "{" + i + "}";
						});

					Opl.Add(EmptyMultiClilocs[index, 0], String.Join("\t", format), args);
				}
			}

			_Index = 0;
		}

		/// <summary>
		///     Gets a collection of the current entries within range of the current ClilocBreak index
		/// </summary>
		/// <returns>A collection of the current entries within range of the current ClilocBreak index</returns>
		public List<string> GetCurrentRange()
		{
			int index = (_Index * LineBreak);
			int length = LineBreak;

			if (index < 0)
			{
				index = 0;
			}
			else if (index >= Count)
			{
				index = Count;
			}

			var list = new List<string>();

			for (int i = index; i < index + length; i++)
			{
				if (i >= Count)
				{
					break;
				}

				if (!String.IsNullOrEmpty(this[i]) && !list.Contains(this[i]))
				{
					list.Add(this[i]);
				}
			}

			return list;
		}

		public void Add(string format, params object[] args)
		{
			base.Add(String.Format(format, args));
		}
	}
}