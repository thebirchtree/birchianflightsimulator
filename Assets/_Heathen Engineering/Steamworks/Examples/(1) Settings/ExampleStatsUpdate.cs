#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using UnityEngine;

namespace HeathenEngineering.SteamApi.Foundation.Demo
{
    public class ExampleStatsUpdate : MonoBehaviour
    {
        /// <summary>
        /// Reference to the <see cref="SteamSettings"/> object
        /// </summary>
        public SteamSettings SteamSettings;
        /// <summary>
        /// Reference to the <see cref="SteamFloatStatData"/> object
        /// </summary>
        public SteamFloatStatData StatDataObject;
        /// <summary>
        /// Reference to the <see cref="SteamAchievementData"/> object
        /// </summary>
        public SteamAchievementData WinnerAchievement;
        /// <summary>
        /// Used to display the current stat value
        /// </summary>
        public UnityEngine.UI.Text StatValue;
        /// <summary>
        /// Used to display the current achievement unlock status
        /// </summary>
        public UnityEngine.UI.Text WinnerAchievmentStatus;

        private void Update()
        {
            StatValue.text = "Feet Traveled = " + StatDataObject.Value.ToString();
            WinnerAchievmentStatus.text = WinnerAchievement.displayName + "\n" + (WinnerAchievement.isAchieved ? "(Unlocked)" : "(Locked)");
        }

        /// <summary>
        /// Sets and stores the value of the Steam stat
        /// </summary>
        /// <param name="amount"></param>
        public void UpdateStatValue(float amount)
        {
            StatDataObject.SetFloatStat(StatDataObject.Value + amount);
            SteamSettings.StoreStatsAndAchievements();
        }

        /// <summary>
        /// Open the Valve documentation to the Achievements page
        /// </summary>
        public void GetHelp()
        {
            Application.OpenURL("https://partner.steamgames.com/doc/features/achievements");
        }

        /// <summary>
        /// Open the valve documentation to the Overlay page
        /// </summary>
        public void GetOverlayHelp()
        {
            Application.OpenURL("https://partner.steamgames.com/doc/features/overlay");
        }

        /// <summary>
        /// Notify when the stats are recieved
        /// This is meant to be connected to the Unity Events on the Steamworks Foundation Manager
        /// </summary>
        public void OnRetrieveStatsAndAchievements()
        {
            Debug.Log("[ExampleStatsUpdate.OnRetrieveStatsAndAchievement]\nStats loaded!");
        }

        /// <summary>
        /// Notify when the stats are stored
        /// This is meant to be connected to the Unity Events on the Steamworks Foundation Manager
        /// </summary>
        public void OnStoredStatsAndAchievements()
        {
            Debug.Log("[ExampleStatsUpdate.OnStoredStatsAndAchievements]\nStats stored!");
        }
    }
}
#endif