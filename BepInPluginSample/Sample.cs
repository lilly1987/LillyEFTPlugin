using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static EFT.UI.InventoryScreen;
using KeyboardShortcut = BepInEx.Configuration.KeyboardShortcut;

namespace BepInPluginSample
{
    [BepInPlugin("Game.Lilly.Plugin", "Lilly", "1.3.4")]
    public class Sample : BaseUnityPlugin
    {
        #region 변수
        // =========================================================
        static Sample my;
        static Harmony harmony;
        static ManualLogSource logger;


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
        // =========================================================
        #endregion

        public void Awake()
        {
            #region GUI
            logger = Logger;
            Logger.LogMessage("Awake");
            my = this;
            #endregion

            #region 변수 설정
            // =========================================================
            SlotOn = Config.Bind("Inventory", "Slot Panel Zoom On", true,
                new ConfigDescription(
                    "Zoom in or out on the center panel"
                    //,new AcceptableValueRange<float>(0f, 2f)
                    //, new ConfigurationManagerAttributes { IsAdvanced = true }
                    )
                );
            SlotPanelSizeKey = Config.Bind("Inventory", "Slot Panel Zoom Key", new KeyboardShortcut(KeyCode.W));// 이건 단축키
            SlotPanelSize = Config.Bind("Inventory", "Slot Panel Zoom scale", 0.75f, 
                new ConfigDescription(
                    "Zoom in or out on the center panel"
                    , new AcceptableValueRange<float>(0f, 2f)
                    , new ConfigurationManagerAttributes { IsAdvanced = true }
                    )
                );

            SlotPanel = Config.Bind("Inventory", "Slot Panel icon On", true,
                new ConfigDescription(
                    "Zoom in or out on the center panel"
                    //,new AcceptableValueRange<float>(0f, 2f)
                    //, new ConfigurationManagerAttributes { IsAdvanced = true }
                    )
                );
            SlotPanelKey = Config.Bind("Inventory", "Slot Panel icon Key", new KeyboardShortcut(KeyCode.S));// 이건 단축키

            LeftPanel = Config.Bind("Inventory", "Left extension On", true,
                new ConfigDescription(
                    "Expand the middle panel"
                    //,new AcceptableValueRange<float>(0f, 2f)
                    //, new ConfigurationManagerAttributes { IsAdvanced = true }
                    )
                );
            LeftPanelKey = Config.Bind("Inventory", "Left extension Key", new KeyboardShortcut(KeyCode.A));// 이건 단축키
            LeftPanelX = Config.Bind("Inventory", "Left extension Size", 1.05f,
                new ConfigDescription(
                    "For fine tuning."
                    , new AcceptableValueRange<float>(0f, 2f)
                    , new ConfigurationManagerAttributes { IsAdvanced = true }
                    )
                );

            StashPanel = Config.Bind("Inventory", "Right extension On", true,
                new ConfigDescription(
                    "Expand the middle panel"
                    //,new AcceptableValueRange<float>(0f, 2f)
                    //, new ConfigurationManagerAttributes { IsAdvanced = true }
                    )
                );
            StashPanelKey = Config.Bind("Inventory", "Right extension Key", new KeyboardShortcut(KeyCode.D));// 이건 단축키
            StashPanelX = Config.Bind("Inventory", "Right extension Size", 1.025f,
                new ConfigDescription(
                    "For fine tuning."
                    , new AcceptableValueRange<float>(0f, 2f)
                    , new ConfigurationManagerAttributes { IsAdvanced = true }
                    )
                );
            // =========================================================
            #endregion
        }

        public void OnEnable()
        {
            Logger.LogWarning("OnEnable");
            // 하모니 패치
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                harmony = Harmony.CreateAndPatchAll(typeof(Sample));
            }
            catch (Exception e)
            {
                Logger.LogError("harmony");
                Logger.LogError(e.ToString());
            }

            SlotPanelSize.SettingChanged += size_SettingChanged;
            SlotOn.SettingChanged += size_SettingChanged;
            SlotPanelSizeKey.SettingChanged += size_SettingChanged;

            SlotPanel.SettingChanged += slot_SettingChanged;
            LeftPanelX.SettingChanged += slot_SettingChanged;

            LeftPanel.SettingChanged += LeftPanel_SettingChanged;

            StashPanel.SettingChanged += StashPanel_SettingChanged;

        }

