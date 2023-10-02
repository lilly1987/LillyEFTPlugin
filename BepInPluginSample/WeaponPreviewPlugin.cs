using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.UI.WeaponModding;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LillyEFTPlugin
{
    internal class WeaponPreviewPlugin
    {
        #region 변수
        static ConfigFile Config;
        internal static ManualLogSource logger;
        static Harmony harmony;
        static bool pluginOn = false;

        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> sk;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> bk;
        static ConfigEntry<float> size;

        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> pr;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> pl;
        static ConfigEntry<float> pos;

        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> rk;
        static ConfigEntry<float> step;

        static bool isOn = false;
        static Vector3 vpos = new Vector3();
        static Vector3 vsize;

        static Transform Rotator;
        #endregion

        internal static void init(ConfigFile Config, ManualLogSource logger)
        {
            WeaponPreviewPlugin.Config = Config;
            WeaponPreviewPlugin.logger = logger;
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                harmony = Harmony.CreateAndPatchAll(typeof(WeaponPreviewPlugin));
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return;
            }
            pluginOn = true;
            Main.awake += Awake;
            //Main.onEnable += OnEnable;
            Main.onDisable += OnDisable;
            Main.update += Update;
        }

        internal static void Awake()
        {
            logger.LogWarning("Awake WeaponPreviewPlugin");

            sk = Config.Bind("WeaponPreview", "scale - Key", new KeyboardShortcut(KeyCode.S),
                new ConfigDescription(
                    ""
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            bk = Config.Bind("WeaponPreview", "scale + Key", new KeyboardShortcut(KeyCode.W),
                new ConfigDescription(
                    ""
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            size = Config.Bind("WeaponPreview", "scale", 1f,
                new ConfigDescription(
                    "."
                    , new AcceptableValueRange<float>(0f, 10f)
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            vsize = Vector3.one * size.Value;
            size.SettingChanged += Size_SettingChanged;


            pr = Config.Bind("WeaponPreview", "pos x - Key", new KeyboardShortcut(KeyCode.A),
                new ConfigDescription(
                    ""
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            pl = Config.Bind("WeaponPreview", "pos x + Key", new KeyboardShortcut(KeyCode.D),
                new ConfigDescription(
                    ""
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            pos = Config.Bind("WeaponPreview", "pos", 0f,
                new ConfigDescription(
                    "."
                    , new AcceptableValueRange<float>(-1000f, 1000f)
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            pos.SettingChanged += Pos_SettingChanged;

            rk = Config.Bind("WeaponPreview", "reset Key", new KeyboardShortcut(KeyCode.X),
                new ConfigDescription(
                    ""
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );

            step = Config.Bind("WeaponPreview", "scale pos step", 0.01f,
                new ConfigDescription(
                    "."
                    , new AcceptableValueRange<float>(0f, 100f)
                    , new ConfigurationManagerAttributes { Order = Main.ordercount--, IsAdvanced = true }
                    )
                );

        }

        private static void rk_SettingChanged()
        {
            if (isOn && Rotator)
            {
                size.Value = (float)size.DefaultValue;
                pos.Value = (float)pos.DefaultValue;
            }
        }

        private static void Pos_SettingChanged(object sender, EventArgs e)
        {
            if (isOn && Rotator)
            {
                vpos.x = pos.Value;
                Rotator.localPosition = vpos;
            }
        }

        private static void Size_SettingChanged(object sender, EventArgs e)
        {
            if (isOn && Rotator)
            {
                vsize.Set(size.Value, size.Value, size.Value);
                Rotator.localScale = vsize;
            }
        }

        internal static void Update()
        {
            if (isOn && Rotator)
            {
                if (bk.Value.IsPressed())
                {
                    size.Value += step.Value;
                }
                if (sk.Value.IsPressed())
                {
                    size.Value -= step.Value;
                }
                if (pr.Value.IsPressed())
                {
                    pos.Value -= step.Value;
                }
                if (pl.Value.IsPressed())
                {
                    pos.Value += step.Value;
                }
                if (rk.Value.IsUp())
                {
                    rk_SettingChanged();
                }
            }
        }

        

        //  EditBuildScreen
        //      WeaponPreview

        /*
        [HarmonyPatch(typeof(WeaponPreview), "Awake")]
        [HarmonyPostfix]
        public static void WeaponPreview_Awake()
        {
            Logger.LogWarning($"WeaponPreview_Awake");
            
        }

        [HarmonyPatch(typeof(WeaponPreview), "Zoom")]
        [HarmonyPostfix]
        public static void WeaponPreview_Zoom(float zoom)
        {
            Logger.LogWarning($"WeaponPreview_Zoom ; {zoom}");

        }
        */
        [HarmonyPatch(typeof(WeaponPreview), "Rotate")]
        [HarmonyPrefix]
        public static void WeaponPreview_Rotate(ref float minTilt, ref float maxTilt)
        {
            minTilt = -90f;
            maxTilt = 90f;
            //Logger.LogWarning($"WeaponPreview_Rotate ; {angle} ; {tilt} ; {minTilt} ; {maxTilt} ;");
        }
        /*
        [HarmonyPatch(typeof(WeaponPreview), "Rotate")]
        [HarmonyPostfix]
        public static void WeaponPreview_Rotate(float angle, float tilt, float minTilt , float maxTilt )
        {
            Logger.LogWarning($"WeaponPreview_Rotate ; {angle} ; {tilt} ; {minTilt} ; {maxTilt} ;");

        }

        [HarmonyPatch(typeof(WeaponPreview), "ResetRotator")]
        [HarmonyPostfix]
        public static void WeaponPreview_ResetRotator(float defaultZ)
        {
            Logger.LogWarning($"WeaponPreview_ResetRotator ; {defaultZ} ; ");

        }
        */
        [HarmonyPatch(typeof(WeaponPreview), "Init")]
        [HarmonyPostfix]
        public static void WeaponPreview_Init(WeaponPreview __instance)
        {
            logger.LogWarning($"WeaponPreview_Init");
            Rotator = __instance.Rotator;
            isOn = true;
            Size_SettingChanged(null, null);
            Pos_SettingChanged(null, null);
        }
        /*
        [HarmonyPatch(typeof(WeaponPreview), "OnDisable")]
        [HarmonyPostfix]
        public static void WeaponPreview_OnDisable()
        {
            Logger.LogWarning($"WeaponPreview_OnDisable");

        }
        */
        [HarmonyPatch(typeof(WeaponPreview), "Hide")]
        [HarmonyPostfix]
        public static void WeaponPreview_Hide()
        {
            isOn = false;
            logger.LogWarning($"WeaponPreview_Hide");

        }
        /*
        [HarmonyPatch(typeof(WeaponPreview), "OnDestroy")]
        [HarmonyPostfix]
        public static void WeaponPreview_OnDestroy()
        {
            Logger.LogWarning($"WeaponPreview_OnDestroy");

        }
        internal static void OnEnable()
        {
            logger.LogWarning("OnEnable WeaponPreviewPlugin");

        }
        */

        internal static void OnDisable()
        {
            harmony?.UnpatchSelf();
        }


    }
}
