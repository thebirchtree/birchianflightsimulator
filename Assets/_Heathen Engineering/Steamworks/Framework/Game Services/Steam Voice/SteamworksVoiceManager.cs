#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.GameServices
{
    /// <summary>
    /// Manages the Steam Voice feature.
    /// <a href="https://partner.steamgames.com/doc/features/voice">https://partner.steamgames.com/doc/features/voice</a>
    /// </summary>
    public class SteamworksVoiceManager : MonoBehaviour
    {
        /// <summary>
        /// The audio source to output recieved and decoded voice messages to.
        /// </summary>
        public AudioSource OutputSource;

        [ReadOnly(true)]
        [SerializeField]
        private bool isRecording = false;
        /// <summary>
        /// Is the system currently recording audio data.
        /// </summary>
        public bool IsRecording
        {
            get { return isRecording; }
        }
        /// <summary>
        /// Occures when the Voice Result Restricted EVoiceResult is recieved from the Steam API.
        /// </summary>
        public UnityEvent StopedOnChatRestricted;
        /// <summary>
        /// Occures every frame when the Steam API has a voice stream payload from the user.
        /// </summary>
        public ByteArrayEvent VoiceStream;

        private AudioClip voiceOutput;
        private int previousSampleRate;

        private void Update()
        {
            if (isRecording)
            {
                var result = SteamUser.GetAvailableVoice(out uint pcbCompressed);
                switch (result)
                {
                    case EVoiceResult.k_EVoiceResultNoData:
                        //No data so do nothing
                        break;
                    case EVoiceResult.k_EVoiceResultNotInitialized:
                        //Not initalized ... report the error
                        Debug.LogError("The Steam Voice systemis not initalized and will be stoped.");
                        SteamUser.StopVoiceRecording();
                        break;
                    case EVoiceResult.k_EVoiceResultNotRecording:
                        //We are not recording but think we are
                        SteamUser.StartVoiceRecording();
                        break;
                    case EVoiceResult.k_EVoiceResultRestricted:
                        //User is chat restricted ... report this out and turn off recording.
                        StopedOnChatRestricted.Invoke();
                        SteamUser.StopVoiceRecording();
                        break;
                    case EVoiceResult.k_EVoiceResultOK:
                        //All is well check the compressed size to see if we have data and if so package it
                        byte[] buffer = new byte[pcbCompressed];
                        SteamUser.GetVoice(true, buffer, pcbCompressed, out uint bytesWriten);
                        if (bytesWriten > 0)
                            VoiceStream.Invoke(buffer);
                        break;
                }
            }
        }

        /// <summary>
        /// Starts the Steam API recording audio from the user's configured mic
        /// </summary>
        public void StartRecording()
        {
            isRecording = true;
            SteamUser.StartVoiceRecording();
        }

        /// <summary>
        /// Stops the Steam API from recording audio for the user's configured mic
        /// </summary>
        public void StopRecording()
        {
            isRecording = false;
            SteamUser.StopVoiceRecording();
        }

        /// <summary>
        /// Players a recieved Steam Voice package through the <see cref="OutputSource"/> <see cref="AudioSource"/>.
        /// </summary>
        /// <param name="buffer"></param>
        public void PlayVoiceData(byte[] buffer)
        {
            var sampleRate = SteamUser.GetVoiceOptimalSampleRate();
            byte[] destBuffer = new byte[sampleRate * 2];
            var result = SteamUser.DecompressVoice(buffer, (uint)buffer.Length, destBuffer, (uint)destBuffer.Length, out uint bytesWritten, sampleRate);

            if (result == EVoiceResult.k_EVoiceResultOK && bytesWritten > 0)
            {
                if (voiceOutput == null)
                    voiceOutput = AudioClip.Create("VOICE", (int)sampleRate, 1, (int)sampleRate, false);
                else //if (previousSampleRate != sampleRate)
                {
                    Destroy(voiceOutput);
                    voiceOutput = AudioClip.Create("VOICE", (int)sampleRate, 1, (int)sampleRate, false);
                }

                float[] bitConversion = new float[sampleRate];
                for (int i = 0; i < bitConversion.Length; ++i)
                {
                    bitConversion[i] = (short)(destBuffer[i * 2] | destBuffer[i * 2 + 1] << 8) / 32768.0f;
                }

                voiceOutput.SetData(bitConversion, 0);
                OutputSource.clip = voiceOutput;
                OutputSource.Play();
            }
        }
    }
}
#endif