        public void Update()
        {
            #region GUI
            if (InventoryScreenShow)
            {
                if (SlotPanelKey.Value.IsUp())// 단축키가 일치할때
                {
                    SlotPanel.Value = !SlotPanel.Value;
                }
                if (SlotPanelSizeKey.Value.IsUp())// 단축키가 일치할때
                {
                    SlotOn.Value = !SlotOn.Value;
                }
                if (LeftPanelKey.Value.IsUp())// 단축키가 일치할때
                {
                    LeftPanel.Value = !LeftPanel.Value;
                }
                if (StashPanelKey.Value.IsUp())// 단축키가 일치할때
                {
                    StashPanel.Value = !StashPanel.Value;
                }
            }
            #endregion
        }

        public void OnDisable()
        {
            Logger.LogWarning("OnDisable");
            harmony?.UnpatchSelf();
            SlotPanelSize.SettingChanged -= size_SettingChanged;
            SlotOn.SettingChanged -= size_SettingChanged;
            SlotPanelSizeKey.SettingChanged -= size_SettingChanged;
            SlotPanel.SettingChanged -= slot_SettingChanged;
            LeftPanelX.SettingChanged -= slot_SettingChanged;
            LeftPanel.SettingChanged -= LeftPanel_SettingChanged;
            StashPanel.SettingChanged -= StashPanel_SettingChanged;

        }



        #region size_SettingChanged
        public void size_SettingChanged(object sender, EventArgs e)
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
        public void slot_SettingChanged(object sender, EventArgs e)
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
        public void LeftPanel_SettingChanged(object sender, EventArgs e)
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

        private void StashPanel_SettingChanged(object sender, EventArgs e)
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

        #region Harmony

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
            Vector2 v = new Vector2();
            v.x = 10;
            rContainersScrollview.anchoredPosition = v;
            rContainersScrollview.offsetMin = v;

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

