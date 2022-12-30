using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.Renderer
{
    public interface OffscreenRenderer
    {
        /**
         * Starts offscreen rendering. All following rendering operations will render
         * into the returned offscreen surface. Rendering outside the specified area
         * will be ignored.
         * 
         * @param widget the widget which will render to the returned surface - can be null.
         * @param oldSurface the previous offscreen surface to reuse / overwrite
         * @param x the X coordinate of the region, can be negative.
         * @param y the Y coordinate of the region, can be negative.
         * @param width the width, can be larger then the screen size
         * @param height the height, can be larger then the screen size
         * @return the OffscreenSurface or null if offscreen rendering could not be started.
         */
        OffscreenSurface StartOffscreenRendering(Widget widget,
                OffscreenSurface oldSurface, int x, int y, int width, int height);

        /**
         * Ends the current offscreen rendering.
         * Only call this method after a sucessful call of
         * {@link #startOffscreenRendering(de.matthiasmann.twl.renderer.OffscreenSurface, int, int, int, int) }
         */
        void EndOffscreenRendering();
    }
}
