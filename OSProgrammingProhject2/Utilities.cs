using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProgrammingProject2
{
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    public static class Utilities
    {
        public static double RequestDistance(Request request1, Request request2, Drive.Stats driveSpecifications)
        {
            return driveSpecifications.SeekTimeFunc(Math.Abs(request1.Track - request2.Track))
                   + (Math.Abs(request1.Sector - request2.Sector) / driveSpecifications.SectorsPerTrack)
                   * driveSpecifications.RotationTime;
        }

        public static double RequestDistance(
            int trackRequest1,
            int sectorRequest1,
            int trackRequest2,
            int sectorRequest2, 
            Drive.Stats driveSpecifications)
        {
            return driveSpecifications.SeekTimeFunc(Math.Abs(trackRequest1 - trackRequest2))
                   + (Math.Abs(sectorRequest1 - sectorRequest2) / driveSpecifications.SectorsPerTrack)
                   * driveSpecifications.RotationTime;
        }

        public static double RequestDistance(Request request, Drive.Stats driveSpecifications)
        {
            return driveSpecifications.SeekTimeFunc(Math.Abs(request.Track - driveSpecifications.TrackPosition))
                   + (Math.Abs(request.Sector - driveSpecifications.SectorPosition)
                      / driveSpecifications.SectorsPerTrack) * driveSpecifications.RotationTime;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var element in enumerable)
            {
                action(element);
            }
        }

        public static double Variance(this IEnumerable<double> Idoubles)
        {
            return Idoubles.Aggregate((d, d1) => d + Math.Pow(d1 - Idoubles.Average(), 2)) / (Idoubles.Count() - 1);
        }
    }
}
