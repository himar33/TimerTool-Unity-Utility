
/*
 * Timer Tool for Unity
 *
 * Version: 0.5
 * By: Himar Bravo González
 * 
 * Based on UnityTimer tool by Alexander Biggs & Adam Robinson-Yu
 * Link: https://github.com/akbiggs/UnityTimer/tree/master
 * 
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TimerTool
{
    /// <summary>
    /// Structure representing timer data containing various properties related to the timer's state and progress.
    /// </summary>
    public struct TimerData
    {
        /// <summary>
        /// The elapsed time (in seconds) since the timer started.
        /// </summary>
        public float TimeElapsed;

        /// <summary>
        /// The remaining time (in seconds) until the timer completes its duration.
        /// </summary>
        public float TimeRemaining;

        /// <summary>
        /// The ratio of elapsed time to the total duration (expressed as a fraction between 0 and 1).
        /// </summary>
        public float RatioElapsed;

        /// <summary>
        /// The ratio of remaining time to the total duration (expressed as a fraction between 0 and 1).
        /// </summary>
        public float RatioRemaining;

        /// <summary>
        /// The current world time (in seconds) when the TimerData is updated.
        /// </summary>
        public float WorldTime;

        /// <summary>
        /// The expected time (in seconds) when the timer will finish its duration.
        /// </summary>
        public float FinishTime;

        /// <summary>
        /// The time interval (in seconds) between the last update and the current update of the TimerData.
        /// </summary>
        public float TimeDelta;
    }
    /// <summary>
    /// A class representing an event associated with a specific time during the execution of a Timer.
    /// </summary>
    [System.Serializable]
    public class TimerEvent
    {
        /// <summary>
        /// The time (in seconds) when this event should be triggered during the Timer's execution.
        /// </summary>
        public float KeyTime;

        /// <summary>
        /// The UnityEvent that will be invoked when the associated time is reached during the Timer's execution.
        /// </summary>
        public UnityEvent<TimerData> KeyEvent;

        /// <summary>
        /// A flag indicating whether the event has already been called during the current Timer execution.
        /// </summary>
        private bool _isCalled = false;

        /// <summary>
        /// Checks if the event has been called during the current Timer execution.
        /// </summary>
        /// <returns>True if the event has been called, otherwise false.</returns>
        public bool IsCalled()
        {
            return _isCalled;
        }

        /// <summary>
        /// Calls the associated UnityEvent, triggering the event's actions with the given TimerData.
        /// </summary>
        /// <param name="data">The TimerData to be passed to the UnityEvent when invoked.</param>
        public void CallEvent(TimerData data)
        {
            KeyEvent.Invoke(data);
            _isCalled = true;
        }

        /// <summary>
        /// Resets the state of the TimerEvent, allowing it to be called again in future Timer executions.
        /// </summary>
        public void Reset()
        {
            _isCalled = false;
        }
    }
    public class Timer
    {
        #region Public Properties

        /// <summary>
        /// The total duration (in seconds) of the timer.
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// A flag indicating whether the timer should be looped/restarted after it completes.
        /// </summary>
        public bool IsLooped { get; set; }

        /// <summary>
        /// A flag indicating whether the timer has completed its duration.
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// A flag indicating whether the timer is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// A flag indicating whether the timer uses real-time or game-time.
        /// Real-time is unaffected by changes to the timescale of the game (e.g., pausing, slow-mo),
        /// while game-time is affected by these changes.
        /// </summary>
        public bool UsesRealTime { get; private set; }

        /// <summary>
        /// A flag indicating whether the timer is currently paused.
        /// </summary>
        public bool IsPaused
        {
            get { return this._timeBeforePause.HasValue; }
        }

        /// <summary>
        /// A flag indicating whether the timer is currently stopped.
        /// </summary>
        public bool IsStopped
        {
            get { return this._timeBeforeStop.HasValue; }
        }

        /// <summary>
        /// A flag indicating whether the timer has either completed or been stopped.
        /// </summary>
        public bool IsFinished
        {
            get { return this.IsCompleted || this.IsStopped; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Constructor for the Timer class that initializes the timer with the provided parameters.
        /// </summary>
        /// <param name="duration">The total duration (in seconds) of the timer.</param>
        /// <param name="isLooped">Flag indicating whether the timer should be looped (restarted) after completion.</param>
        /// <param name="usesRealTime">Flag indicating whether the timer uses real-time or game-time.</param>
        /// <param name="onFinish">UnityEvent invoked when the timer completes its duration.</param>
        /// <param name="onUpdate">UnityEvent invoked each frame with updated TimerData during the timer's execution.</param>
        /// <param name="timerEvents">List of TimerEvents representing events to be triggered during the timer's execution.</param>
        public Timer(float duration, bool isLooped, bool usesRealTime, UnityEvent onFinish, UnityEvent<TimerData> onUpdate, List<TimerEvent> timerEvents)
        {
            this.Duration = duration;
            this.IsLooped = isLooped;
            this.UsesRealTime = usesRealTime;

            this._onFinish = onFinish;
            this._onUpdate = onUpdate;

            this._startTime = this.GetWorldTime();
            this._lastUpdatedTime = this._startTime;
            this._timerEvents = timerEvents;
        }

        /// <summary>
        /// Stops the timer's execution. If the timer is already finished, it has no effect.
        /// </summary>
        public void Stop()
        {
            if (this.IsFinished)
            {
                return;
            }

            this._timeBeforeStop = this.GetTimeElapsed();
            this._timeBeforePause = null;
        }

        /// <summary>
        /// Pauses the timer's execution. If the timer is already paused or finished, it has no effect.
        /// </summary>
        public void Pause()
        {
            if (this.IsPaused || this.IsFinished)
            {
                return;
            }

            this._timeBeforePause = this.GetTimeElapsed();
        }

        /// <summary>
        /// Resumes the timer's execution if it was previously paused. If the timer is not paused or finished, it has no effect.
        /// </summary>
        public void Resume()
        {
            if (!this.IsPaused || this.IsFinished)
            {
                return;
            }

            this._timeBeforePause = null;
        }

        /// <summary>
        /// Adds a TimerEvent to the list of events associated with the timer.
        /// </summary>
        /// <param name="tEvent">The TimerEvent to be added.</param>
        public void AddEvent(TimerEvent tEvent)
        {
            _timerEvents.Add(tEvent);
        }


        /// <summary>
        /// Adds a list of TimerEvents to the existing list of events associated with the timer.
        /// </summary>
        /// <param name="tEvents">The list of TimerEvents to be added.</param>
        public void AddEvents(List<TimerEvent> tEvents)
        {
            _timerEvents.AddRange(tEvents);
        }

        /// <summary>
        /// Returns the elapsed time (in seconds) since the timer started. If the timer has finished or the world time exceeds the finish time,
        /// it returns the total duration of the timer to prevent negative values.
        /// </summary>
        /// <returns>The elapsed time in seconds.</returns>
        public float GetTimeElapsed()
        {
            if (this.IsFinished || this.GetWorldTime() >= this.GetFinishTime())
            {
                return this.Duration;
            }

            return this._timeBeforeStop ??
                   this._timeBeforePause ??
                   this.GetWorldTime() - this._startTime;
        }

        /// <summary>
        /// Returns the remaining time (in seconds) until the timer completes its duration.
        /// </summary>
        /// <returns>The remaining time in seconds.</returns>
        public float GetTimeRemaining()
        {
            return this.Duration - this.GetTimeElapsed();
        }

        /// <summary>
        /// Returns the ratio of elapsed time to the total duration (as a fraction between 0 and 1).
        /// Optionally, the ratio can be returned as a percentage if 'inPercentage' is true.
        /// </summary>
        /// <param name="inPercentage">If true, the ratio is returned as a percentage.</param>
        /// <returns>The ratio of elapsed time to total duration.</returns>
        public float GetRatio(bool inPercentage = false)
        {
            return (this.GetTimeElapsed() / this.Duration) * (inPercentage ? 100 : 1);
        }

        /// <summary>
        /// Returns the ratio of remaining time to the total duration (as a fraction between 0 and 1).
        /// Optionally, the ratio can be returned as a percentage if 'inPercentage' is true.
        /// </summary>
        /// <param name="inPercentage">If true, the ratio is returned as a percentage.</param>
        /// <returns>The ratio of remaining time to total duration.</returns>
        public float GetRatioRemaining(bool inPercentage = false)
        {
            return (this.GetTimeRemaining() / this.Duration) * (inPercentage ? 100 : 1);
        }

        /// <summary>
        /// Returns the current world time (in seconds) based on whether the timer uses real-time or game-time.
        /// </summary>
        /// <returns>The current world time in seconds.</returns>
        public float GetWorldTime()
        {
            return this.UsesRealTime ? Time.realtimeSinceStartup : Time.time;
        }

        /// <summary>
        /// Returns the expected time (in seconds) when the timer will finish its duration.
        /// </summary>
        /// <returns>The expected finish time in seconds.</returns>
        public float GetFinishTime()
        {
            return this._startTime + this.Duration;
        }

        /// <summary>
        /// Returns the time interval (in seconds) between the last update and the current update of the TimerData.
        /// </summary>
        /// <returns>The time interval between the last two updates in seconds.</returns>
        public float GetTimeDelta()
        {
            return this.GetWorldTime() - this._lastUpdatedTime;
        }

        /// <summary>
        /// Updates the timer's state and executes associated events. This method should be called each frame
        /// to keep the timer active and to trigger events at the appropriate times.
        /// </summary>
        public void Update()
        {
            if (this.IsFinished) return;

            if (this.IsPaused)
            {
                this._startTime += this.GetTimeDelta();
                this._lastUpdatedTime = this.GetWorldTime();
                return;
            }

            this._lastUpdatedTime = this.GetWorldTime();

            TimerData frameData = new TimerData();
            frameData.TimeElapsed = this.GetTimeElapsed();
            frameData.TimeRemaining = this.GetTimeRemaining();
            frameData.RatioElapsed = this.GetRatio();
            frameData.RatioRemaining = this.GetRatioRemaining();
            frameData.WorldTime = this.GetWorldTime();
            frameData.FinishTime = this.GetFinishTime();
            frameData.TimeDelta = this.GetTimeDelta();

            foreach (TimerEvent timerEvent in this._timerEvents)
            {
                if (!timerEvent.IsCalled() && GetTimeElapsed() >= timerEvent.KeyTime)
                {
                    timerEvent.CallEvent(frameData);
                }
            }

            if (this._onUpdate != null)
            {
                this._onUpdate.Invoke(frameData);
            }

            if (this.GetWorldTime() >= this.GetFinishTime())
            {
                if (this._onFinish != null)
                {
                    this._onFinish.Invoke();
                }

                if (this.IsLooped)
                {
                    this._startTime = this.GetWorldTime();
                    foreach (TimerEvent timerEvent in _timerEvents)
                    {
                        timerEvent.Reset();
                    }
                }
                else
                {
                    this.IsCompleted = true;
                }
            }
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// The time (in seconds) when the timer started or was last resumed.
        /// </summary>
        private float _startTime;

        /// <summary>
        /// The time (in seconds) when the TimerData was last updated or when the timer was last resumed.
        /// </summary>
        private float _lastUpdatedTime;

        /// <summary>
        /// The time (in seconds) when the timer was stopped. Null if the timer is not stopped.
        /// </summary>
        private float? _timeBeforeStop;

        /// <summary>
        /// The time (in seconds) when the timer was paused. Null if the timer is not paused.
        /// </summary>
        private float? _timeBeforePause;

        /// <summary>
        /// The UnityEvent invoked when the timer completes its duration.
        /// </summary>
        private readonly UnityEvent _onFinish;

        /// <summary>
        /// The UnityEvent<TimerData> invoked each frame with updated TimerData during the timer's execution.
        /// </summary>
        private readonly UnityEvent<TimerData> _onUpdate;

        /// <summary>
        /// List of TimerEvents representing events to be triggered during the timer's execution.
        /// </summary>
        private List<TimerEvent> _timerEvents = new List<TimerEvent>();

        #endregion
    }
}