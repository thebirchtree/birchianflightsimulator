#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using UnityEngine;
using HeathenEngineering.Tools;
using HeathenEngineering.Scriptable;

namespace HeathenEngineering.SteamApi.Foundation.UI
{
    /// <summary>
    /// <para>A composit control for displaying the avatar, name and status of a given <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> object.</para>
    /// </summary>
    public class SteamUserFullIcon : HeathenUIBehaviour
    {
        /// <summary>
        /// The <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> to load.
        /// This should be set by calling <see cref="HeathenEngineering.SteamApi.Foundation.UI.SteamUserFullIcon.LinkSteamUser(SteamUserData)"/>
        /// </summary>
        public SteamUserData UserData;
        /// <summary>
        /// Should the status label be shown or not
        /// </summary>
        public BoolReference ShowStatusLabel;

        /// <summary>
        /// The image to load the avatar into.
        /// </summary>
        [Header("References")]
        public UnityEngine.UI.RawImage Avatar;
        /// <summary>
        /// The text field used to display the users name
        /// </summary>
        public UnityEngine.UI.Text PersonaName;
        /// <summary>
        /// The text field used to display the users status
        /// </summary>
        public UnityEngine.UI.Text StatusLabel;
        /// <summary>
        /// An image board around the icon ... this will have its color changed based on status
        /// </summary>
        public UnityEngine.UI.Image IconBorder;
        /// <summary>
        /// The root object containing the status label parts ... this is what is enabled or disabled as the label is shown or hidden.
        /// </summary>
        public GameObject StatusLabelContainer;
        /// <summary>
        /// Should the persona name be colored based on status
        /// </summary>
        public bool ColorThePersonaName = true;
        /// <summary>
        /// Should the status label be colored based on status
        /// </summary>
        public bool ColorTheStatusLabel = true;
        /// <summary>
        /// <para></para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        [Header("Border Colors")]
        public ColorReference OfflineColor;
        /// <summary>
        /// <para>The color to use for Online</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        public ColorReference OnlineColor;
        /// <summary>
        /// <para>The color to use for Away</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        public ColorReference AwayColor;
        /// <summary>
        /// <para>The color to use for Buisy</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        public ColorReference BuisyColor;
        /// <summary>
        /// <para>The color to use for Snooze</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        public ColorReference SnoozeColor;
        /// <summary>
        /// <para>The color to use for the Want to Play status</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        public ColorReference WantPlayColor;
        /// <summary>
        /// <para>The color to use for the Want to Trade status</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        public ColorReference WantTradeColor;
        /// <summary>
        /// <para>Color to use for In Game satus</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        public ColorReference InGameColor;
        /// <summary>
        /// <para>The color to use when in this specific game</para>
        /// <para>You can use <see cref="HeathenEngineering.Scriptable.ColorVariable"/> to configure these with an asset or set them in editor.</para>
        /// </summary>
        public ColorReference ThisGameColor;

        private void Start()
        {
            if (UserData != null)
                LinkSteamUser(UserData);
        }

        private void Update()
        {
            if (ShowStatusLabel.Value != StatusLabelContainer.activeSelf)
                StatusLabelContainer.SetActive(ShowStatusLabel.Value);
        }

        /// <summary>
        /// Sets and registeres for the provided <see cref="HeathenEngineering.SteamApi.Foundation.SteamUserData"/> object.
        /// </summary>
        /// <param name="newUserData">The user to connect to and to display the avatar for.</param>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description>Set the icon to display the current user as read from the SteamSettings settings member.</description>
        /// <code>
        /// myUserFullIcon.LinkSteamUser(settings.UserData);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void LinkSteamUser(SteamUserData newUserData)
        {
            if (UserData != null)
            {
                if (UserData.OnAvatarChanged != null)
                    UserData.OnAvatarChanged.RemoveListener(handleAvatarChange);
                if (UserData.OnStateChange != null)
                    UserData.OnStateChange.RemoveListener(handleStateChange);
                if (UserData.OnNameChanged != null)
                    UserData.OnNameChanged.RemoveListener(handleNameChanged);
                if (UserData.OnAvatarLoaded != null)
                    UserData.OnAvatarLoaded.RemoveListener(handleAvatarChange);
            }

            UserData = newUserData;
            handleAvatarChange();
            handleNameChanged();
            handleStateChange();

            if (UserData != null)
            {
                if (!UserData.IconLoaded)
                    SteamworksFoundationManager.Instance.Settings.RefreshAvatar(UserData);

                Avatar.texture = UserData.Avatar;
                if (UserData.OnAvatarChanged == null)
                    UserData.OnAvatarChanged = new UnityEngine.Events.UnityEvent();
                UserData.OnAvatarChanged.AddListener(handleAvatarChange);
                if (UserData.OnStateChange == null)
                    UserData.OnStateChange = new UnityEngine.Events.UnityEvent();
                UserData.OnStateChange.AddListener(handleStateChange);
                if (UserData.OnNameChanged == null)
                    UserData.OnNameChanged = new UnityEngine.Events.UnityEvent();
                UserData.OnNameChanged.AddListener(handleNameChanged);
                if (UserData.OnAvatarLoaded == null)
                    UserData.OnAvatarLoaded = new UnityEngine.Events.UnityEvent();
                UserData.OnAvatarLoaded.AddListener(handleAvatarChange);
            }
        }

