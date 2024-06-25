using UnityEngine;

namespace Engine.Manager
{
    public class GPUInstanceEnabler : MonoBehaviour
    {
        void Awake()
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            if (TryGetComponent(out MeshRenderer meshRenderer))
            {
                meshRenderer.SetPropertyBlock(block);
            }
            else if (TryGetComponent(out SkinnedMeshRenderer skinnedRenderer))
            {
                skinnedRenderer.SetPropertyBlock(block);
            }
        }
    }
}