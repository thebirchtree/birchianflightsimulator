#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// <para>Defines file data relative to Steam Remote Storage</para>
    /// <para>This is the raw structure of data as seen on the Steam Remote Storage system and includes its address, metadata about the file and the raw data of the file.</para>
    /// </summary>
    [Serializable]
    public class SteamDataFile
    {
        /// <summary>
        /// Metadata regarding the Steam Data File including its location, size, name and time stamps.
        /// </summary>
        public SteamDataFileAddress address;
        /// <summary>
        /// The binary data of the file in question.
        /// </summary>
        [HideInInspector]
        public byte[] binaryData;
        /// <summary>
        /// apiCall handle ... this is used internally to direct callbacks from asynchronious operations.
        /// </summary>
        [HideInInspector]
        public SteamAPICall_t? apiCall;
        /// <summary>
        /// The status of of the most resent operation ran against this process.
        /// </summary>
        [HideInInspector]
        public EResult result = EResult.k_EResultPending;
        /// <summary>
        /// The Steam Data Library this file is assoceated with. This is used to determ how to deserialize the byte data returned into meaningful fields for use in Unity.
        /// </summary>
        [HideInInspector]
        public SteamDataLibrary linkedLibrary;

        /// <summary>
        /// Reads the data from a SteamDataLibrary into the byte[] in preperation for submiting the data to the Steam Remote Storage system.
        /// </summary>
        /// <param name="dataLibrary"></param>
        public void ReadFromLibrary(SteamDataLibrary dataLibrary)
        {
            linkedLibrary = dataLibrary;
            dataLibrary.SyncToBuffer(out binaryData);
        }

        /// <summary>
        /// Writes the data stored in the <see cref="binaryData"/> field into the target SteamDataLibrary e.g. loads the data into meaningful Unity memory.
        /// </summary>
        /// <param name="dataLibrary"></param>
        public void WriteToLibrary(SteamDataLibrary dataLibrary)
        {
            linkedLibrary = dataLibrary;
            dataLibrary.SyncFromBuffer(binaryData);
        }

        #region Encoding
        /// <summary>
        /// Encodes the binary data into UTF8
        /// </summary>
        /// <returns></returns>
        public string EncodeUTF8()
        {
            if (binaryData.Length > 0)
                return System.Text.Encoding.UTF8.GetString(binaryData);
            else
                return string.Empty;
        }
        /// <summary>
        /// Encodes the binary data into UTF32
        /// </summary>
        /// <returns></returns>
        public string EncodeUTF32()
        {
            if (binaryData.Length > 0)
                return System.Text.Encoding.UTF32.GetString(binaryData);
            else
                return string.Empty;
        }
        /// <summary>
        /// Encodes the binary data into Unicode
        /// </summary>
        /// <returns></returns>
        public string EncodeUnicode()
        {
            if (binaryData.Length > 0)
                return System.Text.Encoding.Unicode.GetString(binaryData);
            else
                return string.Empty;
        }
        /// <summary>
        /// Econdes the binary data into the system default encoding this will be platform dependent
        /// </summary>
        /// <returns></returns>
        public string EncodeDefault()
        {
            if (binaryData.Length > 0)
                return System.Text.Encoding.Default.GetString(binaryData);
            else
                return string.Empty;
        }
        /// <summary>
        /// Encodes the binary data into ASCII
        /// </summary>
        /// <returns></returns>
        public string EncodeASCII()
        {
            if (binaryData.Length > 0)
                return System.Text.Encoding.ASCII.GetString(binaryData);
            else
                return string.Empty;
        }
        #endregion
    }
}
#endif