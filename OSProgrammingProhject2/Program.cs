using System;
using System.Collections.Generic;
using System.Linq;

namespace OSProgrammingProject2
{
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Linq;

    class Program
    {
        private static Request[] requestCollection;

        private static List<Drive> drives = new List<Drive>();

        static void Main(string[] args)
        {
            initalize(args);
            var immediateResults = drives.Select(simulateDrive).ToList();
            var processedResults = immediateResults.Select(processResults);
            drives.Zip(
                processedResults,
                (drive, tuple) => new Tuple<Drive, double, double, double>(drive, tuple.Item1, tuple.Item2, tuple.Item3))
                .ForEach(
                    tuple =>
                    Console.WriteLine(
                        "*************DRIVE*******\n" + tuple.Item1.ToString() + "\nAverage: " + tuple.Item2
                        + "\nVariance: " + tuple.Item3 + "\nStandard Deviation: " + tuple.Item4));
            Console.ReadLine();
        }

        static void initalize(string[]  args)
        {
            XDocument doc;
            //If an input file isn't specified then generate a random file (or if difficulty loading)
            if (args.Length > 0)
            {
              doc = XDocument.Load(args[0]);
            }
            else
            {
                doc = generateRandom();
            }
            var driveSpecifications = doc.Root.Element("DriveSpecs");
            var requests = doc.Root.Element("Requests");
            initializeDrives(driveSpecifications);
            populateJobList(requests);
        }

        static XDocument generateRandom()
        {
            var document = new XDocument(new XElement("Simulation"));
            document.Root.Add(new XElement("DriveSpecs"), new XElement("Requests"));
            var driveSpecs = document.Root.Element("DriveSpecs");
            var randomGen = new Random();
            driveSpecs.Add(new XElement("NumberOfDrives", randomGen.Next(Constants.NUMBER_OF_DRIVES.Item1, Constants.NUMBER_OF_DRIVES.Item2)));
            var numOfTracks = randomGen.Next(Constants.TRACKS_PER_FACE.Item1, Constants.TRACKS_PER_FACE.Item2);
            var numOfSectors = randomGen.Next(Constants.SECTORS_PER_TRACK.Item1, Constants.SECTORS_PER_TRACK.Item2);
            for (int i = 0; i < int.Parse(document.Root.Element("DriveSpecs").Element("NumberOfDrives").Value); i++)
            {
                var drive = new XElement("Drive");
                drive.Add(
                    new XElement("Tracks", numOfTracks),
                    new XElement("Sectors", numOfSectors),
                    new XElement(
                        "RotationTime",
                        Constants.ROTATION_TIME.Item1
                        + (Constants.ROTATION_TIME.Item2 - Constants.ROTATION_TIME.Item1) * randomGen.NextDouble()),
                    new XElement(
                        "TransferTime",
                        Constants.TRANSFER_TIME.Item1
                        + (Constants.TRANSFER_TIME.Item2 - Constants.TRANSFER_TIME.Item2) * randomGen.Next()),
                    new XElement(
                        "SeekFunc",
                        new XElement(
                            "Scalar",
                            Constants.SEEK_SCALAR.Item1
                            + (Constants.SEEK_SCALAR.Item2 - Constants.SEEK_SCALAR.Item1) * randomGen.NextDouble()),
                        new XElement("Constant",
                        Constants.SEEK_CONSTANT.Item1
                        + (Constants.SEEK_CONSTANT.Item2 - Constants.SEEK_CONSTANT.Item1) * randomGen.NextDouble())));
                drive.Add(
                    new XElement("InitialTrackPosition", randomGen.Next(int.Parse(drive.Element("Tracks").Value))),
                    new XElement(
                        "InitialSectorPosition",
                        randomGen.Next(int.Parse(drive.Element("Sectors").Value))),
                    new XElement("HeadDirection", randomGen.Next() % 1 == 0));
                drive.Add(new XElement("AlgorithmOrder"));
                for (int j = 0;
                     j < randomGen.Next(Constants.ALGORITHM_SWITCHES.Item1, Constants.ALGORITHM_SWITCHES.Item2);
                     j++)
                {
                    drive.Element("AlgorithmOrder")
                        .Add(new XElement("Type", (new[] { "FCFS", "SSTF", "LOOK", "CLOOK" })[randomGen.Next(3)]));
                }
                driveSpecs.Add(drive);
                
            }

            var requests = document.Root.Element("Requests");
            for (int i = 0; i < randomGen.Next(Constants.MAX_REQUESTS.Item1, Constants.MAX_REQUESTS.Item2); i++)
            {
                requests.Add(new XElement("Request",
                    new XElement("ArrivalTime", (i == 0) ? 0 : randomGen.Next(Constants.ARRIVAL_TIME.Item1, Constants.ARRIVAL_TIME.Item2)),
                    new XElement("Track", randomGen.Next(Constants.TRACKS_PER_FACE.Item1, int.Parse(driveSpecs.Element("Drive").Element("Tracks").Value))),
                    new XElement("Sector", randomGen.Next(Constants.SECTORS_PER_TRACK.Item1, int.Parse(driveSpecs.Element("Drive").Element("Sectors").Value)))));
            }
            using (XmlWriter doc = XmlWriter.Create("testdata.xml"))
            {
                document.WriteTo(doc);  
            }
            return document;
        }

