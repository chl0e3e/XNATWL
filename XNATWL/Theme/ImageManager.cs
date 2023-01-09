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
using System.Collections.Generic;
using System.IO;
using XNATWL.IO;
using XNATWL.Renderer;
using XNATWL.Utils;

namespace XNATWL.Theme
{
    public class ImageManager
    {
        ParameterMapImpl constants;
        private Renderer.Renderer renderer;
        private SortedDictionary<String, Image> images;
        private SortedDictionary<String, MouseCursor> cursors;
        private MathInterpreter mathInterpreter;

        private Texture currentTexture;

        internal static EmptyImage NONE = new EmptyImage(0, 0);
        private static MouseCursor INHERIT_CURSOR = InheritedMouseCursor.INHERITED_DEFAULT;// new MouseCursor() { };

        public ImageManager(ParameterMapImpl constants, Renderer.Renderer renderer)
        {
            this.constants = constants;
            this.renderer = renderer;
            this.images = new SortedDictionary<String, Image>();
            this.cursors = new SortedDictionary<String, MouseCursor>();
            this.mathInterpreter = new MathInterpreter(this);

            images.Add("none", NONE);
            cursors.Add("os-default", DefaultMouseCursor.OS_DEFAULT);
            cursors.Add("inherit", INHERIT_CURSOR);
        }

        public Image getImage(String name)
        {
            if (!images.ContainsKey(name))
            {
                return null;
            }
            return images[name];
        }

        public Image getReferencedImage(XMLParser xmlp)
        {
            String reference = xmlp.getAttributeNotNull("ref");
            return getReferencedImage(xmlp, reference);
        }

        public Image getReferencedImage(XMLParser xmlp, String reference)
        {
            if (reference.EndsWith(".*"))
            {
                throw xmlp.error("wildcard mapping not allowed");
            }
            Image img = images[reference];
            if (img == null)
            {
                throw xmlp.error("referenced image \"" + reference + "\" not found");
            }
            return img;
        }

        public MouseCursor getReferencedCursor(XMLParser xmlp, String reference)
        {
            MouseCursor cursor = cursors[reference];
            if (cursor == null)
            {
                throw xmlp.error("referenced cursor \"" + reference + "\" not found");
            }
            return unwrapCursor(cursor);
        }

        public Dictionary<String, Image> getImages(String reference, String name)
        {
            return ParserUtil.resolve(images, reference, name, null);
        }

        public MouseCursor getCursor(String name)
        {
            return unwrapCursor(cursors[name]);
        }

        public Dictionary<String, MouseCursor> getCursors(String reference, String name)
        {
            return ParserUtil.resolve(cursors, reference, name, INHERIT_CURSOR);
        }

        public void parseImages(XMLParser xmlp, FileSystemObject baseFolder)
        {
            xmlp.require(XmlPullParser.START_TAG, null, null);

            Texture texture = null;
            String fileName = xmlp.getAttributeValue(null, "file");
            if (fileName != null)
            {
                String fmt = xmlp.getAttributeValue(null, "format");
                String filter = xmlp.getAttributeValue(null, "filter");
                // ignore the comment so that it does not cause a warning
                xmlp.getAttributeValue(null, "comment");

                try
                {
                    texture = renderer.LoadTexture(new FileSystemObject(baseFolder, fileName), fmt, filter);
                    if (texture == null)
                    {
                        throw new NullReferenceException("loadTexture returned null");
                    }
                }
                catch (IOException ex)
                {
                    throw xmlp.error("Unable to load image file: " + fileName, ex);
                }
            }

            this.currentTexture = texture;

            try
            {
                xmlp.nextTag();
                while (!xmlp.isEndTag())
                {
                    bool emptyElement = xmlp.isEmptyElement();
                    String tagName = xmlp.getName();
                    String name = xmlp.getAttributeNotNull("name");
                    checkImageName(name, xmlp);

                    if ("cursor".Equals(xmlp.getName()))
                    {
                        parseCursor(xmlp, name);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Parsing image subtag: " + name + " <" + tagName + ">");
                        Image image = parseImage(xmlp, tagName);
                        images.Add(name, image);
                    }

                    if (!emptyElement)
                    {
                        xmlp.require(XmlPullParser.END_TAG, null, tagName);
                    }

                    xmlp.nextTag();
                }
            }
            finally
            {
                currentTexture = null;
                if (texture != null)
                {
                    texture.ThemeLoadingDone();
                }
            }
        }

