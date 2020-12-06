#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.Scriptable;
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Networking
{

    [CreateAssetMenu(menuName = "Steamworks/Networking/Lobby Settings")]
    [Serializable]
    public class SteamworksLobbySettings : ScriptableObject
    {
        /// <summary>
        /// Controls the further Steam distance that will be searched for a lobby
        /// </summary>
        /// <remarks>
        /// <see cref="MaxDistanceFilter"/> is used during <see cref="QuickMatch(LobbyHunterFilter, string, bool)"/> operations to determin the maximum distance the quick mach should search when expanding.
        /// </remarks>
        [Header("Quick Match Settings")]
        public ELobbyDistanceFilter MaxDistanceFilter = ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault;
        /// <summary>
        /// True if the player is in the lobby that the Steam Lobby Settings object is tracking
        /// </summary>
        /// <remarks>
        /// This tests the local player's CSteamID against Valve's GetLobbyMemberData. If the method returns null then the user is not part of the tracked lobby.
        /// </remarks>
        public bool InLobby
        {
            get
            {
                //Lobby Member Data is supposed to return null if the player is not in the lobby or the lobby id is invalid
                //In any other case it should return the value of the key or if none return string.empty thus if it returns null this user is not in this lobby
                if (SteamMatchmaking.GetLobbyMemberData(LobbyId, SteamUser.GetSteamID(), "anyField") == null)
                    return false;
                else
                    return true;
            }
        }
        /// <summary>
        /// True if the system is tracking a lobby
        /// </summary>
        /// <remarks>
        /// This returns true if the provided lobby ID is a legitimate ID and if Valve indicates that the lobby has members.
        /// </remarks>
        public bool HasLobby
        {
            get
            {
                if (LobbyId != CSteamID.Nil
                    && SteamMatchmaking.GetNumLobbyMembers(LobbyId) > 0)
                    return true;
                else
                    return false;
            }
        }
        /// <summary>
        /// True if the Steam Lobby Settings object has a valid lobby and the current player is that lobby's owner
        /// </summary>
        /// <remarks>
        /// This returns true if the provided lobby ID is a legitimate ID and if Valve indicates that the lobby has members and if the owner of the lobby is the current player.
        /// </remarks>
        public bool IsHost
        {
            get
            {
                return HasLobby && SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(LobbyId);
            }
        }

        /// <summary>
        /// True if the tracked lobby has game server information
        /// </summary>
        public bool HasGameServer
        {
            get
            {
                return SteamMatchmaking.GetLobbyGameServer(LobbyId, out _, out _, out _);
            }
        }

        /// <summary>
        /// The game server information stored against the lobby
        /// </summary>
        /// <remarks>
        /// <para>
        /// This data is set when the host calls <see cref="SetLobbyGameServer"/> or one of its variants. Uppon calling <see cref="SetLobbyGameServer"/> the Valve backend will raise <see cref="OnGameServerSet"/> for all members other than the host the paramiter of which also contains server data.
        /// The typical use case of this field is when a member has join a persistent lobby after the game server has been started.
        /// </para>
        /// </remarks>
        public LobbyGameServerInformation GameServerInformation
        {
            get
            {
                uint ip;
                ushort port;
                CSteamID id = CSteamID.Nil;

                SteamMatchmaking.GetLobbyGameServer(LobbyId, out ip, out port, out id);

                return new LobbyGameServerInformation()
                {
                    ipAddress = ip,
                    port = port,
                    serverId = id
                };
            }
        }
        
        /// <summary>
        /// True while the system is waiting for search result responce
        /// </summary>
        /// <remarks>
        /// </remarks>
        public bool IsSearching
        {
            get { return standardSearch; }
        }
        /// <summary>
        /// Returns true while the system is performing a quick search
        /// </summary>
        public bool IsQuickSearching
        {
            get { return quickMatchSearch; }
        }
        public string LobbyName;
        [Tooltip("This will be set by the Lobby Manager when joining a lobby")]
        protected CSteamID LobbyId;
        public CSteamID lobbyId { get { return LobbyId; } }
        [Tooltip("Steam will only accept values between 0 and 250 but expects a datatype of int")]
        public IntReference MaxMemberCount = new IntReference(4);
        [Header("Current Lobby Data")]
        [Tooltip("These are the keys of the data that can be registered against a members metadata")]
        public List<string> MemberDataKeys;
        public SteamworksLobbyMember LobbyOwner;
        public List<SteamworksLobbyMember> Members;
        public SteamworksLobbyMetadata Metadata;
        

        [HideInInspector]
        public ISteamworksLobbyManager Manager;

        #region Internal Data
        private bool quickMatchCreateOnFail = false;
        [NonSerialized]
        private bool standardSearch = false;
        [NonSerialized]
        private bool quickMatchSearch = false;
        [NonSerialized]
        private bool callbacksRegistered = false;
        private LobbyHunterFilter createLobbyFilter;
        private LobbyHunterFilter quickMatchFilter;
        #endregion

        #region Callbacks
        private CallResult<LobbyCreated_t> m_LobbyCreated;
        private Callback<LobbyEnter_t> m_LobbyEntered;
        private Callback<GameLobbyJoinRequested_t> m_GameLobbyJoinRequested;
        private Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;
        private CallResult<LobbyMatchList_t> m_LobbyMatchList;
        private Callback<LobbyGameCreated_t> m_LobbyGameCreated;
        private Callback<LobbyDataUpdate_t> m_LobbyDataUpdated;
        private Callback<LobbyChatMsg_t> m_LobbyChatMsg;
        #endregion

        #region Events
        [HideInInspector]
        public UnityEvent LobbyDataUpdateFailed;

        [HideInInspector]
        public UnityEvent OnKickedFromLobby;
        /// <summary>
        /// Occures when a request to join the lobby has been recieved such as through Steam's invite friend dialog in the Steam Overlay
        /// </summary>
        [HideInInspector]
        public UnityGameLobbyJoinRequestedEvent OnGameLobbyJoinRequest = new UnityGameLobbyJoinRequestedEvent();
        /// <summary>
        /// Occures when list of Lobbies is retured from a search
        /// </summary>
        [HideInInspector]
        public UnityLobbyHunterListEvent OnLobbyMatchList = new UnityLobbyHunterListEvent();
        /// <summary>
        /// Occures when a lobby is created by the player
        /// </summary>
        [HideInInspector]
        public UnityLobbyCreatedEvent OnLobbyCreated = new UnityLobbyCreatedEvent();
        /// <summary>
        /// Occures when the owner of the currently tracked lobby changes
        /// </summary>
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnOwnershipChange = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when a member joins the lobby
        /// </summary>
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnMemberJoined = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when a member leaves the lobby
        /// </summary>
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnMemberLeft = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when Steam metadata for a member changes
        /// </summary>
        [HideInInspector]
        public SteamworksLobbyMemberEvent OnMemberDataChanged = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when the player joins a lobby
        /// </summary>
        [HideInInspector]
        public UnityLobbyEnterEvent OnLobbyEnter = new UnityLobbyEnterEvent();
        /// <summary>
        /// Occures when the player leaves a lobby
        /// </summary>
        [HideInInspector]
        public UnityEvent OnLobbyExit = new UnityEvent();
        /// <summary>
        /// Occures when lobby metadata changes
        /// </summary>
        [HideInInspector]
        public UnityEvent OnLobbyDataChanged = new UnityEvent();
        /// <summary>
        /// Occures when the host of the lobby starts the game e.g. sets game server data on the lobby
        /// </summary>
        [HideInInspector]
        public UnityLobbyGameCreatedEvent OnGameServerSet = new UnityLobbyGameCreatedEvent();
        /// <summary>
        /// Occures when lobby chat metadata has been updated such as a kick or ban.
        /// </summary>
        [HideInInspector]
        public UnityLobbyChatUpdateEvent OnLobbyChatUpdate = new UnityLobbyChatUpdateEvent();
        /// <summary>
        /// Occures when a quick match search fails to return a lobby match
        /// </summary>
        [HideInInspector]
        public UnityEvent QuickMatchFailed = new UnityEvent();
        /// <summary>
        /// Occures when a search for a lobby has started
        /// </summary>
        [HideInInspector]
        public UnityEvent SearchStarted = new UnityEvent();
        /// <summary>
        /// Occures when a lobby chat message is recieved
        /// </summary>
        [HideInInspector]
        public LobbyChatMessageEvent OnChatMessageReceived = new LobbyChatMessageEvent();
        /// <summary>
        /// Occures when a member of the lobby chat enters the chat
        /// </summary>
        [HideInInspector]
        public SteamworksLobbyMemberEvent ChatMemberStateChangeEntered = new SteamworksLobbyMemberEvent();
        /// <summary>
        /// Occures when a member of the lobby chat leaves the chat
        /// </summary>
        [HideInInspector]
        public UnityPersonaEvent ChatMemberStateChangeLeft = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is disconnected from the chat
        /// </summary>
        [HideInInspector]
        public UnityPersonaEvent ChatMemberStateChangeDisconnected = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is kicked out of the chat
        /// </summary>
        [HideInInspector]
        public UnityPersonaEvent ChatMemberStateChangeKicked = new UnityPersonaEvent();
        /// <summary>
        /// Occures when a member of the lobby chat is banned from the chat
        /// </summary>
        [HideInInspector]
        public UnityPersonaEvent ChatMemberStateChangeBanned = new UnityPersonaEvent();
        #endregion

        /// <summary>
        /// Typically called by the HeathenSteamManager.OnEnable()
        /// This registeres the Valve callbacks and CallResult deligates
        /// </summary>
        public void RegisterCallbacks()
        {
            if (SteamworksFoundationManager.Initialized)
            {   
                if (!callbacksRegistered)
                {
                    callbacksRegistered = true;
                    m_LobbyCreated = CallResult<LobbyCreated_t>.Create(HandleLobbyCreated);
                    m_LobbyEntered = Callback<LobbyEnter_t>.Create(HandleLobbyEntered);
                    m_GameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(HandleGameLobbyJoinRequested);
                    m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(HandleLobbyChatUpdate);
                    m_LobbyMatchList = CallResult<LobbyMatchList_t>.Create(HandleLobbyMatchList);
                    m_LobbyGameCreated = Callback<LobbyGameCreated_t>.Create(HandleLobbyGameCreated);
                    m_LobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(HandleLobbyDataUpdate);
                    m_LobbyChatMsg = Callback<LobbyChatMsg_t>.Create(HandleLobbyChatMessage);
                }
            }
        }

        /// <summary>
        /// Sets the tracked lobby ID and updates the relivent member data and metadata lists
        /// </summary>
        /// <param name="lobbyId"></param>
        public void SetLobbyId(CSteamID lobbyId)
        {
            if (lobbyId == CSteamID.Nil)
            {
                LobbyId = CSteamID.Nil;
                Members.Clear();
                LobbyOwner = null;
            }
            else if (LobbyId != lobbyId)
            {
                LobbyId = lobbyId;
                Members.Clear();
                LobbyOwner = null;
                CSteamID ownerId = SteamMatchmaking.GetLobbyOwner(LobbyId);
                var count = SteamMatchmaking.GetNumLobbyMembers(LobbyId);
                for (int i = 0; i < count; i++)
                {
                    var memberId = SteamMatchmaking.GetLobbyMemberByIndex(LobbyId, i);
                    var record = ProcessLobbyMember(memberId);
                    if (memberId == ownerId)
                        LobbyOwner = record;
                }
            }
            else
            {
                //We are already aware of this lobby so just refresh the owner and member data
                CSteamID ownerId = SteamMatchmaking.GetLobbyOwner(LobbyId);
                var count = SteamMatchmaking.GetNumLobbyMembers(LobbyId);
                for (int i = 0; i < count; i++)
                {
                    var memberId = SteamMatchmaking.GetLobbyMemberByIndex(LobbyId, i);
                    var record = ProcessLobbyMember(memberId);
                    if (memberId == ownerId)
                        LobbyOwner = record;
                }
            }
        }

        /// <summary>
        /// Forces an update on change of lobby ownership, this is automatically called when the original host is removed from the lobby
        /// </summary>
        private void HandleOwnershipChange()
        {
            var ownerId = SteamMatchmaking.GetLobbyOwner(LobbyId);
            LobbyOwner = ProcessLobbyMember(ownerId);
        }

        /// <summary>
        /// Removes the member from the settings list
        /// </summary>
        /// <param name="memberId">The member to remove</param>
        /// <returns>True if the removed member was the lobby owner, otherwise false</returns>
        private bool RemoveMember(CSteamID memberId)
        {
            var targetMember = Members.FirstOrDefault(p => p.UserData != null && p.UserData.SteamId == memberId);
            Members.Remove(targetMember);
            OnMemberLeft.Invoke(targetMember);

            //Insure we remove any duplicates
            Members.RemoveAll(p => p.UserData != null && p.UserData.SteamId == memberId);

            var kickList = Metadata["z_heathenKick"];

            if (kickList == null)
                kickList = string.Empty;

            if (kickList.Contains("[" + memberId.ToString() + "]"))
            {
                kickList = kickList.Replace("[" + memberId.ToString() + "]", string.Empty);
                SetLobbyMetadata("z_heathenKick", kickList);
            }

            if (memberId == SteamMatchmaking.GetLobbyOwner(LobbyId))
            {
                HandleOwnershipChange();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Called when a lobby member joins or updates metadata
        /// </summary>
        /// <param name="memberId"></param>
        private SteamworksLobbyMember ProcessLobbyMember(CSteamID memberId)
        {
            SteamworksLobbyMember member = Members.FirstOrDefault(p => p.UserData != null && p.UserData.SteamId == memberId);

            if (member == null)
            {
                member = new SteamworksLobbyMember();
                member.UserData = SteamworksFoundationManager._GetUserData(memberId);
                Members.Add(member);
                OnMemberJoined.Invoke(member);
            }

            if (member.Metadata.Records == null)
                member.Metadata.Records = new List<MetadataRecord>();
            else
                member.Metadata.Records.Clear();

            foreach (var dataKey in MemberDataKeys)
            {
                MetadataRecord record = new MetadataRecord() { key = dataKey };
                record.value = SteamMatchmaking.GetLobbyMemberData(LobbyId, memberId, dataKey);
                member.Metadata.Records.Add(record);
            }

            OnMemberDataChanged.Invoke(member);

            return member;
        }

        #region Callbacks
        private void HandleLobbyList(SteamLobbyLobbyList lobbyList)
        {
            int lobbyCount = lobbyList.Count;

            if (quickMatchSearch)
            {
                if (lobbyCount == 0)
                {
                    if (!quickMatchFilter.useDistanceFilter)
                        quickMatchFilter.useDistanceFilter = true;

                    switch (quickMatchFilter.distanceOption)
                    {
                        case ELobbyDistanceFilter.k_ELobbyDistanceFilterClose:
                            if ((int)MaxDistanceFilter >= 1)
                            {
                                quickMatchFilter.distanceOption = ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault;
                                FindQuickMatch();
                            }
                            else
                            {
                                HandleQuickMatchFailed();
                                return;
                            }
                            break;
                        case ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault:
                            if ((int)MaxDistanceFilter >= 2)
                            {
                                quickMatchFilter.distanceOption = ELobbyDistanceFilter.k_ELobbyDistanceFilterFar;
                                FindQuickMatch();
                            }
                            else
                            {
                                HandleQuickMatchFailed();
                                return;
                            }
                            break;
                        case ELobbyDistanceFilter.k_ELobbyDistanceFilterFar:
                            if ((int)MaxDistanceFilter >= 3)
                            {
                                quickMatchFilter.distanceOption = ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide;
                                FindQuickMatch();
                            }
                            else
                            {
                                HandleQuickMatchFailed();
                                return;
                            }
                            break;
                        case ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide:
                            HandleQuickMatchFailed();
                            return;
                    }
                }
                else
                {
                    //We got a hit, the top option should be the best option so join it
                    var lobby = SteamMatchmaking.GetLobbyByIndex(0);
                    JoinLobby(lobby);
                }
            }
        }

        private void HandleQuickMatchFailed()
        {
            quickMatchSearch = false;
            if (quickMatchCreateOnFail)
            {
                Debug.Log("Quick Match failed to find a lobby and will create a new one.");
                CreateLobby(quickMatchFilter, LobbyName, ELobbyType.k_ELobbyTypePublic);
            }
            else
            {
                Debug.Log("Quick Match failed to find a lobby.");
                QuickMatchFailed.Invoke();
            }
        }

        private void FindQuickMatch()
        {
            if (!callbacksRegistered)
                RegisterCallbacks();

            SetLobbyFilter(quickMatchFilter);

            var call = SteamMatchmaking.RequestLobbyList();
            m_LobbyMatchList.Set(call, HandleLobbyMatchList);

            SearchStarted.Invoke();
        }

        private void SetLobbyFilter(LobbyHunterFilter LobbyFilter)
        {
            if (LobbyFilter.useSlotsAvailable)
                SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(LobbyFilter.requiredOpenSlots);

            if (LobbyFilter.useDistanceFilter)
                SteamMatchmaking.AddRequestLobbyListDistanceFilter(LobbyFilter.distanceOption);

            if (LobbyFilter.maxResults > 0)
                SteamMatchmaking.AddRequestLobbyListResultCountFilter(LobbyFilter.maxResults);

            if (LobbyFilter.numberValues != null)
            {
                foreach (var f in LobbyFilter.numberValues)
                    SteamMatchmaking.AddRequestLobbyListNumericalFilter(f.key, f.value, f.method);
            }

            if (LobbyFilter.nearValues != null)
            {
                foreach (var f in LobbyFilter.nearValues)
                    SteamMatchmaking.AddRequestLobbyListNearValueFilter(f.key, f.value);
            }

            if (LobbyFilter.stringValues != null)
            {
                foreach (var f in LobbyFilter.stringValues)
                    SteamMatchmaking.AddRequestLobbyListStringFilter(f.key, f.value, f.method);
            }
        }
        #endregion  

        #region Callback Handlers
        void HandleLobbyGameCreated(LobbyGameCreated_t param)
        {
            OnGameServerSet.Invoke(param);
        }

        void HandleLobbyMatchList(LobbyMatchList_t pCallback, bool bIOFailure)
        {
            uint numLobbies = pCallback.m_nLobbiesMatching;
            var result = new SteamLobbyLobbyList();
            
            if (numLobbies <= 0)
            {
                if (quickMatchSearch)
                {
                    Debug.Log("Lobby match list returned (0), refining search paramiters.");
                    HandleLobbyList(result);
                }
                else
                {
                    Debug.Log("Lobby match list returned (" + numLobbies.ToString() + ")");
                    standardSearch = false;
                    OnLobbyMatchList.Invoke(result);
                }
            }
            else
            {
                Debug.Log("Lobby match list returned (" + numLobbies.ToString() + ")");
                for (int i = 0; i < numLobbies; i++)
                {
                    LobbyHunterLobbyRecord record = new LobbyHunterLobbyRecord();

                    record.metadata = new Dictionary<string, string>();
                    record.lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                    //record.hostId = SteamMatchmaking.GetLobbyOwner(record.lobbyId);
                    //int memberCount = SteamMatchmaking.GetNumLobbyMembers(record.lobbyId);
                    record.maxSlots = SteamMatchmaking.GetLobbyMemberLimit(record.lobbyId);
                    

                    int dataCount = SteamMatchmaking.GetLobbyDataCount(record.lobbyId);

                    if (record.lobbyId == LobbyId)
                    {
                        Debug.Log("Browsed our own lobby and found " + dataCount.ToString() + " metadata records.");
                    }

                    for (int ii = 0; ii < dataCount; ii++)
                    {
                        bool isUs = (record.lobbyId == LobbyId);
                        string key;
                        string value;
                        if (SteamMatchmaking.GetLobbyDataByIndex(record.lobbyId, ii, out key, Constants.k_nMaxLobbyKeyLength, out value, Constants.k_cubChatMetadataMax))
                        {
                            record.metadata.Add(key, value);
                            if (key == "name")
                                record.name = value;
                            if (key == "OwnerID")
                            {
                                ulong val;
                                if (ulong.TryParse(value, out val))
                                {
                                    record.hostId = new CSteamID(val);
                                }
                            }

                            if (isUs)
                            {
                                Debug.Log("My Lobby data key = [" + key + "], value = [" + value + "]");
                            }
                        }
                    }

                    result.Add(record);
                }

                if (quickMatchSearch)
                {
                    HandleLobbyList(result);
                }
                else
                {
                    standardSearch = false;
                    OnLobbyMatchList.Invoke(result);
                }
            }
        }

        void HandleLobbyChatUpdate(LobbyChatUpdate_t pCallback)
        {
            if (LobbyId.m_SteamID != pCallback.m_ulSteamIDLobby)
                return;

            if (pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeLeft)
            {
                var memberId = new CSteamID(pCallback.m_ulSteamIDUserChanged);
                var member = Members.FirstOrDefault(p => p.UserData != null && p.UserData.SteamId == memberId);

                if (RemoveMember(memberId))
                {
                    OnOwnershipChange.Invoke(LobbyOwner);
                    //OnMemberLeft.Invoke(member);
                }
                //else
                //{
                //    OnMemberLeft.Invoke(member);
                //}
                ChatMemberStateChangeLeft.Invoke(SteamworksFoundationManager._GetUserData(memberId));
            }
            else if (pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeEntered)
            {
                var member = ProcessLobbyMember(new CSteamID(pCallback.m_ulSteamIDUserChanged));

                //OnMemberJoined.Invoke(member);
                ChatMemberStateChangeEntered.Invoke(member);
            }
            else if (pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
            {
                var memberId = new CSteamID(pCallback.m_ulSteamIDUserChanged);
                var member = Members.FirstOrDefault(p => p.UserData != null && p.UserData.SteamId == memberId);

                if (RemoveMember(memberId))
                {
                    OnOwnershipChange.Invoke(LobbyOwner);
                    //OnMemberLeft.Invoke(member);
                }
                //else
                //{
                //    OnMemberLeft.Invoke(member);
                //}
                ChatMemberStateChangeDisconnected.Invoke(SteamworksFoundationManager._GetUserData(memberId));
            }
            else if (pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeKicked)
            {
                var memberId = new CSteamID(pCallback.m_ulSteamIDUserChanged);
                var member = Members.FirstOrDefault(p => p.UserData != null && p.UserData.SteamId == memberId);

                if (RemoveMember(memberId))
                {
                    OnOwnershipChange.Invoke(LobbyOwner);
                    //OnMemberLeft.Invoke(member);
                }
                //else
                //{
                //    OnMemberLeft.Invoke(member);
                //}
                ChatMemberStateChangeKicked.Invoke(SteamworksFoundationManager._GetUserData(memberId));
            }
            else if (pCallback.m_rgfChatMemberStateChange == (uint)EChatMemberStateChange.k_EChatMemberStateChangeBanned)
            {
                var memberId = new CSteamID(pCallback.m_ulSteamIDUserChanged);
                var member = Members.FirstOrDefault(p => p.UserData != null && p.UserData.SteamId == memberId);

                if (RemoveMember(memberId))
                {
                    OnOwnershipChange.Invoke(LobbyOwner);
                    //OnMemberLeft.Invoke(member);
                }
                //else
                //{
                //    OnMemberLeft.Invoke(member);
                //}
                ChatMemberStateChangeBanned.Invoke(SteamworksFoundationManager._GetUserData(memberId));
            }

            OnLobbyChatUpdate.Invoke(pCallback);
        }

        void HandleGameLobbyJoinRequested(GameLobbyJoinRequested_t param)
        {
            //JoinLobby(param.m_steamIDLobby);
            OnGameLobbyJoinRequest.Invoke(param);
        }

        void HandleLobbyEntered(LobbyEnter_t param)
        {
            var hostId = SteamMatchmaking.GetLobbyOwner(new CSteamID(param.m_ulSteamIDLobby));
            var userData = SteamworksFoundationManager.Instance.GetUserName(hostId);
            Debug.Log("Entered lobby: " + param.m_ulSteamIDLobby.ToString() + " User Name: " + userData + " User Id: " + hostId.m_SteamID);

            SetLobbyId(new CSteamID(param.m_ulSteamIDLobby));

            OnLobbyEnter.Invoke(param);
        }

        void HandleLobbyCreated(LobbyCreated_t param, bool bIOFailure)
        {
            SetLobbyId(new CSteamID(param.m_ulSteamIDLobby));

            SteamMatchmaking.SetLobbyMemberLimit(LobbyId, MaxMemberCount);

            if (SteamMatchmaking.SetLobbyData(LobbyId, "name", LobbyName))
                Debug.Log("Set lobby data [name] to [" + LobbyName + "]");
            else
                Debug.Log("Failed to set lobby data [name] to [" + LobbyName + "]");

            if (Metadata.Records == null)
            {
                Metadata.Records = new List<MetadataRecord>();
            }
            else
            {
                Metadata.Records.Clear();
            }

            if (createLobbyFilter.stringValues != null)
            {
                foreach (var f in createLobbyFilter.stringValues)
                {
                    if (SteamMatchmaking.SetLobbyData(LobbyId, f.key, f.value))
                    {
                        Metadata[f.key] = f.value;
                        Debug.Log("Set lobby data [" + f.key + "] to [" + f.value + "]");
                    }
                    else
                        Debug.Log("Failed to set lobby data [" + f.key + "] to [" + f.value + "]");
                }
            }

            if (createLobbyFilter.numberValues != null)
            {
                foreach (var f in createLobbyFilter.numberValues)
                {
                    if (SteamMatchmaking.SetLobbyData(LobbyId, f.key, f.value.ToString()))
                    {
                        Metadata[f.key] = f.value.ToString();
                        Debug.Log("Set lobby data [" + f.key + "] to [" + f.value.ToString() + "]");
                    }
                    else
                        Debug.Log("Failed to set lobby data [" + f.key + "] to [" + f.value.ToString() + "]");
                }
            }

            var ownerData = new SteamworksLobbyMember()
            {
                Metadata = new SteamworksLobbyMetadata(),
                UserData = SteamworksFoundationManager._UserData,
            };

            LobbyOwner = ownerData;

            //Remove any existing member entry for the owner other than the one we created now
            Members.RemoveAll(p => p.UserData.SteamId == ownerData.UserData.SteamId);
            Members.Add(ownerData);

            OnLobbyCreated.Invoke(param);
        }

        void HandleLobbyDataUpdate(LobbyDataUpdate_t param)
        {
            var askedToLeave = false;

            if(param.m_bSuccess == 0)
            {
                LobbyDataUpdateFailed.Invoke();
                return;
            }

            if (param.m_ulSteamIDLobby == param.m_ulSteamIDMember)
            {
                var count = SteamMatchmaking.GetLobbyDataCount(LobbyId);
                var key = "";
                var value = "";
                for (int i = 0; i < count; i++)
                {
                    if (SteamMatchmaking.GetLobbyDataByIndex(LobbyId, i, out key, Constants.k_nMaxLobbyKeyLength, out value, Constants.k_cubChatMetadataMax))
                    {
                        if (key == "name")
                            LobbyName = value;
                        if(key == "z_heathenKick")
                        {
                            if(value != null && value.Contains("[" + SteamUser.GetSteamID().m_SteamID.ToString() + "]"))
                            {
                                //We have been asked to leave
                                Debug.Log("User has been kicked from the lobby.");
                                askedToLeave = true;
                            }
                        }

                        Metadata[key] = value;
                    }
                }
                OnLobbyDataChanged.Invoke();
            }
            else
            {
                ProcessLobbyMember(new CSteamID(param.m_ulSteamIDMember));
            }

            if (askedToLeave)
            {
                LeaveLobby();
                OnKickedFromLobby.Invoke();
            }
            else
            {
                var steamHost = SteamMatchmaking.GetLobbyOwner(LobbyId);
                if (steamHost.m_SteamID != LobbyOwner.UserData.SteamId.m_SteamID)
                {
                    //Host changed
                    var memberInfo = Members.FirstOrDefault(p => p.UserData.SteamId.m_SteamID == steamHost.m_SteamID);

                    if (memberInfo != null)
                    {
                        LobbyOwner = memberInfo;
                        OnOwnershipChange.Invoke(LobbyOwner);
                    }
                }
            }
        }

        void HandleLobbyChatMessage(LobbyChatMsg_t pCallback)
        {
            var subjectLobby = (CSteamID)pCallback.m_ulSteamIDLobby;
            if (subjectLobby != LobbyId)
                return;

            CSteamID SteamIDUser;
            byte[] Data = new byte[4096];
            EChatEntryType ChatEntryType;
            int ret = SteamMatchmaking.GetLobbyChatEntry(subjectLobby, (int)pCallback.m_iChatID, out SteamIDUser, Data, Data.Length, out ChatEntryType);
            byte[] truncated = new byte[ret];
            Array.Copy(Data, truncated, ret);

            LobbyChatMessageData record = new LobbyChatMessageData();
            record.sender = Members.FirstOrDefault(p => p.UserData.SteamId == SteamIDUser);
            record.message = System.Text.Encoding.UTF8.GetString(truncated);
            record.recievedTime = DateTime.Now;
            record.chatEntryType = ChatEntryType;

            OnChatMessageReceived.Invoke(record);
        }
        #endregion

        /// <summary>
        /// Creates a new lobby according the LobbyFilter information provided
        /// </summary>
        /// <param name="LobbyFilter">Defines the base metadata in the form of a search filter</param>
        /// <param name="LobbyName">The name to be applied to the lobby, all Heathen lobbies define a lobby name if none is passed the current users display name will be used</param>
        /// <param name="lobbyType">The type of lobby to be created ... see Valve's documentation regarding ELobbyType for more informaiton</param>
        public void CreateLobby(LobbyHunterFilter LobbyFilter, string LobbyName = "", ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic)
        {
            if (LobbyName == "")
                LobbyName = SteamworksFoundationManager.Instance.Settings.UserData.DisplayName;
            else
                this.LobbyName = LobbyName;

            createLobbyFilter = LobbyFilter;

            var call = SteamMatchmaking.CreateLobby(lobbyType, MaxMemberCount);
            m_LobbyCreated.Set(call, HandleLobbyCreated);
        }

        /// <summary>
        /// Joins a steam lobby
        /// Note this will leave any current lobby before joining the new lobby
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby to join</param>
        public void JoinLobby(CSteamID lobbyId)
        {
            

            if (LobbyId == lobbyId)
            {
                Debug.Log("Already in this lobby");
                return;
            }

            if (LobbyId != CSteamID.Nil)
                SteamMatchmaking.LeaveLobby(LobbyId);

            LobbyId = CSteamID.Nil;

            SteamMatchmaking.JoinLobby(lobbyId);
            Debug.Log("Joined lobby: " + lobbyId.ToString());
        }

        /// <summary>
        /// Joins a steam lobby
        /// Note this will leave any current lobby before joining the new lobby
        /// </summary>
        /// <param name="lobbyId">The ID of the lobby to join</param>
        public void JoinLobby(ulong lobbyId)
        {
            if (LobbyId == new CSteamID(lobbyId))
                return;

            if (LobbyId != CSteamID.Nil)
                SteamMatchmaking.LeaveLobby(LobbyId);

            LobbyId = CSteamID.Nil;

            SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));
        }

        /// <summary>
        /// Leaves the current lobby if any
        /// </summary>
        public void LeaveLobby()
        {
            var result = false;
            if (InLobby)
                result = true;

            try
            {
                SteamMatchmaking.LeaveLobby(LobbyId);
            }
            catch { }
            LobbyId = CSteamID.Nil;
            LobbyOwner = null;
            Members = new List<SteamworksLobbyMember>();
            Metadata = new SteamworksLobbyMetadata();

            OnLobbyExit.Invoke();

            if (result)
                OnLobbyExit.Invoke();
        }

        /// <summary>
        /// Searches for a matching lobby according to the provided filter data.
        /// Note that a search will only start if no search is currently running.
        /// </summary>
        /// <param name="LobbyFilter">Describes the metadata to search for in a lobby</param>
        public void FindMatch(LobbyHunterFilter LobbyFilter)
        {
            if (quickMatchSearch)
            {
                Debug.LogError("Attempted to search for a lobby while a quick search is processing. This search will be ignored, you must call CancelQuickMatch to abort a search before it completes, note that results may still come back resulting in the next match list not being as expected.");
                return;
            }

            standardSearch = true;

            SetLobbyFilter(LobbyFilter);

            var call = SteamMatchmaking.RequestLobbyList();
            m_LobbyMatchList.Set(call, HandleLobbyMatchList);

            SearchStarted.Invoke();
        }

        /// <summary>
        /// Starts a staged search for a matching lobby. Search will only start if no searches are currently running.
        /// </summary>
        /// <param name="LobbyFilter">The metadata of a lobby to search for</param>
        /// <param name="autoCreate">Should the system create a lobby if no matching lobby is found</param>
        /// <returns>True if the search was started, false otherwise.</returns>
        public bool QuickMatch(LobbyHunterFilter LobbyFilter, string onCreateName, bool autoCreate = false)
        {
            if (!callbacksRegistered)
                RegisterCallbacks();

            if (quickMatchSearch || standardSearch)
            {
                return false;
            }

            LobbyName = onCreateName;
            quickMatchCreateOnFail = autoCreate;
            quickMatchSearch = true;
            quickMatchFilter = LobbyFilter;
            quickMatchFilter.distanceOption = ELobbyDistanceFilter.k_ELobbyDistanceFilterClose;
            quickMatchFilter.useDistanceFilter = true;
            FindQuickMatch();

            return true;
        }

        /// <summary>
        /// Terminates a quick search process
        /// Note that lobby searches are asynchronious and result may return after the cancelation
        /// </summary>
        public void CancelQuickMatch()
        {
            if (!callbacksRegistered)
                RegisterCallbacks();

            if (quickMatchSearch)
            {
                quickMatchSearch = false;
                Debug.LogWarning("Quick Match search has been canceled, note that results may still come back resulting in the next match list not being as expected.");
            }
        }

        /// <summary>
        /// Terminates a standard search
        /// Note that lobby searches are asynchronious and result may return after the cancelation
        /// </summary>
        public void CancelStandardSearch()
        {
            if (!callbacksRegistered)
                RegisterCallbacks();

            if (standardSearch)
            {
                standardSearch = false;
                Debug.LogWarning("Search has been canceled, note that results may still come back resulting in the next match list not being as expected.");
            }
        }

        /// <summary>
        /// Sends a chat message via Valve's Lobby Chat system
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendChatMessage(string message)
        {
            if (!callbacksRegistered)
                RegisterCallbacks();

            byte[] MsgBody = System.Text.Encoding.UTF8.GetBytes(message);
            SteamMatchmaking.SendLobbyChatMsg(LobbyId, MsgBody, MsgBody.Length);
        }

        /// <summary>
        /// Sets metadata on the lobby, this can only be called by the host of the lobby
        /// </summary>
        /// <param name="key">The key of the metadata to set</param>
        /// <param name="value">The value of the metadata to set</param>
        public void SetLobbyMetadata(string key, string value)
        {
            if (!callbacksRegistered)
                RegisterCallbacks();

            if (!IsHost)
                return;

            if (key == "name")
                LobbyName = value;

            Metadata[key] = value;
            SteamMatchmaking.SetLobbyData(LobbyId, key, value);
        }

        /// <summary>
        /// Sets metadata for the player on the lobby
        /// </summary>
        /// <param name="key">The key of the metadata to set</param>
        /// <param name="value">The value of the metadata to set</param>
        public void SetMemberMetadata(string key, string value)
        {
            if (!callbacksRegistered)
                RegisterCallbacks();

            if (MemberDataKeys == null)
                MemberDataKeys = new List<string>();

            if (!MemberDataKeys.Contains(key))
                MemberDataKeys.Add(key);

            //Fetch this users member entry
            var memberEntry = Members.FirstOrDefault(p => p.UserData != null && p.UserData.SteamId == SteamworksFoundationManager._UserData.SteamId);

            //If no entry found create one
            if (memberEntry == null)
            {
                memberEntry = new SteamworksLobbyMember()
                {
                    Metadata = new SteamworksLobbyMetadata(),
                    UserData = SteamworksFoundationManager._UserData,
                };

                Members.Add(memberEntry);
            }

            //Add the entry locally
            memberEntry.Metadata[key] = value;

            //Add the entry on the server
            SteamMatchmaking.SetLobbyMemberData(LobbyId, key, value);
        }

        /// <summary>
        /// Sets the lobby game server e.g. game start using the lobby Host as the server ID
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will trigger GameServerSet on all members of the lobby
        /// This should be called after the server is started
        /// </para>
        /// </remarks>
        public void SetLobbyGameServer()
        {
            if (!callbacksRegistered)
                RegisterCallbacks();

            Debug.Log("Calling Set Lobby Game Server for P2P with a host of '" + LobbyOwner.UserData.DisplayName + "' with CSteamID of [" + LobbyOwner.UserData.SteamId.m_SteamID.ToString() + "]");

            SteamMatchmaking.SetLobbyGameServer(LobbyId, 0, 0, LobbyOwner.UserData.SteamId);
        }

        /// <summary>
        /// Sets the lobby game server e.g. game start 
        /// </summary>
        /// <param name="gameServerId">The CSteamID of the steam game server</param>
        /// <remarks>
        /// <para>
        /// This will trigger GameServerSet on all members of the lobby
        /// This should be called after the server is started
        /// </para>
        /// </remarks>
        public void SetLobbyGameServer(CSteamID gameServerId)
        {
            if (!callbacksRegistered)
                RegisterCallbacks();

            Debug.Log("Calling Set Lobby Game Server with a server ID of [" + gameServerId.m_SteamID.ToString() + "]");

            SteamMatchmaking.SetLobbyGameServer(LobbyId, 0, 0, gameServerId);
        }

        /// <summary>
        /// Sets the lobby game server e.g. game start
        /// </summary>
        /// <param name="ipAddress">The ip address of the server</param>
        /// <param name="port">The port to be used to connect to the server</param>
        /// <remarks>
        /// <para>
        /// This will trigger GameServerSet on all members of the lobby
        /// This should be called after the server is started
        /// </para>
        /// </remarks>
        public void SetLobbyGameServer(uint ipAddress, ushort port)
        {
            if (!callbacksRegistered)
                RegisterCallbacks();

            Debug.Log("Calling Set Lobby Game Server with a server IP Address [" + SteamUtilities.IPUintToString(ipAddress) + "] + Port [" + port.ToString() + "]");

            SteamMatchmaking.SetLobbyGameServer(LobbyId, ipAddress, port, CSteamID.Nil);
        }

        /// <summary>
        /// Sets the lobby game server e.g. game start
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="serverId"></param>
        /// <remarks>
        /// <para>
        /// This will trigger GameServerSet on all members of the lobby
        /// This should be called after the server is started
        /// </para>
        /// </remarks>
        public void SetLobbyGameServer(uint ipAddress, ushort port, CSteamID serverId)
        {
            if (!callbacksRegistered)
                RegisterCallbacks();

            Debug.Log("Calling Set Lobby Game Server with a server IP Address [" + SteamUtilities.IPUintToString(ipAddress) + "] + Port [" + port.ToString() + "] + Server ID [" + serverId.m_SteamID.ToString() + "]");

            SteamMatchmaking.SetLobbyGameServer(LobbyId, ipAddress, port, serverId);
        }

        /// <summary>
        /// Sets the lobby game server e.g. game start
        /// </summary>
        /// <param name="ipAddress">The ip address of the server</param>
        /// <param name="port">The port to be used to connect to the server</param>
        /// <remarks>
        /// <para>
        /// This will trigger GameServerSet on all members of the lobby
        /// This should be called after the server is started
        /// </para>
        /// </remarks>
        public void SetLobbyGameServer(string ipAddress, ushort port)
        {
            SetLobbyGameServer(SteamUtilities.IPStringToUint(ipAddress), port);
        }

        /// <summary>
        /// Sets the lobby game server e.g. game start
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="serverId"></param>
        /// <remarks>
        /// <para>
        /// This will trigger GameServerSet on all members of the lobby
        /// This should be called after the server is started
        /// </para>
        /// </remarks>
        public void SetLobbyGameServer(string ipAddress, ushort port, CSteamID serverId)
        {
            SetLobbyGameServer(SteamUtilities.IPStringToUint(ipAddress), port, serverId);
        }

        /// <summary>
        /// Sets the lobby as joinable or not. The default is that a lobby is joinable.
        /// </summary>
        /// <param name="value"></param>
        public void SetLobbyJoinable(bool value)
        {
            SteamMatchmaking.SetLobbyJoinable(lobbyId, value);
        }

        /// <summary>
        /// Gets the game server information from the Valve backend for the tracked lobby
        /// </summary>
        /// <param name="address">
        /// <para>
        /// Valve style uint packed IP address where the first octive is packed in the left most 8 bits and the last octive is packed in the right most 8 bits.
        /// You can use <see cref="SteamUtilities.IPUintToString(uint)"/> to convert from a packed address and <see cref="SteamUtilities.IPStringToUint(string)"/> to pack a string based address.
        /// </para>
        /// </param>
        /// <param name="port"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public bool GetLobbyGameServer_ValveStyle(out uint address, out ushort port, out CSteamID serverId)
        {
            return SteamMatchmaking.GetLobbyGameServer(LobbyId, out address, out port, out serverId);
        }

        /// <summary>
        /// Gets the game server information from the Valve backend for the tracked lobby
        /// </summary>
        /// <param name="address">A string based IP address as is expected by Mirror and similar high level interfaces</param>
        /// <param name="port"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public bool GetLobbyGameServer_MirrorStyle(out string address, out ushort port, out CSteamID serverId)
        {
            uint ipAddress;
            address = string.Empty;
            var result = SteamMatchmaking.GetLobbyGameServer(LobbyId, out ipAddress, out port, out serverId);
            if(result)
            {
                address = SteamUtilities.IPUintToString(ipAddress);
            }

            return result;
        }

        /// <summary>
        /// Gets the game server information from the Valve backend for the tracked lobby
        /// </summary>
        /// <param name="serverInformation">A sructure containing both the Valve and Mirror style addressing information as well as CSteamID information</param>
        /// <returns></returns>
        public bool GetLobbyGameServer_HeathenStyle(out LobbyGameServerInformation serverInformation)
        {
            uint ipAddress;
            ushort port;
            CSteamID serverId;
            var result = SteamMatchmaking.GetLobbyGameServer(LobbyId, out ipAddress, out port, out serverId);

            serverInformation = new LobbyGameServerInformation()
            {
                ipAddress = ipAddress,
                port = port,
                serverId = serverId
            };

            return result;
        }

        /// <summary>
        /// Marks the user to be removed
        /// </summary>
        /// <param name="memberId"></param>
        /// <remarks>
        /// This creates an entry in the metadata named z_heathenKick which contains a string array of Ids of users that should leave the lobby.
        /// When users detect their ID in the string they will automatically leave the lobby on leaving the lobby the users ID will be removed from the array.
        /// </remarks>
        public void KickMember(CSteamID memberId)
        {
            if (!IsHost)
            {
                Debug.LogError("Only the host of a lobby can kick a member from it.");
                return;
            }

            if(memberId.m_SteamID == SteamUser.GetSteamID().m_SteamID)
            {
                Debug.Log("Host is kicking its self out");
                LeaveLobby();
                OnKickedFromLobby.Invoke();
                return;
            }
            else
            {
                Debug.Log("Marking " + memberId.m_SteamID + " for removal");
            }

            var kickList = Metadata["z_heathenKick"];

            if (kickList == null)
                kickList = string.Empty;

            if (!kickList.Contains("[" + memberId.ToString() + "]"))
                kickList += "[" + memberId.ToString() + "]";

            SetLobbyMetadata("z_heathenKick", kickList);
        }

        /// <summary>
        /// Marks the user to be removed
        /// </summary>
        /// <param name="member"></param>
        /// <remarks>
        /// This creates an entry in the metadata named z_heathenKick which contains a string array of Ids of users that should leave the lobby.
        /// When users detect their ID in the string they will automatically leave the lobby on leaving the lobby the users ID will be removed from the array.
        /// </remarks>
        public void KickMember(SteamUserData member)
        {
            KickMember(member.SteamId);
        }

        /// <summary>
        /// Marks the user to be removed
        /// </summary>
        /// <param name="memberIndex"></param>
        /// <remarks>
        /// This creates an entry in the metadata named z_heathenKick which contains a string array of Ids of users that should leave the lobby.
        /// When users detect their ID in the string they will automatically leave the lobby on leaving the lobby the users ID will be removed from the array.
        /// </remarks>
        public void KickMember(int memberIndex)
        {
            KickMember(Members[memberIndex].UserData.SteamId);
        }

        /// <summary>
        /// Sets the indicated user as the new owner of the lobby
        /// </summary>
        /// <param name="newOwner"></param>
        /// <remarks>
        /// <para>
        /// This does not effect the NetworkManager or other networking funcitonality it only changes the ownership of a lobby
        /// </para>
        /// </remarks>
        public void ChangeOwner(CSteamID newOwner)
        {
            if (IsHost && SteamUser.GetSteamID().m_SteamID != newOwner.m_SteamID)
                SteamMatchmaking.SetLobbyOwner(LobbyId, newOwner);
            else
            {
                Debug.LogWarning("Only the host of the lobby can request change of ownership and cannot change ownership to its self.");
            }
        }

        /// <summary>
        /// Sets the indicated user as the new owner of the lobby
        /// </summary>
        /// <param name="newOwner"></param>
        /// <remarks>
        /// <para>
        /// This does not effect the NetworkManager or other networking funcitonality it only changes the ownership of a lobby
        /// </para>
        /// </remarks>
        public void ChangeOwner(SteamUserData newOwner)
        {
            if (IsHost && SteamUser.GetSteamID().m_SteamID != newOwner.SteamId.m_SteamID)
                SteamMatchmaking.SetLobbyOwner(LobbyId, newOwner.SteamId);
            else
            {
                Debug.LogWarning("Only the host of the lobby can request change of ownership and cannot change ownership to its self.");
            }
        }

        /// <summary>
        /// Sets the indicated user as the new owner of the lobby
        /// </summary>
        /// <param name="newOwner"></param>
        /// <remarks>
        /// <para>
        /// This does not effect the NetworkManager or other networking funcitonality it only changes the ownership of a lobby
        /// </para>
        /// </remarks>
        public void ChangeOwner(int newOwnerIndex)
        {
            var newOwner = Members[newOwnerIndex].UserData.SteamId;

            if (IsHost && SteamUser.GetSteamID().m_SteamID != newOwner.m_SteamID)
                SteamMatchmaking.SetLobbyOwner(LobbyId, newOwner);
            else
            {
                Debug.LogWarning("Only the host of the lobby can request change of ownership and cannot change ownership to its self.");
            }
        }
    }
}
#endif