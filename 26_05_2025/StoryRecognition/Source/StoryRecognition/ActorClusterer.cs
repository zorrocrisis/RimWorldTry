using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryRecognition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ActorClusterer: IClusterer
    {
        public string Name => "ActorClusterer";

        int _minimumNumberOfSharedActors;
        bool _writeClusters;
        string _writeDestination;

        public ActorClusterer(int minimumNumberOfSharedActors = 2, bool writeClusters = false, string writeDestination = "")
        {
            _minimumNumberOfSharedActors = minimumNumberOfSharedActors;
            _writeClusters = writeClusters;
            _writeDestination = writeDestination;
        }

        public List<List<LogEvent>> Cluster(List<LogEvent> events)
        {

            List<List<LogEvent>> clusters = new List<List<LogEvent>>();

            foreach (LogEvent log in events)
            {
                if (log.Actors == null || log.Actors.Count == 0)
                    continue;

                bool addedToCluster = false;

                foreach (List<LogEvent> cluster in clusters)
                {
                    IEnumerable<string> sharedActors = log.Actors.Union(cluster.First().Actors, StringComparer.OrdinalIgnoreCase);

                    if (sharedActors.Count() >= _minimumNumberOfSharedActors)
                    {
                        cluster.Add(log);
                        addedToCluster = true;
                        break;
                    }
                }

                if (!addedToCluster)
                {
                    clusters.Add(new List<LogEvent> { log });
                }

            }

            // Only keeps the clusters with the minimum number of shared actors
            clusters = clusters
                        .Where(cluster => cluster
                        .SelectMany(log => log.Actors)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Count() >= _minimumNumberOfSharedActors)
                        .ToList();


            if (_writeClusters && !string.IsNullOrEmpty(_writeDestination))
                ClustersUtil.WriteClusters(clusters, _writeDestination);

            return clusters;
        }

        private string GetActorKey(List<string> actors)
        {
            return string.Join("|", actors.Select(a => a.ToLowerInvariant()).OrderBy(a => a));
        }

    }
}