#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// Represents the metadat of a DLC entry for a given app
    /// This is returned by <see cref="HeathenEngineering.SteamApi.SteamUtilities.GetDLCData"/>
    /// </summary>
    /// <example>
    /// <list type="bullet">
    /// <item>
    /// <description>Return the list of all DLC for the current application</description>
    /// <code>
    /// var results = SteamUtilities.GetDLCData();
    /// foreach(var result in results)
    /// {
    ///    Debug.Log("Located DLC " + result.name " with an AppId of " + result.appId.m_AppId.ToString() + ", this DLC is " + (result.available ? "available!" : "not available!"));
    /// }
    /// </code>
    /// </item>
    /// </list>
    /// </example>
    public struct AppDlcData
    {
        public AppId_t appId;
        public string name;
        public bool available;
    }
}
#endif