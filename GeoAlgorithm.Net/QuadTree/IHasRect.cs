//  Weiqing Chen <kevincwq@gmail.com>
//
//  Copyright (c) 2015 Weiqing Chen

using GeoObject.Net;

namespace GeoAlgorithm.Net.QuadTree
{
    /// <summary>
    /// An interface that defines and object with a rectangle
    /// </summary>
    public interface IHasRect
    {
        Envelope Rectangle { get; }
    }
}
