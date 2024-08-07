using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class EffectLayer : LayerBase
    {
        public override void Init()
        {
            base.Init();
            m_RawImage.material = Instantiate(LayerManager.BlurEffectMaterial);
            blur = 0f;
        }

        public override void ResetLayer()
        {
            base.ResetLayer();
        }
    }
}
