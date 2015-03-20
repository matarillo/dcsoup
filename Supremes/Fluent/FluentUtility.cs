using Supremes.Nodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Supremes.Fluent
{
    /// <summary>
    /// Provides a set of static methods for jQuery-like method chainings.
    /// </summary>
    public static class FluentUtility
    {
        #region DataNode

        /// <summary>
        /// Set the data contents of the specified node, and returns the node itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="DataNode"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="data">unencoded data</param>
        /// <returns>The input <see cref="DataNode"/>, for method chaining.</returns>
        /// <seealso cref="DataNode.WholeData">DataNode.WholeData</seealso>
        public static DataNode WholeData(this DataNode self, string data)
        {
            self.WholeData = data;
            return self;
        }

        #endregion

        #region DocumentOutputSettings

        /// <summary>
        /// Set the current HTML escape mode of the specified document settings, and returns the document settings itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="DocumentOutputSettings"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="escapeMode">the new escape mode to use</param>
        /// <returns>The input <see cref="DocumentOutputSettings"/>, for method chaining.</returns>
        /// <seealso cref="DocumentOutputSettings.EscapeMode">DocumentOutputSettings.EscapeMode</seealso>
        public static DocumentOutputSettings EscapeMode(this DocumentOutputSettings self, DocumentEscapeMode escapeMode)
        {
            self.EscapeMode = escapeMode;
            return self;
        }

        /// <summary>
        /// Set the current output charset of the specified document settings, and returns the document settings itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="DocumentOutputSettings"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="charset">the new charset to use</param>
        /// <returns>The input <see cref="DocumentOutputSettings"/>, for method chaining.</returns>
        /// <seealso cref="DocumentOutputSettings.Charset">DocumentOutputSettings.Charset</seealso>
        public static DocumentOutputSettings Charset(this DocumentOutputSettings self, Encoding charset)
        {
            self.Charset = charset;
            return self;
        }

        /// <summary>
        /// Set the current output charset of the specified document settings, and returns the document settings itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="DocumentOutputSettings"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="charset">the new charset name to use</param>
        /// <returns>The input <see cref="DocumentOutputSettings"/>, for method chaining.</returns>
        /// <seealso cref="DocumentOutputSettings.Charset">DocumentOutputSettings.Charset</seealso>
        public static DocumentOutputSettings Charset(this DocumentOutputSettings self, String charset)
        {
            self.Charset = Encoding.GetEncoding(charset);
            return self;
        }

        /// <summary>
        /// Set the current output syntax of the specified document settings, and returns the document settings itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="DocumentOutputSettings"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="syntax">the new syntax to use</param>
        /// <returns>The input <see cref="DocumentOutputSettings"/>, for method chaining.</returns>
        /// <seealso cref="DocumentOutputSettings.Syntax">DocumentOutputSettings.Syntax</seealso>
        public static DocumentOutputSettings Syntax(this DocumentOutputSettings self, DocumentSyntax syntax)
        {
            self.Syntax = syntax;
            return self;
        }

        /// <summary>
        /// Set if pretty printing is enabled to the specified document settings, and returns the document settings itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="DocumentOutputSettings"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="pretty">the new pretty print setting</param>
        /// <returns>The input <see cref="DocumentOutputSettings"/>, for method chaining.</returns>
        /// <seealso cref="DocumentOutputSettings.PrettyPrint">DocumentOutputSettings.PrettyPrint</seealso>
        public static DocumentOutputSettings PrettyPrint(this DocumentOutputSettings self, bool pretty)
        {
            self.PrettyPrint = pretty;
            return self;
        }

        /// <summary>
        /// Set if outline mode is enabled to the specified document settings, and returns the document settings itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="DocumentOutputSettings"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="outlineMode">the new outline mode</param>
        /// <returns>The input <see cref="DocumentOutputSettings"/>, for method chaining.</returns>
        /// <seealso cref="DocumentOutputSettings.Outline">DocumentOutputSettings.Outline</seealso>
        public static DocumentOutputSettings Outline(this DocumentOutputSettings self, bool outlineMode)
        {
            self.Outline = outlineMode;
            return self;
        }

        /// <summary>
        /// Set the current tag indent amount of the specified document settings, used when pretty printing, and returns the document settings itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="DocumentOutputSettings"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="indentAmount">number of spaces to use for indenting each level</param>
        /// <returns>The input <see cref="DocumentOutputSettings"/>, for method chaining.</returns>
        /// <seealso cref="DocumentOutputSettings.IndentAmount">DocumentOutputSettings.IndentAmount</seealso>
        public static DocumentOutputSettings IndentAmount(this DocumentOutputSettings self, int indentAmount)
        {
            self.IndentAmount = indentAmount;
            return self;
        }

        #endregion

        #region Document

        /// <summary>
        /// Set the combined text of the specified document and all its children, and returns the document itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="Document"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="text">unencoded text</param>
        /// <returns>The input <see cref="Document"/>, for method chaining.</returns>
        /// <seealso cref="Document.Text">Document.Text</seealso>
        public static Document Text(this Document self, string text)
        {
            self.Text = text;
            return self;
        }

        /// <summary>
        /// Set the specified document's current output settings, and returns the document itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="Document"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="outputSettings">new output settings</param>
        /// <returns>The input <see cref="Document"/>, for method chaining.</returns>
        /// <seealso cref="Document.OutputSettings">Document.OutputSettings</seealso>
        public static Document OutputSettings(this Document self, DocumentOutputSettings outputSettings)
        {
            self.OutputSettings = outputSettings;
            return self;
        }

        /// <summary>
        /// Set the specified document's quirks mode, and returns the document itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="Document"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="quirksMode">new quirks mode</param>
        /// <returns>The input <see cref="Document"/>, for method chaining.</returns>
        /// <seealso cref="Document.QuirksMode">Document.QuirksMode</seealso>
        public static Document QuirksMode(this Document self, DocumentQuirksMode quirksMode)
        {
            self.QuirksMode = quirksMode;
            return self;
        }

        #endregion

        #region Element

        /// <summary>
        /// Set the name of the tag for the specified element, and returns the element itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="Element"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="tagName">the new tag name</param>
        /// <returns>The input <see cref="Element"/>, for method chaining.</returns>
        /// <seealso cref="Element.TagName">Element.TagName</seealso>
        public static Element TagName(this Element self, string tagName)
        {
            self.TagName = tagName;
            return self;
        }

        /// <summary>
        /// Set all of the specified element's class names, and returns the element itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="Element"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="classNames">the new set of classes</param>
        /// <returns>The input <see cref="Element"/>, for method chaining.</returns>
        /// <seealso cref="Element.ClassNames">Element.ClassNames</seealso>
        public static Element ClassNames(this Element self, ICollection<string> classNames)
        {
            self.ClassNames = classNames;
            return self;
        }

        /// <summary>
        /// Set the value of the specified form element (input, textarea, etc), and returns the element itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="Element"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="value">the new value to set</param>
        /// <returns>The input <see cref="Element"/>, for method chaining.</returns>
        /// <seealso cref="Element.Val">Element.Val</seealso>
        public static Element Val(this Element self, string value)
        {
            self.Val = value;
            return self;
        }

        /// <summary>
        /// Set the specified element's inner HTML, and returns the element itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="Element"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="html">HTML to parse and set into this element</param>
        /// <returns>The input <see cref="Element"/>, for method chaining.</returns>
        /// <seealso cref="Element.Html">Element.Html</seealso>
        public static Element Html(this Element self, string html)
        {
            self.Html = html;
            return self;
        }

        #endregion

        #region Elements

        /// <summary>
        /// Set the form element's value in each of the matched elements, and returns the elements itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="Elements"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="value">the new value to set into each matched element</param>
        /// <returns>The input <see cref="Elements"/>, for method chaining.</returns>
        /// <seealso cref="Elements.Val">Elements.Val</seealso>
        public static Elements Val(this Elements self, string value)
        {
            self.Val = value;
            return self;
        }

        /// <summary>
        /// Set the tag name of each matched element, and returns the elements itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="Elements"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="tagName">the new tag name to set into each matched element</param>
        /// <returns>The input <see cref="Elements"/>, for method chaining.</returns>
        /// <seealso cref="Elements.TagName">Elements.TagName</seealso>
        public static Elements TagName(this Elements self, string tagName)
        {
            self.TagName = tagName;
            return self;
        }

        /// <summary>
        /// Set the inner HTML of each matched element, and returns the elements itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="Elements"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="html">HTML to parse and set into each matched element</param>
        /// <returns>The input <see cref="Elements"/>, for method chaining.</returns>
        /// <seealso cref="Elements.Html">Elements.Html</seealso>
        public static Elements Html(this Elements self, string html)
        {
            self.Html = html;
            return self;
        }

        #endregion

        #region TextNode

        /// <summary>
        /// Set text content of the specified text node, and returns the text node itself.
        /// </summary>
        /// <param name="self">
        /// The input <see cref="TextNode"/>,
        /// which acts as the <b>this</b> instance for the extension method.
        /// </param>
        /// <param name="text">unencoded text</param>
        /// <returns>The input <see cref="TextNode"/>, for method chaining.</returns>
        /// <seealso cref="TextNode.Text">TextNode.Text</seealso>
        public static TextNode Text(this TextNode self, string text)
        {
            self.Text = text;
            return self;
        }

        #endregion
    }
}
