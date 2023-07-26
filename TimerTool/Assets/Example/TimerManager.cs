using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TimerTool.Manager
{
    public class TimerManager : MonoBehaviour
    {
        [Header("Timer settings")]
        [SerializeField] private float _duration;

        [Space(10)]

        [SerializeField] private bool _isLooped;
        [SerializeField] private bool _usesRealTime;

        [Space(10)]

        [Header("On Update Text References")]
        [SerializeField] private Text _timeElapsedText;
        [SerializeField] private Text _timeRemainingText;
        [SerializeField] private Text _ratioElapsedText;
        [SerializeField] private Text _ratioRemainingText;
        [SerializeField] private Text _worldTimeText;
        [SerializeField] private Text _finishTimeText;
        [SerializeField] private Text _timeDletaText;

        [Space(20)]

        [SerializeField] private List<TimerEvent> _timerEvents;

        [Space(20)]

        [SerializeField] private UnityEvent _onFinish;

        [Space(20)]

        [SerializeField] private UnityEvent<TimerData> _onUpdate;


        private Timer _timer;

        private void Awake()
        {
            _timeElapsedText.text = $"Time Elapsed:";
            _timeRemainingText.text = $"Time Remaining:";
            _ratioElapsedText.text = $"Ratio Elapsed:";
            _ratioRemainingText.text = $"Ratio Remaining:";
            _worldTimeText.text = $"World Time Elapsed:";
            _finishTimeText.text = $"Finish Time:";
            _timeDletaText.text = $"Time Delta:";
        }

        private void Update()
        {
            _timer?.Update();
        }

        public void StartTimer()
        {
            _timer = new Timer(
                duration: _duration,
                isLooped: _isLooped,
                usesRealTime: _usesRealTime,
                onFinish: _onFinish,
                onUpdate: _onUpdate,
                timerEvents: _timerEvents);
        }

        public void StopTimer()
        {
            _timer?.Stop();
        }

        public void ResumeTimer()
        {
            _timer?.Resume();
        }

        public void PauseTimer()
        {
            _timer?.Pause();
        }

        public void UpdateTimer(TimerData data)
        {
            _timeElapsedText.text = $"Time Elapsed: {data.TimeElapsed}";
            _timeRemainingText.text = $"Time Remaining: {data.TimeRemaining}";
            _ratioElapsedText.text = $"Ratio Elapsed: {data.RatioElapsed}";
            _ratioRemainingText.text = $"Ratio Remaining: {data.RatioRemaining}";
            _worldTimeText.text = $"World Time Elapsed: {data.WorldTime}";
            _finishTimeText.text = $"Finish Time: {data.FinishTime}";
            _timeDletaText.text = $"Time Delta: {data.TimeDelta}";
        }

        public void EventDebugger(TimerData data)
        {
            Debug.Log($"Event called at: {data.TimeElapsed}");
        }
    }
}