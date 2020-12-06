#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.Events;
using HeathenEngineering.Serializable;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// <para>The root of Heathen Engieering's Steamworks system. <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> provides access to all core funcitonality including stats, achievements, the friend system and the overlay system.</para>
    /// <para>This object defines your application's ID, your local user's <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> object and manages the lists of all known users.</para>
    /// </summary>
    [CreateAssetMenu(menuName = "Steamworks/Foundation/Steam Settings")]
    public class SteamSettings : ScriptableObject
    {
        public bool EnableGameServerInit = false;
        /// <summary>
        /// The local players <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/>
        /// </summary>
        public SteamUserData UserData;
        /// <summary>
        /// The current applicaiton ID
        /// </summary>
        public AppId_t ApplicationId = new AppId_t(0x0);
        /// <summary>
        /// The last known player count
        /// This value is refreshed by calling <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings.RefreshPlayerCount()"/>
        /// </summary>
        public int LastKnownPlayerCount;
        /// <summary>
        /// active instance of <see cref="HeathenEngineering.SteamApi.Foundation.HeathensSteamOverlay"/>
        /// </summary>
        public HeathensSteamOverlay Overlay = new HeathensSteamOverlay();
        /// <summary>
        /// The list of <see cref="HeathenEngineering.SteamApi.Foundation.SteamStatData"/> tracked by the system
        /// </summary>
        public List<SteamStatData> stats;
        /// <summary>
        /// The list of <see cref="HeathenEngineering.SteamApi.Foundation.SteamAchievementData"/> tracked by the system
        /// </summary>
        public List<SteamAchievementData> achievements;
        /// <summary>
        /// A disctionary of <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> keyed on the <see cref="ulong"/> id of the user.
        /// </summary>
        public Dictionary<ulong, SteamUserData> KnownUsers = new Dictionary<ulong, SteamUserData>();

        /// <summary>
        /// The position of the Steam notification panel relative to the game window
        /// </summary>
        [Header("Overlay Notifications")]
        public ENotificationPosition NotificationPosition = ENotificationPosition.k_EPositionBottomRight;
        /// <summary>
        /// The offset of the Steam notification panel relative to its notification position
        /// </summary>
        public Vector2Int NotificationInset;

        #region Events
        /// <summary>
        /// Occures on load of a Steam avatar
        /// </summary>
        [HideInInspector]
        public UnityAvatarImageLoadedEvent OnAvatarLoaded;
        /// <summary>
        /// Ocucres on change of Steam User Data persona information
        /// </summary>
        [HideInInspector]
        public UnityPersonaStateChangeEvent OnPersonaStateChanged;
        /// <summary>
        /// Occures when user stats and achievements are recieved from Valve
        /// </summary>
        [HideInInspector]
        public UnityUserStatsReceivedEvent OnUserStatsReceived;
        /// <summary>
        /// Occures when user stats are stored to Valve
        /// </summary>
        [HideInInspector]
        public UnityUserStatsStoredEvent OnUserStatsStored;
        /// <summary>
        /// Occures when the Steam overlay is activated / shown
        /// </summary>
        [HideInInspector]
        public UnityBoolEvent OnOverlayActivated;
        /// <summary>
        /// Occures when Achivements are stored to Valve
        /// </summary>
        [HideInInspector]
        public UnityUserAchievementStoredEvent OnAchievementStored;

        /// <summary>
        /// Occures when a chat message from a friend is recieved.
        /// </summary>
        [HideInInspector]
        public FriendChatMessageEvent OnRecievedFriendChatMessage;

        /// <summary>
        /// Occures as the result of a RefreshPlayerCount call
        /// </summary>
        [HideInInspector]
        public UnityNumberOfCurrentPlayersResultEvent OnNumberOfCurrentPlayersResult;
        #endregion

        private CGameID m_GameID;
        private bool m_bRequestedStats;
        private bool m_bStatsValid;

        private Callback<AvatarImageLoaded_t> avatarLoadedCallback;
        private Callback<PersonaStateChange_t> personaStateChange;
        private Callback<UserStatsReceived_t> m_UserStatsReceived;
        private Callback<UserStatsStored_t> m_UserStatsStored;
        /// <summary>
        /// For internal user
        /// </summary>
        public Callback<GameOverlayActivated_t> m_GameOverlayActivated;
        private Callback<UserAchievementStored_t> m_UserAchievementStored;
        private Callback<GameConnectedFriendChatMsg_t> m_GameConnectedFrinedChatMsg;
        private CallResult<NumberOfCurrentPlayers_t> m_OnNumberOfCurrentPlayersCallResult;

        #region Achievement System
        /// <summary>
        /// <para>Stores the stats and achievements to Valve</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#StoreStats">https://partner.steamgames.com/doc/api/ISteamUserStats#StoreStats</a>
        /// </summary>
        /// <remarks>
        /// This must be called in order to store updated stats to the backend. Note that this will get called when the game closes.
        /// </remarks>
        public void StoreStatsAndAchievements()
        {
            SteamUserStats.StoreStats();
        }

        /// <summary>
        /// Registeres the achievement callbacks
        /// </summary>
        public void RegisterAchievementsSystem()
        {
            // Cache the GameID for use in the Callbacks
            m_GameID = new CGameID(SteamUtils.GetAppID());

            m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(HandleUserStatsReceived);
            m_UserStatsStored = Callback<UserStatsStored_t>.Create(HandleUserStatsStored);
            m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(HandleAchievementStored);
            m_OnNumberOfCurrentPlayersCallResult = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);

            // These need to be reset to get the stats upon an Assembly reload in the Editor.
            m_bRequestedStats = false;
            m_bStatsValid = false;
        }

        /// <summary>
        /// <para>Requests the current users stats from Valve servers</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#RequestCurrentStats">https://partner.steamgames.com/doc/api/ISteamUserStats#RequestCurrentStats</a>
        /// </summary>
        /// <returns>Returns true if the server accepted the request.</returns>
        public bool RequestCurrentStats()
        {
            var handle = SteamUserStats.GetNumberOfCurrentPlayers();
            m_OnNumberOfCurrentPlayersCallResult.Set(handle);
            return SteamUserStats.RequestCurrentStats();
        }

        /// <summary>
        /// <para>
        /// Requests the count of current players from Steam for this application
        /// On return this will update the SteamSettings.LastKnownPlayerCount value
        /// and trigger the OnNumberOfCurrentPlayersResult event for the SteamSettings 
        /// object and the connected Foundation Manager
        /// </para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#GetNumberOfCurrentPlayers">https://partner.steamgames.com/doc/api/ISteamUserStats#GetNumberOfCurrentPlayers</a>
        /// </summary>
        public void RefreshPlayerCount()
        {
            var handle = SteamUserStats.GetNumberOfCurrentPlayers();
            m_OnNumberOfCurrentPlayersCallResult.Set(handle);
        }

        private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure)
        {
            if(!bIOFailure)
            {
                if (pCallback.m_bSuccess == 1)
                    LastKnownPlayerCount = pCallback.m_cPlayers;

                if (OnNumberOfCurrentPlayersResult != null)
                    OnNumberOfCurrentPlayersResult.Invoke(pCallback);
            }
        }

        private void HandleUserStatsReceived(UserStatsReceived_t pCallback)
        {
            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (EResult.k_EResultOK == pCallback.m_eResult)
                {
                    m_bStatsValid = true;

                    // load achievements
                    foreach (SteamAchievementData ach in achievements)
                    {
                        bool ret = SteamUserStats.GetAchievement(ach.achievementId.ToString(), out ach.isAchieved);
                        if (ret)
                        {
                            ach.displayName = SteamUserStats.GetAchievementDisplayAttribute(ach.achievementId, "name");
                            ach.displayDescription = SteamUserStats.GetAchievementDisplayAttribute(ach.achievementId, "desc");
                            ach.hidden = SteamUserStats.GetAchievementDisplayAttribute(ach.achievementId, "hidden") == "1";
                        }
                        else
                        {
                            Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + ach.achievementId + "\nIs it registered in the Steam Partner site?");
                        }
                    }

                    foreach (var stat in stats)
                    {
                        if (stat.DataType == SteamStatData.StatDataType.Float)
                        {
                            float rValue;
                            if (SteamUserStats.GetStat(stat.statName, out rValue))
                                stat.InternalUpdateValue(rValue);
                            else
                                Debug.LogWarning("SteamUserStats.GetAchievement failed for Stat " + stat.statName + "\nIs it registered in the Steam Partner site and the correct data type?");
                        }
                        else
                        {
                            int rValue;
                            if (SteamUserStats.GetStat(stat.statName, out rValue))
                                stat.InternalUpdateValue(rValue);
                            else
                                Debug.LogWarning("SteamUserStats.GetAchievement failed for Stat " + stat.statName + "\nIs it registered in the Steam Partner site and the correct data type?");
                        }
                    }

                    OnUserStatsReceived.Invoke(pCallback);
                }
                else
                {
                    Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
                }
            }
        }

        private void HandleUserStatsStored(UserStatsStored_t pCallback)
        {
            // we may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (EResult.k_EResultOK == pCallback.m_eResult)
                {
                    OnUserStatsStored.Invoke(pCallback);
                }
                else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
                {
                    // One or more stats we set broke a constraint. They've been reverted,
                    // and we should re-iterate the values now to keep in sync.
                    Debug.Log("StoreStats - some failed to validate, re-syncing data now in an attempt to correct.");
                    // Fake up a callback here so that we re-load the values.
                    UserStatsReceived_t callback = new UserStatsReceived_t();
                    callback.m_eResult = EResult.k_EResultOK;
                    callback.m_nGameID = (ulong)m_GameID;
                    HandleUserStatsReceived(callback);
                }
                else
                {
                    Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
                }
            }
        }

        private void HandleAchievementStored(UserAchievementStored_t pCallback)
        {
            // We may get callbacks for other games' stats arriving, ignore them
            if ((ulong)m_GameID == pCallback.m_nGameID)
            {
                if (0 == pCallback.m_nMaxProgress)
                {
                    Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' unlocked!");
                }
                else
                {
                    Debug.Log("Achievement '" + pCallback.m_rgchAchievementName + "' progress callback, (" + pCallback.m_nCurProgress + "," + pCallback.m_nMaxProgress + ")");
                }

                OnAchievementStored.Invoke(pCallback);
            }
        }

        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement</a>
        /// </summary>
        public void UnlockAchievement(uint achievementIndex)
        {
            SteamAchievementData target = achievements[System.Convert.ToInt32(achievementIndex)];
            if (target != default(SteamAchievementData) && !target.isAchieved)
                UnlockAchievementData(target);
        }

        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement</a>
        /// </summary>
        public void UnlockAchievementData(SteamAchievementData data)
        {
            data.Unlock();
        }

        /// <summary>
        /// <para>Resets the unlock status of an achievmeent.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement</a>
        /// </summary>
        /// <param name="achievementIndex">The index of the registered achievment you wish to reset.</param>
        public void ClearAchievement(uint achievementIndex)
        {
            SteamAchievementData target = achievements[System.Convert.ToInt32(achievementIndex)];
            if (target != default(SteamAchievementData) && !target.isAchieved)
                ClearAchievement(target);
        }

        /// <summary>
        /// <para>Resets the unlock status of an achievmeent.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement</a>
        /// </summary>
        /// <param name="data">The achievement you wish to reset.</param>
        public void ClearAchievement(SteamAchievementData data)
        {
            data.ClearAchievement();
        }
        #endregion

        #region Friend System
        /// <summary>
        /// For internal use, this regisers the Friend system and is called by the <see cref="HeathenEngineering.SteamApi.Foundation.SteamworksFoundationManager"/> as required.
        /// </summary>
        /// <param name="data"></param>
        public void RegisterFriendsSystem(SteamUserData data = null)
        {
            avatarLoadedCallback = Callback<AvatarImageLoaded_t>.Create(HandleAvatarLoaded);
            personaStateChange = Callback<PersonaStateChange_t>.Create(HandlePersonaStatReceived);
            m_GameConnectedFrinedChatMsg = Callback<GameConnectedFriendChatMsg_t>.Create(HandleGameConnectedFriendMsg);

            if (OnRecievedFriendChatMessage == null)
                OnRecievedFriendChatMessage = new FriendChatMessageEvent();

            if (OnAvatarLoaded == null)
                OnAvatarLoaded = new UnityAvatarImageLoadedEvent();

            if (OnPersonaStateChanged == null)
                OnPersonaStateChanged = new UnityPersonaStateChangeEvent();

            if (data != null)
                UserData = data;

            if (UserData == null)
                UserData = ScriptableObject.CreateInstance<SteamUserData>();

            UserData.SteamId = SteamUser.GetSteamID();
            UserData.DisplayName = SteamFriends.GetFriendPersonaName(UserData.SteamId);
            UserData.State = SteamFriends.GetFriendPersonaState(UserData.SteamId);
            UserData.InGame = SteamFriends.GetFriendGamePlayed(UserData.SteamId, out UserData.GameInfo);

            KnownUsers.Clear();
            KnownUsers.Add(UserData.SteamId.m_SteamID, UserData);

            int imageId = SteamFriends.GetLargeFriendAvatar(UserData.SteamId);
            //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
            if (imageId > 0)
            {
                ApplyAvatarImage(UserData, imageId);
            }
        }

        private void HandleAvatarLoaded(AvatarImageLoaded_t data)
        {
            if (KnownUsers.ContainsKey(data.m_steamID.m_SteamID))
            {
                SteamUserData u = KnownUsers[data.m_steamID.m_SteamID];
                ApplyAvatarImage(u, data.m_iImage);
                if (u.OnAvatarLoaded == null)
                    u.OnAvatarLoaded = new UnityEngine.Events.UnityEvent();
                u.OnAvatarLoaded.Invoke();
            }
            else
            {
                var n = ScriptableObject.CreateInstance<SteamUserData>();
                n.SteamId = data.m_steamID;
                n.DisplayName = SteamFriends.GetFriendPersonaName(n.SteamId);
                n.State = SteamFriends.GetFriendPersonaState(n.SteamId);
                n.InGame = SteamFriends.GetFriendGamePlayed(n.SteamId, out n.GameInfo);
                KnownUsers.Add(n.SteamId.m_SteamID, n);
                ApplyAvatarImage(n, data.m_iImage);
                n.OnAvatarLoaded.Invoke();
            }

            OnAvatarLoaded.Invoke(data);
        }

        private void HandleGameConnectedFriendMsg(GameConnectedFriendChatMsg_t callback)
        {
            string message;
            EChatEntryType chatType;
            SteamFriends.GetFriendMessage(callback.m_steamIDUser, callback.m_iMessageID, out message, 2048, out chatType);
            OnRecievedFriendChatMessage.Invoke(GetUserData(callback.m_steamIDUser), message, chatType);
        }

        private void HandlePersonaStatReceived(PersonaStateChange_t pCallback)
        {
            SteamUserData target = null;
            if (KnownUsers.ContainsKey(pCallback.m_ulSteamID))
            {
                target = KnownUsers[pCallback.m_ulSteamID];
            }
            else
            {
                target = ScriptableObject.CreateInstance<SteamUserData>();
                target.SteamId = new CSteamID(pCallback.m_ulSteamID);
                KnownUsers.Add(target.SteamId.m_SteamID, target);
                target.DisplayName = SteamFriends.GetFriendPersonaName(target.SteamId);
                target.State = SteamFriends.GetFriendPersonaState(target.SteamId);
                target.InGame = SteamFriends.GetFriendGamePlayed(target.SteamId, out target.GameInfo);
            }

            switch (pCallback.m_nChangeFlags)
            {
                case EPersonaChange.k_EPersonaChangeAvatar:
                    try
                    {
                        int imageId = SteamFriends.GetLargeFriendAvatar(SteamUser.GetSteamID());
                        if (imageId > 0)
                        {
                            target.IconLoaded = true;
                            uint imageWidth, imageHeight;
                            SteamUtils.GetImageSize(imageId, out imageWidth, out imageHeight);
                            byte[] imageBuffer = new byte[4 * imageWidth * imageHeight];
                            if (SteamUtils.GetImageRGBA(imageId, imageBuffer, imageBuffer.Length))
                            {
                                target.Avatar.LoadRawTextureData(SteamUtilities.FlipImageBufferVertical((int)imageWidth, (int)imageHeight, imageBuffer));
                                target.Avatar.Apply();
                                target.OnAvatarChanged.Invoke();
                            }
                        }
                    }
                    catch { }
                    break;
                case EPersonaChange.k_EPersonaChangeComeOnline:
                    target.State = SteamFriends.GetFriendPersonaState(target.SteamId);
                    if (target.OnComeOnline != null)
                        target.OnComeOnline.Invoke();
                    if (target.OnStateChange != null)
                        target.OnStateChange.Invoke();
                    break;
                case EPersonaChange.k_EPersonaChangeGamePlayed:
                    target.InGame = SteamFriends.GetFriendGamePlayed(target.SteamId, out target.GameInfo);
                    if (target.OnGameChanged != null)
                        target.OnGameChanged.Invoke();
                    if (target.OnStateChange != null)
                        target.OnStateChange.Invoke();
                    break;
                case EPersonaChange.k_EPersonaChangeGoneOffline:
                    target.State = SteamFriends.GetFriendPersonaState(target.SteamId);
                    if (target.OnGoneOffline != null)
                        target.OnGoneOffline.Invoke();
                    if (target.OnStateChange != null)
                        target.OnStateChange.Invoke();
                    break;
                case EPersonaChange.k_EPersonaChangeName:
                    target.DisplayName = SteamFriends.GetFriendPersonaName(target.SteamId);
                    if (target.OnNameChanged != null)
                        target.OnNameChanged.Invoke();
                    break;
            }

            OnPersonaStateChanged.Invoke(pCallback);
        }

        private void ApplyAvatarImage(SteamUserData user, int imageId)
        {
            uint width, height;
            SteamUtils.GetImageSize(imageId, out width, out height);
            user.Avatar = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
            int bufferSize = (int)(width * height * 4);
            byte[] imageBuffer = new byte[bufferSize];
            SteamUtils.GetImageRGBA(imageId, imageBuffer, bufferSize);
            user.Avatar.LoadRawTextureData(SteamUtilities.FlipImageBufferVertical((int)width, (int)height, imageBuffer));
            user.Avatar.Apply();
            user.IconLoaded = true;
        }

        /// <summary>
        /// <para>Set rather or not the system should listen for Steam Friend chat messages</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#SetListenForFriendsMessages">https://partner.steamgames.com/doc/api/ISteamFriends#SetListenForFriendsMessages</a>
        /// </summary>
        /// <param name="isOn">True if you want to turn this feature on, otherwise false</param>
        /// <returns>True if successfully enabled, otherwise false</returns>
        public bool ListenForFriendMessages(bool isOn)
        {
            return SteamFriends.SetListenForFriendsMessages(isOn);
        }

        /// <summary>
        /// Send a Steam Friend Chat message to the indicated user
        /// </summary>
        /// <param name="friend"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendFriendChatMessage(SteamUserData friend, string message)
        {
            return friend.SendMessage(message);
        }

        /// <summary>
        /// <para>Send a Steam Friend Chat message to the indicated user</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage">https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage</a>
        /// </summary>
        /// <param name="friend">The friend you wish to send the message to</param>
        /// <param name="message">The message to be sent</param>
        /// <returns></returns>
        public bool SendFriendChatMessage(ulong friendId, string message)
        {
            return SendFriendChatMessage(new CSteamID(friendId), message);
        }

        /// <summary>
        /// <para>Send a Steam Friend Chat message to the indicated user</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage">https://partner.steamgames.com/doc/api/ISteamFriends#ReplyToFriendMessage</a>
        /// </summary>
        /// <param name="friend">The friend you wish to send the message to</param>
        /// <param name="message">The message to be sent</param>
        /// <returns></returns>
        public bool SendFriendChatMessage(CSteamID friend, string message)
        {
            return SteamFriends.ReplyToFriendMessage(friend, message);
        }

        /// <summary>
        /// <para>Requests the users avatar from Valve
        /// This is handled by the Friends subsystem but can be called manually to force a refresh</para>  
        /// <a href="https://partner.steamgames.com/doc/api/ISteamFriends#GetLargeFriendAvatar">https://partner.steamgames.com/doc/api/ISteamFriends#GetLargeFriendAvatar</a>
        /// </summary>
        /// <param name="userData">The user whoes avatar should be updated</param>
        public void RefreshAvatar(SteamUserData userData)
        {
            int imageId = SteamFriends.GetLargeFriendAvatar(userData.SteamId);
            //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
            if (imageId > 0)
            {
                ApplyAvatarImage(userData, imageId);
            }
        }

        /// <summary>
        /// <para>Locates the Steam User Data for the user provided 
        /// This will read from the friends subsystem if availabel or will create a new entery if none is found</para>
        /// </summary>
        /// <param name="steamID">THe user to find or load as required.</param>
        /// <returns>The <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> for the indicated user.</returns>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description>Get the <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> of a Steam user whose ID is stored in myFriendId</description>
        /// <code>
        /// var userData = settings.GetUserData(myFriendId);
        /// Debug.Log("Located the user data for " + userData.DisplayName);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public SteamUserData GetUserData(CSteamID steamID)
        {
            if (KnownUsers.ContainsKey(steamID.m_SteamID))
            {
                var n = KnownUsers[steamID.m_SteamID];

                int imageId = SteamFriends.GetLargeFriendAvatar(steamID);
                //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
                if (imageId > 0)
                {
                    ApplyAvatarImage(n, imageId);
                }

                return n;
            }
            else
            {
                SteamUserData n = CreateInstance<SteamUserData>();
                n.SteamId = steamID;
                n.DisplayName = SteamFriends.GetFriendPersonaName(steamID);
                n.State = SteamFriends.GetFriendPersonaState(steamID);
                n.InGame = SteamFriends.GetFriendGamePlayed(steamID, out n.GameInfo);

                KnownUsers.Add(steamID.m_SteamID, n);

                int imageId = SteamFriends.GetLargeFriendAvatar(steamID);
                //If the image is already in cashe then get it from there else the avatar loaded callback will catch and load
                if (imageId > 0)
                {
                    ApplyAvatarImage(n, imageId);
                }

                return n;
            }
        }
        #endregion

        #region Overlay System
        /// <summary>
        /// Called by the Heathen Steam Manager when the GameOverlayActivated callback is triggered
        /// </summary>
        /// <param name="data"></param>
        public void HandleOnOverlayOpen(GameOverlayActivated_t data)
        {
            Overlay.HandleOnOverlayOpen(data);
            OnOverlayActivated.Invoke(Overlay.IsOpen);
        }

        /// <summary>
        /// <para>Sets the overlay notification positon.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUtils#SetOverlayNotificationPosition">https://partner.steamgames.com/doc/api/ISteamUtils#SetOverlayNotificationPosition</a>
        /// </summary>
        /// <param name="position">The ENotificationPosition to set, see <a href="https://partner.steamgames.com/doc/api/steam_api#ENotificationPosition">https://partner.steamgames.com/doc/api/steam_api#ENotificationPosition</a> for details</param>
        public void SetNotificationPosition(ENotificationPosition position)
        {
            Steamworks.SteamUtils.SetOverlayNotificationPosition(NotificationPosition);
            NotificationPosition = position;
        }

        /// <summary>
        /// Updates the notification inset
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public void SetNotificationInset(int X, int Y)
        {
            Steamworks.SteamUtils.SetOverlayNotificationInset(X, Y);
            NotificationInset = new Vector2Int(X, Y);
        }

        /// <summary>
        /// Updates the notification inset
        /// </summary>
        /// <param name="inset"></param>
        public void SetNotificationInset(Vector2Int inset)
        {
            Steamworks.SteamUtils.SetOverlayNotificationInset(inset.x, inset.y);
            NotificationInset = inset;
        }
        #endregion
    }
}
#endif