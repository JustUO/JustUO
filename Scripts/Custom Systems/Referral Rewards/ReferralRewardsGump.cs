#region References

using System;
using System.Collections;
using System.IO;
using Server.Engines.XmlSpawner2;
using Server.Items;
using Server.Mobiles;
using Server.Network;

#endregion

namespace Server.Gumps
{
	public class ReferralRewardDescriptionGump : Gump
	{
		public ReferralRewardDescriptionGump(ReferralRewardItems r) : base(610, 30)
		{
			AddBackground(0, 0, 200, 400, 0x2436);
			AddHtml(20, 20, 160, 50, String.Format("<basefont color=cyan><center><Big><B>{0}", r.Name), false, false);
			AddLabel(38, 60, 32, String.Format("Cost: {0} {1}", r.Cost, (r.Cost != 1 ? "Referral Points" : "Referral Point")));
			// display the item's image
			if (r.ItemID > 0)
			{
				AddItem(80, 90, r.ItemID, r.Hue);
			}

			AddHtml(20, 125, 160, 245, String.Format("<basefont color=white><center>{0}", r.Description), false, false);
		}
	}

	public class ReferralRewardsGump : Gump
	{
		private readonly ArrayList Rewards;

		private int y_inc = 35;
		private int x_creditoffset = 350;
		private int x_pointsoffset = 480;
		private int maxItemsPerPage = 9;
		private readonly int viewpage;