        static void initializeDrives(XElement specs)
        {
            specs.Elements("Drive").ForEach(element => drives.Add(new Drive(element)));
        }

        static void populateJobList(XElement requests)
        {
            requestCollection = requests.Elements("Request").Select(request => new Request(request)).OrderBy(request => request.ArrivalTime).ToArray();
        }

        static double[] simulateDrive(Drive drive)
        {
            var outputMetricList = new List<double>();
            drive.AlgorithmEnumerator.Reset();
            for (double time = 0, requestsRemaining = requestCollection.Length;
                 requestsRemaining > 0 || drive.ActiveRequest != null || drive.RequestsQueued > 0;)
            {
                if (drive.ActiveRequest != null)
                {
                    var completionData = drive.CompleteJob();
                    time = completionData.Item1;
                    outputMetricList.Add(time - completionData.Item2);
                    if (requestsRemaining == 0) break;
                }
                requestCollection.Where(
                    request =>
                    (requestCollection.ElementAt(requestCollection.Count() - Convert.ToInt32(requestsRemaining))
                         .ArrivalTime <= request.ArrivalTime) && request.ArrivalTime <= time).ForEach(
                             request =>
                                 {
                                     requestsRemaining--;
                                     drive.AddRequest(request);
                                 });
                if (drive.IsQueueEmpty())
                {
                    time++;
                    continue;
                }
                if (drive.AlgorithmEnumerator.Current != null || drive.AlgorithmEnumerator.MoveNext())
                {
                    Func<Queue<Request>, Drive.Stats, Queue<Request>> seekAlgorithm = null;
                    switch (drive.AlgorithmEnumerator.Current)
                    {
                        case "FCFS":
                            seekAlgorithm = RequestAlgorithms.FCFS;
                            break;
                        case "SSTF":
                            seekAlgorithm = RequestAlgorithms.SSTF;
                            break;
                        case "LOOK":
                            seekAlgorithm = RequestAlgorithms.LOOK;
                            break;
                        case "CLOOK":
                            seekAlgorithm = RequestAlgorithms.CLOOK;
                            break;
                    }
                    drive.ActivateNewJob(seekAlgorithm, time);
                }
            }
            return outputMetricList.ToArray();
        }

        static Tuple<double, double, double> processResults(double[] inputArray)
        {
            return new Tuple<double, double, double>(inputArray.Average(), inputArray.Variance(), Math.Sqrt(inputArray.Variance()));
        }
    }
}
