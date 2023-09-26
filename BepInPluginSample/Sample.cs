using BepInEx;
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


        static bool ContainersPanelOn=false;

        static ConfigEntry<bool> SlotOn;
        static ConfigEntry<float> size;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> sizeKey;
        static ConfigEntry<float> SlotPanelX;

        static ConfigEntry<bool> SlotPanel;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> SlotPanelKey;

        static ConfigEntry<bool> LeftPanel;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> LeftPanelKey;

        static ConfigEntry<bool> StashPanel;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> StashPanelKey;
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
            size = Config.Bind("GUI", "Containers", 0.75f);
            SlotOn = Config.Bind("GUI", "ContainersSizeOn", true);
            sizeKey = Config.Bind("GUI", "ContainersSizeKey", new KeyboardShortcut(KeyCode.W));// 이건 단축키

            SlotPanel = Config.Bind("GUI", "SlotPanel", true);
            SlotPanelKey= Config.Bind("GUI", "SlotPanelKey", new KeyboardShortcut(KeyCode.S));// 이건 단축키
            SlotPanelX = Config.Bind("GUI", "SlotPanelX", 1.05f);


            LeftPanel = Config.Bind("GUI", "Left Panel", true);
            LeftPanelKey = Config.Bind("GUI", "Left Panel Key", new KeyboardShortcut(KeyCode.A));// 이건 단축키

            StashPanel = Config.Bind("GUI", "StashPanel", true);
            StashPanelKey = Config.Bind("GUI", "StashPanelKey", new KeyboardShortcut(KeyCode.D));// 이건 단축키
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

                size.SettingChanged += size_SettingChanged;
                SlotOn.SettingChanged += size_SettingChanged;
                sizeKey.SettingChanged += size_SettingChanged;

                SlotPanel.SettingChanged += slot_SettingChanged;
                SlotPanelX.SettingChanged += slot_SettingChanged;                

                LeftPanel.SettingChanged += LeftPanel_SettingChanged;                

                StashPanel.SettingChanged += StashPanel_SettingChanged;                

        }

        public void Update()
        {
            #region GUI
            if (SlotPanelKey.Value.IsUp())// 단축키가 일치할때
            {
                SlotPanel.Value = !SlotPanel.Value;
            }
            if (sizeKey.Value.IsUp())// 단축키가 일치할때
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
            #endregion
        }

        public void OnDisable()
        {
            Logger.LogWarning("OnDisable");
            harmony?.UnpatchSelf();
            size.SettingChanged -= size_SettingChanged;
            SlotOn.SettingChanged -= size_SettingChanged;
            sizeKey.SettingChanged -= size_SettingChanged;
            SlotPanel.SettingChanged -= slot_SettingChanged;
            SlotPanelX.SettingChanged -= slot_SettingChanged;
            LeftPanel.SettingChanged -= LeftPanel_SettingChanged;
            StashPanel.SettingChanged -= StashPanel_SettingChanged;

        }



        #region size_SettingChanged
        public void size_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"size_SettingChanged {size.Value}");
            if (SlotOn.Value)
            {
                tContent.localScale = Vector3.one * size.Value;
            }
            else
                tContent.localScale = Vector3.one;
        }
        #endregion

        #region slot_SettingChanged
        public void slot_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"slot_SettingChanged {ContainersPanelOn}");
            if (ContainersPanelOn)
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
            logger.LogInfo($"Wide_SettingChanged {LeftPanel.Value} {rContainersPaneloffsetMin.x} {rLeftPanel.sizeDelta.x}");
            if (LeftPanel.Value)
            {
                rContainersPanel.offsetMin = new Vector2(rContainersPaneloffsetMin.x - rLeftPanel.rect.size.x* SlotPanelX.Value, rContainersPaneloffsetMin.y);
            }
            else
            {
                rContainersPanel.offsetMin = rContainersPaneloffsetMin;
            }
        }
        #endregion

        private void StashPanel_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"StashPanel_SettingChanged {StashPanel.Value}");
            tStashPanel.gameObject.SetActive(!StashPanel.Value);
            if (StashPanel.Value)
            {
                rContainersPanel.offsetMax = new Vector2(rContainersPaneloffsetMax.x + rStashPanel.rect.size.x, rContainersPaneloffsetMax.y);
            }
            else
            {
                rContainersPanel.offsetMax = rContainersPaneloffsetMax;
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
        static Vector2 rContainersPaneloffsetMin ;
        static Vector2 rContainersPaneloffsetMax ;
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
            rContainersPaneloffsetMin=rContainersPanel.offsetMin;
            rContainersPaneloffsetMax=rContainersPanel.offsetMax;

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

            tLeftPanel = tItemsPanel.transform.Find("Left Panel");
            rLeftPanel = (RectTransform)tLeftPanel;

            tGearPanel = tLeftPanel.transform.Find("Gear Panel");
            rGearPanel = (RectTransform)tGearPanel;
//            lGearPanel = (RectTransform)tGearPanel;
            my.LeftPanel_SettingChanged(null, null);
            my.StashPanel_SettingChanged(null, null);
        }

        [HarmonyPatch(typeof(ContainersPanel), "Show")]
        [HarmonyPostfix]
        public static void ContainersPanel_Show()
        {
            logger.LogWarning($"ContainersPanel_Show ");
            ContainersPanelOn = true;
            my.size_SettingChanged(null, null);
            my.slot_SettingChanged(null, null);
        }

        [HarmonyPatch(typeof(ContainersPanel), "Close")]
        [HarmonyPostfix]
        public static void ContainersPanel_Close()
        {
            logger.LogWarning($"ContainersPanel_Close");
            ContainersPanelOn = false;
        }


        /*
        static ContainersPanel containersPanel;

        [HarmonyPatch(typeof(ContainersPanel), MethodType.Constructor)]
        [HarmonyPostfix]
        public static void ContainersPanel_cont(ContainersPanel __instance)
        {
            logger.LogWarning($"ContainersPanel_cont ");
            containersPanel = __instance;
            foreach (Transform child in containersPanel.Transform)
            {
                logger.LogWarning($"ContainersPanel_cont {child.name}");
                //transform.find("Turret/Cannon/spPoint");
            }
        }
        */


        /*
        static ItemsPanel itemsPanel;
        static Transform containers = null;
        [HarmonyPatch(typeof(ItemsPanel), "Show")]
        [HarmonyPostfix]
        public static void ItemsPanel_Show(ItemsPanel __instance)
        {
            logger.LogWarning($"ItemsPanel_Show ");
            // Common UI/Common UI/InventoryScreen/
            //itemsPanel = __instance;
            //// Common UI/Common UI/InventoryScreen/Items Panel/Containers Panel/Scrollview Parent/Containers Scrollview/
            ////ocontent = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/Containers Panel/Scrollview Parent/Containers Scrollview/Content");
            ////logger.LogWarning($"ItemsPanel_Show {ocontent.name}");
            //if ( containers == null )
            //{
            //    containers = itemsPanel.Transform.Find("Containers Panel");
            //}
            //if (tcontent == null )
            //{
            //    tcontent = containers.transform.Find("Scrollview Parent/Containers Scrollview/Content");
            //}
            logger.LogWarning($"ItemsPanel_Show {tcontent.name}");
            my.size_SettingChanged(null, null);
            // slotView.gameObject.GetComponent<HorizontalLayoutGroup>().spacing += 10f;
            //containers.gameObject.GetComponent<HorizontalLayoutGroup>().spacing += 10f;

            
      
            foreach (Transform child in ocontent.transform)
            {
                logger.LogWarning($"ItemsPanel_Show {child.name}");
                    //transform.find("Turret/Cannon/spPoint");
            }

    }
        */
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
