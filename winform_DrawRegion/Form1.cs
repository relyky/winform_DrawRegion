using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Drawing.Extended;

namespace winform_DrawRegion
{
    public partial class Form1 : Form
    {
        #region Document
        protected Collection<SimsState> m_states = new Collection<SimsState>();
        protected Collection<SimsAffect> m_affects = new Collection<SimsAffect>();

        protected SimsArrowLine m_arrow = new SimsArrowLine(50, 400, 150, 400, 200, 350);
        #endregion

        #region UI control flag

        protected void SetSelectedObject(object obj)
        {
            if(obj == null)
            {
                _selectedState = null;
                _selectedAffect = null;
                return;
            }

            switch(obj.GetType().Name)
            {
                case "SimsState":
                    _selectedState = (SimsState)obj;
                    _selectedAffect = null;
                    break;
                case "SimsAffect":
                    _selectedState = null;
                    _selectedAffect = (SimsAffect)obj;
                    break;
            }
        }

        protected void SetSelectedObject(object obj, bool addnewAffect)
        {
            SetSelectedObject(obj);
            f_addnewAffect = addnewAffect;
        }

        protected void SetSelectedObject(object obj, bool addnewAffect, Point mouseLocation)
        {
            SetSelectedObject(obj);
            f_addnewAffect = addnewAffect;
            m_mouseLocation = mouseLocation;
        }

        protected SimsState _selectedState = null;
        protected SimsAffect _selectedAffect = null;
        protected bool f_addnewAffect = false; // 旗標：新增中
        protected Point m_mouseLocation;

        #endregion

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // 使用雙緩衝圖形解決
            this.KeyPreview = true; // 可接受key event


            //float x1 = 2;
            //float x2 = 7;
            //float xc = 4;
            //float x1p = 2;
            //float x2p = 9;
            //float xcp = CalcHelper.CalcScaleDot(x1, x2,xc, x1p, x2p);
            //Debug.Print("xcp {0}", xcp);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            m_states.Add(new SimsState("企業", 500, 120, 50));
            m_states.Add(new SimsState("市場", 700, 360, 50));
            m_states.Add(new SimsState("人民", 300, 360, 50));

            m_affects.Add(new SimsAffect(m_states[0], m_states[1], 1.0f));
            m_affects.Add(new SimsAffect(m_states[2], m_states[1], 1.0f));
            m_affects.Add(new SimsAffect(m_states[2], m_states[0], 1.0f));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // draw here
            foreach (var c in m_states)
                c.Paint(e.Graphics);

            foreach (var c in m_affects)
                c.Paint(e.Graphics);

            // draw foucs
            if (_selectedState != null)
                _selectedState.PaintFocus(e.Graphics);

            if (_selectedAffect != null)
                _selectedAffect.PaintFocus(e.Graphics);

            // draw adding...
            if (this.f_addnewAffect && this._selectedState != null)
            {
                e.Graphics.DrawLine(Pens.OrangeRed, _selectedState.Location, m_mouseLocation);
            }

        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:

                        #region f_addnewAffect - commit
                        if (f_addnewAffect)
                        {
                            foreach (var c in m_states)
                            {
                                if (c.HitTest(e.Location))
                                {
                                    m_affects.Add(new SimsAffect(_selectedState, c, +1.0f));
                                    this.SetSelectedObject(null, false);
                                    return;
                                }
                            }
                            return;
                        } 
                        //# 若有選取 SimsAffect 則移動 amid 
                        else if (_selectedAffect != null)
                        {
                            _selectedAffect.SetAmidPoint(e.Location);
                            this.SetSelectedObject(null, false);
                            return;
                        }
                        #endregion

                        #region # 選取 SimsState
                        foreach (var c in m_states)
                        {
                            if(c.HistTestOutline(e.Location))
                            {
                                this.StatusLabel1.Text += "  hit outline";
                                return;
                            }
                            else if (c.HitTest(e.Location))
                            {
                                this.StatusLabel1.Text += "  hit inside";
                                this.SetSelectedObject(c);
                                return;
                            }
                        }
                        #endregion

                        // cancel all action
                        SetSelectedObject(null, false);

                        break;
                    case MouseButtons.Right:

                        #region f_addnewAffect - cancel
                        if (f_addnewAffect)
                        {
                            SetSelectedObject(null, false);
                            return;
                        } // ------------------------------
                        #endregion

                        #region # 選取 SimsAffect
                        foreach (var c in m_affects)
                        {
                            if (c.HitTest(e.Location))
                            {
                                this.SetSelectedObject(c);
                                return;
                            }
                        }
                        #endregion