		public ReferralRewardsGump(Mobile from, int page) : base(20, 30)
		{
			from.CloseGump(typeof (ReferralRewardsGump));

			// determine the gump size based on the number of rewards
			Rewards = ReferralRewardItems.RewardsList;

			viewpage = page;

			int height = maxItemsPerPage*y_inc + 120;
			int width = x_pointsoffset + 110;

			AddBackground(0, 0, width, height, 0x2436);

			AddHtml(40, 20, 350, 50, "<basefont color=white><Big><B>Referral Rewards", false, false);


			// add the button to open the browser to the website donation store
			//AddButton( width - 40, 35, 2472, 2474, 14, GumpButtonType.Reply, 0 );	// Top Right Corner
			//AddLabel( width - 165, 35, 1265, "Make A Donation: ");

			XmlReferralRewards re = (XmlReferralRewards) XmlAttach.FindAttachment(from, typeof (XmlReferralRewards));

			if (re != null)
			{
				AddLabel(width - 185, 35, 1265, String.Format("Points Available: {0}", re.PointsAvailable));
			}


			// put the page buttons in the lower right corner
			if (Rewards != null && Rewards.Count > 0)
			{
				AddLabel(width - 165, height - 35, 32,
					String.Format("Page: {0}/{1}", viewpage + 1, Rewards.Count/maxItemsPerPage + 1));

				// page up and down buttons
				AddButton(width - 55, height - 35, 0x15E0, 0x15E4, 13, GumpButtonType.Reply, 0);
				AddButton(width - 35, height - 35, 0x15E2, 0x15E6, 12, GumpButtonType.Reply, 0);
			}

			AddLabel(70, 50, 32, "Reward");
			AddLabel(x_creditoffset - 20, 50, 32, "Referral Points");

			// display the items with their selection buttons
			if (Rewards != null)
			{
				int y = 50;
				for (int i = 0; i < Rewards.Count; i++)
				{
					if (i/maxItemsPerPage != viewpage)
					{
						continue;
					}

					ReferralRewardItems r = Rewards[i] as ReferralRewardItems;
					if (r == null)
					{
						continue;
					}

					y += y_inc;

					int texthue = 1149;


					// add the selection button
					AddButton(30, y, 0xFA5, 0xFA7, 1000 + i, GumpButtonType.Reply, 0);


					// display the name
					AddLabel(70, y + 3, texthue, r.Name);

					// display the cost
					AddLabel(x_creditoffset, y + 3, texthue, r.Cost.ToString());

					// display the item
					if (r.ItemID > 0)
					{
						AddItem(x_creditoffset + 60, y, r.ItemID, r.Hue);
					}
				}
			}

			// display the items with their selection buttons
			if (Rewards != null)
			{
				int y = 50;
				for (int i = 0; i < Rewards.Count; i++)
				{
					if (i/maxItemsPerPage != viewpage)
					{
						continue;
					}

					ReferralRewardItems r = Rewards[i] as ReferralRewardItems;
					if (r == null)
					{
						continue;
					}

					y += y_inc;

					//int texthue = 1149;

					// add the selection button
					AddButton(width - 33, y, 22153, 22155, 2000 + i, GumpButtonType.Reply, 0);
				}
			}
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (info == null || state == null || state.Mobile == null || Rewards == null)
			{
				return;
			}

			Mobile from = state.Mobile;

			switch (info.ButtonID)
			{
				case 12:
					// page up
					int nitems = 0;
					if (Rewards != null)
					{
						nitems = Rewards.Count;
					}

					int page = viewpage + 1;
					if (page > nitems/maxItemsPerPage)
					{
						page = nitems/maxItemsPerPage;
					}
					state.Mobile.SendGump(new ReferralRewardsGump(state.Mobile, page));
					break;
				case 13:
					// page down
					page = viewpage - 1;
					if (page < 0)
					{
						page = 0;
					}
					state.Mobile.SendGump(new ReferralRewardsGump(state.Mobile, page));
					break;
				default:
				{
					if (info.ButtonID >= 1000 && info.ButtonID < 2000)
					{
						int selection = info.ButtonID - 1000;
						if (selection < Rewards.Count)
						{
							ReferralRewardItems r = Rewards[selection] as ReferralRewardItems;

							XmlReferralRewards re = (XmlReferralRewards) XmlAttach.FindAttachment(from, typeof (XmlReferralRewards));

							if (((re.LastRewardChosen + TimeSpan.FromDays(1.0)) > DateTime.Now) && from.AccessLevel < AccessLevel.GameMaster)
							{
								from.SendMessage(39, "You can only purchase one referral reward per day.");
								return;
							}

							if (re != null && re.PointsAvailable >= r.Cost)
							{
								re.PointsAvailable = re.PointsAvailable - r.Cost;
								re.PointsSpent = re.PointsSpent + r.Cost;
								re.RewardsChosen++;
								re.LastRewardChosen = DateTime.Now;

								// create an instance of the reward type
								object o = null;

								try
								{
									o = Activator.CreateInstance(r.RewardType, r.RewardArgs);
								}
								catch
								{
								}

								bool received = true;

								if (o is Item)
								{
									if (o is BasePotion)
									{
										BasePotion bp = o as BasePotion;
										bp.Amount = 500;
									}
									// and give them the item
									PlayerMobile pm = from as PlayerMobile;
									BankBox box = null;

									if (pm != null)
									{
										box = pm.FindBankNoCreate();
									}

									if (box != null)
									{
										box.AddItem((Item) o);
										from.SendMessage("{0} has been placed in your bank box.", ((Item) o).Name);
									}
									else if (pm.Backpack != null && !pm.Backpack.Deleted)
									{
										pm.Backpack.AddItem((Item) o);
										from.SendMessage("{0} has been placed in your backpack.", ((Item) o).Name);
									}
									else
									{
										received = false;
										from.SendMessage("An error has occured, please page staff about this issue immediately!");
									}
								}
								else if (o is Mobile)
								{
									// if it is controllable then set the buyer as master.  Note this does not check for control slot limits.
									if (o is BaseCreature)
									{
										BaseCreature b = o as BaseCreature;
										b.Controlled = true;
										b.ControlMaster = from;
									}

									((Mobile) o).MoveToWorld(from.Location, from.Map);
								}
								else if (o is XmlAttachment)
								{
									XmlAttachment a = o as XmlAttachment;


									XmlAttach.AttachTo(from, a);
								}
								else
								{
									from.SendMessage(33, "Unable to create {0}. Please page a staff member.", r.RewardType.Name);
									received = false;
								}

								from.SendMessage("You have purchased {0} for {1} Referral Points.", r.Name, r.Cost);

								LogPurchase(r, from, state, received);
									// creates a log of the purchased items to Donations.log in the main server folder. This is for Owners to verify donation claims, tracking what sells best, etc.
							}
							else
							{
								from.SendMessage("Insufficient Referral Points for {0}.", r.Name);
							}
							from.SendGump(new ReferralRewardsGump(from, viewpage));
						}
					}
					else if (info.ButtonID >= 2000)
					{
						int selection = info.ButtonID - 2000;
						if (selection < Rewards.Count)
						{
							ReferralRewardItems r = Rewards[selection] as ReferralRewardItems;

							from.SendGump(new ReferralRewardsGump(from, viewpage));

							if (r != null)
							{
								from.CloseGump(typeof (ReferralRewardDescriptionGump));
								from.SendGump(new ReferralRewardDescriptionGump(r));
							}
						}
					}
					break;
				}
			}
		}

		// This will log the purchase to a text file.
		public static void LogPurchase(ReferralRewardItems r, Mobile from, NetState state, bool received)
		{
			try
			{
				using (StreamWriter sw = new StreamWriter("ReferralRewards.log", true))
				{
					sw.WriteLine("{0} [{1} {2}] \"{3}\" buys \"{4}\" for {5}EC.{6}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:sstt"),
						state, @from.Account, @from.RawName, r.Name, r.Cost, (received ? "" : " ***ERROR: Not Received***"));
				}
			}
			catch
			{
			}
		}
	}
}