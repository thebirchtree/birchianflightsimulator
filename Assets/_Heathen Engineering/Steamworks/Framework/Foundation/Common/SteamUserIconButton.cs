#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using UnityEngine.EventSystems;

namespace HeathenEngineering.SteamApi.Foundation.UI
{
    /// <summary>
    /// Extends the <see cref="SteamUserFullIcon"/> control adding support for mouse clicks on the icon
    /// </summary>
    public class SteamUserIconButton : SteamUserFullIcon, IPointerClickHandler
    {
        /// <summary>
        /// Occures when the icon is clicked once with the left mouse button
        /// </summary>
        public UnityPersonaEvent OnLeftClick;
        /// <summary>
        /// Occures when the icon is clicked once with the middle mouse button
        /// </summary>
        public UnityPersonaEvent OnMiddleClick;
        /// <summary>
        /// Occures when the icon is clicked once with the right mouse button
        /// </summary>
        public UnityPersonaEvent OnRightClick;
        /// <summary>
        /// Occures when the icon is double clicked with the left mouse button
        /// </summary>
        public UnityPersonaEvent OnLeftDoubleClick;
        /// <summary>
        /// Occures when the icon is double clicked with the middle mouse button
        /// </summary>
        public UnityPersonaEvent OnMiddleDoubleClick;
        /// <summary>
        /// Occures when the icon is double clicked with the right mouse button
        /// </summary>
        public UnityPersonaEvent OnRightDoubleClick;

        /// <summary>
        /// handler for <see cref="IPointerClickHandler"/> see the Unity SDK for more information.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                if (eventData.clickCount > 1)
                    OnLeftDoubleClick.Invoke(UserData);
                else
                    OnLeftClick.Invoke(UserData);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (eventData.clickCount > 1)
                    OnRightDoubleClick.Invoke(UserData);
                else
                    OnRightClick.Invoke(UserData);
            }
            else if (eventData.button == PointerEventData.InputButton.Middle)
            {
                if (eventData.clickCount > 1)
                    OnMiddleDoubleClick.Invoke(UserData);
                else
                    OnMiddleClick.Invoke(UserData);
            }
        }
    }
}
#endif