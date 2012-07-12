using System;

namespace AssemblyCSharp
{
	public class Clock
	{
		// Member Variables
		private float _maxTime;
		private float _elapsedTime;
		
		private TimerStates _state;
		
		// Properties
		public TimerStates State
		{
			get { return this._state; }
		}
		
		public float TimeLeft
		{
			get { return (_maxTime - _elapsedTime); }
		}
		
		public float MaxTime
		{
			set { this._maxTime = value; }
		}
		
		public String TimeString
		{
			get
			{
				return TimeToString(this.TimeLeft);
			}
		}
		
		// Methods
		public Clock (float maxTime)
		{
			this._maxTime = maxTime;
			this._elapsedTime = 0.0f;
			
			this._state = TimerStates.Stopped;
		}
		
		public Clock ()
		{
			this._maxTime = 0;
			this._elapsedTime = 0.0f;
			
			this._state = TimerStates.Stopped;
		}
		
		public void Update (float deltaTime)
		{
			if (_state == TimerStates.Started)
			{
				if (_elapsedTime < _maxTime)
				{
					_elapsedTime += deltaTime;
				}
				if (_elapsedTime > _maxTime)
				{
					_elapsedTime = _maxTime;
					this._state = TimerStates.Expired;
				}			
			}
		}
		
		public void AddTime(float time)
		{
			this._maxTime += time;
		}
		
		public void Stop()
		{
			this._state = TimerStates.Stopped;
			this._elapsedTime = 0.0f;
		}
		
		public void Pause()
		{
			this._state = TimerStates.Paused;
		}
		
		public void Start()
		{
			this._state = TimerStates.Started;
		}		
		
		public static String TimeToString(float time)
		{
			int minutes;
			float seconds;
			
			minutes = (int)time / 60;
			seconds = (int)time % 60;
			seconds += time - (int)time;
			
			return (minutes.ToString("00") + ":" + seconds.ToString("00.00"));
		}
	}
}