        private MouseCursor unwrapCursor(MouseCursor cursor)
        {
            return (cursor == INHERIT_CURSOR) ? null : cursor;
        }

        private void checkImageName(String name, XMLParser xmlp)
        {
            ParserUtil.checkNameNotEmpty(name, xmlp);
            if (images.ContainsKey(name))
            {
                throw xmlp.error("image \"" + name + "\" already defined");
            }
        }

        private static Border getBorder(Image image, Border border)
        {
            if (border == null && (image is HasBorder))
            {
                border = ((HasBorder)image).Border;
            }
            return border;
        }

        private void parseCursor(XMLParser xmlp, String name)
        {
            String reference = xmlp.getAttributeValue(null, "ref");
            MouseCursor cursor;
            if (reference != null)
            {
                cursor = cursors[reference];
                if (cursor == null)
                {
                    throw xmlp.error("referenced cursor \"" + reference + "\" not found");
                }
            }
            else
            {
                ImageParams imageParams = new ImageParams();
                parseRectFromAttribute(xmlp, imageParams);
                int hotSpotX = xmlp.parseIntFromAttribute("hotSpotX");
                int hotSpotY = xmlp.parseIntFromAttribute("hotSpotY");
                String imageRefStr = xmlp.getAttributeValue(null, "imageRef");

                Image imageRef = null;
                if (imageRefStr != null)
                {
                    imageRef = getReferencedImage(xmlp, imageRefStr);
                }
                cursor = currentTexture.CreateCursor(imageParams.x, imageParams.y, imageParams.w, imageParams.h, hotSpotX, hotSpotY, imageRef);
                if (cursor == null)
                {
                    cursor = DefaultMouseCursor.OS_DEFAULT;
                }
            }
            cursors.Add(name, cursor);
            xmlp.nextTag();
        }

        private Image parseImage(XMLParser xmlp, String tagName)
        {
            ImageParams parameters = new ImageParams();
            parameters.condition = ParserUtil.parseCondition(xmlp);
            return parseImageNoCond(xmlp, tagName, parameters);
        }

        private Image parseImageNoCond(XMLParser xmlp, String tagName, ImageParams parameters)
        {
            parseStdAttributes(xmlp, parameters);
            Image image = parseImageDelegate(xmlp, tagName, parameters);
            return adjustImage(image, parameters);
        }

        private Image adjustImage(Image image, ImageParams parameters)
        {
            Border border = getBorder(image, parameters.border);
            if (parameters.tintColor != null && !Color.WHITE.Equals(parameters.tintColor))
            {
                image = image.CreateTintedVersion(parameters.tintColor);
            }
            if (parameters.repeatX || parameters.repeatY)
            {
                image = new RepeatImage(image, border, parameters.repeatX, parameters.repeatY);
            }
            Border imgBorder = getBorder(image, null);
            if ((border != null && border != imgBorder) || parameters.inset != null ||
                    parameters.center || parameters.condition != null ||
                    parameters.sizeOverwriteH >= 0 || parameters.sizeOverwriteV >= 0)
            {
                image = new ImageAdjustments(image, border, parameters.inset,
                        parameters.sizeOverwriteH, parameters.sizeOverwriteV,
                        parameters.center, parameters.condition);
            }
            return image;
        }

        private Image parseImageDelegate(XMLParser xmlp, String tagName, ImageParams parameters)
        {
            System.Diagnostics.Debug.WriteLine("parseImageDelegate : " + tagName);
            if ("area".Equals(tagName))
            {
                return parseArea(xmlp, parameters);
            }
            else if ("alias".Equals(tagName))
            {
                return parseAlias(xmlp);
            }
            else if ("composed".Equals(tagName))
            {
                return parseComposed(xmlp, parameters);
            }
            else if ("select".Equals(tagName))
            {
                return parseStateSelect(xmlp, parameters);
            }
            else if ("grid".Equals(tagName))
            {
                return parseGrid(xmlp, parameters);
            }
            else if ("animation".Equals(tagName))
            {
                return parseAnimation(xmlp, parameters);
            }
            else if ("gradient".Equals(tagName))
            {
                return parseGradient(xmlp, parameters);
            }
            else
            {
                throw xmlp.error("Unexpected '" + tagName + "'");
            }
        }

