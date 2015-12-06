using System;
using Server;

namespace Server.Items
{
	public class TavarasJournal1 : BaseBook
	{
		[Constructable]
		public TavarasJournal1()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 6, false )
		{
			Pages[0].Lines = new string[] { "Day One:", "", "The workers continue", "tirelessly in their", "efforts to unload our", "supplies even as light", "fades.  I feel I should", "lend a hand in the" };

			Pages[1].Lines = new string[] { "effort, and yet I", "cannot bear to take my", "attention away from", "the magnificent stone", "doors of the tomb.", "Every inch of their", "massive frame is", "covered with" };

			Pages[2].Lines = new string[] { "intricately carved", "design work - 'tis", "truly a sight to see.", "I've spent the day", "sketching and", "cataloging what I can", "of them while my", "companions set up our" };

			Pages[3].Lines = new string[] { "camp and make", "preparations for", "tomorrow's work.", "Though the stonework", "symbols inspire me to", "new flights of fancy,", "some of the workers", "seem strangely" };

			Pages[4].Lines = new string[] { "fearful of them.  I", "cannot wait 'til the", "morrow when those", "ancient works of stone", "shall swing open and", "deliver unto me", "everything I have", "dreamed of for the" };

			Pages[5].Lines = new string[] { "last ten years of my", "life." };
		}

