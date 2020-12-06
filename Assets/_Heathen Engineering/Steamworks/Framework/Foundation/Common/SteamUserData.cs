﻿#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// <para>Represents the sum of a users persona information as defined by Steam.</para>
    /// <para>The <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> object provides access and real time update of a given user's display name, avatar, game state informaiton and persona state information.</para>
    /// <para>This object should not be created manually but rather looked up or generated by calling <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings.GetUserData(CSteamID)"/></para>
    /// </summary>
    /// <example>
    /// <list type="bullet">
    /// <item>
    /// <description>To fetch the local user's <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> object you can simply grab the <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings.UserData"/> field however for demonstration purposes this is how you would look it up by CSteamID</description>
    /// <code>
    /// var localUser = settings.GetUserData(SteamUser.SteamId.m_SteamID);
    /// Debug.Log("The local user's Steam display name is " + localUser.DisplayName);
    /// </code>
    /// </item>
    /// </list>
    /// </example>
    [Serializable]
    [CreateAssetMenu(menuName = "Steamworks/Foundation/User Data")]
    public class SteamUserData : ScriptableObject
    {
        /// <summary>
        /// The CSteamID of the user this data belongs to
        /// </summary>
        public CSteamID SteamId;
        /// <summary>
        /// <para>The current display name of this user</para>
        /// <para>Note that this value will update automatically if the user updates its display name.</para>
        /// </summary>
        public string DisplayName;
        /// <summary>
        /// <para>Indicates that the user's avatar is loaded.</para>
        /// <para>Avatars are loaded automatically, if this is false it is because the system is waiting to download the avatar from the server.</para>
        /// </summary>
        public bool IconLoaded = false;
        /// <summary>
        /// A Texture2D representation of the users avatar.
        /// </summary>
        public Texture2D Avatar;
        /// <summary>
        /// <para>The current persona state of this user</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#EPersonaState">https://partner.steamgames.com/doc/api/ISteamFriends#EPersonaState</a>
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>k_EPersonaStateOffline	0	Friend is not currently logged on.</item>
        /// <item>k_EPersonaStateOnline	1	Friend is logged on.</item>
        /// <item>k_EPersonaStateBusy	2	User is on, but busy.</item>
        /// <item>k_EPersonaStateAway	3	Auto-away feature.</item>
        /// <item>k_EPersonaStateSnooze	4	Auto-away for a long time.</item>
        /// <item>k_EPersonaStateLookingToTrade	5	Online, trading..</item>
        /// <item>k_EPersonaStateLookingToPlay	6	Online, wanting to play.</item>
        /// <item>k_EPersonaStateMax	7	The total number of states. Only used for looping and validation.</item>
        /// </list>
        /// </remarks>
        public EPersonaState State;
        /// <summary>
        /// Is this user currently in a game.
        /// </summary>
        public bool InGame;
        /// <summary>
        /// <para>What is the users current game info if any</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#FriendGameInfo_t">https://partner.steamgames.com/doc/api/ISteamFriends#FriendGameInfo_t</a>
        /// </summary>
        public FriendGameInfo_t GameInfo;

        /// <summary>
        /// Occures when this users avatar is loaded
        /// </summary>
        public UnityEvent OnAvatarLoaded = new UnityEvent();
        /// <summary>
        /// Occures when this users avatar changes
        /// </summary>
        public UnityEvent OnAvatarChanged = new UnityEvent();
        /// <summary>
        /// Occures when this users name changes
        /// </summary>
        public UnityEvent OnNameChanged = new UnityEvent();
        /// <summary>
        /// Occures when the users state changes
        /// </summary>
        public UnityEvent OnStateChange = new UnityEvent();
        /// <summary>
        /// Occures when the user comes back on line
        /// </summary>
        public UnityEvent OnComeOnline = new UnityEvent();
        /// <summary>
        /// Occures when the user goes offline
        /// </summary>
        public UnityEvent OnGoneOffline = new UnityEvent();
        /// <summary>
        /// Occures when the users game info changes
        /// </summary>
        public UnityEvent OnGameChanged = new UnityEvent();

        /// <summary>
        /// Default the data stored in memory.
        /// </summary>
        public void ClearData()
        {
            SteamId = new CSteamID();
            DisplayName = string.Empty;
            IconLoaded = false;
            Avatar = null;
            State = EPersonaState.k_EPersonaStateOffline;
            InGame = false;
            GameInfo = new FriendGameInfo_t();
        }

        /// <summary>
        /// Open a chat with this user
        /// </summary>
        public void OpenChat()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("Chat", SteamId);
        }

        /// <summary>
        /// Open this users profile in the overlay
        /// </summary>
        public void OpenProfile()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("steamid", SteamId);
        }

        /// <summary>
        /// Open a trade window with this user in the overlay
        /// </summary>
        public void OpenTrade()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("jointrade", SteamId);
        }

        /// <summary>
        /// Open the stats for this user in the overlay
        /// </summary>
        public void OpenStats()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("stats", SteamId);
        }

        /// <summary>
        /// Open the achievements for this user in the overlay
        /// </summary>
        public void OpenAchievements()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("achievements", SteamId);
        }

        /// <summary>
        /// Open the add friend dialog in the overlay with this user selected
        /// </summary>
        public void OpenFriendAdd()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendadd", SteamId);
        }

        /// <summary>
        /// Open the remove friend dialog in the overlay with this user selected
        /// </summary>
        public void OpenFriendRemove()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendremove", SteamId);
        }

        /// <summary>
        /// Open the accept request dialog with respect to this users requests
        /// </summary>
        public void OpenRequestAccept()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendrequestaccept", SteamId);
        }

        /// <summary>
        /// Open the ignore request dialog with respect to this users requests
        /// </summary>
        public void OpenRequestIgnore()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendrequestignore", SteamId);
        }

        /// <summary>
        /// Send this user a Steam Friend Chat message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(string message)
        {
            return SteamFriends.ReplyToFriendMessage(SteamId, message);
        }
    }
}
#endif