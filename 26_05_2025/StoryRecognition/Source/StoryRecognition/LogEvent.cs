using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StoryRecognition
{

    public class LogEvent
    {
        public string LogNameType { get; set; }
        public int Timestamp { get; set; }
        public (int x, int y, int z) Location { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        public List<string> Actors { get; set; }

        public override string ToString()
        {
            return $"[{Timestamp} @ ({Location.x},{Location.y},{Location.z}) {LogNameType}{ID}] {Description}";
        }

        public string GetActorsAsString()
        {
            return $"Actors: {string.Join(";", Actors)}";
        }
    }
}
