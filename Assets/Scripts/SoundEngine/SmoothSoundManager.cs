using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssemblyCSharp
{
	// This class is a manager of various DriveSound objects
	// It is able to play one sound at a time and smoothly fade into another sound
	// Use two DriveSoundPlayers as sound channels
	public class SmoothSoundManager
	{
		// Each possible sound is saved in this list
		// (so all sounds can be loaded at once)
		private List<DriveSound> _clips;		
		// Needs 2 sound channels to be able to fade between two sounds
		// or even fade between the start and the end of the same sound (non-noticeable looping)
		private SmoothSoundPlayer _channel1;
		private SmoothSoundPlayer _channel2;
		// The sound currently being played on the channel (index position in the _clips list)
		// -1 means no sound is currently being played
		private int _channel1Sound = -1;
		private int _channel2Sound = -1;
		// If a third sound requests to be played its index is stored here.
		// If theres even a fourth sound then it will replace the third one etc..
		// That means there can be a maximum of 3 sounds in the "joblist"
		private int _soundWaiting = -1;
		// fader is responsible for the volume of each channel
		// Meaning of its values:
		// -1.0f = channel1 is playing, channel2 is muted
		//  0.0f = both channels play equally loud (on 100% volume)
		//  1.0f = channel1 is muted, channel2 is playing
		private float _fader = -1.0f;
		// The direction and the speed (stepSize / second) of the fader
		// (a value of -0.1f means, as example, the fader moves 0.1f per second to the left)
		private float _faderMomentum = 0.0f;
		
		/**
		 * Constructors
		 */
		public SmoothSoundManager (SmoothSoundPlayer soundPlayer1, SmoothSoundPlayer soundPlayer2)
		{
			_channel1 = soundPlayer1;
			_channel2 = soundPlayer2;			
			
			_clips = new List<DriveSound>();
		}
		
		/**
		 * Public Methods
		 */
		public void Update(float deltaTime)
		{
			Fade(deltaTime);	
			UpdateChannelVolumes();
			
			_channel1.Update(deltaTime);
			_channel2.Update(deltaTime);
		}
		
		public DriveSound AddSound(string fileName)
		{
			// Adds a sound set to standard pitch and volume (= 1.0f) and looped to true
			DriveSound newSound = new DriveSound(fileName);
			_clips.Add(newSound);
			return newSound;
		}
		
		public DriveSound AddSound(string fileName, float pitch, float volume, bool looped, float loopFadeTime)
		{
			// Adds a new sound with all attributes manually set
			DriveSound newSound = new DriveSound(fileName, pitch, volume, looped, loopFadeTime);
			_clips.Add(newSound);
			return newSound;
		}
		
		public void Stop()
		{
			_channel1.Stop();
			_channel2.Stop();
			
			_channel1Sound = -1;
			_channel2Sound = -1;
			_soundWaiting = -1;
		}
		
		// Fades the sound currently playing into the new sound - fadeTime in seconds
		public void PlaySound(string soundName, float fadeTime)
		{
			if (GameControl._isSoundOn)
			{
				// If no sound is currently being played or, if a sound is playing, the requested sound is a different one
				if ((_channel1Sound < 0) || (soundName != _clips[_channel1Sound].Name))
				{
					int soundIndex = GetSoundIndexByName(soundName);
								
					// If no instant fading is wanted
					if (fadeTime > 0.0001f)
					{
						// If no sound is currently being played
						if (_channel1Sound == -1)
						{
							_channel1.PlaySound(_clips[soundIndex]);
							_channel1Sound = soundIndex;
						}
						
						// If one sound is currently being played
						else if (_channel2Sound == -1)
						{				
							_channel2.PlaySound(_clips[soundIndex]);
							_channel2Sound = soundIndex;
							
							// fader must move a value of 2 to reach the other end
							// (from -1 to +1)
							_faderMomentum = 2f / fadeTime;
						}
						
						// If a fading process is already taking place
						else if (soundName != _clips[_channel2Sound].Name)
						{
							_soundWaiting = soundIndex;
							// Explanation of the following equation:
							// fader position + 1 = current position of the fader from 0 to 2
							// 2 - currentPosition = distance to go till fader reaches sound2
							// 2 + distance to go = distance the fader must cover to reach the waiting sound
							// divided by 2 = value between 1 and 2:
							// 1 means fader must go the usual distance of 2, value of 2 means it must go double the distance
							// ---------------------------------------------------------------------
							// In conclusion: The time it takes to reach the 3rd sound is exactly the fadeTime
							_faderMomentum = ((2f + (2f - (_fader + 1))) / 2) * (2f / fadeTime);
						}
					}
					
					// If fade time is zero or almost zero: Play instantly!
					else
					{
						_channel1.PlaySound(_clips[soundIndex]);
						_channel1Sound = soundIndex;
		
						_channel2.Stop();				
						_channel2Sound = -1;
						_soundWaiting = -1;
						
						_fader = -1.0f;
						_faderMomentum = 0.0f;
					}
				}
			}
		}
		
		public DriveSound GetSoundByName(string soundName)
		{
			int index = GetSoundIndexByName(soundName);
			return _clips[index];
		}
		
		/**
		 * Private Methods
		 */		
		private void UpdateChannelVolumes()
		{
			float channel1Volume = Mathf.Clamp01(-_fader + 1);
			float channel2Volume = Mathf.Clamp01(_fader + 1);
			
			// channel volume = soundTargetVolume * volumeBasedOnFader;
			if (_channel1Sound >= 0)
				_channel1.Volume = _clips[_channel1Sound].Volume * channel1Volume;
			
			if (_channel2Sound >= 0)
				_channel2.Volume = _clips[_channel2Sound].Volume * channel2Volume;
		}
		
		private void Fade(float deltaTime)
		{
			if (_faderMomentum >= 0.0001f)
			{
				_fader += _faderMomentum * deltaTime;
				
				// If channel2 is completely faded in, swap channel1 and channel2
				// and stop playing the now muted sound
				if (_fader >= 1.0f)
				{
					SmoothSoundPlayer temp;
									
					temp = _channel1;
					_channel1 = _channel2;
					_channel2 = temp;

					_channel1Sound = _channel2Sound;										
					_channel2.Stop();	
					
					// If theres a sound waiting to be played
					if (_soundWaiting >= 0)					
					{
						_channel2.PlaySound(_clips[_soundWaiting]);
						_fader = -1.0f;
					}
					else
					{
						_fader = -1.0f;
						_faderMomentum = 0.0f;				
	
						_channel2Sound = -1;	
					}
				}
			}
		}
		
		private int GetSoundIndexByName(string name)
		{
			for (int i=0; i < _clips.Count; i++)
			{
				if (_clips[i].Name == name)
					return i;
			}
			
			return -1;
		}
	}
}

