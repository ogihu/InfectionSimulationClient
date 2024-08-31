using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using POpusCodec;
using System.Threading.Tasks;
using System.Threading;
using static Define;
using System.Collections.Concurrent;

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
        private int _lastSentSample = 0;

        [Header("Microphone selection (optional)")]
        [CanBeNull] public TMP_Dropdown microphoneDropdown;
        public string microphoneDefaultLabel = "Default microphone";

        List<float> _savedClips = new List<float>();
        private AudioClip _clip;

        OpusEncoder _encoder;
        private ConcurrentQueue<IJob> _encodingWorks = new ConcurrentQueue<IJob>();
        private Task _processingTask;
        private CancellationTokenSource _cancellationTokenSource;

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
            frequency = VoiceFrequency;

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

            //_encoder = new OpusEncoder(POpusCodec.Enums.SamplingRate.Sampling16000,
            //                           POpusCodec.Enums.Channels.Mono,
            //                           12000,
            //                           POpusCodec.Enums.OpusApplicationType.Voip,
            //                           POpusCodec.Enums.Delay.Delay20ms);

            //StartBackgroundTaskProcessor();
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
            //StartCoroutine(UpdateVoicePacket(0.02f));
        }

        public void StopRecord()
        {
            if (!IsRecording)
                return;

            Microphone.End(RecordStartMicDevice);
            float[] arr = GetData();
            
            if (arr == null)
                return;

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
            float[] arr;

            if(_clip == null)
            {
                Debug.LogWarning("클립이 비어있습니다.");
                return null;
            }

            if (pos == 0)
            {
                arr = new float[_clip.samples];
                _clip.GetData(arr, 0);
            }
            else
            {
                float[] arr1 = new float[_clip.samples - pos];
                float[] arr2 = new float[pos];
                _clip.GetData(arr1, pos);
                _clip.GetData(arr2, 0);
                arr = arr1.Concat(arr2).ToArray();
            }

            return arr;
        }

        void CopyAndSaveClip()
        {
            if (!IsRecording)
                return;

            float[] arr = GetData();
            if (arr == null)
                return;

            _savedClips.AddRange(arr);
            _clip = Microphone.Start(RecordStartMicDevice, true, maxLengthSec, frequency);
            recordingTime = 0;
            _lastSentSample = 0;
        }

        //void SendUnsentAudio()
        //{
        //    try
        //    {
        //        int pos = Microphone.GetPosition(RecordStartMicDevice);
        //        if (pos < 0 || !_clip)
        //            return;

        //        int sampleCount = pos >= _lastSentSample ? pos - _lastSentSample : _clip.samples - _lastSentSample + pos;
        //        int channel = _clip.channels;

        //        if (channel <= 0)
        //        {
        //            return;
        //        }

        //        if (sampleCount > 0)
        //        {
        //            float[] data = new float[sampleCount];
        //            _clip.GetData(data, _lastSentSample);
        //            _lastSentSample = pos;

        //            Job<float[], int> job = new Job<float[], int>(EncodingAndSendAudioData, data, channel);
        //            _encodingWorks.Enqueue(job);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.LogError($"Error in SendUnsentAudio: {e.Message}");
        //    }
        //}

        //void SendEndFlag()
        //{
        //    C_EndVoice endPacket = new C_EndVoice();
        //    _encodingWorks.Enqueue(new Job(() => Managers.Network.Send(endPacket)));
        //}

        //void EncodingAndSendAudioData(float[] data, int channel)
        //{
        //    byte[] encodedData = null;

        //    if (channel == 1)
        //    {
        //        encodedData = _encoder.Encode(data);
        //    }
        //    else
        //    {
        //        float[] convertedData = Define.ConvertStereoToMono(data);
        //        encodedData = _encoder.Encode(convertedData);
        //    }

        //    C_Voice voicePacket = new C_Voice();
        //    voicePacket.VoiceClip = Google.Protobuf.ByteString.CopyFrom(encodedData);
        //    Managers.Network.Send(voicePacket);
        //}

        //IEnumerator UpdateVoicePacket(float time)
        //{
        //    while (IsRecording)
        //    {
        //        yield return new WaitForSeconds(time);
        //        SendUnsentAudio();
        //    }
        //    SendEndFlag();
        //    Debug.Log("음성 패킷 전송 끝");
        //    yield break;
        //}

        public void ClearClips()
        {
            if(_clip != null)
            {
                Microphone.End(RecordStartMicDevice);
                _clip = null;
            }

            _savedClips.Clear();
        }

        //private void StartBackgroundTaskProcessor()
        //{
        //    _cancellationTokenSource = new CancellationTokenSource();
        //    _processingTask = Task.Run(ProcessQueue, _cancellationTokenSource.Token);
        //}

        //private void StopBackgroundTaskProcessor()
        //{
        //    _cancellationTokenSource.Cancel();
        //    _processingTask?.Wait();
        //}

        //private void OnDestroy()
        //{
        //    StopBackgroundTaskProcessor();
        //}

        //private async Task ProcessQueue()
        //{
        //    while (!_cancellationTokenSource.IsCancellationRequested)
        //    {
        //        if (_encodingWorks.TryDequeue(out var workItem))
        //        {
        //            workItem.Execute();
        //        }
        //        else
        //        {
        //            await Task.Delay(600);
        //        }
        //    }
        //}
    }
}