using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class TheForbiddenFruit : BaseQuest
    {
        public TheForbiddenFruit()
        {
            AddObjective(new ObtainObjective(typeof (BouraSkin), "BouraSkin", 5, 0x11f4));
            AddObjective(new ObtainObjective(typeof (TreefellowWood), "TreefellowWood", 10, 0x1BDD));
            AddObjective(new ObtainObjective(typeof (Dough), "Dough", 1, 0x103D));

            AddReward(new BaseReward(typeof (IrresistiblyTastyTreat), "Irresistibly Tasty Treat"));
        }

        public override object Title
        {
            get { return "The Forbidden Fruit"; }
        }

        public override TimeSpan RestartDelay
        {
            get { return TimeSpan.FromDays(1); }
        }

        public override object Description
        {
            get { return 1112979; }
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
            get { return 1112982; }
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