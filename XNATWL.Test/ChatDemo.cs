using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XNATWL;
using XNATWL.TextAreaModel;

namespace XNATWL.Test
{
    class ChatDemo : DesktopArea
    {
        private ChatFrame _chatFrame;

        public ChatDemo()
        {
            _chatFrame = new ChatFrame();
            Add(_chatFrame);

            _chatFrame.SetSize(400, 200);
            //chatFrame.setPosition(10, 350);
        }

        protected override void Layout()
        {
            base.Layout();
        }

        class ChatFrame : ResizableFrame
        {
            private HTMLTextAreaModel _textAreaModel;
            private TextArea _textArea;
            private ScrollPane _scrollPane;
            private EditField _editField;

            public ChatFrame()
            {
                SetTitle("Chat");

                this._textAreaModel = new HTMLTextAreaModel();
                this._textArea = new TextArea(this._textAreaModel);
                this._editField = new EditField();
                this._editField.SetText("Test");

                this._scrollPane = new ScrollPane(this._textArea);
                this._scrollPane.SetFixed(ScrollPane.Fixed.HORIZONTAL);

                DialogLayout l = new DialogLayout();
                l.SetClip(true);
                l.SetTheme("content");
                l.SetHorizontalGroup(l.CreateParallelGroup(_scrollPane, _editField));
                l.SetVerticalGroup(l.CreateSequentialGroup(_scrollPane, _editField));
                Add(l);

                _textAreaModel.SetHtml("<html><body><div style=\"word-wrap: break-word; font-family: default;\">Test</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div></body></html>");

                _scrollPane.ValidateLayout();
                _scrollPane.SetScrollPositionY(_scrollPane.GetMaxScrollPosY());
            }
        }
    }
}
