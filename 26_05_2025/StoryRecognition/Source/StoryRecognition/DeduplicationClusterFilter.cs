using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryRecognition
{
    internal class DeduplicationClusterFilter: IClusterer
    {

        public string Name => $"DeduplicationClusterFilter";

        bool _writeClusters;
        string _writeDestination;

        public DeduplicationClusterFilter(bool writeClusters = false, string writeDestination = "")
        {
            _writeClusters = writeClusters;
            _writeDestination = writeDestination;
        }

        public List<List<LogEvent>> Cluster(List<LogEvent> events)
        {
            // This clusterer assumes input is already clustered.
            // So we just wrap the whole input as one cluster, and filter

            List<List<LogEvent>> clusters = new List<List<LogEvent>> { events };

            List<List<LogEvent>> result = clusters
                                        .Select(cluster => cluster
                                        .Distinct(new LogEventComparer())
                                        .ToList())
                                        .ToList();

            if (_writeClusters && _writeDestination != "")
                ClustersUtil.WriteClusters(clusters, _writeDestination);

            return result;
        }
    }


}
