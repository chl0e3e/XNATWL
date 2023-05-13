using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;

namespace XNATWL.Test
{
    public class GraphDemoDialog1 : FadeFrame
    {
        private SimpleGraphLineModel gmMsPerFrame;
        private long lastTime = nanoTime();

        private static long nanoTime()
        {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }

        public GraphDemoDialog1()
        {
            gmMsPerFrame = new SimpleGraphLineModel("default", 100, 0, 30);
            GraphLineModel[] lineModel = new GraphLineModel[] { gmMsPerFrame };

            Graph graph = new Graph(new SimpleGraphModel(lineModel));
            graph.setTheme("/graph");

            setTheme(SimpleTest.WITH_TITLE);
            setTitle("MS per frame");
            add(graph);
        }

        protected override void paint(GUI gui)
        {
            long time = nanoTime();
            gmMsPerFrame.AddPoint((float)(time - lastTime) * 1e-6f);
            lastTime = time;

            base.paint(gui);
        }
    }
}
