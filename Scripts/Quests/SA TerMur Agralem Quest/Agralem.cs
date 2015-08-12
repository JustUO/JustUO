using System;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
    public class IntoTheVoidQuest : BaseQuest
    {
        public IntoTheVoidQuest()
        {
            switch (Utility.Random(4))
            {
                case 0:
                    AddObjective(new SlayObjective(typeof (Anzuanord), "Anzuanord", 10));
                    break;
                case 1:
                    AddObjective(new SlayObjective(typeof (Vasanord), "Vasanord", 10));
                    break;
                case 2:
                    AddObjective(new SlayObjective(typeof (UsagralemBallem), "Usagralem Ballem ", 10));
                    break;
                case 3:
                    AddObjective(new SlayObjective(typeof (Anlorzen), "Anlorzen", 10));
                    break;
            }

            AddReward(new BaseReward(typeof (AbyssReaver), 1112694)); /////Abyss Reaver
        }

        public override bool DoneOnce
        {
            get { return true; }
        }

        public override object Title
        {
            get { return 1112687; }
        }

        public override object Description
        {
            get { return 1112690; }
        }

        public override object Refuse
        {
            get { return 1112691; }
        }

        public override object Uncomplete
        {
            get { return 1112692; }
        }

        public override object Complete
        {
            get { return 1112693; }
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

    public class Agralem : MondainQuester
    {
        [Constructable]
        public Agralem()
            : base("Agralem", "the Bladeweaver")
        {
        }

        public Agralem(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        {
            get
            {
                return new[]
                {
                    typeof (IntoTheVoidQuest)
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