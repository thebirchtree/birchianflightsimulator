#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using Steamworks;
using System;

namespace HeathenEngineering.SteamApi.Foundation
{
    /// <summary>
    /// <para>A wrapper around common SteamAPI Overlay funcitonlity. This class is used in the <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> object to provide access to Overlay funcitons and features.</para>
    /// </summary>
    [Serializable]
    public class HeathensSteamOverlay
    {
        /// <summary>
        /// <para>A wrap around <see cref="Steamworks.SteamUtils.IsOverlayEnabled()"/></para>   
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamUtils#IsOverlayEnabled">https://partner.steamgames.com/doc/api/ISteamUtils#IsOverlayEnabled</a> for more information.
        /// </summary>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Checks if the Steam Overlay is running & the user can access it. The overlay process could take a few seconds to start & hook the game process, so this function will initially return false while the overlay is loading.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings.</para></description>
        /// <code>
        /// if(settings.Overlay.IsEnable)
        ///      Debug.Log("The overlay is enabled and ready for use!");
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public bool IsEnabled
        {
            get
            {
                return Steamworks.SteamUtils.IsOverlayEnabled();
            }
        }
        private bool _OverlayOpen = false;

        /// <summary>
        /// <para>Indicates that the Steam Overlay is currently open.</para>   
        /// See <a href="https://partner.steamgames.com/doc/features/overlay">https://partner.steamgames.com/doc/features/overlay</a> for more information.
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Indicates that the overlay is currently open.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings.</para></description>
        /// <code>
        /// if(settings.Overlay.IsOpen)
        ///      Debug.Log("The overlay is currently open.");
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public bool IsOpen
        {
            get
            {
                return _OverlayOpen;
            }
        }

        /// <summary>
        /// For internal use only
        /// </summary>
        /// <param name="data"></param>
        public void HandleOnOverlayOpen(GameOverlayActivated_t data)
        {
            _OverlayOpen = data.m_bActive == 1;
        }

        /// <summary>
        /// <para>A wrap around <see cref="Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(CSteamId lobbyId)"/>.</para>   
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayInviteDialog">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayInviteDialog</a> for more information.
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the overlay with the invite dialog populated for the indicated lobby.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.Invite(myLobby);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void Invite(CSteamID lobbyId)
        {
            Steamworks.SteamFriends.ActivateGameOverlayInviteDialog(lobbyId);
        }

