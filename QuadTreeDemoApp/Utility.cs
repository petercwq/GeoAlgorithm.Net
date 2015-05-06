using System;
using System.Drawing;
using GeoObject.Net;

namespace QuadTreeDemoApp
{
    public static class Utility
    {
        public static int RectEnvTransHeight=0;

        static Random m_rand = new Random(DateTime.Now.Millisecond);

        public static Color RandomColor
        {
            get
            {
                return Color.FromArgb(
                    255,
                    m_rand.Next(255),
                    m_rand.Next(255),
                    m_rand.Next(255));

            }
        }

        public static Rectangle ToRectangle(this Envelope envelope)
        {
            return new Rectangle((int)envelope.MinX, RectEnvTransHeight - (int)envelope.MaxY, (int)envelope.Width, (int)envelope.Height);
        }

        public static Envelope ToEnvelope(this Rectangle rectangle)
        {
            return new Envelope(rectangle.Left, rectangle.Right, RectEnvTransHeight - rectangle.Bottom, RectEnvTransHeight - rectangle.Top);
        }
    }
}
