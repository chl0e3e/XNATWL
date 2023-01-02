using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
