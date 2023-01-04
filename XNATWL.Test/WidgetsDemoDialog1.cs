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
            private SimpleChangeableListModel<String> lm;
            public WidgetsDemoAutoCompletionDataSource(SimpleChangeableListModel<String> lm)
            {
                this.lm = lm;
            }
            public AutoCompletionResult CollectSuggestions(String text, int cursorPos, AutoCompletionResult prev)
            {
                text = text.Substring(0, cursorPos);
                List<String> result = new List<String>();
                for (int i = 0; i < lm.Entries; i++)
                {
                    if (lm.EntryMatchesPrefix(i, text))
                    {
                        result.Add(lm.EntryAt(i));
                    }
                }
                if (result.Count == 0)
                {
                    return null;
                }
                return new SimpleAutoCompletionResult(text, 0, result);
            }
        }
        private IntegerModel mSpeed;
        private ProgressBar progressBar;

        private int progress;
        private int timeout = 100;
        private bool onoff = true;
        private Random r = new Random();

        public WidgetsDemoDialog1()
        {
            Label l1 = new Label("new Entry");
            EditField e1 = new EditField();
            e1.setText("edit me");
            e1.setMaxTextLength(40);
            l1.setLabelFor(e1);

            Label l2 = new Label("Me");
            EditField e2 = new EditField();
            e2.setText("too!");
            e2.setMaxTextLength(40);
            e2.setPasswordMasking(true);
            l2.setLabelFor(e2);

            SimpleChangeableListModel<String> lm = new SimpleChangeableListModel<String>(
                    "Entry 1", "Entry 2", "Entry 3", "Another one", "ok, one more");

            Button addBtn = new Button("Add to list");
            addBtn.Action += (sender, e) => {
                lm.AddElement(e1.getText());
            };
            addBtn.setTooltipContent("Adds the text from the edit field to the list box");

            e1.Callback += (sender, e) => {
                addBtn.setEnabled(e1.getTextLength() > 0);
            };

            e1.setAutoCompletion(new WidgetsDemoAutoCompletionDataSource(lm));

            EditField e3 = new EditField();
            e3.setText("This is a multi line Editfield\nTry it :)");
            e3.setMultiLine(true);

            ScrollPane sp = new ScrollPane(e3);
            sp.setFixed(ScrollPane.Fixed.HORIZONTAL);
            sp.setExpandContentSize(true);

            SimpleChangeableListModel<StyleItem> lmStyle = new SimpleChangeableListModel<StyleItem>(
                    new StyleItem("progressbar", "Simple"),
                    new StyleItem("progressbar-glow", "Glow"),
                    new StyleItem("progressbar-glow-anim", "Animated"));

            progressBar = new ProgressBar();

            ListBox<String> lb = new ListBox<String>(lm);

            ToggleButton tb = new ToggleButton("");
            tb.setTheme("checkbox");
            tb.setActive(true);
            tb.setTooltipContent("Toggles the Frame title on/off");
            tb.Action += (sender, e) => {
                if (tb.isActive())
                {
                    setTheme(SimpleTest.WITH_TITLE);
                }
                else
                {
                    setTheme(SimpleTest.WITHOUT_TITLE);
                }
                reapplyTheme();
            };

            Label tbLabel = new Label("show title");
            tbLabel.setLabelFor(tb);

            ComboBox<StyleItem> cb = new ComboBox<StyleItem>(lmStyle);
            cb.SelectionChanged += (sender, e) => {
                int idx = cb.getSelected();
                progressBar.setTheme(lmStyle.EntryAt(idx).theme);
                progressBar.reapplyTheme();
            };
            cb.setSelected(2);
            cb.setComputeWidthFromModel(true);

            mSpeed = new SimpleIntegerModel(0, 100, 10);
            ValueAdjusterInt vai = new ValueAdjusterInt(mSpeed);
            Label l4 = new Label("Progressbar speed");
            l4.setLabelFor(vai);

            ToggleButton[] optionBtns = new ToggleButton[4];
            SimpleIntegerModel optionModel = new SimpleIntegerModel(1, optionBtns.Length, 1);
            for (int i = 0; i < optionBtns.Length; i++)
            {
                optionBtns[i] = new ToggleButton(new OptionBooleanModel(optionModel, i + 1));
                optionBtns[i].setText((i + 1).ToString());
                optionBtns[i].setTheme("radiobutton");
            }

            DialogLayout box = new DialogLayout();
            box.setTheme("/optionsdialog"); // the '/' causes this theme to start at the root again
            box.setHorizontalGroup(box.createParallelGroup().addGroup(
                    box.createSequentialGroup(
                        box.createParallelGroup(l1, l2, l4),
                        box.createParallelGroup().addGroup(box.createSequentialGroup(e1, addBtn)).addWidgets(e2, vai))).
                    addWidget(progressBar).addWidget(lb).
                    addWidget(sp).
                    addGroup(box.createSequentialGroup(cb).addGap()).
                    addGroup(box.createSequentialGroup(optionBtns).addGap()).
                    addGroup(box.createSequentialGroup().addGap().addWidgets(tbLabel, tb)));
            box.setVerticalGroup(box.createSequentialGroup().
                    addGroup(box.createParallelGroup(l1, e1, addBtn)).
                    addGroup(box.createParallelGroup(l2, e2)).
                    addGroup(box.createParallelGroup(l4, vai)).
                    addWidgets(progressBar, lb, sp, cb).
                    addGroup(box.createParallelGroup(optionBtns)).
                    addGroup(box.createParallelGroup(tbLabel, tb)));

            setTheme(SimpleTest.WITH_TITLE);
            add(box);
            setTitle("TWL Example");
        }

        protected override void paint(GUI gui)
        {
            base.paint(gui);

            if (onoff)
            {
                progressBar.setValue(progress / 5000f);
                progress = (progress + mSpeed.Value) % 5000;
            }
            if (--timeout == 0)
            {
                onoff ^= true;
                timeout = 100 + r.Next(200);
            }
        }

    }

}
