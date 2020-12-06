#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Networking
{
    [CreateAssetMenu(menuName = "Steamworks/Networking/Game Server Settings")]
    [Serializable]
    public class HeathenGameServerSettings : ScriptableObject
    {
        [Header("System Configuraiton")]
        [Tooltip("If true the Game Server will initalize and login into Steam 'On Enable'\nof the Heathen Game Server Manager and on connect will start the Mirror Server if available.\nIf false you must call MeathenGameServerManager.InitalizeGameServer manually when ready.")]
        public bool InitOnEnable = false;
        public bool EnableMirror = true;

        [Header("Initalization Settings")]
        public uint IP = 0;
        [Tooltip("UDP port for the master server updater to listen on")]
        public ushort MasterServerUpdaterPort = 27016;
        [Tooltip("UDP port for the server to do authentication on")]
        public ushort AuthenticationPort = 8766;
        [Tooltip("UDP port for the server to listen on")]
        public ushort ServerPort = 27015;
        public string ServerVersion = "1.0.0.0";
        [Tooltip("Only used if supporting spectators.")]
        public ushort SpectatorPort = 27017;
        

        [Header("Server Settings")]
        [Tooltip("This will get set on logon and is how users will connect.")]
        public CSteamID ServerId;
        [Tooltip("Should the system use the Game Server Authentication API.")]
        public bool USE_GS_AUTH_API = false;
        [Tooltip("Heartbeats notify the master server of this servers details, if disabled your server will not list\nIf USE GS AUTH API is enabled heartbeats are always enabled..")]
        public bool EnableHeartbeats = true;
        [Tooltip("If true the spectator port and server name will be used and configured on the server.")]
        public bool SupportSpectators = false;
        [Tooltip("Only used if supporting spectators.")]
        public string SpectatorServerName = "Usually GameDescription + Spectator";
        public bool AnonymousServerLogin = false;
        [Tooltip("See https://steamcommunity.com/dev/managegameservers \nOr\nUse Anonymous Server Login")]
        public string GameServerToken = "See https://steamcommunity.com/dev/managegameservers";
        public bool IsPasswordProtected = false;
        public string ServerName = "My Server Name";
        [Tooltip("It is recomended to set this to the full name of your game.")]
        public string GameDescription = "Usually the name of your game";
        [Tooltip("Typically the same as the game's name e.g. its folder name.")] 
        public string GameDirectory = "e.g. its folder name";
        public bool IsDedicated = false;
        public int MaxPlayerCount = 4;
        public int BotPlayerCount = 0;
        public string MapName = "";
        [Tooltip("A delimited string used for Matchmaking Filtering e.g. CoolPeopleOnly,NoWagonsAllowed.\nThe above represents 2 data points matchmaking will then filter accordingly\n... see Heathen Game Server Browser for more informaiton.")]
        public string GameData; 
        public List<StringKeyValuePair> RulePairs = new List<StringKeyValuePair>();
        
        [Header("Events")]
        public UnityEvent GameServerShuttingDown;
        public SteamServerDisconnectedEvent Disconnected;
        public SteamServerConnectedEvent Connected;
        public SteamServerFailureEvent Failure;

        public Callback<SteamServerConnectFailure_t> SteamServerConnectFailure;
        public Callback<SteamServersConnected_t> SteamServersConnected;
        public Callback<SteamServersDisconnected_t> SteamServersDisconnected;
        

        public void OnSteamServersDisconnected(SteamServersDisconnected_t param)
        {
            Disconnected.Invoke(param);
        }

        public void OnSteamServersConnected(SteamServersConnected_t param)
        {
            Connected.Invoke(param);
        }

        public void OnSteamServerConnectFailure(SteamServerConnectFailure_t param)
        {
            Failure.Invoke(param);
        }
    }

    [Serializable]
    public struct StringKeyValuePair
    {
        public string key;
        public string value;
    }
}
#endif