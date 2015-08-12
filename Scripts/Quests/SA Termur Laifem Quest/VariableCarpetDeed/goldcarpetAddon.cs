namespace Server.Items
{
    public class goldcarpetAddon : BaseAddon
    {
        [Constructable]
        public goldcarpetAddon()
        {
            AddonComponent ac = null;
            ac = new AddonComponent(2779);
            AddComponent(ac, 2, 2, 0);
            ac = new AddonComponent(2785);
            AddComponent(ac, 2, 1, 0);
            ac = new AddonComponent(2785);
            AddComponent(ac, 2, 0, 0);
            ac = new AddonComponent(2786);
            AddComponent(ac, 1, 2, 0);
            ac = new AddonComponent(2786);
            AddComponent(ac, 0, 2, 0);
            ac = new AddonComponent(2781);
            AddComponent(ac, -2, 2, 0);
            ac = new AddonComponent(2786);
            AddComponent(ac, -1, 2, 0);
            ac = new AddonComponent(2782);
            AddComponent(ac, 2, -2, 0);
            ac = new AddonComponent(2783);
            AddComponent(ac, -2, 1, 0);
            ac = new AddonComponent(2783);
            AddComponent(ac, -2, 0, 0);
            ac = new AddonComponent(2783);
            AddComponent(ac, -2, -1, 0);
            ac = new AddonComponent(2784);
            AddComponent(ac, 1, -2, 0);
            ac = new AddonComponent(2784);
            AddComponent(ac, 0, -2, 0);
            ac = new AddonComponent(2784);
            AddComponent(ac, -1, -2, 0);
            ac = new AddonComponent(2785);
            AddComponent(ac, 2, -1, 0);
            ac = new AddonComponent(2780);
            AddComponent(ac, -2, -2, 0);
            ac = new AddonComponent(2778);
            AddComponent(ac, -1, -1, 0);
            ac = new AddonComponent(2778);
            AddComponent(ac, -1, 0, 0);
            ac = new AddonComponent(2778);
            AddComponent(ac, -1, 1, 0);
            ac = new AddonComponent(2778);
            AddComponent(ac, 0, 1, 0);
            ac = new AddonComponent(2778);
            AddComponent(ac, 0, 0, 0);
            ac = new AddonComponent(2778);
            AddComponent(ac, 0, -1, 0);
            ac = new AddonComponent(2778);
            AddComponent(ac, 1, -1, 0);
            ac = new AddonComponent(2778);
            AddComponent(ac, 1, 0, 0);
            ac = new AddonComponent(2778);
            AddComponent(ac, 1, 1, 0);
        }

        public goldcarpetAddon(Serial serial) : base(serial)
        {
        }

        public override BaseAddonDeed Deed
        {
            get { return new goldcarpetAddonDeed(); }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }

    public class goldcarpetAddonDeed : BaseAddonDeed
    {
        [Constructable]
        public goldcarpetAddonDeed()
        {
            Name = "goldcarpet";
        }

        public goldcarpetAddonDeed(Serial serial) : base(serial)
        {
        }

        public override BaseAddon Addon
        {
            get { return new goldcarpetAddon(); }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // Version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}