using HeathenEngineering.SteamApi.Foundation;
using UnityEngine;

public class DirtyTestScripts : MonoBehaviour
{
    public ulong Id = 76561197995794890;

    private void OnEnable()
    {
        var name = Steamworks.SteamFriends.GetFriendPersonaName(new Steamworks.CSteamID(Id));
        Debug.Log("Raw API User name: " + name);

        var userData = SteamworksFoundationManager.Instance.GetUserData(Id);
        if(userData != null)
        {
            Debug.Log("User Data display name: " + userData.DisplayName);
        }
    }
}
