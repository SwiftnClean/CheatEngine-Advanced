using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CheatEngine.NET.GUI
{
    /// <summary>
    /// A custom button control that displays "PC" with a glowing rainbow effect
    /// </summary>
    public class RainbowPcButton : Control
    {
        // Timer for animation
        private readonly System.Windows.Forms.Timer _animationTimer = new System.Windows.Forms.Timer();
        
        // Current hue for rainbow effect (0-360)
        private float _currentHue = 0;
        
        // Glow intensity
        private float _glowIntensity = 1.0f;
        private bool _glowIncreasing = true;
        
        // Mouse state tracking
        private bool _isMouseOver = false;
        private bool _isMouseDown = false;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RainbowPcButton"/> class
        /// </summary>
        public RainbowPcButton()
        {
            // Set default properties
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.UserPaint, true);
            
            BackColor = Color.Black;
            ForeColor = Color.White;
            Size = new Size(40, 23);
            Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            
            // Set up animation timer
            _animationTimer.Interval = 50; // 20 FPS
            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Start();
        }
        
        /// <summary>
        /// Clean up any resources being used
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer.Stop();
                _animationTimer.Dispose();
            }
            base.Dispose(disposing);
        }
        
        /// <summary>
        /// Handles the animation timer tick event
        /// </summary>
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // Update hue for rainbow effect
            _currentHue = (_currentHue + 2) % 360;
            
            // Update glow intensity
            if (_glowIncreasing)
            {
                _glowIntensity += 0.05f;
                if (_glowIntensity >= 1.5f)
                {
                    _glowIntensity = 1.5f;
                    _glowIncreasing = false;
                }
            }
            else
            {
                _glowIntensity -= 0.05f;
                if (_glowIntensity <= 0.8f)
                {
                    _glowIntensity = 0.8f;
                    _glowIncreasing = true;
                }
            }
            
            // Redraw the control
            Invalidate();
        }
        
        /// <summary>
        /// Handles the paint event
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Calculate dimensions
            Rectangle rect = ClientRectangle;
            int padding = 2;
            Rectangle innerRect = new Rectangle(
                rect.X + padding,
                rect.Y + padding,
                rect.Width - (padding * 2),
                rect.Height - (padding * 2));
            
            // Create rainbow color from current hue
            Color rainbowColor = ColorFromHSV(_currentHue, 1.0f, 1.0f);
            
            // Create glow brush
            using (GraphicsPath path = CreateRoundedRectangle(innerRect, 3))
            {
                // Draw glow effect
                int glowSize = (int)(8 * _glowIntensity);
                for (int i = glowSize; i > 0; i--)
                {
                    int alpha = (int)(200 * (1 - (i / (float)glowSize)));
                    Color glowColor = Color.FromArgb(alpha, rainbowColor);
                    
                    using (Pen glowPen = new Pen(glowColor, i))
                    {
                        glowPen.LineJoin = LineJoin.Round;
                        g.DrawPath(glowPen, path);
                    }
                }
                
                // Fill background
                Color bgColor = _isMouseDown ? Color.FromArgb(60, 60, 60) : 
                               (_isMouseOver ? Color.FromArgb(40, 40, 40) : BackColor);
                using (SolidBrush bgBrush = new SolidBrush(bgColor))
                {
                    g.FillPath(bgBrush, path);
                }
                
                // Draw border
                using (Pen borderPen = new Pen(rainbowColor, 1.5f))
                {
                    g.DrawPath(borderPen, path);
                }
                
                // Draw text
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    
                    // Create gradient brush for text
                    using (LinearGradientBrush textBrush = new LinearGradientBrush(
                        innerRect,
                        Color.White,
                        rainbowColor,
                        LinearGradientMode.ForwardDiagonal))
                    {
                        g.DrawString("PC", Font, textBrush, innerRect, sf);
                    }
                }
            }
        }
        
        /// <summary>
        /// Creates a rounded rectangle path
        /// </summary>
        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            
            // Top left corner
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            
            // Top edge and top right corner
            path.AddArc(rect.Right - (radius * 2), rect.Y, radius * 2, radius * 2, 270, 90);
            
            // Right edge and bottom right corner
            path.AddArc(rect.Right - (radius * 2), rect.Bottom - (radius * 2), radius * 2, radius * 2, 0, 90);
            
            // Bottom edge and bottom left corner
            path.AddArc(rect.X, rect.Bottom - (radius * 2), radius * 2, radius * 2, 90, 90);
            
            // Close the path
            path.CloseFigure();
            
            return path;
        }
        
        /// <summary>
        /// Converts HSV color values to a Color object
        /// </summary>
        private Color ColorFromHSV(float hue, float saturation, float value)
        {
            int hi = (int)(Math.Floor(hue / 60)) % 6;
            float f = (hue / 60) - (float)Math.Floor(hue / 60);
            
            value = value * 255;
            int v = (int)value;
            int p = (int)(value * (1 - saturation));
            int q = (int)(value * (1 - f * saturation));
            int t = (int)(value * (1 - (1 - f) * saturation));
            
            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }
        
        /// <summary>
        /// Handles the mouse enter event
        /// </summary>
        protected override void OnMouseEnter(EventArgs e)
        {
            _isMouseOver = true;
            Invalidate();
            base.OnMouseEnter(e);
        }
        
        /// <summary>
        /// Handles the mouse leave event
        /// </summary>
        protected override void OnMouseLeave(EventArgs e)
        {
            _isMouseOver = false;
            Invalidate();
            base.OnMouseLeave(e);
        }
        
        /// <summary>
        /// Handles the mouse down event
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = true;
                Invalidate();
            }
            base.OnMouseDown(e);
        }
        
        /// <summary>
        /// Handles the mouse up event
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = false;
                Invalidate();
            }
            base.OnMouseUp(e);
        }
    }
}
