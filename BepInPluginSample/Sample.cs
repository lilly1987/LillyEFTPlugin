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

namespace BepInPluginSample
{
    [BepInPlugin("Game.Lilly.Plugin", "Lilly", "1.0")]
    public class Sample : BaseUnityPlugin
    {
        #region 변수
        // =========================================================
        static Sample my;
        static Harmony harmony;
        static ManualLogSource logger;


        static bool ContainersPanelShow=false;

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
            SlotOn = Config.Bind("SlotPanelSize", "On", true);
            SlotPanelSizeKey = Config.Bind("SlotPanelSize", "Key", new KeyboardShortcut(KeyCode.W));// 이건 단축키
            SlotPanelSize = Config.Bind("SlotPanelSize", "Size", 0.75f);

            SlotPanel = Config.Bind("SlotPanel", "On", true);
            SlotPanelKey= Config.Bind("SlotPanel", "Key", new KeyboardShortcut(KeyCode.S));// 이건 단축키

            LeftPanel = Config.Bind("LeftPanel", "On", true);
            LeftPanelKey = Config.Bind("LeftPanel", "Key", new KeyboardShortcut(KeyCode.A));// 이건 단축키
            LeftPanelX = Config.Bind("LeftPanel", "Size", 1.05f);

            StashPanel = Config.Bind("StashPanel", "On", true);
            StashPanelKey = Config.Bind("StashPanel", "Key", new KeyboardShortcut(KeyCode.D));// 이건 단축키
            StashPanelX = Config.Bind("StashPanel", "Size", 1.025f);
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
            if (ContainersPanelShow)
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
            logger.LogInfo($"size_SettingChanged {SlotPanelSize.Value}");
            if (SlotOn.Value)
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
            logger.LogInfo($"slot_SettingChanged {ContainersPanelShow}");
            if (ContainersPanelShow)
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
            logger.LogInfo($"Wide_SettingChanged {LeftPanel.Value} {vContainersPaneloffsetMin.x} {rLeftPanel.sizeDelta.x}");
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
            logger.LogInfo($"StashPanel_SettingChanged {StashPanel.Value}");
            //tStashPanel.gameObject.SetActive(!StashPanel.Value);
            if (StashPanel.Value)
            {
                rContainersPanel.offsetMax = vContainersPanelXs;;
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

        static InventoryScreen inventoryScreen;
        static Transform tItemsPanel = null;
        static Transform tLeftPanel = null;
        static Transform tGearPanel = null;
        static Transform tContainersPanel = null;
        static Transform tStashPanel = null;
        static Transform tContent = null;
        static Transform tScrollviewParent = null;
        static Transform tContainersScrollview= null;
        static RectTransform rContainersPanel = null;
        static RectTransform rStashPanel = null;
        static RectTransform rLeftPanel = null;
        static RectTransform rGearPanel = null;
        static RectTransform rScrollviewParent = null;
        static RectTransform rContainersScrollview= null;
        //static RectTransform lGearPanel = null;
        static Vector2 vContainersPaneloffsetMin ;
        static Vector2 vContainersPaneloffsetMax ;
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
            
            tItemsPanel = inventoryScreen.transform.Find("Items Panel");
            
            tContainersPanel= tItemsPanel.transform.Find("Containers Panel");
            rContainersPanel=(RectTransform)tContainersPanel;
            vContainersPaneloffsetMin=rContainersPanel.offsetMin;
            vContainersPaneloffsetMax=rContainersPanel.offsetMax;

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
        [HarmonyPatch(typeof(ContainersPanel), "Show")]
        [HarmonyPostfix]
        public static void ContainersPanel_Show()
        {
            logger.LogWarning($"ContainersPanel_Show ");
            ContainersPanelShow = true;
            my.size_SettingChanged(null, null);
            my.slot_SettingChanged(null, null);
            my.LeftPanel_SettingChanged(null, null);
        }
        
        [HarmonyPatch(typeof(SimpleStashPanel), "Show")]
        [HarmonyPostfix]
        public static void SimpleStashPanel_Show()
        {
            logger.LogWarning($"SimpleStashPanel_Show ");
            my.StashPanel_SettingChanged(null, null);
        }
        [HarmonyPatch(typeof(ComplexStashPanel), "Show")]
        [HarmonyPostfix]
        public static void ComplexStashPanel_Show()
        {
            logger.LogWarning($"ComplexStashPanel_Show ");
            my.StashPanel_SettingChanged(null, null);
        }

        [HarmonyPatch(typeof(ContainersPanel), "Close")]
        [HarmonyPostfix]
        public static void ContainersPanel_Close()
        {
            logger.LogWarning($"ContainersPanel_Close");
            ContainersPanelShow = false;
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
}
