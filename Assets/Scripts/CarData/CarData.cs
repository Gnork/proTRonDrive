using System;
using System.IO;
using System.Collections;

namespace AssemblyCSharp
{	
	public class CarData
	{
		// Variables
		private DataContainer2D[] tmNeed;
		private DataContainer2D fullLoadCurve;
		private double[] gearTranslations;
		private double finalDrive;
		
		private CarDataFile dataFile;
		
		// Properties
		public double[] GearTranslations
		{
			get { return this.gearTranslations; }
		}
		
		public double FinalDrive
		{
			get { return this.finalDrive; }
		}
		
		// Methods
		public CarData (String file)
		{
			this.LoadCarDataFile(file);
		}
		
		public void LoadCarDataFile(String file)
		{
			String errorMessage = "";
			bool result;
			
			result = LoadDataFromFile(file, ref errorMessage);
			if (!result)
			{
				throw new FileLoadException(errorMessage);
			}
			
			result = LoadGearData(ref errorMessage);
			if (!result)
			{
				throw new FileLoadException(errorMessage);
			}	
			
			result = LoadFLC(ref errorMessage);
			if (!result)
			{
				throw new FileLoadException(errorMessage);
			}	
			
			result = LoadTMNeeds(ref errorMessage);
			if (!result)
			{
				throw new FileLoadException(errorMessage);
			}				
		}
		
		private bool LoadDataFromFile(String file, ref String errorMessage)
		{			
			if (File.Exists(file))
			{
				StreamReader reader = File.OpenText(file);	
				
				ArrayList fileLines = new ArrayList();
				String readBuffer;
				
				while ((readBuffer = reader.ReadLine()) != null)
				{
					fileLines.Add(readBuffer);
				}
				reader.Close();
				
				dataFile = new CarDataFile(fileLines.ToArray());							
				return true;
			}
			else
			{
				errorMessage = "Car data file not found. ("+file+")";
				return false;
			}
		}
		
		private bool LoadGearData(ref String errorMessage)
		{
			if (!dataFile.GetDataByName("FinalDrive", out this.finalDrive, ref errorMessage))
				return false;
			
			if (!dataFile.GetDataByName("GearTranslations", out this.gearTranslations, ref errorMessage))
				return false;
				
			return true;
		}
		
		private bool LoadTMNeeds(ref String errorMessage)
		{
			double[] tmNeedValues;
			double[] RPM;
			
			if (!dataFile.GetDataByName("RPM", out RPM, ref errorMessage))
				return false;
			
			tmNeed = new DataContainer2D[this.gearTranslations.Length];
			
			for (int currentGear=0; currentGear < this.gearTranslations.Length; currentGear++)
			{
				if (!dataFile.GetDataByName("TM"+currentGear.ToString(), out tmNeedValues, ref errorMessage))
					return false;
				
				tmNeed[currentGear] = new DataContainer2D();
				for (int i=0; i < tmNeedValues.Length; i++)
				{
					tmNeed[currentGear].Add(RPM[i], tmNeedValues[i]);
				}
			}
			
			return true;
		}
		
		private bool LoadFLC(ref String errorMessage)
		{			
			double currentRPM, stepSize;
			double[] FLCValues;
			
			fullLoadCurve = new DataContainer2D();
			
			if (!dataFile.GetDataByName("FLC-BeginRPM", out currentRPM, ref errorMessage))
				return false;
			
			if (!dataFile.GetDataByName("FLC-RPMStep", out stepSize, ref errorMessage))
				return false;
			
			if (dataFile.GetDataByName("FLC-TurningMoments", out FLCValues, ref errorMessage))
			{
				foreach (Double d in FLCValues)
				{
					fullLoadCurve.Add(currentRPM, d);
					currentRPM += stepSize;
				}
			}
			else
			{
				return false;
			}
			
			return true;
		}
								
		// Getter
		public double GetMaxTurningMoment(double RPM)
		{
			return this.fullLoadCurve.GetValueAt(RPM);
		}
		
		public double GetNeededTurningMoment(double RPM, int currentGear)
		{
			return this.tmNeed[currentGear].GetValueAt(RPM);
		}
	}
}

