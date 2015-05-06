using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GeoAlgorithm.Net.QuadTree;

namespace QuadTreeDemoApp
{
    public partial class MainForm : Form
    {
        QuadTree<Item> m_quadTree;

        QuadTreeRenderer m_renderer;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Init();
        }

        /// <summary>
        /// Resize the window re-initializes the QuadTree to the new size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Resize(object sender, EventArgs e)
        {
            Init();
        }

        /// <summary>
        /// Draw the QuadTree and the selection rectangle
        /// Also highlight the selecte items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            // draw the QuadTree
            m_renderer.Render(e.Graphics);

            // draw the selection rectangle 
            if (!m_selectionRect.IsEmpty)
            {
                // draw the selection rect in semi-transparent yellow
                using (Brush b = new SolidBrush(Color.FromArgb(128, Color.Yellow)))
                    e.Graphics.FillRectangle(b, m_selectionRect);
            }

            // draw the selected items with a red ring around them
            if (m_selectedItems != null)
            {
                foreach (Item obj in m_selectedItems)
                {
                    var selectedRect = obj.BoundingBox.Clone();
                    selectedRect.ExpandBy(1);
                    using (Pen p = new Pen(Color.Red, 2))
                        e.Graphics.DrawEllipse(p, selectedRect.ToRectangle());
                }
            }
        }

        /// <summary>
        /// Initialize the QuadTree to the size of the window.
        /// Initialize the QuadTreeRenderer
        /// </summary>
        private void Init()
        {
            Utility.RectEnvTransHeight = ClientRectangle.Height;
            // restrict the total level be 6
            m_quadTree = new QuadTree<Item>(this.ClientRectangle.ToEnvelope(), x => x.Level < 5);
            m_renderer = new QuadTreeRenderer(m_quadTree);
            m_selectedItems = null;
            m_selectionRect = Rectangle.Empty;
            Invalidate();
        }

        #region mouse interaction code

        bool m_dragging = false;
        Rectangle m_selectionRect;
        Point m_startPoint;
        List<Item> m_selectedItems;

        /// <summary>
        /// MouseUp: 
        /// - if you're using the left mouse button insert a new item into
        ///   the QuadTree at the click point
        /// - if you're dragging with the right mouse button, query the 
        ///   QuadTree with the selection rectangle defined by the current 
        ///   point and the point when the mouseDown event happened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_dragging && e.Button == MouseButtons.Right)
            {
                m_selectedItems = m_quadTree.Query(m_selectionRect.ToEnvelope());
                m_dragging = false;
            }
            else
            {
                Random rand = new Random(DateTime.Now.Millisecond);
                m_quadTree.Insert(new Item(new Rectangle(e.Location, new Size(rand.Next(25) + 4, rand.Next(25) + 4)).ToEnvelope()));
                m_selectedItems = null;
                m_selectionRect = Rectangle.Empty;
            }

            Invalidate();
        }

        /// <summary>
        /// If the it's a right click, record the start point and start drag operation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                m_dragging = true;
                m_startPoint = e.Location;
            }
        }

        /// <summary>
        /// MouseMove: if we're dragging the update the area of the selection
        /// rectangle using the start point and the current point.
        /// Invalidate causes the form to redraw.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_dragging)
            {
                int x = Math.Min(e.Location.X, m_startPoint.X);
                int y = Math.Min(e.Location.Y, m_startPoint.Y);
                int w = Math.Max(e.Location.X, m_startPoint.X) - x;
                int h = Math.Max(e.Location.Y, m_startPoint.Y) - y;
                m_selectionRect = new Rectangle(x, y, w, h);
                Invalidate();
            }
        }
        #endregion

    }
}
