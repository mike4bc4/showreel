using System.Collections;
using System.Collections.Generic;
using CustomControls;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UI
{
    public class LayerManager : MonoBehaviour
    {
        static LayerManager s_Instance;

        [SerializeField] Canvas m_Canvas;
        [SerializeField] PanelSettings m_TemplatePanelSettings;
        [SerializeField] RenderTexture m_TemplateRenderTexture;
        [SerializeField] Material m_BlurMaterial;

        [SerializeField] VisualTreeAsset test;

        public static Material BlurMaterial
        {
            get => s_Instance.m_BlurMaterial;
        }

        void Awake()
        {
            if (s_Instance != null)
            {
                Destroy(this);
                return;
            }

            s_Instance = this;
        }

        void Start()
        {
            // var layer = AddNewLayer("TestLayer");
            // layer.visualTreeAsset = test;
            // layer.filter = new BlurFilter();
            // layer.alpha = 0f;

            // IEnumerator Coroutine()
            // {
            //     var anim1 = AnimationManager.Animate(layer, nameof(layer.alpha), 1f);
            //     anim1.time = 1f;

            //     yield return new WaitForSeconds(0.5f);

            //     var anim2 = AnimationManager.Animate(layer.filter, nameof(BlurFilter.size), 0f);
            //     anim2.time = 1f;

            //     yield return anim2.coroutine;

            //     Debug.Log(Time.time);

            //     yield return layer.rootVisualElement.Q<DiamondLineHorizontal>().Unfold();

            //     Debug.Log(Time.time);
            // }

            // StartCoroutine(Coroutine());
        }

        public static Layer AddNewLayer(string name = "Layer")
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(s_Instance.m_Canvas.transform);

            // Fill entire canvas.
            var rectTransform = (RectTransform)gameObject.transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var layer = gameObject.AddComponent<Layer>();

            var rawImage = gameObject.AddComponent<RawImage>();
            rawImage.texture = new RenderTexture(s_Instance.m_TemplateRenderTexture);

            var uiDocument = gameObject.AddComponent<UIDocument>();
            uiDocument.panelSettings = Instantiate(s_Instance.m_TemplatePanelSettings);
            uiDocument.panelSettings.targetTexture = (RenderTexture)rawImage.texture;

            layer.Init();
            return layer;
        }

        public static Layer AddNewLayer(VisualTreeAsset vta)
        {
            var layer = AddNewLayer(vta.name);
            layer.visualTreeAsset = vta;
            return layer;
        }
    }
}
