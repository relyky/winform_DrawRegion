using System; 
using System.Drawing; 
using System.Drawing.Drawing2D; 

// A simple extension to the Graphics class for extended 
// graphic routines, such, 
// as for creating rounded rectangles. 
// Because, Graphics class is an abstract class, 
// that is why it can not be inherited. Although, 
// I have provided a simple constructor 
// that builds the ExtendedGraphics object around a 
// previously created Graphics object. 
// Please contact: aaronreginald@yahoo.com for the most 
// recent implementations of
// this class. 
namespace System.Drawing.Extended 
{ 

    /// <SUMMARY> 
    /// Inherited child for the class Graphics encapsulating 
    /// additional functionality for curves and rounded rectangles. 
    /// </SUMMARY> 
    /// <seealso cref="http://www.codeproject.com/Articles/5649/Extended-Graphics-An-implementation-of-Rounded-Rec"/>
    public static class ExtendedGraphics 
    { 
        #region Fills a Rounded Rectangle with integers. 
        public static void FillRoundRectangle(this Graphics g, System.Drawing.Brush brush,
              int x, int y,
              int width, int height, int radius)
        {

            float fx = Convert.ToSingle(x);
            float fy = Convert.ToSingle(y);
            float fwidth = Convert.ToSingle(width);
            float fheight = Convert.ToSingle(height);
            float fradius = Convert.ToSingle(radius);
            g.FillRoundRectangle(brush, fx, fy,
              fwidth, fheight, fradius);

        } 
        #endregion 

        #region Fills a Rounded Rectangle with continuous numbers.
        public static void FillRoundRectangle(this Graphics g, System.Drawing.Brush brush,
              float x, float y,
              float width, float height, float radius)
        {
            RectangleF rectangle = new RectangleF(x, y, width, height);
            GraphicsPath path = GetRoundedRect(rectangle, radius);
            g.FillPath(brush, path);
        } 
        #endregion

        #region Draws a Rounded Rectangle border with integers. 
        public static void DrawRoundRectangle(this Graphics g,System.Drawing.Pen pen, 
            int x, int y,
            int width, int height, int radius)
        {
            float fx = Convert.ToSingle(x);
            float fy = Convert.ToSingle(y);
            float fwidth = Convert.ToSingle(width);
            float fheight = Convert.ToSingle(height);
            float fradius = Convert.ToSingle(radius);
            g.DrawRoundRectangle(pen, fx, fy, fwidth, fheight, fradius);
        }
        #endregion 

        #region Draws a Rounded Rectangle border with continuous numbers. 
        public static void DrawRoundRectangle(this Graphics g, System.Drawing.Pen pen, 
            float x, float y,
            float width, float height, float radius) 
        { 
            RectangleF rectangle = new RectangleF(x, y, width, height); 
            GraphicsPath path = GetRoundedRect(rectangle, radius); 
            g.DrawPath(pen, path); 
        } 
        #endregion 

        public static void AddRoundRectF(this GraphicsPath gp, RoundedRectF roundRect)
        {
            gp.AddPath(roundRect.GetPath(), true);
        }

        public static void AddRoundRectF(this GraphicsPath gp, RoundedRectF roundRect, bool connect)
        {
            gp.AddPath(roundRect.GetPath(), connect);
        }

        #region Get the desired Rounded Rectangle path. 
        public static GraphicsPath GetRoundedRect(RectangleF baseRect, float radius)
        {
            // if corner radius is less than or equal to zero, 
            // return the original rectangle 
            if (radius <= 0.0F)
            {
                GraphicsPath mPath = new GraphicsPath();
                mPath.AddRectangle(baseRect);
                mPath.CloseFigure();
                return mPath;
            }

            // if the corner radius is greater than or equal to 
            // half the width, or height (whichever is shorter) 
            // then return a capsule instead of a lozenge 
            if (radius >= (Math.Min(baseRect.Width, baseRect.Height)) / 2.0)
                return GetCapsule(baseRect);

            // create the arc for the rectangle sides and declare 
            // a graphics path object for the drawing 
            float diameter = radius * 2.0F;
            SizeF sizeF = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(baseRect.Location, sizeF);
            GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();

            // top left arc 
            path.AddArc(arc, 180, 90);

            // top right arc 
            arc.X = baseRect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc 
            arc.Y = baseRect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = baseRect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        } 
        #endregion 

        #region Gets the desired Capsular path.      
        private static GraphicsPath GetCapsule(RectangleF baseRect)
        {
            float diameter;
            RectangleF arc;
            GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            try
            {
                if (baseRect.Width > baseRect.Height)
                {
                    // return horizontal capsule 
                    diameter = baseRect.Height;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 90, 180);
                    arc.X = baseRect.Right - diameter;
                    path.AddArc(arc, 270, 180);
                }
                else if (baseRect.Width < baseRect.Height)
                {
                    // return vertical capsule 
                    diameter = baseRect.Width;
                    SizeF sizeF = new SizeF(diameter, diameter);
                    arc = new RectangleF(baseRect.Location, sizeF);
                    path.AddArc(arc, 180, 180);
                    arc.Y = baseRect.Bottom - diameter;
                    path.AddArc(arc, 0, 180);
                }
                else
                {
                    // return circle 
                    path.AddEllipse(baseRect);
                }
            }
            catch //(Exception ex)
            {
                path.AddEllipse(baseRect);
            }
            finally
            {
                path.CloseFigure();
            }
            return path;
        } 
        #endregion 
    } 

    public struct RoundedRectF
    {
        #region Attributes
        private float _x;
        private float _y;
        private float _width;
        private float _height;
        private float _radius;
        #endregion

        #region Registor
        //private GraphicsPath _drawPath = null;
        #endregion

        public RoundedRectF(float x, float y, float width, float height, float radius)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _radius = radius;
        }

        public GraphicsPath GetPath()
        {
            return ExtendedGraphics.GetRoundedRect(new RectangleF(_x, _y, _width, _height), _radius);
        }

        
    }
}