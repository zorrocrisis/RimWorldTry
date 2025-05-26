using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryRecognition
{
    internal class LogEventComparer: IEqualityComparer<LogEvent>
    {
        public bool Equals(LogEvent x, LogEvent y)
        {
            if (x == null || y == null)
                return false;

            return x.LogNameType == y.LogNameType &&
                   x.Timestamp == y.Timestamp &&
                   x.Location == y.Location &&
                   x.ID == y.ID &&
                   x.Description == y.Description &&
                   Enumerable.SequenceEqual(x.Actors ?? new List<string>(), y.Actors ?? new List<string>());
        }

        public int GetHashCode(LogEvent obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (obj.LogNameType?.GetHashCode() ?? 0);
                hash = hash * 23 + obj.Timestamp.GetHashCode();
                hash = hash * 23 + obj.Location.GetHashCode();
                hash = hash * 23 + (obj.ID?.GetHashCode() ?? 0);
                hash = hash * 23 + (obj.Description?.GetHashCode() ?? 0);

                if (obj.Actors != null)
                {
                    foreach (var actor in obj.Actors)
                        hash = hash * 23 + (actor?.GetHashCode() ?? 0);
                }

                return hash;
            }
        }
    }
}
