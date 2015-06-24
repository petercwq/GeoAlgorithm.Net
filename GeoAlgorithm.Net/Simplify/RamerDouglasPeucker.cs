using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GeoAlgorithm.Net.Simplify
{
    /// <summary>
    /// !http://ianqvist.blogspot.com/2010/05/ramer-douglas-peucker-polygon.html
    /// </summary>
    class RamerDouglasPeucker
    {
        #region Private Helper

        private static double Area(Point a, Point b, Point c)
        {
            return (((b.X - a.X) * (c.Y - a.Y)) - ((c.X - a.X) * (b.Y - a.Y)));
        }

        private static void Cross(ref Point a, ref Point b, out double c)
        {
            c = a.X * b.Y - a.Y * b.X;
        }

        private static double DistancePointPoint(Point p, Point p2)
        {
            double dx = p.X - p2.X;
            double dy = p.Y - p2.X;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static double DistancePointLine(Point p, Point A, Point B)
        {
            // if start == end, then use point-to-point distance
            if (A.X == B.X && A.Y == B.Y)
                return DistancePointPoint(p, A);

            // otherwise use comp.graphics.algorithms Frequently Asked Questions method
            /*(1)     	      AC dot AB
                        r =   ---------
                              ||AB||^2
             
		                r has the following meaning:
		                r=0 Point = A
		                r=1 Point = B
		                r<0 Point is on the backward extension of AB
		                r>1 Point is on the forward extension of AB
		                0<r<1 Point is interior to AB
	        */

            double r = ((p.X - A.X) * (B.X - A.X) + (p.Y - A.Y) * (B.Y - A.Y))
                        /
                        ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));

            if (r <= 0.0) return DistancePointPoint(p, A);
            if (r >= 1.0) return DistancePointPoint(p, B);

            /*(2)
		                    (Ay-Cy)(Bx-Ax)-(Ax-Cx)(By-Ay)
		                s = -----------------------------
		             	                Curve^2

		                Then the distance from C to Point = |s|*Curve.
	        */

            double s = ((A.Y - p.Y) * (B.X - A.X) - (A.X - p.X) * (B.Y - A.Y))
                        / ((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y));

            return Math.Abs(s) * Math.Sqrt(((B.X - A.X) * (B.X - A.X) + (B.Y - A.Y) * (B.Y - A.Y)));
        }


        #endregion

        /// <summary>
        /// Removes all collinear points on the polygon.
        /// </summary>
        /// <param name="vertices">The polygon that needs simplification.</param>
        /// <param name="collinearityTolerance">The collinearity tolerance.</param>
        /// <returns>A simplified polygon.</returns>
        public static List<Point> CollinearSimplify(List<Point> vertices, double collinearityTolerance = 0d)
        {
            //We can't simplify polygons under 3 vertices
            if (vertices.Count < 3)
                return vertices;

            List<Point> simplified = new List<Point>();

            for (int i = 0; i < vertices.Count; i++)
            {
                int prevId = i == 0 ? vertices.Count - 1 : i - 1;
                int nextId = i == vertices.Count - 1 ? 0 : i + 1;

                var prev = vertices[prevId];
                var current = vertices[i];
                var next = vertices[nextId];

                //If they collinear, continue
                if (Area(prev, current, next) <= collinearityTolerance)
                    continue;

                simplified.Add(current);
            }

            return simplified;
        }

        /// <summary>
        /// Ramer-Douglas-Peucker polygon simplification algorithm. 
        /// This is the general recursive version that does not use the speed-up technique by using the Melkman convex hull.
        /// 
        /// If you pass in 0, it will remove all collinear points
        /// </summary>
        /// <returns>The simplified polygon</returns>
        public static List<Point> DouglasPeuckerSimplify(List<Point> vertices, double distanceTolerance)
        {
            List<int> pointIndexesToKeep = new List<int>();

            bool[] _usePt = new bool[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
                _usePt[i] = true;

            SimplifySection(vertices, 0, vertices.Count - 1, distanceTolerance, ref _usePt);
            List<Point> result = new List<Point>();

            for (int i = 0; i < vertices.Count; i++)
                if (_usePt[i])
                    result.Add(vertices[i]);

            return result;
        }

        private static void SimplifySection(List<Point> pts, int first, int last, double distanceTolerance, ref bool[] usePt)
        {
            if ((first + 1) == last)
                return;

            double maxDistance = -1.0;
            int maxIndex = first;
            for (int k = first + 1; k < last; k++)
            {
                double distance = DistancePointLine(pts[k], pts[first], pts[last]);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxIndex = k;
                }
            }
            if (maxDistance <= distanceTolerance)
                for (int k = first + 1; k < last; k++)
                    usePt[k] = false;
            else
            {
                SimplifySection(pts, first, maxIndex, distanceTolerance, ref usePt);
                SimplifySection(pts, maxIndex, last, distanceTolerance, ref usePt);
            }
        }

        //From physics2d.net
        public static List<Point> ReduceByArea(List<Point> vertices, double areaTolerance)
        {
            if (vertices.Count <= 3)
                return vertices;

            if (areaTolerance < 0) { throw new ArgumentOutOfRangeException("areaTolerance", "must be equal to or greater then zero."); }

            List<Point> result = new List<Point>();
            Point v1, v2, v3;
            double old1, old2, new1;
            v1 = vertices[vertices.Count - 2];
            v2 = vertices[vertices.Count - 1];
            areaTolerance *= 2;
            for (int index = 0; index < vertices.Count; ++index, v2 = v3)
            {
                if (index == vertices.Count - 1)
                {
                    if (result.Count == 0) { throw new ArgumentOutOfRangeException("areaTolerance", "The tolerance is too high!"); }
                    v3 = result[0];
                }
                else { v3 = vertices[index]; }
                Cross(ref v1, ref v2, out old1);
                Cross(ref v2, ref v3, out old2);
                Cross(ref v1, ref v3, out new1);
                if (Math.Abs(new1 - (old1 + old2)) > areaTolerance)
                {
                    result.Add(v2);
                    v1 = v2;
                }
            }
            return result;
        }

        //From Eric Jordan's convex decomposition library
        /// <summary>
        /// Merges all parallel edges in the list of vertices
        /// </summary>
        /// <param name="tolerance"></param>
        public static void MergeParallelEdges(List<Point> vertices, double tolerance)
        {
            if (vertices.Count <= 3)
                return; //Can't do anything useful here to a triangle

            bool[] mergeMe = new bool[vertices.Count];
            int newNVertices = vertices.Count;

            //Gather points to process
            for (int i = 0; i < vertices.Count; ++i)
            {
                int lower = (i == 0) ? (vertices.Count - 1) : (i - 1);
                int middle = i;
                int upper = (i == vertices.Count - 1) ? (0) : (i + 1);

                double dx0 = vertices[middle].X - vertices[lower].X;
                double dy0 = vertices[middle].Y - vertices[lower].Y;
                double dx1 = vertices[upper].Y - vertices[middle].X;
                double dy1 = vertices[upper].Y - vertices[middle].Y;
                double norm0 = (double)Math.Sqrt(dx0 * dx0 + dy0 * dy0);
                double norm1 = (double)Math.Sqrt(dx1 * dx1 + dy1 * dy1);

                if (!(norm0 > 0.0f && norm1 > 0.0f) && newNVertices > 3)
                {
                    //Merge identical points
                    mergeMe[i] = true;
                    --newNVertices;
                }

                dx0 /= norm0;
                dy0 /= norm0;
                dx1 /= norm1;
                dy1 /= norm1;
                double cross = dx0 * dy1 - dx1 * dy0;
                double dot = dx0 * dx1 + dy0 * dy1;

                if (Math.Abs(cross) < tolerance && dot > 0 && newNVertices > 3)
                {
                    mergeMe[i] = true;
                    --newNVertices;
                }
                else
                    mergeMe[i] = false;
            }

            if (newNVertices == vertices.Count || newNVertices == 0)
                return;

            int currIndex = 0;

            //Copy the vertices to a new list and clear the old
            List<Point> oldVertices = new List<Point>(vertices);
            vertices.Clear();

            for (int i = 0; i < oldVertices.Count; ++i)
            {
                if (mergeMe[i] || newNVertices == 0 || currIndex == newNVertices)
                    continue;

                Debug.Assert(currIndex < newNVertices);

                vertices.Add(oldVertices[i]);
                ++currIndex;
            }
        }

        //Misc
        /// <summary>
        /// Reduces the polygon by distance.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="distance">The distance between points. Points closer than this will be 'joined'.</param>
        /// <returns></returns>
        public static List<Point> ReduceByDistance(List<Point> vertices, double distance)
        {
            //We can't simplify polygons under 3 vertices
            if (vertices.Count < 3)
                return vertices;

            distance *= 2;
            List<Point> simplified = new List<Point>();
            for (int i = 0; i < vertices.Count; i++)
            {
                int nextId = i == vertices.Count - 1 ? 0 : i + 1;
                var current = vertices[i];
                var next = vertices[nextId];
                var deltaX = next.X - current.X;
                var deltaY = next.Y - current.Y;

                //If they are closer than the distance, continue
                if ((deltaX * deltaX + deltaY * deltaY) <= distance)
                    continue;

                simplified.Add(current);
            }

            return simplified;

        }

        /// <summary>
        /// Reduces the polygon by removing the Nth vertex in the vertices list.
        /// </summary>
        /// <param name="vertices">The vertices.</param>
        /// <param name="Nth">The Nth point to remove. Example: 5.</param>
        /// <returns></returns>
        public static List<Point> ReduceByNth(List<Point> vertices, int Nth)
        {
            //We can't simplify polygons under 3 vertices
            if (vertices.Count < 3)
                return vertices;

            if (Nth == 0)
                return vertices;

            List<Point> result = new List<Point>(vertices.Count);

            for (int i = 0; i < vertices.Count; i++)
            {
                if (i % Nth == 0)
                    continue;

                result.Add(vertices[i]);
            }

            return result;
        }
    }
}
