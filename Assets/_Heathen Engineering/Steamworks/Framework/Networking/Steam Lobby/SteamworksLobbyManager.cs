#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS 
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.Tools;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Networking
{
    /// <summary>
    /// Manages Steam Lobby searches and connections
    /// </summary>
    public class SteamworksLobbyManager : HeathenBehaviour, ISteamworksLobbyManager
    {
        public SteamworksLobbySettings LobbySettings;

        #region Events
        public UnityEvent LobbyDataUpdateFailed = new UnityEvent();
        public UnityEvent OnKickedFromLobby = new UnityEvent();
        /// <summary>
        /// Occures when a request to join the lobby has been recieved such as through Steam's invite friend dialog in the Steam Overlay
        /// </summary>
        public UnityGameLobbyJoinRequestedEvent OnGameLobbyJoinRequest = new UnityGameLobbyJoinRequestedEvent();
        /// <summary>
        /// Occures when list of Lobbies is retured from a search
        /// </summary>
        public UnityLobbyHunterListEvent OnLobbyMatchList = new UnityLobbyHunterListEvent();
        /// <summary>
        /// Occures when a lobby is created by the player
        /// </summary>
        public UnityLobbyCreatedEvent OnLobbyCreated = new UnityLobbyCreatedEvent();
        /// <summary>
        /// Occures when the owner of the currently tracked lobby changes
        /// </summary>
        public SteamworksLobbyMemberEvent OnOwnershipChange = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when a member joins the lobby
        /// </summary>
        public SteamworksLobbyMemberEvent OnMemberJoined = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when a member leaves the lobby
        /// </summary>
        public SteamworksLobbyMemberEvent OnMemberLeft = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when Steam metadata for a member changes
        /// </summary>
        public SteamworksLobbyMemberEvent OnMemberDataChanged = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when the player joins a lobby
        /// </summary>
        public UnityLobbyEnterEvent OnLobbyEnter = new UnityLobbyEnterEvent();
        /// <summary>
        /// Occures when the player leaves a lobby
        /// </summary>
        public UnityEvent OnLobbyExit = new UnityEvent();
        /// <summary>
        /// Occures when lobby metadata changes
        /// </summary>
        public UnityEvent OnLobbyDataChanged = new UnityEvent();
        /// <summary>
        /// Occures when the host of the lobby starts the game e.g. sets game server data on the lobby
        /// </summary>
        public UnityLobbyGameCreatedEvent OnGameServerSet = new UnityLobbyGameCreatedEvent();
        /// <summary>
        /// Occures when lobby chat metadata has been updated such as a kick or ban.
        /// </summary>
        public UnityLobbyChatUpdateEvent OnLobbyChatUpdate = new UnityLobbyChatUpdateEvent();
        /// <summary>
        /// Occures when a quick match search fails to return a lobby match
        /// </summary>
        public UnityEvent QuickMatchFailed = new UnityEvent();
        /// <summary>
        /// Occures when a search for a lobby has started
        /// </summary>
        public UnityEvent SearchStarted = new UnityEvent();
        /// <summary>
        /// Occures when a lobby chat message is recieved
        /// </summary>
        public LobbyChatMessageEvent OnChatMessageReceived = new LobbyChatMessageEvent();
        /// <summary>
        /// Occures when a member of the lobby chat enters the chat
        /// </summary>
        public SteamworksLobbyMemberEvent ChatMemberStateChangeEntered = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when a member of the lobby chat leaves the chat
        /// </summary>
        public UnityPersonaEvent ChatMemberStateChangeLeft = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is disconnected from the chat
        /// </summary>
        public UnityPersonaEvent ChatMemberStateChangeDisconnected = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is kicked out of the chat
        /// </summary>
        public UnityPersonaEvent ChatMemberStateChangeKicked = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is banned from the chat
        /// </summary>
        public UnityPersonaEvent ChatMemberStateChangeBanned = new UnityPersonaEvent();
        #endregion

        /// <summary>
        /// Checks if a search is currently running
        /// </summary>
        public bool IsSearching
        {
            get { return LobbySettings.IsSearching; }
        }

        public bool IsQuickSearching
        {
            get { return LobbySettings.IsQuickSearching; }
        }

        #region Unity Methods
        private void Start()
        {
            if (LobbySettings == null)
            {
                Debug.LogWarning("Lobby settings not found ... creating default settings");
                LobbySettings = ScriptableObject.CreateInstance<SteamworksLobbySettings>();
                LobbySettings.MaxMemberCount = new Scriptable.IntReference(250);
                LobbySettings.Members = new List<SteamworksLobbyMember>();
                LobbySettings.Metadata = new SteamworksLobbyMetadata();
            }
            else if (LobbySettings.Manager != null && (object)LobbySettings.Manager != this)
            {
                Debug.LogWarning("Lobby settings already references a manager, this lobby manager will overwrite it. Please insure there is only 1 Heathen Steam Lobby Manager active at a time.");
            }

            if (LobbySettings.MaxMemberCount < 0 || LobbySettings.MaxMemberCount.Value > 250)
            {
                Debug.LogWarning("Lobby settings Max Member Count (" + LobbySettings.MaxMemberCount.Value.ToString() + ") is out of bounds, Min = 0, Max = 250, the value will be clamped to the valid range.");
            }

            LobbySettings.MaxMemberCount.Value = Mathf.Clamp(LobbySettings.MaxMemberCount.Value, 0, 250);
            LobbySettings.Manager = this;
        }

        private void OnEnable()
        {
            if (LobbySettings == null)
            {
                Debug.LogWarning("Lobby settings not found ... creating default settings");
                LobbySettings = ScriptableObject.CreateInstance<SteamworksLobbySettings>();
                LobbySettings.MaxMemberCount.Value = 250;
                LobbySettings.Members = new List<SteamworksLobbyMember>();
                LobbySettings.Metadata = new SteamworksLobbyMetadata();
            }
            else if (LobbySettings.Manager != null && (object)LobbySettings.Manager != this)
            {
                Debug.LogWarning("Lobby settings already references a manager, this lobby manager will overwrite it. Please insure there is only 1 Heathen Steam Lobby Manager active at a time.");
            }

            if (LobbySettings.MaxMemberCount < 0 || LobbySettings.MaxMemberCount > 250)
            {
                Debug.LogWarning("Lobby settings Max Member Count (" + LobbySettings.MaxMemberCount.Value.ToString() + ") is out of bounds, Min = 0, Max = 250, the value will be clamped to the valid range.");
            }

            LobbySettings.MaxMemberCount.Value = Mathf.Clamp(LobbySettings.MaxMemberCount.Value, 0, 250);
            LobbySettings.Manager = this;
            LobbySettings.RegisterCallbacks();
            Debug.Log("Lobby settings registered with Max Member Count = " + LobbySettings.MaxMemberCount.Value.ToString() + ".");

            LobbySettings.LobbyDataUpdateFailed.AddListener(LobbyDataUpdateFailed.Invoke);
            LobbySettings.OnKickedFromLobby.AddListener(OnKickedFromLobby.Invoke);
            LobbySettings.OnGameLobbyJoinRequest.AddListener(OnGameLobbyJoinRequest.Invoke);
            LobbySettings.OnLobbyMatchList.AddListener(OnLobbyMatchList.Invoke);
            LobbySettings.OnLobbyCreated.AddListener(OnLobbyCreated.Invoke);
            LobbySettings.OnOwnershipChange.AddListener(OnOwnershipChange.Invoke);
            LobbySettings.OnMemberJoined.AddListener(OnMemberJoined.Invoke);
            LobbySettings.OnMemberLeft.AddListener(OnMemberLeft.Invoke);
            LobbySettings.OnMemberDataChanged.AddListener(OnMemberDataChanged.Invoke);
            LobbySettings.OnLobbyEnter.AddListener(OnLobbyEnter.Invoke);
            LobbySettings.OnLobbyExit.AddListener(OnLobbyExit.Invoke);
            LobbySettings.OnLobbyDataChanged.AddListener(OnLobbyDataChanged.Invoke);
            LobbySettings.OnGameServerSet.AddListener(OnGameServerSet.Invoke);
            LobbySettings.OnLobbyChatUpdate.AddListener(OnLobbyChatUpdate.Invoke);
            LobbySettings.QuickMatchFailed.AddListener(QuickMatchFailed.Invoke);
            LobbySettings.SearchStarted.AddListener(SearchStarted.Invoke);
            LobbySettings.OnChatMessageReceived.AddListener(OnChatMessageReceived.Invoke);
            LobbySettings.ChatMemberStateChangeEntered.AddListener(ChatMemberStateChangeEntered.Invoke);
            LobbySettings.ChatMemberStateChangeLeft.AddListener(ChatMemberStateChangeLeft.Invoke);
            LobbySettings.ChatMemberStateChangeDisconnected.AddListener(ChatMemberStateChangeDisconnected.Invoke);
            LobbySettings.ChatMemberStateChangeKicked.AddListener(ChatMemberStateChangeKicked.Invoke);
            LobbySettings.ChatMemberStateChangeBanned.AddListener(ChatMemberStateChangeBanned.Invoke);
        }

        private void OnDestroy()
        {
            try
            {
                if (LobbySettings != null && LobbySettings.Manager == (ISteamworksLobbyManager)this)
                {
                    //This should ideally be a Leave Lobby call however in editor this would cause a crash
                    LobbySettings.SetLobbyId(CSteamID.Nil);
                    LobbySettings.Manager = null;
                    LobbySettings.LobbyOwner = null;
                    LobbySettings.Members = new List<SteamworksLobbyMember>();
                    LobbySettings.Metadata = new SteamworksLobbyMetadata();
                }
            }
            catch { }
        }
        #endregion
        
        public void CreateLobby(LobbyHunterFilter LobbyFilter, string LobbyName = "", ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic)
        {
            LobbySettings.CreateLobby(LobbyFilter, LobbyName, lobbyType);
        }

        public void JoinLobby(CSteamID lobbyId)
        {
            if(LobbySettings == null)
            {
                LobbySettings = ScriptableObject.CreateInstance<SteamworksLobbySettings>();
            }

            LobbySettings.JoinLobby(lobbyId);
        }

        public void LeaveLobby()
        {
            if (LobbySettings != null)
            {
                LobbySettings.LeaveLobby();
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|LeaveLobby] attempted to leave the lobby while [HeathenSteamLobbyManager|LobbySettings] is null");
            }
        }
        
        public void FindMatch(LobbyHunterFilter LobbyFilter)
        {
            if (LobbySettings != null)
            {
                LobbySettings.FindMatch(LobbyFilter);
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|FindMatch] attempted to find a match while [HeathenSteamLobbyManager|LobbySettings] is null");
            }
        }

        /// <summary>
        /// Starts a staged search for a matching lobby. Search will only start if no searches are currently running.
        /// </summary>
        /// <param name="LobbyFilter"></param>
        /// <param name="autoCreate"></param>
        /// <returns>True if the search was started, false otherwise.</returns>
        public bool QuickMatch(LobbyHunterFilter LobbyFilter, string onCreateName, bool autoCreate = false)
        {
            if (LobbySettings != null)
            {
                return LobbySettings.QuickMatch(LobbyFilter, onCreateName, autoCreate);
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|QuickMatch] attempted to quick match while [HeathenSteamLobbyManager|LobbySettings] is null");
                return false;
            }
        }

        public void CancelQuickMatch()
        {
            if (LobbySettings != null)
            {
                LobbySettings.CancelQuickMatch();
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|CancelQuickMatch] attempted to cancel a quick match search while [HeathenSteamLobbyManager|LobbySettings] is null");
            }
        }

        public void CancelStandardSearch()
        {
            if (LobbySettings != null)
            {
                LobbySettings.CancelStandardSearch();
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|CancelStandardSearch] attempted to cancel a standard search while [HeathenSteamLobbyManager|LobbySettings] is null");
            }
        }

        public void SendChatMessage(string message)
        {
            if (LobbySettings != null)
            {
                LobbySettings.SendChatMessage(message);
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|SetLobbyMetadata] attempted to lobby chat message while [HeathenSteamLobbyManager|LobbySettings] is null");
            }
        }

        public void SetLobbyMetadata(string key, string value)
        {
            if (LobbySettings != null)
            {
                LobbySettings.SetLobbyMetadata(key, value);
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|SetLobbyMetadata] attempted to set lobby metadata while [HeathenSteamLobbyManager|LobbySettings] is null");
            }
        }

        public void SetMemberMetadata(string key, string value)
        {
            if (LobbySettings != null)
            {
                LobbySettings.SetMemberMetadata(key, value);
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|SetMemberMetadata] attempted to set member metadata while [HeathenSteamLobbyManager|LobbySettings] is null");
            }
        }

        public void SetLobbyGameServer()
        {
            if (LobbySettings != null)
            {
                LobbySettings.SetLobbyGameServer();
            }
            else
            {
                Debug.LogWarning("[HeatehnSteamLobbyManager|SetLobbyGameServer] attempted to set the lobby game server while [HeathenSteamLobbyManager|LobbySettings] is null");
            }
        }
    }
}
#endif