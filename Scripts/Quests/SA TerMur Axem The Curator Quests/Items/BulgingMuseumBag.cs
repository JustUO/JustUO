namespace Server.Items
{
    public class BulgingMuseumBag : BaseRewardBag
    {
        [Constructable]
        public BulgingMuseumBag()
        {
            AddItem(new Gold(10000));

            switch (Utility.Random(9))
            {
                case 0:
                    AddItem(new Amber(10));
                    break;
                case 1:
                    AddItem(new Amethyst(10));
                    break;
                case 2:
                    AddItem(new Citrine(10));
                    break;
                case 3:
                    AddItem(new Ruby(10));
                    break;
                case 4:
                    AddItem(new Emerald(10));
                    break;
                case 5:
                    AddItem(new Diamond(10));
                    break;
                case 6:
                    AddItem(new Sapphire(10));
                    break;
                case 7:
                    AddItem(new StarSapphire(10));
                    break;
                case 8:
                    AddItem(new Tourmaline(10));
                    break;
            }

            switch (Utility.Random(5))
            {
                case 0:
                    AddItem(new ElvenFletchings(20));
                    break;
                case 1:
                    AddItem(new RelicFragment(20));
                    break;
                case 2:
                    AddItem(new DelicateScales(20));
                    break;
                case 3:
                    AddItem(new ChagaMushroom(20));
                    break;
                case 4:
                    AddItem(new FeyWings(20));
                    break;
            }
        }

        public BulgingMuseumBag(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112995; }
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