        /// <summary>
        /// <para>Opens the overlay to the current games store page.</para>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to the current apps store page.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenStore();
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void OpenStore()
        {
            OpenStore(SteamworksFoundationManager.Instance.Settings != null ? SteamworksFoundationManager.Instance.Settings.ApplicationId : AppId_t.Invalid, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
        }

        /// <summary>
        /// <para>Opens the overlay to the store page of the provide app Id.</para>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to the current apps store page.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenStore(settings.ApplicationId);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="appId">The application id of the game you wish to open the store to</param>
        public void OpenStore(uint appId)
        {
            OpenStore(new AppId_t(appId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
        }

        /// <summary>
        /// <para>Opens the overlay to the store page of the provide app Id with the provided overlay store flag.</para>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to the current apps store page.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenStore(settings.ApplicationId.m_AppId, EOverlayToStoreFlag.k_EOverlayToStoreFlag_AddToCartAndShow);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="appId">The application id of the game you wish to open the store to</param>
        /// <param name="flag">Modifies the behaviour of the store page when opened.</param>
        public void OpenStore(uint appId, EOverlayToStoreFlag flag)
        {
            OpenStore(new AppId_t(appId), flag);
        }

        /// <summary>
        /// <para>Opens the overlay to the store page of the provide app Id with the provided overlay store flag.</para>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToStore</a> for more information.
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to the current apps store page.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenStore(settings.ApplicationId.m_AppId, EOverlayToStoreFlag.k_EOverlayToStoreFlag_AddToCartAndShow);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="appId">The application id of the game you wish to open the store to, See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#EOverlayToStoreFlag">https://partner.steamgames.com/doc/api/ISteamFriends#EOverlayToStoreFlag</a> for more details</param>
        public void OpenStore(AppId_t appId, EOverlayToStoreFlag flag)
        {
            Steamworks.SteamFriends.ActivateGameOverlayToStore(appId, flag);
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to the indicated dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenStore("friends");
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
        public void Open(string dialog)
        {
            Steamworks.SteamFriends.ActivateGameOverlay(dialog);
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToWebPage">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlayToWebPage</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to the indicated web page.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenWebPage("http://www.google.com");
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
        public void OpenWebPage(string URL)
        {
            Steamworks.SteamFriends.ActivateGameOverlayToWebPage(URL);
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to friends dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenFriends();
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void OpenFriends()
        {
            Steamworks.SteamFriends.ActivateGameOverlay("friends");
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to community dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenCommunity();
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void OpenCommunity()
        {
            Steamworks.SteamFriends.ActivateGameOverlay("community");
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to players dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenPlayers();
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void OpenPlayers()
        {
            Steamworks.SteamFriends.ActivateGameOverlay("players");
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to settings dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenSettings();
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void OpenSettings()
        {
            Steamworks.SteamFriends.ActivateGameOverlay("settings");
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to offical game group dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenOfficialGameGroup();
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void OpenOfficialGameGroup()
        {
            Steamworks.SteamFriends.ActivateGameOverlay("officialgamegroup");
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to stats dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenStats();
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void OpenStats()
        {
            Steamworks.SteamFriends.ActivateGameOverlay("stats");
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to achievements dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenArchievements();
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void OpenAchievements()
        {
            Steamworks.SteamFriends.ActivateGameOverlay("achievements");
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to chat dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenChat(myFriendId);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="user">The user to open the chat dialog with</param>
        public void OpenChat(CSteamID user)
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("Chat", user);
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to profile dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenProfile(myFriendId);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="user">The user whoes profile you want to open</param>
        public void OpenProfile(CSteamID user)
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("steamid", user);
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to a trade dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenTrade(myFriendId);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="user">The user who you want to trade with</param>
        public void OpenTrade(CSteamID user)
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("jointrade", user);
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to stats dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenStats(myFriendId);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="user">The user whoes stats you want to display</param>
        public void OpenStats(CSteamID user)
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("stats", user);
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to achievements dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenAchievements(myFriendId);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="user">The id of the user whoes achievements you want to display</param>
        public void OpenAchievements(CSteamID user)
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("achievements", user);
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to friends add dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenFriendAdd(myFriendId);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="user">The Id of the user you want to add as a friend</param>
        public void OpenFriendAdd(CSteamID user)
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendadd", user);
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to friend remove dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenFriendRemove(userId);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="user">The user you want to remove from friends</param>
        public void OpenFriendRemove(CSteamID user)
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendremove", user);
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to request accept dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenRequestAccept(userId);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="user">The user whoes request you want to accept</param>
        public void OpenRequestAccept(CSteamID user)
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendrequestaccept", user);
        }

        /// <summary>
        /// See <a href="https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay">https://partner.steamgames.com/doc/api/ISteamFriends#ActivateGameOverlay</a> for details
        /// </summary>
        /// <remarks>
        /// <para>Note that Steam Overlay depends on the game having a signle window handle. As Visual Studio and Unity Editor have many the overlay system will not work as expected when simulating in Unity Editor or when debugging with the Visual Studio IDE.</para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description><para>Activates the Steam Overlay to request ignore dialog.</para>
        /// <para>The following examples assume a variable or paramiter of type <see cref="HeathenEngineering.SteamApi.Foundation.SteamSettings"/> is available and named settings and that myLobby is a valid lobby id.</para></description>
        /// <code>
        /// settings.Overlay.OpenFriends();
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        /// <param name="user">The user whoes request you want to ignore</param>
        public void OpenRequestIgnore(CSteamID user)
        {
            Steamworks.SteamFriends.ActivateGameOverlayToUser("friendrequestignore", user);
        }
    }
}
#endif