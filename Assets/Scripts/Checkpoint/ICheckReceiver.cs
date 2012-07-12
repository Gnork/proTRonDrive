using System;

namespace AssemblyCSharp
{
	// Any class implementing this interface is able to receive checkpoint passes
	public interface ICheckReceiver
	{
		void CheckpointPassed(bool isDrivingForward);
	}
}

