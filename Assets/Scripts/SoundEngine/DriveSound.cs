using System;
using System.IO;
using UnityEngine;

namespace AssemblyCSharp
{
	// This class contains the attributes of a specific sound
	// When played, you must assign an AudioSource on which it is then played
	public class DriveSound
	{
		// The location of the sound files in the resources folder
		private const string soundPath = "sound/";
		
		/**
		 * Member variables
		 */
		private AudioClip _clip;
		
		private bool _looped = true;
		// What length of the start and the end of the sound will be used to fade the loop (in seconds)
		private float _loopFadeTime = 0.0f;
		private float _pitch = 1.0f;
		private float _volume = 1.0f;
		
		/**
		 * Properties
		 */
		public bool IsLooped
		{
			get { return _looped; }
			set { _looped = value; }
		}
		
		public float LoopFadeTime
		{
			get { return _loopFadeTime; }
			set { _loopFadeTime = value; }
		}
		
		public float Pitch
		{
			get { return _pitch; }
			set 
			{ 
				if (value >= 0f)
					_pitch = value; 
			}
		}
		
		public float Volume
		{
			get { return _volume; }
			set 
			{ 
				if (value >= 0f)
					_volume = value; 
			}
		}
		
		// The fileName will also be the name of the sound
		public string Name
		{
			get { return _clip.name; }
		}
		
		/**
		 * Constructors
		 */
		public DriveSound (string fileName)
		{
			LoadSoundFromResources(fileName);
		}
		
		public DriveSound (string fileName, float pitch, float volume, bool looped, float loopFadeTime)
		{
			LoadSoundFromResources(fileName);
			
			_pitch = pitch;
			_volume = volume;
			_looped = looped;
			_loopFadeTime = loopFadeTime;
		}
		
		/**
		 * Methods
		 */
		// Assigns the sound to a specific audio channel
		public void PlaySound(AudioSource channel)
		{
			channel.clip = _clip;
			channel.pitch = _pitch;
			channel.volume = _volume;
			
			channel.Play();
		}
		
		// Plays the sound on anopther pitch/volume
		public void PlaySound(AudioSource channel, float pitch, float volume)
		{
			channel.clip = _clip;
			channel.pitch = pitch;
			channel.volume = volume;
			
			channel.Play();
		}
		
		public void LoadSoundFromResources(string fileName)
		{
			// Load sound file and throw an exception if it's non existent
			AudioClip newSound = (AudioClip) Resources.Load(soundPath+fileName, typeof(AudioClip));			
			if (newSound == null)
				throw new FileNotFoundException("The sound under the path " + fileName + " could not be loaded.");
			
			// The name of the sound is equally to the filename (without the ending)
			newSound.name = fileName;
			
			_clip = newSound;
		}
	}
}

