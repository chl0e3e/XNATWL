﻿/*
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
using System.Collections.Generic;
using System.IO;
using XNATWL.IO;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL.Theme
{
    /// <summary>
    /// Used for managing <see cref="Image"/> objects by a reference
    /// </summary>
    public class ImageManager
    {
        ParameterMapImpl _constants;
        private Renderer.Renderer _renderer;
        private SortedDictionary<string, Image> _images;
        private SortedDictionary<string, MouseCursor> _cursors;
        private MathInterpreter _mathInterpreter;

        private Texture _currentTexture;

        protected internal static EmptyImage NONE = new EmptyImage(0, 0);
        private static MouseCursor INHERIT_CURSOR = InheritedMouseCursor.INHERITED_DEFAULT;// new MouseCursor() { };

        /// <summary>
        /// Create a new <see cref="ImageManager"/> given constants and the target renderer
        /// </summary>
        /// <param name="constants">Parameter map for constants</param>
        /// <param name="renderer">Target renderer</param>
        public ImageManager(ParameterMapImpl constants, Renderer.Renderer renderer)
        {
            this._constants = constants;
            this._renderer = renderer;
            this._images = new SortedDictionary<string, Image>();
            this._cursors = new SortedDictionary<string, MouseCursor>();
            this._mathInterpreter = new MathInterpreter(this);

            _images.Add("none", NONE);
            _cursors.Add("os-default", DefaultMouseCursor.OS_DEFAULT);
            _cursors.Add("inherit", INHERIT_CURSOR);
        }

        /// <summary>
        /// Retrieve an <see cref="Image"/> given a name
        /// </summary>
        /// <param name="name"><see cref="Image"/> name</param>
        /// <returns>the <see cref="Image"/></returns>
        public Image this[string name]
        {
            get
            {
                if (!_images.ContainsKey(name))
                {
                    return null;
                }
                return _images[name];
            }
        }

        /// <summary>
        /// Get a referenced image from the attribute in the current state of the <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">Theme XML parser</param>
        /// <returns><see cref="Image"/> referenced</returns>
        public Image GetReferencedImage(XMLParser xmlp)
        {
            string reference = xmlp.GetAttributeNotNull("ref");
            return GetReferencedImage(xmlp, reference);
        }

        /// <summary>
        /// Get a referenced <see cref="Image"/> from a reference, failing using the <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">Theme XML parser</param>
        /// <param name="reference">Image reference</param>
        /// <returns>the <see cref="Image"/></returns>
        public Image GetReferencedImage(XMLParser xmlp, string reference)
        {
            if (reference.EndsWith(".*"))
            {
                throw xmlp.Error("wildcard mapping not allowed");
            }
            Image img = _images[reference];
            if (img == null)
            {
                throw xmlp.Error("referenced image \"" + reference + "\" not found");
            }
            return img;
        }

        /// <summary>
        /// Get a referenced <see cref="MouseCursor"/> from a given reference, failing using the <see cref="XMLParser"/>. Defaults to null if the cursor is inherited
        /// </summary>
        /// <param name="xmlp">Theme XML parser</param>
        /// <param name="reference">Cursor reference</param>
        /// <returns><see cref="MouseCursor"/ matching reference></returns>
        public MouseCursor GetReferencedCursor(XMLParser xmlp, string reference)
        {
            MouseCursor cursor = _cursors[reference];
            if (cursor == null)
            {
                throw xmlp.Error("referenced cursor \"" + reference + "\" not found");
            }
            return UnwrapCursor(cursor);
        }

        /// <summary>
        /// Get <see cref="Image"/>s with matching key 'name + reference'
        /// </summary>
        /// <param name="reference">Reference</param>
        /// <param name="name">Identifier</param>
        /// <returns>Matching <see cref="Image"/> dictionary</returns>
        public Dictionary<string, Image> GetImages(string reference, string name)
        {
            return ParserUtil.Resolve(_images, reference, name, null);
        }

        /// <summary>
        /// Get cursor by name
        /// </summary>
        /// <param name="name">Name of cursor</param>
        /// <returns><see cref="MouseCursor"/></returns>
        public MouseCursor GetCursor(string name)
        {
            return UnwrapCursor(_cursors[name]);
        }

        /// <summary>
        /// Get <see cref="MouseCursor"/>s with matching key 'name + reference'
        /// </summary>
        /// <param name="reference">Reference</param>
        /// <param name="name">Identifier</param>
        /// <returns>Matching <see cref="MouseCursor"/> dictionary</returns>
        public Dictionary<string, MouseCursor> GetCursors(string reference, string name)
        {
            return ParserUtil.Resolve(_cursors, reference, name, INHERIT_CURSOR);
        }

        /// <summary>
        /// Parse images from an <see cref="XMLParser"/> pointing at an image tag, finding assets in <paramref name="baseFolder"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="baseFolder">base folder containing images</param>
        /// <exception cref="NullReferenceException">Failed to find image referenced</exception>
        public void ParseImages(XMLParser xmlp, FileSystemObject baseFolder)
        {
            xmlp.Require(XmlPullParser.START_TAG, null, null);

            Texture texture = null;
            String fileName = xmlp.GetAttributeValue(null, "file");
            if (fileName != null)
            {
                String fmt = xmlp.GetAttributeValue(null, "format");
                String filter = xmlp.GetAttributeValue(null, "filter");
                // ignore the comment so that it does not cause a warning
                xmlp.GetAttributeValue(null, "comment");

                try
                {
                    texture = _renderer.LoadTexture(new FileSystemObject(baseFolder, fileName), fmt, filter);
                    if (texture == null)
                    {
                        throw new NullReferenceException("loadTexture returned null");
                    }
                }
                catch (IOException ex)
                {
                    throw xmlp.Error("Unable to load image file: " + fileName, ex);
                }
            }

            this._currentTexture = texture;

            try
            {
                xmlp.NextTag();
                while (!xmlp.IsEndTag())
                {
                    bool emptyElement = xmlp.IsEmptyElement();
                    String tagName = xmlp.GetName();
                    String name = xmlp.GetAttributeNotNull("name");
                    CheckImageName(name, xmlp);

                    if ("cursor".Equals(xmlp.GetName()))
                    {
                        ParseCursor(xmlp, name);
                    }
                    else
                    {
                        Image image = ParseImage(xmlp, tagName);
                        _images.Add(name, image);
                    }

                    if (!emptyElement)
                    {
                        xmlp.Require(XmlPullParser.END_TAG, null, tagName);
                    }

                    xmlp.NextTag();
                }
            }
            finally
            {
                _currentTexture = null;
                if (texture != null)
                {
                    texture.ThemeLoadingDone();
                }
            }
        }

        /// <summary>
        /// Inherited cursors return null, otherwise, returns the <see cref="MouseCursor"/> object given
        /// </summary>
        /// <param name="cursor">Cursor to check</param>
        /// <returns><see cref="MouseCursor"/> that is not the inherited placeholder</returns>
        private MouseCursor UnwrapCursor(MouseCursor cursor)
        {
            return (cursor == INHERIT_CURSOR) ? null : cursor;
        }

        /// <summary>
        /// Throw if the <see cref="Image"/> name is empty or not defined
        /// </summary>
        /// <param name="name">Image name</param>
        /// <param name="xmlp">XML parser</param>
        private void CheckImageName(String name, XMLParser xmlp)
        {
            ParserUtil.CheckNameNotEmpty(name, xmlp);
            if (_images.ContainsKey(name))
            {
                throw xmlp.Error("image \"" + name + "\" already defined");
            }
        }

        /// <summary>
        /// Get <see cref="Border"/> of specified <see cref="Image"/>
        /// </summary>
        /// <param name="image"><see cref="Image"/> to get <see cref="Border"/> from</param>
        /// <param name="border"><see cref="Border"/> returned if not null</param>
        /// <returns><see cref="Image"/> <see cref="Border"/></returns>
        private static Border GetBorder(Image image, Border border)
        {
            if (border == null && (image is HasBorder))
            {
                border = ((HasBorder)image).Border;
            }
            return border;
        }

        /// <summary>
        /// Parse <see cref="MouseCursor"/> from XML parser
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="name"><see cref="MouseCursor"/> name</param>
        private void ParseCursor(XMLParser xmlp, String name)
        {
            String reference = xmlp.GetAttributeValue(null, "ref");
            MouseCursor cursor;
            if (reference != null)
            {
                cursor = _cursors[reference];
                if (cursor == null)
                {
                    throw xmlp.Error("referenced cursor \"" + reference + "\" not found");
                }
            }
            else
            {
                ImageParams imageParams = new ImageParams();
                ParseRectFromAttribute(xmlp, imageParams);
                int hotSpotX = xmlp.ParseIntFromAttribute("hotSpotX");
                int hotSpotY = xmlp.ParseIntFromAttribute("hotSpotY");
                String imageRefStr = xmlp.GetAttributeValue(null, "imageRef");

                Image imageRef = null;
                if (imageRefStr != null)
                {
                    imageRef = GetReferencedImage(xmlp, imageRefStr);
                }
                cursor = _currentTexture.CreateCursor(imageParams.X, imageParams.Y, imageParams.W, imageParams.H, hotSpotX, hotSpotY, imageRef);
                if (cursor == null)
                {
                    cursor = DefaultMouseCursor.OS_DEFAULT;
                }
            }
            _cursors.Add(name, cursor);
            xmlp.NextTag();
        }

        /// <summary>
        /// Parse an <see cref="Image"/> and <see cref="StateExpression"/> from the <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp"><see cref="XMLParser"/></param>
        /// <param name="tagName">Image type denoted by an XML tag</param>
        /// <returns>Parsed drawable <see cref="Image"/></returns>
        private Image ParseImage(XMLParser xmlp, String tagName)
        {
            ImageParams parameters = new ImageParams();
            parameters.Condition = ParserUtil.ParseCondition(xmlp);
            return ParseImageNoCond(xmlp, tagName, parameters);
        }

        /// <summary>
        /// Parse an <see cref="Image"/> without any condition from the <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp"><see cref="XMLParser"/></param>
        /// <param name="tagName">Image type denoted by an XML tag</param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        /// <returns>Parsed drawable <see cref="Image"/></returns>
        private Image ParseImageNoCond(XMLParser xmlp, String tagName, ImageParams parameters)
        {
            ParseStdAttributes(xmlp, parameters);
            Image image = ParseImageDelegate(xmlp, tagName, parameters);
            return AdjustImage(image, parameters);
        }

        /// <summary>
        /// Adjust an <see cref="Image"/> according to the <see cref="ImageParams"/>, returning a wrapper <see cref="Image"/> to be used in place
        /// </summary>
        /// <param name="image">Image to wrap</param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        /// <returns>Parsed drawable <see cref="Image"/> with adjustments</returns>
        private Image AdjustImage(Image image, ImageParams parameters)
        {
            Border border = GetBorder(image, parameters.Border);
            if (parameters.TintColor != null && !Color.WHITE.Equals(parameters.TintColor))
            {
                image = image.CreateTintedVersion(parameters.TintColor);
            }
            if (parameters.RepeatX || parameters.RepeatY)
            {
                image = new RepeatImage(image, border, parameters.RepeatX, parameters.RepeatY);
            }
            Border imgBorder = GetBorder(image, null);
            if ((border != null && border != imgBorder) || parameters.Inset != null ||
                    parameters.Center || parameters.Condition != null ||
                    parameters.SizeOverwriteH >= 0 || parameters.SizeOverwriteV >= 0)
            {
                image = new ImageAdjustments(image, border, parameters.Inset,
                        parameters.SizeOverwriteH, parameters.SizeOverwriteV,
                        parameters.Center, parameters.Condition);
            }
            return image;
        }

        /// <summary>
        /// Parse an <see cref="Image"/> given the element currently being parsed by <see cref="XMLParser"/> and acquired <see cref="ImageParams"/>
        /// </summary>
        /// <param name="xmlp"><see cref="XMLParser"/></param>
        /// <param name="tagName">Image type denoted by an XML tag</param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        /// <returns>Parsed drawable <see cref="Image"/></returns>
        private Image ParseImageDelegate(XMLParser xmlp, String tagName, ImageParams parameters)
        {
            if ("area".Equals(tagName))
            {
                return ParseArea(xmlp, parameters);
            }
            else if ("alias".Equals(tagName))
            {
                return ParseAlias(xmlp);
            }
            else if ("composed".Equals(tagName))
            {
                return ParseComposed(xmlp, parameters);
            }
            else if ("select".Equals(tagName))
            {
                return ParseStateSelect(xmlp, parameters);
            }
            else if ("grid".Equals(tagName))
            {
                return ParseGrid(xmlp, parameters);
            }
            else if ("animation".Equals(tagName))
            {
                return ParseAnimation(xmlp, parameters);
            }
            else if ("gradient".Equals(tagName))
            {
                return ParseGradient(xmlp, parameters);
            }
            else
            {
                throw xmlp.Error("Unexpected '" + tagName + "'");
            }
        }

        /// <summary>
        /// Parse a <see cref="ComposedImage"/> using <see cref="XMLParser"/> which is constructed by layering multiple instances of <see cref="Image"/>
        /// </summary>
        /// <param name="xmlp"><see cref="XMLParser"/></param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        /// <returns>Parsed drawable <see cref="Image"/></returns>
        private Image ParseComposed(XMLParser xmlp, ImageParams parameters)
        {
            List<Image> layers = new List<Image>();
            xmlp.NextTag();
            while (!xmlp.IsEndTag())
            {
                xmlp.Require(XmlPullParser.START_TAG, null, null);
                String tagName = xmlp.GetName();
                Image image = ParseImage(xmlp, tagName);
                layers.Add(image);
                parameters.Border = GetBorder(image, parameters.Border);
                xmlp.Require(XmlPullParser.END_TAG, null, tagName);
                xmlp.NextTag();
            }
            switch (layers.Count)
            {
                case 0:
                    return NONE;
                case 1:
                    return layers[0];
                default:
                    return new ComposedImage(
                            layers.ToArray(),
                            parameters.Border);
            }
        }

        /// <summary>
        /// Parse <see cref="Image"/>s with conditions from <see cref="StateSelect"/> using an <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        /// <returns>Parsed drawable <see cref="Image"/></returns>
        private Image ParseStateSelect(XMLParser xmlp, ImageParams parameters)
        {
            List<Image> stateImages = new List<Image>();
            List<StateExpression> conditions = new List<StateExpression>();
            xmlp.NextTag();
            bool last = false;
            while (!last && !xmlp.IsEndTag())
            {
                xmlp.Require(XmlPullParser.START_TAG, null, null);
                StateExpression cond = ParserUtil.ParseCondition(xmlp);
                String tagName = xmlp.GetName();
                Image ximage = ParseImageNoCond(xmlp, tagName, new ImageParams());
                parameters.Border = GetBorder(ximage, parameters.Border);
                xmlp.Require(XmlPullParser.END_TAG, null, tagName);
                xmlp.NextTag();
                last = cond == null;

                if (ximage is ImageAdjustments)
                {
                    ImageAdjustments ia = (ImageAdjustments)ximage;
                    if (ia.IsSimple())
                    {
                        cond = And(cond, ia._condition);
                        ximage = ia._image;
                    }
                }

                if (StateSelect.IsUseOptimizer() && (ximage is StateSelectImage))
                {
                    InlineSelect((StateSelectImage)ximage, cond, stateImages, conditions);
                }
                else
                {
                    stateImages.Add(ximage);
                    if (cond != null)
                    {
                        conditions.Add(cond);
                    }
                }
            }
            if (conditions.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine(xmlp.GetFilePosition() + ": state select image needs atleast 1 condition");

                if (stateImages.Count == 0)
                {
                    return NONE;
                }
                else
                {
                    return stateImages[0];
                }
            }
            StateSelect select = new StateSelect(conditions);
            Image image = new StateSelectImage(select, parameters.Border, stateImages.ToArray());
            return image;
        }

        /// <summary>
        /// Apply parent expressions to another included <see cref="StateSelectImage"/>
        /// </summary>
        /// <param name="src"><see cref="StateSelectImage"/> with items to compare</param>
        /// <param name="cond">Common <see cref="StateExpression"/></param>
        /// <param name="stateImages">Output here list of working <see cref="Image"/>s</param>
        /// <param name="conditions">Output here list of working conditions</param>
        private static void InlineSelect(StateSelectImage src, StateExpression cond, List<Image> stateImages, List<StateExpression> conditions)
        {
            int n = src.Images.Length;
            int m = src.Select.Expressions();
            for (int i = 0; i < n; i++)
            {
                StateExpression imgCond = (i < m) ? src.Select.ExpressionAt(i) : null;
                imgCond = And(imgCond, cond);
                stateImages.Add(src.Images[i]);
                if (imgCond != null)
                {
                    conditions.Add(imgCond);
                }
            }
            if (n == m && cond != null)
            {
                // when the src StateSelectImage doesn't have a default entry
                // (which is used when no condition matched) then add one with
                // NONE as image (except when inlining as default entry)
                stateImages.Add(NONE);
                conditions.Add(cond);
            }
        }

        /// <summary>
        /// Logic AND - Test 2 <see cref="StateExpression"/>s evaluate true together
        /// </summary>
        /// <param name="imgCond">Left operand</param>
        /// <param name="cond">Right operand</param>
        /// <returns>Logic object used for comparing both, or if one operand is null, the other operand that isn't null</returns>
        private static StateExpression And(StateExpression imgCond, StateExpression cond)
        {
            if (imgCond == null)
            {
                imgCond = cond;
            }
            else if (cond != null)
            {
                imgCond = new Logic('+', imgCond, cond);
            }
            return imgCond;
        }

        /// <summary>
        /// Parse an <see cref="Image"/> defined by an area in an image file
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        /// <returns>Parsed drawable <see cref="Image"/></returns>
        private Image ParseArea(XMLParser xmlp, ImageParams parameters)
        {
            ParseRectFromAttribute(xmlp, parameters);
            ParseRotationFromAttribute(xmlp, parameters);
            bool tiled = xmlp.ParseBoolFromAttribute("tiled", false);
            int[] splitx = ParseSplit2(xmlp, "splitx", Math.Abs(parameters.W));
            int[] splity = ParseSplit2(xmlp, "splity", Math.Abs(parameters.H));
            Image image;
            if (splitx != null || splity != null)
            {
                bool noCenter = xmlp.ParseBoolFromAttribute("nocenter", false);
                int columns = (splitx != null) ? 3 : 1;
                int rows = (splity != null) ? 3 : 1;
                Image[] imageParts = new Image[columns * rows];
                for (int r = 0; r < rows; r++)
                {
                    int imgY, imgH;
                    if (splity != null)
                    {
                        imgY = (parameters.H < 0) ? (parameters.Y - parameters.H - splity[r + 1]) : (parameters.Y + splity[r]);
                        imgH = (splity[r + 1] - splity[r]) * Math.Sign(parameters.H);
                    }
                    else
                    {
                        imgY = parameters.Y;
                        imgH = parameters.H;
                    }
                    for (int c = 0; c < columns; c++)
                    {
                        int imgX, imgW;
                        if (splitx != null)
                        {
                            imgX = (parameters.W < 0) ? (parameters.X - parameters.W - splitx[c + 1]) : (parameters.X + splitx[c]);
                            imgW = (splitx[c + 1] - splitx[c]) * Math.Sign(parameters.W);
                        }
                        else
                        {
                            imgX = parameters.X;
                            imgW = parameters.W;
                        }

                        bool isCenter = (r == rows / 2) && (c == columns / 2);
                        Image img;
                        if (noCenter && isCenter)
                        {
                            img = new EmptyImage(imgW, imgH);
                        }
                        else
                        {
                            img = CreateImage(xmlp, imgX, imgY, imgW, imgH, parameters.TintColor, isCenter & tiled, parameters.Rot);
                        }
                        int idx;
                        switch (parameters.Rot)
                        {
                            default:
                                idx = r * columns + c;
                                break;
                            case TextureRotation.Clockwise90:
                                idx = c * rows + (rows - 1 - r);
                                break;
                            case TextureRotation.Clockwise180:
                                idx = (rows - 1 - r) * columns + (columns - 1 - c);
                                break;
                            case TextureRotation.Clockwise270:
                                idx = (columns - 1 - c) * rows + r;
                                break;

                        }
                        imageParts[idx] = img;
                    }
                }
                switch (parameters.Rot)
                {
                    case TextureRotation.Clockwise90:
                    case TextureRotation.Clockwise270:
                        image = new GridImage(imageParts,
                                (splity != null) ? SPLIT_WEIGHTS_3 : SPLIT_WEIGHTS_1,
                                (splitx != null) ? SPLIT_WEIGHTS_3 : SPLIT_WEIGHTS_1,
                                parameters.Border);
                        break;
                    default:
                        image = new GridImage(imageParts,
                                (splitx != null) ? SPLIT_WEIGHTS_3 : SPLIT_WEIGHTS_1,
                                (splity != null) ? SPLIT_WEIGHTS_3 : SPLIT_WEIGHTS_1,
                                parameters.Border);
                        break;
                }
            }
            else
            {
                image = CreateImage(xmlp, parameters.X, parameters.Y, parameters.W, parameters.H, parameters.TintColor, tiled, parameters.Rot);
            }
            int tagToken = xmlp.NextTag();
            parameters.TintColor = null;
            if (tiled)
            {
                parameters.RepeatX = false;
                parameters.RepeatY = false;
            }
            return image;
        }

        /// <summary>
        /// Parse an alias pointing to an already defined image
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <returns>Parsed drawable <see cref="Image"/></returns>
        private Image ParseAlias(XMLParser xmlp)
        {
            Image image = GetReferencedImage(xmlp);
            xmlp.NextTag();
            return image;
        }

        /// <summary>
        /// Parsed a comma delimited string which contains directions and pixel lengths for <c>area</c> tags, specifying where/how to split the image
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="attribName">XML attribute name</param>
        /// <param name="size">Maximum output parameter</param>
        /// <returns>Area split arguments</returns>
        private static int[] ParseSplit2(XMLParser xmlp, String attribName, int size)
        {
            String splitStr = xmlp.GetAttributeValue(null, attribName);
            if (splitStr != null)
            {
                int comma = splitStr.IndexOf(',');
                if (comma < 0)
                {
                    throw xmlp.Error(attribName + " requires 2 values");
                }
                try
                {
                    int[] result = new int[4];
                    for (int i = 0, start = 0; i < 2; i++)
                    {
                        String part = TextUtil.Trim(splitStr, start, comma);
                        if (part.Length == 0)
                        {
                            throw new FormatException("number is empty string");
                        }
                        int off = 0;
                        int sign = 1;
                        switch (part[0])
                        {
                            case 'b':
                                off = size;
                                sign = -1;
                                part = TextUtil.Trim(part, 1);
                                break;
                            case 'B':
                                off = size;
                                sign = -1;
                                part = TextUtil.Trim(part, 1);
                                break;
                            case 'r':
                                off = size;
                                sign = -1;
                                part = TextUtil.Trim(part, 1);
                                break;
                            case 'R':
                                off = size;
                                sign = -1;
                                part = TextUtil.Trim(part, 1);
                                break;
                            // fall through
                            case 't':
                                part = TextUtil.Trim(part, 1);
                                break;
                            case 'T':
                                part = TextUtil.Trim(part, 1);
                                break;
                            case 'l':
                                part = TextUtil.Trim(part, 1);
                                break;
                            case 'L':
                                part = TextUtil.Trim(part, 1);
                                break;
                        }
                        int value = Int32.Parse(part);
                        result[i + 1] = Math.Max(0, Math.Min(size, off + sign * value));

                        start = comma + 1;
                        comma = splitStr.Length;
                    }
                    if (result[1] > result[2])
                    {
                        int tmp = result[1];
                        result[1] = result[2];
                        result[2] = tmp;
                    }
                    result[3] = size;
                    return result;
                }
                catch (FormatException ex)
                {
                    throw xmlp.Error("Unable to parse " + attribName + ": \"" + splitStr + "\"", ex);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Parse a number of sub-<see cref="Image"/>s into the <paramref name="textures"/> array
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="textures">Array to store sub=images</param>
        private void ParseSubImages(XMLParser xmlp, Image[] textures)
        {
            int idx = 0;
            while (xmlp.IsStartTag())
            {
                if (idx == textures.Length)
                {
                    throw xmlp.Error("Too many sub images");
                }
                String tagName = xmlp.GetName();
                textures[idx++] = ParseImage(xmlp, tagName);
                xmlp.Require(XmlPullParser.END_TAG, null, tagName);
                xmlp.NextTag();
            }
            if (idx != textures.Length)
            {
                throw xmlp.Error("Not enough sub images");
            }
        }

        /// <summary>
        /// Parse a <see cref="GridImage"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        /// <returns>Parsed drawable <see cref="Image"/></returns>
        private Image ParseGrid(XMLParser xmlp, ImageParams parameters)
        {
            try
            {
                int[] weightsX = ParserUtil.ParseIntArrayFromAttribute(xmlp, "weightsX");
                int[] weightsY = ParserUtil.ParseIntArrayFromAttribute(xmlp, "weightsY");
                Image[] textures = new Image[weightsX.Length * weightsY.Length];
                xmlp.NextTag();
                ParseSubImages(xmlp, textures);
                Image image = new GridImage(textures, weightsX, weightsY, parameters.Border);
                return image;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw xmlp.Error("Invalid value", ex);
            }
        }

        private static int[] SPLIT_WEIGHTS_3 = { 0, 1, 0 };
        private static int[] SPLIT_WEIGHTS_1 = { 1 };

        /// <summary>
        /// Parse <see cref="AnimatedImage"/> attributes
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="tagName">Tag of animation attribute</param>
        /// <param name="frames">Accumulated list of frames for the <see cref="AnimatedImage"/></param>
        private void ParseAnimElements(XMLParser xmlp, String tagName, List<AnimatedImage.Element> frames)
        {
            if ("repeat".Equals(tagName))
            {
                frames.Add(ParseAnimRepeat(xmlp));
            }
            else if ("frame".Equals(tagName))
            {
                frames.Add(ParseAnimFrame(xmlp));
            }
            else if ("frames".Equals(tagName))
            {
                ParseAnimFrames(xmlp, frames);
            }
            else
            {
                throw xmlp.Unexpected();
            }
        }

        /// <summary>
        /// Parse single animation frame for <see cref="AnimatedImage"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <returns>object local to <see cref="AnimatedImage"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private AnimatedImage.Img ParseAnimFrame(XMLParser xmlp)
        {
            int duration = xmlp.ParseIntFromAttribute("duration");
            if (duration < 0)
            {
                throw new ArgumentOutOfRangeException("duration must be >= 0 ms");
            }
            AnimParams animParams = ParseAnimParams(xmlp);
            Image image = GetReferencedImage(xmlp);
            AnimatedImage.Img img = new AnimatedImage.Img(duration, image, animParams.TintColor,
                    animParams.ZoomX, animParams.ZoomY, animParams.ZoomCenterX, animParams.ZoomCenterY);
            xmlp.NextTag();
            return img;
        }

        /// <summary>
        /// Parse animation parameters
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <returns><see cref="AnimParams"/></returns>
        private AnimParams ParseAnimParams(XMLParser xmlp)
        {
            AnimParams parameters = new AnimParams();
            parameters.TintColor = ParserUtil.ParseColorFromAttribute(xmlp, "tint", _constants, Color.WHITE);
            float zoom = xmlp.ParseFloatFromAttribute("zoom", 1.0f);
            parameters.ZoomX = xmlp.ParseFloatFromAttribute("zoomX", zoom);
            parameters.ZoomY = xmlp.ParseFloatFromAttribute("zoomY", zoom);
            parameters.ZoomCenterX = xmlp.ParseFloatFromAttribute("zoomCenterX", 0.5f);
            parameters.ZoomCenterY = xmlp.ParseFloatFromAttribute("zoomCenterY", 0.5f);
            return parameters;
        }

        /// <summary>
        /// Parse a number of frames for an <see cref="AnimatedImage"/> specific to the internals of the image.
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="frames">Resultant <see cref="AnimatedImage.Element"/> element list</param>
        /// <exception cref="ArgumentOutOfRangeException">Parsed illegal parameters</exception>
        private void ParseAnimFrames(XMLParser xmlp, List<AnimatedImage.Element> frames)
        {
            ImageParams parameters = new ImageParams();
            ParseRectFromAttribute(xmlp, parameters);
            ParseRotationFromAttribute(xmlp, parameters);
            int duration = xmlp.ParseIntFromAttribute("duration");
            if (duration < 1)
            {
                throw new ArgumentOutOfRangeException("duration must be >= 1 ms");
            }
            int count = xmlp.ParseIntFromAttribute("count");
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count must be >= 1");
            }
            AnimParams animParams = ParseAnimParams(xmlp);
            int xOffset = xmlp.ParseIntFromAttribute("offsetx", 0);
            int yOffset = xmlp.ParseIntFromAttribute("offsety", 0);
            if (count > 1 && (xOffset == 0 && yOffset == 0))
            {
                throw new ArgumentOutOfRangeException("offsets required for multiple frames");
            }
            for (int i = 0; i < count; i++)
            {
                Image image = CreateImage(xmlp, parameters.X, parameters.Y, parameters.W, parameters.H, Color.WHITE, false, parameters.Rot);
                AnimatedImage.Img img = new AnimatedImage.Img(duration, image, animParams.TintColor,
                        animParams.ZoomX, animParams.ZoomY, animParams.ZoomCenterX, animParams.ZoomCenterY);
                frames.Add(img);
                parameters.X += xOffset;
                parameters.Y += yOffset;
            }

            xmlp.NextTag();
        }

        /// <summary>
        /// Parse an <see cref="AnimatedImage.Element"/> specific to repeated frames
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <returns><see cref="AnimatedImage.Repeat"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private AnimatedImage.Repeat ParseAnimRepeat(XMLParser xmlp)
        {
            String strRepeatCount = xmlp.GetAttributeValue(null, "count");
            int repeatCount = 0;
            if (strRepeatCount != null)
            {
                repeatCount = Int32.Parse(strRepeatCount);
                if (repeatCount <= 0)
                {
                    throw new ArgumentOutOfRangeException("Invalid repeat count");
                }
            }
            bool lastRepeatsEndless = false;
            bool hasWarned = false;
            List<AnimatedImage.Element> children = new List<AnimatedImage.Element>();
            xmlp.NextTag();
            while (xmlp.IsStartTag())
            {
                if (lastRepeatsEndless && !hasWarned)
                {
                    hasWarned = true;
                    System.Diagnostics.Debug.WriteLine("Animation frames after an endless repeat won''t be displayed: " + xmlp.GetPositionDescription());
                }
                String tagName = xmlp.GetName();
                ParseAnimElements(xmlp, tagName, children);
                AnimatedImage.Element e = children[children.Count - 1];
                lastRepeatsEndless =
                        (e is AnimatedImage.Repeat) &&
                        ((AnimatedImage.Repeat)e)._repeatCount == 0;
                xmlp.Require(XmlPullParser.END_TAG, null, tagName);
                xmlp.NextTag();
            }
            return new AnimatedImage.Repeat(children.ToArray(), repeatCount);
        }

        /// <summary>
        /// Get an animated element's border
        /// </summary>
        /// <param name="e"><see cref="AnimatedImage"/> element</param>
        /// <returns><see cref="Border"/></returns>
        private Border GetBorder(AnimatedImage.Element e)
        {
            if (e is AnimatedImage.Repeat)
            {
                AnimatedImage.Repeat r = (AnimatedImage.Repeat)e;
                foreach (AnimatedImage.Element c in r._children)
                {
                    Border border = GetBorder(c);
                    if (border != null)
                    {
                        return border;
                    }
                }
            }
            else if (e is AnimatedImage.Img)
            {
                AnimatedImage.Img i = (AnimatedImage.Img)e;
                if (i.Image is HasBorder)
                {
                    return ((HasBorder)i.Image).Border;
                }
            }
            return null;
        }

        /// <summary>
        /// Parse animation from <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        /// <returns>Parsed drawable <see cref="Image"/></returns>
        private Image ParseAnimation(XMLParser xmlp, ImageParams parameters)
        {
            try
            {
                String timeSource = xmlp.GetAttributeNotNull("timeSource");
                int frozenTime = xmlp.ParseIntFromAttribute("frozenTime", -1);
                AnimatedImage.Repeat root = ParseAnimRepeat(xmlp);
                if (parameters.Border == null)
                {
                    parameters.Border = GetBorder(root);
                }
                Image image = new AnimatedImage(_renderer, root, timeSource, parameters.Border,
                        (parameters.TintColor == null) ? Color.WHITE : parameters.TintColor, frozenTime);
                parameters.TintColor = null;
                return image;
            }
            catch (ArgumentException ex)
            {
                throw xmlp.Error("Unable to parse", ex);
            }
        }

        /// <summary>
        /// Parse gradient image from <see cref="XMLParser"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        /// <returns>Parsed drawable <see cref="Image"/></returns>
        private Image ParseGradient(XMLParser xmlp, ImageParams parameters)
        {
            try
            {
                GradientType type = xmlp.ParseEnumFromAttribute<GradientType>("type", typeof(GradientType));
                GradientWrap wrap = xmlp.ParseEnumFromAttribute<GradientWrap>("wrap", typeof(GradientWrap), GradientWrap.Scale);

                Gradient gradient = new Gradient(type);
                gradient.Wrap = wrap;

                xmlp.NextTag();
                while (xmlp.IsStartTag())
                {
                    xmlp.Require(XmlPullParser.START_TAG, null, "stop");
                    float pos = xmlp.ParseFloatFromAttribute("pos");
                    Color color = ParserUtil.ParseColor(xmlp, xmlp.GetAttributeNotNull("color"), _constants);
                    gradient.AddStop(pos, color);
                    xmlp.NextTag();
                    xmlp.Require(XmlPullParser.END_TAG, null, "stop");
                    xmlp.NextTag();
                }

                return _renderer.CreateGradient(gradient);
            }
            catch (ArgumentException ex)
            {
                throw xmlp.Error("Unable to parse", ex);
            }
        }

        /// <summary>
        /// Create an <see cref="Image"/> from a given texture, using the parameters specified
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="x">X coordinate in texture</param>
        /// <param name="y">Y coordinate in texture</param>
        /// <param name="w">Width of sprite in texture</param>
        /// <param name="h">Height of sprite in texture</param>
        /// <param name="tintColor">Adjust image with a tint</param>
        /// <param name="tiled">Is the image tiled repeatedly</param>
        /// <param name="rotation">Rotate the resultant image</param>
        /// <returns><see cref="Image"/> from given parameters</returns>
        private Image CreateImage(XMLParser xmlp, int x, int y, int w, int h, Color tintColor, bool tiled, TextureRotation rotation)
        {
            if (w == 0 || h == 0)
            {
                return new EmptyImage(Math.Abs(w), Math.Abs(h));
            }

            Texture texture = _currentTexture;
            int texWidth = texture.Width;
            int texHeight = texture.Height;

            int x1 = x + Math.Abs(w);
            int y1 = y + Math.Abs(h);

            if (x < 0 || x >= texWidth || x1 < 0 || x1 > texWidth ||
                    y < 0 || y >= texHeight || y1 < 0 || y1 > texHeight)
            {
                System.Diagnostics.Debug.WriteLine("texture partly outside of file: " + xmlp.GetPositionDescription());
                x = Math.Max(0, Math.Min(x, texWidth));
                y = Math.Max(0, Math.Min(y, texHeight));
                w = Math.Sign(w) * (Math.Max(0, Math.Min(x1, texWidth)) - x);
                h = Math.Sign(h) * (Math.Max(0, Math.Min(y1, texHeight)) - y);
            }

            return texture.GetImage(x, y, w, h, tintColor, tiled, rotation);
        }

        /// <summary>
        /// Parse 'xywh' from given <see cref="XMLParser"/> storing it in the <see cref="ImageParams"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        private void ParseRectFromAttribute(XMLParser xmlp, ImageParams parameters)
        {
            if (_currentTexture == null)
            {
                throw xmlp.Error("can't create area outside of <imagefile> object");
            }
            String xywh = xmlp.GetAttributeNotNull("xywh");
            if ("*".Equals(xywh))
            {
                parameters.X = 0;
                parameters.Y = 0;
                parameters.W = _currentTexture.Width;
                parameters.H = _currentTexture.Height;
            }
            else try
                {
                    int[] coords = TextUtil.ParseIntArray(xywh);
                    if (coords.Length != 4)
                    {
                        throw xmlp.Error("xywh requires 4 integer arguments");
                    }
                    parameters.X = coords[0];
                    parameters.Y = coords[1];
                    parameters.W = coords[2];
                    parameters.H = coords[3];
                }
                catch (ArgumentException ex)
                {
                    throw xmlp.Error("can't parse xywh argument", ex);
                }
        }

        /// <summary>
        /// Parse an image rotation from 'rot' in the <see cref="XMLParser"/> storing it in the <see cref="ImageParams"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        private void ParseRotationFromAttribute(XMLParser xmlp, ImageParams parameters)
        {
            if (_currentTexture == null)
            {
                throw xmlp.Error("can't create area outside of <imagefile> object");
            }
            int rot = xmlp.ParseIntFromAttribute("rot", 0);
            switch (rot)
            {
                case 0: parameters.Rot = TextureRotation.None; break;
                case 90: parameters.Rot = TextureRotation.Clockwise90; break;
                case 180: parameters.Rot = TextureRotation.Clockwise180; break;
                case 270: parameters.Rot = TextureRotation.Clockwise270; break;
                default:
                    throw xmlp.Error("invalid rotation angle");
            }
        }

        /// <summary>
        /// Parse standard image attributes into <see cref="ImageParams"/>
        /// </summary>
        /// <param name="xmlp">XML parser</param>
        /// <param name="parameters">Standard image attributes to adjust and render with</param>
        private void ParseStdAttributes(XMLParser xmlp, ImageParams parameters)
        {
            parameters.TintColor = ParserUtil.ParseColorFromAttribute(xmlp, "tint", _constants, null);
            parameters.Border = ParserUtil.ParseBorderFromAttribute(xmlp, "border");
            parameters.Inset = ParserUtil.ParseBorderFromAttribute(xmlp, "inset");
            parameters.RepeatX = xmlp.ParseBoolFromAttribute("repeatX", false);
            parameters.RepeatY = xmlp.ParseBoolFromAttribute("repeatY", false);
            parameters.SizeOverwriteH = ParserUtil.ParseIntExpressionFromAttribute(xmlp, "sizeOverwriteH", -1, _mathInterpreter);
            parameters.SizeOverwriteV = ParserUtil.ParseIntExpressionFromAttribute(xmlp, "sizeOverwriteV", -1, _mathInterpreter);
            parameters.Center = xmlp.ParseBoolFromAttribute("center", false);
        }

        /// <summary>
        /// Manage adjustments or locations of a parsed image
        /// </summary>
        public class ImageParams
        {
            public int X, Y, W, H;
            public Color TintColor;
            public Border Border;
            public Border Inset;
            public bool RepeatX;
            public bool RepeatY;
            public int SizeOverwriteH = -1;
            public int SizeOverwriteV = -1;
            public bool Center;
            public StateExpression Condition;
            public TextureRotation Rot;
        }

        /// <summary>
        /// Manage variables of a specified animation
        /// </summary>
        public class AnimParams
        {
            public Color TintColor;
            public float ZoomX;
            public float ZoomY;
            public float ZoomCenterX;
            public float ZoomCenterY;
        }

        /// <summary>
        /// An implementation of <see cref="AbstractMathInterpreter"/> which can access images, constants or parameter fields and place them on the stack
        /// </summary>
        public class MathInterpreter : AbstractMathInterpreter
        {
            private ImageManager _imageManager;

            /// <summary>
            /// Initialise the math interpreter that can look up using a <see cref="ImageManager"/>
            /// </summary>
            /// <param name="imageManager"><see cref="ImageManager"/> where to access variables</param>
            public MathInterpreter(ImageManager imageManager)
            {
                this._imageManager = imageManager;
            }

            public override void AccessVariable(String name)
            {
                Image img = this._imageManager[name];
                if (img != null)
                {
                    Push(img);
                    return;
                }
                Object obj = this._imageManager._constants[name];
                if (obj != null)
                {
                    Push(obj);
                    return;
                }
                throw new ArgumentOutOfRangeException("variable not found: " + name);
            }

            protected override Object AccessField(Object obj, String field)
            {
                if (obj is ParameterMapImpl)
                {
                    Object result = ((ParameterMapImpl)obj)[field];
                    if (result == null)
                    {
                        throw new ArgumentNullException("field not found: " + field);
                    }
                    return result;
                }
                if ((obj is Image) && "border".Equals(field))
                {
                    Border border = null;
                    if (obj is HasBorder)
                    {
                        border = ((HasBorder)obj).Border;
                    }
                    return (border != null) ? border : Border.ZERO;
                }
                return base.AccessField(obj, field);
            }
        }
    }
}
