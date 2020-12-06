#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// Handles Friend chat message events
    /// See <a href="https://partner.steamgames.com/doc/api/steam_api#EChatEntryType">https://partner.steamgames.com/doc/api/steam_api#EChatEntryType</a> for more details
    /// </summary>
    [Serializable]
    public class FriendChatMessageEvent : UnityEvent<SteamUserData, string, EChatEntryType> { }
}
#endif