using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class EffectLayer : LayerBase
    {
        IEffect m_Effect;

        public IEffect effect
        {
            get => m_Effect;
            set
            {
                m_Effect = value;
                m_RawImage.material = m_Effect != null ? m_Effect.material : null;
            }
        }
    }
}
