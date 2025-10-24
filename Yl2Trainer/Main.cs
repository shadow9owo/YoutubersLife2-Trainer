using HarmonyLib;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;
using UplayOnline.UI;
using System.Reflection;

namespace Yl2Trainer
{
    public static class BuildInfo
    {
        public const string Name = "sh3ds mono trainer";
        public const string Description = "a youtubers life 2 trainer";
        public const string Author = "sh3d";
        public const string Company = null;
        public const string Version = "1.0.0";
        public const string DownloadLink = null;
    }

    public class Yl2Trainer : MelonMod
    {
        [HarmonyPatch(typeof(UILoading), "AddProgress")]
        internal static class Patch_UILoading_AddProgress
        {
            static bool Prefix(UILoading __instance, ref int offset)
            {
                var progressField = typeof(UILoading).GetField("_progressValue",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var maxProgressField = typeof(UILoading).GetField("_maxProgressValue",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var setSlider = typeof(UILoading).GetMethod("SetSliderValue",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (Yl2Trainer.skiparbitraryload)
                {
                    progressField.SetValue(__instance, 100);
                    float maxProg = (float)maxProgressField.GetValue(__instance);
                    setSlider.Invoke(__instance, new object[] { 1f * maxProg, null });

                    return false;
                }

                if ((bool)typeof(UILoading)
                        .GetField("_activeDebug", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        .GetValue(__instance))
                {
                    Debug.Log($"UILoading.AddProgress( offset:{offset} )");
                }

                int progress = (int)progressField.GetValue(__instance);
                progress += offset;
                progressField.SetValue(__instance, progress);

                float maxValue = (float)maxProgressField.GetValue(__instance);
                float value = ((float)progress - 1f) / (float)progress * maxValue;
                setSlider.Invoke(__instance, new object[] { value, null });

                return false;
            }
        }

        [HarmonyPatch(typeof(TimeManager), nameof(TimeManager.TimeIsPaused))]
        internal static class Patch_TimeManager_TimeIsPaused
        {
            static void Postfix(ref bool __result)
            {
                if (Yl2Trainer.frozen_ingame_clock)
                {
                    __result = true;
                }
                else
                {
                    __result = false;
                }
            }
        }


        public enum TypeOfValue
        {
            Money,
            skill_points,
            Tickets,
            Subscribers,
            Timescale,
            Freeze_InGame_clock,
            Freeze_Energy,
            windowed,
            winx,
            winy,
            skipload
        }

        private GUIStyle windowStyle;

        private (int, int, bool) screen;

        private Rect windowRect = new Rect(0, 0, 512, 512);
        private Vector2 scrollPos = Vector2.zero;
        private Dictionary<TypeOfValue, string> pairs = new Dictionary<TypeOfValue, string>();

        public static bool frozen_energy,frozen_ingame_clock, skiparbitraryload = false;

        private void SavePrefs()
        {
            foreach (var pair in pairs)
            {
                PlayerPrefs.SetString(pair.Key.ToString(), pair.Value);
            }

            PlayerPrefs.Save();
        }

        private string IntField(string value, params GUILayoutOption[] options)
        {
            if (value == null)
                value = "";

            string raw = GUILayout.TextField(value, options);

            string filtered = "";
            foreach (char c in raw)
                if (char.IsDigit(c))
                    filtered += c;

            if (string.IsNullOrEmpty(filtered))
                filtered = "0";

            return filtered;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public override void OnLateUpdate()
        {
            var def = ManagerSingleton<DataManager>.get;

            if (frozen_energy)
            {
                
                if (def != null)
                {
                    def.energy.ModifyEnergy((float)ManagerSingleton<DataManager>.get.energy.MaxEnergy, false);
                }
            }

            if (Screen.currentResolution.width != screen.Item1 || Screen.currentResolution.height != screen.Item2 || Screen.fullScreen != screen.Item3)
            {
                Screen.SetResolution(screen.Item1, screen.Item2, !screen.Item3);
            }
        }

        bool ValueChanged(string Input, TypeOfValue typeOfValue)
        {
            return (pairs[typeOfValue] != Input);
        }

        void WindowFunction(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, windowRect.width, 40));

            scrollPos = GUI.BeginScrollView(
                new Rect(10, 50, windowRect.width - 20, windowRect.height - 60),
                scrollPos,
                new Rect(0, 0, windowRect.width - 40, 1000)
            );

            GUILayout.BeginVertical();


            GUIStyle Title = new GUIStyle(GUI.skin.label);
            Title.fontSize = 36;
            Title.fontStyle = FontStyle.Bold;
            Title.alignment = TextAnchor.MiddleCenter;
            Title.normal.textColor = new Color(1f, 1f, 1f); // soft blue glow vibe

            GUILayout.Label("Sh3ds Mono Trainer", Title);
            GUILayout.Space(5);
            GUILayout.Label("for youtubers life 2");

            GUIStyle SubTitle = new GUIStyle(GUI.skin.label);
            SubTitle.fontSize = 24;

            //money

            var a = ManagerSingleton<DataManager>.get;

            GUILayout.Label("Money", SubTitle);
            pairs[TypeOfValue.Money] = IntField(pairs[TypeOfValue.Money], GUILayout.Width(100));

            if (GUILayout.Button("Add", GUILayout.Width(100)))
            {
                if (int.TryParse(pairs[TypeOfValue.Money], out int parsedValue))
                {
                    if (a != null)
                    {
                        a.money.AddMoney((float)parsedValue);
                    }
                }
            }
            if (GUILayout.Button("Deduct", GUILayout.Width(100)))
            {
                if (int.TryParse(pairs[TypeOfValue.Money], out int parsedValue))
                {
                    if (a != null)
                    {
                        a.money.AddMoney((float)parsedValue);
                    }
                }
            }

            //skill points

            GUILayout.Label("Skill Points", SubTitle);

            pairs[TypeOfValue.skill_points] = IntField(pairs[TypeOfValue.skill_points], GUILayout.Width(100));

            if (GUILayout.Button("Add", GUILayout.Width(100)))
            {
                if (int.TryParse(pairs[TypeOfValue.skill_points], out int parsedValue))
                {
                    if (a != null)
                    {
                        a.saveGame.current_talent_points = a.saveGame.current_talent_points + parsedValue;
                    }
                }
            }

            //tickets

            GUILayout.Label("Tickets", SubTitle);

            pairs[TypeOfValue.Tickets] = IntField(pairs[TypeOfValue.Tickets], GUILayout.Width(100));

            if (GUILayout.Button("Add", GUILayout.Width(100)))
            {
                if (int.TryParse(pairs[TypeOfValue.Tickets], out int parsedValue))
                {
                    if (a != null)
                    {
                        a.tubiTicketsManager.AddTubiTickets((int)parsedValue);
                    }
                }
            }
            if (GUILayout.Button("Deduct", GUILayout.Width(100)))
            {
                if (int.TryParse(pairs[TypeOfValue.Tickets], out int parsedValue))
                {
                    if (a != null)
                    {
                        a.tubiTicketsManager.AddTubiTickets((int)-parsedValue);
                    }
                }
            }

            //subscribers

            GUILayout.Label("Subscribers", SubTitle);

            pairs[TypeOfValue.Subscribers] = IntField(pairs[TypeOfValue.Subscribers], GUILayout.Width(100));

            if (GUILayout.Button("Add", GUILayout.Width(100)))
            {
                if (int.TryParse(pairs[TypeOfValue.Subscribers], out int parsedValue))
                {
                    if (a != null)
                    {
                        a.utubeChannel.followers = a.utubeChannel.followers + parsedValue;
                    }
                }
            }
            if (GUILayout.Button("Deduct", GUILayout.Width(100)))
            {
                if (int.TryParse(pairs[TypeOfValue.Subscribers], out int parsedValue))
                {
                    if (a != null)
                    {
                        a.utubeChannel.followers = a.utubeChannel.followers - parsedValue;
                    }
                }
            }

            //timescale

            GUILayout.Label("Timescale", SubTitle);

            float.TryParse(pairs[TypeOfValue.Timescale], out float timescalef); //fails returns 0 instead

            timescalef = GUILayout.HorizontalSlider(timescalef, 0.0f, 10.0f, GUILayout.Width(100));

            Time.timeScale = timescalef;

            if (GUILayout.Button("Reset", GUILayout.Width(100)))
            {
                timescalef = 1.0f;
            }

            pairs[TypeOfValue.Timescale] = timescalef.ToString(); // shit

            GUILayout.Label(pairs[TypeOfValue.Timescale]);

            //freeze in game time

            GUILayout.Label("Freeze InGame clock", SubTitle);

            bool.TryParse(pairs[TypeOfValue.Freeze_InGame_clock], out bool fclockb);

            fclockb = GUILayout.Toggle(fclockb, "Freeze InGame clock");

            if (ValueChanged(fclockb.ToString(), TypeOfValue.Freeze_InGame_clock))
            {
                frozen_ingame_clock = fclockb;
            }

            pairs[TypeOfValue.Freeze_InGame_clock] = fclockb.ToString();

            //infinite energy

            GUILayout.Label("Freeze Energy", SubTitle);

            bool.TryParse(pairs[TypeOfValue.Freeze_Energy], out bool fenergyb);

            fenergyb = GUILayout.Toggle(fenergyb, "Freeze Energy");

            if (ValueChanged(fenergyb.ToString(), TypeOfValue.Freeze_Energy))
            {
                // changed do shit
                frozen_energy = fenergyb;
            }

            pairs[TypeOfValue.Freeze_Energy] = fenergyb.ToString();

            // skip fake loading screens

            GUILayout.Label("Skip arbitrary loading screens", SubTitle);

            bool.TryParse(pairs[TypeOfValue.skipload], out bool skiploadb);

            skiploadb = GUILayout.Toggle(skiploadb, "Skip arbitrary loading screens");

            if (ValueChanged(skiploadb.ToString(), TypeOfValue.skipload))
            {
                skiparbitraryload = skiploadb;
                MelonLogger.Msg("skiparbitraryloadingscreens (value) : " + skiploadb);
            }

            pairs[TypeOfValue.skipload] = skiploadb.ToString();

            //proper settings

            GUILayout.Label("Settings", SubTitle);

            bool.TryParse(pairs[TypeOfValue.windowed], out bool windowedb);

            windowedb = GUILayout.Toggle(windowedb, "windowed mode");

            pairs[TypeOfValue.windowed] = windowedb.ToString();

            pairs[TypeOfValue.winx] = IntField(pairs[TypeOfValue.winx], GUILayout.Width(100));
            pairs[TypeOfValue.winy] = IntField(pairs[TypeOfValue.winy], GUILayout.Width(100));

            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            {
                SavePrefs();
                if (int.TryParse(pairs[TypeOfValue.winx], out int winxp)) //nice
                {
                    if (int.TryParse(pairs[TypeOfValue.winy], out int winyp))
                    {
                        Screen.SetResolution(winxp, winyp, !windowedb); //windowedb doesnt apply
                        screen.Item1 = winxp;
                        screen.Item2 = winyp;
                        screen.Item3 = windowedb;
                    }
                }
            }

            GUILayout.EndVertical();

            GUI.EndScrollView();
        }

        private void LoadPrefs()
        {
            foreach (TypeOfValue type in System.Enum.GetValues(typeof(TypeOfValue)))
            {
                if (PlayerPrefs.HasKey(type.ToString()))
                    pairs[type] = PlayerPrefs.GetString(type.ToString());
            }
        }

        public override void OnInitializeMelon() {
            new HarmonyLib.Harmony("yl2trainer.patches").PatchAll();

            foreach (TypeOfValue type in System.Enum.GetValues(typeof(TypeOfValue)))
            {
                pairs[type] = "0";
            }

            pairs[TypeOfValue.Timescale] = "1.0";
            pairs[TypeOfValue.Freeze_InGame_clock] = "false";
            pairs[TypeOfValue.Freeze_Energy] = "false";
            pairs[TypeOfValue.windowed] = "false";
            pairs[TypeOfValue.winx] = Screen.currentResolution.width.ToString();
            pairs[TypeOfValue.winy] = Screen.currentResolution.height.ToString();

            LoadPrefs();

            bool.TryParse(pairs[TypeOfValue.Freeze_InGame_clock], out bool fclockb);
            bool.TryParse(pairs[TypeOfValue.Freeze_Energy], out bool fenergyb);
            bool.TryParse(pairs[TypeOfValue.skipload], out bool skiploadb);
            bool.TryParse(pairs[TypeOfValue.windowed], out bool windowedb);

            frozen_ingame_clock = fclockb;
            frozen_energy = fenergyb;
            skiparbitraryload = skiploadb;

            if (int.TryParse(pairs[TypeOfValue.winx], out int winxp))
            {
                if (int.TryParse(pairs[TypeOfValue.winy], out int winyp))
                {
                    Screen.SetResolution(winxp, winyp, !windowedb);
                    screen.Item1 = winxp;
                    screen.Item2 = winyp;
                    screen.Item3 = windowedb;
                }
            }
        }

        public override void OnGUI()
        {
            windowStyle = new GUIStyle(GUI.skin.window);
            Texture2D bgTex = MakeTex(1, 1, new Color(0.08f, 0.08f, 0.08f, 0.95f));
            windowStyle.normal.background = bgTex;
            windowStyle.border = new RectOffset(10, 10, 10, 10);
            windowStyle.padding = new RectOffset(8, 8, 8, 8);
            windowStyle.normal.background = bgTex;
            windowStyle.onNormal.background = bgTex;
            windowStyle.focused.background = bgTex;
            windowStyle.onFocused.background = bgTex;
            windowStyle.active.background = bgTex;
            windowStyle.onActive.background = bgTex;
            windowStyle.border = new RectOffset(8, 8, 8, 8);
            windowStyle.padding = new RectOffset(10, 10, 10, 10);

            windowRect = GUI.Window(0, windowRect, WindowFunction, "", windowStyle);
        }

        public override void OnApplicationQuit()
        {
            if (ManagerSingleton<DataManager>.get != null)
            {
                ManagerSingleton<DataManager>.get.saveGame.cheated = false;
            }
            SavePrefs();
        }
    }
}