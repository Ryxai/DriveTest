using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProgrammingProject2
{
    using System.Linq.Expressions;
    using System.Reflection.Emit;
    using System.Threading;
    using System.Xml.Linq;

    public class Drive
    {
        public struct Stats
        {
            public readonly int TracksPerFace;

            public readonly int SectorsPerTrack;

            public readonly Func<int, double> SeekTimeFunc;

            public readonly double TransferTime;

            public readonly double RotationTime;

            public IEnumerable<string> AlgorithmExecutionList;  

            public int TrackPosition;

            public int SectorPosition;
            //True is + direction                              
            public bool HeadDirectionFlag;

            private double scalar;

            private double constant;

            public Stats(
                int tracksPerFace,
                int sectorsPerTrack,
                Func<int, double> seekTimeFunc,
                double transferTime,
                double rotationTime,
                IEnumerable<string> algorithmExecutionList, 
                int initialTrackPosition,
                int initialSectorPosition, 
                bool headDirectionFlag,
                double _constant, 
                double _scalar)
            {
                this.TracksPerFace = tracksPerFace;
                this.SectorsPerTrack = sectorsPerTrack;
                this.SeekTimeFunc = seekTimeFunc;
                this.TransferTime = transferTime;
                this.RotationTime = rotationTime;
                this.AlgorithmExecutionList = algorithmExecutionList;
                this.TrackPosition = initialTrackPosition;
                this.SectorPosition = initialSectorPosition;
                this.HeadDirectionFlag = headDirectionFlag;
                this.constant = _constant;
                this.scalar = _scalar;
            }


            public override string ToString()
            {
                var outputString = "Tracks: " + TracksPerFace + "\nSectors: " + SectorsPerTrack + "\nTransfer Time: "
                                   + TransferTime + "\nRotation Time: " + RotationTime + "\nSeekTime: " + constant
                                   + " + " + scalar + " * T";
                outputString += "\nAlgorithmOrder: ";
                return this.AlgorithmExecutionList.Aggregate(outputString, (current, algorithmName) => current + (algorithmName + " "));
            }

            public Stats CopyStats()
            {
                return new Stats(this.TracksPerFace, this.SectorsPerTrack, this.SeekTimeFunc, this.TransferTime, this.RotationTime, this.AlgorithmExecutionList, this.TrackPosition, this.SectorPosition, this.HeadDirectionFlag, this.constant, this.scalar);
            }
        }

        private Stats driveSpecs;

        private Request activeRequest;

        private Request mostRecentlyCompletedRequest;

        private Queue<Request> requestQueue;

        private double activeRequestCompletionTime;

        private ListEnumerator<string> algorithmEnumerator; 

        public Request ActiveRequest
        {
            get
            {
                return this.activeRequest;
            }
        }
        public double ActiveRequestCompletionTime 
        { 
            get
            {
                return this.activeRequestCompletionTime;
            } 
        }

        public int RequestsQueued
        {
            get
            {
                return this.requestQueue.Count;
            }
        }

        public ListEnumerator<string> AlgorithmEnumerator
        {
            get
            {
                if (this.algorithmEnumerator == null) this.algorithmEnumerator = new ListEnumerator<string>(driveSpecs.AlgorithmExecutionList);
                return this.algorithmEnumerator;
            }
        }  

        public Drive(XElement driveStats)
        {
            this.populateDriveSpecs(driveStats);
            this.requestQueue = new Queue<Request>();
            this.activeRequest = null;
            this.activeRequestCompletionTime = 0;
        }

        public void AddRequest(Request newRequest)
        {
            requestQueue.Enqueue(newRequest);
        }

        public Tuple<double, double> CompleteJob()
        {
            this.driveSpecs.TrackPosition = this.ActiveRequest.Track;
            this.driveSpecs.SectorPosition = this.ActiveRequest.Sector;
            var activeRequestArrivalTime = activeRequest.ArrivalTime;
            mostRecentlyCompletedRequest = activeRequest;
            this.activeRequest = null;
            return new Tuple<double, double>(this.activeRequestCompletionTime, activeRequestArrivalTime);
        }

        public void ActivateNewJob(Func<Queue<Request>, Stats, Queue<Request>> driveSeekAlgorithm, double time)
        {
            requestQueue = driveSeekAlgorithm(requestQueue, driveSpecs.CopyStats());
            activeRequest = requestQueue.Dequeue();
            activeRequestCompletionTime = time + this.calculateRequiredTimeForRequest();
        }

        public bool IsQueueEmpty()
        {
            return requestQueue.Count == 0;
        }

        private double calculateRequiredTimeForRequest()
        {
            var output = 
                this.driveSpecs.SeekTimeFunc(
                    Math.Abs(
                        this.activeRequest.Track
                        - (mostRecentlyCompletedRequest == null
                               ? driveSpecs.TrackPosition
                               : mostRecentlyCompletedRequest.Track)));

            var rotationTime = driveSpecs.RotationTime;
            output += rotationTime * (Math.Abs(this.activeRequest.Sector - (mostRecentlyCompletedRequest == null? driveSpecs.SectorPosition: mostRecentlyCompletedRequest.Sector)) / this.driveSpecs.RotationTime);
            output += this.driveSpecs.TransferTime;
            return output;
        }

        private void populateDriveSpecs(XElement specs)
        {
            this.driveSpecs = new Stats(
                int.Parse(specs.Element("Tracks").Value),
                int.Parse(specs.Element("Sectors").Value),
                (tracks) => double.Parse(specs.Element("SeekFunc").Element("Constant").Value)+ double.Parse(specs.Element("SeekFunc").Element("Scalar").Value)  * tracks,
                double.Parse(specs.Element("TransferTime").Value),
                double.Parse(specs.Element("RotationTime").Value),
                specs.Element("AlgorithmOrder").Elements("Type").Select(element => element.Value),
                int.Parse(specs.Element("InitialTrackPosition").Value),
                int.Parse(specs.Element("InitialSectorPosition").Value),
                bool.Parse(specs.Element("HeadDirection").Value),
                double.Parse(specs.Element("SeekFunc").Element("Constant").Value),
                double.Parse(specs.Element("SeekFunc").Element("Scalar").Value)
            );
        }

        public override string ToString()
        {
            return driveSpecs.ToString();
        }
    }

}