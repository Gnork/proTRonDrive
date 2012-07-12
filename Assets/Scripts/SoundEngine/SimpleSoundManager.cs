using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp
{
	// This class manages several DriveSounds and uses AudioSources as channels for them to be played in.
	// You can setup the number of channels - this will determine the maximum number of sounds that can be
	// played at once.
	public class SimpleSoundManager
	{	
		// This list contains all sounds that can be played
		private List<DriveSound> _sounds;
		// This list contains all channels
		private List<AudioSource> _channels;
		
		public SimpleSoundManager (int channelNumber, GameObject parent)
		{
			_sounds = new List<DriveSound>();			
			_channels = new List<AudioSource>();
			
			// Initialize all channels as components
			AudioSource source;
			for (int i=0; i < channelNumber; i++)
			{
				source = parent.AddComponent<AudioSource>();
				source.panLevel = 0f;
				
				_channels.Add(source);
			}
		}
		
		public bool PlaySound(string soundName)
		{
			if (GameControl._isSoundOn)
			{
				int soundToPlay = GetSoundIndexByName(soundName);
				int channelNumber = GetEmptySoundChannel();
							
				// If an unused channel was found, play the sound on it (if not, do nothing and return false)
				if (channelNumber >= 0)
				{
					_sounds[soundToPlay].PlaySound(_channels[channelNumber]);
					return true;
				}
			}
				
			return false;
		}
		
		public bool PlaySound(string soundName, float pitch, float volume)
		{
			if (GameControl._isSoundOn)
			{
				int soundToPlay = GetSoundIndexByName(soundName);
				int channelNumber = GetEmptySoundChannel();
							
				// If an unused channel was found, play the sound on it (if not, do nothing and return false)
				if (channelNumber >= 0)
				{
					_sounds[soundToPlay].PlaySound(_channels[channelNumber], pitch, volume);
					return true;
				}
			}
			
			return false;
		}
				
		// Adds a new sound	with pitch = 1.0f, volume = 1.0f and looped = false	
		public void AddSound(string fileName)
		{
			DriveSound newSound = new DriveSound(fileName);
			newSound.IsLooped = false;
			
			_sounds.Add(newSound);
		}
		
		// Adds a new sound
		public void AddSound(string fileName, float pitch, float volume, bool looped)
		{
			DriveSound newSound = new DriveSound(fileName, pitch, volume, looped, 0);
			_sounds.Add(newSound);
		}
		
		/**
		 * Private utility methods
		 */
		private int GetEmptySoundChannel()
		{
			for (int i=0; i < _channels.Count; i++)
			{
				// Leave the loop when a channel is found, where no sound is playing
				if (!_channels[i].isPlaying)
				{
					return i;
				}
			}
				
			return -1;
		}
			
		private int GetSoundIndexByName(string name)
		{
			for (int i=0; i < _sounds.Count; i++)
			{
				if (_sounds[i].Name == name)
					return i;
			}
			
			return -1;
		}
	}
}

