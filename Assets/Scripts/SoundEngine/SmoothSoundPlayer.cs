using System;
using UnityEngine;

namespace AssemblyCSharp
{
	// This class is able to loop sounds without noticing the start and end points.
	// It has two AudioSources as sound channels
	public class SmoothSoundPlayer
	{
		// always fade a little bit earlier than needed to avoid moments where no sound can be heard
		// value in seconds
		private const float loopBuffer = 0.05f;
		
		/**
		 * Member variables
		 */
		private AudioSource _channel1;
		private AudioSource _channel2;
		
		private DriveSound _sound;
		
		// fader is responsible for the volume of each channel
		// Meaning of its values:
		// -1.0f = channel1 is playing, channel2 is muted
		//  0.0f = both channels play equally loud (on 100% volume)
		//  1.0f = channel1 is muted, channel2 is playing
		private float _fader = -1.0f;
		private float _faderMomentum = 0.0f;
		private float _volume = 1.0f;
		
		/**
		 * Properties
		 */
		public float Volume
		{
			get { return _volume; }
			set { _volume = value; }
		}
		
		/**
		 * Constructors
		 */
		public SmoothSoundPlayer (AudioSource channel1, AudioSource channel2)
		{
			_channel1 = channel1;
			_channel2 = channel2;
			
			// All looping is done manually
			_channel1.loop = false;
			_channel2.loop = false;
			_channel1.panLevel = 0;
			_channel2.panLevel = 0;
		}
		
		/**
		 * Methods
		 */
		public void Update(float deltaTime)
		{
			Fade(deltaTime);
			UpdateChannelVolumes();
			
			if (_sound != null)
			{
				_channel1.pitch = _sound.Pitch;
				_channel2.pitch = _sound.Pitch;
			}
			
			LoopSound();
		}
		
		public void PlaySound(DriveSound soundToPlay)
		{
			Stop();
			
			_sound = soundToPlay;
			_sound.PlaySound(_channel1);
		}
		
		public void Stop()
		{
			_channel1.Stop();
			_channel2.Stop();
			_sound = null;
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
					AudioSource temp;
									
					temp = _channel1;
					_channel1 = _channel2;
					_channel2 = temp;
										
					_channel2.Stop();	
					
					_fader = -1.0f;
					_faderMomentum = 0.0f;					
				}
			}
		}
	
		private void UpdateChannelVolumes()
		{
			if (_sound != null)
			{
				float channel1Volume = Mathf.Clamp01(-_fader + 1);
				float channel2Volume = Mathf.Clamp01(_fader + 1);
				
				// channel volume = soundTargetVolume * volumeBasedOnFader;
				_channel1.volume = _sound.Volume * channel1Volume * _volume;			
				_channel2.volume = _sound.Volume * channel2Volume * _volume;
			}
		}
		
		private void LoopSound()
		{
			// If a sound is played on that channel, that sound should be looped and a fading is not alreay in progress
			if ((_sound != null) && (_sound.IsLooped) && (_faderMomentum <= 0.0001f))
			{
				// If the remaining playtime of the current clip is under the loop fade time
				if ((_channel1.clip.length - _channel1.time) <= (_sound.LoopFadeTime + loopBuffer))
				{
//					float debug = _channel1.time;
					// Play the same sound again and fade over with the given loop fade time
					if (_sound.LoopFadeTime >= 0.0001f)
					{
						_sound.PlaySound(_channel2);	
						// fading should take exactly the loopFadeTime (fader must move 2 units from -1 to +1)
						// if the pitch is higher it has to fade faster to compensate the faster sound playback
						_faderMomentum = (2f / _sound.LoopFadeTime) * _channel1.pitch;
					}
					
					// If loop time is zero or almost zero: Just play the sound again
					else
					{
						_channel1.Play();
					}
				}
			}		
		}
	}
}

