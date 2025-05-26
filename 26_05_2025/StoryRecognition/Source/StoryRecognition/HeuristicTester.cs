using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StoryRecognition
{
    public class HeuristicTester
    {
        TemporalClusterer temporalClusterer;
        SpatialClusterer spatialClusterer;
        ActorClusterer actorClusterer;

        string printDestination;

        List<LogEvent> logEvents;

        public HeuristicTester(List<LogEvent> events, string path)
        {
            logEvents = events;

            temporalClusterer = new TemporalClusterer();
            spatialClusterer = new SpatialClusterer();
            actorClusterer = new ActorClusterer();

            printDestination = path;
        }

        public void SetEventLog(List<LogEvent> events)
        {
            logEvents = events;
        }

        public void TestTemporalDistanceOnly(int secondsThreshold = 5)
        {
            // Tick
            int tickThreshold = secondsThreshold * 60;

            string path = Path.Combine(printDestination, "temporal_clustering.txt");

            // 1 second -> 60 ticks
            ClusterPipeline pipeline = new ClusterPipeline();
            pipeline.AddStep(new TemporalClusterer(5, true, path));
            pipeline.Run(logEvents, true, path);
        }

        public void TestSpatialDistanceOnly(int distanceThreshold = 5)
        {
            string path = Path.Combine(printDestination, "spatial_clustering.txt");

            ClusterPipeline pipeline = new ClusterPipeline();
            pipeline.AddStep(new SpatialClusterer(distanceThreshold, true, path));
            pipeline.Run(logEvents, true, path);
        }

        public void TestSharedActorsOnly(int minimumNumberOfSharedActors = 2)
        {
            string path = Path.Combine(printDestination, "shared_actors_clustering.txt");

            ClusterPipeline pipeline = new ClusterPipeline();
            pipeline.AddStep(new ActorClusterer(minimumNumberOfSharedActors, true, path));
            pipeline.Run(logEvents, true, path);
        }

        public void TestSpaceAndTime(int secondsThreshold = 5, int distanceThreshold = 5)
        {
            // Tick
            int tickThreshold = secondsThreshold * 60;

            string path = Path.Combine(printDestination, "space_time_clustering.txt");

            ClusterPipeline pipeline = new ClusterPipeline();
            pipeline.AddStep(new SpatialClusterer(distanceThreshold, false, path));
            pipeline.AddStep(new TemporalClusterer(secondsThreshold, false, path));
            pipeline.AddStep(new DeduplicationClusterFilter(false, path));

            pipeline.Run(logEvents, true, path);
        }

        public void TestSpaceAndTimeAndMinimumCount(int secondsThreshold = 5, int distanceThreshold = 5, int minimumNumberOfEvents = 3)
        {
            // Tick
            int tickThreshold = secondsThreshold * 60;

            string path = Path.Combine(printDestination, "space_time_count_clustering.txt");

            ClusterPipeline pipeline = new ClusterPipeline();
            pipeline.AddStep(new SpatialClusterer(distanceThreshold, false, path));
            pipeline.AddStep(new TemporalClusterer(secondsThreshold, false, path));
            pipeline.AddStep(new MinimumCountClusterFilter(minimumNumberOfEvents, false, path));
            pipeline.AddStep(new DeduplicationClusterFilter(false, path));

            pipeline.Run(logEvents, true, path);
        }


    }
}
