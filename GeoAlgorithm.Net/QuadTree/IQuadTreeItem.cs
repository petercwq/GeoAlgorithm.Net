//  Weiqing Chen <kevincwq@gmail.com>
//
//  Copyright (c) 2015 Weiqing Chen

using GeoObject.Net;

namespace GeoAlgorithm.Net.QuadTree
{
     /// <summary>
    /// An interface that defines and object with a rectangle
    /// </summary>
    public interface IQuadTreeItem<T> where T : IIntersectable<T>, IContainable<T>, IHasQuadNodes<T>, IsEmpty
    {
        T Extent { get; }
    }
}
