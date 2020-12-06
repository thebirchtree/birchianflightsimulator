#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using HeathenEngineering.Events;
using HeathenEngineering.Serializable;
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using static HeathenEngineering.SteamApi.PlayerServices.SteamworksAuthentication;

namespace HeathenEngineering.SteamApi.Networking
{
    [RequireComponent(typeof(SteamworksFoundationManager))]
    [DisallowMultipleComponent]
    public class HeathenGameServerManager : MonoBehaviour
    {
        public SteamSettings steamSettings;
        public HeathenGameServerSettings serverSettings;
#if MIRROR
        public Mirror.NetworkManager networkManager;
#endif
        public bool IsDebugging = false;

        [Header("Events")]
        public UnityEvent OnSteamInitialized;
        public UnityStringEvent OnSteamInitializationError;
        public UnityEvent GameServerShuttingDown;
        public SteamServerDisconnectedEvent Disconnected;
        public SteamServerConnectedEvent Connected;
        public SteamServerFailureEvent Failure;

        private void OnEnable()
        {
            //Only do this if this is a headless server build
            if (!Application.isBatchMode)
            {
                Debug.Log("Steam Client Startup Detected!\nDisabling the Heathen Game Server Manager.");
                this.enabled = false;
                return;
            }
            else
            {
                Debug.Log("Steam Server Startup Detected!");
                if (serverSettings.InitOnEnable)
                    InitializeGameServer();
            }
        }
         
