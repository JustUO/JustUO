#region Header
//   Vorspire    _,-'/-'/  SuperGump.cs
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
using System.Linq;
using System.Threading.Tasks;

using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
#endregion

#pragma warning disable 109

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump : Gump, IEquatable<SuperGump>, IDisposable
	{
		public static int DefaultX = 250;
		public static int DefaultY = 200;

		public static int DefaultTextHue = 85;
		public static int DefaultErrorHue = 34;
		public static int DefaultHighlightHue = 51;

		public static TimeSpan DefaultRefreshRate = TimeSpan.FromSeconds(30);
		public static Color DefaultHtmlColor = Color.Gold;

		public static TimeSpan PollInterval = TimeSpan.FromMilliseconds(10.0);

		public static TimeSpan DClickInterval = TimeSpan.FromMilliseconds(1000.0);

		private static void InternalClose(SuperGump g)
		{
			if (g != null)
			{
				VitaNexCore.TryCatch(() => InternalClose(g.User, g));
			}
		}

		private static void InternalClose(Mobile m, Gump g)
		{
			if (m == null || g == null)
			{
				return;
			}

			g.OnServerClose(m.NetState);

			if (m.NetState == null)
			{
				return;
			}

			m.Send(new CloseGump(g.TypeID, 0));
			m.NetState.RemoveGump(g);
		}

		public static TGump Send<TGump>(TGump gump) where TGump : SuperGump
		{
			return gump != null ? (gump.Compiled ? gump.Refresh() : gump.Send()) as TGump : null;
		}

		private Gump _Parent;

		private DateTime _UtcNow = DateTime.UtcNow;

		protected int NextButtonID = 1;
		protected int NextSwitchID;
		protected int NextTextInputID;

		private bool _Modal;
		private bool _EnablePolling;
		private bool _AutoRefresh;

		public DateTime LastButtonClick { get; private set; }
		public bool DoubleClicked { get; private set; }

		public bool IsDisposed { get; private set; }

		public virtual PollTimer InstancePoller { get; protected set; }

		public virtual bool InitPolling { get { return false; } }

		public bool EnablePolling
		{
			get { return _EnablePolling; }
			set
			{
				if (_EnablePolling && !value)
				{
					_EnablePolling = false;

					if (InstancePoller != null)
					{
						InstancePoller.Dispose();
						InstancePoller = null;
					}
				}
				else if (!_EnablePolling && value)
				{
					_EnablePolling = true;

					InitPollTimer();
				}
				else
				{
					InitPollTimer();
				}
			}
		}

		public virtual Gump Parent
		{
			get { return _Parent; }
			set
			{
				if (_Parent == value)
				{
					return;
				}

				if (!IsDisposed && _Parent is SuperGump)
				{
					((SuperGump)_Parent).RemoveChild(this);
				}

				_Parent = value;

				if (!IsDisposed && _Parent is SuperGump)
				{
					((SuperGump)_Parent).AddChild(this);
				}
			}
		}

		public virtual SuperGumpLayout Layout { get; set; }
		public virtual PlayerMobile User { get; set; }

		public virtual bool Modal
		{
			get { return _Modal; }
			set
			{
				_Modal = value;

				if (!_Modal && value)
				{
					_Modal = true;

					CanMove = false;
					CanResize = false;
				}
				else if (_Modal && !value)
				{
					_Modal = false;
				}
			}
		}

		public virtual bool ModalSafety { get; set; }

		public virtual bool BlockSpeech { get; set; }
		public virtual bool BlockMovement { get; set; }

		public virtual bool ForceRecompile { get; set; }

		public virtual int TextHue { get; set; }
		public virtual int ErrorHue { get; set; }
		public virtual int HighlightHue { get; set; }

		public virtual TimeSpan AutoRefreshRate { get; set; }
		public virtual DateTime LastAutoRefresh { get; set; }

		private bool _PollingWasDisabled;

		public virtual bool AutoRefresh
		{
			get { return _AutoRefresh; }
			set
			{
				if (!_AutoRefresh && value)
				{
					_AutoRefresh = true;

					if (!EnablePolling)
					{
						_PollingWasDisabled = EnablePolling = true;
					}
				}
				else if (_AutoRefresh && !value)
				{
					_AutoRefresh = false;

					EnablePolling = !_PollingWasDisabled;
				}
			}
		}

		public bool IsOpen { get; private set; }
		public bool Hidden { get; private set; }
		public bool Compiled { get; private set; }
		public bool Initialized { get; private set; }

		public virtual bool RandomButtonID { get; set; }
		public virtual bool RandomTextEntryID { get; set; }
		public virtual bool RandomSwitchID { get; set; }

		public int IndexedPage
		{
			get
			{
				var list = GetEntries<GumpPage>().Select(e => e.Page).ToList();

				int idx = list.Count > 0 ? list.Max() : -1;

				list.Clear();
				list.TrimExcess();

				return idx;
			}
		}

		public SuperGump(PlayerMobile user, Gump parent = null, int? x = null, int? y = null)
			: base(x ?? DefaultX, y ?? DefaultY)
		{
			Linked = new List<SuperGump>();
			Children = new List<SuperGump>();

			Buttons = new Dictionary<GumpButton, Action<GumpButton>>();
			TileButtons = new Dictionary<GumpImageTileButton, Action<GumpImageTileButton>>();
			Switches = new Dictionary<GumpCheck, Action<GumpCheck, bool>>();
			Radios = new Dictionary<GumpRadio, Action<GumpRadio, bool>>();
			TextInputs = new Dictionary<GumpTextEntry, Action<GumpTextEntry, string>>();
			LimitedTextInputs = new Dictionary<GumpTextEntryLimited, Action<GumpTextEntryLimited, string>>();

			TextHue = DefaultTextHue;
			ErrorHue = DefaultErrorHue;
			HighlightHue = DefaultHighlightHue;

			User = user;
			Parent = parent;
			Modal = false;
			ModalSafety = true;
			BlockSpeech = false;
			BlockMovement = false;

			AutoRefresh = false;
			LastAutoRefresh = DateTime.UtcNow;
			AutoRefreshRate = DefaultRefreshRate;

			RegisterInstance();

			EnablePolling = InitPolling;
		}

		~SuperGump()
		{
			//Console.WriteLine("~{0}: 0x{1:X}", GetType().Name, Serial);

			Dispose();
		}

		public void InitPollTimer()
		{
			if (InstancePoller != null)
			{
				InstancePoller.Dispose();
				InstancePoller = null;
			}

			if (EnablePolling)
			{
				InstancePoller = PollTimer.CreateInstance(PollInterval, OnInstancePollCheck, CanPollInstance);
			}
		}

		protected int NewButtonID()
		{
			return RandomButtonID ? (NextButtonID += Utility.Random(Utility.Dice(6, 6, 6)) + 1) : NextButtonID++;
		}

		protected int NewTextEntryID()
		{
			return RandomTextEntryID ? (NextTextInputID += Utility.Random(Utility.Dice(6, 6, 6)) + 1) : NextTextInputID++;
		}

		protected int NewSwitchID()
		{
			return RandomSwitchID ? (NextSwitchID += Utility.Random(Utility.Dice(6, 6, 6)) + 1) : NextSwitchID++;
		}

		public void AddPage()
		{
			AddPage(IndexedPage + 1);
		}

		public void AddPageBackButton(int x, int y, int normalID, int pressedID)
		{
			AddPageBackButton(x, y, normalID, pressedID, IndexedPage);
		}

		public void AddPageNextButton(int x, int y, int normalID, int pressedID)
		{
			AddPageNextButton(x, y, normalID, pressedID, IndexedPage);
		}

		public void AddPageButton(int x, int y, int normalID, int pressedID)
		{
			AddPageButton(x, y, normalID, pressedID, IndexedPage);
		}

		public void AddPageBackButton(int x, int y, int normalID, int pressedID, int page)
		{
			AddPageButton(x, y, normalID, pressedID, page - 1);
			AddTooltip(1011067); // Previous Page
		}

		public void AddPageNextButton(int x, int y, int normalID, int pressedID, int page)
		{
			AddPageButton(x, y, normalID, pressedID, page + 1);
			AddTooltip(1011066); // Next Page
		}

		public void AddPageButton(int x, int y, int normalID, int pressedID, int page)
		{
			AddButton(x, y, normalID, pressedID, -1, GumpButtonType.Page, page);
		}

		protected virtual new void Compile()
		{
			if (IsDisposed)
			{
				return;
			}

			if (Modal)
			{
				if (X > 0)
				{
					ModalXOffset = X;
					X = 0;
				}

				if (Y > 0)
				{
					ModalYOffset = Y;
					Y = 0;
				}

				CanMove = false;
				CanResize = false;
			}
			else
			{
				if (ModalXOffset > 0)
				{
					X = ModalXOffset;
					ModalXOffset = 0;
				}

				if (ModalYOffset > 0)
				{
					Y = ModalYOffset;
					ModalYOffset = 0;
				}
			}
		}

		protected virtual void CompileLayout(SuperGumpLayout layout)
		{
			if (IsDisposed)
			{
				return;
			}

			if (Modal)
			{
				layout.Add(
					"alpharegion/modal",
					() =>
					{
						for (int a = 0; a < 2; a++)
						{
							for (int b = 0; b < 2; b++)
							{
								AddImageTiled(a * 1024, b * 786, 1024, 786, 2624);
								AddAlphaRegion(a * 1024, b * 786, 1024, 786);
							}
						}
					});
			}
		}

		protected virtual void OnClick()
		{ }

		protected virtual void OnDoubleClick()
		{ }

		protected virtual void OnInstancePollCheck()
		{
			_UtcNow = DateTime.UtcNow;

			if (!IsDisposed && CanAutoRefresh())
			{
				OnAutoRefresh();
			}
		}

		protected virtual bool CanPollInstance()
		{
			return !IsDisposed && IsOpen && !Hidden && EnablePolling;
		}

		protected virtual void OnAutoRefresh()
		{
			if (IsDisposed)
			{
				return;
			}

			LastAutoRefresh = _UtcNow;
			Refresh();
		}

		protected virtual bool CanAutoRefresh()
		{
			return !IsDisposed && IsOpen && AutoRefresh && !HasChildren && (_UtcNow - LastAutoRefresh) >= AutoRefreshRate;
		}

		protected virtual void OnRefreshed()
		{
			if (IsDisposed)
			{
				return;
			}

			LastAutoRefresh = DateTime.UtcNow;

			RegisterInstance();

			if (Parent is SuperGump)
			{
				((SuperGump)Parent).AddChild(this);
			}

			if (InstancePoller == null)
			{
				InitPollTimer();
			}
			else if (!InstancePoller.Running)
			{
				InstancePoller.Running = EnablePolling;
			}

			Linked.AsEnumerable().ForEach(g => g.OnLinkRefreshed(this));
		}

		protected virtual void Refresh(GumpButton b)
		{
			if (!IsDisposed)
			{
				Refresh(true);
			}
		}

		protected virtual void Refresh(GumpImageTileButton b)
		{
			if (!IsDisposed)
			{
				Refresh(true);
			}
		}

		public virtual SuperGump Refresh(bool recompile = false)
		{
			if (IsDisposed)
			{
				return this;
			}

			if (IsOpen)
			{
				InternalClose(this);
			}

			if (ForceRecompile || !Compiled || recompile)
			{
				return Send();
			}

			if (Modal && ModalSafety && Buttons.Count == 0 && TileButtons.Count == 0)
			{
				CanDispose = true;
				CanClose = true;
			}

			IsOpen = User.SendGump(this, false);
			Hidden = false;
			OnRefreshed();
			return this;
		}

		public T Send<T>() where T : SuperGump
		{
			return Send() as T;
		}

		private bool _Sending;

		public virtual SuperGump Send()
		{
			if (IsDisposed || _Sending)
			{
				return this;
			}

			_Sending = true;

			return VitaNexCore.TryCatchGet(
				() =>
				{
					if (IsOpen)
					{
						InternalClose(this);
					}

					Compile();
					Clear();

					AddPage();

					CompileLayout(Layout);
					Layout.ApplyTo(this);

					InvalidateOffsets();
					InvalidateSize();

					Compiled = true;

					if (Modal && ModalSafety && Buttons.Count == 0 && TileButtons.Count == 0)
					{
						CanDispose = true;
						CanClose = true;
					}

					OnBeforeSend();

					Initialized = true;
					IsOpen = User.SendGump(this, false);
					Hidden = false;

					if (IsOpen)
					{
						OnSend();
					}
					else
					{
						OnSendFail();
					}

					_Sending = false;

					return this;
				},
				e =>
				{
					Console.WriteLine("SuperGump '{0}' could not be sent, an exception was caught:", GetType().FullName);
					VitaNexCore.ToConsole(e);
					IsOpen = false;
					Hidden = false;
					OnSendFail();

					_Sending = false;
				}) ?? this;
		}

		protected virtual void OnBeforeSend()
		{
			VitaNexCore.TryCatch(() => EnumerateInstances(User, GetType()).Where(g => g != this).ForEach(InternalClose));
		}

		protected virtual void OnSend()
		{
			if (IsDisposed)
			{
				return;
			}

			LastAutoRefresh = DateTime.UtcNow;

			RegisterInstance();

			if (Parent is SuperGump)
			{
				((SuperGump)Parent).AddChild(this);
				Children.TrimExcess();
			}

			if (InstancePoller == null)
			{
				InitPollTimer();
			}
			else if (!InstancePoller.Running)
			{
				InstancePoller.Running = EnablePolling;
			}

			Linked.AsEnumerable().ForEach(g => g.OnLinkSend(this));
			Linked.TrimExcess();

			if (Modal && User != null && User.Holding != null)
			{
				Timer.DelayCall(TimeSpan.FromSeconds(1.0), User.DropHolding);
			}
		}

		protected virtual void OnSendFail()
		{
			if (IsDisposed)
			{
				return;
			}

			UnregisterInstance();

			if (Parent is SuperGump)
			{
				((SuperGump)Parent).RemoveChild(this);
				Children.TrimExcess();
			}

			if (InstancePoller != null)
			{
				InstancePoller.Dispose();
				InstancePoller = null;
			}

			Linked.AsEnumerable().ForEach(g => g.OnLinkSendFail(this));
			Linked.TrimExcess();
		}

		protected virtual void OnHidden(bool all)
		{
			if (IsDisposed)
			{
				return;
			}

			Linked.AsEnumerable().ForEach(g => g.OnLinkHidden(this));
			Linked.TrimExcess();
		}

		protected virtual void Hide(GumpButton b)
		{
			if (!IsDisposed)
			{
				Hide();
			}
		}

		protected virtual void Hide(GumpImageTileButton b)
		{
			if (!IsDisposed)
			{
				Hide();
			}
		}

		public virtual SuperGump Hide(bool all = false)
		{
			if (IsDisposed)
			{
				return this;
			}

			Hidden = true;

			if (IsOpen)
			{
				InternalClose(this);
			}

			if (Parent != null)
			{
				if (all)
				{
					if (Parent is SuperGump)
					{
						((SuperGump)Parent).Hide(true);
					}
					else
					{
						InternalClose(User, Parent);
					}
				}
			}

			OnHidden(all);
			return this;
		}

		protected virtual void OnClosed(bool all)
		{
			if (IsDisposed)
			{
				return;
			}

			UnregisterInstance();

			if (InstancePoller != null)
			{
				InstancePoller.Dispose();
				InstancePoller = null;
			}

			Linked.AsEnumerable().ForEach(g => g.OnLinkClosed(this));
			Linked.TrimExcess();
		}

		protected virtual void Close(GumpButton b)
		{
			if (!IsDisposed)
			{
				Close();
			}
		}

		protected virtual void Close(GumpImageTileButton b)
		{
			if (!IsDisposed)
			{
				Close();
			}
		}

		public virtual void Close(bool all = false)
		{
			if (IsDisposed)
			{
				return;
			}

			if (IsOpen || Hidden)
			{
				InternalClose(this);
			}

			IsOpen = false;
			Hidden = false;

			if (Parent != null)
			{
				if (all)
				{
					if (Parent is SuperGump)
					{
						((SuperGump)Parent).Close(true);
					}
					else
					{
						InternalClose(User, Parent);
					}
				}
				else
				{
					if (Parent is SuperGump)
					{
						((SuperGump)Parent).Send();
					}
					else
					{
						User.SendGump(Parent);
					}
				}
			}

			OnClosed(all);
		}

		protected virtual void Clear()
		{
			NextButtonID = 1;
			NextSwitchID = 0;
			NextTextInputID = 0;

			Buttons.Clear();
			TileButtons.Clear();
			Switches.Clear();
			Radios.Clear();
			TextInputs.Clear();
			LimitedTextInputs.Clear();

			Entries.Clear();
			Entries.TrimExcess();

			if (Layout == null)
			{
				Layout = new SuperGumpLayout();
			}
			else
			{
				Layout.Clear();
			}
		}

		protected virtual void OnSpeech(SpeechEventArgs e)
		{
			if (BlockSpeech && !IsDisposed && IsOpen && !Hidden && !e.Blocked && User.AccessLevel < AccessLevel.Counselor)
			{
				e.Blocked = true;
			}
		}

		protected virtual void OnMovement(MovementEventArgs e)
		{
			if (BlockMovement && !IsDisposed && IsOpen && !Hidden && !e.Blocked && User.AccessLevel < AccessLevel.Counselor)
			{
				e.Blocked = true;
			}
		}

		public override sealed void OnServerClose(NetState owner)
		{
			if (IsDisposed)
			{
				return;
			}

			IsOpen = false;

			UnregisterInstance();

			if (!Hidden && InstancePoller != null)
			{
				InstancePoller.Dispose();
				InstancePoller = null;
			}

			base.OnServerClose(owner);
		}

		public override sealed void OnResponse(NetState sender, RelayInfo info)
		{
			if (IsDisposed)
			{
				return;
			}

			if (Switches.Count > 128)
			{
				Parallel.ForEach(Switches.Keys, e => HandleSwitch(e, info.IsSwitched(e.SwitchID)));
			}
			else
			{
				Switches.Keys.ForEach(e => HandleSwitch(e, info.IsSwitched(e.SwitchID)));
			}

			if (Radios.Count > 128)
			{
				Parallel.ForEach(Radios.Keys, e => HandleRadio(e, info.IsSwitched(e.SwitchID)));
			}
			else
			{
				Radios.Keys.ForEach(e => HandleRadio(e, info.IsSwitched(e.SwitchID)));
			}

			if (TextInputs.Count > 128)
			{
				Parallel.ForEach(
					TextInputs.Keys,
					e =>
					{
						var r = info.GetTextEntry(e.EntryID);
						HandleTextInput(e, r != null ? r.Text : String.Empty);
					});
			}
			else
			{
				TextInputs.Keys.ForEach(
					e =>
					{
						var r = info.GetTextEntry(e.EntryID);
						HandleTextInput(e, r != null ? r.Text : String.Empty);
					});
			}

			if (LimitedTextInputs.Count > 128)
			{
				Parallel.ForEach(
					LimitedTextInputs.Keys,
					e =>
					{
						var r = info.GetTextEntry(e.EntryID);
						HandleLimitedTextInput(e, r != null ? r.Text : String.Empty);
					});
			}
			else
			{
				LimitedTextInputs.Keys.ForEach(
					e =>
					{
						var r = info.GetTextEntry(e.EntryID);
						HandleLimitedTextInput(e, r != null ? r.Text : String.Empty);
					});
			}

			GumpButton button1 = GetButtonEntry(info.ButtonID);
			GumpImageTileButton button2 = GetTileButtonEntry(info.ButtonID);

			if (button1 == null && button2 == null)
			{
				Close();
			}
			else
			{
				Hide();

				if (button1 != null)
				{
					HandleButtonClick(button1);
				}

				if (button2 != null)
				{
					HandleTileButtonClick(button2);
				}
			}

			base.OnResponse(sender, info);
		}

		public virtual IEnumerable<T> GetEntries<T>() where T : GumpEntry
		{
			return Entries.OfType<T>();
		}

		public override int GetHashCode()
		{
			return Serial;
		}

		public override bool Equals(object other)
		{
			return !ReferenceEquals(other, null) && (other is SuperGump ? Equals((SuperGump)other) : base.Equals(other));
		}

		public virtual bool Equals(SuperGump other)
		{
			return !ReferenceEquals(other, null) && other.Serial == Serial;
		}

		public void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			IsDisposed = true;

			//Console.WriteLine("SuperGump Disposing: {0} (0x{1:X})", GetType(), Serial);

			//GC.SuppressFinalize(this);

			VitaNexCore.TryCatch(OnDispose);

			VitaNexCore.TryCatch(UnregisterInstance);

			NextButtonID = 1;
			NextSwitchID = 0;
			NextTextInputID = 0;

			if (InstancePoller != null)
			{
				VitaNexCore.TryCatch(InstancePoller.Dispose);
			}

			VitaNexCore.TryCatch(
				() =>
				{
					Buttons.Clear();
					TileButtons.Clear();
					Switches.Clear();
					Radios.Clear();
					TextInputs.Clear();
					LimitedTextInputs.Clear();

					Entries.Free(true);

					Layout.Clear();
				});

			VitaNexCore.TryCatch(
				() =>
				{
					Linked.AsEnumerable().ForEach(Unlink);
					Linked.Free(true);
				});

			VitaNexCore.TryCatch(
				() =>
				{
					Children.AsEnumerable().ForEach(RemoveChild);
					Children.Free(true);
				});

			IsOpen = false;
			Hidden = false;

			VitaNexCore.TryCatch(OnDisposed);

			InstancePoller = null;

			Parent = null;
			User = null;

			Buttons = null;
			TileButtons = null;
			Switches = null;
			Radios = null;
			TextInputs = null;
			LimitedTextInputs = null;

			Layout = null;

			Linked = null;
			Children = null;
		}

		protected virtual void OnDispose()
		{ }

		protected virtual void OnDisposed()
		{ }
	}
}