		public TavarasJournal1( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal2 : BaseBook
	{
		[Constructable]
		public TavarasJournal2()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 9, false )
		{
			Pages[0].Lines = new string[] { "Day Two:", "", "Everything we'd", "heard and read of the", "tomb has proved", "correct - and yet,", "nothing could prepare", "me for the sight of it" };

			Pages[1].Lines = new string[] { "with my own eyes.", "The Tomb of Khal", "Ankur has given up", "its secrets at last!  The", "intricate stonework", "that covered the tomb", "doors seems to", "continue throughout" };

			Pages[2].Lines = new string[] { "the entirety of the", "catacombs, each", "hallway and room", "yielding a seemingly", "endless amount of", "information for my", "companions and I to", "record.  It will take" };

			Pages[3].Lines = new string[] { "years to catalogue the", "entirety of the Tomb,", "if those legends of its", "massive size prove", "true.  Sadly, a good", "deal of the Tomb's", "interior has been", "damaged or utterly" };

			Pages[4].Lines = new string[] { "destroyed, whether", "by seismic activity in", "the surrounding", "mountainside or", "merely the slow", "efforts of Time", "itself, I do not know.", "A good deal of the" };

			Pages[5].Lines = new string[] { "stonework has been", "cracked or collapsed", "entirely, especially", "near the entrance", "supports of the main", "hall.  Our passage has", "indeed already been", "entirely blocked in the" };

			Pages[6].Lines = new string[] { "first major room", "we've discovered, a", "massive pile of", "boulders and stones", "blocking any exit", "from the", "antechamber.  What", "could have caused" };

			Pages[7].Lines = new string[] { "such a localized", "disruption of the", "support structures,", "one can only guess -", "but it will surely take", "an entire afternoon's", "effort to remove even", "a fraction of it.  I look" };

			Pages[8].Lines = new string[] { "forward to more", "progress tomorrow", "once the workers have", "set to excavating the", "hall." };
		}

		public TavarasJournal2( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal3 : BaseBook
	{
		[Constructable]
		public TavarasJournal3()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 6, false )
		{
			Pages[0].Lines = new string[] { "Day Three - Day Five:", "", "I do not understand", "this place... not as I", "once thought I did.", "Something palatable", "seems to hinder our", "every attempt to" };

			Pages[1].Lines = new string[] { "investigate this", "ancient site.", "Excavation work on", "the first major", "hallway finished only", "yesterday - the", "amount of stone and", "rubble blocking the" };

			Pages[2].Lines = new string[] { "egress was", "astounding, it stands", "in immense piles", "outside the Tomb's", "entrance, as if we", "were digging the", "tunnels of this", "abhorred place" };

			Pages[3].Lines = new string[] { "ourselves!  The", "satisfaction of", "completing our efforts", "was quickly thwarted,", "however, as we", "discovered the end of", "the hallway we had", "just revealed was" };

			Pages[4].Lines = new string[] { "blocked by yet another", "colossal pile of stone.", "I've had a few of the", "workers set up", "primitive scaffolding", "in the main", "antechamber so that I", "can spend my time" };

			Pages[5].Lines = new string[] { "pouring over the detail", "work on the stone", "carvings while the", "rest of our crew", "continue excavating", "the inner halls." };
		}

		public TavarasJournal3( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal6 : BaseBook
	{
		[Constructable]
		public TavarasJournal6()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 7, false )
		{
			Pages[0].Lines = new string[] { "Day Six:", "", "Late last night our", "camp was set upon by", "a pack of wild beasts", "- behemoth creatures", "with a speed and", "viciousness I'd n'ere" };

			Pages[1].Lines = new string[] { "before seen.  Even", "Grimmoch, well", "versed in all manner", "of wildlife, was", "unsure as to their", "nature - though I lay", "blame upon the", "darkness covering" };

			Pages[2].Lines = new string[] { "their movements", "rather than on his", "skill as a huntsman.", "The attacks did not let", "up the entire night,", "and we were", "eventually forced to", "flee into the Tomb" };

			Pages[3].Lines = new string[] { "itself to take refuge", "from the ravenous", "creatures - e'en", "Lysander's spells", "could not keep the foul", "things from attacking", "in great numbers.", "The Tomb performed" };

			Pages[4].Lines = new string[] { "well as an impromptu", "fortress, and we", "managed to spend the", "night unscathed.", "Morning's light", "seemed to have", "scattered the beasts,", "as not a single one of" };

			Pages[5].Lines = new string[] { "them was to be seen as", "we exited the Tomb -", "not even a carcass of", "the few that were", "slain a'fore we fled.", "Lysander set the crew", "to work, moving our", "supplies and gear into" };

			Pages[6].Lines = new string[] { "the Tomb, in case the", "creatures did opt to", "return.  Such savage", "fury had the beasts -", "and not a single one", "ever turned to run,", "even in the face of", "certain death." };
		}

		public TavarasJournal6( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal7 : BaseBook
	{
		[Constructable]
		public TavarasJournal7()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 6, false )
		{
			Pages[0].Lines = new string[] { "Day Seven:", "", "T'was written that,", "upon his death, Khal", "Ankur's followers,", "those known as the", "Keepers of the", "Seventh Death, sealed" };

			Pages[1].Lines = new string[] { "themselves within the", "Sanctum they had", "carved from the", "mountains in his", "honor.  The Zealots of", "his order entombed", "the lesser followers", "alive, then, when all" };

			Pages[2].Lines = new string[] { "but two remained, slit", "their throats and", "joined Khal Ankur in", "death.  Surely this is", "not surprising for a", "Cult that worshipped", "death and sacrifice so", "vehemently as it is" };

			Pages[3].Lines = new string[] { "said that the Keepers", "did - and yet, to be in", "this Tomb, to know", "that somewhere in its", "depths hundreds upon", "hundreds of bodies", "lay, sealed alive at", "their own behest..." };

			Pages[4].Lines = new string[] { "I must confess that", "the very thought of it", "troubles my dreams at", "night.  I've asked", "Lysander if we might", "reestablish the camp", "outside the Tomb,", "setting up night" };

			Pages[5].Lines = new string[] { "watches and some sort", "of fortification, but", "he'll have none of it.  I", "did not press the", "issue, as I suddenly", "felt foolish even at", "my askance." };
		}

		public TavarasJournal7( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal8 : BaseBook
	{
		[Constructable]
		public TavarasJournal8()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 8, false )
		{
			Pages[0].Lines = new string[] { "Day Eight :", "", "Astounding progress", "was made today, and", "my very head spins", "with the excitement", "of it.  Upon full", "excavation of the far" };

			Pages[1].Lines = new string[] { "western hall, another", "large antechamber", "was revealed.  By the", "larger, mosaic style of", "the wall carvings and", "their framing, as well", "as the numerous", "vellum scrolls and" };

			Pages[2].Lines = new string[] { "tomes held within, the", "room appears to have", "been a great museum", "or library of sorts.", "The sheer amount of", "written information", "encased within this", "room would surely" };

			Pages[3].Lines = new string[] { "take me decades to", "study e'en if I could", "immediately decipher", "the strange text with", "which it was written.", "My sheer joy at the", "discovery was quickly", "noted by the brute" };

			Pages[4].Lines = new string[] { "known as Morg", "Bergen, who, even in", "his simple way,", "seemed just as", "delighted as I that", "some progress had", "been made.  I must", "confess, upon his" };

			Pages[5].Lines = new string[] { "inclusion in our party", "at the beginning of", "this journey I was", "somewhat suspect of", "his nature, but he has", "a startlingly quick wit", "about him for such a", "massive, calloused" };

			Pages[6].Lines = new string[] { "warrior.  While", "Lysander and e'en", "Grimmoch always", "seem to investigate the", "tomb with a scowling", "determination, Bergen", "seems to feel the same", "thrill of discovery as" };

			Pages[7].Lines = new string[] { "I.  I am proud to now", "count him as a friend,", "and am thankful for", "his laughter as well", "as his strength." };
		}

		public TavarasJournal8( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal9 : BaseBook
	{
		[Constructable]
		public TavarasJournal9()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 6, false )
		{
			Pages[0].Lines = new string[] { "Day Nine - Day Ten:", "", "The excavation of the", "next set of tunnels", "has ceased, as three", "of the workers have", "gone missing in the", "night.  Bergen voiced" };

			Pages[1].Lines = new string[] { "the opinion that they", "had most likely", "abandoned our group", "altogether and headed", "back, as they were of", "the number that", "seemed especially", "disturbed by the" };

			Pages[2].Lines = new string[] { "Tomb.  Lysander had", "other ideas, however.", "In the middle of our", "discussion on the", "matter, he went into a", "wild tirade on the", "possibility that they", "had somehow" };

			Pages[3].Lines = new string[] { "infiltrated the tomb's", "interior without us.", "The pure, hateful", "venom in his voice", "when he spoke of the", "workers shocked me,", "as I had always", "thought him to be a" };

			Pages[4].Lines = new string[] { "levelheaded man of", "great learning.  As we", "are still at work", "digging out the rubble", "that blocks all access", "to the inner chambers,", "I cannot help but", "believe the workers" };

			Pages[5].Lines = new string[] { "must have fled the", "site altogether, as", "Bergen said." };
		}

		public TavarasJournal9( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal11 : BaseBook
	{
		[Constructable]
		public TavarasJournal11()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 8, false )
		{
			Pages[0].Lines = new string[] { "Day Eleven - Day", "Thirteen:", "", "Two more workers", "have gone missing.", "Even more disturbing", "is the fact that", "Lysander has joined" };

			Pages[1].Lines = new string[] { "them. Late last night", "the workers finished", "excavating the next", "main hall, and we", "retired to the main", "antechamber and our", "camp to rest up for", "exploration on the" };

			Pages[2].Lines = new string[] { "'morrow.  In the", "middle of the night we", "woke to a strange", "howling sound, and as", "the men prepared", "themselves for", "another onslaught of", "the beasts that had" };

			Pages[3].Lines = new string[] { "troubled our outer", "camp, it was noticed", "that Lysander was", "nowhere in our", "number.  I cannot", "fathom where he has", "gone - the newly", "revealed chamber" };

			Pages[4].Lines = new string[] { "holds no immediate", "egress, blocked again", "by piles of stone and", "rubble, and I cannot", "believe that Lysander,", "of all people, would", "have fled this site -", "indeed, he had lately" };

			Pages[5].Lines = new string[] { "grown almost fanatical", "in his work to", "discover more of the", "secrets barred to us", "by the consistently", "slow progress of", "excavating each new", "hallway.  The men are" };

			Pages[6].Lines = new string[] { "at work even now, and", "as the ceaseless", "thumps and cracks of", "their picks", "reverberate", "throughout the", "entirety of the tomb,", "the dust continues to" };

			Pages[7].Lines = new string[] { "pour down from the", "ancient stonework", "above us like some", "horrible, eldritch", "curse upon us all." };
		}

		public TavarasJournal11( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal14 : BaseBook
	{
		[Constructable]
		public TavarasJournal14()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 7, false )
		{
			Pages[0].Lines = new string[] { "Day Fourteen - Day", "Fifteen:", "", "Lysander has", "returned... and yet,", "how can I describe the", "horror of it?  He", "stands across the" };

			Pages[1].Lines = new string[] { "chamber from me", "even now, a changed", "man.  His hair hangs", "in grimy knots across", "his face, his clothes", "filthy and torn in", "places... and the blood", "- covered in blood, his" };

			Pages[2].Lines = new string[] { "skin shining in", "scarlet reflections of", "the torchlight.  He", "will let no one", "approach; a thick,", "rusted dagger in his", "hand warding off any", "attempts to overcome" };

			Pages[3].Lines = new string[] { "him.  And the blood,", "which runs down in", "great rivulets from", "his arms and hands -", "it is not his own, and", "this is enough to keep", "us at a wary distance.", "Morg Bergen wishes" };

			Pages[4].Lines = new string[] { "to subdue him", "quickly, but there is", "something in", "Lysander's eyes - and", "I remember the power", "of his spells, even as", "he swings the jagged", "dagger back and forth" };

			Pages[5].Lines = new string[] { "in a wide swath", "before him.", "Something about the", "sight of it makes my", "stomach churn.", "Something has", "happened, something", "that changes" };

			Pages[6].Lines = new string[] { "everything.  Lysander", "has lost his sanity to", "this tomb... or to", "something within it.", "Do we dare approach?", "We must make a", "decision soon." };
		}

		public TavarasJournal14( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal16 : BaseBook
	{
		[Constructable]
		public TavarasJournal16()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 8, false )
		{
			Pages[0].Lines = new string[] { "Day Sixteen:", "", "Why do I write?  I", "must... not so much", "because there must be", "some record of", "this... what's", "happened here... as" };

			Pages[1].Lines = new string[] { "for my own sanity.", "The act of putting pen", "to paper calms me,", "focuses me, even in", "this madness.", "Lysander is dead.  So", "many are dead.  And", "we're trapped here," };

			Pages[2].Lines = new string[] { "trapped forever in", "this nightmare.  He", "would not let us pass,", "wild in his psychosis,", "furious, spitting,", "covered in blood, he", "swung the ancient", "dagger at any who" };

			Pages[3].Lines = new string[] { "approached.  He", "babbled incoherently,", "cursed at us, the most", "hateful curses,", "prophecy, doom upon", "us.  Bergen would", "have none of it.", "Finally, he leapt at" };

			Pages[4].Lines = new string[] { "Lysander, his", "massive axe at his", "side.  But he would not", "be the end of the mad", "mage... no... they", "were... those hands,", "covered in the dirt of", "the grave, maggots," };

			Pages[5].Lines = new string[] { "filth.  They rose up", "behind Lysander.", "That look of curiosity", "on the mage's face as", "Bergen skidded to a", "halt... t'was almost a", "moment of sanity for", "him, surely, to" };

			Pages[6].Lines = new string[] { "attempt to comprehend", "what could have", "stopped the warrior in", "his tracks.  And then", "they were upon him.", "Skeletal hands, arms", "and faces with loose,", "corrupted flesh" };

			Pages[7].Lines = new string[] { "hanging from yellow", "bone.  Inhuman, yet", "once human,", "staggering towards us", "as their companions", "tore at Lysander,", "coming towards us in", "droves." };
		}

		public TavarasJournal16( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal16b : BaseBook
	{
		[Constructable]
		public TavarasJournal16b()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 7, false )
		{
			Pages[0].Lines = new string[] { "Day Sixteen, Later :", "", "We ran. What could", "we do?  We ran back", "towards the entrance,", "cutting at them when", "we could.  T'was a", "nightmare, and yet" };

			Pages[1].Lines = new string[] { "nothing to prepare us", "for what would come.", "We were almost there,", "the entrance to this", "abhorred crypt in", "sight.  Then the earth", "shook with such a", "force that we were" };

			Pages[2].Lines = new string[] { "dropped to our hands", "and knees, stumbling", "in the darkness with", "those... those things", "surely behind us.", "The noise of falling", "rock and crumbling", "stone drowned out our" };

			Pages[3].Lines = new string[] { "piteous cries.  No sign", "of the entrance", "remained.", "We owe our lives to", "Bergen, whose wits", "returned quickly.", "That he could make us", "hurry back into the" };

			Pages[4].Lines = new string[] { "main antechamber...", "actually run back", "towards those eldritch", "dead that stalked us.", "But we did, the", "strength of his", "convictions enough for", "us in the moment." };

			Pages[5].Lines = new string[] { "And at our campsite", "we erected our last", "defense, a pitiable", "wall of wood and", "stone, anything at", "hand that might block", "the tide of those", "nightmare creatures." };

			Pages[6].Lines = new string[] { "And I sit against it", "even now.  I can hear", "their moans, their", "wailing cries in the", "distance - they'll be", "here soon, even at the", "unhurried pace of the", "shuffling dead." };
		}

		public TavarasJournal16b( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal17 : BaseBook
	{
		[Constructable]
		public TavarasJournal17()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 4, false )
		{
			Pages[0].Lines = new string[] { "Day Seventeen - Day", "Eighteen :", "", "I cannot go on much", "longer.  I know now", "t'was no work of the", "earth that trapped us", "here - I can feel His" };

			Pages[1].Lines = new string[] { "force in it.  It was His", "will, His power that", "has sealed us here in", "this nightmare.  The", "barricade will not be", "enough.  So many of", "them. They come like", "unto the ocean's waves" };

			Pages[2].Lines = new string[] { "- ceaseless,", "neverending.  For", "every five we strike", "down, another ten rise", "up against us.  And", "like the sands we", "cannot help but be", "brought down, wasted" };

			Pages[3].Lines = new string[] { "away in this ocean of", "blood." };
		}

		public TavarasJournal17( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}

	public class TavarasJournal19 : BaseBook
	{
		[Constructable]
		public TavarasJournal19()
			: base( Utility.Random( 0xFF1, 2 ), "Journal: Discovery of the Tomb", "Tavara Sewel", 3, false )
		{
			Pages[0].Lines = new string[] { "Day Nineteen - Day", "Twenty-One :", "", "The barricade won't", "hold - never, and", "they'll come, they", "come even now.  I", "would tear the last of" };

			Pages[1].Lines = new string[] { "it down, let them in to", "devour us all, if only", "to stop the screaming", "- the awful, wailing", "cries that fill the tomb", "with their presence.", "May my ancestors", "forgive me, but it" };

			Pages[2].Lines = new string[] { "must be done. I must", "end this." };
		}

		public TavarasJournal19( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}
}