        private void handleNameChanged()
        {
            PersonaName.text = UserData.DisplayName;
        }

        private void handleAvatarChange()
        {
            Avatar.texture = UserData.Avatar;
        }

        private void handleStateChange()
        {
            switch(UserData.State)
            {
                case Steamworks.EPersonaState.k_EPersonaStateAway:
                    if (UserData.InGame)
                    {
                        if (UserData.GameInfo.m_gameID.AppID().m_AppId == SteamworksFoundationManager.Instance.Settings.ApplicationId.m_AppId)
                        {
                            StatusLabel.text = "Playing";
                            IconBorder.color = ThisGameColor.Value;
                        }
                        else
                        {
                            StatusLabel.text = "In-Game";
                            IconBorder.color = InGameColor.Value;
                        }
                    }
                    else
                    {
                        StatusLabel.text = "Away";
                        IconBorder.color = AwayColor.Value;
                    }
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateBusy:
                    if (UserData.InGame)
                    {
                        if (UserData.GameInfo.m_gameID.AppID().m_AppId == SteamworksFoundationManager.Instance.Settings.ApplicationId.m_AppId)
                        {
                            StatusLabel.text = "Playing";
                            IconBorder.color = ThisGameColor.Value;
                        }
                        else
                        {
                            StatusLabel.text = "In-Game";
                            IconBorder.color = InGameColor.Value;
                        }
                    }
                    else
                    {
                        StatusLabel.text = "Buisy";
                        IconBorder.color = BuisyColor.Value;
                    }
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateLookingToPlay:
                    StatusLabel.text = "Looking to Play";
                    IconBorder.color = WantPlayColor.Value;
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateLookingToTrade:
                    StatusLabel.text = "Looking to Trade";
                    IconBorder.color = WantTradeColor.Value;
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateOffline:
                    StatusLabel.text = "Offline";
                    IconBorder.color = OfflineColor.Value;
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateOnline:
                    if (UserData.InGame)
                    {
                        if (UserData.GameInfo.m_gameID.AppID().m_AppId == SteamworksFoundationManager.Instance.Settings.ApplicationId.m_AppId)
                        {
                            StatusLabel.text = "Playing";
                            IconBorder.color = ThisGameColor.Value;
                        }
                        else
                        {
                            StatusLabel.text = "In-Game";
                            IconBorder.color = InGameColor.Value;
                        }
                    }
                    else
                    {
                        StatusLabel.text = "Online";
                        IconBorder.color = OnlineColor.Value;
                    }
                    break;
                case Steamworks.EPersonaState.k_EPersonaStateSnooze:
                    if (UserData.InGame)
                    {
                        if (UserData.GameInfo.m_gameID.AppID().m_AppId == SteamworksFoundationManager.Instance.Settings.ApplicationId.m_AppId)
                        {
                            StatusLabel.text = "Playing";
                            IconBorder.color = ThisGameColor.Value;
                        }
                        else
                        {
                            StatusLabel.text = "In-Game";
                            IconBorder.color = InGameColor.Value;
                        }
                    }
                    else
                    {
                        StatusLabel.text = "Snooze";
                        IconBorder.color = SnoozeColor.Value;
                    }
                    break;
            }
            if (ColorTheStatusLabel)
                StatusLabel.color = IconBorder.color;
            if (ColorThePersonaName)
                PersonaName.color = IconBorder.color;
        }

        private void OnDestroy()
        {
            if (UserData != null)
                UserData.OnAvatarChanged.RemoveListener(handleAvatarChange);
        }
    }
}
#endif