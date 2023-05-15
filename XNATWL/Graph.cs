/*
 * Copyright (c) 2008-2012, Matthias Mann
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *     * Redistributions of source code must retain the above copyright notice,
 *       this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of Matthias Mann nor the names of its contributors may
 *       be used to endorse or promote products derived from this software
 *       without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using XNATWL.Model;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL
{
    public class Graph : Widget
    {
        private GraphArea _area;

        GraphModel _model;
        private ParameterMap _themeLineStyles;
        private int _sizeMultipleX = 1;
        private int _sizeMultipleY = 1;

        LineStyle[] _lineStyles = new LineStyle[8];
        private float[] _renderXYBuffer = new float[128];

        public Graph()
        {
            _area = new GraphArea(this);
            _area.SetClip(true);
            Add(_area);
        }

        public Graph(GraphModel model) : this()
        {
            SetModel(model);
        }

        public GraphModel GetModel()
        {
            return _model;
        }

        public void SetModel(GraphModel model)
        {
            this._model = model;
            InvalidateLineStyles();
        }

        public int GetSizeMultipleX()
        {
            return _sizeMultipleX;
        }

        public void SetSizeMultipleX(int sizeMultipleX)
        {
            if (sizeMultipleX < 1)
            {
                throw new ArgumentOutOfRangeException("sizeMultipleX must be >= 1");
            }
            this._sizeMultipleX = sizeMultipleX;
        }

        public int GetSizeMultipleY()
        {
            return _sizeMultipleY;
        }

        public void SetSizeMultipleY(int sizeMultipleY)
        {
            if (sizeMultipleY < 1)
            {
                throw new ArgumentOutOfRangeException("sizeMultipleX must be >= 1");
            }
            this._sizeMultipleY = sizeMultipleY;
        }

        protected override void ApplyTheme(ThemeInfo themeInfo)
        {
            base.ApplyTheme(themeInfo);
            ApplyThemeGraph(themeInfo);
        }

        protected void ApplyThemeGraph(ThemeInfo themeInfo)
        {
            this._themeLineStyles = themeInfo.GetParameterMap("lineStyles");
            SetSizeMultipleX(themeInfo.GetParameter("sizeMultipleX", 1));
            SetSizeMultipleY(themeInfo.GetParameter("sizeMultipleY", 1));
            InvalidateLineStyles();
        }

        protected void InvalidateLineStyles()
        {
            for (int i = 0; i < _lineStyles.Length; i++)
            {
                _lineStyles[i] = null;
            }
        }

        void SyncLineStyles()
        {
            int numLines = _model.Lines;
            if (_lineStyles.Length < numLines)
            {
                LineStyle[] newLineStyles = new LineStyle[numLines];
                Array.Copy(_lineStyles, 0, newLineStyles, 0, _lineStyles.Length);
                this._lineStyles = newLineStyles;
            }

            for (int i = 0; i < numLines; i++)
            {
                GraphLineModel line = _model.LineAt(i);
                LineStyle style = _lineStyles[i];
                if (style == null)
                {
                    style = new LineStyle();
                    _lineStyles[i] = style;
                }
                String visualStyle = TextUtil.NotNull(line.VisualStyleName);
                if (!style._name.Equals(visualStyle))
                {
                    ParameterMap lineStyle = null;
                    if (_themeLineStyles != null)
                    {
                        lineStyle = _themeLineStyles.GetParameterMap(visualStyle);
                    }
                    style.SetStyleName(visualStyle, lineStyle);
                }
            }
        }

        private static float EPSILON = 1e-4f;

        void RenderLine(LineRenderer lineRenderer, GraphLineModel line, float minValue, float maxValue, LineStyle style)
        {
            int numPoints = line.Points;
            if (numPoints <= 0)
            {
                // nothing to render
                return;
            }

            if (_renderXYBuffer.Length < numPoints * 2)
            {
                // no need to copy - we generate new values anyway
                _renderXYBuffer = new float[numPoints * 2];
            }

            float[] xy = this._renderXYBuffer;

            float delta = maxValue - minValue;
            if (Math.Abs(delta) < EPSILON)
            {
                // Math.copySign is Java 1.6+
                delta = CopySign(EPSILON, delta);
            }

            float yscale = (float)-GetInnerHeight() / delta;
            float yoff = GetInnerBottom();
            float xscale = (float)GetInnerWidth() / (float)Math.Max(1, numPoints - 1);
            float xoff = GetInnerX();

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

            lineRenderer.DrawLine(xy, numPoints, style._lineWidth, style._color, false);
        }

        private static float CopySign(float magnitude, float sign)
        {
            // this copies the sign bit from sign to magnitude
            // it assumes the magnitude is positive
            ;
            int rawMagnitude = BitConverter.ToInt32(BitConverter.GetBytes(magnitude), 0);
            int rawSign = BitConverter.ToInt32(BitConverter.GetBytes(sign), 0);
            int rawResult = rawMagnitude | (rawSign & (1 << 31));
            return BitConverter.ToSingle(BitConverter.GetBytes(rawResult), 0);
        }

        public override bool SetSize(int width, int height)
        {
            return base.SetSize(
                    Round(width, _sizeMultipleX),
                    Round(height, _sizeMultipleY));
        }

        private static int Round(int value, int grid)
        {
            return value - (value % grid);
        }

        protected override void Layout()
        {
            LayoutChildFullInnerArea(_area);
        }

        public class LineStyle
        {
            internal String _name = "";
            internal Color _color = Color.WHITE;
            internal float _lineWidth = 1.0f;

            internal void SetStyleName(String name, ParameterMap lineStyle)
            {
                this._name = name;
                if (lineStyle != null)
                {
                    this._color = lineStyle.GetParameter("color", Color.WHITE);
                    this._lineWidth = Math.Max(EPSILON, lineStyle.GetParameter("width", 1.0f));
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

            protected override void PaintWidget(GUI gui)
            {
                if (this._graph._model != null)
                {
                    this._graph.SyncLineStyles();
                    LineRenderer lineRenderer = gui.GetRenderer().LineRenderer;

                    int numLines = this._graph._model.Lines;
                    bool independantScale = this._graph._model.ScaleLinesIndependent();
                    float minValue = float.MaxValue;
                    float maxValue = -float.MaxValue;
                    if (independantScale)
                    {
                        for (int i = 0; i < numLines; i++)
                        {
                            GraphLineModel line = this._graph._model.LineAt(i);
                            minValue = Math.Min(minValue, line.MinValue);
                            maxValue = Math.Max(maxValue, line.MaxValue);
                        }
                    }

                    for (int i = 0; i < numLines; i++)
                    {
                        GraphLineModel line = this._graph._model.LineAt(i);
                        LineStyle style = this._graph._lineStyles[i];
                        if (independantScale)
                        {
                            this._graph.RenderLine(lineRenderer, line, minValue, maxValue, style);
                        }
                        else
                        {
                            this._graph.RenderLine(lineRenderer, line, line.MinValue, line.MaxValue, style);
                        }
                    }
                }
            }
        }
    }
}
