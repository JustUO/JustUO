using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class PotteryWheelQuest : BaseQuest
    {
        public PotteryWheelQuest()
        {
            AddObjective(new ObtainObjective(typeof (AncientPotteryFragments), "Ancient Pottery Fragments", 15, 0x223B));

            AddReward(new BaseReward(typeof (PotteryWheelAddonDeed), "Pottery Wheel Addon Deed"));
        }

        public override bool DoneOnce
        {
            get { return true; }
        }

        public override object Title
        {
            get { return "Pottery Wheel Quest"; }
        }

        public override object Description
        {
            get
            {
                return
                    "My grandfather has passed away and I need for someone to go and gather Ancient pottery fragments for me. They can be found on Toxic and Stone Sliths, also on Raptors or Enraged earth elementals. I will reward you with a pottery wheel deed, so that you too, may learn this wonderful craft.";
            }
        }

        public override object Refuse
        {
            get { return "That's a shame, I can't get the fragments myself."; }
        }

        public override object Uncomplete
        {
            get { return "You haven't brought me enough pottery fragments yet.."; }
        }

        public override object Complete
        {
            get { return "Oh Wonderful! Here is the reward I promised!"; }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class Valem : MondainQuester
    {
        [Constructable]
        public Valem()
            : base("Valem", "the Pottery Dealer")
        {
        }

        public Valem(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        {
            get
            {
                return new[]
                {
                    typeof (PotteryWheelQuest)
                };
            }
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
            CantWalk = true;

            Body = 666;
            HairItemID = 16987;
            HairHue = 1801;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());

            AddItem(new GargishClothChest(Utility.RandomNeutralHue()));
            AddItem(new GargishClothKilt(Utility.RandomNeutralHue()));
            AddItem(new GargishClothLegs(Utility.RandomNeutralHue()));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }
}