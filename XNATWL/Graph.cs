using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using XNATWL.Renderer;
using XNATWL.TextAreaModel;
using XNATWL.Utils;

namespace XNATWL
{
    public class Graph : Widget
    {
        private GraphArea area;

        GraphModel model;
        private ParameterMap themeLineStyles;
        private int sizeMultipleX = 1;
        private int sizeMultipleY = 1;

        LineStyle[] lineStyles = new LineStyle[8];
        private float[] renderXYBuffer = new float[128];

        public Graph()
        {
            area = new GraphArea(this);
            area.setClip(true);
            add(area);
        }

        public Graph(GraphModel model) : this()
        {
            setModel(model);
        }

        public GraphModel getModel()
        {
            return model;
        }

        public void setModel(GraphModel model)
        {
            this.model = model;
            invalidateLineStyles();
        }

        public int getSizeMultipleX()
        {
            return sizeMultipleX;
        }

        public void setSizeMultipleX(int sizeMultipleX)
        {
            if (sizeMultipleX < 1)
            {
                throw new ArgumentOutOfRangeException("sizeMultipleX must be >= 1");
            }
            this.sizeMultipleX = sizeMultipleX;
        }

        public int getSizeMultipleY()
        {
            return sizeMultipleY;
        }

        public void setSizeMultipleY(int sizeMultipleY)
        {
            if (sizeMultipleY < 1)
            {
                throw new ArgumentOutOfRangeException("sizeMultipleX must be >= 1");
            }
            this.sizeMultipleY = sizeMultipleY;
        }

        protected override void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            applyThemeGraph(themeInfo);
        }

        protected void applyThemeGraph(ThemeInfo themeInfo)
        {
            this.themeLineStyles = themeInfo.getParameterMap("lineStyles");
            setSizeMultipleX(themeInfo.getParameter("sizeMultipleX", 1));
            setSizeMultipleY(themeInfo.getParameter("sizeMultipleY", 1));
            invalidateLineStyles();
        }

        protected void invalidateLineStyles()
        {
            for (int i = 0; i < lineStyles.Length; i++)
            {
                lineStyles[i] = null;
            }
        }

        void syncLineStyles()
        {
            int numLines = model.Lines;
            if (lineStyles.Length < numLines)
            {
                LineStyle[] newLineStyles = new LineStyle[numLines];
                Array.Copy(lineStyles, 0, newLineStyles, 0, lineStyles.Length);
                this.lineStyles = newLineStyles;
            }

            for (int i = 0; i < numLines; i++)
            {
                GraphLineModel line = model.LineAt(i);
                LineStyle style = lineStyles[i];
                if (style == null)
                {
                    style = new LineStyle();
                    lineStyles[i] = style;
                }
                String visualStyle = TextUtil.notNull(line.VisualStyleName);
                if (!style.name.Equals(visualStyle))
                {
                    ParameterMap lineStyle = null;
                    if (themeLineStyles != null)
                    {
                        lineStyle = themeLineStyles.getParameterMap(visualStyle);
                    }
                    style.setStyleName(visualStyle, lineStyle);
                }
            }
        }

        private static float EPSILON = 1e-4f;

        void renderLine(LineRenderer lineRenderer, GraphLineModel line,
                float minValue, float maxValue, LineStyle style)
        {
            int numPoints = line.Points;
            if (numPoints <= 0)
            {
                // nothing to render
                return;
            }

            if (renderXYBuffer.Length < numPoints * 2)
            {
                // no need to copy - we generate new values anyway
                renderXYBuffer = new float[numPoints * 2];
            }

            float[] xy = this.renderXYBuffer;

            float delta = maxValue - minValue;
            if (Math.Abs(delta) < EPSILON)
            {
                // Math.copySign is Java 1.6+
                delta = copySign(EPSILON, delta);
            }

            float yscale = (float)-getInnerHeight() / delta;
            float yoff = getInnerBottom();
            float xscale = (float)getInnerWidth() / (float)Math.Max(1, numPoints - 1);
            float xoff = getInnerX();

            for (int i = 0; i < numPoints; i++)
            {
                float value = line.Point(i);
                xy[i * 2 + 0] = i * xscale + xoff;
                xy[i * 2 + 1] = (value - minValue) * yscale + yoff;
            }

            if (numPoints == 1)
            {
                // a single point will be rendered as horizontal line
                // as we never shrink the xy array and the initial size is >= 4 we have enough room left
                xy[2] = xoff + xscale;
                xy[3] = xy[1];
                numPoints = 2;
            }

            lineRenderer.DrawLine(xy, numPoints, style.lineWidth, style.color, false);
        }

        private static float copySign(float magnitude, float sign)
        {
            // this copies the sign bit from sign to magnitude
            // it assumes the magnitude is positive
            ;
            int rawMagnitude = BitConverter.ToInt32(BitConverter.GetBytes(magnitude), 0);
            int rawSign = BitConverter.ToInt32(BitConverter.GetBytes(sign), 0);
            int rawResult = rawMagnitude | (rawSign & (1 << 31));
            return BitConverter.ToSingle(BitConverter.GetBytes(rawResult), 0);
        }

        public override bool setSize(int width, int height)
        {
            return base.setSize(
                    round(width, sizeMultipleX),
                    round(height, sizeMultipleY));
        }

        private static int round(int value, int grid)
        {
            return value - (value % grid);
        }

        protected override void layout()
        {
            layoutChildFullInnerArea(area);
        }

        public class LineStyle
        {
            internal String name = "";
            internal Color color = Color.WHITE;
            internal float lineWidth = 1.0f;

            internal void setStyleName(String name, ParameterMap lineStyle)
            {
                this.name = name;
                if (lineStyle != null)
                {
                    this.color = lineStyle.getParameter("color", Color.WHITE);
                    this.lineWidth = Math.Max(EPSILON, lineStyle.getParameter("width", 1.0f));
                }
            }
        }

        class GraphArea : Widget
        {
            private Graph _graph;

            public GraphArea(Graph graph)
            {
                this._graph = graph;
            }

            protected override void paintWidget(GUI gui)
            {
                if (this._graph.model != null)
                {
                    this._graph.syncLineStyles();
                    LineRenderer lineRenderer = gui.getRenderer().LineRenderer;

                    int numLines = this._graph.model.Lines;
                    bool independantScale = this._graph.model.ScaleLinesIndependent();
                    float minValue = float.MaxValue;
                    float maxValue = -float.MaxValue;
                    if (independantScale)
                    {
                        for (int i = 0; i < numLines; i++)
                        {
                            GraphLineModel line = this._graph.model.LineAt(i);
                            minValue = Math.Min(minValue, line.MinValue);
                            maxValue = Math.Max(maxValue, line.MaxValue);
                        }
                    }

                    for (int i = 0; i < numLines; i++)
                    {
                        GraphLineModel line = this._graph.model.LineAt(i);
                        LineStyle style = this._graph.lineStyles[i];
                        if (independantScale)
                        {
                            this._graph.renderLine(lineRenderer, line, minValue, maxValue, style);
                        }
                        else
                        {
                            this._graph.renderLine(lineRenderer, line, line.MinValue, line.MaxValue, style);
                        }
                    }
                }
            }

        }
    }

}