            my.LeftPanel_SettingChanged(null, null);
            my.StashPanel_SettingChanged(null, null);
        }
        /*
        [HarmonyPatch(typeof(InventoryScreen), "TranslateCommand")]
        [HarmonyPostfix]
        public static void InventoryScreen_TranslateCommand()
        {
            logger.LogWarning($"InventoryScreen_TranslateCommand");    
        }
        */
        [HarmonyPatch(typeof(InventoryScreen), "Show" , new Type[] { typeof(GClass2984) })]
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
            my.size_SettingChanged(null, null);
            my.slot_SettingChanged(null, null);
            my.LeftPanel_SettingChanged(null, null);
            my.StashPanel_SettingChanged(null, null);
        }

        [HarmonyPatch(typeof(ContainersPanel), "Close")]
        [HarmonyPostfix]
        public static void ContainersPanel_Close()
        {
            logger.LogWarning($"ContainersPanel_Close");
            InventoryScreenShow = false;
        }

        [HarmonyPatch(typeof(SimpleStashPanel), "Show")]
        [HarmonyPostfix]
        public static void SimpleStashPanel_Show()
        {
            logger.LogWarning($"SimpleStashPanel_Show");

        }

        [HarmonyPatch(typeof(SimpleStashPanel), "Close")]
        [HarmonyPostfix]
        public static void SimpleStashPanel_Close()
        {
            logger.LogWarning($"SimpleStashPanel_Close");
        }

        // 적 npc  , 보험
        [HarmonyPatch(typeof(ComplexStashPanel), "Show")]
        [HarmonyPostfix]
        public static void ComplexStashPanel_Show()
        {
            logger.LogWarning($"ComplexStashPanel_Show");
        }

        // 적 npc , 보험
        [HarmonyPatch(typeof(ComplexStashPanel), "Close")]
        [HarmonyPostfix]
        public static void ComplexStashPanel_Close()
        {
            logger.LogWarning($"ComplexStashPanel_Close");
            
        }



        // ====================== 하모니 패치 샘플 ===================================
        /*

        [HarmonyPatch(typeof(XPPicker), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void XPPickerCtor(XPPicker __instance, ref float ___pickupRadius)
        {
            //logger.LogWarning($"XPPicker.ctor {___pickupRadius}");
            ___pickupRadius = pickupRadius.Value;
        }

        [HarmonyPatch(typeof(AEnemy), "DamageMult", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SetDamageMult(ref float __0)
        {
            if (!eMultOn.Value)
            {
                return;
            }
            __0 *= eDamageMult.Value;
        }
        */
        // =========================================================
        #endregion
    }
    internal sealed class ConfigurationManagerAttributes
    {
        /// <summary>
        /// Should the setting be shown as a percentage (only use with value range settings).
        /// </summary>
        public bool? ShowRangeAsPercent;

        /// <summary>
        /// Custom setting editor (OnGUI code that replaces the default editor provided by ConfigurationManager).
        /// See below for a deeper explanation. Using a custom drawer will cause many of the other fields to do nothing.
        /// </summary>
        public System.Action<BepInEx.Configuration.ConfigEntryBase> CustomDrawer;

        /// <summary>
        /// Custom setting editor that allows polling keyboard input with the Input (or UnityInput) class.
        /// Use either CustomDrawer or CustomHotkeyDrawer, using both at the same time leads to undefined behaviour.
        /// </summary>
        public CustomHotkeyDrawerFunc CustomHotkeyDrawer;

        /// <summary>
        /// Custom setting draw action that allows polling keyboard input with the Input class.
        /// Note: Make sure to focus on your UI control when you are accepting input so user doesn't type in the search box or in another setting (best to do this on every frame).
        /// If you don't draw any selectable UI controls You can use `GUIUtility.keyboardControl = -1;` on every frame to make sure that nothing is selected.
        /// </summary>
        /// <example>
        /// CustomHotkeyDrawer = (ConfigEntryBase setting, ref bool isEditing) =>
        /// {
        ///     if (isEditing)
        ///     {
        ///         // Make sure nothing else is selected since we aren't focusing on a text box with GUI.FocusControl.
        ///         GUIUtility.keyboardControl = -1;
        ///                     
        ///         // Use Input.GetKeyDown and others here, remember to set isEditing to false after you're done!
        ///         // It's best to check Input.anyKeyDown and set isEditing to false immediately if it's true,
        ///         // so that the input doesn't have a chance to propagate to the game itself.
        /// 
        ///         if (GUILayout.Button("Stop"))
        ///             isEditing = false;
        ///     }
        ///     else
        ///     {
        ///         if (GUILayout.Button("Start"))
        ///             isEditing = true;
        ///     }
        /// 
        ///     // This will only be true when isEditing is true and you hold any key
        ///     GUILayout.Label("Any key pressed: " + Input.anyKey);
        /// }
        /// </example>
        /// <param name="setting">
        /// Setting currently being set (if available).
        /// </param>
        /// <param name="isCurrentlyAcceptingInput">
        /// Set this ref parameter to true when you want the current setting drawer to receive Input events.
        /// The value will persist after being set, use it to see if the current instance is being edited.
        /// Remember to set it to false after you are done!
        /// </param>
        public delegate void CustomHotkeyDrawerFunc(BepInEx.Configuration.ConfigEntryBase setting, ref bool isCurrentlyAcceptingInput);

        /// <summary>
        /// Show this setting in the settings screen at all? If false, don't show.
        /// </summary>
        public bool? Browsable;

        /// <summary>
        /// Category the setting is under. Null to be directly under the plugin.
        /// </summary>
        public string Category;

        /// <summary>
        /// If set, a "Default" button will be shown next to the setting to allow resetting to default.
        /// </summary>
        public object DefaultValue;

        /// <summary>
        /// Force the "Reset" button to not be displayed, even if a valid DefaultValue is available. 
        /// </summary>
        public bool? HideDefaultButton;

        /// <summary>
        /// Force the setting name to not be displayed. Should only be used with a <see cref="CustomDrawer"/> to get more space.
        /// Can be used together with <see cref="HideDefaultButton"/> to gain even more space.
        /// </summary>
        public bool? HideSettingName;

        /// <summary>
        /// Optional description shown when hovering over the setting.
        /// Not recommended, provide the description when creating the setting instead.
        /// </summary>
        public string Description;

        /// <summary>
        /// Name of the setting.
        /// </summary>
        public string DispName;

        /// <summary>
        /// Order of the setting on the settings list relative to other settings in a category.
        /// 0 by default, higher number is higher on the list.
        /// </summary>
        public int? Order;

        /// <summary>
        /// Only show the value, don't allow editing it.
        /// </summary>
        public bool? ReadOnly;

        /// <summary>
        /// If true, don't show the setting by default. User has to turn on showing advanced settings or search for it.
        /// </summary>
        public bool? IsAdvanced;

        /// <summary>
        /// Custom converter from setting type to string for the built-in editor textboxes.
        /// </summary>
        public System.Func<object, string> ObjToStr;

        /// <summary>
        /// Custom converter from string to setting type for the built-in editor textboxes.
        /// </summary>
        public System.Func<string, object> StrToObj;
    }
}
