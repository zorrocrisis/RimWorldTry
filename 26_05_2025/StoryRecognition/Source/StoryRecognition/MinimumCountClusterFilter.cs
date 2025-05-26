using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryRecognition
{
    public class MinimumCountClusterFilter: IClusterer
    {
        
        public string Name => $"MinimumEventCountClusterer";

        private readonly int _minimumEventCount;
        bool _writeClusters;
        string _writeDestination;

        public MinimumCountClusterFilter(int minimumEventCount = 3, bool writeClusters = false, string writeDestination = "")
        {
            _minimumEventCount = minimumEventCount;
            _writeClusters = writeClusters;
            _writeDestination = writeDestination;
        }

        public List<List<LogEvent>> Cluster(List<LogEvent> events)
        {
            // This clusterer assumes input is already clustered.
            // So we just wrap the whole input as one cluster, and filter

            List<List<LogEvent>> clusters = new List<List<LogEvent>> { events };

            List<List<LogEvent>> result = clusters.Where(c => c.Count >= _minimumEventCount).ToList();

            if (_writeClusters && _writeDestination != "")
                ClustersUtil.WriteClusters(clusters, _writeDestination);

            return result;
        }
    }
}
