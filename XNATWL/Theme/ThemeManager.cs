using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Renderer;

namespace XNATWL.Theme
{
    public class ThemeManager
    {
        ParameterMapImpl emptyMap;
        ParameterListImpl emptyList;

        private ThemeManager(Renderer.Renderer renderer, CacheContext cacheContext)
        {
            this.constants = new ParameterMapImpl(this, null);
            this.renderer = renderer;
            this.cacheContext = cacheContext;
            this.imageManager = new ImageManager(constants, renderer);
            this.fonts  = new Dictionary<String, Font>();
            this.themes = new Dictionary<String, ThemeInfoImpl>();
            this.inputMaps = new Dictionary<String, InputMap>();
            this.emptyMap = new ParameterMapImpl(this, null);
            this.emptyList = new ParameterListImpl(this, null);
            this.mathInterpreter = new MathInterpreter();
        }   
    }
}
