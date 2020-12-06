#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.Events;
using HeathenEngineering.Scriptable;
using HeathenEngineering.Serializable;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// <para>This replaces the SteamManager concept from classic Steamworks.NET</para>
    /// <para>The <see cref="SteamworksFoundationManager"/> initalizes the client SteamAPI and handles callbacks for the system. For the convenance of users using a singleton model this class also provides a <see cref="Instance"/> static member and wraps all major funcitons and event of the <see cref="SteamSettings"/> object.</para>
    /// </summary>
    [DisallowMultipleComponent]
    public class SteamworksFoundationManager : MonoBehaviour
    {
        #region Editor Exposed Values
        public SteamSettings Settings;
        public BoolReference _doNotDistroyOnLoad = new BoolReference(false);
        public BoolReference _Initialized;
        public UnityEvent OnSteamInitalized;
        public UnityStringEvent OnSteamInitalizationError;
        public UnityBoolEvent OnOverlayActivated;
        public UnityUserStatsReceivedEvent OnUserStatsRecieved;
        public UnityUserStatsStoredEvent OnUserStatsStored;
        public UnityNumberOfCurrentPlayersResultEvent OnNumberOfCurrentPlayersResult;
        public UnityUserAchievementStoredEvent OnAchievementStored;
        public UnityAvatarImageLoadedEvent OnAvatarLoaded;
        public UnityPersonaStateChangeEvent OnPersonaStateChanged;
        public FriendChatMessageEvent OnRecievedFriendChatMessage;
        #endregion

        private static SteamworksFoundationManager s_instance;
        /// <summary>
        /// <para>For use with the singleton approch</para>
        /// <para>Heathen Engineering recomends the use of direct references. Note that all required funcitonality of the <see cref="SteamworksFoundationManager"/> is available in the <see cref="SteamSettings"/> scriptable object which can be referenced on any game object directly as it is not a scene object it is not limited to references within the current scene.</para>
        /// </summary>
        public static SteamworksFoundationManager Instance
        {
            get
            {
                return s_instance;
            }
        }

        /// <summary>
        /// For internal use
        /// </summary>
        public static bool s_EverInialized;
        private ENotificationPosition currentNotificationPosition = ENotificationPosition.k_EPositionBottomRight;
        private Vector2Int currentNotificationIndent = Vector2Int.zero;

        /// <summary>
        /// Is the foundaiton manager initalized and ready to use
        /// </summary>
        public static bool Initialized
        {
            get
            {
                return Instance._Initialized.Value;
            }
        }

        private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
        private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }

        private void Awake()
        {
            // Only one instance of SteamManager at a time!
            if (s_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            s_instance = this;

            if (s_EverInialized)
            {
                // This is almost always an error.
                // The most common case where this happens is when SteamManager gets destroyed because of Application.Quit(),
                // and then some Steamworks code in some other OnDestroy gets called afterwards, creating a new SteamManager.
                // You should never call Steamworks functions in OnDestroy, always prefer OnDisable if possible.
                OnSteamInitalizationError.Invoke("Tried to Initialize the SteamAPI twice in one session!");
                throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
            }

            if (_doNotDistroyOnLoad.Value)
                DontDestroyOnLoad(gameObject);

            if (!Packsize.Test())
            {
                Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
                OnSteamInitalizationError.Invoke("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
            }

            if (!DllCheck.Test())
            {
                Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
                OnSteamInitalizationError.Invoke("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            }

            try
            {
                // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
                // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

                // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
                // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
                // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
                //AppId = SteamAppId != null ? new AppId_t(SteamAppId.Value) : AppId_t.Invalid;
                if (SteamAPI.RestartAppIfNecessary(Settings.ApplicationId))
                {
                    Application.Quit();
                    return;
                }
            }
            catch (System.DllNotFoundException e)
            { // We catch this exception here, as it will be the first occurence of it.
                Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);
                OnSteamInitalizationError.Invoke("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e);
                Application.Quit();
                return;
            }

            // Initializes the Steamworks API.
            // If this returns false then this indicates one of the following conditions:
            // [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
            // [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the [code-inline]steam_appid.txt[/code-inline] file.
            // [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
            // [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
            // [*] Your App ID is not completely set up, i.e. in [code-inline]Release State: Unavailable[/code-inline], or it's missing default packages.
            // Valve's documentation for this is located here:
            // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
            if (Application.isBatchMode && Settings.EnableGameServerInit)
            {
                Debug.Log("Game Server Initalization detected, Steam Initalization will occure from the Heathen Steam Game Server Manager.");
            }
            else
            {
                _Initialized.Value = SteamAPI.Init();

                if (!_Initialized.Value)
                {
                    Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
                    OnSteamInitalizationError.Invoke("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.");
                    return;
                }

                s_EverInialized = true;
                OnSteamInitalized.Invoke();
                Debug.Log("Steam client Initalized!");
            }
        }

        // This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
        private void OnEnable()
        {
            if (s_instance == null)
            {
                s_instance = this;
            }

            if (!_Initialized.Value)
            {
                return;
            }

            if (m_SteamAPIWarningMessageHook == null)
            {
                // Set up our callback to recieve warning messages from Steam.
                // You must launch with "-debug_steamapi" in the launch args to recieve warnings.
                m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
            }

            if (Settings.Overlay == null)
                Settings.Overlay = new HeathensSteamOverlay();

            //Register the overlay callbacks
            if (Settings.Overlay != null)
            {
                Settings.m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(Settings.HandleOnOverlayOpen);
                Settings.OnOverlayActivated.AddListener(OnOverlayActivated.Invoke);
            }

            //Register the achievements system
            Settings.RegisterAchievementsSystem();
            Settings.OnAchievementStored.AddListener(OnAchievementStored.Invoke);
            Settings.OnUserStatsReceived.AddListener(OnUserStatsRecieved.Invoke);
            Settings.OnUserStatsStored.AddListener(OnUserStatsStored.Invoke);
            Settings.OnNumberOfCurrentPlayersResult.AddListener(OnNumberOfCurrentPlayersResult.Invoke);
            Settings.RequestCurrentStats();

            //Register the friends system
            Settings.RegisterFriendsSystem(Settings.UserData);
            Settings.OnAvatarLoaded.AddListener(OnAvatarLoaded.Invoke);
            Settings.OnPersonaStateChanged.AddListener(OnPersonaStateChanged.Invoke);
            Settings.OnRecievedFriendChatMessage.AddListener(OnRecievedFriendChatMessage.Invoke);
        }
        
        // OnApplicationQuit gets called too early to shutdown the SteamAPI.
        // Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
        // Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
        private void OnDestroy()
        {
            if (Settings != null && Settings.UserData != null)
                Settings.UserData.ClearData();

            if (s_instance != this)
            {
                return;
            }
            s_instance = null;

            if (!_Initialized.Value)
            {
                return;
            }

            SteamAPI.Shutdown();
        }

        private void Update()
        {
            if (!_Initialized.Value)
            {
                return;
            }

            if (!Application.isBatchMode || (Application.isBatchMode && !Settings.EnableGameServerInit))
            {
                // Run Steam client callbacks
                SteamAPI.RunCallbacks();
            }

            if (Settings != null)
            {
                //Refresh the notification position
                if (currentNotificationPosition != Settings.NotificationPosition)
                {
                    currentNotificationPosition = Settings.NotificationPosition;
                    Settings.SetNotificationPosition(Settings.NotificationPosition);
                }

                if (currentNotificationIndent != Settings.NotificationInset)
                {
                    currentNotificationIndent = Settings.NotificationInset;
                    Settings.SetNotificationInset(Settings.NotificationInset);
                }
            }
        }
        
        #region Static Steam Overlay Settings Wrapper
        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.SetNotificationPosition(ENotificationPosition)"/>
        /// </summary>
        /// <param name="position"></param>
        public static void _SetNotificationPosition(ENotificationPosition position)
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.SetNotificationPosition(position);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.SetNotificationInset(Vector2Int)"/>
        /// </summary>
        /// <param name="position"></param>
        public static void _SetNotificationInset(Vector2Int inset)
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.SetNotificationInset(inset);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenStore"/>
        /// </summary>
        public static void _OpenStore()
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenStore();
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenStore"/>
        /// </summary>
        public static void _OpenStore(uint appId)
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenStore(appId);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenStore"/>
        /// </summary>
        public static void _OpenStore(uint appId, EOverlayToStoreFlag flag)
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenStore(appId, flag);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenStore"/>
        /// </summary>
        public static void _OpenStore(AppId_t appId, EOverlayToStoreFlag flag)
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenStore(appId, flag);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.Open"/>
        /// </summary>
        public static void _Open(string dialog)
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.Open(dialog);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenWebPage(string)"/>
        /// </summary>
        public static void _OpenWebPage(string URL)
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenWebPage(URL);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenFriends"/>
        /// </summary>
        public static void _OpenFriends()
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenFriends();
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenCommunity"/>
        /// </summary>
        public static void _OpenCommunity()
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenCommunity();
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenPlayers"/>
        /// </summary>
        public static void _OpenPlayers()
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenPlayers();
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenSettings"/>
        /// </summary>
        public static void _OpenSettings()
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenSettings();
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenOfficialGameGroup"/>
        /// </summary>
        public static void _OpenOfficialGameGroup()
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenOfficialGameGroup();
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenStats"/>
        /// </summary>
        public static void _OpenStats()
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenStats();
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenAchievements"/>
        /// </summary>
        public static void _OpenAchievements()
        {
            if (Instance != null && Instance.Settings != null)
                Instance.Settings.Overlay.OpenAchievements();
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenChat(CSteamID)"/>
        /// </summary>
        public static void _OpenChat(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.Settings.Overlay.OpenChat(user.SteamId);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenProfile(CSteamID)"/>
        /// </summary>
        public static void _OpenProfile(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.Settings.Overlay.OpenProfile(user.SteamId);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenTrade(CSteamID)"/>
        /// </summary>
        public static void _OpenTrade(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.Settings.Overlay.OpenTrade(user.SteamId);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenStats(CSteamID)"/>
        /// </summary>
        public static void _OpenStats(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.Settings.Overlay.OpenStats(user.SteamId);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenAchievements(CSteamID)"/>
        /// </summary>
        public static void _OpenAchievements(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.Settings.Overlay.OpenAchievements(user.SteamId);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenFriendAdd(CSteamID)"/>
        /// </summary>
        public static void _OpenFriendAdd(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.Settings.Overlay.OpenFriendAdd(user.SteamId);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenFriendRemove(CSteamID)"/>
        /// </summary>
        public static void _OpenFriendRemove(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.Settings.Overlay.OpenFriendRemove(user.SteamId);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenRequestAccept(CSteamID)"/>
        /// </summary>
        public static void _OpenRequestAccept(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.Settings.Overlay.OpenRequestAccept(user.SteamId);
        }

        /// <summary>
        /// Static wrapper to <see cref="SteamSettings.Overlay"/>'s <see cref="HeathensSteamOverlay.OpenRequestIgnore(CSteamID)"/>
        /// </summary>
        public static void _OpenRequestIgnore(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Instance.Settings.Overlay.OpenRequestIgnore(user.SteamId);
        }
        #endregion

        #region Steam Overlay Settings Wrapper
        /// <summary>
        /// This wraps back to the SteamManager to insure the displayed value is the correct value for the current position state
        /// e.g. same as setting HeathenSteamManager.Instance.NotificationPosition
        /// </summary>
        /// <param name="position"></param>
        public void SetNotificationPosition(ENotificationPosition position)
        {
            if (Settings != null)
                Settings.SetNotificationPosition(position);
        }

        public void SetNotificationInset(Vector2Int inset)
        {
            if (Settings != null)
                Settings.SetNotificationInset(inset);
        }

        /// <summary>
        /// Opens the overlay to the current games store page
        /// </summary>
        public void OpenStore()
        {
            if (Settings != null)
                Settings.Overlay.OpenStore();
        }

        /// <summary>
        /// Opens the overlay to the indicated applicaitons store page
        /// </summary>
        /// <param name="appId">Steam App Id of the applicaiton to open</param>
        public void OpenStore(uint appId)
        {
            if (Settings != null)
                Settings.Overlay.OpenStore(appId);
        }

        /// <summary>
        /// Opens the store page to the indicated applications store page with store options
        /// </summary>
        /// <param name="appId">Steam App Id of the applicaiton to open</param>
        /// <param name="flag">Modifies behavior of the page when opened</param>
        public void OpenStore(uint appId, EOverlayToStoreFlag flag)
        {
            if (Settings != null)
                Settings.Overlay.OpenStore(appId, flag);
        }

        /// <summary>
        /// Opens the store page to the indicated applications store page with store options
        /// </summary>
        /// <param name="appId">Steam App Id of the applicaiton to open</param>
        /// <param name="flag">Modifies behavior of the page when opened</param>
        public void OpenStore(AppId_t appId, EOverlayToStoreFlag flag)
        {
            if (Settings != null)
                Settings.Overlay.OpenStore(appId, flag);
        }

        /// <summary>
        /// From Steamworks ActivateGameOverlay
        /// </summary>
        /// <remarks>
        /// See https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay for details
        /// </remarks>
        /// <param name="dialog"></param>
        public void Open(string dialog)
        {
            if (Settings != null)
                Settings.Overlay.Open(dialog);
        }

        public void OpenWebPage(string URL)
        {
            if (Settings != null)
                Settings.Overlay.OpenWebPage(URL);
        }

        public void OpenFriends()
        {
            if (Settings != null)
                Settings.Overlay.OpenFriends();
        }

        public void OpenCommunity()
        {
            if (Settings != null)
                Settings.Overlay.OpenCommunity();
        }

        public void OpenPlayers()
        {
            if (Settings != null)
                Settings.Overlay.OpenPlayers();
        }

        public void OpenSettings()
        {
            if (Settings != null)
                Settings.Overlay.OpenSettings();
        }

        public void OpenOfficialGameGroup()
        {
            if (Settings != null)
                Settings.Overlay.OpenOfficialGameGroup();
        }

        public void OpenStats()
        {
            if (Settings != null)
                Settings.Overlay.OpenStats();
        }

        public void OpenAchievements()
        {
            if (Settings != null)
                Settings.Overlay.OpenAchievements();
        }

        public void OpenChat(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Settings.Overlay.OpenChat(user.SteamId);
        }

        public void OpenProfile(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Settings.Overlay.OpenProfile(user.SteamId);
        }

        public void OpenTrade(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Settings.Overlay.OpenTrade(user.SteamId);
        }

        public void OpenStats(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Settings.Overlay.OpenStats(user.SteamId);
        }

        public void OpenAchievements(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Settings.Overlay.OpenAchievements(user.SteamId);
        }

        public void OpenFriendAdd(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Settings.Overlay.OpenFriendAdd(user.SteamId);
        }

        public void OpenFriendRemove(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Settings.Overlay.OpenFriendRemove(user.SteamId);
        }

        public void OpenRequestAccept(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Settings.Overlay.OpenRequestAccept(user.SteamId);
        }

        public void OpenRequestIgnore(SteamUserData user)
        {
            if (user.SteamId.m_SteamID != SteamUser.GetSteamID().m_SteamID)
                Settings.Overlay.OpenRequestIgnore(user.SteamId);
        }
        #endregion

        #region Static Steam Friends Wrapper
        /// <summary>
        /// The Steam User Data for the player
        /// </summary>
        public static SteamUserData _UserData
        {
            get
            {
                if (Instance != null && Instance.Settings != null)
                    return Instance.Settings.UserData;
                else
                    return null;
            }
        }

        public static string _GetUserName(ulong steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
                return u.DisplayName;
            else
                return string.Empty;
        }

        public static FriendGameInfo_t _GetUserGameInfo(ulong steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
                return u.GameInfo;
            else
                return default(FriendGameInfo_t);
        }

        public static Texture2D _GetUserAvatar(ulong steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
            {
                if (!u.IconLoaded)
                {
                    _RefreshAvatar(u);
                }
                return u.Avatar;
            }
            else
                return null;
        }

        public static SteamUserData _GetUserData(ulong steamId)
        {
            return Instance.Settings.GetUserData(new CSteamID(steamId));
        }

        public static string _GetUserName(CSteamID steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
                return u.DisplayName;
            else
                return string.Empty;
        }

        public static FriendGameInfo_t _GetUserGameInfo(CSteamID steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
                return u.GameInfo;
            else
                return default(FriendGameInfo_t);
        }

        public static Texture2D _GetUserAvatar(CSteamID steamId)
        {
            var u = _GetUserData(steamId);
            if (u != null)
            {
                if (!u.IconLoaded)
                {
                    _RefreshAvatar(u);
                }
                return u.Avatar;
            }
            else
                return null;
        }

        public static SteamUserData _GetUserData(CSteamID steamId)
        {
            return Instance.Settings.GetUserData(steamId);
        }

        public static void _RefreshAvatar(SteamUserData userData)
        {
            Instance.Settings.RefreshAvatar(userData);
        }
        #endregion

        #region Steam Friends Wrapper
        public SteamUserData UserData
        {
            get
            {
                if (Settings != null)
                    return Settings.UserData;
                else
                    return null;
            }
        }

        public string GetUserName(ulong steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
                return u.DisplayName;
            else
                return string.Empty;
        }

        public FriendGameInfo_t GetUserGameInfo(ulong steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
                return u.GameInfo;
            else
                return default(FriendGameInfo_t);
        }

        public Texture2D GetUserAvatar(ulong steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
            {
                if (!u.IconLoaded)
                {
                    RefreshAvatar(u);
                }
                return u.Avatar;
            }
            else
                return null;
        }

        public SteamUserData GetUserData(ulong steamId)
        {
            return Settings.GetUserData(new CSteamID(steamId));
        }

        public string GetUserName(CSteamID steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
                return u.DisplayName;
            else
                return string.Empty;
        }

        public FriendGameInfo_t GetUserGameInfo(CSteamID steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
                return u.GameInfo;
            else
                return default(FriendGameInfo_t);
        }

        public Texture2D GetUserAvatar(CSteamID steamId)
        {
            var u = GetUserData(steamId);
            if (u != null)
            {
                if (!u.IconLoaded)
                {
                    RefreshAvatar(u);
                }
                return u.Avatar;
            }
            else
                return null;
        }

        public SteamUserData GetUserData(CSteamID steamId)
        {
            return Settings.GetUserData(steamId);
        }

        public void RefreshAvatar(SteamUserData userData)
        {
            Settings.RefreshAvatar(userData);
        }

        /// <summary>
        /// Set rather or not the system should listen for Steam Friend chat messages
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
        /// Send a Steam Friend Chat message to the indicated user
        /// </summary>
        /// <param name="friendId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendFriendChatMessage(ulong friendId, string message)
        {
            return SendFriendChatMessage(new CSteamID(friendId), message);
        }

        /// <summary>
        /// Send a Steam Friend Chat message to the indicated user
        /// </summary>
        /// <param name="friend"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendFriendChatMessage(CSteamID friend, string message)
        {
            return SteamFriends.ReplyToFriendMessage(friend, message);
        }
        #endregion

        #region Steam Achievement Wrapper
        public void StoreStatsAndAchievements()
        {
            Settings.StoreStatsAndAchievements();
        }

        /// <summary>
        /// Locates the achievement by its string id
        /// </summary>
        /// <param name="achievementId"></param>
        /// <returns></returns>
        public SteamAchievementData GetAchievement(string achievementId)
        {
            if (Settings != null)
            {
                if (Settings.achievements.Exists(a => a.achievementId == achievementId))
                {
                    var ach = Settings.achievements.FirstOrDefault(a => a.achievementId == achievementId);
                    return ach;
                }
                else
                    return null;
            }
            else
                return null;
        }
        /// <summary>
        /// Locates the achievement based on its index in the manager
        /// </summary>
        /// <param name="achievementIndex"></param>
        /// <returns></returns>
        public SteamAchievementData GetAchievement(int achievementIndex)
        {
            if (Settings != null)
            {
                if (Settings.achievements.Count > achievementIndex && achievementIndex > -1)
                {
                    var ach = Settings.achievements[achievementIndex];
                    return ach;
                }
                else
                    return null;
            }
            else
                return null;
        }
        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// <see cref="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement"/>
        /// </summary>
        public void UnlockAchievement(SteamAchievementData achievementData)
        {
            achievementData.Unlock();
        }
        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// <see cref="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement"/>
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (Settings != null)
            {
                if (Settings.achievements.Exists(a => a.achievementId == achievementId))
                {
                    var ach = Settings.achievements.FirstOrDefault(a => a.achievementId == achievementId);
                    ach.Unlock();
                }
            }
        }
        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// <see cref="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement"/>
        /// </summary>
        public void UnlockAchievement(int achievementIndex)
        {
            if (Settings != null)
            {
                if (Settings.achievements.Count > achievementIndex && achievementIndex > -1)
                {
                    var ach = Settings.achievements[achievementIndex];
                    ach.Unlock();
                }
            }
        }
        /// <summary>
        /// Determin if an achievement with this string ID has been achieved by the player
        /// </summary>
        /// <param name="achievementId"></param>
        /// <returns></returns>
        public bool IsAchievementAchieved(string achievementId)
        {
            var ach = GetAchievement(achievementId);
            if (ach != null)
                return ach.isAchieved;
            else
                return false;
        }
        /// <summary>
        /// Determin if the achievement at this index in the achievements list has been achieved by the player
        /// </summary>
        /// <param name="achievementIndex"></param>
        /// <returns></returns>
        public bool IsAchievementAchieved(int achievementIndex)
        {
            var ach = GetAchievement(achievementIndex);
            if (ach != null)
                return ach.isAchieved;
            else
                return false;
        }
        /// <summary>
        /// Does an achievement with this string id exist in the list
        /// </summary>
        /// <param name="achievementId"></param>
        /// <returns></returns>
        public bool AchievementExists(string achievementId)
        {
            var ach = GetAchievement(achievementId);
            return ach != null;
        }
        /// <summary>
        /// Does an achievement at this index exist in the list
        /// </summary>
        /// <param name="achievementIndex"></param>
        /// <returns></returns>
        public bool AchievementExists(int achievementIndex)
        {
            var ach = GetAchievement(achievementIndex);
            return ach != null;
        }
        #endregion

        #region Static Steam Achievement Wrapper
        public static void _StoreStatsAndAchievements()
        {
            Instance.Settings.StoreStatsAndAchievements();
        }

        /// <summary>
        /// Locates the achievement by its string id
        /// </summary>
        /// <param name="achievementId"></param>
        /// <returns></returns>
        public static SteamAchievementData _GetAchievement(string achievementId)
        {
            if (Instance != null)
                return Instance.GetAchievement(achievementId);
            else
                return null;
        }
        /// <summary>
        /// Locates the achievement based on its index in the manager
        /// </summary>
        /// <param name="achievementIndex"></param>
        /// <returns></returns>
        public static SteamAchievementData _GetAchievement(int achievementIndex)
        {
            if (Instance != null)
                return Instance.GetAchievement(achievementIndex);
            else
                return null;
        }
        /// <summary>
        /// Unlocks the given achievement
        /// </summary>
        /// <param name="achievementData"></param>
        public static void _UnlockAchievement(SteamAchievementData achievementData)
        {
            if (Instance != null)
                Instance.UnlockAchievement(achievementData);
        }
        /// <summary>
        /// Locates the achievement and unlocks it if found
        /// </summary>
        /// <param name="achievementId"></param>
        public static void _UnlockAchievement(string achievementId)
        {
            if (Instance != null)
                Instance.UnlockAchievement(achievementId);
        }
        /// <summary>
        /// Locates the achievement and unlocks it if found
        /// </summary>
        /// <param name="achievementIndex"></param>
        public static void _UnlockAchievement(int achievementIndex)
        {
            if (Instance != null)
                Instance.UnlockAchievement(achievementIndex);
        }
        /// <summary>
        /// Determin if an achievement with this string ID has been achieved by the player
        /// </summary>
        /// <param name="achievementId"></param>
        /// <returns></returns>
        public static bool _IsAchievementAchieved(string achievementId)
        {
            if (Instance != null)
                return Instance.AchievementExists(achievementId);
            return false;
        }
        /// <summary>
        /// Determin if the achievement at this index in the achievements list has been achieved by the player
        /// </summary>
        /// <param name="achievementIndex"></param>
        /// <returns></returns>
        public static bool _IsAchievementAchieved(int achievementIndex)
        {
            if (Instance != null)
                return Instance.IsAchievementAchieved(achievementIndex);
            return false;
        }
        /// <summary>
        /// Does an achievement with this string id exist in the list
        /// </summary>
        /// <param name="achievementId"></param>
        /// <returns></returns>
        public static bool _AchievementExists(string achievementId)
        {
            if (Instance != null)
                return Instance.AchievementExists(achievementId);
            return false;
        }
        /// <summary>
        /// Does an achievement at this index exist in the list
        /// </summary>
        /// <param name="achievementIndex"></param>
        /// <returns></returns>
        public static bool _AchievementExists(int achievementIndex)
        {
            if (Instance != null)
                return Instance.AchievementExists(achievementIndex);
            return false;
        }
        #endregion
    }
}
#endif