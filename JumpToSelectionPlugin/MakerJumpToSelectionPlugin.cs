using BepInEx;
using CharaCustom;
using HarmonyLib;
using KKAPI;
using KKAPI.Maker;
using KKAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MakerJumpToSelectionPlugin
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
#if HS2
    [BepInProcess("HoneySelect2.exe")]
#else
    [BepInProcess("AI-Syoujyo.exe")]
#endif
    public class MakerJumpToSelectionPlugin : BaseUnityPlugin
    {

        public const string GUID = "orange.spork.makerjumptoselection";
        public const string Version = "1.0.0";
        public const string PluginName = "Maker Jump to Selection";

        public static MakerJumpToSelectionPlugin Instance { get; set; }

        internal BepInEx.Logging.ManualLogSource Log => Logger;

        public MakerJumpToSelectionPlugin()
        {
            if (Instance != null)
                throw new InvalidOperationException("Singleton only.");

            Instance = this;

#if DEBUG
            Log.LogInfo("Jump to Maker Loaded.");
#endif
        }

        public void Start()
        {
            MakerAPI.MakerBaseLoaded += MakerLoaded;


        }

        private void MakerLoaded(object sender, RegisterCustomControlsEvent eventArgs)
        {
            StartCoroutine(OnMakerLoading());
        }

        private static IEnumerator OnMakerLoading()
        {
            yield return new WaitUntil(() => GameObject.Find("CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/Setting/Setting01/SelectBox/Scroll View") != null);

            foreach (string buttonPointName in StandardJumpPoints.Keys)
            {
                CreateJumpButton(buttonPointName, StandardJumpPoints[buttonPointName], (g) => { g.GetComponent<CustomSelectScrollController>().SetNowLine(); }, new Vector3(-40, 0, 0), true);
            }
            foreach (string buttonPointName in ClothesJumpPoints.Keys)
            {
                CreateJumpButton(buttonPointName, ClothesJumpPoints[buttonPointName], (g) => { g.GetComponent<CustomClothesScrollController>().SetNowLine(); }, new Vector3(0, -40, 0), false);
            }
            foreach (string buttonPointName in CharaJumpPoints.Keys)
            {
                CreateJumpButton(buttonPointName, CharaJumpPoints[buttonPointName], (g) => { g.GetComponent<CustomCharaScrollController>().SetNowLine(); }, new Vector3(0, -40, 0), false);
            }
        }

        private static void CreateJumpButton(String buttonPointName, String[] scrollerNames, Action<GameObject> invocation, Vector3 adjustment, bool changeDragRect)
        {
            List<GameObject> scrollerObjects = new List<GameObject>();
            foreach (string scrollerName in scrollerNames)
            {
                scrollerObjects.Add(GameObject.Find(scrollerName));
            }
            
            GameObject buttonPoint = GameObject.Find(buttonPointName);

            GameObject jumpButton = UnityEngine.Object.Instantiate(buttonPoint, buttonPoint.transform.parent, false);
            jumpButton.transform.Translate(adjustment);
            UI_ButtonEx button = jumpButton.GetComponent<UI_ButtonEx>();
            jumpButton.name = "JumpToSelection";
            button.onClick.ActuallyRemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                foreach (GameObject scrollObject in scrollerObjects)
                {
                    invocation.Invoke(scrollObject);
                }                
            });
            Image buttonImage = jumpButton.GetComponent<Image>();
            UnityEngine.Object.DestroyImmediate(buttonImage);

            buttonImage = jumpButton.AddComponent<Image>();
            Texture2D tex = new Texture2D(28, 28);
            if (jumpIcon == null)
            {
                LoadJumpIcon();
            }
            tex.LoadImage(jumpIcon);

            buttonImage.sprite = Sprite.Create(tex, new Rect(0, 0, 28, 28), new Vector2(0.5f, 0.5f));
            button.targetGraphic = buttonImage;

            Image hoverImage = jumpButton.transform.GetChild(0).GetComponent<Image>();
            UnityEngine.Object.DestroyImmediate(hoverImage);
            hoverImage = jumpButton.transform.GetChild(0).gameObject.AddComponent<Image>();
            Texture2D hoverTex = new Texture2D(28, 28);
            hoverTex.LoadImage(jumpSelIcon);
            hoverImage.sprite = Sprite.Create(hoverTex, new Rect(0, 0, 28, 28), new Vector2(0.5f, 0.5f));
            hoverImage.enabled = false;

            button.overImage = hoverImage;           
            
            if (changeDragRect)
            {
                Transform dragRect = buttonPoint.transform.parent.parent.Find("DragRect");
                if (dragRect != null)
                {
                    dragRect.localScale = new Vector3(0.95f, 1, 1);
                }
            }
        }



        private static byte[] jumpIcon;
        private static byte[] jumpSelIcon;
        private static void LoadJumpIcon()
        {
#if HS2
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"HS2_JumpToSelectionPlugin.resources.jump_icon.png"))
#else
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"AI_JumpToSelectionPlugin.resources.jump_icon.png"))
#endif
            {
                jumpIcon = new byte[stream.Length];
                stream.Read(jumpIcon, 0, jumpIcon.Length);
            }
#if HS2
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"HS2_JumpToSelectionPlugin.resources.jump_icon_sel.png"))
#else
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"AI_JumpToSelectionPlugin.resources.jump_icon_sel.png"))
#endif
            {
                jumpSelIcon = new byte[stream.Length];
                stream.Read(jumpSelIcon, 0, jumpSelIcon.Length);
            }
        }

        private static readonly Dictionary<string, string[]> CharaJumpPoints = new Dictionary<string, string[]> 
        {
            { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinOption/SystemWin/imgWinBack/btnClose", new string[] { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinOption/SystemWin/O_SaveDelete/Scroll View", "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinOption/SystemWin/O_Load/Scroll View" } }
        };

        private static readonly Dictionary<string, string[]> ClothesJumpPoints = new Dictionary<string, string[]>
        {
            { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/SystemWin/imgWinBack/btnClose", new string[] { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/SystemWin/C_SaveDelete/Scroll View", "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/SystemWin/C_Load/Scroll View" } }
        };


        private static readonly Dictionary<string, string[]> StandardJumpPoints = new Dictionary<string, string[]>
        {
            { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/imgWinBack/btnClose", new string[] { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinClothes/DefaultWin/C_Clothes/Setting/Setting01/SelectBox/Scroll View" } },
            { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinAccessory/imgWinBack/btnClose", new string[] { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinAccessory/A_Slot/Setting/Setting01/SelectBox/Scroll View" } },
            { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinHair/imgWinBack/btnClose", new string[] { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinHair/H_Hair/Setting/Setting01/SelectBox/Scroll View" } },
            { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/imgWinBack/btnClose", new string[] { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Skin/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Skin/Setting/Setting02/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Sunburn/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Nip/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Underhair/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinBody/B_Paint/Setting/Setting01/SelectBox/Scroll View"
            } },

            { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/imgWinBack/btnClose", new string[] { "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_FaceType/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_FaceType/Setting/Setting02/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_FaceType/Setting/Setting03/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_Mole/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_EyeLR/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_EyeLR/Setting/Setting03/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_EyeHL/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_Eyebrow/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_Eyelashes/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_MakeupEyeshadow/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_MakeupCheek/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_MakeupLip/Setting/Setting01/SelectBox/Scroll View",
            "CharaCustom/CustomControl/CanvasSub/SettingWindow/WinFace/F_MakeupPaint/Setting/Setting01/SelectBox/Scroll View"
            } }
        };
    }
}
