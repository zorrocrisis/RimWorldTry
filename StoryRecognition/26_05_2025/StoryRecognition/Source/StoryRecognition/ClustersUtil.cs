using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryRecognition
{
    public static class ClustersUtil
    {
        public static void WriteClusters(List<List<LogEvent>> clusters, string printDestination)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Number of clusters: {clusters.Count}");
            sb.AppendLine();

            for (int i = 0; i < clusters.Count; i++)
            {
                sb.AppendLine($"--- Cluster {i + 1} ---");

                foreach (var ev in clusters[i])
                {
                    sb.AppendLine($"- {ev}");
                }

                sb.AppendLine(); // Separate clusters
            }

            File.WriteAllText(printDestination, sb.ToString());
        }


        public static void WriteClustersAfterPipelineStep(List<List<LogEvent>> clusters, string printDestination, string stepName)
        {
            StringBuilder sb = new StringBuilder();


            sb.AppendLine($"--- After {stepName} ---");
            sb.AppendLine();

            sb.AppendLine($"Number of clusters: {clusters.Count}");
            sb.AppendLine();

            for (int i = 0; i < clusters.Count; i++)
            {
                sb.AppendLine($"--- Cluster {i + 1} ---");

                foreach (var ev in clusters[i])
                {
                    sb.AppendLine($"- {ev}");
                }

                sb.AppendLine(); // Separate clusters
            }

            File.AppendAllText(printDestination, sb.ToString());
        }

    }
}
