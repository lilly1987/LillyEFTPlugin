using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GClass1711;
using UnityEngine;

namespace LillyEFTPlugin
{
    internal class GridWindowPlugin
    {
        static Harmony harmony=null;
        internal static ManualLogSource Logger;
        static ConfigFile Config;

        static ConfigEntry<bool> isOn;

        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Ak;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Sk;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Wk;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Dk;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> SPk;
        static ConfigEntry<float> head;
        static ConfigEntry<float> step;

        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Xk;
        static ConfigEntry<float> scale;

        static LinkedList<Transform> list = new LinkedList<Transform>();
        internal static Transform nTransform = null;
        internal static Vector3 vTransform = Vector3.zero;

        internal static void init(ConfigFile config, ManualLogSource logger)
        {
            Logger = logger;
            Config = config;
            Main.awake += Awake;
            Main.update += Update;
            Main.onEnable += OnEnable;
            //Main.onDisable += OnDisable;
        }
        internal static void Awake()
        {
            isOn = Config.Bind("GridWindow", "is on",false
                , new ConfigDescription(
                    "GridWindow on"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            isOn.SettingChanged += IsOn_SettingChanged;
            Wk = Config.Bind("GridWindow", "y+ Key", new KeyboardShortcut(KeyCode.W)
                , new ConfigDescription(
                    "move y+"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            Sk = Config.Bind("GridWindow", "y- Key", new KeyboardShortcut(KeyCode.S)
                , new ConfigDescription(
                    "move y-"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            Ak = Config.Bind("GridWindow", "x- Key", new KeyboardShortcut(KeyCode.A)
                , new ConfigDescription(
                    "move x-"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            Dk = Config.Bind("GridWindow", "x+ Key", new KeyboardShortcut(KeyCode.D)
                , new ConfigDescription(
                    "move x+"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            step = Config.Bind("GridWindow", "step", 50f,
                new ConfigDescription(
                    "For fine tuning."
                    , new AcceptableValueRange<float>(0f, 1000f)
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            SPk = Config.Bind("GridWindow", "head Key", new KeyboardShortcut(KeyCode.Space)
                , new ConfigDescription(
                    "move x+"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            head = Config.Bind("GridWindow", "head y fix", 600f,
                new ConfigDescription(
                    "For fine tuning."
                    , new AcceptableValueRange<float>(0f, 2000f)
                    , new ConfigurationManagerAttributes { Order = Main.ordercount--,IsAdvanced=true }
                    )
                );

            Xk = Config.Bind("GridWindow", "scale Key", new KeyboardShortcut(KeyCode.X)
                , new ConfigDescription(
                    "move x+"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            scale = Config.Bind("GridWindow", "scale", 0.75f,
                new ConfigDescription(
                    "For fine tuning."
                    , new AcceptableValueRange<float>(0f, 2f)
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
        }

        private static void IsOn_SettingChanged(object sender, EventArgs ev)
        {
            Logger.LogWarning($"GridWindow IsOn_SettingChanged");
            if (isOn.Value)
            {
                if (harmony==null)
        {
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                harmony = Harmony.CreateAndPatchAll(typeof(GridWindowPlugin));
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
        }
            }
            else
            {
                harmony?.UnpatchSelf();
                nTransform = null;
                vTransform = Vector3.zero;
            }
        }

        internal static void OnEnable()
        {
            Logger.LogWarning($"GridWindow OnEnable");
            //IsOn_SettingChanged(null,null);
        }

        internal static void Update()
        {
            if (nTransform && isOn.Value)
            {
                if (Wk.Value.IsPressed())// 단축키가 일치할때
                {
                    Logger.LogWarning($"GridWindow_OnPointerClick change {nTransform.localPosition.x} {nTransform.localPosition.y}");
                    Logger.LogWarning($"GridWindow_OnPointerClick change {vTransform.x} {vTransform.y}");
                    vTransform.y += step.Value;
                    nTransform.localPosition = vTransform;
                }
                else if (Sk.Value.IsPressed())// 단축키가 일치할때
                {
                    vTransform.y -= step.Value;
                    nTransform.localPosition = vTransform;
                }
                else if (Ak.Value.IsPressed())// 단축키가 일치할때
                {
                    vTransform.x -= step.Value;
                    nTransform.localPosition = vTransform;
                }
                else if (Dk.Value.IsPressed())// 단축키가 일치할때
                {
                    vTransform.x += step.Value;
                    nTransform.localPosition = vTransform;
                }
                else if (SPk.Value.IsUp())// 단축키가 일치할때
                {
                    
                    vTransform.y = - nTransform.RectTransform().sizeDelta.y/2 + head.Value;
                    nTransform.localPosition = vTransform;
                }
                else if (Xk.Value.IsUp())// 단축키가 일치할때
                {
                    if (nTransform.localScale.x == 1)
                    {
                        var v = nTransform.localScale;
                        v.Set(scale.Value, scale.Value, scale.Value);
                        nTransform.localScale = v;
                    }
                    else
                    {
                        nTransform.localScale.Set(1, 1, 1);
                        nTransform.localScale = Vector3.one;
                        Logger.LogWarning($"GridWindow Xk {scale.Value}");
                    }
                }
            }
        }


        // GridWindow.OnPointerClick(PointerEventData)

        //internal static Vector3 vTransformScale = Vector3.one;

        /// fail
        /*
        [HarmonyPatch(typeof(GridWindow), "CorrectPosition")]
        [HarmonyPostfix]
        public static void GridWindow_CorrectPosition(GridWindow __instance)
        {
            Logger.LogWarning($"GridWindow_CorrectPosition");
        }
        */

        [HarmonyPatch(typeof(GridWindow), "Show")]
        [HarmonyPostfix]
        public static void GridWindow_Show(GridWindow __instance)
        {
            Logger.LogWarning($"GridWindow_Show");
            nTransform = __instance.transform;
            vTransform = nTransform.localPosition;
            //vTransformScale = nTransform.localScale;
            list.AddLast(nTransform);
        }

        [HarmonyPatch(typeof(GridWindow), "OnPointerClick")]
        [HarmonyPostfix]
        public static void GridWindow_OnPointerClick(GridWindow __instance)
        {
            Logger.LogWarning($"GridWindow_OnPointerClick");
            if (nTransform != __instance.transform)
            {
                nTransform = __instance.transform;
                vTransform = nTransform.localPosition;
                Logger.LogWarning($"GridWindow_OnPointerClick change {vTransform.x} {vTransform.y}");
                //vTransformScale = nTransform.localScale;
                list.Remove(__instance.transform);
                list.AddLast(__instance.transform);
            }
        }

        [HarmonyPatch(typeof(GridWindow), "Close")]
        [HarmonyPrefix]
        public static void GridWindow_Closek(GridWindow __instance)
        {
            Logger.LogWarning($"GridWindow_Close");
            list.Remove(__instance.transform);
            if (list.Count > 0)
            {
                nTransform = list.Last();
                vTransform = nTransform.localPosition;
                //vTransformScale = nTransform.localScale;
            }
            else
            {
                nTransform = null;
                vTransform = Vector3.zero;
                //vTransformScale = Vector3.one;
            }
        }


    }
}
