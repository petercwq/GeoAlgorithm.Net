using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GeoAlgorithm.Net.ConvexHull;

namespace ConvexHullDemoApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private List<Point> m_Points = new List<Point>();

        private void btnClear_Click(object sender, EventArgs e)
        {
            m_Points = new List<Point>();
            this.Invalidate();
        }

        // Add a new Point.
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            m_Points.Add(new Point(e.X, e.Y));
            this.Invalidate();
        }

        // Redraw.
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw the convex hull.

            // Fill all of the points.
            foreach (Point pt in m_Points)
            {
                e.Graphics.FillEllipse(Brushes.Cyan, pt.X - 3, pt.Y - 3, 7, 7);
            }

            List<ConvexHull.Point> hull = null;
            if (m_Points.Count >= 3)
            {
                // Get the convex hull.
                hull = ConvexHull.MakeConvexHull(m_Points.Select(p => new ConvexHull.Point(p.X, p.Y)).ToList());

                // Draw.
                // Fill the non-culled points.
                foreach (var pt in ConvexHull.g_NonCulledPoints)
                {
                    e.Graphics.FillEllipse(Brushes.White, (float)pt.X - 3, (float)pt.Y - 3, 7, 7);
                }
            }

            // Draw all of the points.
            foreach (Point pt in m_Points)
            {
                e.Graphics.DrawEllipse(Pens.Black, pt.X - 3, pt.Y - 3, 7, 7);
            }

            if (m_Points.Count >= 3)
            {
                // Draw the MinMax quadrilateral.
                e.Graphics.DrawPolygon(Pens.Red, ConvexHull.g_MinMaxCorners.Select(p => new Point((int)p.X, (int)p.Y)).ToArray());

                // Draw the culling box.
                var box = ConvexHull.g_MinMaxBox;
                e.Graphics.DrawRectangle(Pens.Orange, new Rectangle((int)box.Left, (int)box.Top, (int)(box.Right - box.Left), (int)(box.Bottom - box.Top)));

                // Draw the convex hull.
                var hull_points = new ConvexHull.Point[hull.Count];
                hull.CopyTo(hull_points);
                e.Graphics.DrawPolygon(Pens.Blue, hull_points.Select(p => new Point((int)p.X, (int)p.Y)).ToArray());
            }
        }
    }
}
