using System;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class DataContainer2D
	{
		private List<KeyValuePair<double, double>> values;
		
		public DataContainer2D ()
		{
			this.values = new List<KeyValuePair<double, double>>();
		}
		
		public void Add(double newKey, double newValue)
		{
			this.values.Add(new KeyValuePair<double, double>(newKey, newValue));
		}
				
		// Getter
		public double GetValueAt(double at)
		{			
			// Get closest key-value-pair
			KeyValuePair<double, double> prev;
			KeyValuePair<double, double> next;
			GetPairsAround(at, out prev, out next);
			
			// Interpolate (linear)
			double percentage = (at - prev.Key) / (next.Key - prev.Key);						
			return prev.Value + (percentage * (next.Value - prev.Value));
		}
		
		// Utility:		
		private void GetPairsAround(double key, out KeyValuePair<double, double> prev, out KeyValuePair<double, double> next)
		{
			prev = values[0];
			next = values[1];
			
			if (values.Count > 2)
			{
				double bestDifference = key - values[1].Key;
				double difference;
				
				for (int i=2; i < values.Count; i++)
				{
					difference = key - values[i].Key;
					if (Math.Abs(difference) < bestDifference)
					{
						if ((difference > 0) && (i+1 < values.Count))
						{
							prev = values[i];
							next = values[i+1];
						}
						else
						{
							prev = values[i-1];
							next = values[i];
						}
						bestDifference = Math.Abs(difference);
					}
					if (difference < 0)
						break;
				}
			}
		}
	}
}

