using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Demystify
{
	public class AdvancedLogData
	{
		public int MaxSize;
		public int Length => Data?.Count ?? 0;

		public ILogData this[int index] => Data[index];
		public List<ILogData> Data;

		public float MinValue { get; private set; } = float.MaxValue;
		public float MaxValue { get; private set; } = float.MinValue;
		
		public void GetFloatData(List<float> floats, out float min, out float max, int index = 0)
		{
			min = float.MaxValue;
			max = float.MinValue;
			if (Data == null) return;
			var lastFrame = -1;
			var currentIndex = 0;
			for (var i = 0; i < Data.Count; i++)
			{
				var entry = Data[i];
				if (lastFrame < 0 || currentIndex == index)
				{
					if (entry is LogData<float> vl)
					{
						min = Mathf.Min(vl.Value, min);
						max = Mathf.Max(vl.Value, max);
						floats.Add(vl.Value);
					}
				}

				if (entry.Frame == lastFrame) currentIndex += 1;
				else currentIndex = 0;
				lastFrame = entry.Frame;
			}
		}

		public AdvancedLogData() : this(300)
		{
		}

		public AdvancedLogData(int maxSize)
		{
			this.MaxSize = maxSize;
			this.Data = new List<ILogData>(maxSize);
		}

		public void AddData(float vl)
		{
			MinValue = Mathf.Min(vl, MinValue);
			MaxValue = Mathf.Max(vl, MaxValue);
			if (Data.Count + 1 > MaxSize) Data.RemoveAt(0);
			Data.Add(new LogData<float>
			{
				Value = vl,
				Frame = Time.frameCount
			});
		}
	}

	public interface ILogData
	{
		int Frame { get; set; }
		Type ValueType();
	}

	public struct LogData<T> : ILogData
	{
		public T Value;
		public int Frame { get; set; }
		
		public Type ValueType()
		{
			return typeof(T);
		}
	}
}