﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageLightManeuver
{
    [ExecuteAlways]
    [AddComponentMenu("")]
    public class SmoothLookAtChannel:StageLightChannelBase
    {
#region DoNotSaveToProfile-Configs
        [ChannelField(true, false)] public List<Transform> targetList = new List<Transform>();
        [ChannelField(true, false)] public SmoothLookAt smoothLookAt;
#endregion


#region Configs
        [ChannelField(true)] public float autoDisableTime = 1f;
#endregion


#region params
        [ChannelField(false)] private float _autoDisableTime = 0f;
        [ChannelField(false)] private int _targetIndex = 0;
        
        [ChannelField(false)] public bool autoDisableLookAt = true;
#endregion


        public override void EvaluateQue(float currentTime)
        {
            base.EvaluateQue(currentTime);
            _targetIndex = 0;
            while (stageLightDataQueue.Count >0)
            {
                var queueData = stageLightDataQueue.Dequeue();
                var smoothLookAtProperty = queueData.TryGetActiveProperty<SmoothLookAtProperty>() as SmoothLookAtProperty;
                if (smoothLookAtProperty == null) continue;
                var weight = queueData.weight;
                if (weight >= 0.5f) _targetIndex = smoothLookAtProperty.targetIndex.value;
                _autoDisableTime = autoDisableTime;
            }
        }

        public override void UpdateChannel()
        {
            base.UpdateChannel();
            if(smoothLookAt == null) return;
            if (_targetIndex == 0)
            {
                smoothLookAt.target = null;
            }
            else if(_targetIndex > 0)
            {
                if (targetList.Count > (_targetIndex - 1))
                {
                    smoothLookAt.target = targetList[_targetIndex - 1];
                }
            }
            
        }

        private void LateUpdate()
        {
            if(!autoDisableLookAt) return;
            if (smoothLookAt == null) return;
            if (stageLightDataQueue.Count == 0)
            {
                if (_autoDisableTime > 0f) _autoDisableTime -= Time.deltaTime;
            }
            else
            {
                _autoDisableTime = autoDisableTime;
            }
            
            if (_autoDisableTime <= 0f)
            {
                smoothLookAt.target = null;
            }
        }

        public override void Init()
        {
            base.Init();
            PropertyTypes.Add(typeof(SmoothLookAtProperty));
            
        }
    }
}