        private Image parseComposed(XMLParser xmlp, ImageParams parameters)
        {
            List<Image> layers = new List<Image>();
            xmlp.nextTag();
            while (!xmlp.isEndTag())
            {
                xmlp.require(XmlPullParser.START_TAG, null, null);
                String tagName = xmlp.getName();
                Image image = parseImage(xmlp, tagName);
                layers.Add(image);
                parameters.border = getBorder(image, parameters.border);
                xmlp.require(XmlPullParser.END_TAG, null, tagName);
                xmlp.nextTag();
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
                            parameters.border);
            }
        }

        private Image parseStateSelect(XMLParser xmlp, ImageParams parameters)
        {
            List<Image> stateImages = new List<Image>();
            List<StateExpression> conditions = new List<StateExpression>();
            xmlp.nextTag();
            bool last = false;
            while (!last && !xmlp.isEndTag())
            {
                xmlp.require(XmlPullParser.START_TAG, null, null);
                StateExpression cond = ParserUtil.parseCondition(xmlp);
                String tagName = xmlp.getName();
                Image ximage = parseImageNoCond(xmlp, tagName, new ImageParams());
                parameters.border = getBorder(ximage, parameters.border);
                xmlp.require(XmlPullParser.END_TAG, null, tagName);
                xmlp.nextTag();
                last = cond == null;

                if (ximage is ImageAdjustments)
                {
                    ImageAdjustments ia = (ImageAdjustments)ximage;
                    if (ia.IsSimple())
                    {
                        cond = and(cond, ia.condition);
                        ximage = ia.image;
                    }
                }

                if (StateSelect.IsUseOptimizer() && (ximage is StateSelectImage))
                {
                    inlineSelect((StateSelectImage)ximage, cond, stateImages, conditions);
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
                System.Diagnostics.Debug.WriteLine(xmlp.getFilePosition() + ": state select image needs atleast 1 condition");

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
            Image image = new StateSelectImage(select, parameters.border, stateImages.ToArray());
            return image;
        }

        private static void inlineSelect(StateSelectImage src, StateExpression cond, List<Image> stateImages, List<StateExpression> conditions)
        {
            int n = src.images.Length;
            int m = src.select.Expressions();
            for (int i = 0; i < n; i++)
            {
                StateExpression imgCond = (i < m) ? src.select.ExpressionAt(i) : null;
                imgCond = and(imgCond, cond);
                stateImages.Add(src.images[i]);
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

        private static StateExpression and(StateExpression imgCond, StateExpression cond)
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

        private Image parseArea(XMLParser xmlp, ImageParams parameters)
        {
            parseRectFromAttribute(xmlp, parameters);
            parseRotationFromAttribute(xmlp, parameters);
            bool tiled = xmlp.parseBoolFromAttribute("tiled", false);
            int[] splitx = parseSplit2(xmlp, "splitx", Math.Abs(parameters.w));
            int[] splity = parseSplit2(xmlp, "splity", Math.Abs(parameters.h));
            Image image;
            if (splitx != null || splity != null)
            {
                bool noCenter = xmlp.parseBoolFromAttribute("nocenter", false);
                int columns = (splitx != null) ? 3 : 1;
                int rows = (splity != null) ? 3 : 1;
                Image[] imageParts = new Image[columns * rows];
                for (int r = 0; r < rows; r++)
                {
                    int imgY, imgH;
                    if (splity != null)
                    {
                        imgY = (parameters.h < 0) ? (parameters.y - parameters.h - splity[r + 1]) : (parameters.y + splity[r]);
                        imgH = (splity[r + 1] - splity[r]) * Math.Sign(parameters.h);
                    }
                    else
                    {
                        imgY = parameters.y;
                        imgH = parameters.h;
                    }
                    for (int c = 0; c < columns; c++)
                    {
                        int imgX, imgW;
                        if (splitx != null)
                        {
                            imgX = (parameters.w < 0) ? (parameters.x - parameters.w - splitx[c + 1]) : (parameters.x + splitx[c]);
                            imgW = (splitx[c + 1] - splitx[c]) * Math.Sign(parameters.w);
                        }
                        else
                        {
                            imgX = parameters.x;
                            imgW = parameters.w;
                        }

                        bool isCenter = (r == rows / 2) && (c == columns / 2);
                        Image img;
                        if (noCenter && isCenter)
                        {
                            img = new EmptyImage(imgW, imgH);
                        }
                        else
                        {
                            img = createImage(xmlp, imgX, imgY, imgW, imgH, parameters.tintColor, isCenter & tiled, parameters.rot);
                        }
                        int idx;
                        switch (parameters.rot)
                        {
                            default:
                                idx = r * columns + c;
                                break;
                            case TextureRotation.CLOCKWISE_90:
                                idx = c * rows + (rows - 1 - r);
                                break;
                            case TextureRotation.CLOCKWISE_180:
                                idx = (rows - 1 - r) * columns + (columns - 1 - c);
                                break;
                            case TextureRotation.CLOCKWISE_270:
                                idx = (columns - 1 - c) * rows + r;
                                break;

                        }
                        imageParts[idx] = img;
                    }
                }
                switch (parameters.rot)
                {
                    case TextureRotation.CLOCKWISE_90:
                    case TextureRotation.CLOCKWISE_270:
                        image = new GridImage(imageParts,
                                (splity != null) ? SPLIT_WEIGHTS_3 : SPLIT_WEIGHTS_1,
                                (splitx != null) ? SPLIT_WEIGHTS_3 : SPLIT_WEIGHTS_1,
                                parameters.border);
                        break;
                    default:
                        image = new GridImage(imageParts,
                                (splitx != null) ? SPLIT_WEIGHTS_3 : SPLIT_WEIGHTS_1,
                                (splity != null) ? SPLIT_WEIGHTS_3 : SPLIT_WEIGHTS_1,
                                parameters.border);
                        break;
                }
            }
            else
            {
                image = createImage(xmlp, parameters.x, parameters.y, parameters.w, parameters.h, parameters.tintColor, tiled, parameters.rot);
            }
            int tagToken = xmlp.nextTag();
            parameters.tintColor = null;
            if (tiled)
            {
                parameters.repeatX = false;
                parameters.repeatY = false;
            }
            return image;
        }

        private Image parseAlias(XMLParser xmlp)
        {
            Image image = getReferencedImage(xmlp);
            xmlp.nextTag();
            return image;
        }

        private static int[] parseSplit2(XMLParser xmlp, String attribName, int size)
        {
            String splitStr = xmlp.getAttributeValue(null, attribName);
            if (splitStr != null)
            {
                int comma = splitStr.IndexOf(',');
                if (comma < 0)
                {
                    throw xmlp.error(attribName + " requires 2 values");
                }
                try
                {
                    int[] result = new int[4];
                    for (int i = 0, start = 0; i < 2; i++)
                    {
                        String part = TextUtil.trim(splitStr, start, comma);
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
                                part = TextUtil.trim(part, 1);
                                break;
                            case 'B':
                                off = size;
                                sign = -1;
                                part = TextUtil.trim(part, 1);
                                break;
                            case 'r':
                                off = size;
                                sign = -1;
                                part = TextUtil.trim(part, 1);
                                break;
                            case 'R':
                                off = size;
                                sign = -1;
                                part = TextUtil.trim(part, 1);
                                break;
                            // fall through
                            case 't':
                                part = TextUtil.trim(part, 1);
                                break;
                            case 'T':
                                part = TextUtil.trim(part, 1);
                                break;
                            case 'l':
                                part = TextUtil.trim(part, 1);
                                break;
                            case 'L':
                                part = TextUtil.trim(part, 1);
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
                    throw xmlp.error("Unable to parse " + attribName + ": \"" + splitStr + "\"", ex);
                }
            }
            else
            {
                return null;
            }
        }

        private void parseSubImages(XMLParser xmlp, Image[] textures)
        {
            int idx = 0;
            while (xmlp.isStartTag())
            {
                if (idx == textures.Length)
                {
                    throw xmlp.error("Too many sub images");
                }
                String tagName = xmlp.getName();
                textures[idx++] = parseImage(xmlp, tagName);
                xmlp.require(XmlPullParser.END_TAG, null, tagName);
                xmlp.nextTag();
            }
            if (idx != textures.Length)
            {
                throw xmlp.error("Not enough sub images");
            }
        }

        private Image parseGrid(XMLParser xmlp, ImageParams parameters)
        {
            try
            {
                int[] weightsX = ParserUtil.parseIntArrayFromAttribute(xmlp, "weightsX");
                int[] weightsY = ParserUtil.parseIntArrayFromAttribute(xmlp, "weightsY");
                Image[] textures = new Image[weightsX.Length * weightsY.Length];
                xmlp.nextTag();
                parseSubImages(xmlp, textures);
                Image image = new GridImage(textures, weightsX, weightsY, parameters.border);
                return image;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw xmlp.error("Invalid value", ex);
            }
        }

        private static int[] SPLIT_WEIGHTS_3 = { 0, 1, 0 };
        private static int[] SPLIT_WEIGHTS_1 = { 1 };

        private void parseAnimElements(XMLParser xmlp, String tagName, List<AnimatedImage.Element> frames)
        {
            if ("repeat".Equals(tagName))
            {
                frames.Add(parseAnimRepeat(xmlp));
            }
            else if ("frame".Equals(tagName))
            {
                frames.Add(parseAnimFrame(xmlp));
            }
            else if ("frames".Equals(tagName))
            {
                parseAnimFrames(xmlp, frames);
            }
            else
            {
                throw xmlp.unexpected();
            }
        }

        private AnimatedImage.Img parseAnimFrame(XMLParser xmlp)
        {
            int duration = xmlp.parseIntFromAttribute("duration");
            if (duration < 0)
            {
                throw new ArgumentOutOfRangeException("duration must be >= 0 ms");
            }
            AnimParams animParams = parseAnimParams(xmlp);
            Image image = getReferencedImage(xmlp);
            AnimatedImage.Img img = new AnimatedImage.Img(duration, image, animParams.tintColor,
                    animParams.zoomX, animParams.zoomY, animParams.zoomCenterX, animParams.zoomCenterY);
            xmlp.nextTag();
            return img;
        }

        private AnimParams parseAnimParams(XMLParser xmlp)
        {
            AnimParams parameters = new AnimParams();
            parameters.tintColor = ParserUtil.parseColorFromAttribute(xmlp, "tint", constants, Color.WHITE);
            float zoom = xmlp.parseFloatFromAttribute("zoom", 1.0f);
            parameters.zoomX = xmlp.parseFloatFromAttribute("zoomX", zoom);
            parameters.zoomY = xmlp.parseFloatFromAttribute("zoomY", zoom);
            parameters.zoomCenterX = xmlp.parseFloatFromAttribute("zoomCenterX", 0.5f);
            parameters.zoomCenterY = xmlp.parseFloatFromAttribute("zoomCenterY", 0.5f);
            return parameters;
        }

        private void parseAnimFrames(XMLParser xmlp, List<AnimatedImage.Element> frames)
        {
            ImageParams parameters = new ImageParams();
            parseRectFromAttribute(xmlp, parameters);
            parseRotationFromAttribute(xmlp, parameters);
            int duration = xmlp.parseIntFromAttribute("duration");
            if (duration < 1)
            {
                throw new ArgumentOutOfRangeException("duration must be >= 1 ms");
            }
            int count = xmlp.parseIntFromAttribute("count");
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count must be >= 1");
            }
            AnimParams animParams = parseAnimParams(xmlp);
            int xOffset = xmlp.parseIntFromAttribute("offsetx", 0);
            int yOffset = xmlp.parseIntFromAttribute("offsety", 0);
            if (count > 1 && (xOffset == 0 && yOffset == 0))
            {
                throw new ArgumentOutOfRangeException("offsets required for multiple frames");
            }
            for (int i = 0; i < count; i++)
            {
                Image image = createImage(xmlp, parameters.x, parameters.y, parameters.w, parameters.h, Color.WHITE, false, parameters.rot);
                AnimatedImage.Img img = new AnimatedImage.Img(duration, image, animParams.tintColor,
                        animParams.zoomX, animParams.zoomY, animParams.zoomCenterX, animParams.zoomCenterY);
                frames.Add(img);
                parameters.x += xOffset;
                parameters.y += yOffset;
            }

            xmlp.nextTag();
        }

        private AnimatedImage.Repeat parseAnimRepeat(XMLParser xmlp)
        {
            String strRepeatCount = xmlp.getAttributeValue(null, "count");
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
            xmlp.nextTag();
            while (xmlp.isStartTag())
            {
                if (lastRepeatsEndless && !hasWarned)
                {
                    hasWarned = true;
                    System.Diagnostics.Debug.WriteLine("Animation frames after an endless repeat won''t be displayed: " + xmlp.getPositionDescription());
                }
                String tagName = xmlp.getName();
                parseAnimElements(xmlp, tagName, children);
                AnimatedImage.Element e = children[children.Count - 1];
                lastRepeatsEndless =
                        (e is AnimatedImage.Repeat) &&
                        ((AnimatedImage.Repeat)e).repeatCount == 0;
                xmlp.require(XmlPullParser.END_TAG, null, tagName);
                xmlp.nextTag();
            }
            return new AnimatedImage.Repeat(children.ToArray(), repeatCount);
        }

        private Border getBorder(AnimatedImage.Element e)
        {
            if (e is AnimatedImage.Repeat)
            {
                AnimatedImage.Repeat r = (AnimatedImage.Repeat)e;
                foreach (AnimatedImage.Element c in r.children)
                {
                    Border border = getBorder(c);
                    if (border != null)
                    {
                        return border;
                    }
                }
            }
            else if (e is AnimatedImage.Img)
            {
                AnimatedImage.Img i = (AnimatedImage.Img)e;
                if (i.image is HasBorder)
                {
                    return ((HasBorder)i.image).Border;
                }
            }
            return null;
        }

        private Image parseAnimation(XMLParser xmlp, ImageParams parameters)
        {
            try
            {
                String timeSource = xmlp.getAttributeNotNull("timeSource");
                int frozenTime = xmlp.parseIntFromAttribute("frozenTime", -1);
                AnimatedImage.Repeat root = parseAnimRepeat(xmlp);
                if (parameters.border == null)
                {
                    parameters.border = getBorder(root);
                }
                Image image = new AnimatedImage(renderer, root, timeSource, parameters.border,
                        (parameters.tintColor == null) ? Color.WHITE : parameters.tintColor, frozenTime);
                parameters.tintColor = null;
                return image;
            }
            catch (ArgumentException ex)
            {
                throw xmlp.error("Unable to parse", ex);
            }
        }

        private Image parseGradient(XMLParser xmlp, ImageParams parameters)
        {
            try
            {
                GradientType type = xmlp.parseEnumFromAttribute<GradientType>("type", typeof(GradientType));
                GradientWrap wrap = xmlp.parseEnumFromAttribute<GradientWrap>("wrap", typeof(GradientWrap), GradientWrap.SCALE);

                Gradient gradient = new Gradient(type);
                gradient.Wrap = wrap;

                xmlp.nextTag();
                while (xmlp.isStartTag())
                {
                    xmlp.require(XmlPullParser.START_TAG, null, "stop");
                    float pos = xmlp.parseFloatFromAttribute("pos");
                    Color color = ParserUtil.parseColor(xmlp, xmlp.getAttributeNotNull("color"), constants);
                    gradient.AddStop(pos, color);
                    xmlp.nextTag();
                    xmlp.require(XmlPullParser.END_TAG, null, "stop");
                    xmlp.nextTag();
                }

                return renderer.CreateGradient(gradient);
            }
            catch (ArgumentException ex)
            {
                throw xmlp.error("Unable to parse", ex);
            }
        }

        private Image createImage(XMLParser xmlp, int x, int y, int w, int h, Color tintColor, bool tiled, TextureRotation rotation)
        {
            if (w == 0 || h == 0)
            {
                return new EmptyImage(Math.Abs(w), Math.Abs(h));
            }

            Texture texture = currentTexture;
            int texWidth = texture.Width;
            int texHeight = texture.Height;

            int x1 = x + Math.Abs(w);
            int y1 = y + Math.Abs(h);

            if (x < 0 || x >= texWidth || x1 < 0 || x1 > texWidth ||
                    y < 0 || y >= texHeight || y1 < 0 || y1 > texHeight)
            {
                System.Diagnostics.Debug.WriteLine("texture partly outside of file: " + xmlp.getPositionDescription());
                x = Math.Max(0, Math.Min(x, texWidth));
                y = Math.Max(0, Math.Min(y, texHeight));
                w = Math.Sign(w) * (Math.Max(0, Math.Min(x1, texWidth)) - x);
                h = Math.Sign(h) * (Math.Max(0, Math.Min(y1, texHeight)) - y);
            }

            return texture.GetImage(x, y, w, h, tintColor, tiled, rotation);
        }

        private void parseRectFromAttribute(XMLParser xmlp, ImageParams parameters)
        {
            if (currentTexture == null)
            {
                throw xmlp.error("can't create area outside of <imagefile> object");
            }
            String xywh = xmlp.getAttributeNotNull("xywh");
            System.Diagnostics.Debug.WriteLine("xywh: " + xywh);
            if ("*".Equals(xywh))
            {
                parameters.x = 0;
                parameters.y = 0;
                parameters.w = currentTexture.Width;
                parameters.h = currentTexture.Height;
            }
            else try
                {
                    int[] coords = TextUtil.parseIntArray(xywh);
                    if (coords.Length != 4)
                    {
                        throw xmlp.error("xywh requires 4 integer arguments");
                    }
                    parameters.x = coords[0];
                    parameters.y = coords[1];
                    parameters.w = coords[2];
                    parameters.h = coords[3];
                }
                catch (ArgumentException ex)
                {
                    throw xmlp.error("can't parse xywh argument", ex);
                }
        }

        private void parseRotationFromAttribute(XMLParser xmlp, ImageParams parameters)
        {
            if (currentTexture == null)
            {
                throw xmlp.error("can't create area outside of <imagefile> object");
            }
            int rot = xmlp.parseIntFromAttribute("rot", 0);
            switch (rot)
            {
                case 0: parameters.rot = TextureRotation.NONE; break;
                case 90: parameters.rot = TextureRotation.CLOCKWISE_90; break;
                case 180: parameters.rot = TextureRotation.CLOCKWISE_180; break;
                case 270: parameters.rot = TextureRotation.CLOCKWISE_270; break;
                default:
                    throw xmlp.error("invalid rotation angle");
            }
        }

        private void parseStdAttributes(XMLParser xmlp, ImageParams parameters)
        {
            parameters.tintColor = ParserUtil.parseColorFromAttribute(xmlp, "tint", constants, null);
            parameters.border = ParserUtil.parseBorderFromAttribute(xmlp, "border");
            parameters.inset = ParserUtil.parseBorderFromAttribute(xmlp, "inset");
            parameters.repeatX = xmlp.parseBoolFromAttribute("repeatX", false);
            parameters.repeatY = xmlp.parseBoolFromAttribute("repeatY", false);
            parameters.sizeOverwriteH = ParserUtil.parseIntExpressionFromAttribute(xmlp, "sizeOverwriteH", -1, mathInterpreter);
            parameters.sizeOverwriteV = ParserUtil.parseIntExpressionFromAttribute(xmlp, "sizeOverwriteV", -1, mathInterpreter);
            parameters.center = xmlp.parseBoolFromAttribute("center", false);
        }

        public class ImageParams
        {
            public int x, y, w, h;
            public Color tintColor;
            public Border border;
            public Border inset;
            public bool repeatX;
            public bool repeatY;
            public int sizeOverwriteH = -1;
            public int sizeOverwriteV = -1;
            public bool center;
            public StateExpression condition;
            public TextureRotation rot;
        }

        public class AnimParams
        {
            public Color tintColor;
            public float zoomX;
            public float zoomY;
            public float zoomCenterX;
            public float zoomCenterY;
        }

        public class MathInterpreter : AbstractMathInterpreter
        {
            public ImageManager ImageManager;

            public MathInterpreter(ImageManager imageManager)
            {
                this.ImageManager = imageManager;
            }

            public override void accessVariable(String name)
            {
                Image img = this.ImageManager.getImage(name);
                if (img != null)
                {
                    push(img);
                    return;
                }
                Object obj = this.ImageManager.constants.getParam(name);
                if (obj != null)
                {
                    push(obj);
                    return;
                }
                throw new ArgumentOutOfRangeException("variable not found: " + name);
            }

            //@Override
            protected override Object accessField(Object obj, String field)
            {
                if (obj is ParameterMapImpl)
                {
                    Object result = ((ParameterMapImpl)obj).getParam(field);
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
                return base.accessField(obj, field);
            }
        }
    }
}
