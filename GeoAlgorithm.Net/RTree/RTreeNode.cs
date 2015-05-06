//  Weiqing Chen <kevincwq@gmail.com>
//
//  Copyright (c) 2015 Weiqing Chen

using System;
using System.Collections.Generic;
using System.Threading;
using GeoObject.Net;

namespace GeoAlgorithm.Net.RTree
{
    public class RTreeNode<T>
    {
        private readonly Lazy<List<RTreeNode<T>>> children;

        internal RTreeNode() : this(default(T), new Envelope()) { }

        public RTreeNode(T data, Envelope envelope)
        {
            Data = data;
            Envelope = envelope;
            children = new Lazy<List<RTreeNode<T>>>(() => new List<RTreeNode<T>>(), LazyThreadSafetyMode.None);
        }

        public T Data { get; private set; }
        public Envelope Envelope { get; internal set; }

        internal bool IsLeaf { get; set; }
        internal int Height { get; set; }
        internal List<RTreeNode<T>> Children { get { return children.Value; } }
    }
}
