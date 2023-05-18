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

namespace XNATWL.Renderer
{
    /// <summary>
    /// A helper class to implement tinting.
    /// </summary>
    public class TintStack
    {
        private static float ONE_OVER_255 = 1f;// / 255f;

        TintStack _prev;
        TintStack _next;
        float _r, _g, _b, _a;

        /// <summary>
        /// Create a clean tint stack
        /// </summary>
        public TintStack()
        {
            this._prev = this;
            this._r = ONE_OVER_255;
            this._g = ONE_OVER_255;
            this._b = ONE_OVER_255;
            this._a = ONE_OVER_255;
        }

        /// <summary>
        /// Create a new tint stack given a previous tint
        /// </summary>
        /// <param name="prev">Previous tint on the stack</param>
        private TintStack(TintStack prev)
        {
            this._prev = prev;
        }

        /// <summary>
        /// Push a reset tint to the stack
        /// </summary>
        /// <returns>New tint on the stack, set up using the default tint value</returns>
        public TintStack PushReset()
        {
            if (_next == null)
            {
                _next = new TintStack(this);
            }
            _next._r = ONE_OVER_255;
            _next._g = ONE_OVER_255;
            _next._b = ONE_OVER_255;
            _next._a = ONE_OVER_255;
            return _next;
        }

        /// <summary>
        /// Push a specific RGBA tinting on the stack 
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="a">Alpha</param>
        /// <returns>New tint on the stack, set up using parameter values</returns>
        public TintStack Push(float r, float g, float b, float a)
        {
            if (_next == null)
            {
                _next = new TintStack(this);
            }
            _next._r = this._r * r;
            _next._g = this._g * g;
            _next._b = this._b * b;
            _next._a = this._a * a;
            return _next;
        }

        /// <summary>
        /// Push a specific RGBA tinting on to the stack using values from <see cref="Color"/>
        /// </summary>
        /// <param name="color">Color to extract RGBA values from</param>
        /// <returns>New tint on the stack</returns>
        public TintStack Push(Color color)
        {
            return Push(
                    color.RedF,
                    color.GreenF,
                    color.BlueF,
                    color.AlphaF) ;
        }

        /// <summary>
        /// Returns a matching XNA colour object
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public Microsoft.Xna.Framework.Color XNATint(Color color)
        {
            return new Microsoft.Xna.Framework.Color(this._r * color.RedF, this._g * color.GreenF, this._b * color.BlueF, this._a * color.AlphaF);
        }

        /// <summary>
        /// Return the last tint on the stack
        /// </summary>
        /// <returns></returns>
        public TintStack Pop()
        {
            return _prev;
        }
    }
}
