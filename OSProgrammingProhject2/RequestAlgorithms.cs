using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProgrammingProject2
{
    using System.Collections;

    public static class RequestAlgorithms
    {
        public static Queue<Request> FCFS(Queue<Request> requestQueue, Drive.Stats driveStatistics)
        {
            if (requestQueue.Count <= 1) 
                return new Queue<Request>(requestQueue);
            return new Queue<Request>(requestQueue.OrderBy(request => request.ArrivalTime));
        }

        public static Queue<Request> SSTF(Queue<Request> requestQueue, Drive.Stats driveStatistics)
        {
            if (requestQueue.Count <= 1)
                return new Queue<Request>(requestQueue);
            return
                new Queue<Request>(
                    requestQueue.OrderBy(request => Utilities.RequestDistance(request, driveStatistics)));
        }

        public static Queue<Request> LOOK(Queue<Request> requestQueue, Drive.Stats driveStatistics)
        {
            if (requestQueue.Count <= 1)
                return new Queue<Request>(requestQueue);
            Func<int, int, bool> comparisonFunc = driveStatistics.HeadDirectionFlag ? (Func<int, int, bool>)((a,b) => a > b) : ((a, b) => a < b);
            var requestsInDirectPath =
                requestQueue.Where(request => comparisonFunc(request.Track, driveStatistics.TrackPosition));
                    
            var requestsInOppositePath = requestQueue.Except(requestsInDirectPath);
            requestsInDirectPath=
                requestsInDirectPath.OrderBy(
                    request => Utilities.RequestDistance(request, driveStatistics));
            requestsInOppositePath= requestsInOppositePath.OrderBy(request => Utilities.RequestDistance(request,(requestsInDirectPath.Count() == 0) ? new Request(0,(driveStatistics.HeadDirectionFlag) ? driveStatistics.TracksPerFace : 0, 0) : requestsInDirectPath.Last(), driveStatistics));
            return new Queue<Request>(requestsInDirectPath.Concat(requestsInOppositePath));
        }

        public static Queue<Request> CLOOK(Queue<Request> requestQueue, Drive.Stats driveStatistics)
        {
            if (requestQueue.Count <= 1)
                return new Queue<Request>(requestQueue);
            var requestsAbove =
                requestQueue.Where(
                    request =>
                    request.Track > driveStatistics.TrackPosition && request.Sector > driveStatistics.SectorPosition);
            var requestsBelow = requestQueue.Except(requestsAbove);
            return
                new Queue<Request>(requestsAbove.OrderBy(request => Utilities.RequestDistance(request, driveStatistics))
                    .Concat(
                        requestsBelow.OrderBy(
                            request => Utilities.RequestDistance(request.Track, 0, request.Sector, 0, driveStatistics))));
        }
    }
}
