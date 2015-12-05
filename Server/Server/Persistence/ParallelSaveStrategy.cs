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
//  [2014] ParallelSaveStrategy.cs
// ************************************/
#endregion

#region References
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using CustomsFramework;

using Server.Guilds;
#endregion

namespace Server
{
	public sealed class ParallelSaveStrategy : SaveStrategy
	{
		private readonly int processorCount;

		private readonly Queue<Item> _decayQueue;
		private SaveMetrics metrics;

		private SequentialFileWriter itemData, itemIndex;
		private SequentialFileWriter mobileData, mobileIndex;
		private SequentialFileWriter guildData, guildIndex;
		private SequentialFileWriter customData, customIndex;

		private Consumer[] consumers;
		private int cycle;

		private bool finished;

		public ParallelSaveStrategy(int processorCount)
		{
			this.processorCount = processorCount;

			_decayQueue = new Queue<Item>();
		}

		public override string Name { get { return "Parallel"; } }

		public override void Save(SaveMetrics m, bool permitBackgroundWrite)
		{
			metrics = m;

			OpenFiles();

			consumers = new Consumer[GetThreadCount()];

			for (int i = 0; i < consumers.Length; ++i)
			{
				consumers[i] = new Consumer(this, 256);
			}

			IEnumerable<ISerializable> collection = new Producer();

			foreach (ISerializable value in collection)
			{
				while (!Enqueue(value))
				{
					if (!Commit())
					{
						Thread.Sleep(0);
					}
				}
			}

			finished = true;

			SaveTypeDatabases();

			WaitHandle.WaitAll(Array.ConvertAll<Consumer, WaitHandle>(consumers, input => input.completionEvent));

			Commit();

			CloseFiles();
		}

		public override void ProcessDecay()
		{
			while (_decayQueue.Count > 0)
			{
				Item item = _decayQueue.Dequeue();

				if (item.OnDecay())
				{
					item.Delete();
				}
			}
		}

		private int GetThreadCount()
		{
			return processorCount - 1;
		}

		private void SaveTypeDatabases()
		{
			SaveTypeDatabase(World.ItemTypesPath, World.m_ItemTypes);
			SaveTypeDatabase(World.MobileTypesPath, World.m_MobileTypes);
			SaveTypeDatabase(World.DataTypesPath, World._DataTypes);
		}

		private void SaveTypeDatabase(string path, List<Type> types)
		{
			var bfw = new BinaryFileWriter(path, false);

			bfw.Write(types.Count);

			foreach (Type type in types)
			{
				bfw.Write(type.FullName);
			}

			bfw.Flush();

			bfw.Close();
		}

		private void OpenFiles()
		{
			itemData = new SequentialFileWriter(World.ItemDataPath, metrics);
			itemIndex = new SequentialFileWriter(World.ItemIndexPath, metrics);

			mobileData = new SequentialFileWriter(World.MobileDataPath, metrics);
			mobileIndex = new SequentialFileWriter(World.MobileIndexPath, metrics);

			guildData = new SequentialFileWriter(World.GuildDataPath, metrics);
			guildIndex = new SequentialFileWriter(World.GuildIndexPath, metrics);

			customData = new SequentialFileWriter(World.DataBinaryPath, metrics);
			customIndex = new SequentialFileWriter(World.DataIndexPath, metrics);

			WriteCount(itemIndex, World.Items.Count);
			WriteCount(mobileIndex, World.Mobiles.Count);
			WriteCount(guildIndex, BaseGuild.List.Count);
			WriteCount(customIndex, World.Data.Count);
		}

		private void WriteCount(SequentialFileWriter indexFile, int count)
		{
			var buffer = new byte[4];

			buffer[0] = (byte)(count);
			buffer[1] = (byte)(count >> 8);
			buffer[2] = (byte)(count >> 16);
			buffer[3] = (byte)(count >> 24);

			indexFile.Write(buffer, 0, buffer.Length);
		}

		private void CloseFiles()
		{
			itemData.Close();
			itemIndex.Close();

			mobileData.Close();
			mobileIndex.Close();

			guildData.Close();
			guildIndex.Close();

			customData.Close();
			customIndex.Close();

			World.NotifyDiskWriteComplete();
		}

