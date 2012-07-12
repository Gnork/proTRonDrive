using System;

namespace AssemblyCSharp
{
	public class CarDataFile
	{
		private const char idChar = '#';
		
		private String[] lines;
		
		public CarDataFile (String[] fileContent)
		{
			this.lines = fileContent;
		}
		
		public CarDataFile (Object[] fileContent)
		{
			this.lines = new string[fileContent.Length];
			for (int i=0; i < fileContent.Length; i++)
			{
				this.lines[i] = (String) fileContent[i];
			}
		}
		
		// Get multiple double values as data (as many as there are in the line)
		public bool GetDataByName(String identifier, out double[] data, ref String errorMessage)
		{
			String[] dataAsString;
			int startIndex;
			
			foreach (String line in lines)
			{
				if (line.Contains(idChar + identifier + idChar))
				{
					startIndex = line.LastIndexOf(idChar) + 1;
					dataAsString = (line.Substring(startIndex)).Split(',');
					if (!Utility.StringsAsDoubles(dataAsString, out data))
					{
						errorMessage = "Could find, but not read values with identifier \""+ identifier + "\".";
						return false;
					}
					return true;
				}
			}
			
			data = new double[0];
			errorMessage = "Could not find any data with identifier \"" + identifier + "\".";
			return false;
		}
		
		// Get a single double value (more than one value in that line produces an exception)
		public bool GetDataByName(String identifier, out double data, ref String errorMessage)
		{
			data = -1;
			String dataAsString;
			int startIndex;
			
			foreach (String line in lines)
			{
				if (line.Contains(idChar + identifier + idChar))
				{
					startIndex = line.LastIndexOf(idChar) + 1;
					dataAsString = (line.Substring(startIndex)).Trim();
					
					try
					{
						data = Convert.ToDouble(dataAsString);
					}
					catch (Exception)
					{
						errorMessage = "Could find, but not read value with identifier \""+ identifier + "\".";
						return false;
					}
					
					return true;
				}
			}
			
			errorMessage = "Could not find any data with identifier \"" + identifier + "\".";
			return false;
		}
	}
}

