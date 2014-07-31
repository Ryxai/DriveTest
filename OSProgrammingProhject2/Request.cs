using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProgrammingProject2
{
    using System.Xml.Linq;

    public class Request
    {
        public int ArrivalTime { get; private set; }

        public int Track{ get; private set; }

        public int Sector { get; private set; }

        public Request(XElement request)
        {
            ArrivalTime = int.Parse(request.Element("ArrivalTime").Value);
            Track = int.Parse(request.Element("Track").Value);
            Sector = int.Parse(request.Element("Sector").Value);;
        }

        public Request(int arrivalTime, int track, int sector)
        {
            ArrivalTime = arrivalTime;
            Track = track;
            Sector = sector;
        }
    }
}
