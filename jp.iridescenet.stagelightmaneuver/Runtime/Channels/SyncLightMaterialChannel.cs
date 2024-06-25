using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#if USE_HDRP
using UnityEngine.Rendering.HighDefinition;
#elif USE_URP
using UnityEngine.Rendering.Universal;
#endif



namespace StageLightManeuver
{
    [ExecuteAlways]
    [AddComponentMenu("")]
    public class SyncLightMaterialChannel : StageLightChannelBase
    {
#region DoNotSaveToProfile-Configs
        [ChannelField(true, false)] public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
#endregion


#region Configs
#if USE_HDRP
        [ChannelFieldBehavior(true)] public string materialPropertyName =  "_EmissiveColor";
#elif USE_URP
        [ChannelField(true)] public string materialPropertyName =  "_EmissionColor";
#endif
        [ChannelField(true)] public LightChannel lightChannel;
        [ChannelField(true)] public float maxIntensityLimit = 3;
        [ChannelField(true)] public int materialIndex = 0;
        [FormerlySerializedAs("lightChannelChannel")] [FormerlySerializedAs("lightFxChannel")]
#endregion

#region params
        [ChannelField(false)] public bool brightnessDecreasesToBlack = true;
        [ChannelField(false)] private Dictionary<MeshRenderer,MaterialPropertyBlock> _materialPropertyBlocks;
        [ChannelField(false)] public float intensityMultiplier = 1f;
#endregion


        private void Start()
        {
            Init();
        }

        private void OnEnable()
        {
            Init(); 
            lightChannel = GetComponent<LightChannel>();
        }

        [ContextMenu("GetMeshRenderer")]
        public void GetMeshRenderer()
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null && !meshRenderers.Contains(meshRenderer))
            {
                meshRenderers.Add(meshRenderer);
            }
        }
        
        [ContextMenu("GetMeshRenderersInChild")]
        public void GetMeshRenderersInChild()
        {
            var fetchMeshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var meshRenderer in fetchMeshRenderers)
            {
                if (!meshRenderers.Contains(meshRenderer))
                {
                    meshRenderers.Add(meshRenderer);
                }
            }
        }
        

        public override void Init()
        {
            if(_materialPropertyBlocks != null) _materialPropertyBlocks.Clear();
            _materialPropertyBlocks = new Dictionary<MeshRenderer, MaterialPropertyBlock>();

            foreach (var meshRenderer in meshRenderers)
            {
                var materialPropertyBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(materialPropertyBlock,materialIndex);
                _materialPropertyBlocks.Add(meshRenderer,materialPropertyBlock);
            }
            
            PropertyTypes.Add(typeof(SyncLightMaterialProperty));
        }
        public override void EvaluateQue(float currentTime)
        {
            if(meshRenderers == null || _materialPropertyBlocks == null) return;

            intensityMultiplier = 0f;
            maxIntensityLimit = 0f;
            while (stageLightDataQueue.Count>0)
            {
                
                var data = stageLightDataQueue.Dequeue();
                // var t=GetNormalizedTime(currentTime,data,typeof(SyncLightMaterialProperty));

                var syncLightMaterialProperty = data.TryGetActiveProperty<SyncLightMaterialProperty>();
                if(syncLightMaterialProperty != null)
                {
                    intensityMultiplier += syncLightMaterialProperty.intensitymultiplier.value * data.weight;
                    if(data.weight > 0.5f)
                    {
                        brightnessDecreasesToBlack = syncLightMaterialProperty.brightnessDecreasesToBlack.value;
                    }

                    maxIntensityLimit += syncLightMaterialProperty.maxIntensityLimit.value * data.weight;
                }

            }
            
            base.EvaluateQue(currentTime);

        }

        public override void UpdateChannel()
        {
            if(lightChannel == null) return;
            if (_materialPropertyBlocks == null|| _materialPropertyBlocks.Count != meshRenderers.Count)
            {
                Init();
            }
            
            var intensity = Mathf.Min(lightChannel.lightIntensity * intensityMultiplier,maxIntensityLimit);
            var hdrColor = SlmUtility.GetHDRColor(lightChannel.lightColor, intensity);
            var result = brightnessDecreasesToBlack ? Color.Lerp(Color.black,hdrColor, Mathf.Clamp(intensity, 0, 1f)) : hdrColor;

            foreach (var materialPropertyBlock in _materialPropertyBlocks)
            {
                materialPropertyBlock.Value.SetColor(materialPropertyName,result);
                materialPropertyBlock.Key.SetPropertyBlock(materialPropertyBlock.Value,materialIndex);
            }
        }
    }

}
