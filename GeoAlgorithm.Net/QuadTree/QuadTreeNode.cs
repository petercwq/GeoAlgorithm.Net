//  Weiqing Chen <kevincwq@gmail.com>
//
//  Copyright (c) 2015 Weiqing Chen

using System;
using System.Collections.Generic;
using GeoObject.Net;

namespace GeoAlgorithm.Net.QuadTree
{
    /// <summary>
    /// The QuadTreeNode
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class QuadTreeNode<TItem, TBox>
        where TItem : IQuadTreeItem<TBox>
        where TBox : IIntersectable<TBox>, IContainable<TBox>, IHasQuadNodes<TBox>, IsEmpty
    {
        /// <summary>
        /// Construct a quadtree node with the given bounds 
        /// </summary>
        /// <param name="area"></param>
        public QuadTreeNode(TBox bounds, int level = 0, Func<QuadTreeNode<TItem, TBox>, bool> ifCreateSubnode = null)
        {
            m_bounds = bounds;
            m_level = level;
            m_ifCreateSubnode = ifCreateSubnode;
        }

        /// <summary>
        /// The area of this node
        /// </summary>
        TBox m_bounds;

        /// <summary>
        /// The level of this node
        /// </summary>
        int m_level;

        Func<QuadTreeNode<TItem, TBox>, bool> m_ifCreateSubnode;

        /// <summary>
        /// The contents of this node.
        /// Note that the contents have no limit: this is not the standard way to implement a QuadTree
        /// </summary>
        List<TItem> m_contents = new List<TItem>();

        /// <summary>
        /// The child nodes of the QuadTree
        /// </summary>
        QuadTreeNode<TItem, TBox>[] m_nodes;// = new QuadTreeNode<T>[4];

        /// <summary>
        /// Is the node empty
        /// </summary>
        public bool IsEmpty { get { return m_bounds.IsEmpty || (/* m_nodes.Count == 0*/m_nodes == null && m_contents.Count == 0); } }

        /// <summary>
        /// Area of the quadtree node
        /// </summary>
        public TBox Bounds { get { return m_bounds; } }

        /// <summary>
        /// The level of the quadtree node
        /// </summary>
        public int Level { get { return m_level; } }

        /// <summary>
        /// Total number of nodes in the this node and all SubNodes
        /// </summary>
        public int Count
        {
            get
            {
                int count = 0;
                if (m_nodes != null)
                {
                    foreach (var node in m_nodes)
                        count += node.Count;
                }
                count += this.Contents.Count;

                return count;
            }
        }

        /// <summary>
        /// Return the contents of this node and all subnodes in the true below this one.
        /// </summary>
        public IEnumerable<TItem> SubTreeContents
        {
            get
            {
                // List<T> results = new List<T>();
                //if (m_nodes != null)
                //{
                //    foreach (QuadTreeNode<T> node in m_nodes)
                //        results.AddRange(node.SubTreeContents);
                //}
                //results.AddRange(this.Contents);
                //return results;

                foreach (var item in this.Contents)
                    yield return item;
                if (m_nodes != null)
                {
                    foreach (var node in m_nodes)
                    {
                        foreach (var item in node.SubTreeContents)
                            yield return item;
                    }
                }
            }
        }

        public List<TItem> Contents { get { return m_contents; } }

        /// <summary>
        /// Query the QuadTree for items that are in the given area
        /// </summary>
        /// <param name="queryArea"></pasram>
        /// <remarks>
        /// Not use "ref" on results parameter, the Query method should not change the the value of the reference itself during execution.
        /// </remarks>
        public IEnumerable<TItem> Query(TBox queryArea/*, List<T> results*/)
        {
            // create a list of the items that are found
            // List<T> results = new List<T>();

            // this quad contains items that are not entirely contained by
            // it's four sub-quads. Iterate through the items in this quad 
            // to see if they intersect.
            foreach (TItem item in this.Contents)
            {
                if (queryArea.Intersects(item.Extent))
                    // results.Add(item);
                    yield return item;
            }

            // if no node exists, return
            if (m_nodes == null)
                yield break;

            foreach (var node in m_nodes)
            {
                if (node.IsEmpty)
                    continue;

                // Case 1: search area completely contained by sub-quad
                // if a node completely contains the query area, go down that branch
                // and skip the remaining nodes (break this loop)
                if (node.Bounds.Contains(queryArea))
                {
                    // results.AddRange(node.Query(queryArea));
                    foreach (var item in node.Query(queryArea))
                        yield return item;
                    break;
                }

                // Case 2: Sub-quad completely contained by search area 
                // if the query area completely contains a sub-quad,
                // just add all the contents of that quad and it's children 
                // to the result set. You need to continue the loop to test 
                // the other quads
                if (queryArea.Contains(node.Bounds))
                {
                    foreach (var item in node.SubTreeContents)
                        yield return item;
                    continue;
                }

                // Case 3: search area intersects with sub-quad
                // traverse into this quad, continue the loop to search other
                // quads
                if (node.Bounds.Intersects(queryArea))
                {
                    // results.AddRange(node.Query(queryArea));
                    foreach (var item in node.Query(queryArea))
                        yield return item;
                }
            }

            // return results;
        }

        /// <summary>
        /// Insert an item to this node
        /// </summary>
        /// <param name="item"></param>
        public bool Insert(TItem item)
        {
            // if the item is not contained in this quad, there's a problem
            if (!m_bounds.Contains(item.Extent))
            {
                return false;
                //throw new System.InvalidOperationException("feature is out of the bounds of this quadtree node");
            }

            // if the subnodes are null create them. may not be successful: see below
            // we may be at the smallest allowed size in which case the subnodes will not be created
            if (m_nodes == null)
                CreateSubNodes();

            if (m_nodes != null)
            {
                // for each subnode:
                // if the node contains the item, add the item to that node and return
                // this recurses into the node that is just large enough to fit this item
                foreach (var node in m_nodes)
                {
                    if (node.Bounds.Contains(item.Extent))
                    {
                        node.Insert(item);
                        return true;
                    }
                }
            }

            // if we make it to here, either
            // 1) none of the subnodes completely contained the item. or
            // 2) we're at the smallest subnode size allowed add the item to this node's contents.
            this.Contents.Add(item);
            return true;
        }

        public void ForEach(Action<QuadTreeNode<TItem, TBox>> action)
        {
            action(this);

            if (m_nodes != null)
            {
                // draw the child quads
                foreach (var node in this.m_nodes)
                    node.ForEach(action);
            }
        }

        /// <summary>
        /// Internal method to create the subnodes (partitions space)
        /// </summary>
        private void CreateSubNodes()
        {
            // the smallest subnode has an area 
            //if ((m_bounds.Height * m_bounds.Width) <= 10)
            //    return;

            if (m_ifCreateSubnode != null && !m_ifCreateSubnode(this))
                return;

            var boxes = m_bounds.QuadNodes;
            m_nodes = new QuadTreeNode<TItem, TBox>[boxes.Length];
            for (int i = 0; i < boxes.Length; i++)
            {
                m_nodes[i] = new QuadTreeNode<TItem, TBox>(boxes[i], m_level + 1, m_ifCreateSubnode);
            }

            //var minx = m_bounds.MinX;
            //var miny = m_bounds.MinY;
            //var maxx = m_bounds.MaxX;
            //var maxy = m_bounds.MaxY;
            //var centerx = minx + (m_bounds.Width / 2d);
            //var centery = miny + (m_bounds.Height / 2d);

            //m_nodes = new QuadTreeNode<TItem, TBox>[4]{
            //    new QuadTreeNode<TItem, TBox>(new Envelope(minx, centerx, miny, centery), m_level + 1, m_ifCreateSubnode),
            //    new QuadTreeNode<TItem, TBox>(new Envelope(centerx, maxx, miny, centery), m_level + 1, m_ifCreateSubnode),
            //    new QuadTreeNode<TItem, TBox>(new Envelope(minx, centerx, centery, maxy), m_level + 1, m_ifCreateSubnode),
            //    new QuadTreeNode<TItem, TBox>(new Envelope(centerx, maxx, centery, maxy), m_level + 1, m_ifCreateSubnode)
            //};
        }
    }
}
