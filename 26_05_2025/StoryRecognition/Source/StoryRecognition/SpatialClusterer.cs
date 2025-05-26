using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryRecognition
{
    internal class SpatialClusterer: IClusterer
    {
        public string Name => "SpatialClusterer";

        double _threshold;
        bool _writeClusters;
        string _writeDestination;

        public SpatialClusterer(double threshold = 5.0, bool writeClusters = false, string writeDestination = "")
        {
            _threshold = threshold;
            _writeClusters = writeClusters;
            _writeDestination = writeDestination;
        }

        public List<List<LogEvent>> Cluster(List<LogEvent> events)
        {
            List<LogEvent> sorted = events.OrderBy(e => e.Timestamp).ToList(); // Optional: sort by time for consistency
            List<List<LogEvent>> clusters = new List<List<LogEvent>>();
            List<LogEvent> currentCluster = new List<LogEvent>();

            foreach (LogEvent log in sorted)
            {
                if (currentCluster.Count == 0)
                {
                    currentCluster.Add(log);
                    continue;
                }

                LogEvent last = currentCluster.Last();
                double dist = SpatialDistance(log, last);

                if (dist <= _threshold)
                {
                    currentCluster.Add(log);
                }
                else
                {
                    clusters.Add(new List<LogEvent>(currentCluster));
                    currentCluster.Clear();
                    currentCluster.Add(log);
                }
            }

            if (currentCluster.Count > 0)
                clusters.Add(currentCluster);


            if (_writeClusters && _writeDestination != "")
                ClustersUtil.WriteClusters(clusters, _writeDestination);

            return clusters;
        }



        private double SpatialDistance(LogEvent a, LogEvent b)
        {
            int dx = a.Location.x - b.Location.x;
            int dy = a.Location.y - b.Location.y;
            int dz = a.Location.z - b.Location.z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
