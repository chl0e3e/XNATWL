using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static XNATWL.Utils.Logger;
using XNATWL.Utils;

namespace XNATWL
{
    public class Timer
    {
        private static int TIMER_COUNTER_IN_CALLBACK = -1;
        private static int TIMER_COUNTER_DO_START = -2;
        private static int TIMER_COUNTER_DO_STOP = -3;

        GUI gui;
        int counter;
        int delay = 10;
        bool continuous;
        public event EventHandler<TimerTickEventArgs> Tick;

        /**
         * Constructs a new timer
         *
         * @param gui the GUI instance
         * @throws NullPointerException when gui is null
         */
        public Timer(GUI gui)
        {
            if (gui == null)
            {
                throw new NullReferenceException("gui");
            }
            this.gui = gui;
        }

        /**
         * Returns true if the timer is already running.
         * @return true if the timer is already running.
         */
        public bool isRunning()
        {
            return counter > 0 || (continuous && counter == TIMER_COUNTER_IN_CALLBACK);
        }

        /**
         * Sets the delay in ms till next expiration.
         *
         * @param delay in ms
         * @throws IllegalArgumentException if delay < 1 ms
         */
        public void setDelay(int delay)
        {
            if (delay < 1)
            {
                throw new ArgumentOutOfRangeException("delay < 1");
            }
            this.delay = delay;
        }

        /**
         * Starts the timer. If it is already running then this method does nothing.
         */
        public void start()
        {
            if (counter == 0)
            {
                counter = delay;
                gui.activeTimers.Add(this);
            }
            else if (counter < 0)
            {
                counter = TIMER_COUNTER_DO_START;
            }
        }

        /**
         * Stops the timer. If the timer is not running then this method does nothing.
         */
        public void stop()
        {
            if (counter > 0)
            {
                counter = 0;
                gui.activeTimers.Remove(this);
            }
            else if (counter < 0)
            {
                counter = TIMER_COUNTER_DO_STOP;
            }
        }

        /**
         * Returns true if the timer is a continous firing timer.
         * @return true if the timer is a continous firing timer.
         */
        public bool isContinuous()
        {
            return continuous;
        }

        /**
         * Sets the timer continous mode. A timer in continous mode must be stopped manually.
         * @param continuous true if the timer should auto restart after firing.
         */
        public void setContinuous(bool continuous)
        {
            this.continuous = continuous;
        }

        internal bool tick(int delta)
        {
            int newCounter = counter - delta;
            if (newCounter <= 0)
            {
                bool doStop = !continuous;
                counter = TIMER_COUNTER_IN_CALLBACK;
                this.Tick.Invoke(this, new TimerTickEventArgs());
                if (counter == TIMER_COUNTER_DO_STOP)
                {
                    counter = 0;
                    return false;
                }
                if (doStop && counter != TIMER_COUNTER_DO_START)
                {
                    counter = 0;
                    return false;
                }
                // timer is already running
                counter = Math.Max(1, newCounter + delay);
            }
            else
            {
                counter = newCounter;
            }
            return true;
        }
    }

    public class TimerTickEventArgs : EventArgs
    {
    }
}
