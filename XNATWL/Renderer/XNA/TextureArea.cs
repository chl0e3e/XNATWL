using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer.XNA
{
    public class TextureArea : TextureAreaBase, Image, SupportsDrawRepeat, QueriablePixels
    {
        protected static int REPEAT_CACHE_SIZE = 10;

        protected Color tintColor;
        protected int repeatCacheID = -1;

        public TextureArea(XNATexture texture, int x, int y, int width, int height, Color tintColor) : base(texture, x, y, width, height)
        {
            this.tintColor = (tintColor == null) ? Color.WHITE : tintColor;
        }

        public TextureArea(TextureArea src, Color tintColor) : base(src)
        {
            this.tintColor = tintColor;
        }

        public int Width
        {
            get
            {
                return this.getWidth();
            }
        }

        public int Height
        {
            get
            {
                return this.getHeight();
            }
        }

        public int PixelValueAt(int x, int y)
        {
            /*if (x < 0 || y < 0 || x >= width || y >= height)
            {
                throw new ArgumentOutOfRangeException();
            }

            int texWidth = texture.Width;
            int texHeight = texture.Height;

            int baseX = (int)(tx0 * texWidth);
            int baseY = (int)(ty0 * texHeight);

            if (tx0 > tx1)
            {
                x = baseX - x;
            }
            else
            {
                x = baseX + x;
            }

            if (ty0 > ty1)
            {
                y = baseY - y;
            }
            else
            {
                y = baseY + y;
            }

            if (x < 0)
            {
                x = 0;
            }
            else if (x >= texWidth)
            {
                x = texWidth - 1;
            }

            if (y < 0)
            {
                y = 0;
            }
            else if (y >= texHeight)
            {
                y = texHeight - 1;
            }*/

            return 0;
            //return texture.getPixelValue(x, y);
        }

        public void Draw(AnimationState animationState, int x, int y)
        {
            this.Draw(animationState, x, y, width, height);
        }

        public void Draw(AnimationState animationState, int x, int y, int w, int h)
        {
            drawQuad(this.tintColor, x, y, w, h);
        }

        public void Draw(AnimationState animationState, int x, int y, int width, int height, int repeatCountX, int repeatCountY)
        {
            if ((repeatCountX * this.width != width) || (repeatCountY * this.height != height))
            {
                DrawRepeatSlow(x, y, width, height, repeatCountX, repeatCountY);
                return;
            }

            if (repeatCountX < REPEAT_CACHE_SIZE || repeatCountY < REPEAT_CACHE_SIZE)
            {
                DrawRepeat(x, y, repeatCountX, repeatCountY);
                return;
            }

            DrawRepeatCached(x, y, repeatCountX, repeatCountY);
        }

        private void DrawRepeatSlow(int x, int y, int width, int height, int repeatCountX, int repeatCountY)
        {
            //GL11.glBegin(GL11.GL_QUADS);
            while (repeatCountY > 0)
            {
                int rowHeight = height / repeatCountY;

                int cx = 0;
                for (int xi = 0; xi < repeatCountX;)
                {
                    int nx = ++xi * width / repeatCountX;
                    drawQuad(this.tintColor, x + cx, y, nx - cx, rowHeight);
                    cx = nx;
                }

                y += rowHeight;
                height -= rowHeight;
                repeatCountY--;
            }
            //GL11.glEnd();
        }

        protected void DrawRepeat(int x, int y, int repeatCountX, int repeatCountY)
        {
            int w = width;
            int h = height;
            //GL11.glBegin(GL11.GL_QUADS);
            while (repeatCountY-- > 0)
            {
                int curX = x;
                int cntX = repeatCountX;
                while (cntX-- > 0)
                {
                    drawQuad(this.tintColor, curX, y, w, h);
                    curX += w;
                }
                y += h;
            }
            //GL11.glEnd();
        }

        protected void DrawRepeatCached(int x, int y, int repeatCountX, int repeatCountY)
        {
            if (repeatCacheID < 0)
            {
                CreateRepeatCache();
            }

            int cacheBlocksX = repeatCountX / REPEAT_CACHE_SIZE;
            int repeatsByCacheX = cacheBlocksX * REPEAT_CACHE_SIZE;

            if (repeatCountX > repeatsByCacheX)
            {
                DrawRepeat(x + width * repeatsByCacheX, y,
                        repeatCountX - repeatsByCacheX, repeatCountY);
            }

            do
            {
                throw new NotImplementedException();
                //GL11.glPushMatrix();
                //GL11.glTranslatef(x, y, 0f);
                //GL11.glCallList(repeatCacheID);

                for (int i = 1; i < cacheBlocksX; i++)
                {
                    //GL11.glTranslatef(width * REPEAT_CACHE_SIZE, 0f, 0f);
                    //GL11.glCallList(repeatCacheID);
                }

                //GL11.glPopMatrix();
                repeatCountY -= REPEAT_CACHE_SIZE;
                y += height * REPEAT_CACHE_SIZE;
            } while (repeatCountY >= REPEAT_CACHE_SIZE);

            if (repeatCountY > 0)
            {
                DrawRepeat(x, y, repeatsByCacheX, repeatCountY);
            }
        }

        protected void CreateRepeatCache()
        {
            throw new NotImplementedException();
            //repeatCacheID = GL11.glGenLists(1);
            //texture.renderer.textureAreas.add(this);

            //GL11.glNewList(repeatCacheID, GL11.GL_COMPILE);
            //drawRepeat(0, 0, REPEAT_CACHE_SIZE, REPEAT_CACHE_SIZE);
            //GL11.glEndList();
        }

        void DestroyRepeatCache()
        {
            throw new NotImplementedException();
            //GL11.glDeleteLists(repeatCacheID, 1);
            //repeatCacheID = -1;
        }

        public Image CreateTintedVersion(Color color)
        {
            if (color == null)
            {
                throw new NullReferenceException("color");
            }
            Color newTintColor = tintColor.Multiply(color);
            if (newTintColor.Equals(tintColor))
            {
                return this;
            }
            return new TextureArea(this, newTintColor);
        }
    }
}
