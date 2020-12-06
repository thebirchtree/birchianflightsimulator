#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamApi.Editors
{
    [CustomEditor(typeof(SteamworksFoundationManager))]
    public class SteamworksFoundationManagerEditor : Editor
    {
        private SteamworksFoundationManager pManager;
        private SerializedProperty Settings;
        
        private SerializedProperty DoNotDestroyOnLoad;
        private SerializedProperty OnSteamInitalized;
        private SerializedProperty OnSteamInitalizationError;
        private SerializedProperty OnOverlayActivated;
        private SerializedProperty OnUserStatsRecieved;
        private SerializedProperty OnUserStatsStored;
        private SerializedProperty OnAchievementStored;
        private SerializedProperty OnAvatarLoaded;
        private SerializedProperty OnPersonaStateChanged;
        private SerializedProperty OnNumberOfCurrentPlayersResult;
        private SerializedProperty OnRecievedFriendChatMessage;
        public Texture2D achievementIcon;
        public Texture2D statIcon;
        public Texture2D dropBoxTexture;


        private int tabPage = 0;
        private int appTabPage = 0;
        private int seTab = 0;

        private void OnEnable()
        {
            Settings = serializedObject.FindProperty("Settings");
            
            DoNotDestroyOnLoad = serializedObject.FindProperty("_doNotDistroyOnLoad");
            OnSteamInitalized = serializedObject.FindProperty("OnSteamInitalized");
            OnSteamInitalizationError = serializedObject.FindProperty("OnSteamInitalizationError");
            OnOverlayActivated = serializedObject.FindProperty("OnOverlayActivated");

            OnUserStatsRecieved = serializedObject.FindProperty("OnUserStatsRecieved");
            OnUserStatsStored = serializedObject.FindProperty("OnUserStatsStored");
            OnAchievementStored = serializedObject.FindProperty("OnAchievementStored");
            OnAvatarLoaded = serializedObject.FindProperty("OnAvatarLoaded");
            OnPersonaStateChanged = serializedObject.FindProperty("OnPersonaStateChanged");
            OnRecievedFriendChatMessage = serializedObject.FindProperty("OnRecievedFriendChatMessage");

            OnNumberOfCurrentPlayersResult = serializedObject.FindProperty("OnNumberOfCurrentPlayersResult");
        }

        public override void OnInspectorGUI()
        { 
            pManager = target as SteamworksFoundationManager;

            EditorGUILayout.PropertyField(Settings);

            if (pManager.Settings == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Assign a Steam Settings object to get started!");
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Create a new Steam Settings object by right clicking in your Project panel and selecting [Create] > [Steamworks] > [Steam Settings]", MessageType.Info);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                seTab = GUILayout.Toggle(seTab == 0, "Settings", EditorStyles.toolbarButton) ? 0 : seTab;
                seTab = GUILayout.Toggle(seTab == 1, "Events", EditorStyles.toolbarButton) ? 1 : seTab;
                EditorGUILayout.EndHorizontal();

                if (seTab == 0)
                {
                    if (pManager.Settings != null)
                    {
                        GeneralDropAreaGUI("... Drop Stats & Achievments Here ...", pManager);

                        DrawAppOverlayData(pManager);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        DrawStatsList(pManager);
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        DrawAchievementList(pManager);
                    }
                }
                else
                {
                    if (pManager.Settings != null)
                    {

                        EditorGUILayout.BeginHorizontal();

                        appTabPage = GUILayout.Toggle(appTabPage == 0, "Application", EditorStyles.toolbarButton) ? 0 : appTabPage;
                        appTabPage = GUILayout.Toggle(appTabPage == 1, "Overlay", EditorStyles.toolbarButton) ? 1 : appTabPage;
                        appTabPage = GUILayout.Toggle(appTabPage == 2, "Friends", EditorStyles.toolbarButton) ? 2 : appTabPage;
                        EditorGUILayout.EndHorizontal();

                        if (appTabPage == 0)
                        {
                            EditorGUILayout.PropertyField(OnNumberOfCurrentPlayersResult);
                            EditorGUILayout.PropertyField(OnSteamInitalized);
                            EditorGUILayout.PropertyField(OnSteamInitalizationError);
                        }
                        else if (appTabPage == 1)
                        {
                            EditorGUILayout.PropertyField(OnOverlayActivated);
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(OnAvatarLoaded);
                            EditorGUILayout.PropertyField(OnPersonaStateChanged);
                            EditorGUILayout.PropertyField(OnRecievedFriendChatMessage);
                            EditorGUILayout.PropertyField(OnUserStatsRecieved);
                            EditorGUILayout.PropertyField(OnUserStatsStored);
                            EditorGUILayout.PropertyField(OnAchievementStored);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Requires Steam Settings");
                    }
                }
                //}
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSteamUserData(SteamworksFoundationManager pManager)
        {
            if(pManager.Settings == null)
            {
                EditorGUILayout.HelpBox("Requires Steam Settings", MessageType.Info);
                return;
            }

            if(pManager.Settings.UserData == null)
            {
                EditorGUILayout.HelpBox("Requires you reference a Steam User Data object in your Steam Settings", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Steam Id");
            EditorGUILayout.LabelField(pManager != null ? pManager.Settings.UserData.SteamId.m_SteamID.ToString() : "unknown");
            EditorGUILayout.EndHorizontal();

            if (pManager.Settings.UserData.State == Steamworks.EPersonaState.k_EPersonaStateAway)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Away");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.Settings.UserData.State == Steamworks.EPersonaState.k_EPersonaStateBusy)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Busy");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.Settings.UserData.State == Steamworks.EPersonaState.k_EPersonaStateLookingToPlay)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Looking to Play");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.Settings.UserData.State == Steamworks.EPersonaState.k_EPersonaStateLookingToTrade)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Looking to Trade");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.Settings.UserData.State == Steamworks.EPersonaState.k_EPersonaStateMax)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Max");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.Settings.UserData.State == Steamworks.EPersonaState.k_EPersonaStateOffline)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Offline");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.Settings.UserData.State == Steamworks.EPersonaState.k_EPersonaStateOnline)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Online");
                EditorGUILayout.EndHorizontal();
            }
            else if (pManager.Settings.UserData.State == Steamworks.EPersonaState.k_EPersonaStateSnooze)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("Snooze");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.LabelField("unknown");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Steam Name");
            EditorGUILayout.LabelField(pManager != null && !string.IsNullOrEmpty(pManager.Settings.UserData.DisplayName) ? pManager.Settings.UserData.DisplayName : "unknown");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Avatar");
            Rect iRect = EditorGUILayout.GetControlRect(true, 150);
            EditorGUILayout.EndHorizontal();

            //iRect.y += iRect.height;
            iRect.width = 150;
            //iRect.height = 150;

            EditorGUILayout.Space();

            if (pManager.Settings.UserData.Avatar != null)
            {
                EditorGUI.DrawPreviewTexture(iRect, pManager.Settings.UserData.Avatar);
            }
            else
            {
                EditorGUI.DrawRect(iRect, Color.black);
            }
        }

        private void DrawAppOverlayData(SteamworksFoundationManager pManager)
        {
            EditorGUILayout.PropertyField(DoNotDestroyOnLoad);
            EditorGUILayout.BeginHorizontal();
            if (pManager.Settings != null)
            {
                var v = System.Convert.ToUInt32(EditorGUILayout.IntField("Steam App Id", System.Convert.ToInt32(pManager.Settings.ApplicationId.m_AppId)));
                if (v != pManager.Settings.ApplicationId.m_AppId)
                {
                    pManager.Settings.ApplicationId.m_AppId = v;
                    EditorUtility.SetDirty(pManager.Settings);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Notification Settings");
            if (pManager.Settings == null)
            {
                EditorGUILayout.LabelField("Requires Steam Settings");
            }
            else
            {
                int cSelected = (int)pManager.Settings.NotificationPosition;

                EditorGUILayout.BeginVertical();
                cSelected = EditorGUILayout.Popup(cSelected, new string[] { "Top Left", "Top Right", "Bottom Left", "Bottom Right" });

                var v = EditorGUILayout.Vector2IntField(GUIContent.none, pManager.Settings.NotificationInset);
                if (pManager.Settings.NotificationInset != v)
                {
                    pManager.Settings.NotificationInset = v;
                    EditorUtility.SetDirty(pManager.Settings);
                }
                EditorGUILayout.EndVertical();

                if (pManager.Settings.NotificationPosition != (ENotificationPosition)cSelected)
                {
                    pManager.Settings.NotificationPosition = (ENotificationPosition)cSelected;
                    EditorUtility.SetDirty(pManager.Settings);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatsList(SteamworksFoundationManager pManager)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Stats", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < pManager.Settings.stats.Count; i++)
            {
                Color sC = GUI.backgroundColor;

                GUI.backgroundColor = sC;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(statIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(pManager.Settings.stats[i]);
                    Selection.activeObject = pManager.Settings.stats[i];
                }
                if (GUILayout.Button(pManager.Settings.stats[i].name.Replace(" Float Stat Data", "").Replace(" Int Stat Data", "").Replace("Float Stat Data ", "").Replace("Int Stat Data ", "") + " ID", EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(pManager.Settings.stats[i]);
                }

                pManager.Settings.stats[i].statName = EditorGUILayout.TextField(pManager.Settings.stats[i].statName);

                var color = GUI.contentColor;
                GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    pManager.Settings.stats.RemoveAt(i);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;
        }

        private void DrawAchievementList(SteamworksFoundationManager pManager)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Achievements", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            int il = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            for (int i = 0; i < pManager.Settings.achievements.Count; i++)
            {
                Color sC = GUI.backgroundColor;

                GUI.backgroundColor = sC;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(achievementIcon, EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(pManager.Settings.stats[i]);
                    Selection.activeObject = pManager.Settings.stats[i];
                }
                if (GUILayout.Button(pManager.Settings.achievements[i].name.Replace("Steam Achievement Data ", "") + " ID", EditorStyles.toolbarButton))
                {
                    GUI.FocusControl(null);
                    EditorGUIUtility.PingObject(pManager.Settings.achievements[i]);
                }

                pManager.Settings.achievements[i].achievementId = EditorGUILayout.TextField(pManager.Settings.achievements[i].achievementId);

                var color = GUI.contentColor;
                GUI.contentColor = new Color(1, 0.50f, 0.50f, 1);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    GUI.FocusControl(null);
                    pManager.Settings.achievements.RemoveAt(i);
                    return;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel = il;
        }
        
        private bool GeneralDropAreaGUI(string message, SteamworksFoundationManager pManager)
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 70.0f, GUILayout.ExpandWidth(true));

            var style = new GUIStyle(GUI.skin.box);
            style.normal.background = dropBoxTexture;
            style.normal.textColor = Color.white;
            style.border = new RectOffset(20, 20, 20, 20);
            var color = GUI.backgroundColor;
            var fontColor = GUI.contentColor;
            GUI.backgroundColor = SteamUtilities.Colors.SteamGreen * SteamUtilities.Colors.HalfAlpha;
            GUI.contentColor = SteamUtilities.Colors.BrightGreen;
            GUI.Box(drop_area, "\n\n" + message, style);
            GUI.backgroundColor = color;
            GUI.contentColor = fontColor;

            bool result = false;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return false;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            // Do On Drag Stuff here
                            if (dragged_object.GetType() == typeof(SteamFloatStatData) || dragged_object.GetType() == typeof(SteamIntStatData))
                            {
                                SteamStatData go = dragged_object as SteamStatData;
                                if (!pManager.Settings.stats.Exists(p => p == go))
                                {
                                    pManager.Settings.stats.Add(go);
                                    EditorUtility.SetDirty(pManager.Settings);
                                    result = true;
                                }
                            }
                            else if (dragged_object.GetType() == typeof(SteamAchievementData))
                            {
                                SteamAchievementData go = dragged_object as SteamAchievementData;
                                if (!pManager.Settings.achievements.Exists(p => p == go))
                                {
                                    pManager.Settings.achievements.Add(go);
                                    EditorUtility.SetDirty(pManager.Settings);
                                    result = true;
                                }
                            }
                        }
                    }
                    break;
            }

            return result;
        }

    }
}
#endif