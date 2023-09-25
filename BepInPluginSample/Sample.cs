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

        static ConfigEntry<float> size;

        static bool ContainersPanelOn=false;

        static ConfigEntry<bool> SlotPanel;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> SlotPanelKey;

        static ConfigEntry<bool> WidePanel;
        static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> WidePanelKey;
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

            SlotPanel = Config.Bind("GUI", "SlotPanel", true);
            SlotPanelKey= Config.Bind("GUI", "SlotPanelKey", new KeyboardShortcut(KeyCode.S));// 이건 단축키

            WidePanel = Config.Bind("GUI", "WidePanel", true);
            WidePanelKey = Config.Bind("GUI", "WidePanelKey", new KeyboardShortcut(KeyCode.A));// 이건 단축키
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
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                size.SettingChanged += size_SettingChanged;
            }
            catch (Exception e)
            {
                Logger.LogError("size_SettingChanged");
                Logger.LogError(e.ToString());
            }
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                SlotPanel.SettingChanged += slot_SettingChanged;                
            }
            catch (Exception e)
            {
                Logger.LogError("slot_SettingChanged");
                Logger.LogError(e.ToString());
            }
        }

        public void Update()
        {
            #region GUI
            if (SlotPanelKey.Value.IsUp())// 단축키가 일치할때
            {
                SlotPanel.Value = !SlotPanel.Value;
            }
            if (WidePanelKey.Value.IsUp())// 단축키가 일치할때
            {
                WidePanel.Value = !WidePanel.Value;
            }
            #endregion
        }

        public void OnDisable()
        {
            Logger.LogWarning("OnDisable");
            harmony?.UnpatchSelf();
            size.SettingChanged -= size_SettingChanged;
            SlotPanel.SettingChanged -= slot_SettingChanged;
        }

        #region Harmony

        static InventoryScreen inventoryScreen;
        static Transform tcontent = null;
        static Transform TacticalVest = null;
        static Transform Backpack = null;
        static Transform SecuredContainer = null;

        [HarmonyPatch(typeof(InventoryScreen), "Awake")]
        [HarmonyPostfix]
        public static void InventoryScreen_Awake(InventoryScreen __instance)
        {
            logger.LogWarning($"InventoryScreen_Awake ");
            inventoryScreen = __instance;
        }

        [HarmonyPatch(typeof(ContainersPanel), "Show")]
        [HarmonyPostfix]
        public static void ContainersPanel_Show()
        {
            logger.LogWarning($"ContainersPanel_Show ");
            ContainersPanelOn = true;
            if (!tcontent)
            {
                tcontent = inventoryScreen.transform.Find("Items Panel/Containers Panel/Scrollview Parent/Containers Scrollview/Content");
            }
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

        #region size_SettingChanged
        public void size_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"size_SettingChanged {size.Value}");
            tcontent.localScale = Vector3.one * size.Value;
        }
        #endregion

        #region slot_SettingChanged
        public void slot_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"slot_SettingChanged {ContainersPanelOn}");
            if (ContainersPanelOn)
            {
                if (!TacticalVest) TacticalVest = tcontent.transform.Find("TacticalVest Slot/Slot Panel");
                if (!Backpack) Backpack = tcontent.transform.Find("Backpack Slot/Slot Panel");
                if (!SecuredContainer) SecuredContainer = tcontent.transform.Find("SecuredContainer Slot/Slot Panel");
                TacticalVest.gameObject.SetActive(SlotPanel.Value);
                Backpack.gameObject.SetActive(SlotPanel.Value);
                SecuredContainer.gameObject.SetActive(SlotPanel.Value);
            }
        }
        #endregion
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
