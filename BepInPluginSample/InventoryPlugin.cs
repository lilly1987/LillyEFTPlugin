using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EFT.UI.InventoryScreen;

namespace LillyEFTPlugin
{
    internal class InventoryPlugin
    {
        #region 변수
        static ManualLogSource logger;
        static Harmony harmony;
        static ConfigFile Config;
        static bool pluginOn = false;

        static bool InventoryScreenShow = false;

        static ConfigEntry<bool> SlotOn;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> SlotPanelSizeKey;
        static ConfigEntry<float> SlotPanelSize;

        static ConfigEntry<bool> SlotPanel;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> SlotPanelKey;

        static ConfigEntry<bool> LeftPanel;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> LeftPanelKey;
        static ConfigEntry<float> LeftPanelX;

        static ConfigEntry<bool> StashPanel;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> StashPanelKey;
        static ConfigEntry<float> StashPanelX;

        static InventoryScreen inventoryScreen = null;
        static GameObject oinventoryScreen;
        static Transform tItemsPanel = null;
        static Transform tLeftPanel = null;
        static Transform tGearPanel = null;
        static Transform tContainersPanel = null;
        static Transform tStashPanel = null;
        static Transform tContent = null;
        static Transform tScrollviewParent = null;
        static Transform tContainersScrollview = null;
        static RectTransform rContainersPanel = null;
        static RectTransform rStashPanel = null;
        static RectTransform rLeftPanel = null;
        static RectTransform rGearPanel = null;
        static RectTransform rScrollviewParent = null;
        static RectTransform rContainersScrollview = null;
        //static RectTransform lGearPanel = null;
        static Vector2 vContainersPaneloffsetMin;
        static Vector2 vContainersPaneloffsetMax;
        static Vector2 vContainersPanelXl;
        static Vector2 vContainersPanelXs;
        static Vector2 vStashPanel;
        static Vector2 vStashPaneloffsetMin;
        static Vector2 vStashPaneloffsetMax;
        static Vector2 vStashPanelXMin;
        static Vector2 vStashPanelXMax;
        static Transform TacticalVest = null;
        static Transform Backpack = null;
        static Transform SecuredContainer = null;
        static Vector2 vContainersScrollview = new Vector2();

        #endregion

        #region Harmony
        [HarmonyPatch(typeof(InventoryScreen), "Awake")]
        [HarmonyPostfix]
        public static void InventoryScreen_Awake(InventoryScreen __instance)
        {

            logger.LogWarning($"InventoryScreen_Awake ");
            inventoryScreen = __instance;
            oinventoryScreen = inventoryScreen.gameObject;


            tItemsPanel = inventoryScreen.transform.Find("Items Panel");

            tContainersPanel = tItemsPanel.transform.Find("Containers Panel");
            rContainersPanel = (RectTransform)tContainersPanel;
            vContainersPaneloffsetMin = rContainersPanel.offsetMin;
            vContainersPaneloffsetMax = rContainersPanel.offsetMax;

            tScrollviewParent = tContainersPanel.transform.Find("Scrollview Parent");
            rScrollviewParent = (RectTransform)tScrollviewParent;

            tContainersScrollview = tScrollviewParent.transform.Find("Containers Scrollview");
            rContainersScrollview = (RectTransform)tContainersScrollview;
            rContainersScrollview.anchorMin = Vector2.zero;

            rContainersScrollview.anchoredPosition = vContainersScrollview;
            rContainersScrollview.offsetMin = vContainersScrollview;

            tContent = tContainersScrollview.transform.Find("Content");

            tStashPanel = tItemsPanel.transform.Find("Stash Panel");
            rStashPanel = (RectTransform)tStashPanel;
            vStashPanel = rStashPanel.sizeDelta;
            vStashPaneloffsetMin = rStashPanel.offsetMin;
            vStashPaneloffsetMax = rStashPanel.offsetMax;
            tLeftPanel = tItemsPanel.transform.Find("Left Panel");
            rLeftPanel = (RectTransform)tLeftPanel;

            tGearPanel = tLeftPanel.transform.Find("Gear Panel");
            rGearPanel = (RectTransform)tGearPanel;
            //            lGearPanel = (RectTransform)tGearPanel;

            vContainersPanelXl = new Vector2(vContainersPaneloffsetMin.x - rLeftPanel.rect.size.x * LeftPanelX.Value, vContainersPaneloffsetMin.y);

            vContainersPanelXs = new Vector2(vContainersPaneloffsetMax.x + rStashPanel.rect.size.x * StashPanelX.Value, vContainersPaneloffsetMax.y);
            vStashPanelXMin = new Vector2(vStashPaneloffsetMin.x + rStashPanel.rect.size.x * StashPanelX.Value, vStashPaneloffsetMin.y);
            vStashPanelXMax = new Vector2(vStashPaneloffsetMax.x + rStashPanel.rect.size.x * StashPanelX.Value, vStashPaneloffsetMax.y);

            LeftPanel_SettingChanged(null, null);
            StashPanel_SettingChanged(null, null);
        }

