using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Project1Adams
{
    class Program
    {
        //file location where I store all TSPs
        static string tspDirectory = "../../TSPs/";
        //alloted city connections
        static Dictionary<int, List<int>> cityPaths = new Dictionary<int, List<int>>()
        {
            { 1, new List<int>(){2,3,4}},
            { 2, new List<int>(){3}},
            { 3, new List<int>(){4,5}},
            { 4, new List<int>(){5,6,7}},
            { 5, new List<int>(){7,8}},
            { 6, new List<int>(){8}},
            { 7, new List<int>(){9,10}},
            { 8, new List<int>(){9,10,11}},
            { 9, new List<int>(){11}},
            { 10, new List<int>(){11}},
        };
        static List<Coordinate> coordinates = new List<Coordinate>();

        //track shortest route and cost of breadth first
        static double lowestDistanceBFS = -1;
        static List<Coordinate> shortestOrderBFS = new List<Coordinate>();

        //track shortest route and cost of depth first
        static double lowestDistanceDFS = -1;
        static List<Coordinate> shortestOrderDFS = new List<Coordinate>();

        static void Main(string[] args)
        {
            //loop through all files
            //in this case it's just the one file
            foreach (var file in Directory.GetFiles(tspDirectory).OrderByDescending(x => x))
            {
                var lines = File.ReadAllLines(file);

                //get all lines after the 7th line since that's where coordinates start
                for (var i = 7; i < lines.Count(); i++)
                {
                    //split out line into coordinate class I created
                    var coordsText = lines[i].Split(' ');
                    coordinates.Add(new Coordinate() { Id = Convert.ToInt32(coordsText[0]), Latitude = Convert.ToDouble(coordsText[1]), Longitude = Convert.ToDouble(coordsText[2]) });
                }

                var startingStep = new List<Coordinate>() { coordinates[0] };
                double startingDistance = 0;
                int goalCity = 11;

                var startingTimeBFS = DateTime.Now;
                BreadthFirstSearch(new List <BreadthPath>() { new BreadthPath() { currentSteps = startingStep, overallDistance = startingDistance }}, goalCity);
                var EndingTimeBFS = DateTime.Now;

                //print out shortest distance of the breadth first algorithm and make the list readable and print out as well
                Console.WriteLine("Shortest distance using the Breadth First Algorithm for \"" + Path.GetFileName(file) + "\" is " + Math.Round(lowestDistanceBFS, 2).ToString());
                var coordString = "";
                foreach (var coord in shortestOrderBFS)
                {
                    coordString += coord.Id.ToString() + ", ";
                }
                Console.WriteLine("In order of Id's: " + coordString.Substring(0, coordString.Length - 2));
                Console.WriteLine("With the time of " + (startingTimeBFS - EndingTimeBFS).ToString("ffffff") + " microseconds");
                Console.WriteLine();


                var startingTimeDFS = DateTime.Now;
                DepthFirstSearch(startingStep, startingDistance, goalCity);
                var EndingTimeDFS = DateTime.Now;

                //print out shortest distance of the depth first algorithm and make the list readable and print out as well
                Console.WriteLine("Shortest distance using the Depth First Algorithm for \"" + Path.GetFileName(file) + "\" is " + Math.Round(lowestDistanceDFS, 2).ToString());
                coordString = "";
                foreach (var coord in shortestOrderDFS)
                {
                    coordString += coord.Id.ToString() + ", ";
                }
                Console.WriteLine("In order of Id's: " + coordString.Substring(0, coordString.Length - 2));
                Console.WriteLine("With the time of " + (startingTimeDFS - EndingTimeDFS).ToString("ffffff") + " microseconds");
            }
            //pause
            Console.ReadKey();
        }

        //use BreadthFirstSearch recursively to find the shortest distance
        private static void BreadthFirstSearch(List<BreadthPath> currentBreadthPaths, int goalCity)
        {
            //the paths that will be passed into the next call of this function
            List<BreadthPath> newBreadthPaths = new List<BreadthPath>();
            //loop through the existing paths
            foreach (var breadthPath in currentBreadthPaths)
            {
                //grab which city the algorithm is currently on
                int currentCity = breadthPath.currentSteps[breadthPath.currentSteps.Count - 1].Id;

                //if it's on the goal city, go to the next path
                if (currentCity == goalCity)
                    continue;

                //foreach potential next step, track the distance and the path
                foreach (var step in cityPaths[currentCity])
                {
                    BreadthPath newBP = new BreadthPath() { currentSteps = new List<Coordinate>(breadthPath.currentSteps), overallDistance = breadthPath.overallDistance};
                    newBP.currentSteps.Add(coordinates.First(x => x.Id == step));
                    newBP.overallDistance += DistanceBetween(coordinates.First(x => x.Id == currentCity), coordinates.First(x => x.Id == step));
                    newBreadthPaths.Add(newBP);

                    //if this path reached the goal city, check if it's the shortest path so far
                    if(step == goalCity)
                    {
                        if (lowestDistanceBFS < 0 || newBP.overallDistance < lowestDistanceBFS)
                        {
                            lowestDistanceBFS = newBP.overallDistance;
                            shortestOrderBFS = newBP.currentSteps;
                        }
                    }
                }
            }
            //check if there are more ongoing paths to see if continuing is needed
            if(newBreadthPaths.Count > 0)
                BreadthFirstSearch(newBreadthPaths, goalCity);
        }

        //use DepthFirstSearch recursively to find the shortest distance
        private static void DepthFirstSearch(List<Coordinate> currentSteps, double overallDistance, int goalCity)
        {
            //grab which city that the algorithm is currently on
            int currentCity = currentSteps[currentSteps.Count - 1].Id;
            //if that's the goal city, check if it's the shortest distance yet and return
            if(currentCity == goalCity)
            {
                if (lowestDistanceDFS < 0 || overallDistance < lowestDistanceDFS)
                {
                    lowestDistanceDFS = overallDistance;
                    shortestOrderDFS = currentSteps;
                }
                return;
            }

            //foreach possible path
            foreach(var path in cityPaths[currentCity])
            {
                //calculate distance with this path option
                var pathDistance = overallDistance + DistanceBetween(coordinates.First(x=>x.Id == currentCity), coordinates.First(x => x.Id == path));
                var pathSteps = new List<Coordinate>(currentSteps);
                pathSteps.Add(coordinates.First(x => x.Id == path));
                
                //recursively call this function
                DepthFirstSearch(pathSteps, pathDistance, goalCity);
            }
        }

        //use distance formula to find distance between two points
        private static double DistanceBetween(Coordinate coord1, Coordinate coord2)
        {
            return Math.Sqrt(Math.Pow((coord2.Latitude - coord1.Latitude), 2) + Math.Pow((coord2.Longitude - coord1.Longitude), 2));
        }
    }

    //class to hold id, lat & long
    class Coordinate
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    class BreadthPath
    {
        public List<Coordinate> currentSteps { get; set; }
        public double overallDistance { get; set; }
    }
}
