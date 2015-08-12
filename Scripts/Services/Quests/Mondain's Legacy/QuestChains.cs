using System;

namespace Server.Engines.Quests
{
    public enum QuestChain
    {
        None,
        Aemaeth,
        AncientWorld,
        BlightedGrove,
        CovetousGhost,
        GemkeeperWarriors,
        HonestBeggar,
        LibraryFriends,
        Marauders,
        MiniBoss,
        SummonFey,
        SummonFiend,
        TuitionReimbursement,
        Spellweaving,
        SpellweavingS,
        UnfadingMemories,
        PercolemTheHunter,
        KingVernixQuests,
        DoughtyWarriors,
        HonorOfDeBoors,
        LaifemTheWeaver
    }

    public class BaseChain
    {
        public BaseChain(Type currentQuest, Type quester)
        {
            CurrentQuest = currentQuest;
            Quester = quester;
        }

        public Type CurrentQuest { get; set; }
        public Type Quester { get; set; }
    }
}