namespace Server.Items
{
    public class BaseRewardBackpack : Backpack
    {
        public BaseRewardBackpack()
        {
            Hue = 1127;
        }

        public BaseRewardBackpack(Serial serial)
            : base(serial)
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

            var version = reader.ReadInt();
        }
    }

    public class DustyAdventurersBackpack : BaseRewardBackpack
    {
        [Constructable]
        public DustyAdventurersBackpack()
        {
            AddItem(new MagicalResidue(20));
            AddItem(new Amber());
            AddItem(new Sapphire());
            AddItem(new Gold(2000));
            AddItem(new Bow());
            AddItem(new GargishBracelet());

            switch (Utility.Random(5))
            {
                case 0:
                    AddItem(new LeatherChest());
                    break;
                case 1:
                    AddItem(new LeatherArms());
                    break;
                case 2:
                    AddItem(new LeatherLegs());
                    break;
                case 3:
                    AddItem(new GargishLeatherChest());
                    break;
                case 4:
                    AddItem(new GargishLeatherArms());
                    break;
                case 5:
                    AddItem(new GargishLeatherLegs());
                    break;
            }
        }

        public DustyAdventurersBackpack(Serial serial)
            : base(serial)
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

            var version = reader.ReadInt();
        }
    }

    public class DustyExplorersBackpack : BaseRewardBackpack
    {
        [Constructable]
        public DustyExplorersBackpack()
        {
            AddItem(new EnchantEssence(10));
            AddItem(new Amber());
            AddItem(new Citrine());
            AddItem(new Amethyst());
            AddItem(new Gold(4000));
            switch (Utility.Random(4))
            {
                case 0:
                    AddItem(new GargishRing());
                    break;
                case 1:
                    AddItem(new GargishNecklace());
                    break;
                case 2:
                    AddItem(new GargishBracelet());
                    break;
                case 3:
                    AddItem(new GargishEarrings());
                    break;
            }

            switch (Utility.Random(6))
            {
                case 0:
                    AddItem(new GlassSword());
                    break;
                case 1:
                    AddItem(new Katana());
                    break;
                case 2:
                    AddItem(new Broadsword());
                    break;
                case 3:
                    AddItem(new Mace());
                    break;
                case 4:
                    AddItem(new Halberd());
                    break;
                case 5:
                    AddItem(new Shortblade());
                    break;
            }
            switch (Utility.Random(6))
            {
                case 0:
                    AddItem(new LeatherChest());
                    break;
                case 1:
                    AddItem(new LeatherArms());
                    break;
                case 2:
                    AddItem(new LeatherLegs());
                    break;
                case 3:
                    AddItem(new GargishLeatherChest());
                    break;
                case 4:
                    AddItem(new GargishLeatherArms());
                    break;
                case 5:
                    AddItem(new GargishLeatherLegs());
                    break;
            }
        }

        public DustyExplorersBackpack(Serial serial)
            : base(serial)
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

            var version = reader.ReadInt();
        }
    }

    public class DustyHuntersBackpack : BaseRewardBackpack
    {
        [Constructable]
        public DustyHuntersBackpack()
        {
            AddItem(new RelicFragment(1));
            AddItem(new Amber());
            AddItem(new Ruby());
            AddItem(new Diamond());
            AddItem(new Gold(6000));

            switch (Utility.Random(4))
            {
                case 0:
                    AddItem(new GargishRing());
                    break;
                case 1:
                    AddItem(new GargishNecklace());
                    break;
                case 2:
                    AddItem(new GargishBracelet());
                    break;
                case 3:
                    AddItem(new GargishEarrings());
                    break;
            }

            switch (Utility.Random(6))
            {
                case 0:
                    AddItem(new GlassSword());
                    break;
                case 1:
                    AddItem(new Katana());
                    break;
                case 2:
                    AddItem(new Broadsword());
                    break;
                case 3:
                    AddItem(new Mace());
                    break;
                case 4:
                    AddItem(new Halberd());
                    break;
                case 5:
                    AddItem(new Shortblade());
                    break;
            }
            switch (Utility.Random(6))
            {
                case 0:
                    AddItem(new LeatherChest());
                    break;
                case 1:
                    AddItem(new LeatherArms());
                    break;
                case 2:
                    AddItem(new LeatherLegs());
                    break;
                case 3:
                    AddItem(new GargishLeatherChest());
                    break;
                case 4:
                    AddItem(new GargishLeatherArms());
                    break;
                case 5:
                    AddItem(new GargishLeatherLegs());
                    break;
            }

            AddItem(new LeatherTalons());
            AddItem(new Boomerang());
        }

        public DustyHuntersBackpack(Serial serial)
            : base(serial)
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

            var version = reader.ReadInt();
        }
    }
}