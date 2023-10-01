using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace LillyEFTPlugin
{
    internal class TagPanelFix
    {
        static Harmony harmony = null;

        static ConfigEntry<bool> TagPanelFixOn;
        static ConfigEntry<float> TagPanelFiY;
        static Vector3 TagPanelFiV=new Vector3();
        static ManualLogSource Logger;

        internal static void Awake(ConfigFile Config , ManualLogSource logger)
        {
            Logger = logger;
            TagPanelFixOn = Config.Bind("Inventory", "TagPanelFixOn", true,
                new ConfigDescription(
                    "TagPanelFix"
                    , null
                    , new ConfigurationManagerAttributes { Order = 203 }
                )
            );
            TagPanelFiY = Config.Bind("Inventory", "TagPanelFiY", -12f,
                new ConfigDescription(
                    "TagPanelFix"
                    , null
                    , new ConfigurationManagerAttributes { Order = 202 }
                )
            );
            TagPanelFixOn.SettingChanged += TagPanelFix_SettingChanged;
            TagPanelFix_SettingChanged(null, null);
        }

        private static void TagPanelFix_SettingChanged(object sender, EventArgs e)
        {
            Logger.LogWarning($"TagPanelFix_SettingChanged {TagPanelFixOn.Value}");
            if (TagPanelFixOn.Value)
            {
                if (harmony == null)
                    TagPanelFiV.y = TagPanelFiY.Value;
                    //TagPanelFiV =new Vector3(0, TagPanelFiY.Value);
                    try // 가급적 try 처리 해주기. 하모니 패치중에 오류나면 다른 플러그인까지 영향 미침
                    {
                        harmony = Harmony.CreateAndPatchAll(typeof(TagPanelFix));
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("harmony TagPanelFix");
                        Logger.LogError(ex.ToString());
                    }
            }
            else
            {
                harmony?.UnpatchSelf();
            }
        }

        //  GridViewMagnifier
        //      GridView

        //  ItemViewStats
        //      TextMeshProUGUI

        /*
        /// <summary>
        /// 다음에 GridItemView_NewGridItemView 를 호출
        /// </summary>
        /// <param name="item"></param>
        [HarmonyPatch(typeof(ItemView), "NewItemView")]
        [HarmonyPostfix]
        public static void ItemView_NewItemView(Item item)
        {
            Logger.LogWarning($"ItemView_NewItemView ; {item.Name} ");
        }
        [HarmonyPatch(typeof(ItemViewStats), "SetStaticInfo")]
        [HarmonyPostfix]
        public static void ItemViewStats_SetStaticInfo(Item item, bool examined)
        {
            Logger.LogWarning($"ItemViewStats_SetStaticInfo ; {item.Name} ; {examined} ");
        }
        */

        /// <summary>
        /// 
        /// </summary>
        // protected GridItemView NewGridItemView(Item item, GClass2710 sourceContext, ItemRotation rotation, TraderControllerClass itemController, IItemOwner itemOwner, [CanBeNull] FilterPanel filterPanel, [CanBeNull] GInterface324 container, [CanBeNull] ItemUiContext itemUiContext, InsuranceCompanyClass insurance, bool isSearched = true);
        [HarmonyPatch(typeof(GridItemView), "NewGridItemView")]
        [HarmonyPostfix]
        public static void GridItemView_NewGridItemView(GridItemView __instance, TextMeshProUGUI ___TagName)
        {
            //Logger.LogWarning($"GridItemView_NewGridItemView ; {__instance.Item.Name} ; {__instance.Item.ShortName}");
            var tagPanel = __instance.transform.Find("TagPanel");
            if (tagPanel && tagPanel.gameObject.activeSelf)
            {
                tagPanel.localPosition = TagPanelFiV;
                
                //var tagName = tagPanel.Find("TagName");
                //tagName.gameObject.SetActive(true);

                tagPanel.Find("TagName").gameObject.SetActive(true);
                
                //var textMeshProUGUI = tagName.GetComponent<TextMeshProUGUI>();
                //textMeshProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Left;
                //___TagName.horizontalAlignment = HorizontalAlignmentOptions.Left;
                //textMeshProUGUI.autoSizeTextContainer = true;
                //___TagName.
            }
        }

    }
}
