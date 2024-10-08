using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Layers
{
    public class CustomLayer : BaseLayer
    {
        Material m_Material;

        public Material material => m_Material;

        public void Init(Material material)
        {
            base.Init();
            m_Material = material;
        }

        void OnDestroy()
        {
            Destroy(material);
        }
    }
}
