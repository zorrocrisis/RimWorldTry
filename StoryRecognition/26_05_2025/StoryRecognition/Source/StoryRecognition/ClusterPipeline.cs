using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StoryRecognition
{
    internal class ClusterPipeline
    {
        private readonly List<IClusterer> steps = new List<IClusterer>();


        public void AddStep(IClusterer clusterer)
        {
            steps.Add(clusterer);
        }

        public List<List<LogEvent>> Run(List<LogEvent> initialEvents, bool writeToFile = false, string writeDestination = "")
        {
            var currentClusters = new List<List<LogEvent>> { initialEvents };


            foreach (var step in steps)
            {
                List<List<LogEvent>> nextClusters = new List<List<LogEvent>>();

                foreach (var cluster in currentClusters)
                {
                    List<List<LogEvent>> result = step.Cluster(cluster.Select(e => e).ToList());
                    nextClusters.AddRange(result);
                }

                currentClusters = nextClusters;

                if (writeToFile)
                {
                    ClustersUtil.WriteClustersAfterPipelineStep(currentClusters, writeDestination, step.Name);
                }
            }

            return currentClusters;
        }
    }
}
