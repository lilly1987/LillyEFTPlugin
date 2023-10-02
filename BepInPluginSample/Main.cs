using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using EFTConfiguration.Attributes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EFT.UI.InventoryScreen;
using KeyboardShortcut = BepInEx.Configuration.KeyboardShortcut;

namespace LillyEFTPlugin
{
    [BepInPlugin("Game.Lilly.Plugin", "Lilly", "1.3.4")]
    //[BepInDependency("com.bepinex.plugin.somedependency", BepInDependency.DependencyFlags.SoftDependency)]
    [EFTConfigurationPluginAttributes("https://hub.sp-tarkov.com/files/file/1475-lilly-eft-plugin/", "../localized/core", false, false)]
    public class Main : BaseUnityPlugin
    {
        #region 변수
        // =========================================================
        static Main main;
        static Harmony harmony;
        internal static ManualLogSource logger;

        internal static int ordercount=int.MaxValue;

        internal static Action awake;
        internal static Action onEnable;
        internal static Action update;
        internal static Action onDisable;
        // =========================================================
        #endregion

        public void Awake()
        {
            #region GUI
            logger = Logger;
            Logger.LogMessage("Awake");
            main = this;
            #endregion

            //
            InventoryPlugin.init(Config, logger);
            TagPanelFix.init(Config, logger);
            WeaponPreviewPlugin.init(Config, logger);
            GridWindowPlugin.init(Config, logger);

            // =========================================================
            awake();
        }

        public void OnEnable()
        {
            logger.LogWarning("OnEnable");
            /*
            // 하모니 패치
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                harmony = Harmony.CreateAndPatchAll(typeof(Main));
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
            */
            onEnable();
            //WeaponPreviewPlugin.OnEnable();
            //GridWindowPlugin.OnEnable();
        }



        public void Update()
        {
            update();
            //WeaponPreviewPlugin.Update();
            //GridWindowPlugin.Update();
        }

        public void OnDisable()
        {
            Logger.LogWarning("OnDisable");
            //WeaponPreviewPlugin.OnDisable();
            //InventoryPlugin.OnDisable();
            onDisable();
            harmony?.UnpatchSelf();
            /*
            SlotPanelSize.SettingChanged -= size_SettingChanged;
            SlotOn.SettingChanged -= size_SettingChanged;
            SlotPanelSizeKey.SettingChanged -= size_SettingChanged;
            SlotPanel.SettingChanged -= slot_SettingChanged;
            LeftPanelX.SettingChanged -= slot_SettingChanged;
            LeftPanel.SettingChanged -= LeftPanel_SettingChanged;
            StashPanel.SettingChanged -= StashPanel_SettingChanged;
            */
        }



        /*
        [HarmonyPatch(typeof(InventoryScreen), "TranslateCommand")]
        [HarmonyPostfix]
        public static void InventoryScreen_TranslateCommand()
        {
            logger.LogWarning($"InventoryScreen_TranslateCommand");    
        }
        */

        /*
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
        */

        /*
        [HarmonyPatch(typeof(ItemViewStats), "NewItemView")]
        [HarmonyPostfix]
        public static void ItemView_NewItemView(Item item)
        {
            logger.LogWarning($"ItemView_NewItemView ; {item.Name} ");
        }
        */



        /*
        // public void OnRefreshItem(GEventArgs22 eventArgs);
        [HarmonyPatch(typeof(GridItemView), "OnRefreshItem")]
        [HarmonyPostfix]
        public static void GridItemView_OnRefreshItem()
        {
            logger.LogWarning($"GridItemView_OnRefreshItem");
        }
        
        // public void OnRefreshItem(GEventArgs22 eventArgs);
        [HarmonyPatch(typeof(GridItemView), "OnItemAdded")]
        [HarmonyPostfix]
        public static void GridItemView_OnItemAdded()
        {
            logger.LogWarning($"GridItemView_OnItemAdded");
        }
        
        // public void OnRefreshItem(GEventArgs22 eventArgs);
        [HarmonyPatch(typeof(GridItemView), "OnItemRemoved")]
        [HarmonyPostfix]
        public static void GridItemView_OnItemRemoved()
        {
            logger.LogWarning($"GridItemView_OnItemRemoved");
        }

        /// <summary>
        /// 연속으로 두번 호출
        /// [Warning:     Lilly] TagComponent_Set ; adf , 0 , True
        /// [Warning: Lilly] TagComponent_Set ; adf , 0 , False
        /// 이후 모든 아이템 GridItemView_OnRefreshItem 호출
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <param name="simulate"></param>
        [HarmonyPatch(typeof(TagComponent), "Set")]
        [HarmonyPostfix]
        public static void TagComponent_Set(string name, int color, bool simulate)
        {
            logger.LogWarning($"TagComponent_Set ; {name} , {color} , {simulate} ");
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

    }

}
