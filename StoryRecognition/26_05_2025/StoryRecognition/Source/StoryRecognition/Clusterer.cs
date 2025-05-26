using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryRecognition
{
    public interface IClusterer
    {
        List<List<LogEvent>> Cluster(List<LogEvent> events);
        string Name { get; }

    }
}
