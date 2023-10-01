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
        static Harmony harmony;
        internal static ManualLogSource Logger;
        static ConfigFile Config;

        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Ak;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Sk;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Wk;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Dk;
        static ConfigEntry<float> step;

        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> Xk;
        static ConfigEntry<float> scale;

        internal static void Awake(ConfigFile config, ManualLogSource logger)
        {
            Logger = logger;
            Config = config;
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

            step = Config.Bind("GridWindow", "step", 10f,
                new ConfigDescription(
                    "For fine tuning."
                    , new AcceptableValueRange<float>(0f, 1000f)
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
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

        internal static void OnEnable()
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

        internal static void Update()
        {
            if (nTransform)
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
                else if (Xk.Value.IsPressed())// 단축키가 일치할때
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
        static LinkedList<Transform> list = new LinkedList<Transform>();
        internal static Transform nTransform = null;
        internal static Vector3 vTransform = Vector3.zero;
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
