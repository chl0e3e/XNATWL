using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL.Model;

namespace XNATWL.Test
{
    public class WidgetsDemoDialog1 : FadeFrame
    {
        class WidgetsDemoAutoCompletionDataSource : AutoCompletionDataSource
        {
            private SimpleChangeableListModel<String> _lm;

            public WidgetsDemoAutoCompletionDataSource(SimpleChangeableListModel<String> lm)
            {
                this._lm = lm;
            }

            public AutoCompletionResult CollectSuggestions(String text, int cursorPos, AutoCompletionResult prev)
            {
                text = text.Substring(0, cursorPos);
                List<String> result = new List<String>();
                for (int i = 0; i < _lm.Entries; i++)
                {
                    if (_lm.EntryMatchesPrefix(i, text))
                    {
                        result.Add(_lm.EntryAt(i));
                    }
                }
                if (result.Count == 0)
                {
                    return null;
                }
                return new SimpleAutoCompletionResult(text, 0, result);
            }
        }
        private IntegerModel _mSpeed;
        private ProgressBar _progressBar;

        private int _progress;
        private int _timeout = 100;
        private bool _onoff = true;
        private Random _r = new Random();

        public WidgetsDemoDialog1()
        {
            Label l1 = new Label("new Entry");
            EditField e1 = new EditField();
            e1.SetText("edit me");
            e1.SetMaxTextLength(40);
            l1.SetLabelFor(e1);

            Label l2 = new Label("Me");
            EditField e2 = new EditField();
            e2.SetText("too!");
            e2.SetMaxTextLength(40);
            e2.SetPasswordMasking(true);
            l2.SetLabelFor(e2);

            SimpleChangeableListModel<String> lm = new SimpleChangeableListModel<String>(
                    "Entry 1", "Entry 2", "Entry 3", "Another one", "ok, one more");

            Button addBtn = new Button("Add to list");
            addBtn.Action += (sender, e) => {
                lm.AddElement(e1.GetText());
            };
            addBtn.SetTooltipContent("Adds the text from the edit field to the list box");

            e1.Callback += (sender, e) => {
                addBtn.SetEnabled(e1.GetTextLength() > 0);
            };

            e1.SetAutoCompletion(new WidgetsDemoAutoCompletionDataSource(lm));

            EditField e3 = new EditField();
            e3.SetText("This is a multi line Editfield\nTry it :)");
            e3.SetMultiLine(true);

            ScrollPane sp = new ScrollPane(e3);
            sp.SetFixed(ScrollPane.Fixed.HORIZONTAL);
            sp.SetExpandContentSize(true);

            SimpleChangeableListModel<SimpleTest.StyleItem> lmStyle = new SimpleChangeableListModel<SimpleTest.StyleItem>(
                    new SimpleTest.StyleItem("progressbar", "Simple"),
                    new SimpleTest.StyleItem("progressbar-glow", "Glow"),
                    new SimpleTest.StyleItem("progressbar-glow-anim", "Animated"));

            _progressBar = new ProgressBar();

            ListBox<String> lb = new ListBox<String>(lm);

            ToggleButton tb = new ToggleButton("");
            tb.SetTheme("checkbox");
            tb.SetActive(true);
            tb.SetTooltipContent("Toggles the Frame title on/off");
            tb.Action += (sender, e) => {
                if (tb.IsActive())
                {
                    SetTheme(SimpleTest.WITH_TITLE);
                }
                else
                {
                    SetTheme(SimpleTest.WITHOUT_TITLE);
                }
                ReapplyTheme();
            };

            Label tbLabel = new Label("show title");
            tbLabel.SetLabelFor(tb);

            ComboBox<SimpleTest.StyleItem> cb = new ComboBox<SimpleTest.StyleItem>(lmStyle);
            cb.SelectionChanged += (sender, e) => {
                int idx = cb.GetSelected();
                _progressBar.SetTheme(lmStyle.EntryAt(idx).Theme);
                _progressBar.ReapplyTheme();
            };
            cb.SetSelected(2);
            cb.SetComputeWidthFromModel(true);

            _mSpeed = new SimpleIntegerModel(0, 100, 10);
            ValueAdjusterInt vai = new ValueAdjusterInt(_mSpeed);
            Label l4 = new Label("Progressbar speed");
            l4.SetLabelFor(vai);

            ToggleButton[] optionBtns = new ToggleButton[4];
            SimpleIntegerModel optionModel = new SimpleIntegerModel(1, optionBtns.Length, 1);
            for (int i = 0; i < optionBtns.Length; i++)
            {
                optionBtns[i] = new ToggleButton(new OptionBooleanModel(optionModel, i + 1));
                optionBtns[i].SetText((i + 1).ToString());
                optionBtns[i].SetTheme("radiobutton");
            }

            DialogLayout box = new DialogLayout();
            box.SetTheme("/optionsdialog"); // the '/' causes this theme to start at the root again
            box.SetHorizontalGroup(box.CreateParallelGroup().AddGroup(
                    box.CreateSequentialGroup(
                        box.CreateParallelGroup(l1, l2, l4),
                        box.CreateParallelGroup().AddGroup(box.CreateSequentialGroup(e1, addBtn)).AddWidgets(e2, vai))).
                    AddWidget(_progressBar).AddWidget(lb).
                    AddWidget(sp).
                    AddGroup(box.CreateSequentialGroup(cb).AddGap()).
                    AddGroup(box.CreateSequentialGroup(optionBtns).AddGap()).
                    AddGroup(box.CreateSequentialGroup().AddGap().AddWidgets(tbLabel, tb)));
            box.SetVerticalGroup(box.CreateSequentialGroup().
                    AddGroup(box.CreateParallelGroup(l1, e1, addBtn)).
                    AddGroup(box.CreateParallelGroup(l2, e2)).
                    AddGroup(box.CreateParallelGroup(l4, vai)).
                    AddWidgets(_progressBar, lb, sp, cb).
                    AddGroup(box.CreateParallelGroup(optionBtns)).
                    AddGroup(box.CreateParallelGroup(tbLabel, tb)));

            SetTheme(SimpleTest.WITH_TITLE);
            Add(box);
            SetTitle("TWL Example");
        }

        protected override void Paint(GUI gui)
        {
            base.Paint(gui);

            if (_onoff)
            {
                _progressBar.SetValue(_progress / 5000f);
                _progress = (_progress + _mSpeed.Value) % 5000;
            }
            if (--_timeout == 0)
            {
                _onoff ^= true;
                _timeout = 100 + _r.Next(200);
            }
        }
    }
}
