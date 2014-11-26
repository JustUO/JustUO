#region Header
//   Vorspire    _,-'/-'/  FixMe.cs
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
using System.Text;

using Server;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
using VitaNex.Targets;
#endregion

namespace VitaNex.Commands
{
	[Flags]
	public enum FixMeFlags
	{
		None = 0x0000,
		Mount = 0x0001,
		Pets = 0x0002,
		Equip = 0x0004,
		Gumps = 0x0008,
		Tags = 0x0010,
		Skills = 0x0020,
		Quests = 0x0040,
		All = Mount | Pets | Equip | Gumps | Tags | Skills | Quests
	}

	public static class FixMeCommand
	{
		public static event Action<FixMeGump> OnGumpSend;
		public static event Action<PlayerMobile, FixMeFlags> OnFix;

		public static List<FixMeFlags> DisabledFlags { get; private set; }

		public static void Configure()
		{
			DisabledFlags = new List<FixMeFlags>();

			CommandSystem.Register(
				"FixMe",
				AccessLevel.Player,
				e =>
				{
					if (e == null || e.Mobile == null || !(e.Mobile is PlayerMobile))
					{
						return;
					}

					var g = new FixMeGump(e.Mobile as PlayerMobile);

					if (OnGumpSend != null)
					{
						OnGumpSend(g);
					}

					SuperGump.Send(g);
				});

			CommandSystem.Register(
				"FixThem",
				AccessLevel.GameMaster,
				e =>
				{
					if (e == null || e.Mobile == null || !(e.Mobile is PlayerMobile))
					{
						return;
					}

					e.Mobile.SendMessage(0x22, "Target an on-line player to send them the FixMe gump.");
					e.Mobile.Target = new MobileSelectTarget<PlayerMobile>(
						(m, target) =>
						{
							if (target == null || target.Deleted)
							{
								return;
							}

							if (!target.IsOnline())
							{
								m.SendMessage(0x22, "{0} must be on-line.", target.RawName);
								return;
							}

							m.SendMessage(0x55, "Opening FixMe gump for {0}...", target.RawName);

							var g = new FixMeGump(target);

							if (OnGumpSend != null)
							{
								OnGumpSend(g);
							}

							SuperGump.Send(g);
						},
						m => m.SendMessage(0x22, "Target an on-line player to send them the FixMe gump."));
				});
		}

		public static string GetDescription(this FixMeFlags flags)
		{
			var html = new StringBuilder();

			switch (flags)
			{
				case FixMeFlags.Mount:
					html.Append("Forcibly dismounts if mounted.");
					break;
				case FixMeFlags.Pets:
					{
						html.AppendLine("All pets will be teleported if not stabled and the follower count will be normalized.");
						html.Append("If mounted, the mount will not be included.");
					}
					break;
				case FixMeFlags.Equip:
					html.Append("Equipment will be validated, any invalid equipment will be unequipped.");
					break;
				case FixMeFlags.Gumps:
					html.Append("All open gumps will be refreshed.");
					break;
				case FixMeFlags.Tags:
					html.Append("The property lists for everything will be invalidated.");
					break;
				case FixMeFlags.Skills:
					html.Append("All skills will be normalized if they are detected as invalid.");
					break;
				case FixMeFlags.Quests:
					html.Append("All quests will be repaired if they are detected as invalid.");
					break;
			}

			return html.ToString();
		}

