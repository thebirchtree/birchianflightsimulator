﻿#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// <para>Represents the sum of a users persona information as defined by Steam.</para>
    /// <para>The <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> object provides access and real time update of a given user's display name, avatar, game state informaiton and persona state information.</para>
    /// <para>This object should not be created manually but rather looked up or generated by calling <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings.GetUserData(CSteamID)"/></para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// You should attempt to sync <see cref="SteamUserData"/> objects over the network. It is more efficent to sent the <see cref="ulong"/> value of the user's ID instead for example 
    /// <code>
    /// userData.id.m_SteamID;
    /// </code>
    /// This value can then be casts to a CSteamID or used on another client ot fetch the <see cref="SteamUserData"/> object that matches the id
    /// </para>
    /// </remarks>
    /// <example>
    /// <list type="bullet">
    /// <item>
    /// <description>To fetch the local user's <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> object you can the userData member of <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings.client"/>, however for demonstration purposes this is how you would look it up by CSteamID</description>
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
        #region Deprecated Members
        /// <summary>
        /// Please use id instead, this member will be removed in later updates
        /// </summary>
        [Obsolete("Please use id instead, this member will be removed in later updates", false)]
        public CSteamID SteamId { get { return id; } set { id = value; } }

        /// <summary>
        /// Please use iconLoaded instead, this member will be removed in later updates
        /// </summary>
        [Obsolete("Please use iconLoaded instead, this member will be removed in later updates", false)]
        public bool IconLoaded { get { return iconLoaded; } set { iconLoaded = value; } }

        /// <summary>
        /// Please use avatar instead, this member will be removed in later updates
        /// </summary>
        [Obsolete("Please use avatar instead, this member will be removed in later updates", false)]
        public Texture2D Avatar { get { return avatar; } set { avatar = value; } }
        #endregion

        /// <summary>
        /// The CSteamID of the user this data belongs to
        /// </summary>
        public CSteamID id;

        /// <summary>
        /// <para>The current display name of this user</para>
        /// <para>Note that this value will update automatically if the user updates its display name.</para>
        /// </summary>
        public string DisplayName => SteamFriends.GetFriendPersonaName(id);

        /// <summary>
        /// <para>Indicates that the user's avatar is loaded.</para>
        /// <para>Avatars are loaded automatically, if this is false it is because the system is waiting to download the avatar from the server.</para>
        /// </summary>
        [NonSerialized]
        public bool iconLoaded = false;

        /// <summary>
        /// A Texture2D representation of the users avatar.
        /// </summary>
        /// <remarks>
        /// Note that this field is not serialized and so will not presist between editor sessions.
        /// This is by design as the avatar should be loaded at run time and should not be sent when serialized such as over the network.
        /// </remarks>
        [NonSerialized]
        public Texture2D avatar;

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
        public EPersonaState State => SteamFriends.GetFriendPersonaState(id);

        /// <summary>
        /// Is this user currently in a game.
        /// </summary>
        public bool InGame { get { return SteamFriends.GetFriendGamePlayed(id, out _); } }

        /// <summary>
        /// <para>What is the users current game info if any</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#FriendGameInfo_t">https://partner.steamgames.com/doc/api/ISteamFriends#FriendGameInfo_t</a>
        /// </summary>
        /// <remarks>
        /// This calls <see cref="SteamFriends.GetFriendGamePlayed(CSteamID, out FriendGameInfo_t)"/> so you should cashe the result in a local variable and use that to perform any additional tests ... e.g. 
        /// <code>
        /// //Do not do this
        /// var gameId = userData.GameInfo.m_gameID;
        /// var lobbyId = userData.GameInfo.m_steamIDLobby;
        /// 
        /// //Instead do this
        /// var gameInfo = userData.GameInfo;
        /// var gameId = gameInfo.m_gameID;
        /// var lobbyId = gameInfo.m_steamIDLobby;
        /// </code>
        /// </remarks>
        public FriendGameInfo_t GameInfo
        {
            get
            {
                FriendGameInfo_t result;
                SteamFriends.GetFriendGamePlayed(id, out result);
                return result;
            }
        }

        /// <summary>
        /// Returns the 'Steam Level' of the user
        /// </summary>
        public int Level => SteamFriends.GetFriendSteamLevel(id);

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
        /// Returns the value for the specific rich presence entry on this user.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetRichPresenceValue(string key)
        {
            return SteamFriends.GetFriendRichPresence(id, key);
        }

        /// <summary>
        /// Returns a dictionary containing all this users rich presence entries.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetRichPresenceValues()
        {
            var results = new Dictionary<string, string>();

            var count = SteamFriends.GetFriendRichPresenceKeyCount(id);
            for (int i = 0; i < count; i++)
            {
                var key = SteamFriends.GetFriendRichPresenceKeyByIndex(id, i);
                results.Add(key, SteamFriends.GetFriendRichPresence(id, key));
            }

            return results;
        }

        /// <summary>
        /// Default the data stored in memory.
        /// </summary>
        public void ClearData()
        {
            id = new CSteamID();
            iconLoaded = false;
            avatar = null;
        }

#if !CONDITIONAL_COMPILE || (!UNITY_SERVER || UNITY_EDITOR)
        /// <summary>
        /// Open a chat with this user
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is only avialable on client builds e.g. this will not compile in headless builds.
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public void OpenChat()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("Chat", id);
        }

        /// <summary>
        /// Open this users profile in the overlay
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is only avialable on client builds e.g. this will not compile in headless builds.
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public void OpenProfile()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("steamid", id);
        }

        /// <summary>
        /// Open a trade window with this user in the overlay
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is only avialable on client builds e.g. this will not compile in headless builds.
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public void OpenTrade()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("jointrade", id);
        }

        /// <summary>
        /// Open the stats for this user in the overlay
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is only avialable on client builds e.g. this will not compile in headless builds.
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public void OpenStats()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("stats", id);
        }

        /// <summary>
        /// Open the achievements for this user in the overlay
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is only avialable on client builds e.g. this will not compile in headless builds.
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public void OpenAchievements()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("achievements", id);
        }

        /// <summary>
        /// Open the add friend dialog in the overlay with this user selected
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is only avialable on client builds e.g. this will not compile in headless builds.
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public void OpenFriendAdd()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendadd", id);
        }

        /// <summary>
        /// Open the remove friend dialog in the overlay with this user selected
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is only avialable on client builds e.g. this will not compile in headless builds.
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public void OpenFriendRemove()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendremove", id);
        }

        /// <summary>
        /// Open the accept request dialog with respect to this users requests
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is only avialable on client builds e.g. this will not compile in headless builds.
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public void OpenRequestAccept()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendrequestaccept", id);
        }

        /// <summary>
        /// Open the ignore request dialog with respect to this users requests
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is only avialable on client builds e.g. this will not compile in headless builds.
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public void OpenRequestIgnore()
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendrequestignore", id);
        }

        /// <summary>
        /// Send this user a Steam Friend Chat message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method is only avialable on client builds e.g. this will not compile in headless builds.
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public bool SendMessage(string message)
        {
            return SteamFriends.ReplyToFriendMessage(id, message);
        }
#endif
    }
}
#endif