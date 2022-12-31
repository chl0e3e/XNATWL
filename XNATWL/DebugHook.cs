using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL
{
    internal class DebugHook
    {
        public static DebugHook INSTANCE = new DebugHook();

        /**
         * Returns the currently active debug hook for this thread.
         * @return the debug hook. Never null.
         */
        public static DebugHook getDebugHook()
        {
            return DebugHook.INSTANCE;
        }

        /**
         * Installs a new debug hook.
         *
         * @param hook the new debug hook
         * @return the previous debug hook
         * @throws NullPointerException if hook is null
         */
        /*public static DebugHook installHook(DebugHook hook)
        {
            if (hook == null)
            {
                throw new ArgumentNullException("hook");
            }

            INSTANCE = hook;
        }*/

        public void beforeApplyTheme(Widget widget)
        {
        }

        public void afterApplyTheme(Widget widget)
        {
        }

        public void missingTheme(String themePath)
        {
            System.Diagnostics.Debug.WriteLine("Could not find theme: " + themePath);
        }

        public void missingChildTheme(ThemeInfo parent, String theme)
        {
            System.Diagnostics.Debug.WriteLine("Missing child theme \"" + theme + "\" for \"" + parent.getThemePath() + "\"");
        }

        public void missingParameter(ParameterMap map, String paramName, String parentDescription, Type dataType)
        {
            StringBuilder sb = new StringBuilder("Parameter \"").Append(paramName).Append("\" ");
            if (dataType != null)
            {
                sb.Append("of type ");
                if (dataType.IsEnum)
                {
                    sb.Append("enum ");
                }
                sb.Append('"').Append(dataType.Name).Append('"');
            }
            sb.Append(" not set");
            if (map is ThemeInfo) {
                sb.Append(" for \"").Append(((ThemeInfo)map).getThemePath()).Append("\"");
            } else
            {
                sb.Append(parentDescription);
            }
            System.Diagnostics.Debug.WriteLine(sb.ToString());
        }

        public void wrongParameterType(ParameterMap map, String paramName, Type expectedType, Type foundType, String parentDescription)
        {
            System.Diagnostics.Debug.WriteLine("Parameter \"" + paramName + "\" is a " +
                    foundType.Name + " expected a " +
                    expectedType.Name + parentDescription);
        }

        public void wrongParameterType(ParameterList map, int idx, Type expectedType, Type foundType, String parentDescription)
        {
            System.Diagnostics.Debug.WriteLine("Parameter at index " + idx + " is a " +
                    foundType.Name + " expected a " +
                    expectedType.Name + parentDescription);
        }

        public void replacingWithDifferentType(ParameterMap map, String paramName, Type oldType, Type newType, String parentDescription)
        {
            System.Diagnostics.Debug.WriteLine("Paramter \"" + paramName + "\" of type " +
                    oldType + " is replaced with type " + newType + parentDescription);
        }

        public void missingImage(String name)
        {
            System.Diagnostics.Debug.WriteLine("Could not find image: " + name);
        }

        /**
         * Called when GUI has validated the layout tree
         * @param iterations the number of iterations required to solve layout
         * @param loop the widgets involved in a layout loop if the layout could not be solved - is null if layout was solved
         */
        public void guiLayoutValidated(int iterations, ICollection<Widget> loop)
        {
            if (loop != null)
            {
                System.Diagnostics.Debug.WriteLine("WARNING: layout loop detected - printing");
                int index = 1;
                foreach (Widget w in loop)
                {
                    System.Diagnostics.Debug.WriteLine(index + ": " + w);
                    index++;
                }
            }
        }

        /**
         * Called when wildcard resolution failed to find a theme and the fallback theme was specified
         * @param themePath the requested theme name
         */
        public void usingFallbackTheme(String themePath)
        {
            System.Diagnostics.Debug.WriteLine("Selected fallback theme for missing theme \"" + themePath + "\"");
        }
    }
}
