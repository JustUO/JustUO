/*                                                             .---.
/  .  \
|\_/|   |
|   |  /|
.----------------------------------------------------------------' |
/  .-.                                                              |
|  /   \         Contribute To The Orbsydia SA Project               |
| |\_.  |                                                            |
|\|  | /|                        By Lotar84                          |
| `---' |                                                            |
|       |       (Orbanised by Orb SA Core Development Team)          | 
|       |                                                           /
|       |----------------------------------------------------------'
\       |
\     /
`---'
*/

namespace Server.Items
{
    public class EssenceBox : WoodenBox
    {
        [Constructable]
        public EssenceBox()
        {
            Movable = true;
            Hue = 1161;

            switch (Utility.Random(42))
            {
                case 0:
                    DropItem(new EssencePrecision());
                    break;
                case 1:
                    DropItem(new EssenceAchievement());
                    break;
                case 2:
                    DropItem(new EssenceBalance());
                    break;
                case 3:
                    DropItem(new EssenceControl());
                    break;
                case 4:
                    DropItem(new EssenceDiligence());
                    break;
                case 5:
                    DropItem(new EssenceDirection());
                    break;
                case 6:
                    DropItem(new EssenceFeeling());
                    break;
                case 7:
                    DropItem(new EssenceOrder());
                    break;
                case 8:
                    DropItem(new EssencePassion());
                    break;
                case 9:
                    DropItem(new EssencePersistence());
                    break;
                case 10:
                    DropItem(new EssenceSingularity());
                    break;
                case 11:
                    DropItem(new FeyWings());
                    break;
                case 12:
                    DropItem(new FaeryDust());
                    break;
                case 13:
                    DropItem(new Fur());
                    break;
                case 14:
                    DropItem(new GoblinBlood());
                    break;
                case 15:
                    DropItem(new HornAbyssalInferno());
                    break;
                case 16:
                    DropItem(new KepetchWax());
                    break;
                case 17:
                    DropItem(new LavaSerpenCrust());
                    break;
                case 18:
                    DropItem(new MedusaBlood());
                    break;
                case 19:
                    DropItem(new PowderedIron());
                    break;
                case 20:
                    DropItem(new PrimalLichDust());
                    break;
                case 21:
                    DropItem(new RaptorTeeth());
                    break;
                case 22:
                    DropItem(new ReflectiveWolfEye());
                    break;
                case 23:
                    DropItem(new SeedRenewal());
                    break;
                case 24:
                    DropItem(new SilverSerpentVenom());
                    break;
                case 25:
                    DropItem(new SilverSnakeSkin());
                    break;
                case 26:
                    DropItem(new SlithEye());
                    break;
                case 27:
                    DropItem(new SlithTongue());
                    break;
                case 28:
                    DropItem(new SpiderCarapace());
                    break;
                case 29:
                    DropItem(new ScouringToxin());
                    break;
                case 30:
                    DropItem(new ToxicVenomSac());
                    break;
                case 31:
                    DropItem(new UndyingFlesh());
                    break;
                case 32:
                    DropItem(new VialVitirol());
                    break;
                case 33:
                    DropItem(new DelicateScales());
                    break;
                case 34:
                    DropItem(new VoidCore());
                    break;
                case 35:
                    DropItem(new VoidEssence());
                    break;
                case 36:
                    DropItem(new BottleIchor());
                    break;
                case 37:
                    DropItem(new ChagaMushroom());
                    break;
                case 38:
                    DropItem(new CrushedGlass());
                    break;
                case 39:
                    DropItem(new CrystalShards());
                    break;
                case 40:
                    DropItem(new CrystallineBlackrock());
                    break;
                case 41:
                    DropItem(new DaemonClaw());
                    break;
            }
        }

        public EssenceBox(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1113770; }
        } //Essence Box

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