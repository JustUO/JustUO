#region References

using System;
using Server.Gumps;

#endregion

namespace Server.Items
{
	[Flipable(3804, 3803)]
	public class ReferralRewardsStone : Item
	{
		public override string DefaultName
		{
			get { return "Blood Oath Referral Rewards"; }
		}

		[Constructable]
		public ReferralRewardsStone() : base(3804) // looks like a faction signup stone
		{
			Movable = false;
			Hue = 1157; // except it is hued blood red
		}

		public override void OnDoubleClick(Mobile from)
		{
		    if (from == null)
		        return;

		    if (!TellAFriend.Enabled)
		    {
		        from.SendMessage("This system is currently disabled.");
		        return;
		    }

            from.CloseGump(typeof (ReferralRewardsGump));
			from.SendGump(new ReferralRewardsGump(from, 0));
			from.SendMessage(
				String.Format(
					"Welcome to the Referral Rewards shop!  You may obtain points to spend by having your friends mark you as their referrer by using the [referral command!"));
		}

		public ReferralRewardsStone(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}