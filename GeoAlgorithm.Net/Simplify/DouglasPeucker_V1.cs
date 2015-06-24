using System;
using System.Collections.Generic;

namespace GeoAlgorithm.Net.Simplify
{
    class Point
    {
        public double X;
        public double Y;
    }

    class DouglasPeucker_V1
    { 
        /// <summary>
        /// Uses the Douglas Peucker algorithim to reduce the number of points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        public static List<Point> DouglasPeuckerReduction(List<Point> points, double tolerance)
        {
            if (points == null || points.Count < 3)
                return points;

            int firstPoint = 0;
            int lastPoint = points.Count - 1;
            List<int> pointIndexesToKeep = new List<int>();

            //Add the first and last index to the keepers
            pointIndexesToKeep.Add(firstPoint);
            pointIndexesToKeep.Add(lastPoint);

            // The first and the last point can not be the same
            while (points[firstPoint].Equals(points[lastPoint]))
            {
                lastPoint--;
            }

            DouglasPeuckerReduction(points, firstPoint, lastPoint, tolerance, ref pointIndexesToKeep);

            List<Point> returnPoints = new List<Point>();
            pointIndexesToKeep.Sort();
            foreach (int index in pointIndexesToKeep)
            {
                returnPoints.Add(points[index]);
            }

            return returnPoints;
        }

        /// <summary>
        /// Reduction.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="firstPoint">The first point.</param>
        /// <param name="lastPoint">The last point.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="pointIndexesToKeep">The point indexes to keep.</param>
        private static void DouglasPeuckerReduction(List<Point> points, int firstPoint, int lastPoint, double tolerance, ref List<int> pointIndexesToKeep)
        {
            double maxDistance = 0;
            int indexFarthest = 0;

            for (int index = firstPoint; index < lastPoint; index++)
            {
                double distance = PerpendicularDistance(points[firstPoint], points[lastPoint], points[index]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0)
            {
                //Add the largest point that exceeds the tolerance
                pointIndexesToKeep.Add(indexFarthest);

                DouglasPeuckerReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexesToKeep);
                DouglasPeuckerReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexesToKeep);
            }
        }

        /// <summary>
        /// The distance of a point from a line made from point1 and point2.
        /// </summary>
        /// <param name="pt1">The PT1.</param>
        /// <param name="pt2">The PT2.</param>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        private static double PerpendicularDistance(Point point1, Point point2, Point point)
        {
            //Area = |(1/2)(x1y2 + x2y3 + x3y1 - x2y1 - x3y2 - x1y3)|   *Area of triangle
            //Base = √((x1-x2)²+(x1-x2)²)                               *Base of Triangle*
            //Area = .5*Base*H                                          *Solve for height
            //Height = Area/.5/Base

            double area = Math.Abs(.5 * (point1.X * point2.Y + point2.X * point.Y + point.X * point1.Y - point2.X * point1.Y - point.X * point2.Y - point1.X * point.Y));
            double bottom = Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
            double height = area / bottom * 2;

            return height;
        }
    }

    class SimplifyLine
    {
        #region  Douglas Peucker algorithm

        /// <summary>
        /// Uses the Douglas Peucker algorithim to reduce the number of points.
        /// </summary>
        public static void DouglasPeuckerReduction(double[] x, double[] y, double tolerance, out double[] nx, out double[] ny)
        {
            if (x == null || y == null || x.Length != y.Length || x.Length < 3)
            {
                nx = x;
                ny = y;
                return;
            }

            int first = 0;
            int last = x.Length - 1;
            List<int> pointIndexesToKeep = new List<int>();

            //Add the first and last index to the keepers
            pointIndexesToKeep.Add(first);
            pointIndexesToKeep.Add(last);

            // The first and the last point can not be the same
            while (x[first] == x[last] && y[first] == y[last])
            {
                last--;
            }

            DouglasPeuckerReduction(x, y, first, last, tolerance, ref pointIndexesToKeep);

            pointIndexesToKeep.Sort();
            nx = new double[pointIndexesToKeep.Count];
            ny = new double[pointIndexesToKeep.Count];
            for (int nindex = 0; nindex < pointIndexesToKeep.Count; nindex++)
            {
                nx[nindex] = x[pointIndexesToKeep[nindex]];
                ny[nindex] = y[pointIndexesToKeep[nindex]];
            }
        }

        /// <summary>
        /// Reduction.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="first">The first point.</param>
        /// <param name="last">The last point.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <param name="pointIndexesToKeep">The point indexes to keep.</param>
        private static void DouglasPeuckerReduction(double[] x, double[] y, int first, int last, double tolerance, ref List<int> pointIndexesToKeep)
        {
            double maxDistance = 0;
            int indexFarthest = 0;

            for (int index = first; index < last; index++)
            {
                double distance = PerpendicularDistance(x[first], y[first], x[last], y[last], x[index], y[index]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0)
            {
                //Add the largest point that exceeds the tolerance
                pointIndexesToKeep.Add(indexFarthest);

                DouglasPeuckerReduction(x, y, first, indexFarthest, tolerance, ref pointIndexesToKeep);
                DouglasPeuckerReduction(x, y, indexFarthest, last, tolerance, ref pointIndexesToKeep);
            }
        }

        /// <summary>
        /// The distance of a point from a line made from point1 and point2.
        /// </summary>
        /// <param name="pt1">The PT1.</param>
        /// <param name="pt2">The PT2.</param>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        private static double PerpendicularDistance(double x1, double y1, double x2, double y2, double x, double y)
        {
            //Area = |(1/2)(x1y2 + x2y3 + x3y1 - x2y1 - x3y2 - x1y3)|   *Area of triangle
            //Base = √((x1-x2)²+(x1-x2)²)                               *Base of Triangle*
            //Area = .5*Base*H                                          *Solve for height
            //Height = Area/.5/Base

            double area = Math.Abs(.5 * (x1 * y2 + x2 * y + x * y1 - x2 * y1 - x * y2 - x1 * y));
            double bottom = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            double height = area / bottom * 2;

            return height;
        }

        #endregion
    }
}