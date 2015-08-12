using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests.Ninja
{
    public class EnshroudedFigure : BaseQuester
    {
        [Constructable]
        public EnshroudedFigure()
        {
        }

        public EnshroudedFigure(Serial serial)
            : base(serial)
        {
        }

        public override int TalkNumber
        {
            get { return -1; }
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Hue = 0x8401;
            Female = false;
            Body = 0x190;
            Name = "enshrouded figure";
        }

        public override void InitOutfit()
        {
            AddItem(new DeathShroud());
            AddItem(new ThighBoots());
        }

        public override void OnTalk(PlayerMobile player, bool contextMenu)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadEncodedInt();
        }
    }
}