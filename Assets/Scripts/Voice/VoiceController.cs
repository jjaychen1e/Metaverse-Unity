//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections;
using Microsoft.CognitiveServices.Speech.Audio;
using System.IO;
using TMPro;
using UnityEngine.Serialization;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

#if PLATFORM_IOS
using UnityEngine.iOS;
using System.Collections;
#endif

public class VoiceController : MonoBehaviour {
    private bool micPermissionGranted = false;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI answerText;
    public Button recoButton;
    SpeechRecognizer recognizer;
    SpeechConfig config;
    AudioConfig audioInput;
    PushAudioInputStream pushStream;

    private readonly SemaphoreLocker _locker = new SemaphoreLocker();
    private object threadLocker = new object();
    private bool recognitionStarted = false;
    private string questionMessage;
    private string answerMessage;
    int lastSample = 0;
    AudioSource audioSource;

    // Required to manifest microphone permission, cf.
    // https://docs.unity3d.com/Manual/android-manifest.html
    private Microphone mic;

    private byte[] ConvertAudioClipDataToInt16ByteArray(float[] data) {
        MemoryStream dataStream = new MemoryStream();
        int x = sizeof(Int16);
        Int16 maxValue = Int16.MaxValue;
        int i = 0;
        while (i < data.Length) {
            dataStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[i] * maxValue)), 0, x);
            ++i;
        }

        byte[] bytes = dataStream.ToArray();
        dataStream.Dispose();
        return bytes;
    }

    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e) {
        lock (threadLocker) {
            questionMessage = e.Result.Text;
            answerMessage = "等待问题识别中...";
            Debug.Log("RecognizingHandler: " + questionMessage);
        }
    }

    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e) {
        lock (threadLocker) {
            questionMessage = e.Result.Text;
            DebugHelper.Log("RecognizedHandler: " + questionMessage);
            answerMessage = "寻找答案中...";
            QANetwork.Shared.GetQuestionAnswer(questionMessage, (response) => {
                DebugHelper.Log("Response: " + response);
                if (response != null) {
                    DebugHelper.Log(response.userQuestion);
                    DebugHelper.Log(response.answerText);
                    answerMessage = response.answerText;
                }
            });
        }
    }

    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e) {
        lock (threadLocker) {
            questionMessage = e.ErrorDetails.ToString();
            Debug.Log("CanceledHandler: " + questionMessage);
        }
    }

    public void StartRecognitionWithLockAsync() {
        StartRecognitionWithLock();
    }

    public async System.Threading.Tasks.Task StartRecognitionWithLock() {
        await _locker.LockAsync(async () => {
            questionMessage = "问题识别中...";
            answerMessage = "等待问题识别中...";
            await StartRecognitionWithoutLock();
        });
    }

    public async System.Threading.Tasks.Task StartRecognitionWithoutLock() {
        if (!recognitionStarted) {
            if (!Microphone.IsRecording(Microphone.devices[0])) {
                Debug.Log("Microphone.Start: " + Microphone.devices[0]);
                audioSource.clip = Microphone.Start(Microphone.devices[0], true, 200, 16000);
                Debug.Log("audioSource.clip channels: " + audioSource.clip.channels);
                Debug.Log("audioSource.clip frequency: " + audioSource.clip.frequency);
            }

            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            recognitionStarted = true;
            Debug.Log("RecognitionStarted: " + recognitionStarted.ToString());
        }
    }

    public void StopRecognitionWithLockAsync() {
        StopRecognitionWithLock();
    }

    public async System.Threading.Tasks.Task StopRecognitionWithLock() {
        await _locker.LockAsync(async () => { await StopRecognitionWithoutLock(); });
    }

    public async System.Threading.Tasks.Task StopRecognitionWithoutLock() {
        if (recognitionStarted) {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(true);

            if (Microphone.IsRecording(Microphone.devices[0])) {
                Debug.Log("Microphone.End: " + Microphone.devices[0]);
                Microphone.End(null);
                lastSample = 0;
            }

            recognitionStarted = false;
            Debug.Log("RecognitionStarted: " + recognitionStarted.ToString());
        }
    }

    public async void ButtonClick() {
        await _locker.LockAsync(async () => {
            if (!recognitionStarted) {
                await StartRecognitionWithoutLock();
            }
            else {
                await StopRecognitionWithoutLock();
            }
        });
    }

    void Start() {
        if (questionText == null) {
            UnityEngine.Debug.LogError("outputText property is null! Assign a UI Text element to it.");
        }
        else if (recoButton == null) {
            questionMessage = "recoButton property is null! Assign a UI Button to it.";
            UnityEngine.Debug.LogError(questionMessage);
        }
        else {
            // Continue with normal initialization, Text and Button objects are present.

            // Request to use the microphone, cf.
            // https://docs.unity3d.com/Manual/android-RequestingPermissions.html
            questionMessage = "Waiting for mic permission";
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone)) {
                Permission.RequestUserPermission(Permission.Microphone);
            }

            config = SpeechConfig.FromSubscription("9b14d5328e294346b55489b2f25b04ed", "eastasia");
            config.SpeechRecognitionLanguage = "zh-CN";
            pushStream = AudioInputStream.CreatePushStream();
            audioInput = AudioConfig.FromStreamInput(pushStream);
            recognizer = new SpeechRecognizer(config, audioInput);
            recognizer.Recognizing += RecognizingHandler;
            recognizer.Recognized += RecognizedHandler;
            recognizer.Canceled += CanceledHandler;

            recoButton.onClick.AddListener(ButtonClick);
            foreach (var device in Microphone.devices) {
                Debug.Log("DeviceName: " + device);
            }

            audioSource = GetComponent<AudioSource>();
        }
    }

    void Disable() {
        recognizer.Recognizing -= RecognizingHandler;
        recognizer.Recognized -= RecognizedHandler;
        recognizer.Canceled -= CanceledHandler;
        pushStream.Close();
        recognizer.Dispose();
    }

    void FixedUpdate() {
        lock (threadLocker) {
            if (!micPermissionGranted && Permission.HasUserAuthorizedPermission(Permission.Microphone)) {
                micPermissionGranted = true;
                questionMessage = "Click button to recognize speech";
            }

            if (recoButton != null) {
                recoButton.interactable = micPermissionGranted;
            }

            if (questionText != null) {
                if (questionText.text != questionMessage) {
                    questionText.text = questionMessage;
                    if (!questionText.IsActive()) {
                        questionText.gameObject.SetActive(true);
                    }
                }
            }

            if (answerText != null) {
                if (answerText.text != answerMessage) {
                    answerText.text = answerMessage;
                    if (!answerText.IsActive()) {
                        answerText.gameObject.SetActive(true);
                    }
                }
            }

            if (Microphone.IsRecording(Microphone.devices[0]) && recognitionStarted == true) {
                if (recoButton != null) {
                    recoButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop";
                }

                int pos = Microphone.GetPosition(Microphone.devices[0]);
                int diff = pos - lastSample;

                if (diff > 0) {
                    float[] samples = new float[diff * audioSource.clip.channels];
                    audioSource.clip.GetData(samples, lastSample);
                    byte[] ba = ConvertAudioClipDataToInt16ByteArray(samples);
                    if (ba.Length != 0) {
                        Debug.Log("pushStream.Write pos:" + Microphone.GetPosition(Microphone.devices[0]).ToString() +
                                  " length: " + ba.Length.ToString());
                        pushStream.Write(ba);
                    }
                }

                lastSample = pos;
            }
            else if (!Microphone.IsRecording(Microphone.devices[0]) && recognitionStarted == false) {
                if (recoButton != null) {
                    recoButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start";
                }
            }
        }
    }
}