                        #region # 選取 SimsState 以新增 SimsAffect
                        foreach (var c in m_states)
                        {
                            if (c.HitTest(e.Location))
                            {
                                this.SetSelectedObject(c, true, e.Location);
                                return;
                            }
                        }
                        #endregion

                        // cancel all action
                        SetSelectedObject(null, false);

                        break;
                }
            }
            finally
            {
                this.Invalidate();
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // f_addnewAffect - ing
                if (f_addnewAffect && _selectedState != null)
                {
                    m_mouseLocation = e.Location;
                    return;
                }  
                //# 若有選取 SimsAffect 則移動 amid 
                else if (_selectedAffect != null)
                {
                    _selectedAffect.SetAmidPoint(e.Location);
                    return;
                }

                switch (e.Button)
                {
                    case MouseButtons.Left:                
                        //# 若有選取 SimsState 則移動它
                        if (_selectedState != null)
                        {
                            _selectedState.SetPosition(e.Location); // 移動
                        }
                        break;
                }
            }
            finally
            {
                // 重繪
                this.Invalidate();
            }
        }

        private void btnAddEntity_Click(object sender, EventArgs e)
        {
            SimsState newState = new SimsState("新狀態", 120, 100, 50);
            m_states.Add(newState);

            SetSelectedObject(newState);
            this.Invalidate();
        }

    }

    public class SimsDocument
    {
        protected Collection<SimsState> m_states = new Collection<SimsState>();
        protected Collection<SimsAffect> m_affects = new Collection<SimsAffect>();

        public void AddSims(SimsState state)
        {
            m_states.Add(state);
        }

        public void AddSims(SimsAffect affect)
        {
            m_affects.Add(affect);
        }

        public void Paint(Graphics g)
        {
            // draw here
            foreach (var c in m_states)
                c.Paint(g);

            foreach (var c in m_affects)
                c.Paint(g);
        }

    }

    public class SimsState
    {
        #region Attributes
        protected string _name = "name";
        protected float _money = 1000.0f;
        protected float _materials = 1000.0f;
        #endregion

        #region contact relationship
        protected List<SimsAffect> m_contacts = new List<SimsAffect>();
        #endregion

        #region visualization
        protected int _cx = 100;
        protected int _cy = 100;
        protected int _r = 50;
        //protected int _rw = 80;
        //protected int _rh = 40;

        protected Pen pen = new Pen(Color.Blue, 2);
        protected Font font = SystemFonts.DialogFont;
        protected Brush fontBrush = Brushes.Black;
        protected Brush brush = Brushes.LightBlue;
        protected Pen focusPen = new Pen(Color.YellowGreen, 3);

        protected GraphicsPath drawPath = null;
        //protected Region region = null;
        #endregion

        #region Property

        public Point Location { get { return new Point(_cx, _cy); } }

        public int Radius { get { return _r; } }

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public float Money
        {
            get { return _money; }
            set { _money = value; }
        }

        public float Materials
        {
            get { return _materials; }
            set { _materials = value; }
        }

        #endregion

        public SimsState(string name, int cx, int cy, int r)
        {
            // setup default attributes
            focusPen.DashStyle = DashStyle.Dash; // 虛線 
            
            // create instance
            _name = name;
            _cx = cx;
            _cy = cy;
            _r = r;
            //_rw = rw;
            //_rh = rh;

            // update draw path
            this.UpdateDrawPath();
        }

        protected void UpdateDrawPath()
        {
            // cascade update 
            this.drawPath = new GraphicsPath();
            this.drawPath.AddRoundRectF(new RoundedRectF(_cx - _r, _cy - _r, _r + _r, _r + _r, _r / 2));
            this.drawPath.AddEllipse(_cx - _r + 3, _cy - _r + 3, _r + _r - 6, _r + _r - 6);
            //this.region = new Region(this.drawPath);

            // cascade update contect relationship entitys
            foreach (var c in m_contacts)
                c.UpdateDrawPath();
        }

        public void Paint(Graphics g)
        {
            // fill region
            g.FillPath(this.brush, this.drawPath);

            //// draw center
            //const int pad = 2;
            //g.DrawLine(Pens.Black, _cx-pad, _cy-pad, _cx+pad, _cy+pad);
            //g.DrawLine(Pens.Black, _cx-pad, _cy+pad, _cx+pad, _cy-pad);

            // draw border
            g.DrawPath(this.pen, this.drawPath);

            // show name
            SizeF sz = g.MeasureString(_name, this.font);
            float x = _cx - sz.Width / 2;
            float y = _cy - sz.Height * 1.2f;
            g.DrawString(_name, this.font, this.fontBrush, x, y);

            // show value 
            string money_str = "$" + _money.ToString("N2");
            sz = g.MeasureString(money_str, this.font);
            x = _cx - sz.Width / 2;
            y = _cy; // +sz.Height; // *1.5f;
            g.DrawString(money_str, this.font, this.fontBrush, x, y);

            string _materials_str = "U" + _materials.ToString("N2");
            sz = g.MeasureString(_materials_str, this.font);
            x = _cx - sz.Width / 2;
            y = _cy + sz.Height; // *1.5f;
            g.DrawString(_materials_str, this.font, this.fontBrush, x, y);
        }

        public void PaintFocus(Graphics g)
        {
            RectangleF bounds = this.drawPath.GetBounds();
            g.DrawRectangle(this.focusPen, bounds.X - 6.0f, bounds.Y - 6.0f, bounds.Width + 12.0f, bounds.Height + 12.0f);
        }

        public bool HitTest(Point pos)
        {
            //return this.region.IsVisible(pos);
            return this.drawPath.IsVisible(pos);
        }
        
        public bool HistTestOutline(Point pos)
        {
            return this.drawPath.IsOutlineVisible(pos, this.pen);
        }

        public void SetPosition(Point pos)
        {
            // 
            _cx = pos.X;
            _cy = pos.Y;

            // update draw path
            this.UpdateDrawPath();
        } 

        public void Contact(SimsAffect affect)
        {
            m_contacts.Add(affect);
        }
    }

    public class SimsArrowLine
    {
        private PointF _from;
        private PointF _amid;
        private PointF _to;

        #region visual
        private GraphicsPath drawPath = null;
        private Pen _pen;
        private Pen _focusPen;
        #endregion

        #region Property

        public PointF Amid { get { return _amid; } }

        #endregion

        public SimsArrowLine(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            _from = new PointF(x1, y1);
            _amid = new PointF(x2, y2);
            _to = new PointF(x3, y3);

            _pen = new Pen(Color.OrangeRed, 2);
            GraphicsPath capPath = new GraphicsPath();
            capPath.AddLines(new Point[] { new Point(-3, -4), new Point(0, 0), new Point(3, -4)});
            //_pen.StartCap = LineCap.SquareAnchor;
            //_pen.EndCap = LineCap.ArrowAnchor;
            _pen.CustomEndCap = new CustomLineCap(null, capPath);

            _focusPen = (Pen)_pen.Clone();
            _focusPen.Color = Color.GreenYellow;

            // update draw path
            this.UpdateDrawPath();
        }

        public void Draw(Graphics g)
        {
            //g.DrawCurve(_pen, new PointF[] {_from, _amid, _to } );
            g.DrawPath(_pen, this.drawPath);
            g.FillEllipse(Brushes.Blue, _amid.X - 3, _amid.Y - 3, 6, 6);
        }

        public void DrawFocus(Graphics g)
        {
            g.DrawPath(_focusPen, this.drawPath);
        }

        public bool HitTest(Point pos)
        {
            //return this.drawPath.IsVisible(pos);
            return this.drawPath.IsOutlineVisible(pos, _pen);
        }

        public HitTestResult HitTestEx(Point pos)
        {
            const int pad = 2;
            Rectangle rectTo = new Rectangle((int)_to.X - pad, (int)_to.Y - pad, (int)_to.X + pad, (int)_to.Y + pad);
            if (rectTo.Contains(pos))
                return HitTestResult.TO;

            Rectangle rectFrom = new Rectangle((int)_from.X - pad, (int)_from.Y - pad, (int)_from.X + pad, (int)_from.Y + pad);
            if (rectFrom.Contains(pos))
                return HitTestResult.FROM;

            if (this.drawPath.IsOutlineVisible(pos, _pen))
                return HitTestResult.AMID;

            return HitTestResult.NON;
        }

        public void SetFromPoint(float x1, float y1)
        {
            _from.X = x1;
            _from.Y = y1;

            this.UpdateDrawPath();
        }

        public void SetAmidPoint(float x2, float y2)
        {
            _amid.X = x2;
            _amid.Y = y2;

            this.UpdateDrawPath();
        }

        public void SetToPoint(float x3, float y3)
        {
            _to.X = x3;
            _to.Y = y3;

            this.UpdateDrawPath();
        }

        internal protected void UpdateDrawPath()
        {
            // cascade update 
            this.drawPath = new GraphicsPath();
            this.drawPath.AddCurve(new PointF[] { _from, _amid, _to });
        }

        public enum HitTestResult { NON, FROM, AMID, TO }; 

    }

    public class SimsAffect
    {
        #region attributes properties
        public SimsState A;
        public SimsState B;
        public float affectVolumn;
        #endregion

        #region visual
        protected SimsArrowLine m_ArrowLine;
        //protected Pen pen;
        //protected Font font = SystemFonts.DialogFont;
        //protected Brush brush = Brushes.BlueViolet;
        //protected Pen focusPen = new Pen(Color.YellowGreen, 3);
        #endregion

        #region properties
        public float AffectVolumn
        {
            get { return this.affectVolumn; }
            set { this.affectVolumn = value; }
        }

        #endregion

        #region meta

        /// <summary>
        /// Previous Location
        /// </summary>
        protected PointF _preAp = PointF.Empty;
        protected PointF _preBp = PointF.Empty;

        #endregion

        public SimsAffect(SimsState _A, SimsState _B, float _volum)
        {
            A = _A;
            B = _B;
            affectVolumn = _volum;

            // contact connectionship
            A.Contact(this);
            B.Contact(this);

            // visual - init. 
            PointF amidPt = new PointF((float)(A.Location.X + B.Location.X) / 2.0f, (float)(A.Location.Y + B.Location.Y) / 2.0f);
            PointF Ap = CalcHelper.CalcCircleBoundPos(A.Location.X, A.Location.Y, A.Radius, amidPt.X, amidPt.Y);
            PointF Bp = CalcHelper.CalcCircleBoundPos(B.Location.X, B.Location.Y, B.Radius, amidPt.X, amidPt.Y);
            _preAp = Ap;
            _preBp = Bp;

            m_ArrowLine = new SimsArrowLine(Ap.X, Ap.Y, amidPt.X, amidPt.Y, Bp.X, Bp.Y);
        }

        internal protected void UpdateDrawPath()
        {
            PointF amidPt = m_ArrowLine.Amid;
            PointF Ap = CalcHelper.CalcCircleBoundPos(A.Location.X, A.Location.Y, A.Radius, amidPt.X, amidPt.Y);
            PointF Bp = CalcHelper.CalcCircleBoundPos(B.Location.X, B.Location.Y, B.Radius, amidPt.X, amidPt.Y);

            // calculate the new amid point
            float x2 = CalcHelper.CalcScaleDot(_preAp.X, _preBp.X, amidPt.X, Ap.X, Bp.X);
            float y2 = CalcHelper.CalcScaleDot(_preAp.Y, _preBp.Y, amidPt.Y, Ap.Y, Bp.Y);
            Debug.Print("x2 {0} y2 {1}", x2, y2);

            m_ArrowLine.SetFromPoint(Ap.X, Ap.Y);
            m_ArrowLine.SetAmidPoint(x2, y2);
            m_ArrowLine.SetToPoint(Bp.X, Bp.Y);
            m_ArrowLine.UpdateDrawPath();

            //# next round
            _preAp = Ap;
            _preBp = Bp;
        }

        public void Paint(Graphics g)
        {
            m_ArrowLine.Draw(g);

            ////
            //g.FillEllipse(Brushes.Red, _preAp.X - 3, _preAp.Y - 3, 6, 6);
            //g.FillEllipse(Brushes.Red, _preBp.X - 3, _preBp.Y - 3, 6, 6);
        }

        public void PaintFocus(Graphics g)
        {
            m_ArrowLine.DrawFocus(g);
        }

        public bool HitTest(Point pos)
        {
            return m_ArrowLine.HitTest(pos);
        }

        public void SetAmidPoint(Point pos)
        {
            m_ArrowLine.SetAmidPoint(pos.X, pos.Y);
            this.UpdateDrawPath();
        }
    }

    public class CalcHelper
    {
        /// <summary>
        /// 計算圓與中剖線交點
        /// </summary>
        public static PointF CalcCircleBoundPos(float cx, float cy, float r, float cx2, float cy2)
        {
            float H = cy2 - cy;
            float W = cx2 - cx;
            float R = (float)Math.Sqrt(H * H + W * W);
            float rate = r / R;

            float w = W * rate;
            float h = H * rate;

            return new PointF(cx + w, cy + h);
        }

        /// <summary>
        /// 計算點到線的距離
        /// </summary>
        public static double CalcDotToLineDistance(double cx, double cy, double x1, double y1, double x2, double y2)
        {
            // 計算點到線的距離
            return (Math.Abs((y2 - y1) * cx + (x1 - x2) * cy + ((x2 * y1) - (x1 * y2))))
                 / (Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
        }

        // 計算申縮點
        public static float CalcScaleDot(float x1, float x2, float xc, float x1p, float x2p)
        {
            // float r = (xc  - x1 ) / (x2  - x1 ) = (xcp - x1p) / (x2p - x1p);

            float xcp;
            if (x1 == x2)
            {
                xcp = xc;
            }
            else
            {
                xcp = (xc - x1) / (x2 - x1) * (x2p - x1p) + x1p;
            }

            return xcp;
        }
    }

}
