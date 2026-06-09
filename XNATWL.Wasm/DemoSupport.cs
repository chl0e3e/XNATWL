using System;

namespace XNATWL.Test
{
    /// <summary>
    /// Minimal shim that provides the handful of symbols the demo dialogs reference from the
    /// desktop <c>SimpleTest</c> harness (which is not linked into the wasm build). Keep the names
    /// and values in sync with <c>XNATWL.Test/SimpleTest.cs</c>.
    /// </summary>
    public class SimpleTest
    {
        public static readonly string WITH_TITLE = "resizableframe-title";
        public static readonly string WITHOUT_TITLE = "resizableframe";

        public class StyleItem
        {
            public string Theme;
            public string Name;

            public StyleItem(string theme, string name)
            {
                this.Theme = theme;
                this.Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
