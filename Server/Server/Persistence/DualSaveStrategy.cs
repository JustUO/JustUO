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
//  [2014] DualSaveStrategy.cs
// ************************************/
#endregion

#region References
using System.Threading;
#endregion

namespace Server
{
	public sealed class DualSaveStrategy : StandardSaveStrategy
	{
		public override string Name { get { return "Dual"; } }

		public override void Save(SaveMetrics metrics, bool permitBackgroundWrite)
		{
			PermitBackgroundWrite = permitBackgroundWrite;

			var saveThread = new Thread(() => SaveItems(metrics)) {
				Name = "Item Save Subset"
			};

			saveThread.Start();

			SaveMobiles(metrics);
			SaveGuilds(metrics);
			SaveData(metrics);

			saveThread.Join();

			if (permitBackgroundWrite && UseSequentialWriters)
			{
				//If we're permitted to write in the background, but we don't anyways, then notify.
				World.NotifyDiskWriteComplete();
			}
		}
	}
}