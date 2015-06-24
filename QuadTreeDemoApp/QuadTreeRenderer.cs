using System.Collections.Generic;
using System.Drawing;
using GeoAlgorithm.Net.QuadTree;
using GeoObject.Net;

namespace QuadTreeDemoApp
{
    /// <summary>
    /// Class draws a QuadTree
    /// </summary>
    class QuadTreeRenderer
    {
        /// <summary>
        /// Create the renderer, give the QuadTree to render.
        /// </summary>
        /// <param name="quadTree"></param>
        public QuadTreeRenderer(QuadTree<Item, Envelope> quadTree)
        {
            m_quadTree = quadTree;
        }

        QuadTree<Item, Envelope> m_quadTree;

        /// <summary>
        /// Hashtable contains a colour for every node in the quad tree so that they are
        /// rendered with a consistant colour.
        /// </summary>
        Dictionary<QuadTreeNode<Item, Envelope>, Color> m_dictionary = new Dictionary<QuadTreeNode<Item, Envelope>, Color>();

        /// <summary>
        /// Get the colour for a QuadTreeNode from the hash table or else create a new colour
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        Color GetColor(QuadTreeNode<Item, Envelope> node)
        {
            if (m_dictionary.ContainsKey(node))
                return m_dictionary[node];

            Color color = Utility.RandomColor;
            m_dictionary.Add(node, color);
            return color;
        }

        /// <summary>
        /// Render the QuadTree into the given Graphics context
        /// </summary>
        /// <param name="graphics"></param>
        internal void Render(Graphics graphics)
        {
            m_quadTree.ForEach(node =>
            {
                // draw the contents of this quad
                if (node.Contents != null)
                {
                    foreach (Item item in node.Contents)
                    {
                        using (Brush b = new SolidBrush(item.Color))
                            graphics.FillEllipse(b, item.Extent.ToRectangle());
                    }
                }

                // draw this quad

                // Draw the border
                Color color = GetColor(node);
                graphics.DrawRectangle(Pens.Black, node.Bounds.ToRectangle());

                //// draw the inside of the border in a distinct colour
                //using (Pen p = new Pen(color))
                //{
                //    var inside = node.Bounds.Clone();
                //    inside.ExpandBy(-1);
                //    graphics.DrawRectangle(p, inside.ToRectangle(Height));
                //}
            });
        }
    }
}
