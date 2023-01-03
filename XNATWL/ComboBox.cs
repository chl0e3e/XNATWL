using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL
{
    public class ComboBox<T> : ComboBoxBase
    {
        public static StateKey STATE_ERROR = StateKey.Get("error");

        private static int INVALID_WIDTH = -1;

        private ComboboxLabel label;
        private ListBox<T> listbox;

        String displayTextNoSelection = "";
        bool noSelectionIsError;
        bool computeWidthFromModel;
        int modelWidth = INVALID_WIDTH;
        int selectionOnPopupOpen = ListBox<T>.NO_SELECTION;

        public event EventHandler<ComboBoxSelectionChangedEventArgs> SelectionChanged;

        public ComboBox(ListSelectionModel<T> model) : this()
        {
            setModel(model);
        }

        public ComboBox(ListModel<T> model, IntegerModel selectionModel) : this()
        {
            setModel(model);
            setSelectionModel(selectionModel);
        }

        public ComboBox(ListModel<T> model) : this()
        {
            setModel(model);
        }

        public ComboBox()
        {
            this.label = new ComboboxLabel(this, getAnimationState());
            this.listbox = new ComboboxListbox<T>();

            this.button.getModel().State += ComboBox_State;
            this.listbox.Callback += Listbox_Callback;

            popup.setTheme("comboboxPopup");
            popup.add(listbox);
            add(label);
        }

        private void Listbox_Callback(object sender, ListBoxEventArgs e)
        {
            switch (e.Reason)
            {
                case ListBoxCallbackReason.KEYBOARD_RETURN:
                case ListBoxCallbackReason.MOUSE_CLICK:
                case ListBoxCallbackReason.MOUSE_DOUBLE_CLICK:
                    listBoxSelectionChanged(true);
                    break;
                default:
                    listBoxSelectionChanged(false);
                    break;
            }
        }

        private void ComboBox_State(object sender, ButtonStateChangedEventArgs e)
        {
            this.updateHover();
        }

        public void setModel(ListModel<T> model)
        {
            unregisterModelChangeListener();
            listbox.setModel(model);
            if (computeWidthFromModel)
            {
                registerModelChangeListener();
            }
        }

        public ListModel<T> getModel()
        {
            return listbox.getModel();
        }

        public void setSelectionModel(IntegerModel selectionModel)
        {
            listbox.setSelectionModel(selectionModel);
        }

        public IntegerModel getSelectionModel()
        {
            return listbox.getSelectionModel();
        }

        public void setModel(ListSelectionModel<T> model)
        {
            listbox.setModel(model);
        }

        public void setSelected(int selected)
        {
            listbox.setSelected(selected);
            updateLabel();
        }

        public int getSelected()
        {
            return listbox.getSelected();
        }

        public bool isComputeWidthFromModel()
        {
            return computeWidthFromModel;
        }

        public void setComputeWidthFromModel(bool computeWidthFromModel)
        {
            if (this.computeWidthFromModel != computeWidthFromModel)
            {
                this.computeWidthFromModel = computeWidthFromModel;
                if (computeWidthFromModel)
                {
                    registerModelChangeListener();
                }
                else
                {
                    unregisterModelChangeListener();
                }
            }
        }

        public String getDisplayTextNoSelection()
        {
            return displayTextNoSelection;
        }

        /**
         * Sets the text to display when nothing is selected.
         * Default is {@code ""}
         *
         * @param displayTextNoSelection the text to display
         * @throws NullPointerException when displayTextNoSelection is null
         */
        public void setDisplayTextNoSelection(String displayTextNoSelection)
        {
            if (displayTextNoSelection == null)
            {
                throw new ArgumentNullException("displayTextNoSelection");
            }
            this.displayTextNoSelection = displayTextNoSelection;
            updateLabel();
        }

        public bool isNoSelectionIsError()
        {
            return noSelectionIsError;
        }

        /**
         * Controls the value of {@link #STATE_ERROR} on the combobox display when nothing is selected.
         * Default is false.
         * 
         * @param noSelectionIsError
         */
        public void setNoSelectionIsError(bool noSelectionIsError)
        {
            this.noSelectionIsError = noSelectionIsError;
            updateLabel();
        }

        private void registerModelChangeListener()
        {
            ListModel<T> model = getModel();
            if (model != null)
            {
                modelWidth = INVALID_WIDTH;
                model.AllChanged += Model_AllChanged;
                model.EntriesChanged += Model_EntriesChanged;
                model.EntriesDeleted += Model_EntriesDeleted;
                model.EntriesInserted += Model_EntriesInserted;
            }
        }

        private void Model_EntriesInserted(object sender, ListSubsetChangedEventArgs e)
        {
            updateModelWidth(e.First, e.Last);
        }

        private void Model_EntriesDeleted(object sender, ListSubsetChangedEventArgs e)
        {
            invalidateModelWidth();
        }

        private void Model_EntriesChanged(object sender, ListSubsetChangedEventArgs e)
        {
            invalidateModelWidth();
        }

        private void Model_AllChanged(object sender, ListAllChangedEventArgs e)
        {
            invalidateModelWidth();
        }

        private void unregisterModelChangeListener()
        {
                ListModel<T> model = getModel();
                if (model != null)
                {
                    model.AllChanged -= Model_AllChanged;
                    model.EntriesChanged -= Model_EntriesChanged;
                    model.EntriesDeleted -= Model_EntriesDeleted;
                    model.EntriesInserted -= Model_EntriesInserted;
                }
        }

        protected override bool openPopup()
        {
            if (base.openPopup())
            {
                popup.validateLayout();
                selectionOnPopupOpen = getSelected();
                listbox.scrollToSelected();
                return true;
            }
            return false;
        }

        protected override void popupEscapePressed(Event evt)
        {
            setSelected(selectionOnPopupOpen);
            base.popupEscapePressed(evt);
        }

        /**
         * Called when a right click was made on the ComboboxLabel.
         * The default implementation does nothing
         */
        protected void handleRightClick()
        {
        }

        protected void listBoxSelectionChanged(bool close)
        {
            updateLabel();
            if (close)
            {
                popup.closePopup();
            }
            this.SelectionChanged.Invoke(this, new ComboBoxSelectionChangedEventArgs());
        }

        protected String getModelData(int idx)
        {
            return getModel().EntryAt(idx).ToString();
        }

        protected override Widget getLabel()
        {
            return label;
        }

        protected void updateLabel()
        {
            int selected = getSelected();
            if (selected == ListBox<T>.NO_SELECTION)
            {
                label.setText(displayTextNoSelection);
                label.getAnimationState().setAnimationState(STATE_ERROR, noSelectionIsError);
            }
            else
            {
                label.setText(getModelData(selected));
                label.getAnimationState().setAnimationState(STATE_ERROR, false);
            }
            if (!computeWidthFromModel)
            {
                invalidateLayout();
            }
        }

        protected void applyTheme(ThemeInfo themeInfo)
        {
            base.applyTheme(themeInfo);
            modelWidth = INVALID_WIDTH;
        }

        public override bool handleEvent(Event evt)
        {
            if (base.handleEvent(evt))
            {
                return true;
            }
            if (evt.isKeyPressedEvent())
            {
                switch (evt.getKeyCode())
                {
                    case Event.KEY_UP:
                    case Event.KEY_DOWN:
                    case Event.KEY_HOME:
                    case Event.KEY_END:
                        // let the listbox handle this :)
                        listbox.handleEvent(evt);
                        return true;
                    case Event.KEY_SPACE:
                    case Event.KEY_RETURN:
                        openPopup();
                        return true;
                }
            }
            return false;
        }

        void invalidateModelWidth()
        {
            if (computeWidthFromModel)
            {
                modelWidth = INVALID_WIDTH;
                invalidateLayout();
            }
        }

        void updateModelWidth()
        {
            if (computeWidthFromModel)
            {
                modelWidth = 0;
                updateModelWidth(0, getModel().Entries - 1);
            }
        }

        void updateModelWidth(int first, int last)
        {
            if (computeWidthFromModel)
            {
                int newModelWidth = modelWidth;
                for (int idx = first; idx <= last; idx++)
                {
                    newModelWidth = Math.Max(newModelWidth, computeEntryWidth(idx));
                }
                if (newModelWidth > modelWidth)
                {
                    modelWidth = newModelWidth;
                    invalidateLayout();
                }
            }
        }

        protected int computeEntryWidth(int idx)
        {
            int width = label.getBorderHorizontal();
            Font font = label.getFont();
            if (font != null)
            {
                width += font.ComputeMultiLineTextWidth(getModelData(idx));
            }
            return width;
        }

        void updateHover()
        {
            getAnimationState().setAnimationState(Label.STATE_HOVER,
                    label.hover || button.getModel().Hover);
        }

        class ComboboxLabel : Label
        {
            internal bool hover;
            private ComboBox<T> comboBox;

            public ComboboxLabel(ComboBox<T> comboBox, AnimationState animState) : base(animState)
            {
                this.comboBox = comboBox;

                setAutoSize(false);
                setClip(true);
                setTheme("display");
            }

            public override int getPreferredInnerWidth()
            {
                if (this.comboBox.computeWidthFromModel && this.comboBox.getModel() != null)
                {
                    if (this.comboBox.modelWidth == INVALID_WIDTH)
                    {
                        this.comboBox.updateModelWidth();
                    }
                    return this.comboBox.modelWidth;
                }
                else
                {
                    return base.getPreferredInnerWidth();
                }
            }

            public override int getPreferredInnerHeight()
            {
                int prefHeight = base.getPreferredInnerHeight();
                if (getFont() != null)
                {
                    prefHeight = Math.Max(prefHeight, getFont().LineHeight);
                }
                return prefHeight;
            }

            public override bool handleEvent(Event evt)
            {
                if (evt.isMouseEvent())
                {
                    bool newHover = evt.getEventType() != Event.EventType.MOUSE_EXITED;
                    if (newHover != hover)
                    {
                        hover = newHover;
                        this.comboBox.updateHover();
                    }

                    if (evt.getEventType() == Event.EventType.MOUSE_CLICKED)
                    {
                        this.comboBox.openPopup();
                    }

                    if (evt.getEventType() == Event.EventType.MOUSE_BTNDOWN &&
                            evt.getMouseButton() == Event.MOUSE_RBUTTON)
                    {
                        this.comboBox.handleRightClick();
                    }

                    return evt.getEventType() != Event.EventType.MOUSE_WHEEL;
                }
                return false;
            }
        }

        /*class ModelChangeListener : ListModel.ChangeListener
        {
            public void entriesInserted(int first, int last)
            {
                updateModelWidth(first, last);
            }
            public void entriesDeleted(int first, int last)
            {
                invalidateModelWidth();
            }
            public void entriesChanged(int first, int last)
            {
                invalidateModelWidth();
            }
            public void allChanged()
            {
                invalidateModelWidth();
            }
        }*/

        class ComboboxListbox<T> : ListBox<T>
        {
            public ComboboxListbox()
            {
                setTheme("listbox");
            }

            protected override ListBoxDisplay createDisplay()
            {
                return new ComboboxListboxLabel();
            }
        }

        class ComboboxListboxLabel : ListBox<T>.ListBoxLabel
        {
            protected override bool handleListBoxEvent(Event evt)
            {
                if (evt.getEventType() == Event.EventType.MOUSE_CLICKED)
                {
                    doListBoxCallback(ListBoxCallbackReason.MOUSE_CLICK);
                    return true;
                }
                if (evt.getEventType() == Event.EventType.MOUSE_BTNDOWN)
                {
                    doListBoxCallback(ListBoxCallbackReason.SET_SELECTED);
                    return true;
                }
                return false;
            }
        }
    }

    public class ComboBoxSelectionChangedEventArgs : EventArgs
    {
    }
}
