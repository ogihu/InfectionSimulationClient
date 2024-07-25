using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Whisper.Utils
{
    public class LoopingMicrophone : MonoBehaviour
    {
        public int maxLengthSec;
        float recordingTime = 0;
        public int frequency;
        public float evaluationTime;
        private bool _madeLoopLap;
        public int ClipSamples => _clip.samples * _clip.channels;
        private float elapsedTime = 0f;

        [Header("Microphone selection (optional)")]
        [CanBeNull] public TMP_Dropdown microphoneDropdown;
        public string microphoneDefaultLabel = "Default microphone";

        List<float> _savedClips = new List<float>();
        private AudioClip _clip;

        private string _selectedMicDevice;
        public string SelectedMicDevice
        {
            get => _selectedMicDevice;
            set
            {
                if (value != null && !AvailableMicDevices.Contains(value))
                    throw new ArgumentException("Microphone device not found");
                _selectedMicDevice = value;
            }
        }

        public string RecordStartMicDevice { get; private set; }
        public bool IsRecording { get; private set; }

        public IEnumerable<string> AvailableMicDevices => Microphone.devices;

        public delegate void OnEvaluateDelegate(AudioClip clip);
        public delegate void OnRecordStopDelegate(AudioClip clip);
        public delegate void OnRecordStartDelegate();
        public event OnEvaluateDelegate OnEvaluate;
        public event OnRecordStartDelegate OnRecordStart;
        public event OnRecordStopDelegate OnRecordStop;

        private void Awake()
        {
            IsRecording = false;
            if (microphoneDropdown != null)
            {
                microphoneDropdown.options = AvailableMicDevices
                    .Prepend(microphoneDefaultLabel)
                    .Select(text => new TMP_Dropdown.OptionData(text))
                    .ToList();
                microphoneDropdown.value = microphoneDropdown.options
                    .FindIndex(op => op.text == microphoneDefaultLabel);
                microphoneDropdown.onValueChanged.AddListener(OnMicrophoneChanged);
            }
        }

        private void Update()
        {
            if (!IsRecording)
                return;

            elapsedTime += Time.deltaTime;
            recordingTime += Time.deltaTime;

            if (elapsedTime >= evaluationTime)
            {
                elapsedTime -= evaluationTime;
                OnEvaluate?.Invoke(_clip);
            }

            if(recordingTime >= maxLengthSec)
            {
                CopyAndSaveClip();
            }
        }

        public void GetMics(TMP_Dropdown dropdown)
        {
            microphoneDropdown = dropdown;

            if (microphoneDropdown != null)
            {
                microphoneDropdown.options = AvailableMicDevices
                    .Prepend(microphoneDefaultLabel)
                    .Select(text => new TMP_Dropdown.OptionData(text))
                    .ToList();
                microphoneDropdown.value = microphoneDropdown.options
                    .FindIndex(op => op.text == SelectedMicDevice);
                microphoneDropdown.onValueChanged.AddListener(OnMicrophoneChanged);
            }
        }

        private void OnMicrophoneChanged(int ind)
        {
            if (microphoneDropdown == null) return;
            var opt = microphoneDropdown.options[ind];
            SelectedMicDevice = opt.text == microphoneDefaultLabel ? null : opt.text;
        }

        public void StartRecord()
        {
            OnRecordStart?.Invoke();
            RecordStartMicDevice = SelectedMicDevice;
            ClearClips();
            _clip = Microphone.Start(RecordStartMicDevice, true, maxLengthSec, frequency);
            IsRecording = true;
        }

        public void StopRecord()
        {
            if (!IsRecording)
                return;

            Microphone.End(RecordStartMicDevice);
            float[] arr = GetData();
            _savedClips.AddRange(arr);
            recordingTime = 0;

            OnRecordStop?.Invoke(_clip);
            IsRecording = false;
        }

        public float[] GetAllClipData()
        {
            return _savedClips.ToArray();
        }

        float[] GetData()
        {
            int pos = Microphone.GetPosition(RecordStartMicDevice);
            if (pos == 0)
            {
                float[] arr = new float[_clip.samples];
                _clip.GetData(arr, 0);
                return arr;
            }
            float[] arr1 = new float[_clip.samples - pos];
            float[] arr2 = new float[pos];
            _clip.GetData(arr1, pos);
            _clip.GetData(arr2, 0);

            return arr1.Concat(arr2).ToArray();
        }

        void CopyAndSaveClip()
        {
            if (!IsRecording)
                return;

            float[] arr = GetData();
            _savedClips.AddRange(arr);
            _clip = Microphone.Start(RecordStartMicDevice, true, maxLengthSec, frequency);
            recordingTime = 0;
        }

        public void ClearClips()
        {
            if(_clip != null)
            {
                Microphone.End(RecordStartMicDevice);
                _clip = null;
            }

            _savedClips.Clear();
        }
    }
}