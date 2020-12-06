#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// Defines the address of a data file as stored in Steam Remote Storage system.
    /// </summary>
    [Serializable]
    public struct SteamDataFileAddress : IEquatable<SteamDataFileAddress>
    {
        /// <summary>
        /// The index of the file in the current users Steam Remote Storage system.
        /// </summary>
        public int fileIndex;
        /// <summary>
        /// The size of the file in bytes
        /// </summary>
        public int fileSize;
        /// <summary>
        /// The name of the fille as it appears on the Steam Remote Storage system.
        /// </summary>
        public string fileName;
        /// <summary>
        /// The UTC time stamp of the file as read from Steam Remote Storage.
        /// </summary>
        public DateTime UtcTimestamp;
        /// <summary>
        /// The local time translation of the UTC time stamp of the file.
        /// </summary>
        public DateTime LocalTimestamp
        {
            get
            {
                return UtcTimestamp.ToLocalTime();
            }
            set
            {
                UtcTimestamp = value.ToUniversalTime();
            }
        }

        /// <summary>
        /// Compares the equivlancy of a SteamDataFileAddress to another SteamDataFileAddress
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(SteamDataFileAddress obj1, SteamDataFileAddress obj2)
        {
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Compares the equivlancy of a SteamDataFileAddress to another SteamDataFileAddress
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(SteamDataFileAddress obj1, SteamDataFileAddress obj2)
        {
            return !obj1.Equals(obj2);
        }

        /// <summary>
        /// Compares the equivlancy of a SteamDataFileAddress to another SteamDataFileAddress
        /// </summary>
        /// <returns></returns>
        public bool Equals(SteamDataFileAddress other)
        {
            return fileIndex == other.fileIndex && fileName == other.fileName && fileSize == other.fileSize;
        }

        /// <summary>
        /// Compares the equivlancy of a SteamDataFileAddress to an object
        /// </summary>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj.GetType() == GetType() && Equals((SteamDataFileAddress)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance of an address.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = fileIndex.GetHashCode();
                hashCode = (hashCode * 397) ^ fileSize.GetHashCode();
                hashCode = (hashCode * 397) ^ fileName.GetHashCode();
                return hashCode;
            }
        }
    }
}
#endif