		public static void FixMe(this PlayerMobile m, FixMeFlags flags)
		{
			if (m == null || m.Deleted)
			{
				return;
			}

			if (m.Mounted && flags.HasFlag(FixMeFlags.Mount))
			{
				var mountItem = m.FindItemOnLayer(Layer.Mount) as IMountItem;

				if (mountItem != null)
				{
					if (mountItem.Mount == null || mountItem.Mount != m.Mount)
					{
						m.RemoveItem(mountItem as Item);
					}
					else if (mountItem.Mount.Rider == null)
					{
						mountItem.Mount.Rider = m;
					}
				}
				else if (m.Mount != null && m.Mount.Rider == null)
				{
					m.Mount.Rider = m;
				}

				m.Delta(MobileDelta.Followers);
				m.SendMessage(0x55, "Your mount has been invalidated.");
			}

			if (flags.HasFlag(FixMeFlags.Pets))
			{
				m.Followers = 0;
				m.AllFollowers.ToArray().ForEach(
					f =>
					{
						if (f == null || !(f is BaseCreature))
						{
							return;
						}

						var pet = (BaseCreature)f;

						if (pet.IsStabled || !pet.Controlled || pet.ControlMaster != m)
						{
							return;
						}

						if (pet != m.Mount)
						{
							pet.MoveToWorld(m.Location, m.Map);
							pet.ControlTarget = m;
							pet.ControlOrder = OrderType.Follow;
						}

						m.Followers += pet.ControlSlots;
					});

				m.Followers = Math.Max(0, Math.Min(m.FollowersMax, m.Followers));
				m.Delta(MobileDelta.Followers);

				m.SendMessage(
					0x55,
					"Your pets have been teleported to you and your follower count has been normalized, it is now {0}.",
					m.Followers);
			}

			if (flags.HasFlag(FixMeFlags.Equip))
			{
				m.ValidateEquipment();
			}

			if (m.IsOnline() && flags.HasFlag(FixMeFlags.Gumps))
			{
				foreach (Gump gump in m.NetState.Gumps.ToArray())
				{
					if (gump is SuperGump)
					{
						((SuperGump)gump).Refresh();
					}
					else if (m.HasGump(gump.GetType()))
					{
						m.CloseGump(gump.GetType());
						m.SendGump(gump);
					}
				}

				m.SendMessage(0x55, "Your gumps have been refreshed.");
			}

			if (flags.HasFlag(FixMeFlags.Tags))
			{
				m.InvalidateProperties();

				m.Items.ForEach(
					item =>
					{
						if (item != null && !item.Deleted)
						{
							item.InvalidateProperties();
						}
					});

				if (m.Backpack != null)
				{
					m.Backpack.InvalidateProperties();
					m.Backpack.FindItemsByType<Item>(true).ForEach(
						item =>
						{
							if (item != null && !item.Deleted)
							{
								item.InvalidateProperties();
							}
						});
				}

				m.SendMessage(0x55, "Your tags have been invalidated.");
			}

			if (flags.HasFlag(FixMeFlags.Skills))
			{
				foreach (Skill skill in m.Skills)
				{
					skill.Normalize();
				}

				m.SendMessage(0x55, "Your skills have been normalized.");
			}

			if (flags.HasFlag(FixMeFlags.Quests))
			{
				if (m.Quest != null)
				{
					if (m.Quest.From == null)
					{
						m.Quest.From = m;
					}

					if (m.Quest.Objectives == null || m.Quest.Objectives.Count == 0 || m.Quest.Conversations == null ||
						m.Quest.Conversations.Count == 0)
					{
						m.Quest.Cancel();
					}
				}

				m.SendMessage(0x55, "Your quests have been validated.");
			}

			if (OnFix != null)
			{
				OnFix(m, flags);
			}

			m.SendMessage(0x55, "FixMe completed! If you still have issues, contact a member of staff.");
		}
	}

	public sealed class FixMeGump : ListGump<FixMeFlags>
	{
		private static List<FixMeFlags> _InternalFlags;

		public FixMeGump(PlayerMobile user, Gump parent = null)
			: base(user, parent, title: "Fix Me!", emptyText: "There are no operations to display.")
		{
			Modal = true;
			CanMove = false;
			CanResize = false;
			BlockSpeech = true;
		}

		protected override void Compile()
		{
			if (_InternalFlags == null)
			{
				var ops = ((FixMeFlags)0).GetValues<FixMeFlags>();

				if (ops != null)
				{
					_InternalFlags = new List<FixMeFlags>(ops);
					_InternalFlags.Remove(FixMeFlags.None);
					_InternalFlags.Remove(FixMeFlags.All);

					FixMeCommand.DisabledFlags.ForEach(f => _InternalFlags.Remove(f));
				}
			}

			base.Compile();
		}

		protected override void CompileList(List<FixMeFlags> list)
		{
			list.Clear();
			list.Capacity = _InternalFlags.Count;
			list.AddRange(_InternalFlags);

			base.CompileList(list);
		}

		protected override void SelectEntry(GumpButton button, FixMeFlags entry)
		{
			base.SelectEntry(button, entry);

			var html = new StringBuilder();

			html.AppendFormat("This operation will fix your {0}.", entry.ToString().ToLower());
			html.AppendLine();
			html.Append(entry.GetDescription());
			html.AppendLine();
			html.AppendLine("Do you want to continue?");

			Send(
				new ConfirmDialogGump(
					User,
					Refresh(),
					title: "Confirm Operation",
					html: html.ToString(),
					onAccept: b =>
					{
						User.FixMe(entry);
						Refresh(true);
					}));
		}
	}
}