using System;

namespace AssemblyCSharp
{
	// If a class implements this interface it is able to receive a message if any checkpoint in the game is passed
	// Therefore you can set the static member of Checkpoint to that specific class
	public interface IMasterCheckReceiver
	{
		void AnyCheckpointPassed(bool isDrivingForward);
	}
}

