using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProgrammingProject2
{
    static class Constants
    {
        public static Tuple<int, int> TRACKS_PER_FACE = new Tuple<int, int>(1,5000);

        public static Tuple<int, int> SECTORS_PER_TRACK = new Tuple<int, int>(1, 300);

        public static Tuple<double, double> TRANSFER_TIME = new Tuple<double, double>(0.5, 50);

        public static Tuple<double, double> ROTATION_TIME = new Tuple<double, double>(0.5, 200);

        public static Tuple<int, int> MAX_REQUESTS = new Tuple<int, int>(1, 45);

        public static Tuple<double, double> SEEK_SCALAR = new Tuple<double, double>(0.001, 2);

        public static Tuple<double, double> SEEK_CONSTANT = new Tuple<double,double>(1, 30000);

        public static Tuple <int, int> NUMBER_OF_DRIVES = new Tuple<int, int>(1,15);

        public static Tuple<int, int> ARRIVAL_TIME = new Tuple<int, int>(0,2000);

        public static Tuple<int, int> ALGORITHM_SWITCHES = new Tuple<int, int>(1, 5);
    }
}
