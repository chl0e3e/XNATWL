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
        private ChatFrame chatFrame;

        public ChatDemo()
        {
            chatFrame = new ChatFrame();
            add(chatFrame);

            chatFrame.setSize(400, 200);
            //chatFrame.setPosition(10, 350);
        }

        protected override void layout()
        {
            base.layout();
        }

        class ChatFrame : ResizableFrame
        {
            private HTMLTextAreaModel textAreaModel;
            private TextArea textArea;
            private ScrollPane scrollPane;
            private EditField editField;

            public ChatFrame()
            {
                setTitle("Chat");

                this.textAreaModel = new HTMLTextAreaModel();
                this.textArea = new TextArea(this.textAreaModel);
                this.editField = new EditField();
                this.editField.setText("Test");

                this.scrollPane = new ScrollPane(this.textArea);
                this.scrollPane.setFixed(ScrollPane.Fixed.HORIZONTAL);

                DialogLayout l = new DialogLayout();
                l.setClip(true);
                l.setTheme("content");
                l.setHorizontalGroup(l.createParallelGroup(scrollPane, editField));
                l.setVerticalGroup(l.createSequentialGroup(scrollPane, editField));
                add(l);

                textAreaModel.setHtml("<html><body><div style=\"word-wrap: break-word; font-family: default;\">Test</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div><div style=\"word-wrap: break-word; font-family: default; \">The quick brown fox jumped over the lazy white dog.</div></body></html>");

                scrollPane.validateLayout();
                scrollPane.setScrollPositionY(scrollPane.getMaxScrollPosY());
            }
        }
    }
}
