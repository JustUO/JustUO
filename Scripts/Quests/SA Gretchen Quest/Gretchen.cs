using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class Curiosities : BaseQuest
    {
        public Curiosities()
        {
            AddObjective(new ObtainObjective(typeof (FertileDirt), "Fertil Dirt", 3, 0xF81));
            AddObjective(new ObtainObjective(typeof (Bone), "Bone", 3, 0xF7e));

            AddReward(new BaseReward(typeof (ExplodingTarPotion), "Exploding Tar Potion"));
        }

        /* Curiosities */

        public override object Title
        {
            get { return "Curiosities"; }
        }

        public override object Description
        {
            get { return 1094978; }
        }

        public override object Refuse
        {
            get { return "You are Scared from this Task !! Muahahah"; }
        }

        public override object Uncomplete
        {
            get { return "I am sorry that you have not accepted!"; }
        }

        public override object Complete
        {
            get { return 1094981; }
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

    public class Gretchen : MondainQuester
    {
        [Constructable]
        public Gretchen() : base("Gretchen", "the Alchemist")
        {
        }

        public Gretchen(Serial serial) : base(serial)
        {
        }

        public override Type[] Quests
        {
            get
            {
                return new[]
                {
                    typeof (Curiosities)
                };
            }
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
            Race = Race.Human;

            Hue = 0x8412;
            HairItemID = 0x2047;
            HairHue = 0x465;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Shoes(0x1BB));
            AddItem(new LongPants(0x901));
            AddItem(new Tunic(0x70A));
            AddItem(new Cloak(0x675));
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