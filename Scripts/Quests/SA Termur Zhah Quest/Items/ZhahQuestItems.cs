namespace Server.Items
{
    public class TheChallengeRite : Item
    {
        [Constructable]
        public TheChallengeRite()
            : base(0x0FF2)
        {
            Hue = 447;
        }

        public TheChallengeRite(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1150904; }
        } //The Challenge Rite

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

    public class OnTheVoid : Item
    {
        [Constructable]
        public OnTheVoid()
            : base(0x0FF2)
        {
            Hue = 447;
        }

        public OnTheVoid(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1150907; }
        } //On the Void

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

    public class InMemory : Item
    {
        [Constructable]
        public InMemory()
            : base(0x0FF2)
        {
            Hue = 447;
        }

        public InMemory(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1150913; }
        } //In Memory

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

    public class AthenaeumDecree : Item
    {
        [Constructable]
        public AthenaeumDecree()
            : base(0x14ED)
        {
            Hue = 447;
        }

        public AthenaeumDecree(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1150905; }
        } //Athenaeum Decree

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

    public class ALetterFromTheKing : Item
    {
        [Constructable]
        public ALetterFromTheKing()
            : base(0x14ED)
        {
            Hue = 447;
        }

        public ALetterFromTheKing(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1150906; }
        } //A Letter from the King

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

    public class ShilaxrinarsMemorial : Item
    {
        [Constructable]
        public ShilaxrinarsMemorial()
            : base(0x14ED)
        {
            Hue = 447;
        }

        public ShilaxrinarsMemorial(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1150908; }
        } //Shilaxrinar's Memorial

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

    public class ToTheHighScholar : Item
    {
        [Constructable]
        public ToTheHighScholar()
            : base(0x14ED)
        {
            Hue = 447;
        }

        public ToTheHighScholar(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1150909; }
        } //To the High Scholar

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

    public class ToTheHighBroodmother : Item
    {
        [Constructable]
        public ToTheHighBroodmother()
            : base(0x14ED)
        {
            Hue = 447;
        }

        public ToTheHighBroodmother(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1150910; }
        } //To the High Broodmother

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

    public class ReplyToTheHighScholar : Item
    {
        [Constructable]
        public ReplyToTheHighScholar()
            : base(0x14ED)
        {
            Hue = 447;
        }

        public ReplyToTheHighScholar(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1150911; }
        } //Reply to the High Scholar

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

    public class AccessToTheIsle : Item
    {
        [Constructable]
        public AccessToTheIsle()
            : base(0x14ED)
        {
            Hue = 447;
        }

        public AccessToTheIsle(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1150912; }
        } //Access to the Isle

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