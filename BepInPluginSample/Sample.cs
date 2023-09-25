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

            // hpNotChg = Config.Bind("game", "hpNotChg", true);
            // xpMulti = Config.Bind("game", "xpMulti", 2f);

            // =========================================================
            #endregion
        }

        #region size_SettingChanged
        public void size_SettingChanged(object sender, EventArgs e)
        {
            logger.LogInfo($"size_SettingChanged {size.Value}");
            if(tcontent)
                tcontent.transform.localScale = Vector3.one * size.Value;
        }
        #endregion

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
                Logger.LogError(e);
            }
            try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
            {
                size.SettingChanged += size_SettingChanged;
                size_SettingChanged(null, null);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        public void OnDisable()
        {
            Logger.LogWarning("OnDisable");
            harmony?.UnpatchSelf();
            size_SettingChanged(null, null);
            size.SettingChanged -= size_SettingChanged;
        }

        #region Harmony

        static InventoryScreen inventoryScreen;

        [HarmonyPatch(typeof(InventoryScreen), "Awake")]
        [HarmonyPostfix]
        public static void InventoryScreen_Awake(InventoryScreen __instance)
        {
            logger.LogWarning($"InventoryScreen_Awake ");
            inventoryScreen = __instance;
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
        static ItemsPanel itemsPanel;
        static GameObject ocontent;
        static Transform tcontent=null;
        static Transform containers = null;

        [HarmonyPatch(typeof(ItemsPanel), "Show")]
        [HarmonyPostfix]
        public static void ItemsPanel_Show(ItemsPanel __instance)
        {
            logger.LogWarning($"ItemsPanel_Show ");
            // Common UI/Common UI/InventoryScreen/
            itemsPanel = __instance;
            // Common UI/Common UI/InventoryScreen/Items Panel/Containers Panel/Scrollview Parent/Containers Scrollview/
            //ocontent = GameObject.Find("Common UI/Common UI/InventoryScreen/Items Panel/Containers Panel/Scrollview Parent/Containers Scrollview/Content");
            //logger.LogWarning($"ItemsPanel_Show {ocontent.name}");
            containers = itemsPanel.Transform.Find("Containers Panel");
            tcontent = containers.transform.Find("Scrollview Parent/Containers Scrollview/Content");
            logger.LogWarning($"ItemsPanel_Show {tcontent.name}");
            my.size_SettingChanged(null, null);
            // slotView.gameObject.GetComponent<HorizontalLayoutGroup>().spacing += 10f;
            //containers.gameObject.GetComponent<HorizontalLayoutGroup>().spacing += 10f;

            /*
            foreach (Transform child in ocontent.transform)
            {
                logger.LogWarning($"ItemsPanel_Show {child.name}");
                    //transform.find("Turret/Cannon/spPoint");
            }
            */
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