        public void InitializeGameServer()
        {
            //Insure the setting events are initalized ... Unity doesn't do this for you as it does with behaviours
            if (serverSettings.GameServerShuttingDown == null)
                serverSettings.GameServerShuttingDown = new UnityEvent();
            if (serverSettings.Disconnected == null)
                serverSettings.Disconnected = new SteamServerDisconnectedEvent();
            if (serverSettings.Connected == null)
                serverSettings.Connected = new SteamServerConnectedEvent();
            if (serverSettings.Failure == null)
                serverSettings.Failure = new SteamServerFailureEvent();

            //Register on the Steam callback for the related events
            serverSettings.SteamServerConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(serverSettings.OnSteamServerConnectFailure);
            serverSettings.SteamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(serverSettings.OnSteamServersConnected);
            serverSettings.SteamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(serverSettings.OnSteamServersDisconnected);

            //Pass through the invoke to the settings events to the behaviour events
            serverSettings.GameServerShuttingDown.AddListener(GameServerShuttingDown.Invoke);
            serverSettings.Disconnected.AddListener(Disconnected.Invoke);
            serverSettings.Connected.AddListener(Connected.Invoke);
            serverSettings.Connected.AddListener(OnSteamServersConnected);
            serverSettings.Failure.AddListener(Failure.Invoke);

            //If debugging
            if (IsDebugging)
            {
                Debug.Log("Establishing debug hooks");
                serverSettings.GameServerShuttingDown.AddListener(LogShutDown);
                serverSettings.Disconnected.AddListener(LogDisconnect);
                serverSettings.Connected.AddListener(LogConnect);
                serverSettings.Failure.AddListener(LogFailure);
            }

            EServerMode eMode = EServerMode.eServerModeNoAuthentication;
            if (serverSettings.USE_GS_AUTH_API)
                eMode = EServerMode.eServerModeAuthenticationAndSecure;

            // Initialize the SteamGameServer interface, we tell it some info about us, and we request support
            // for both Authentication (making sure users own games) and secure mode, VAC running in our game
            // and kicking users who are VAC banned
            SteamworksFoundationManager.Instance._Initialized.Value = GameServer.Init(serverSettings.IP, serverSettings.AuthenticationPort, serverSettings.ServerPort, serverSettings.MasterServerUpdaterPort, eMode, serverSettings.ServerVersion);
            if (!SteamworksFoundationManager.Instance._Initialized.Value)
            {
                Debug.Log("SteamGameServer_Init call failed!");
                OnSteamInitializationError.Invoke("SteamGameServer_Init call failed!");
                return;
            }
            else
            {
                Debug.Log("SteamGameServer_Init call succed!ed\n\tPublic IP = " + SteamGameServer.GetPublicIP().ToString() + "\n\tIP = " + serverSettings.IP.ToString() + "\n\tAuthentication Port = " + serverSettings.AuthenticationPort.ToString() + "\n\tServer Port = " + serverSettings.ServerPort.ToString() + "\n\tMaster Server Updater Port = " + serverSettings.MasterServerUpdaterPort.ToString() + "\n\tMode = " + eMode.ToString() + "\n\tVersion = " + serverSettings.ServerVersion);
                OnSteamInitialized.Invoke();
            }

            // Set the "game dir".
            // This is currently required for all games.  However, soon we will be
            // using the AppID for most purposes, and this string will only be needed
            // for mods.  it may not be changed after the server has logged on
            SteamGameServer.SetModDir(serverSettings.GameDirectory);

            // These fields are currently required, but will go away soon.
            // See their documentation for more info
            SteamGameServer.SetProduct(steamSettings.ApplicationId.m_AppId.ToString());
            SteamGameServer.SetGameDescription(serverSettings.GameDescription);

            if (serverSettings.SupportSpectators)
            {
                if (IsDebugging)
                    Debug.Log("Spectator enabled:\n\tName = " + serverSettings.SpectatorServerName + "\n\tSpectator Port = " + serverSettings.SpectatorPort.ToString());

                SteamGameServer.SetSpectatorPort(serverSettings.SpectatorPort);
                SteamGameServer.SetSpectatorServerName(serverSettings.SpectatorServerName);
            }
            else if (IsDebugging)
                Debug.Log("Spectator Set Up Skipped");

            if (serverSettings.AnonymousServerLogin)
            {
                if (IsDebugging)
                    Debug.Log("Logging on with Anonymous");

                SteamGameServer.LogOnAnonymous();
            }
            else
            {
                if (IsDebugging)
                    Debug.Log("Logging on with token");

                SteamGameServer.LogOn(serverSettings.GameServerToken);
            }

            // We want to actively update the master server with our presence so players can
            // find us via the steam matchmaking/server browser interfaces
            if (serverSettings.USE_GS_AUTH_API || serverSettings.EnableHeartbeats)
                SteamGameServer.EnableHeartbeats(true);

            Debug.Log("Steam Game Server Started.\nWaiting for connection result from Steam");
        }

        private void Update()
        {
            if (Application.isBatchMode && steamSettings.EnableGameServerInit)
            {
                if (!SteamworksFoundationManager.Initialized)
                {
                    return;
                }

                GameServer.RunCallbacks();
            }
            else
            {
                Debug.Log("Heathen Game Server Manager is only valid on headless builds and when the Steam Settings 'Enable Game Server Init' is true.\nThe Heathen Game Server Manager will be disabled!");
            }
        }

        void OnSteamServersConnected(SteamServersConnected_t pLogonSuccess)
        {
            serverSettings.ServerId = SteamGameServer.GetSteamID();
            Debug.Log("Game Server connected to Steam successfully!\n\tMod Directory = " + serverSettings.GameDirectory + "\n\tApplicaiton ID = " + steamSettings.ApplicationId.m_AppId.ToString() + "\n\tServer ID = " + serverSettings.ServerId.m_SteamID.ToString() + "\n\tServer Name = " + serverSettings.ServerName + "\n\tGame Description = " + serverSettings.GameDescription + "\n\tMax Player Count = " + serverSettings.MaxPlayerCount.ToString());

#if MIRROR
            if (serverSettings.EnableMirror)
            {
                networkManager.maxConnections = serverSettings.MaxPlayerCount;
                networkManager.networkAddress = "localhost";
                networkManager.StartServer();
            }
#endif

            // Tell Steam about our server details
            SendUpdatedServerDetailsToSteam();
        }

