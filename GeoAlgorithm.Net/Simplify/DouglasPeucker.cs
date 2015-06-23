//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace GeoAlgorithm.Net.Simplify
//{
//    class DouglasPeucker
//    {
//        public class RDPWorker
//        {
//            public int start;
//            public double Tolerance;
//            public volatile float[] samples;
//            public List<int> samplesToKeep;

//            public void DoWork()
//            {
//                int h = samples.Length / 2;
//                float[] half = new float[h];
//                unsafe
//                {
//                    fixed (float* ptr = samples, ptrH = half)
//                    {
//                        float* _half = ptrH;
//                        for (int i = start; i < h; ++i)
//                            *(_half) = *(ptr + i);
//                    }
//                }
//                DouglasPeucker.DouglasPeuckerReduction(half, 0, h - 1, Tolerance, ref samplesToKeep);
//            }
//        }

//        public static float[] DouglasPeuckerReduction(float[] points, Double Tolerance)
//        {
//            if (points == null || points.Length < 3)
//                return points;

//            int firstPoint = 0;
//            int lastPoint = 0;
//            List<int> pointIndexsToKeep = new List<int>();

//            pointIndexsToKeep.Add(firstPoint);
//            pointIndexsToKeep.Add(lastPoint);

//            int h = points.Length / Environment.ProcessorCount;
//            List<RDPWorker> workers = new List<RDPWorker>();
//            List<Thread> threads = new List<Thread>();
//            int cpu = 0;

//            while (cpu < Environment.ProcessorCount)
//            {
//                RDPWorker _w = new RDPWorker();
//                _w.samplesToKeep = new List<int>();
//                _w.Tolerance = Tolerance;
//                _w.start = h * cpu;
//                _w.samples = points;
//                workers.Add(_w);
//                ++cpu;
//            }

//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            foreach (RDPWorker worker in workers)
//            {
//                Thread t = new Thread(worker.DoWork);
//                t.IsBackground = true;
//                threads.Add(t);
//                t.Start();
//            }

//            foreach (Thread thread in threads)
//                thread.Join();

//            sw.Stop();
//            Debug.WriteLine("Time = " + sw.ElapsedMilliseconds + " ms");
//            threads.Clear();
//            threads = null;

//            foreach (RDPWorker worker in workers)
//                pointIndexsToKeep = pointIndexsToKeep.Concat(worker.samplesToKeep).ToList();

//            workers.Clear();
//            workers = null;


//            int l = pointIndexsToKeep.Count;
//            float[] returnPoints = new float[l];
//            pointIndexsToKeep.Sort();

//            unsafe
//            {
//                fixed (float* ptr = points, result = returnPoints)
//                {
//                    float* res = result;
//                    for (int i = 0; i < l; ++i)
//                        *(res + i) = *(ptr + pointIndexsToKeep[i]);
//                }
//            }

//            pointIndexsToKeep.Clear();
//            pointIndexsToKeep = null;

//            return returnPoints;
//        }

//        internal static void DouglasPeuckerReduction(float[] points, int firstPoint, int lastPoint, Double tolerance, ref List<int> pointIndexsToKeep)
//        {
//            float maxDistance = 0, tmp = 0, area = 0, X = 0, Y = 0, bottom = 0, distance = 0;
//            int indexFarthest = 0;

//            unsafe
//            {
//                fixed (float* samples = points)
//                {
//                    for (int i = firstPoint; i < lastPoint; ++i)
//                    {
//                        //Perpendicular distance 
//                        tmp = 0.5f * ((lastPoint - i) * (firstPoint - i) + (*(samples + lastPoint) - *(samples + i)) * (*(samples + firstPoint) - *(samples + i)));
//                        //Abs
//                        area = tmp < 0 ? -tmp : tmp;
//                        X = (firstPoint - lastPoint);
//                        Y = (*(samples + firstPoint) - *(samples + lastPoint));
//                        bottom = Sqrt((X * X) + (Y * Y));
//                        distance = area / bottom;

//                        if (distance > maxDistance)
//                        {
//                            maxDistance = distance;
//                            indexFarthest = i;
//                        }
//                    }
//                }
//            }

//            if (maxDistance > tolerance && indexFarthest != 0)
//            {
//                //Add the largest point that exceeds the tolerance
//                pointIndexsToKeep.Add(indexFarthest);
//                DouglasPeuckerReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexsToKeep);
//                DouglasPeuckerReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexsToKeep);
//            }
//        }


//        //http://blog.wouldbetheologian.com/2011/11/fast-approximate-sqrt-method-in-c.html
//        internal static float Sqrt(float z)
//        {
//            if (z == 0) return 0;
//            FloatIntUnion u;
//            u.tmp = 0;
//            u.f = z;
//            u.tmp -= 1 << 23; /* Subtract 2^m. */
//            u.tmp >>= 1; /* Divide by 2. */
//            u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
//            return u.f;
//        }
//    }
//}
