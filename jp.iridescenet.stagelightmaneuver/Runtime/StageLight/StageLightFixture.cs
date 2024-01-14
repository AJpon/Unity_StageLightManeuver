using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StageLightManeuver
{
    [ExecuteAlways]
    public class StageLightFixture: StageLightFixtureBase,IStageLightFixture
    {
        [SerializeReference] private List<StageLightChannelBase> stageLightChannels = new List<StageLightChannelBase>();
        public List<StageLightChannelBase> StageLightChannels { get => stageLightChannels; set => stageLightChannels = value; }
 
        public int order = 0;
        [ContextMenu("Init")]
        public override void Init()
        {
            FindChannels();
            stageLightChannels.Sort( (a,b) => a.updateOrder.CompareTo(b.updateOrder));
            foreach (var stageLightChannel in StageLightChannels)
            {
                stageLightChannel.Init();
                stageLightChannel.parentStageLightFixture = this;
            }

            stageLightFixtures = new List<StageLightFixture>() { this };
        }


        private void Start()
        {
            Init();
        }

        public override void AddQue(StageLightQueueData stageLightQueData)
        {
            // base.AddQue(stageLightQueData);
            foreach (var stageLightChannel in StageLightChannels)
            {
                if(stageLightChannel != null)stageLightChannel.stageLightDataQueue.Enqueue(stageLightQueData);
            }
        }

        public override void EvaluateQue(float time)
        {
            // base.EvaluateQue(time);
            foreach (var stageLightChannel in StageLightChannels)
            {
                if (stageLightChannel != null)
                {
                    stageLightChannel.EvaluateQue(time);
                    // stageLightChannel.Index = Index;
                }
            }
        }

        public override void UpdateChannel()
        {
            if(stageLightChannels == null) stageLightChannels = new List<StageLightChannelBase>();
            foreach (var stageLightChannel in stageLightChannels)
            {
                if(stageLightChannel)stageLightChannel.UpdateChannel();
            }
        }


        private void Update()
        {
            // UpdateChannel();
        }

        private void OnDestroy()
        {
            // Debug.Log("On Destroy");
            // for (int i = stageLightChannels.Count-1; i >=0; i--)
            // {
            //     try
            //     {
            //         if(stageLightChannels[i]!= null)DestroyImmediate(stageLightChannels[i]);
            //     }
            //     catch (Exception e)
            //     {
            //         Console.WriteLine(e);
            //         throw;
            //     }
            // }
        }


        [ContextMenu("Find Channels")]
        public void FindChannels()
        {
            if (stageLightChannels != null)
            {
                StageLightChannels.Clear();
            }
            else
            {
                stageLightChannels = new List<StageLightChannelBase>();
            }
            StageLightChannels = GetComponents<StageLightChannelBase>().ToList();
        }
    }
}