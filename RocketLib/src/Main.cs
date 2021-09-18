﻿using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;



namespace RocketLibLoadMod
{
    using RocketLib;

    /// <summary>
    /// Really, you don't need it. Anyway you will just have the function of the UnityModManager.
    /// </summary>
    public class Main
    {
        /// <summary>
        /// </summary>
        public static UnityModManager.ModEntry mod;
        /// <summary>
        /// </summary>
        public static bool enabled;
        /// <summary>
        /// </summary>
        public static Settings settings;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;

            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = OnGui;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnHideGUI = OnHideGUI;

            settings = Settings.Load<Settings>(modEntry);

            var harmony = new Harmony(modEntry.Info.Id);
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                harmony.PatchAll(assembly);
            }
            catch (Exception ex)
            {
                mod.Logger.Log(ex.ToString());
            }

            try
            {
                RocketLib.Load();
            }
            catch (Exception ex)
            {
                mod.Logger.Log("Error while Loading RocketLib :\n" + ex.ToString());

            }

            FirstLaunch();

            return true;
        }
        static void OnGui(UnityModManager.ModEntry modEntry)
        {
            GUIStyle testBtnStyle = new GUIStyle("button");
            testBtnStyle.normal.textColor = Color.yellow;
            /*if (GUILayout.Button("TEST", testBtnStyle, new GUILayoutOption[] { GUILayout.Width(150)}))
            {
                try
                {
                }
                catch (Exception ex)
                {
                    Main.Log(ex);

                }
            }*/
            
            GUILayout.BeginHorizontal();
            settings.DebugMode = GUILayout.Toggle(settings.DebugMode, "Enable Debug log");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Show the current bros order in log")) RocketLib._HeroUnlockController.ShowHeroUnlockIntervals();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.OnScreenLog = GUILayout.Toggle(settings.OnScreenLog, "Enable OnScreenLog");
            settings.ShowScreenLogOption = GUILayout.Toggle(settings.ShowScreenLogOption, "Show ScreenLog option");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (settings.ShowScreenLogOption)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.BeginVertical();
                if (GUILayout.Button("Clear Log on screen", GUILayout.Width(150)))
                {
                    RocketLib.ScreenLogger.Clear();
                }
                settings.ShowManagerLog = GUILayout.Toggle(settings.ShowManagerLog, new GUIContent("Show Unity Mod Manager Log"/*, "Don't recommend to do it, because you can have double Log."*/));
                Rect lastRect = GUILayoutUtility.GetLastRect();
                lastRect.y += 20;
                lastRect.width += 300;
                GUILayout.Label(GUI.tooltip);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Time before log disapear : "+settings.logTimer.ToString(), GUILayout.ExpandWidth(false));
                settings.logTimer = (int)GUILayout.HorizontalScrollbar(settings.logTimer, 1f, 1f, 11f, GUILayout.MaxWidth(200));
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
        static void OnHideGUI(UnityModManager.ModEntry modEntry)
        {
            if (!LevelEditorGUI.IsActive) ShowMouseController.ShowMouse = false;
            Cursor.lockState = CursorLockMode.None;
        }
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        internal static void Log(object str, RLogType type = RLogType.Log)
        {
            if (!RocketLib.ScreenLogger.isSuccessfullyLoad) mod.Logger.Log(str.ToString());
            else
            {
                RocketLib.ScreenLogger.ModId = "RocketLib";
                mod.Logger.Log(str.ToString());
                RocketLib.ScreenLogger.Log(str, type);
            }
        }

        internal static void Dbg(object str, RLogType type = RLogType.Information)
        {
            if (settings.DebugMode)
            {
                string prefixDbg = " [RocketLib DEBUG] : ";
                Main.Log(prefixDbg + str, type);
            }
        }

        static void FirstLaunch()
        {
            if(!settings.HaveItFirstLaunch)
            {
                settings.logTimer = 3;
                settings.OnScreenLog = true;
                settings.DebugMode = false;
                settings.ShowManagerLog = false;

                settings.HaveItFirstLaunch = true;
                OnSaveGUI(mod);
            }
        }
    }

    /// <summary>
    /// Really, you don't need it. Anyway you will just have the function of the UnityModManager.
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        // ScreenLogger Option
        /// <summary>
        /// </summary>
        public bool OnScreenLog;
        /// <summary>
        /// </summary>
        public bool ShowScreenLogOption;
        /// <summary>
        /// </summary>
        public bool ShowManagerLog;
        /// <summary>
        /// </summary>
        public bool DebugMode;
        /// <summary>
        /// </summary>
        public bool HaveItFirstLaunch;
        /// <summary>
        /// </summary>
        public int logTimer;

        /// <summary>
        /// </summary>
        /// <param name="modEntry"></param>
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }

    // Patch for don't destroy Mod.
    [HarmonyPatch(typeof(PauseMenu), "ReturnToMenu")]
    static class PauseMenu_ReturnToMenu_Patch
    {
        static bool Prefix(PauseMenu __instance)
        {
            if (!Main.enabled)
            {
                return true;
            }

            PauseGameConfirmationPopup m_ConfirmationPopup = (Traverse.Create(__instance).Field("m_ConfirmationPopup").GetValue() as PauseGameConfirmationPopup);

            MethodInfo dynMethod = m_ConfirmationPopup.GetType().GetMethod("ConfirmReturnToMenu", BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(m_ConfirmationPopup, null);

            return false;
        }

    }
    [HarmonyPatch(typeof(PauseMenu), "ReturnToMap")]
    static class PauseMenu_ReturnToMap_Patch
    {
        static bool Prefix(PauseMenu __instance)
        {
            if (!Main.enabled)
            {
                return true;
            }

            __instance.CloseMenu();
            GameModeController.Instance.ReturnToWorldMap();
            return false;
        }

    }
    [HarmonyPatch(typeof(PauseMenu), "RestartLevel")]
    static class PauseMenu_RestartLevel_Patch
    {
        static bool Prefix(PauseMenu __instance)
        {
            if (!Main.enabled)
            {
                return true;
            }

            Map.ClearSuperCheckpointStatus();

            (Traverse.Create(typeof(TriggerManager)).Field("alreadyTriggeredTriggerOnceTriggers").GetValue() as List<string>).Clear();

            if (GameModeController.publishRun)
            {
                GameModeController.publishRun = false;
                LevelEditorGUI.levelEditorActive = true;
            }
            PauseController.SetPause(PauseStatus.UnPaused);
            GameModeController.RestartLevel();

            return false;
        }
    }
}

