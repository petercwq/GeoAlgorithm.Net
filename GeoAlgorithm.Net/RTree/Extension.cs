//  Weiqing Chen <kevincwq@gmail.com>
//
//  Copyright (c) 2015 Weiqing Chen

using System.Collections.Generic;
using GeoObject.Net;

namespace GeoAlgorithm.Net.RTree
{
    internal static class StackExtensions
    {
        public static T TryPop<T>(this Stack<T> stack)
        {
            return stack.Count == 0 ? default(T) : stack.Pop();
        }

        public static T TryPeek<T>(this Stack<T> stack)
        {
            return stack.Count == 0 ? default(T) : stack.Peek();
        }

        public static int Margin(this Envelope envelope)
        {
            return (int)(envelope.Width + envelope.Height);
        }
    }
}
