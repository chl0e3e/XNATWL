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

namespace XNATWL
{

    public class DraggableButton : Button
    {

        /**
         * The listener interface which receives all drag related events
         */
        public interface DragListener
        {
            /**
             * Called when the user starts dragging the button
             */
            void dragStarted();

            /**
             * The mouse was moved
             *
             * @param deltaX the delta mouse X position since the drag was started
             * @param deltaY the delta mouse Y position since the drag was started
             */
            void dragged(int deltaX, int deltaY);

            /**
             * The user has stopped dragging the button
             */
            void dragStopped();
        }

        private int dragStartX;
        private int dragStartY;
        private bool dragging;

        private DragListener listener;

        public DraggableButton()
        {
        }

        /**
         * Creates a DraggableButton with a shared animation state
         *
         * @param animState the animation state to share, can be null
         */
        public DraggableButton(AnimationState animState) : base(animState)
        {

        }

        /**
         * Creates a DraggableButton with a shared or inherited animation state
         *
         * @param animState the animation state to share or inherit, can be null
         * @param inherit true if the animation state should be inherited false for sharing
         */
        public DraggableButton(AnimationState animState, bool inherit) : base(animState, inherit)
        {
            
        }

        public bool isDragActive()
        {
            return dragging;
        }

        public DragListener getListener()
        {
            return listener;
        }

        /**
         * Sets the DragListener. Only one listener can be set. Setting a new one
         * will replace the previous one.
         *
         * Changing the listener while a drag is active will result in incomplete
         * events for both listeners (previous and new one).
         * 
         * @param listener the new listener or null
         */
        public void setListener(DragListener listener)
        {
            this.listener = listener;
        }

        //@Override
        public override bool handleEvent(Event evt)
        {
            if (evt.isMouseEvent() && dragging)
            {
                if (evt.getEventType() == Event.EventType.MOUSE_DRAGGED)
                {
                    if (listener != null)
                    {
                        listener.dragged(evt.getMouseX() - dragStartX, evt.getMouseY() - dragStartY);
                    }
                }
                if (evt.isMouseDragEnd())
                {
                    stopDragging(evt);
                }
                return true;
            }

            if (evt.getEventType() == Event.EventType.MOUSE_BTNDOWN)
            {
                dragStartX = evt.getMouseX();
                dragStartY = evt.getMouseY();
            }
            else if (evt.getEventType() == Event.EventType.MOUSE_DRAGGED)
            {
                System.Diagnostics.Debug.Assert(!dragging);
                dragging = true;
                getModel().Armed = (false);
                getModel().Pressed = (true);
                if (listener != null)
                {
                    listener.dragStarted();
                }
                return true;
            }

            return base.handleEvent(evt);
        }

        private void stopDragging(Event evt)
        {
            if (listener != null)
            {
                listener.dragStopped();
            }
            dragging = false;
            getModel().Armed = (false);
            getModel().Pressed = (false);
            getModel().Hover = (isMouseInside(evt));
        }
    }
}
