// Code of Milan Rüll
using System;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class RoundData
	{
		// Variables
		private List<List<float>> averageConsumptionMemory;
		
		private List<float> passedTime;
		private bool measureConsumption;
		
		private int currentRound;
			
		// Methods
		public RoundData ()
		{
			this.averageConsumptionMemory = new List<List<float>>();
			passedTime = new List<float>();
			Reset ();
		}
		
		public void Update(float currentConsumption, float passedTime)
		{
			this.passedTime[currentRound-1] += passedTime;
			UpdateConsumption(currentConsumption);
		}
		
		public void Reset()
		{
			this.currentRound = 1;
			
			this.passedTime.Clear();
			this.passedTime.Add(0);
			
			this.averageConsumptionMemory.Clear();
			this.averageConsumptionMemory.Add(new List<float>());
		}
		
		public void NextRound()
		{
			this.currentRound++;
			
			this.passedTime.Add(0);
			this.averageConsumptionMemory.Add(new List<float>());
		}
		
		public float GetAverageConsumption(int round)
		{
			if (averageConsumptionMemory[round-1].Count > 0)
			{
				float totalConsumption = 0.0f;
				foreach (float f in averageConsumptionMemory[round-1])
					totalConsumption += f;
				
				float averageConsumption = (totalConsumption / (float)averageConsumptionMemory[round-1].Count);
				
				return averageConsumption;
			}
			else
				return 0;
		}
		
		public float GetNeededTime(int round)
		{
			return this.passedTime[round-1];
		}
		
		private void UpdateConsumption(float currentConsumption)
		{
			this.averageConsumptionMemory[currentRound-1].Add(currentConsumption);
		}
	}
}

