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

namespace XNATWL
{
    public class Timer
    {
        private static int TIMER_COUNTER_IN_CALLBACK = -1;
        private static int TIMER_COUNTER_DO_START = -2;
        private static int TIMER_COUNTER_DO_STOP = -3;

        GUI _gui;
        int _counter;
        int _delay = 10;
        bool _continuous;

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
            this._gui = gui;
        }

        /**
         * Returns true if the timer is already running.
         * @return true if the timer is already running.
         */
        public bool IsRunning()
        {
            return _counter > 0 || (_continuous && _counter == TIMER_COUNTER_IN_CALLBACK);
        }

        /**
         * Sets the delay in ms till next expiration.
         *
         * @param delay in ms
         * @throws IllegalArgumentException if delay < 1 ms
         */
        public void SetDelay(int delay)
        {
            if (delay < 1)
            {
                throw new ArgumentOutOfRangeException("delay < 1");
            }
            this._delay = delay;
        }

        /**
         * Starts the timer. If it is already running then this method does nothing.
         */
        public void Start()
        {
            if (_counter == 0)
            {
                _counter = _delay;
                _gui._activeTimers.Add(this);
            }
            else if (_counter < 0)
            {
                _counter = TIMER_COUNTER_DO_START;
            }
        }

        /**
         * Stops the timer. If the timer is not running then this method does nothing.
         */
        public void Stop()
        {
            if (_counter > 0)
            {
                _counter = 0;
                _gui._activeTimers.Remove(this);
            }
            else if (_counter < 0)
            {
                _counter = TIMER_COUNTER_DO_STOP;
            }
        }

        /**
         * Returns true if the timer is a continous firing timer.
         * @return true if the timer is a continous firing timer.
         */
        public bool IsContinuous()
        {
            return _continuous;
        }

        /**
         * Sets the timer continous mode. A timer in continous mode must be stopped manually.
         * @param continuous true if the timer should auto restart after firing.
         */
        public void SetContinuous(bool continuous)
        {
            this._continuous = continuous;
        }

        public bool RunOneTick(int delta)
        {
            int newCounter = _counter - delta;
            if (newCounter <= 0)
            {
                bool doStop = !_continuous;
                _counter = TIMER_COUNTER_IN_CALLBACK;
                this.Tick.Invoke(this, new TimerTickEventArgs());
                if (_counter == TIMER_COUNTER_DO_STOP)
                {
                    _counter = 0;
                    return false;
                }
                if (doStop && _counter != TIMER_COUNTER_DO_START)
                {
                    _counter = 0;
                    return false;
                }
                // timer is already running
                _counter = Math.Max(1, newCounter + _delay);
            }
            else
            {
                _counter = newCounter;
            }
            return true;
        }
    }

    public class TimerTickEventArgs : EventArgs
    {
    }
}
