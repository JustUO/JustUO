#region Header
// **************************************\
//     _  _   _   __  ___  _   _   ___   |
//    |# |#  |#  |## |### |#  |#  |###   |
//    |# |#  |# |#    |#  |#  |# |#  |#  |
//    |# |#  |#  |#   |#  |#  |# |#  |#  |
//   _|# |#__|#  _|#  |#  |#__|# |#__|#  |
//  |##   |##   |##   |#   |##    |###   |
//        [http://www.playuo.org]        |
// **************************************/
//  [2014] SaveStrategy.cs
// ************************************/
#endregion

namespace Server
{
	public abstract class SaveStrategy
	{
		public abstract string Name { get; }

		public static SaveStrategy Acquire()
		{
			if (Core.MultiProcessor)
			{
				int processorCount = Core.ProcessorCount;

#if DynamicSaveStrategy
                if (processorCount > 2)
                {
                    return new DualSaveStrategy(); // new DynamicSaveStrategy(); // TODO: Stabilize
                }
#else
				if (processorCount > 16)
				{
					return new DualSaveStrategy(); // new ParallelSaveStrategy(processorCount); // TODO: Stabilize
				}
#endif
				return new DualSaveStrategy();
			}
			
			return new StandardSaveStrategy();
		}

		public abstract void Save(SaveMetrics metrics, bool permitBackgroundWrite);

		public abstract void ProcessDecay();
	}
}