        void SendUpdatedServerDetailsToSteam()
        {
            SteamGameServer.SetMaxPlayerCount(serverSettings.MaxPlayerCount);
            SteamGameServer.SetPasswordProtected(serverSettings.IsPasswordProtected);
            SteamGameServer.SetServerName(serverSettings.ServerName);
            SteamGameServer.SetBotPlayerCount(serverSettings.BotPlayerCount);
            SteamGameServer.SetMapName(serverSettings.MapName);
            SteamGameServer.SetDedicatedServer(serverSettings.IsDedicated);
            
            if(serverSettings.RulePairs != null && serverSettings.RulePairs.Count > 0)
            {
                foreach(var pair in serverSettings.RulePairs)
                {
                    SteamGameServer.SetKeyValue(pair.key, pair.value);
                }
            }

            //SteamGameServer.SetGameData("AppId=" + steamSettings.ApplicationId.m_AppId.ToString() + ",ServerName=" + serverSettings.ServerName + "," + serverSettings.GameData);
        }

        private void LogFailure(SteamServerConnectFailure_t arg0)
        {
            Debug.LogError("Connection Failure: " + arg0.m_eResult.ToString());
        }

        private void LogConnect(SteamServersConnected_t arg0)
        {
            Debug.LogError("Connection Ready");
        }

        private void LogDisconnect(SteamServersDisconnected_t arg0)
        {
            Debug.LogError("Connection Closed: " + arg0.m_eResult.ToString());
        }

        private void LogShutDown()
        {
            Debug.LogError("Game Server Logging Off");
        }

        private void OnDisable()
        {
            //Only do this if this is a headless server build
            if (Application.isBatchMode)
            {
                Debug.Log("Logging off the Steam Game Server");

                if (serverSettings.USE_GS_AUTH_API)
                    SteamGameServer.EnableHeartbeats(false);

                //Notify listeners of the shutdown
                serverSettings.GameServerShuttingDown.Invoke();

#if MIRROR
                if (serverSettings.EnableMirror)
                    networkManager.StopServer();
#endif

                //Remove the settings event listeners
                serverSettings.GameServerShuttingDown.RemoveListener(GameServerShuttingDown.Invoke);
                serverSettings.Disconnected.RemoveListener(Disconnected.Invoke);
                serverSettings.Connected.RemoveListener(Connected.Invoke);
                serverSettings.Failure.RemoveListener(Failure.Invoke);
                serverSettings.GameServerShuttingDown.RemoveListener(LogShutDown);
                serverSettings.Disconnected.RemoveListener(LogDisconnect);
                serverSettings.Connected.RemoveListener(LogConnect);
                serverSettings.Failure.RemoveListener(LogFailure);

                //Log the server off of Steam
                SteamGameServer.LogOff();
                Debug.Log("Steam Game Server has been logged off");
            }
        }

        public static EBeginAuthSessionResult BeginAuthSession(byte[] ticket, CSteamID user)
        {
            return SteamGameServer.BeginAuthSession(ticket, ticket.Length, user);
        }

        public static void EndAuthSession(CSteamID user)
        {
            SteamGameServer.EndAuthSession(user);
        }

        public static Ticket GetAuthSessionTicket()
        {
            uint m_pcbTicket;
            var ticket = new Ticket();
            ticket.Data = new byte[1024];
            ticket.Handle = SteamUser.GetAuthSessionTicket(ticket.Data, 1024, out m_pcbTicket);
            ticket.CreatedOn = SteamUtils.GetServerRealTime();
            Array.Resize(ref ticket.Data, (int)m_pcbTicket);

            if (ActiveTickets == null)
                ActiveTickets = new List<Ticket>();

            ActiveTickets.Add(ticket);

            return ticket;
        }
    }
}
#endif