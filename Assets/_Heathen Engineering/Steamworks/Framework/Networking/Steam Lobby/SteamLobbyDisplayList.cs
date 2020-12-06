#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.Networking
{
    public class SteamLobbyDisplayList : MonoBehaviour
    {
        public SteamworksLobbySettings LobbySettings;
        public LobbyHunterFilter Filter;
        public LobbyRecordBehvaiour recordPrototype;
        public Transform collection;
        public UnityEvent OnSearchStarted;
        public UnityEvent OnSearchCompleted;
        public UnitySteamIdEvent OnLobbySelected;

        private void OnEnable()
        {
            if (LobbySettings != null && LobbySettings.Manager != null)
            {
                LobbySettings.OnLobbyMatchList.AddListener(HandleBrowseLobbies);
            }
            else
            {
                Debug.LogWarning("SteamLobbyDisplayList requires a HeathenSteamLobbySettings reference which has been registered to a HeathenSteamLobbyManager. If you have provided a HeathenSteamLobbySettings that has been applied to an active HeathenSteamLobbyManager then check to insure that the HeathenSteamLobbyManager has initalized before this control.");
                this.enabled = false;
            }
        }

        private void OnDisable()
        {
            if (LobbySettings != null && LobbySettings.Manager != null)
            {
                LobbySettings.OnLobbyMatchList.RemoveListener(HandleBrowseLobbies);
            }
        }

        private void HandleBrowseLobbies(SteamLobbyLobbyList lobbies)
        {
            foreach (var l in lobbies)
            {
                var go = Instantiate(recordPrototype.gameObject, collection);
                var rec = go.GetComponent<LobbyRecordBehvaiour>();
                rec.SetLobby(l, LobbySettings);
                rec.OnSelected.AddListener(HandleOnSelected);
            }
            OnSearchCompleted.Invoke();
        }

        private void HandleOnSelected(CSteamID lobbyId)
        {
            //Pass the event on
            OnLobbySelected.Invoke(lobbyId);
        }

        public void QuickMatch()
        {
            OnSearchStarted.Invoke();
            LobbySettings.Manager.QuickMatch(Filter, SteamworksFoundationManager.Instance.Settings.UserData.DisplayName, true);
        }

        public void QuickMatch(string onCreateName)
        {
            OnSearchStarted.Invoke();
            LobbySettings.Manager.QuickMatch(Filter, onCreateName, true);
        }

        public void BrowseLobbies()
        {
            OnSearchStarted.Invoke();
            LobbySettings.Manager.FindMatch(Filter);
        }

        public void ClearLobbies()
        {
            while (collection.childCount > 0)
            {
                var target = collection.GetChild(0);
                var go = target.gameObject;
                go.SetActive(false);
                target.parent = null;
                Destroy(go);
            }
        }
    }
}
#endif