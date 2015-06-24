//  Weiqing Chen <kevincwq@gmail.com>
//
//  Copyright (c) 2015 Weiqing Chen

using System;
using System.Collections.Generic;
using GeoObject.Net;

namespace GeoAlgorithm.Net.QuadTree
{
    /// <Summary>
    /// Quadtrees are a very straightforward spatial indexing technique. 
    /// In a Quadtree, each node represents a bounding box covering some part of the space being indexed, 
    /// with the root node covering the entire area. Each node is either a leaf node - in which case it 
    /// contains one or more indexed points, and no children, or it is an internal node, in which case 
    /// it has exactly four children, one for each quadrant obtained by dividing the area covered in half
    ///  along both axes - hence the name.
    /// http://en.wikipedia.org/wiki/Quadtree
    /// </Summary>
    public class QuadTree<TItem, TBox> where TItem : IQuadTreeItem<TBox>
        where TBox : IIntersectable<TBox>, IContainable<TBox>, IHasQuadNodes<TBox>, IsEmpty
    {
        /// <summary>
        /// The root QuadTreeNode
        /// </summary>
        QuadTreeNode<TItem, TBox> m_root;

        /// <summary>
        /// The bounds of this QuadTree
        /// </summary>
        TBox m_rectangle;

        ///// <summary>
        ///// An delegate that performs an action on a QuadTreeNode
        ///// </summary>
        ///// <param name="obj"></param>
        //public delegate void QTAction(QuadTreeNode<T> obj);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectangle"></param>
        public QuadTree(TBox rectangle, Func<QuadTreeNode<TItem, TBox>, bool> ifCreateSubnode = null)
        {
            m_rectangle = rectangle;
            m_root = new QuadTreeNode<TItem, TBox>(m_rectangle, 0, ifCreateSubnode);
        }

        /// <summary>
        /// Get the count of items in the QuadTree
        /// </summary>
        public int Count { get { return m_root.Count; } }

        /// <summary>
        /// Insert the feature into the QuadTree
        /// </summary>
        /// <param name="item"></param>
        public bool Insert(TItem item)
        {
            return m_root.Insert(item);
        }

        /// <summary>
        /// Query the QuadTree, returning the items that are in the given area
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        public IEnumerable<TItem> Query(TBox area)
        {
            return m_root.Query(area);
        }

        /// <summary>
        /// Do the specified action for each item in the quadtree
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<QuadTreeNode<TItem, TBox>> action)
        {
            m_root.ForEach(action);
        }
    }
}
