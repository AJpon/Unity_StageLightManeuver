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
        [SerializeField] private int materialIndex = 0;
        public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
#if USE_HDRP
        public string materialPropertyName =  "_EmissiveColor";
#elif USE_URP
        public string materialPropertyName =  "_EmissionColor";
#endif
        
        public float intensityMultiplier = 1f;
        public float maxIntensityLimit = 3;
        public bool brightnessDecreasesToBlack = true;
        private Dictionary<MeshRenderer,MaterialPropertyBlock> _materialPropertyBlocks;
        [FormerlySerializedAs("lightChannelChannel")] [FormerlySerializedAs("lightFxChannel")]
        public LightChannel lightChannel;
        private void Start()
        {
            Init();
        }

        private void OnEnable()
        {
            Init(); 
            lightChannel = GetComponent<LightChannel>();
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
            var hdrColor = SlmUtility.GetHDRColor(lightChannel.lightColor,
                intensity);
            var result = brightnessDecreasesToBlack ? Color.Lerp(Color.black,hdrColor, Mathf.Clamp(intensity, 0, 1f)) : hdrColor;

            foreach (var materialPropertyBlock in _materialPropertyBlocks)
            {
                materialPropertyBlock.Value.SetColor(materialPropertyName,result);
                materialPropertyBlock.Key.SetPropertyBlock(materialPropertyBlock.Value,materialIndex);
            }
        }
    }

}
