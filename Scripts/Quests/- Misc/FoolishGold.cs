namespace Server.Items
{
    public class NiporailemsTreasure : Item
    {
        [Constructable]
        public NiporailemsTreasure() : base(0xEEF)
        {
            Weight = 100.0;
        }

        public NiporailemsTreasure(Serial serial) : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112113; }
        } //  Niporailem's Treasure   

        public override bool DropToWorld(Mobile from, Point3D p)
        {
            var convert = base.DropToWorld(from, p);

            if (convert)
                ConvertItem(from);

            return convert;
        }

        public override bool DropToMobile(Mobile from, Mobile target, Point3D p)
        {
            var convert = base.DropToMobile(from, target, p);

            if (convert)
                ConvertItem(from);

            return convert;
        }

         public override bool DropToItem(Mobile from, Item target, Point3D p, byte gridloc)
        {
            var convert = base.DropToItem(from, target, p, gridloc);

            if (convert && Parent != from.Backpack)
                ConvertItem(from);

            return convert;
			
        }

        public virtual void ConvertItem(Mobile from)
        {
            from.SendLocalizedMessage(1112112); // To carry the burden of greed!
            Delete();
            from.AddToBackpack(new TreasureSand());
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

    public class TreasureSand : Item
    {
        [Constructable]
        public TreasureSand() : base(0x11EA)
        {
            Weight = 25.0;
        }

        public TreasureSand(Serial serial) : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1112115; }
        } //  Treasure Sand           

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