﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNATWL.TextArea
{
    /**
     * This class is a scanner generated by 
     * <a href="http://www.jflex.de/">JFlex</a> 1.4.3
     * on 03.01.12 11:50 from the specification file
     * <tt>parser.flex</tt>
     */
    class Parser
    {

        /** This character denotes the end of file */
        public static int YYEOF = -1;

        /** initial size of the lookahead buffer */
        private static int ZZ_BUFFERSIZE = 16384;

        /** lexical states */
        public static int YYSTRING1 = 6;
        public static int YYINITIAL = 0;
        public static int YYSTYLE = 2;
        public static int YYVALUE = 4;
        public static int YYSTRING2 = 8;

        /**
         * ZZ_LEXSTATE[l] is the state in the DFA for the lexical state l
         * ZZ_LEXSTATE[l+1] is the state in the DFA for the lexical state l
         *                  at the beginning of a line
         * l is of the form l = 2*k, k a non negative integer
         */
        //  private static int ZZ_LEXSTATE[] = {
        //     0,  0,  1,  1,  2,  2,  3,  3,  4, 4
        //  };

        /** 
         * Translates characters to character classes
         */
        private static String ZZ_CMAP_PACKED =
          "\x09\x00\x01\x03\x01\x02\x01\x00\x01\x03\x01\x01\x12\x00\x01\x03\x01\x00\x01\x13\x01\x0c\x03\x00\x01\x12\x02\x00\x01\x05\x01\x00\x01\x0a\x01\x06\x01\x09\x01\x04\x0a\x08\x01\x0d\x01\x11\x02\x00\x01\x0b\x01\x00\x01\x0e\x1a\x07\x04\x00\x01\x07\x01\x00\x1a\x07\x01\x0f\x01\x00\x01\x10\xef\xbe\x82\x00";

        /** 
         * Translates characters to character classes
         */
        private static char[] ZZ_CMAP = zzUnpackCMap(ZZ_CMAP_PACKED);

        /** 
         * Translates DFA states to action switch labels.
         */
        private static int[] ZZ_ACTION = zzUnpackAction();

        private static String ZZ_ACTION_PACKED_0 =
          "\x09\x00\x01\x03\x01\x02\x01\x00\x01\x03\x01\x01\x12\x00\x01\x03\x01\x00\x01\x13\x01\x0c\x03\x00\x01\x12\x02\x00\x01\x05\x01\x00\x01\x0a\x01\x06\x01\x09\x01\x04\x0a\x08\x01\x0d\x01\x11\x02\x00\x01\x0b\x01\x00\x01\x0e\x1a\x07\x04\x00\x01\x07\x01\x00\x1a\x07\x01\x0f\x01\x00\x01\x10\xef\xbe\x82\x00";

        private static int[] zzUnpackAction()
        {
            int[] result = new int[36];
            zzUnpackAction(ZZ_ACTION_PACKED_0, 0, result);
            return result;
        }

        private static int zzUnpackAction(String packed, int offset, int[] result)
        {
            int i = 0;       /* index in packed string  */
            int j = offset;  /* index in unpacked array */
            int l = packed.Length;
            while (i < l)
            {
                int count = packed[i++];
                int value = packed[i++];
                do result[j++] = value; while (--count > 0);
            }
            return j;
        }


        /** 
         * Translates a state to a row index in the transition table
         */
        private static int[] ZZ_ROWMAP = zzUnpackRowMap();

        private static String ZZ_ROWMAP_PACKED_0 =
          "\x00\x00\x00\x14\x00\x28\x00\x3c\x00\x50\x00\x64\x00\x78\x00\xc2\x8c\x00\x64\x00\xc2\xa0\x00\xc2\xb4\x00\x64\x00\x64\x00\x64\x00\x64\x00\x64\x00\x64\x00\x64\x00\xc3\x88\x00\x64\x00\xc3\x9c\x00\xc3\xb0\x00\x64\x00\x64\x00\xc4\x84\x00\x64\x00\x64\x00\x64\x00\xc4\x98\x00\x64\x00\xc4\xac\x00\x64\x00\xc5\x80\x00\xc5\x94\x00\xc5\xa8\x00\xc5\xbc";

        private static int[] zzUnpackRowMap()
        {
            int[] result = new int[36];
            zzUnpackRowMap(ZZ_ROWMAP_PACKED_0, 0, result);
            return result;
        }

        private static int zzUnpackRowMap(String packed, int offset, int[] result)
        {
            int i = 0;  /* index in packed string  */
            int j = offset;  /* index in unpacked array */
            int l = packed.Length;
            while (i < l)
            {
                int high = packed[i++] << 16;
                result[j++] = high | packed[i++];
            }
            return j;
        }

        /** 
         * The transition table of the DFA
         */
        private static int[] ZZ_TRANS = zzUnpackTrans();

        private static String ZZ_TRANS_PACKED_0 =
          "\x01\x06\x03\x07\x01\x08\x01\x09\x01\x0a\x01\x0b\x01\x06\x01\x0c\x01\x0d\x01\x0e\x01\x0f\x01\x10\x01\x11\x01\x12\x05\x06\x01\x13\x02\x14\x01\x08\x01\x06\x01\x15\x01\x16\x05\x06\x01\x17\x02\x06\x01\x18\x03\x06\x10\x19\x01\x18\x01\x1a\x01\x1b\x01\x1c\x12\x1d\x01\x1e\x01\x1d\x13\x1f\x01\x20\x15\x00\x03\x07\x15\x00\x01\x21\x15\x00\x01\x0b\x12\x00\x03\x0b\x0d\x00\x01\x14\x18\x00\x01\x16\x12\x00\x03\x16\x0b\x00\x10\x19\x04\x00\x12\x1d\x01\x00\x01\x1d\x13\x1f\x01\x00\x05\x22\x01\x23\x13\x22\x01\x24\x0e\x22\x04\x00\x01\x14\x01\x23\x0e\x00\x04\x22\x01\x14\x01\x24\x0e\x22";

        private static int[] zzUnpackTrans()
        {
            int[] result = new int[400];
            zzUnpackTrans(ZZ_TRANS_PACKED_0, 0, result);
            return result;
        }

        private static int zzUnpackTrans(String packed, int offset, int[] result)
        {
            int i = 0;       /* index in packed string  */
            int j = offset;  /* index in unpacked array */
            int l = packed.Length;
            while (i < l)
            {
                int count = packed[i++];
                int value = packed[i++];
                value--;
                do result[j++] = value; while (--count > 0);
            }
            return j;
        }

        /**
         * ZZ_ATTRIBUTE[aState] contains the attributes of state <code>aState</code>
         */
        private static int[] ZZ_ATTRIBUTE = zzUnpackAttribute();

        private static String ZZ_ATTRIBUTE_PACKED_0 =
          "\x05\x00\x01\x09\x02\x01\x01\x09\x02\x01\x07\x09\x01\x01\x01\x09\x02\x01\x02\x09\x01\x01\x03\x09\x01\x01\x01\x09\x01\x01\x01\x09\x04\x00";

        private static int[] zzUnpackAttribute()
        {
            int[] result = new int[36];
            zzUnpackAttribute(ZZ_ATTRIBUTE_PACKED_0, 0, result);
            return result;
        }

        private static int zzUnpackAttribute(String packed, int offset, int[] result)
        {
            int i = 0;       /* index in packed string  */
            int j = offset;  /* index in unpacked array */
            int l = packed.Length;
            while (i < l)
            {
                int count = packed[i++];
                int value = packed[i++];
                do result[j++] = value; while (--count > 0);
            }
            return j;
        }

        /** the input device */
        private StreamReader zzReader;

        /** the current state of the DFA */
        private int zzState;

        /** the current lexical state */
        private int zzLexicalState = YYINITIAL;

        /** this buffer contains the current text to be matched and is
            the source of the yytext() string */
        private char[] zzBuffer = new char[ZZ_BUFFERSIZE];

        /** the textposition at the last accepting state */
        private int zzMarkedPos;

        /** the current text position in the buffer */
        private int zzCurrentPos;

        /** startRead marks the beginning of the yytext() string in the buffer */
        private int zzStartRead;

        /** endRead marks the last character in the buffer, that has been read
            from input */
        private int zzEndRead;

        /** number of newlines encountered up to the start of the matched text */
        private int yyline;

        /**
         * the number of characters from the last newline up to the start of the 
         * matched text
         */
        private int yycolumn;

        /** zzAtEOF == true <=> the scanner is at the EOF */
        private bool zzAtEOF;

        /* user code: */
        internal static int EOF = 0;
        internal static int IDENT = 1;
        internal static int STAR = 2;
        internal static int DOT = 3;
        internal static int HASH = 4;
        internal static int GT = 5;
        internal static int COMMA = 6;
        internal static int STYLE_BEGIN = 7;
        internal static int STYLE_END = 8;
        internal static int COLON = 9;
        internal static int SEMICOLON = 10;
        internal static int ATRULE = 11;

        internal bool sawWhitespace;

        internal StringBuilder sb = new StringBuilder();

        private void append()
        {
            sb.Append(zzBuffer, zzStartRead, zzMarkedPos - zzStartRead);
        }

        public void unexpected()
        {
            throw new IOException("Unexpected \"" + yytext() + "\" at line " + yyline + ", column " + yycolumn);
        }

        public void expect(int token)
        {
            if (yylex() != token) unexpected();
        }


        /**
         * Creates a new scanner
         * There is also a java.io.InputStream version of this constructor.
         *
         * @param   in  the java.io.Reader to read input from.
         */
        internal Parser(StreamReader srin)
        {
            this.zzReader = srin;
        }

        /**
         * Unpacks the compressed character translation table.
         *
         * @param packed   the packed character translation table
         * @return         the unpacked character translation table
         */
        private static char[] zzUnpackCMap(String packed)
        {
            char[] map = new char[0x10000];
            int i = 0;  /* index in packed string  */
            int j = 0;  /* index in unpacked array */
            while (i < 72)
            {
                int count = packed[i++];
                char value = packed[i++];
                do map[j++] = value; while (--count > 0);
            }
            return map;
        }


        /**
         * Refills the input buffer.
         *
         * @return      <code>false</code>, iff there was new input.
         * 
         * @exception   java.io.IOException  if any I/O-Error occurs
         */
        private bool zzRefill()
        {

            /* first: make room (if you can) */
            if (zzStartRead > 0)
            {
                Array.Copy(zzBuffer, zzStartRead,
                                 zzBuffer, 0,
                                 zzEndRead - zzStartRead);

                /* translate stored positions */
                zzEndRead -= zzStartRead;
                zzCurrentPos -= zzStartRead;
                zzMarkedPos -= zzStartRead;
                zzStartRead = 0;
            }

            /* is the buffer big enough? */
            if (zzCurrentPos >= zzBuffer.Length)
            {
                /* if not: blow it up */
                char[] newBuffer = new char[zzCurrentPos * 2];
                Array.Copy(zzBuffer, 0, newBuffer, 0, zzBuffer.Length);
                zzBuffer = newBuffer;
            }

            /* finally: fill the buffer with new input */
            int numRead = zzReader.Read(zzBuffer, zzEndRead, zzBuffer.Length - zzEndRead);

            if (numRead > 0)
            {
                zzEndRead += numRead;
                return false;
            }
            // unlikely but not impossible: read 0 characters, but not at end of stream    
            if (numRead == 0)
            {
                int c = zzReader.Read();
                if (c == -1)
                {
                    return true;
                }
                else
                {
                    zzBuffer[zzEndRead++] = (char)c;
                    return false;
                }
            }

            // numRead < 0
            return true;
        }


        /**
         * Enters a new lexical state
         *
         * @param newState the new lexical state
         */
        public void yybegin(int newState)
        {
            zzLexicalState = newState;
        }


        /**
         * Returns the text matched by the current regular expression.
         */
        public String yytext()
        {
            return new String(zzBuffer, zzStartRead, zzMarkedPos - zzStartRead);
        }


        /**
         * Reports an error that occured while scanning.
         *
         * In a wellformed scanner (no or only correct usage of 
         * yypushback(int) and a match-all fallback rule) this method 
         * will only be called with things that "Can't Possibly Happen".
         * If this method is called, something is seriously wrong
         * (e.g. a JFlex bug producing a faulty scanner etc.).
         *
         * Usual syntax/scanner level error handling should be done
         * in error fallback rules.
         *
         * @param   message  the errormessage to display
         */
        private void zzScanError(String message)
        {
            throw new Exception(message);
        }


        /**
         * Resumes scanning until the next regular expression is matched,
         * the end of input is encountered or an I/O-Error occurs.
         *
         * @return      the next token
         * @exception   java.io.IOException  if any I/O-Error occurs
         */
        public int yylex()
        {
            int zzInput;
            int zzAction;

            // cached fields:
            int zzCurrentPosL;
            int zzMarkedPosL;
            int zzEndReadL = zzEndRead;
            char[] zzBufferL = zzBuffer;
            char[] zzCMapL = ZZ_CMAP;

            int[] zzTransL = ZZ_TRANS;
            int[] zzRowMapL = ZZ_ROWMAP;
            int[] zzAttrL = ZZ_ATTRIBUTE;

            while (true)
            {
                zzMarkedPosL = zzMarkedPos;

                bool zzR = false;
                for (zzCurrentPosL = zzStartRead; zzCurrentPosL < zzMarkedPosL;
                                                                       zzCurrentPosL++)
                {
                    switch (zzBufferL[zzCurrentPosL])
                    {
                        case '\u000B':
                        case '\u000C':
                        case '\u0085':
                        case '\u2028':
                        case '\u2029':
                            yyline++;
                            yycolumn = 0;
                            zzR = false;
                            break;
                        case '\r':
                            yyline++;
                            yycolumn = 0;
                            zzR = true;
                            break;
                        case '\n':
                            if (zzR)
                                zzR = false;
                            else
                            {
                                yyline++;
                                yycolumn = 0;
                            }
                            break;
                        default:
                            zzR = false;
                            yycolumn++;
                            break;
                    }
                }

                if (zzR)
                {
                    // peek one character ahead if it is \n (if we have counted one line too much)
                    bool zzPeek;
                    if (zzMarkedPosL < zzEndReadL)
                        zzPeek = zzBufferL[zzMarkedPosL] == '\n';
                    else if (zzAtEOF)
                        zzPeek = false;
                    else
                    {
                        bool eof = zzRefill();
                        zzEndReadL = zzEndRead;
                        zzMarkedPosL = zzMarkedPos;
                        zzBufferL = zzBuffer;
                        if (eof)
                            zzPeek = false;
                        else
                            zzPeek = zzBufferL[zzMarkedPosL] == '\n';
                    }
                    if (zzPeek) yyline--;
                }
                zzAction = -1;

                zzCurrentPosL = zzCurrentPos = zzStartRead = zzMarkedPosL;

                //      zzState = ZZ_LEXSTATE[zzLexicalState];
                zzState = zzLexicalState / 2;


            zzForAction:
                {
                    while (true)
                    {

                        if (zzCurrentPosL < zzEndReadL)
                            zzInput = zzBufferL[zzCurrentPosL++];
                        else if (zzAtEOF)
                        {
                            zzInput = YYEOF;
                            break;
                        }
                        else
                        {
                            // store back cached positions
                            zzCurrentPos = zzCurrentPosL;
                            zzMarkedPos = zzMarkedPosL;
                            bool eof = zzRefill();
                            // get translated positions and possibly new buffer
                            zzCurrentPosL = zzCurrentPos;
                            zzMarkedPosL = zzMarkedPos;
                            zzBufferL = zzBuffer;
                            zzEndReadL = zzEndRead;
                            if (eof)
                            {
                                zzInput = YYEOF;
                                break;
                            }
                            else
                            {
                                zzInput = zzBufferL[zzCurrentPosL++];
                            }
                        }
                        int zzNext = zzTransL[zzRowMapL[zzState] + zzCMapL[zzInput]];
                        if (zzNext == -1) goto zzForAction;
                        zzState = zzNext;

                        int zzAttributes = zzAttrL[zzState];
                        if ((zzAttributes & 1) == 1)
                        {
                            zzAction = zzState;
                            zzMarkedPosL = zzCurrentPosL;
                            if ((zzAttributes & 8) == 8) break;
                        }

                    }
                }

                // store back cached position
                zzMarkedPos = zzMarkedPosL;

                switch (zzAction < 0 ? zzAction : ZZ_ACTION[zzAction])
                {
                    case 6:
                        {
                            return COMMA;
                        }
                    case 22: break;
                    case 20:
                        {
                            yybegin(YYVALUE); sb.Append('\'');
                        }
                        break;
                    case 23: break;
                    case 10:
                        {
                            return ATRULE;
                        }
                    case 24: break;
                    case 3:
                        {
                            sawWhitespace = false; return STAR;
                        }
                    case 25: break;
                    case 18:
                        {
                            yybegin(YYSTRING1); sb.Append('\'');
                        }
                        break;
                    case 26: break;
                    case 19:
                        {
                            yybegin(YYSTRING2); sb.Append('\"');
                        }
                        break;
                    case 27: break;
                    case 16:
                        {
                            append();
                        }
                        break;
                    case 28: break;
                    case 4:
                        {
                            sawWhitespace = false; return IDENT;
                        }
                    case 29: break;
                    case 21:
                        {
                            yybegin(YYVALUE); sb.Append('\"');
                        }
                        break;
                    case 30: break;
                    case 9:
                        {
                            return COLON;
                        }
                    case 31: break;
                    case 2:
                        {
                            sawWhitespace = true;
                        }
                        break;
                    case 32: break;
                    case 15:
                        {
                            yybegin(YYINITIAL); return STYLE_END;
                        }
                    case 33: break;
                    case 17:
                        {
                            yybegin(YYSTYLE); return SEMICOLON;
                        }
                    case 34: break;
                    case 14:
                        {
                            yybegin(YYVALUE); sb.Length = 0; return COLON;
                        }
                        break;
                    case 35: break;
                    case 7:
                        {
                            return GT;
                        }
                    case 36: break;
                    case 11:
                        {
                            yybegin(YYSTYLE); return STYLE_BEGIN;
                        }
                    case 37: break;
                    case 13:
                        {
                            return IDENT;
                        }
                    case 38: break;
                    case 1:
                        {
                            unexpected();
                        }
                        break;
                    case 39: break;
                    case 5:
                        {
                            return DOT;
                        }
                    case 40: break;
                    case 8:
                        {
                            return HASH;
                        }
                    case 41: break;
                    case 12:
                        { /* ignore */
                        }
                        break;
                    case 42: break;
                    default:
                        if (zzInput == YYEOF && zzStartRead == zzCurrentPos)
                        {
                            zzAtEOF = true;
                            {
                                return EOF;
                            }
                        }
                        else
                        {
                            zzScanError("Error: could not match input");
                        }
                        break;
                }
            }
        }
    }
}