		private void OnSerialized(ConsumableEntry entry)
		{
			ISerializable value = entry.value;
			BinaryMemoryWriter writer = entry.writer;

			var item = value as Item;

			if (item != null)
			{
				Save(item, writer);
			}
			else
			{
				var mob = value as Mobile;

				if (mob != null)
				{
					Save(mob, writer);
				}
				else
				{
					var guild = value as BaseGuild;

					if (guild != null)
					{
						Save(guild, writer);
					}
					else
					{
						var data = value as SaveData;

						if (data != null)
						{
							Save(data, writer);
						}
					}
				}
			}
		}

		private void Save(Item item, BinaryMemoryWriter writer)
		{
			int length = writer.CommitTo(itemData, itemIndex, item.m_TypeRef, item.Serial);

			if (metrics != null)
			{
				metrics.OnItemSaved(length);
			}

			if (item.Decays && item.Parent == null && item.Map != Map.Internal &&
				DateTime.UtcNow > (item.LastMoved + item.DecayTime))
			{
				_decayQueue.Enqueue(item);
			}
		}

		private void Save(Mobile mob, BinaryMemoryWriter writer)
		{
			int length = writer.CommitTo(mobileData, mobileIndex, mob._TypeRef, mob.Serial);

			if (metrics != null)
			{
				metrics.OnMobileSaved(length);
			}
		}

		private void Save(BaseGuild guild, BinaryMemoryWriter writer)
		{
			int length = writer.CommitTo(guildData, guildIndex, 0, guild.Id);

			if (metrics != null)
			{
				metrics.OnGuildSaved(length);
			}
		}

		private void Save(SaveData data, BinaryMemoryWriter writer)
		{
			int length = writer.CommitTo(customData, customIndex, data._TypeID, data.Serial);

			if (metrics != null)
			{
				metrics.OnDataSaved(length);
			}
		}

		private bool Enqueue(ISerializable value)
		{
			int count = consumers.Length;

			while (--count >= 0)
			{
				Consumer consumer = consumers[cycle++ % consumers.Length];

				if ((consumer.tail - consumer.head) >= consumer.buffer.Length)
				{
					continue;
				}

				consumer.buffer[consumer.tail % consumer.buffer.Length].value = value;
				consumer.tail++;

				return true;
			}

			return false;
		}

		private bool Commit()
		{
			bool committed = false;

			foreach (Consumer consumer in consumers)
			{
				while (consumer.head < consumer.done)
				{
					OnSerialized(consumer.buffer[consumer.head % consumer.buffer.Length]);
					consumer.head++;

					committed = true;
				}
			}

			return committed;
		}

		private struct ConsumableEntry
		{
			public ISerializable value;
			public BinaryMemoryWriter writer;
		}

		private sealed class Producer : IEnumerable<ISerializable>
		{
			private readonly IEnumerable<Item> items;
			private readonly IEnumerable<Mobile> mobiles;
			private readonly IEnumerable<BaseGuild> guilds;
			private readonly IEnumerable<SaveData> data;

			public Producer()
			{
				items = World.Items.Values;
				mobiles = World.Mobiles.Values;
				guilds = BaseGuild.List.Values;
				data = World.Data.Values;
			}

			public IEnumerator<ISerializable> GetEnumerator()
			{
				foreach (Item item in items)
				{
					yield return item;
				}

				foreach (Mobile mob in mobiles)
				{
					yield return mob;
				}

				foreach (BaseGuild guild in guilds)
				{
					yield return guild;
				}

				foreach (SaveData sd in data)
				{
					yield return sd;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}
		}

		private sealed class Consumer
		{
			public readonly ManualResetEvent completionEvent;
			public readonly ConsumableEntry[] buffer;

			public int head, done, tail;

			private readonly ParallelSaveStrategy owner;
			private readonly Thread thread;

			public Consumer(ParallelSaveStrategy owner, int bufferSize)
			{
				this.owner = owner;

				buffer = new ConsumableEntry[bufferSize];

				for (int i = 0; i < buffer.Length; ++i)
				{
					buffer[i].writer = new BinaryMemoryWriter();
				}

				completionEvent = new ManualResetEvent(false);

				thread = new Thread(Processor)
				{
					Name = "Parallel Serialization Thread"
				};

				thread.Start();
			}

			private void Processor()
			{
				try
				{
					while (!owner.finished)
					{
						Process();
						Thread.Sleep(0);
					}

					Process();

					completionEvent.Set();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}

			private void Process()
			{
				ConsumableEntry entry;

				while (done < tail)
				{
					entry = buffer[done % buffer.Length];

					entry.value.Serialize(entry.writer);

					++done;
				}
			}
		}
	}
}