        [HarmonyPatch(typeof(InventoryScreen), "Show", new Type[] { typeof(GClass2984) })]
        [HarmonyPostfix]
        public static void InventoryScreen_Show()
        {
            logger.LogWarning($"InventoryScreen_Show ");
            InventoryScreenShow = true;
        }

        [HarmonyPatch(typeof(InventoryScreen), "Close")]
        [HarmonyPostfix]
        public static void InventoryScreen_Close()
        {
            logger.LogWarning($"InventoryScreen_Close");
            InventoryScreenShow = false;
        }

        [HarmonyPatch(typeof(ContainersPanel), "Show")]
        [HarmonyPostfix]
        public static void ContainersPanel_Show()
        {
            logger.LogWarning($"ContainersPanel_Show ");
            size_SettingChanged(null, null);
            slot_SettingChanged(null, null);
            LeftPanel_SettingChanged(null, null);
            StashPanel_SettingChanged(null, null);
        }

        #endregion

        #region size_SettingChanged
        public static void size_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"size_SettingChanged");
            if (oinventoryScreen && InventoryScreenShow)
                if (SlotOn.Value && oinventoryScreen.activeSelf)
                {
                    tContent.localScale = Vector3.one * SlotPanelSize.Value;
                }
                else
                    tContent.localScale = Vector3.one;
        }
        #endregion

        #region slot_SettingChanged
        public static void slot_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"slot_SettingChanged");
            if (oinventoryScreen && InventoryScreenShow)
                if (InventoryScreenShow && tContent)
                {
                    if (!TacticalVest) TacticalVest = tContent.transform.Find("TacticalVest Slot/Slot Panel");
                    if (!Backpack) Backpack = tContent.transform.Find("Backpack Slot/Slot Panel");
                    if (!SecuredContainer) SecuredContainer = tContent.transform.Find("SecuredContainer Slot/Slot Panel");
                    TacticalVest.gameObject.SetActive(SlotPanel.Value);
                    Backpack.gameObject.SetActive(SlotPanel.Value);
                    SecuredContainer.gameObject.SetActive(SlotPanel.Value);
                }
        }
        #endregion

        #region Wide_SettingChanged
        public static void LeftPanel_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"Wide_SettingChanged");
            if (oinventoryScreen && InventoryScreenShow)
                if (LeftPanel.Value)
                {
                    rContainersPanel.offsetMin = vContainersPanelXl;
                }
                else
                {
                    rContainersPanel.offsetMin = vContainersPaneloffsetMin;
                }
        }
        #endregion

        #region Wide_SettingChanged
        private static void StashPanel_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"StashPanel_SettingChanged");
            //tStashPanel.gameObject.SetActive(!StashPanel.Value);
            //if (oinventoryScreen.activeSelf)
            if (oinventoryScreen && InventoryScreenShow)
                if (StashPanel.Value)
                {
                    rContainersPanel.offsetMax = vContainersPanelXs; ;
                    rStashPanel.offsetMin = vStashPanelXMin;
                    rStashPanel.offsetMax = vStashPanelXMax;
                }
                else
                {
                    rContainersPanel.offsetMax = vContainersPaneloffsetMax;
                    rStashPanel.offsetMin = vStashPaneloffsetMin;
                    rStashPanel.offsetMax = vStashPaneloffsetMax;
                }
        }
        #endregion

        internal static void init(ConfigFile config, ManualLogSource logger)
        {
            InventoryPlugin.logger = logger;
            Config = config;
            logger.LogWarning("Awake");
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                harmony = Harmony.CreateAndPatchAll(typeof(InventoryPlugin));
                pluginOn = true;
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
                return;
            }
            pluginOn = true;
            //Main.onEnable+= OnEnable;
            Main.awake += Awake;
            Main.update += Update;
            Main.onDisable += OnDisable;
        }

        internal static void Awake() { 
            // =========================================================
            SlotOn = Config.Bind("Inventory", "Slot Panel Zoom On", true,
                new ConfigDescription(
                    "Change center panel scale"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            SlotPanelSizeKey = Config.Bind("Inventory", "Slot Panel Zoom Key", new KeyboardShortcut(KeyCode.W)
                , new ConfigDescription(
                    "Change center panel scale"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );// 이건 단축키
            SlotPanelSize = Config.Bind("Inventory", "Slot Panel Zoom scale", 0.75f,
                new ConfigDescription(
                    "Zoom in or out on the center panel"
                    , new AcceptableValueRange<float>(0f, 2f)
                    , new ConfigurationManagerAttributes { IsAdvanced = true, Order = Main.ordercount-- }
                    )
                );

            SlotPanel = Config.Bind("Inventory", "Slot Panel icon On", true,
                new ConfigDescription(
                    "Hiding the mounting slots in the center panel"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            SlotPanelKey = Config.Bind("Inventory", "Slot Panel icon Key", new KeyboardShortcut(KeyCode.S),
                new ConfigDescription(
                    "Hiding the mounting slots in the center panel"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );// 이건 단축키

            //
            LeftPanel = Config.Bind("Inventory", "Left extension On", true,
                new ConfigDescription(
                    "Extend the center panel to the left"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            LeftPanelKey = Config.Bind("Inventory", "Left extension Key", new KeyboardShortcut(KeyCode.A),
                new ConfigDescription(
                    "Extend the center panel to the left"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );// 이건 단축키
            LeftPanelX = Config.Bind("Inventory", "Left extension Size", 1.05f,
                new ConfigDescription(
                    "For fine tuning."
                    , new AcceptableValueRange<float>(0f, 2f)
                    , new ConfigurationManagerAttributes { IsAdvanced = true, Order = Main.ordercount-- }
                    )
                );

            //
            StashPanel = Config.Bind("Inventory", "Right extension On", true,
                new ConfigDescription(
                    "Extend the center panel to the right"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );
            StashPanelKey = Config.Bind("Inventory", "Right extension Key", new KeyboardShortcut(KeyCode.D),
                new ConfigDescription(
                    "Extend the center panel to the right"
                    , null
                    , new ConfigurationManagerAttributes { Order = Main.ordercount-- }
                    )
                );// 이건 단축키
            StashPanelX = Config.Bind("Inventory", "Right extension Size", 1.025f,
                new ConfigDescription(
                    "For fine tuning."
                    , new AcceptableValueRange<float>(0f, 2f)
                    , new ConfigurationManagerAttributes { Order = Main.ordercount--, IsAdvanced = true }
                    )
                );

            vContainersScrollview.x = 10;


            SlotPanelSize.SettingChanged += size_SettingChanged;
            SlotOn.SettingChanged += size_SettingChanged;
            SlotPanelSizeKey.SettingChanged += size_SettingChanged;

            SlotPanel.SettingChanged += slot_SettingChanged;
            LeftPanelX.SettingChanged += slot_SettingChanged;

            LeftPanel.SettingChanged += LeftPanel_SettingChanged;

            StashPanel.SettingChanged += StashPanel_SettingChanged;
        }
        /*
        internal static void OnEnable()
        {
            logger.LogWarning("OnEnable InventoryPlugin");
            // 하모니 패치
        }
        */
        internal static void Update()
        {
            if (InventoryScreenShow && GridWindowPlugin.nTransform == null)
            {
                if (SlotPanelKey.Value.IsUp())// 단축키가 일치할때
                {
                    SlotPanel.Value = !SlotPanel.Value;
                }
                else if (SlotPanelSizeKey.Value.IsUp())// 단축키가 일치할때
                {
                    SlotOn.Value = !SlotOn.Value;
                }
                else if (LeftPanelKey.Value.IsUp())// 단축키가 일치할때
                {
                    LeftPanel.Value = !LeftPanel.Value;
                }
                else if (StashPanelKey.Value.IsUp())// 단축키가 일치할때
                {
                    StashPanel.Value = !StashPanel.Value;
                }
            }
        }

        internal static void OnDisable()
        {
            harmony?.UnpatchSelf();
        }


    }
}
