using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class Beninort : MondainQuester
    {
        [Constructable]
        public Beninort()
            : base("Beninort", "the Artificer")
        {
            SetSkill(SkillName.Imbuing, 60.0, 80.0);
        }

        public Beninort(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        {
            get
            {
                return new[]
                {
                    typeof (SecretsoftheSoulforge)
                };
            }
        }

        public override void InitBody()
        {
            HairItemID = 0x2044; //
            HairHue = 1153;
            FacialHairItemID = 0x204B;
            FacialHairHue = 1153;
            Body = 666;
            Blessed = true;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Boots());
            AddItem(new LongPants(0x6C7));
            AddItem(new FancyShirt(0x6BB));
            AddItem(new Cloak(0x59));
        }

        public override void Advertise()
        {
            Say(1112521); // Know the secrets. Learn of the soulforge.
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