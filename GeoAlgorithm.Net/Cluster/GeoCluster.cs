//  Weiqing Chen <kevincwq@gmail.com>
//
//  Copyright (c) 2015 Weiqing Chen

using System;

namespace GeoAlgorithm.Net.Cluster
{
    /// <summary>
    /// The reason behind clustering is because we want to avoid overwhelming the display data.
    /// There're many ways to do this: 
    /// - Google Maps does it by just showing a portion of their overall data depending on your zoom level; 
    ///   The more you zoom in, the finer the detail gets until you see all the available annotations.
    /// - The another approach is that the final annotations will be represented as a circle with the number
    ///   of data in the middle. e.g. a simplistic image downsizing operation. The middle could be the average
    ///   of all coordinates in this cluster.
    /// </summary>
    public class GeoCluster
    {
        const double ScaleFactorAlpha = 0.3;
        const double ScaleFactorBeta = 0.4;

        /// <summary>
        /// https://images.thoughtbot.com/ios-maps-blog-post-images/graphAndEquation.png
        /// f(x) = 1 / (e^(-alpha * x^beta) + 1)
        /// Here lower values of x will grow quickly and tail off as x gets larger. 
        /// Beta will control how fast our values go to 1, while alpha affects the 
        /// lower bounds (in our case we want the smallest cluster (count of 1) to be 60% of our largest size)
        /// </summary>
        /// <returns></returns>
        public static double ScaleEquation(double value)
        {
            return 1.0 / (1.0 + Math.Exp(-ScaleFactorAlpha * Math.Pow(value, ScaleFactorBeta)));
        }
    }
}
