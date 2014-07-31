using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestsOSProject2
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using OSProgrammingProject2;
    [TestClass]
    public class SortingAlgorithmTests
    {
        private static Request[] array = {new Request(1,3,4), new Request(10, 7, 4), new Request(2,3,5), new Request(30,100, 20), new Request(100, 90, 30), new Request(47, 20, 20)};

        private static Func<Request, Request, bool> comparisonFunc = ((request, request1) => request.ArrivalTime == request1.ArrivalTime && request.Track == request1.Track && request.Sector == request1.Sector); 
        [TestMethod]
        public void FCFS_Test()
        {
            Queue<Request> queue = new Queue<Request>(array);
            Queue<Request> resultQueue = new Queue<Request>(array);
            queue = RequestAlgorithms.FCFS(queue, new Drive.Stats(100, 50, i => 10 + i * 0.2, 0.3, 0.5, null, 0, 0, true, 10, 0.2));
            Assert.IsTrue(resultQueue.OrderBy(request => request.ArrivalTime).Zip(queue, (request, request1) => comparisonFunc(request, request1)).Aggregate((b, b1) => b && b1));
        }

        [TestMethod]                                                                  
        public void SSTF_Test()
        {
            Queue<Request> queue = new Queue<Request>(array);
            queue = RequestAlgorithms.SSTF(
                queue,
                new Drive.Stats(100, 50, i => 10 + i * 0.2, 0.3, 0.5, null, 0, 0, true, 10, 0.2));
            var expected = new Request[] { array[0], array[2], array[1], array[5], array[4], array[3] };
            Assert.IsTrue(expected.Zip(queue, comparisonFunc).Aggregate((b, b1) => b && b1));
        }

        [TestMethod]
        public void LOOK_Test()
        {
            Queue<Request> queue = new Queue<Request>(array);
            var expected1 = new Request[] { array[5], array[4], array[3], array[1], array[0], array[2] };
            var expected2 = new Request[] { array[1], array[0], array[2], array[5], array[4], array[3] };
            var queue1 = RequestAlgorithms.LOOK(
                queue,
                new Drive.Stats(100, 50, i => 10 + i * 0.2, 0.3, 0.5, null, 15, 0, true, 10, 0.2));
            var queue2 = RequestAlgorithms.LOOK(
                queue,
                new Drive.Stats(100, 50, i => 10 + i * 0.2, 0.3, 0.5, null, 15, 0, false, 10, 0.2));
            Assert.IsTrue(expected1.Zip(queue1,comparisonFunc).Aggregate((b, b1) => b && b1));
            Assert.IsTrue(expected2.Zip(queue2,comparisonFunc).Aggregate((b, b1) => b && b1));
        }


        [TestMethod]
        public void CLOOK_Test()
        {
            var queue = new Queue<Request>(array);
            var expected = new Request[] { array[5], array[4], array[3], array[0], array[2], array[1] };
            queue = RequestAlgorithms.CLOOK(
                queue,
                new Drive.Stats(100, 50, i => 10 + i * 0.2, 0.3, 0.5, null, 15, 0, false, 10, 0.2));
            Assert.IsTrue(expected.Zip(queue, comparisonFunc).Aggregate((b, b1) => b && b1));
        }
    }
}
