using System;

namespace AssemblyCSharp
{
	public static class Utility
	{
		public static bool HaveTheSameSign(float var1, float var2)
		{
			return ((var1 >= 0) == (var2 >= 0));	
		}
		
		public static T[] CopyArray<T>(T[] array)
		{
			T[] arrayCopy = new T[array.Length];
			for (int i=0; i < array.Length; i++)
				arrayCopy[i] = array[i];
			
			return arrayCopy;
		}
		
		public static bool StringsAsDoubles(String[] strings, out double[] doubleValues)
		{
			String[] stringValues = Utility.CopyArray<String>(strings);
			doubleValues = new double[stringValues.Length];			
			TrimStrings(stringValues);			
			
			for (int i=0; i < stringValues.Length; i++)
			{
				try 
				{
					doubleValues[i] = Convert.ToDouble(stringValues[i]);
				}
				catch (InvalidCastException e)
				{
					Console.Write(e.StackTrace);
					return false;
				}
			}
			
			return true;
		}
		
		public static void TrimStrings(String[] strings)
		{
			for (int i=0; i < strings.Length; i++)
			{
				strings[i] = ((String) strings[i]).Trim();
			}
		}
	}
}

