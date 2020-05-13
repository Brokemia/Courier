﻿#region LICENSE

/**
 * MIT License
 * 
 * Copyright (c) 2019 Denis Zhidkikh
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tommy
{
    #region TOML Nodes

    public abstract class TomlNode : IEnumerable
    {
        public virtual bool HasValue { get; } = false;
        public virtual bool IsArray { get; } = false;
        public virtual bool IsTable { get; } = false;
        public virtual bool IsString { get; } = false;
        public virtual bool IsInteger { get; } = false;
        public virtual bool IsFloat { get; } = false;
        public virtual bool IsDateTime { get; } = false;
        public virtual bool IsBoolean { get; } = false;
        public virtual string Comment { get; set; }
        public virtual int CollapseLevel { get; set; } = 0;

        public virtual TomlTable AsTable => this as TomlTable;
        public virtual TomlString AsString => this as TomlString;
        public virtual TomlInteger AsInteger => this as TomlInteger;
        public virtual TomlFloat AsFloat => this as TomlFloat;
        public virtual TomlBoolean AsBoolean => this as TomlBoolean;
        public virtual TomlDateTime AsDateTime => this as TomlDateTime;
        public virtual TomlArray AsArray => this as TomlArray;

        public virtual int ChildrenCount => 0;

        public virtual TomlNode this[string key]
        {
            get => null;
            set { }
        }

        public virtual TomlNode this[int index]
        {
            get => null;
            set { }
        }

        public virtual IEnumerable<TomlNode> Children
        {
            get { yield break; }
        }

        public virtual IEnumerable<string> Keys
        {
            get { yield break; }
        }

        public IEnumerator GetEnumerator() => Children.GetEnumerator();

        public virtual bool TryGetNode(string key, out TomlNode node)
        {
            node = null;
            return false;
        }

        public virtual bool HasKey(string key) => false;

        public virtual bool HasItemAt(int index) => false;

        public virtual void Add(string key, TomlNode node) { }

        public virtual void Add(TomlNode node) { }

        public virtual void Delete(TomlNode node) { }

        public virtual void Delete(string key) { }

        public virtual void Delete(int index) { }

        public virtual void AddRange(IEnumerable<TomlNode> nodes)
        {
            foreach (var tomlNode in nodes) Add(tomlNode);
        }

        public virtual void ToTomlString(TextWriter tw, string name = null) { }

        #region Native type to TOML cast

        public static implicit operator TomlNode(string value) =>
            new TomlString
            {
                Value = value
            };

        public static implicit operator TomlNode(bool value) =>
            new TomlBoolean
            {
                Value = value
            };

        public static implicit operator TomlNode(long value) =>
            new TomlInteger
            {
                Value = value
            };

        public static implicit operator TomlNode(float value) =>
            new TomlFloat
            {
                Value = value
            };

        public static implicit operator TomlNode(double value) =>
            new TomlFloat
            {
                Value = value
            };

        public static implicit operator TomlNode(DateTime value) =>
            new TomlDateTime
            {
                Value = value
            };

        public static implicit operator TomlNode(TomlNode[] nodes)
        {
            var result = new TomlArray();
            result.AddRange(nodes);
            return result;
        }

        #endregion

        #region TOML to native type cast

        public static implicit operator string(TomlNode value) => value.ToString();

        public static implicit operator int(TomlNode value) => (int) value.AsInteger.Value;

        public static implicit operator long(TomlNode value) => value.AsInteger.Value;

        public static implicit operator float(TomlNode value) => (float) value.AsFloat.Value;

        public static implicit operator double(TomlNode value) => value.AsFloat.Value;

        public static implicit operator bool(TomlNode value) => value.AsBoolean.Value;

        public static implicit operator DateTime(TomlNode value) => value.AsDateTime.Value;

        #endregion
    }

    public class TomlString : TomlNode
    {
        public override bool HasValue { get; } = true;
        public override bool IsString { get; } = true;
        public bool IsMultiline { get; set; }
        public bool PreferLiteral { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            if (Value.IndexOf(TomlSyntax.LITERAL_STRING_SYMBOL) != -1 && PreferLiteral) PreferLiteral = false;

            var quotes = new string(PreferLiteral ? TomlSyntax.LITERAL_STRING_SYMBOL : TomlSyntax.BASIC_STRING_SYMBOL,
                                    IsMultiline ? 3 : 1);
            var result = PreferLiteral ? Value : Value.Escape(!IsMultiline);

            return $"{quotes}{result}{quotes}";
        }

        public override void ToTomlString(TextWriter tw, string name = null)
        {
            tw.Write(ToString());
        }
    }

    public class TomlInteger : TomlNode
    {
        public enum Base
        {
            Binary = 2,
            Octal = 8,
            Decimal = 10,
            Hexadecimal = 16
        }

        public override bool IsInteger { get; } = true;
        public override bool HasValue { get; } = true;
        public Base IntegerBase { get; set; } = Base.Decimal;

        public long Value { get; set; }

        public override string ToString()
        {
            if (IntegerBase != Base.Decimal)
                return $"0{TomlSyntax.BaseIdentifiers[(int) IntegerBase]}{Convert.ToString(Value, (int) IntegerBase)}";
            return Value.ToString(CultureInfo.InvariantCulture);
        }

        public override void ToTomlString(TextWriter tw, string name = null) => tw.Write(ToString());
    }

    public class TomlFloat : TomlNode
    {
        public override bool IsFloat { get; } = true;
        public override bool HasValue { get; } = true;

        public double Value { get; set; }

        public override string ToString()
        {
            if (double.IsNaN(Value)) return TomlSyntax.NAN_VALUE;

            if (double.IsPositiveInfinity(Value)) return TomlSyntax.INF_VALUE;

            if (double.IsNegativeInfinity(Value)) return TomlSyntax.NEG_INF_VALUE;

            return Value.ToString("G", CultureInfo.InvariantCulture);
        }

        public override void ToTomlString(TextWriter tw, string name = null) => tw.Write(ToString());
    }

    public class TomlBoolean : TomlNode
    {
        public override bool IsBoolean { get; } = true;
        public override bool HasValue { get; } = true;

        public bool Value { get; set; }

        public override string ToString() => Value ? TomlSyntax.TRUE_VALUE : TomlSyntax.FALSE_VALUE;

        public override void ToTomlString(TextWriter tw, string name = null) => tw.Write(ToString());
    }

    public class TomlDateTime : TomlNode
    {
        public override bool IsDateTime { get; } = true;
        public override bool HasValue { get; } = true;
        public bool OnlyDate { get; set; }
        public bool OnlyTime { get; set; }
        public int SecondsPrecision { get; set; }

        public DateTime Value { get; set; }

        public override string ToString()
        {
            if (OnlyDate) return Value.ToString(TomlSyntax.LocalDateFormat);
            if (OnlyTime) return Value.ToString(TomlSyntax.RFC3339LocalTimeFormats[SecondsPrecision]);
            if (Value.Kind == DateTimeKind.Local)
                return Value.ToString(TomlSyntax.RFC3339LocalDateTimeFormats[SecondsPrecision]);
            return Value.ToString(TomlSyntax.RFC3339Formats[SecondsPrecision]);
        }

        public override void ToTomlString(TextWriter tw, string name = null) => tw.Write(ToString());
    }

    public class TomlArray : TomlNode
    {
        private List<TomlNode> values;

        public override bool HasValue { get; } = true;
        public override bool IsArray { get; } = true;
        public bool IsTableArray { get; set; }
        public List<TomlNode> RawArray => values ?? (values = new List<TomlNode>());

        public override TomlNode this[int index]
        {
            get
            {
                if (index < RawArray.Count) return RawArray[index];
                var lazy = new TomlLazy(this);
                this[index] = lazy;
                return lazy;
            }
            set
            {
                if (index == RawArray.Count)
                    RawArray.Add(value);
                else
                    RawArray[index] = value;
            }
        }

        public override int ChildrenCount => RawArray.Count;

        public override IEnumerable<TomlNode> Children => RawArray.AsEnumerable();

        public override void Add(TomlNode node) => RawArray.Add(node);

        public override void AddRange(IEnumerable<TomlNode> nodes) => RawArray.AddRange(nodes);

        public override void Delete(TomlNode node) => RawArray.Remove(node);

        public override void Delete(int index) => RawArray.RemoveAt(index);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(TomlSyntax.ARRAY_START_SYMBOL);

            if (ChildrenCount != 0)
                sb.Append(' ')
                  .Append($"{TomlSyntax.ITEM_SEPARATOR} ".Join(RawArray.Select(n => n.ToString())))
                  .Append(' ');

            sb.Append(TomlSyntax.ARRAY_END_SYMBOL);
            return sb.ToString();
        }

        public override void ToTomlString(TextWriter tw, string name = null)
        {
            // If it's a normal array, write it as usual
            if (!IsTableArray)
            {
                tw.Write(ToString());
                return;
            }

            tw.WriteLine();

            Comment?.AsComment(tw);
            tw.Write(TomlSyntax.ARRAY_START_SYMBOL);
            tw.Write(TomlSyntax.ARRAY_START_SYMBOL);
            tw.Write(name);
            tw.Write(TomlSyntax.ARRAY_END_SYMBOL);
            tw.Write(TomlSyntax.ARRAY_END_SYMBOL);
            tw.WriteLine();

            var first = true;

            foreach (var tomlNode in RawArray)
            {
                if (!(tomlNode is TomlTable tbl))
                    throw new TomlFormatException("The array is marked as array table but contains non-table nodes!");

                // Ensure it's parsed as a section
                tbl.IsInline = false;

                if (!first)
                {
                    tw.WriteLine();

                    Comment?.AsComment(tw);
                    tw.Write(TomlSyntax.ARRAY_START_SYMBOL);
                    tw.Write(TomlSyntax.ARRAY_START_SYMBOL);
                    tw.Write(name);
                    tw.Write(TomlSyntax.ARRAY_END_SYMBOL);
                    tw.Write(TomlSyntax.ARRAY_END_SYMBOL);
                    tw.WriteLine();
                }

                first = false;

                // Don't pass section name because we already specified it
                tbl.ToTomlString(tw);

                tw.WriteLine();
            }
        }
    }

    public class TomlTable : TomlNode
    {
        private Dictionary<string, TomlNode> children;

        public override bool HasValue { get; } = false;
        public override bool IsTable { get; } = true;
        public bool IsInline { get; set; }
        public Dictionary<string, TomlNode> RawTable => children ?? (children = new Dictionary<string, TomlNode>());

        public override TomlNode this[string key]
        {
            get
            {
                if (RawTable.TryGetValue(key, out var result)) return result;

                var lazy = new TomlLazy(this);
                RawTable[key] = lazy;
                return lazy;
            }
            set => RawTable[key] = value;
        }

        public override int ChildrenCount => RawTable.Count;

        public override IEnumerable<TomlNode> Children => RawTable.Select(kv => kv.Value);

        public override IEnumerable<string> Keys => RawTable.Select(kv => kv.Key);

        public override bool HasKey(string key) => RawTable.ContainsKey(key);

        public override void Add(string key, TomlNode node) => RawTable.Add(key, node);

        public override bool TryGetNode(string key, out TomlNode node) => RawTable.TryGetValue(key, out node);

        public override void Delete(TomlNode node) => RawTable.Remove(RawTable.First(kv => kv.Value == node).Key);

        public override void Delete(string key) => RawTable.Remove(key);

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(TomlSyntax.INLINE_TABLE_START_SYMBOL);

            if (ChildrenCount != 0)
            {
                var collapsed = CollectCollapsedItems(out var nonCollapsible);

                sb.Append(' ');
                sb.Append($"{TomlSyntax.ITEM_SEPARATOR} ".Join(RawTable.Where(n => nonCollapsible.Contains(n.Key))
                                                                       .Select(n => $"{n.Key.AsKey()} {TomlSyntax.KEY_VALUE_SEPARATOR} {n.Value.ToString()}")));

                if (collapsed.Count != 0)
                    sb.Append(TomlSyntax.ITEM_SEPARATOR)
                      .Append(' ')
                      .Append($"{TomlSyntax.ITEM_SEPARATOR} ".Join(collapsed.Select(n => $"{n.Key} {TomlSyntax.KEY_VALUE_SEPARATOR} {n.Value.ToString()}")));

                sb.Append(' ');
            }

            sb.Append(TomlSyntax.INLINE_TABLE_END_SYMBOL);
            return sb.ToString();
        }

        private Dictionary<string, TomlNode> CollectCollapsedItems(out HashSet<string> nonCollapsibleItems, string prefix = "",
                                                                   Dictionary<string, TomlNode> nodes = null,
                                                                   int level = 0)
        {
            nonCollapsibleItems = new HashSet<string>();
            if (nodes == null)
            {
                nodes = new Dictionary<string, TomlNode>();
                foreach (var keyValuePair in RawTable)
                {
                    var node = keyValuePair.Value;
                    var key = keyValuePair.Key.AsKey();
                    if (node is TomlTable tbl)
                    {
                        tbl.CollectCollapsedItems(out var nonCollapsible, $"{prefix}{key}.", nodes, level + 1);
                        if (nonCollapsible.Count != 0)
                            nonCollapsibleItems.Add(key);
                    }
                    else
                        nonCollapsibleItems.Add(key);
                }

                return nodes;
            }

            foreach (var keyValuePair in RawTable)
            {
                var node = keyValuePair.Value;
                var key = keyValuePair.Key.AsKey();

                if (node.CollapseLevel == level)
                    nodes.Add($"{prefix}{key}", node);
                else if (node is TomlTable tbl)
                {
                    tbl.CollectCollapsedItems(out var nonCollapsible, $"{prefix}{key}.", nodes, level + 1);
                    if (nonCollapsible.Count != 0)
                        nonCollapsibleItems.Add(key);
                }
                else
                    nonCollapsibleItems.Add(key);
            }

            return nodes;
        }

        public override void ToTomlString(TextWriter tw, string name = null)
        {
            // The table is inline table
            if (IsInline && name != null)
            {
                tw.Write(ToString());
                return;
            }

            if (RawTable.All(n => n.Value.CollapseLevel != 0))
                return;

            var hasRealValues = !RawTable.All(n => n.Value is TomlTable tbl && !tbl.IsInline);

            var collapsedItems = CollectCollapsedItems(out var _);

            Comment?.AsComment(tw);

            if (name != null && (hasRealValues || collapsedItems.Count > 0))
            {
                tw.Write(TomlSyntax.ARRAY_START_SYMBOL);
                tw.Write(name);
                tw.Write(TomlSyntax.ARRAY_END_SYMBOL);
                tw.WriteLine();
            }
            else if (Comment != null) // Add some spacing between the first node and the comment
            {
                tw.WriteLine();
            }

            var namePrefix = name == null ? "" : $"{name}.";
            var first = true;

            var sectionableItems = new Dictionary<string, TomlNode>();

            foreach (var child in RawTable)
            {
                // If value should be parsed as section, separate if from the bunch
                if (child.Value is TomlArray arr && arr.IsTableArray || child.Value is TomlTable tbl && !tbl.IsInline)
                {
                    sectionableItems.Add(child.Key, child.Value);
                    continue;
                }

                // If the value is collapsed, it belongs to the parent
                if (child.Value.CollapseLevel != 0)
                    continue;

                if (!first) tw.WriteLine();
                first = false;

                var key = child.Key.AsKey();
                child.Value.Comment?.AsComment(tw);
                tw.Write(key);
                tw.Write(' ');
                tw.Write(TomlSyntax.KEY_VALUE_SEPARATOR);
                tw.Write(' ');

                child.Value.ToTomlString(tw, $"{namePrefix}{key}");
            }

            foreach (var collapsedItem in collapsedItems)
            {
                if (collapsedItem.Value is TomlArray arr && arr.IsTableArray ||
                    collapsedItem.Value is TomlTable tbl && !tbl.IsInline)
                    throw new
                        TomlFormatException($"Value {collapsedItem.Key} cannot be defined as collapsed, because it is not an inline value!");

                tw.WriteLine();
                var key = collapsedItem.Key;
                collapsedItem.Value.Comment?.AsComment(tw);
                tw.Write(key);
                tw.Write(' ');
                tw.Write(TomlSyntax.KEY_VALUE_SEPARATOR);
                tw.Write(' ');

                collapsedItem.Value.ToTomlString(tw, $"{namePrefix}{key}");
            }

            if (sectionableItems.Count == 0)
                return;

            tw.WriteLine();
            tw.WriteLine();
            first = true;
            foreach (var child in sectionableItems)
            {
                if (!first) tw.WriteLine();
                first = false;

                child.Value.ToTomlString(tw, $"{namePrefix}{child.Key}");
            }
        }
    }

    internal class TomlLazy : TomlNode
    {
        private readonly TomlNode parent;
        private TomlNode replacement;

        public TomlLazy(TomlNode parent) => this.parent = parent;

        public override TomlNode this[int index]
        {
            get => Set<TomlArray>()[index];
            set => Set<TomlArray>()[index] = value;
        }

        public override TomlNode this[string key]
        {
            get => Set<TomlTable>()[key];
            set => Set<TomlTable>()[key] = value;
        }

        public override void Add(TomlNode node) => Set<TomlArray>().Add(node);

        public override void Add(string key, TomlNode node) => Set<TomlTable>().Add(key, node);

        public override void AddRange(IEnumerable<TomlNode> nodes) => Set<TomlArray>().AddRange(nodes);

        private TomlNode Set<T>() where T : TomlNode, new()
        {
            if (replacement != null) return replacement;

            var newNode = new T
            {
                Comment = Comment
            };

            if (parent.IsTable)
            {
                var key = parent.Keys.FirstOrDefault(s => parent.TryGetNode(s, out var node) && node.Equals(this));
                if (key == null) return default(T);

                parent[key] = newNode;
            }
            else if (parent.IsArray)
            {
                var index = 0;
                foreach (var child in parent.Children)
                {
                    if (child == this) break;
                    index++;
                }

                if (index == parent.ChildrenCount) return default(T);

                parent[index] = newNode;
            }
            else
            {
                return default(T);
            }

            replacement = newNode;
            return newNode;
        }
    }

    #endregion

    #region Parser

    public class TOMLParser : IDisposable
    {
        public enum ParseState
        {
            None,
            KeyValuePair,
            SkipToNextLine,
            Table
        }

        private readonly TextReader reader;

        private ParseState currentState;
        private int line, col;
        private List<TomlSyntaxException> syntaxErrors;

        public TOMLParser(TextReader reader)
        {
            this.reader = reader;
            line = col = 0;
        }

        public bool ForceASCII { get; set; }

        public void Dispose()
        {
            reader?.Dispose();
        }

        public TomlTable Parse()
        {
            syntaxErrors = new List<TomlSyntaxException>();
            line = col = 0;
            var rootNode = new TomlTable();
            var currentNode = rootNode;
            currentState = ParseState.None;
            var keyParts = new List<string>();
            var arrayTable = false;
            var latestComment = new StringBuilder();
            var firstComment = true;

            int currentChar;
            while ((currentChar = reader.Peek()) >= 0)
            {
                var c = (char) currentChar;

                if (currentState == ParseState.None)
                {
                    // Skip white space
                    if (TomlSyntax.IsWhiteSpace(c)) goto consume_character;

                    if (TomlSyntax.IsNewLine(c))
                    {
                        // Check if there are any comments and so far no items being declared
                        if (latestComment.Length != 0 && firstComment)
                        {
                            rootNode.Comment = latestComment.ToString().TrimEnd();
                            latestComment.Length = 0;
                            firstComment = false;
                        }

                        if (TomlSyntax.IsLineBreak(c))
                            AdvanceLine();

                        goto consume_character;
                    }

                    // Start of a comment; ignore until newline
                    if (c == TomlSyntax.COMMENT_SYMBOL)
                    {
                        // Consume the comment symbol and buffer the whole comment line
                        reader.Read();
                        latestComment.AppendLine(reader.ReadLine()?.Trim());
                        AdvanceLine(0);
                        continue;
                    }

                    // Encountered a non-comment value. The comment must belong to it (ignore possible newlines)!
                    firstComment = false;

                    if (c == TomlSyntax.TABLE_START_SYMBOL)
                    {
                        currentState = ParseState.Table;
                        goto consume_character;
                    }

                    if (TomlSyntax.IsBareKey(c) || TomlSyntax.IsQuoted(c))
                    {
                        currentState = ParseState.KeyValuePair;
                    }
                    else
                    {
                        AddError($"Unexpected character \"{c}\"");
                        continue;
                    }
                }

                if (currentState == ParseState.KeyValuePair)
                {
                    var keyValuePair = ReadKeyValuePair(keyParts);

                    if (keyValuePair == null)
                    {
                        latestComment.Length = 0;
                        keyParts.Clear();

                        if (currentState != ParseState.None)
                            AddError("Failed to parse key-value pair!");
                        continue;
                    }

                    keyValuePair.Comment = latestComment.ToString().TrimEnd();
                    var inserted = InsertNode(keyValuePair, currentNode, keyParts);
                    latestComment.Length = 0;
                    keyParts.Clear();
                    if (inserted)
                        currentState = ParseState.SkipToNextLine;
                    continue;
                }

                if (currentState == ParseState.Table)
                {
                    if (keyParts.Count == 0)
                    {
                        // We have array table
                        if (c == TomlSyntax.TABLE_START_SYMBOL)
                        {
                            // Consume the character
                            ConsumeChar();
                            arrayTable = true;
                        }

                        if (!ReadKeyName(ref keyParts, TomlSyntax.TABLE_END_SYMBOL, true))
                        {
                            keyParts.Clear();
                            continue;
                        }

                        if (keyParts.Count == 0)
                        {
                            AddError("Table name is emtpy.");
                            arrayTable = false;
                            latestComment.Length = 0;
                            keyParts.Clear();
                        }

                        continue;
                    }

                    if (c == TomlSyntax.TABLE_END_SYMBOL)
                    {
                        if (arrayTable)
                        {
                            // Consume the ending bracket so we can peek the next character
                            ConsumeChar();
                            var nextChar = reader.Peek();
                            if (nextChar < 0 || (char) nextChar != TomlSyntax.TABLE_END_SYMBOL)
                            {
                                AddError($"Array table {".".Join(keyParts)} has only one closing bracket.");
                                keyParts.Clear();
                                arrayTable = false;
                                latestComment.Length = 0;
                                continue;
                            }
                        }

                        currentNode = CreateTable(rootNode, keyParts, arrayTable);
                        if (currentNode != null)
                        {
                            currentNode.IsInline = false;
                            currentNode.Comment = latestComment.ToString().TrimEnd();
                        }

                        keyParts.Clear();
                        arrayTable = false;
                        latestComment.Length = 0;

                        if (currentNode == null)
                        {
                            if (currentState != ParseState.None)
                                AddError("Error creating table array!");
                            continue;
                        }

                        currentState = ParseState.SkipToNextLine;
                        goto consume_character;
                    }

                    if (keyParts.Count != 0)
                    {
                        AddError($"Unexpected character \"{c}\"");
                        keyParts.Clear();
                        arrayTable = false;
                        latestComment.Length = 0;
                    }
                }

                if (currentState == ParseState.SkipToNextLine)
                {
                    if (TomlSyntax.IsWhiteSpace(c) || c == TomlSyntax.NEWLINE_CARRIAGE_RETURN_CHARACTER)
                        goto consume_character;

                    if (c == TomlSyntax.COMMENT_SYMBOL || c == TomlSyntax.NEWLINE_CHARACTER)
                    {
                        currentState = ParseState.None;
                        AdvanceLine();

                        if (c == TomlSyntax.COMMENT_SYMBOL)
                        {
                            col++;
                            reader.ReadLine();
                            continue;
                        }

                        goto consume_character;
                    }

                    AddError($"Unexpected character \"{c}\" at the end of the line.");
                }

                consume_character:
                reader.Read();
                col++;
            }

            if (currentState != ParseState.None && currentState != ParseState.SkipToNextLine)
                AddError("Unexpected end of file!");

            if (syntaxErrors.Count > 0)
                throw new TomlParseException(rootNode, syntaxErrors);

            return rootNode;
        }

        private bool AddError(string message)
        {
            syntaxErrors.Add(new TomlSyntaxException(message, currentState, line, col));

            // Skip the whole line in hope that it was only a single faulty value (and non-multiline one at that)
            reader.ReadLine();
            AdvanceLine(0);
            currentState = ParseState.None;
            return false;
        }

        private void AdvanceLine(int startCol = -1)
        {
            line++;
            col = startCol;
        }

        private int ConsumeChar()
        {
            col++;
            return reader.Read();
        }

        #region Key-Value pair parsing

        /**
         * Reads a single key-value pair.
         * Assumes the cursor is at the first character that belong to the pair (including possible whitespace).
         * Consumes all characters that belong to the key and the value (ignoring possible trailing whitespace at the end).
         *
         * Example:
         * foo = "bar"  ==> foo = "bar"
         * ^                           ^
         */

        private TomlNode ReadKeyValuePair(List<string> keyParts)
        {
            int cur;
            while ((cur = reader.Peek()) >= 0)
            {
                var c = (char) cur;

                if (TomlSyntax.IsQuoted(c) || TomlSyntax.IsBareKey(c))
                {
                    if (keyParts.Count != 0)
                    {
                        AddError("Encountered extra characters in key definition!");
                        return null;
                    }

                    if (!ReadKeyName(ref keyParts, TomlSyntax.KEY_VALUE_SEPARATOR))
                        return null;

                    continue;
                }

                if (TomlSyntax.IsWhiteSpace(c))
                {
                    ConsumeChar();
                    continue;
                }

                if (c == TomlSyntax.KEY_VALUE_SEPARATOR)
                {
                    ConsumeChar();
                    return ReadValue();
                }

                AddError($"Unexpected character \"{c}\" in key name.");
                return null;
            }

            return null;
        }

        /**
         * Reads a single value.
         * Assumes the cursor is at the first character that belongs to the value (including possible starting whitespace).
         * Consumes all characters belonging to the value (ignoring possible trailing whitespace at the end).
         *
         * Example:
         * "test"  ==> "test"
         * ^                 ^
         */

        private TomlNode ReadValue(bool skipNewlines = false)
        {
            int cur;
            while ((cur = reader.Peek()) >= 0)
            {
                var c = (char) cur;

                if (TomlSyntax.IsWhiteSpace(c))
                {
                    ConsumeChar();
                    continue;
                }

                if (c == TomlSyntax.COMMENT_SYMBOL)
                {
                    AddError("No value found!");
                    return null;
                }

                if (TomlSyntax.IsNewLine(c))
                {
                    if (skipNewlines)
                    {
                        reader.Read();
                        AdvanceLine(0);
                        continue;
                    }

                    AddError("Encountered a newline when expecting a value!");
                    return null;
                }

                if (TomlSyntax.IsQuoted(c))
                {
                    var isMultiline = IsTripleQuote(c, out var excess);

                    // Error occurred in triple quote parsing
                    if (currentState == ParseState.None)
                        return null;

                    var value = isMultiline
                        ? ReadQuotedValueMultiLine(c)
                        : ReadQuotedValueSingleLine(c, excess);

                    return new TomlString
                    {
                        Value = value,
                        IsMultiline = isMultiline,
                        PreferLiteral = c == TomlSyntax.LITERAL_STRING_SYMBOL
                    };
                }

                if (c == TomlSyntax.INLINE_TABLE_START_SYMBOL) return ReadInlineTable();

                if (c == TomlSyntax.ARRAY_START_SYMBOL) return ReadArray();

                return ReadTomlValue();
            }

            return null;
        }

        /**
         * Reads a single key name.
         * Assumes the cursor is at the first character belonging to the key (with possible trailing whitespace if `skipWhitespace = true`).
         * Consumes all the characters until the `until` character is met (but does not consume the character itself).
         *
         * Example 1:
         * foo.bar  ==>  foo.bar           (`skipWhitespace = false`, `until = ' '`)
         * ^                    ^
         *
         * Example 2:
         * [ foo . bar ] ==>  [ foo . bar ]     (`skipWhitespace = true`, `until = ']'`)
         *  ^                             ^        
         */

        private bool ReadKeyName(ref List<string> parts, char until, bool skipWhitespace = false)
        {
            var buffer = new StringBuilder();
            var quoted = false;
            var prevWasSpace = false;
            int cur;
            while ((cur = reader.Peek()) >= 0)
            {
                var c = (char) cur;

                // Reached the final character
                if (c == until) break;

                if (TomlSyntax.IsWhiteSpace(c))
                    if (skipWhitespace)
                    {
                        prevWasSpace = true;
                        goto consume_character;
                    }
                    else
                    {
                        break;
                    }

                if (buffer.Length == 0) prevWasSpace = false;

                if (c == TomlSyntax.SUBKEY_SEPARATOR)
                {
                    if (buffer.Length == 0)
                        return AddError($"Found an extra subkey separator in {".".Join(parts)}...");

                    parts.Add(buffer.ToString());
                    buffer.Length = 0;
                    quoted = false;
                    prevWasSpace = false;
                    goto consume_character;
                }

                if (prevWasSpace)
                    return AddError("Invalid spacing in key name");

                if (TomlSyntax.IsQuoted(c))
                {
                    if (quoted)

                        return AddError("Expected a subkey separator but got extra data instead!");

                    if (buffer.Length != 0)
                        return AddError("Encountered a quote in the middle of subkey name!");

                    // Consume the quote character and read the key name
                    col++;
                    buffer.Append(ReadQuotedValueSingleLine((char) reader.Read()));
                    quoted = true;
                    continue;
                }

                if (TomlSyntax.IsBareKey(c))
                {
                    buffer.Append(c);
                    goto consume_character;
                }

                // If we see an invalid symbol, let the next parser handle it
                break;

                consume_character:
                reader.Read();
                col++;
            }

            if (buffer.Length == 0)
                return AddError($"Found an extra subkey separator in {".".Join(parts)}...");

            parts.Add(buffer.ToString());

            return true;
        }

        #endregion

        #region Non-string value parsing

        /**
         * Reads the whole raw value until the first non-value character is encountered.
         * Assumes the cursor start position at the first value character and consumes all characters that may be related to the value.
         * Example:
         * 
         * 1_0_0_0  ==>  1_0_0_0
         * ^                    ^
         */

        private string ReadRawValue()
        {
            var result = new StringBuilder();

            int cur;
            while ((cur = reader.Peek()) >= 0)
            {
                var c = (char) cur;

                if (c == TomlSyntax.COMMENT_SYMBOL || TomlSyntax.IsNewLine(c) || TomlSyntax.IsValueSeparator(c)) break;

                result.Append(c);

                ConsumeChar();
            }

            // Replace trim with manual space counting?
            return result.ToString().Trim();
        }

        /**
         * Reads and parses a non-string, non-composite TOML value.
         * Assumes the cursor at the first character that is related to the value (with possible spaces).
         * Consumes all the characters that are related to the value.
         *
         * Example
         * 1_0_0_0 # This is a comment <newline>  ==>  1_0_0_0 # This is a comment
         * ^                                                  ^  
         */

        private TomlNode ReadTomlValue()
        {
            var value = ReadRawValue();

            if (TomlSyntax.IsBoolean(value)) return bool.Parse(value);

            if (TomlSyntax.IsNaN(value)) return double.NaN;

            if (TomlSyntax.IsPosInf(value)) return double.PositiveInfinity;

            if (TomlSyntax.IsNegInf(value)) return double.NegativeInfinity;

            if (TomlSyntax.IsInteger(value))
                return long.Parse(value.RemoveAll(TomlSyntax.INT_NUMBER_SEPARATOR), CultureInfo.InvariantCulture);

            if (TomlSyntax.IsFloat(value))
                return double.Parse(value.RemoveAll(TomlSyntax.INT_NUMBER_SEPARATOR), CultureInfo.InvariantCulture);

            if (TomlSyntax.IsIntegerWithBase(value, out var numberBase))
                return new TomlInteger
                {
                    Value = Convert.ToInt64(value.Substring(2).RemoveAll(TomlSyntax.INT_NUMBER_SEPARATOR), numberBase),
                    IntegerBase = (TomlInteger.Base) numberBase
                };

            value = value.Replace("T", " ");
            if (StringUtils.TryParseDateTime(value,
                                             TomlSyntax.RFC3339LocalDateTimeFormats,
                                             DateTimeStyles.AssumeLocal,
                                             out var dateTimeResult,
                                             out var precision))
                return new TomlDateTime
                {
                    Value = dateTimeResult,
                    SecondsPrecision = precision
                };

            if (StringUtils.TryParseDateTime(value,
                                             TomlSyntax.RFC3339Formats,
                                             DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                                             out dateTimeResult,
                                             out precision))
                return new TomlDateTime
                {
                    Value = dateTimeResult,
                    SecondsPrecision = precision
                };


            if (DateTime.TryParseExact(value,
                                       TomlSyntax.LocalDateFormat,
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.AssumeLocal,
                                       out dateTimeResult))
                return new TomlDateTime
                {
                    Value = dateTimeResult,
                    OnlyDate = true
                };

            if (StringUtils.TryParseDateTime(value,
                                             TomlSyntax.RFC3339LocalTimeFormats,
                                             DateTimeStyles.AssumeLocal,
                                             out dateTimeResult,
                                             out precision))
                return new TomlDateTime
                {
                    Value = dateTimeResult,
                    OnlyTime = true,
                    SecondsPrecision = precision
                };

            AddError($"Value \"{value}\" is not a valid TOML 0.5.0 value!");
            return null;
        }

        /**
         * Reads an array value.
         * Assumes the cursor is at the start of the array definition. Reads all character until the array closing bracket.
         *
         * Example:
         * [1, 2, 3]  ==>  [1, 2, 3]
         * ^                        ^
         */

        private TomlArray ReadArray()
        {
            // Consume the start of array character
            ConsumeChar();

            var result = new TomlArray();

            TomlNode currentValue = null;

            int cur;
            while ((cur = reader.Peek()) >= 0)
            {
                var c = (char) cur;

                if (c == TomlSyntax.ARRAY_END_SYMBOL)
                {
                    ConsumeChar();
                    break;
                }

                if (c == TomlSyntax.COMMENT_SYMBOL)
                {
                    reader.ReadLine();
                    AdvanceLine(0);
                    continue;
                }

                if (TomlSyntax.IsWhiteSpace(c) || TomlSyntax.IsNewLine(c))
                {
                    if (TomlSyntax.IsLineBreak(c))
                        AdvanceLine();
                    goto consume_character;
                }

                if (c == TomlSyntax.ITEM_SEPARATOR)
                {
                    if (currentValue == null)
                    {
                        AddError("Encountered multiple value separators in an array!");
                        return null;
                    }

                    result.Add(currentValue);
                    currentValue = null;
                    goto consume_character;
                }

                currentValue = ReadValue(true);

                if (currentValue == null)
                {
                    if (currentState != ParseState.None)
                        AddError("Failed to determine and parse a value!");
                    return null;
                }

                if (result.ChildrenCount != 0 && result[0].GetType() != currentValue.GetType())
                {
                    AddError($"Arrays cannot have mixed types! Inferred type: {result[0].GetType().FullName}. Element type: {currentValue.GetType().FullName}");
                    return null;
                }

                continue;

                consume_character:
                ConsumeChar();
            }

            if (currentValue != null) result.Add(currentValue);

            return result;
        }

        /**
         * Reads an inline table.
         * Assumes the cursor is at the start of the table definition. Reads all character until the table closing bracket.
         *
         * Example:
         * { test = "foo", value = 1 }  ==>  { test = "foo", value = 1 }
         * ^                                                            ^
         */

        private TomlNode ReadInlineTable()
        {
            ConsumeChar();

            var result = new TomlTable
            {
                IsInline = true
            };

            TomlNode currentValue = null;

            var keyParts = new List<string>();

            int cur;
            while ((cur = reader.Peek()) >= 0)
            {
                var c = (char) cur;

                if (c == TomlSyntax.INLINE_TABLE_END_SYMBOL)
                {
                    ConsumeChar();
                    break;
                }

                if (c == TomlSyntax.COMMENT_SYMBOL)
                {
                    AddError("Incomplete inline table definition!");
                    return null;
                }

                if (TomlSyntax.IsNewLine(c))
                {
                    AddError("Inline tables are only allowed to be on single line");
                    return null;
                }

                if (TomlSyntax.IsWhiteSpace(c))
                    goto consume_character;

                if (c == TomlSyntax.ITEM_SEPARATOR)
                {
                    if (currentValue == null)
                    {
                        AddError("Encountered multiple value separators in inline table!");
                        return null;
                    }

                    if (!InsertNode(currentValue, result, keyParts))
                        return null;
                    keyParts.Clear();
                    currentValue = null;
                    goto consume_character;
                }

                currentValue = ReadKeyValuePair(keyParts);
                continue;

                consume_character:
                ConsumeChar();
            }

            if (currentValue != null && !InsertNode(currentValue, result, keyParts))
                return null;

            return result;
        }

        #endregion

        #region String parsing

        /**
         * Checks if the string value a multiline string (i.e. a triple quoted string).
         * Assumes the cursor is at the first quote character. Consumes the least amount of characters needed to determine if the string is multiline.
         *
         * If the result is false, returns the consumed character through the `excess` variable.
         *
         * Example 1:
         * """test"""  ==>  """test"""
         * ^                   ^
         *
         * Example 2:
         * "test"  ==>  "test"         (doesn't return the first quote)
         * ^             ^
         *
         * Example 3:
         * ""  ==>  ""        (returns the extra `"` through the `excess` variable)
         * ^          ^
         */
        private bool IsTripleQuote(char quote, out char excess)
        {
            // Copypasta, but it's faster...

            int cur;
            // Consume the first quote
            ConsumeChar();

            if ((cur = reader.Peek()) < 0)
            {
                excess = '\0';
                return AddError("Unexpected end of file!");
            }


            if ((char) cur != quote)
            {
                excess = '\0';
                return false;
            }

            // Consume the second quote
            excess = (char) ConsumeChar();

            if ((cur = reader.Peek()) < 0 || (char) cur != quote) return false;

            // Consume the final quote
            ConsumeChar();

            excess = '\0';
            return true;
        }

        /**
         * A convenience method to process a single character within a quote.
         */

        private bool ProcessQuotedValueCharacter(char quote,
                                                 bool isNonLiteral,
                                                 char c,
                                                 int next,
                                                 StringBuilder sb,
                                                 ref bool escaped)
        {
            if (TomlSyntax.ShouldBeEscaped(c))
                return AddError($"The character U+{(int) c:X8} must be escaped in a string!");

            if (escaped)
            {
                sb.Append(c);
                escaped = false;
                return false;
            }

            if (c == quote) return true;

            if (isNonLiteral && c == TomlSyntax.ESCAPE_SYMBOL)
                if (next >= 0 && (char) next == quote)
                    escaped = true;

            if (c == TomlSyntax.NEWLINE_CHARACTER)
                return AddError("Encountered newline in single line string!");

            sb.Append(c);
            return false;
        }

        /**
         * Reads a single-line string.
         * Assumes the cursor is at the first character that belongs to the string.
         * Consumes all characters that belong to the string (including the closing quote).
         *
         * Example:
         * "test"  ==>  "test"
         *  ^                 ^
         */

        private string ReadQuotedValueSingleLine(char quote, char initialData = '\0')
        {
            var isNonLiteral = quote == TomlSyntax.BASIC_STRING_SYMBOL;
            var sb = new StringBuilder();

            var escaped = false;

            if (initialData != '\0')
            {
                var shouldReturn =
                    ProcessQuotedValueCharacter(quote, isNonLiteral, initialData, reader.Peek(), sb, ref escaped);

                if (currentState == ParseState.None)
                    return null;

                if (shouldReturn)
                    return isNonLiteral ? sb.ToString().Unescape() : sb.ToString();
            }

            int cur;
            while ((cur = reader.Read()) >= 0)
            {
                // Consume the character
                col++;
                var c = (char) cur;
                if (ProcessQuotedValueCharacter(quote, isNonLiteral, c, reader.Peek(), sb, ref escaped))
                {
                    if (currentState == ParseState.None)
                        return null;

                    break;
                }
            }

            return isNonLiteral ? sb.ToString().Unescape() : sb.ToString();
        }

        /**
         * Reads a multiline string.
         * Assumes the cursor is at the first character that belongs to the string.
         * Consumes all characters that belong to the string and the three closing quotes.
         *
         * Example:
         * """test"""  ==>  """test"""
         *    ^                       ^
         */

        private string ReadQuotedValueMultiLine(char quote)
        {
            var isBasic = quote == TomlSyntax.BASIC_STRING_SYMBOL;
            var sb = new StringBuilder();

            var escaped = false;
            var skipWhitespace = false;
            var quotesEncountered = 0;
            var first = true;

            int cur;
            while ((cur = ConsumeChar()) >= 0)
            {
                var c = (char) cur;

                if (TomlSyntax.ShouldBeEscaped(c))
                    throw new Exception($"The character U+{(int) c:X8} must be escaped!");

                // Trim the first newline
                if (first && TomlSyntax.IsNewLine(c))
                {
                    if (TomlSyntax.IsLineBreak(c))
                        first = false;
                    else
                        AdvanceLine();

                    continue;
                }

                first = false;

                //TODO: Reuse ProcessQuotedValueCharacter

                // Skip the current character if it is going to be escaped later
                if (escaped)
                {
                    sb.Append(c);
                    escaped = false;
                    continue;
                }

                // If we are currently skipping empty spaces, skip
                if (skipWhitespace)
                {
                    if (TomlSyntax.IsEmptySpace(c))
                    {
                        if (TomlSyntax.IsLineBreak(c))
                            AdvanceLine();
                        continue;
                    }

                    skipWhitespace = false;
                }

                // If we encounter an escape sequence...
                if (isBasic && c == TomlSyntax.ESCAPE_SYMBOL)
                {
                    var next = reader.Peek();
                    if (next >= 0)
                    {
                        // ...and the next char is empty space, we must skip all whitespaces
                        if (TomlSyntax.IsEmptySpace((char) next))
                        {
                            skipWhitespace = true;
                            continue;
                        }

                        // ...and we have \", skip the character
                        if ((char) next == quote) escaped = true;
                    }
                }

                // Count the consecutive quotes
                if (c == quote)
                    quotesEncountered++;
                else
                    quotesEncountered = 0;

                // If the are three quotes, count them as closing quotes
                if (quotesEncountered == 3) break;

                sb.Append(c);
            }

            // Remove last two quotes (third one wasn't included by default
            sb.Length -= 2;

            return isBasic ? sb.ToString().Unescape() : sb.ToString();
        }

        #endregion

        #region Node creation

        private bool InsertNode(TomlNode node, TomlNode root, List<string> path)
        {
            var latestNode = root;

            if (path.Count > 1)
                for (var index = 0; index < path.Count - 1; index++)
                {
                    var subkey = path[index];
                    if (latestNode.TryGetNode(subkey, out var currentNode))
                    {
                        if (currentNode.HasValue)
                            return AddError($"The key {".".Join(path)} already has a value assigned to it!");
                    }
                    else
                    {
                        currentNode = new TomlTable();
                        latestNode[subkey] = currentNode;
                    }

                    latestNode = currentNode;
                }

            if (latestNode.HasKey(path[path.Count - 1]))
                return AddError($"The key {".".Join(path)} is already defined!");

            latestNode[path[path.Count - 1]] = node;
            node.CollapseLevel = path.Count - 1;

            return true;
        }

        private TomlTable CreateTable(TomlNode root, List<string> path, bool arrayTable)
        {
            if (path.Count == 0) return null;

            var latestNode = root;

            for (var index = 0; index < path.Count; index++)
            {
                var subkey = path[index];

                if (latestNode.TryGetNode(subkey, out var node))
                {
                    if (node.IsArray && arrayTable)
                    {
                        var arr = (TomlArray) node;

                        if (!arr.IsTableArray)
                        {
                            AddError($"The array {".".Join(path)} cannot be redefined as an array table!");
                            return null;
                        }

                        if (index == path.Count - 1)
                        {
                            latestNode = new TomlTable();
                            arr.Add(latestNode);
                            break;
                        }

                        latestNode = arr[arr.ChildrenCount - 1];
                        continue;
                    }

                    if (node.HasValue)
                    {
                        if (!(node is TomlArray array) || !array.IsTableArray)
                        {
                            AddError($"The key {".".Join(path)} has a value assigned to it!");
                            return null;
                        }

                        latestNode = array[array.ChildrenCount - 1];
                        continue;
                    }

                    if (index == path.Count - 1)
                    {
                        if (arrayTable && !node.IsArray)
                        {
                            AddError($"The table {".".Join(path)} cannot be redefined as an array table!");
                            return null;
                        }

                        if (node is TomlTable tbl && !tbl.IsInline)
                        {
                            AddError($"The table {".".Join(path)} is defined multiple times!");
                            return null;
                        }
                    }
                }
                else
                {
                    if (index == path.Count - 1 && arrayTable)
                    {
                        var table = new TomlTable();
                        var arr = new TomlArray
                        {
                            IsTableArray = true
                        };
                        arr.Add(table);
                        latestNode[subkey] = arr;
                        latestNode = table;
                        break;
                    }

                    node = new TomlTable
                    {
                        IsInline = true
                    };
                    latestNode[subkey] = node;
                }

                latestNode = node;
            }

            var result = (TomlTable) latestNode;
            return result;
        }

        #endregion
    }

    #endregion

    public static class TOML
    {
        public static bool ForceASCII { get; set; } = false;

        public static TomlTable Parse(TextReader reader)
        {
            using (var parser = new TOMLParser(reader) {ForceASCII = ForceASCII})
            {
                return parser.Parse();
            }
        }
    }

    #region Exception Types

    public class TomlFormatException : Exception
    {
        public TomlFormatException(string message) : base(message) { }
    }

    public class TomlParseException : Exception
    {
        public TomlParseException(TomlTable parsed, IEnumerable<TomlSyntaxException> exceptions) :
            base("TOML file contains format errors")
        {
            ParsedTable = parsed;
            SyntaxErrors = exceptions;
        }

        public TomlTable ParsedTable { get; }

        public IEnumerable<TomlSyntaxException> SyntaxErrors { get; }
    }

    public class TomlSyntaxException : Exception
    {
        public TomlSyntaxException(string message, TOMLParser.ParseState state, int line, int col) : base(message)
        {
            ParseState = state;
            Line = line;
            Column = col;
        }

        public TOMLParser.ParseState ParseState { get; }

        public int Line { get; }

        public int Column { get; }
    }

    #endregion

    #region Parse utilities

    internal static class TomlSyntax
    {
        #region Type Patterns

        public const string TRUE_VALUE = "true";
        public const string FALSE_VALUE = "false";
        public const string NAN_VALUE = "nan";
        public const string POS_NAN_VALUE = "+nan";
        public const string NEG_NAN_VALUE = "-nan";
        public const string INF_VALUE = "inf";
        public const string POS_INF_VALUE = "+inf";
        public const string NEG_INF_VALUE = "-inf";

        public static bool IsBoolean(string s) => s == TRUE_VALUE || s == FALSE_VALUE;

        public static bool IsPosInf(string s) => s == INF_VALUE || s == POS_INF_VALUE;

        public static bool IsNegInf(string s) => s == NEG_INF_VALUE;

        public static bool IsNaN(string s) => s == NAN_VALUE || s == POS_NAN_VALUE || s == NEG_NAN_VALUE;

        public static bool IsInteger(string s) => IntegerPattern.IsMatch(s);

        public static bool IsFloat(string s) => FloatPattern.IsMatch(s);

        public static bool IsIntegerWithBase(string s, out int numberBase)
        {
            numberBase = 10;
            var match = BasedIntegerPattern.Match(s);
            if (!match.Success) return false;
            IntegerBases.TryGetValue(match.Groups["base"].Value, out numberBase);
            return true;
        }

        /**
         * A pattern to verify the integer value according to the TOML specification.
         */
        public static readonly Regex IntegerPattern =
            new Regex(@"^(\+|-)?(?!_)(0|(?!0)(_?\d)*)$");

        /**
         * A pattern to verify a special 0x, 0o and 0b forms of an integer according to the TOML specification.
         */
        public static readonly Regex BasedIntegerPattern =
            new Regex(@"^(\+|-)?0(?<base>x|b|o)(?!_)(_?[0-9A-F])*$", RegexOptions.IgnoreCase);

        /**
         * A pattern to verify the float value according to the TOML specification.
         */
        public static readonly Regex FloatPattern =
            new
                Regex(@"^(\+|-)?(?!_)(0|(?!0)(_?\d)+)(((e(\+|-)?(?!_)(_?\d)+)?)|(\.(?!_)(_?\d)+(e(\+|-)?(?!_)(_?\d)+)?))$",
                      RegexOptions.IgnoreCase);

        /**
         * A helper dictionary to map TOML base codes into the radii.
         */
        public static readonly Dictionary<string, int> IntegerBases = new Dictionary<string, int>
        {
            ["x"] = 16,
            ["o"] = 8,
            ["b"] = 2
        };

        /**
         * A helper dictionary to map non-decimal bases to their TOML identifiers
         */
        public static readonly Dictionary<int, string> BaseIdentifiers = new Dictionary<int, string>
        {
            [2] = "b",
            [8] = "o",
            [16] = "x"
        };

        /**
         *  Valid date formats with timezone as per RFC3339.
         */
        public static readonly string[] RFC3339Formats =
        {
            "yyyy'-'MM-dd HH':'mm':'ssK", "yyyy'-'MM-dd HH':'mm':'ss'.'fK", "yyyy'-'MM-dd HH':'mm':'ss'.'ffK",
            "yyyy'-'MM-dd HH':'mm':'ss'.'fffK", "yyyy'-'MM-dd HH':'mm':'ss'.'ffffK",
            "yyyy'-'MM-dd HH':'mm':'ss'.'fffffK", "yyyy'-'MM-dd HH':'mm':'ss'.'ffffffK",
            "yyyy'-'MM-dd HH':'mm':'ss'.'fffffffK"
        };

        /**
         *  Valid date formats without timezone (assumes local) as per RFC3339.
         */
        public static readonly string[] RFC3339LocalDateTimeFormats =
        {
            "yyyy'-'MM-dd HH':'mm':'ss", "yyyy'-'MM-dd HH':'mm':'ss'.'f", "yyyy'-'MM-dd HH':'mm':'ss'.'ff",
            "yyyy'-'MM-dd HH':'mm':'ss'.'fff", "yyyy'-'MM-dd HH':'mm':'ss'.'ffff",
            "yyyy'-'MM-dd HH':'mm':'ss'.'fffff", "yyyy'-'MM-dd HH':'mm':'ss'.'ffffff",
            "yyyy'-'MM-dd HH':'mm':'ss'.'fffffff"
        };

        /**
         *  Valid full date format as per TOML spec.
         */
        public static readonly string LocalDateFormat = "yyyy'-'MM'-'dd";

        /**
        *  Valid time formats as per TOML spec.
        */
        public static readonly string[] RFC3339LocalTimeFormats =
        {
            "HH':'mm':'ss", "HH':'mm':'ss'.'f", "HH':'mm':'ss'.'ff", "HH':'mm':'ss'.'fff", "HH':'mm':'ss'.'ffff",
            "HH':'mm':'ss'.'fffff", "HH':'mm':'ss'.'ffffff", "HH':'mm':'ss'.'fffffff"
        };

        #endregion

        #region Character definitions

        public const char ARRAY_END_SYMBOL = ']';
        public const char ITEM_SEPARATOR = ',';
        public const char ARRAY_START_SYMBOL = '[';
        public const char BASIC_STRING_SYMBOL = '\"';
        public const char COMMENT_SYMBOL = '#';
        public const char ESCAPE_SYMBOL = '\\';
        public const char KEY_VALUE_SEPARATOR = '=';
        public const char NEWLINE_CARRIAGE_RETURN_CHARACTER = '\r';
        public const char NEWLINE_CHARACTER = '\n';
        public const char SUBKEY_SEPARATOR = '.';
        public const char TABLE_END_SYMBOL = ']';
        public const char TABLE_START_SYMBOL = '[';
        public const char INLINE_TABLE_START_SYMBOL = '{';
        public const char INLINE_TABLE_END_SYMBOL = '}';
        public const char LITERAL_STRING_SYMBOL = '\'';
        public const char INT_NUMBER_SEPARATOR = '_';

        public static readonly char[] NewLineCharacters = {NEWLINE_CHARACTER, NEWLINE_CARRIAGE_RETURN_CHARACTER};

        public static bool IsQuoted(char c) => c == BASIC_STRING_SYMBOL || c == LITERAL_STRING_SYMBOL;

        public static bool IsWhiteSpace(char c) => c == ' ' || c == '\t';

        public static bool IsNewLine(char c) => c == NEWLINE_CHARACTER || c == NEWLINE_CARRIAGE_RETURN_CHARACTER;

        public static bool IsLineBreak(char c) => c == NEWLINE_CHARACTER;

        public static bool IsEmptySpace(char c) => IsWhiteSpace(c) || IsNewLine(c);

        public static bool IsBareKey(char c) =>
            'A' <= c && c <= 'Z' || 'a' <= c && c <= 'z' || '0' <= c && c <= '9' || c == '_' || c == '-';

        public static bool ShouldBeEscaped(char c) => (c <= '\u001f' || c == '\u007f') && !IsNewLine(c);

        public static bool IsValueSeparator(char c) =>
            c == ITEM_SEPARATOR || c == ARRAY_END_SYMBOL || c == INLINE_TABLE_END_SYMBOL;

        #endregion
    }

    internal static class StringUtils
    {
        public static string AsKey(this string key)
        {
            var quote = false;
            foreach (var c in key)
            {
                if (TomlSyntax.IsBareKey(c)) continue;
                quote = true;
                break;
            }

            return !quote ? key : $"{TomlSyntax.BASIC_STRING_SYMBOL}{key.Escape()}{TomlSyntax.BASIC_STRING_SYMBOL}";
        }

        public static string Join(this string self, IEnumerable<string> subItems)
        {
            var sb = new StringBuilder();
            var first = true;

            foreach (var subItem in subItems)
            {
                if (!first) sb.Append(self);
                first = false;
                sb.Append(subItem);
            }

            return sb.ToString();
        }

        public static bool TryParseDateTime(string s,
                                            string[] formats,
                                            DateTimeStyles styles,
                                            out DateTime dateTime,
                                            out int parsedFormat)
        {
            parsedFormat = 0;
            dateTime = new DateTime();

            for (var i = 0; i < formats.Length; i++)
            {
                var format = formats[i];
                if (!DateTime.TryParseExact(s, format, CultureInfo.InvariantCulture, styles, out dateTime)) continue;
                parsedFormat = i;
                return true;
            }

            return false;
        }

        public static void AsComment(this string self, TextWriter tw)
        {
            foreach (var line in self.Split(TomlSyntax.NEWLINE_CHARACTER))
                tw.WriteLine($"{TomlSyntax.COMMENT_SYMBOL} {line.Trim()}");
        }

        public static string RemoveAll(this string txt, char toRemove)
        {
            var sb = new StringBuilder(txt.Length);

            foreach (var c in txt)
                if (c != toRemove)
                    sb.Append(c);

            return sb.ToString();
        }

        public static string Escape(this string txt, bool escapeNewlines = true)
        {
            var stringBuilder = new StringBuilder(txt.Length + 2);
            for (var i = 0; i < txt.Length; i++)
            {
                var c = txt[i];
                switch (c)
                {
                    case '\b':
                        stringBuilder.Append(@"\b");
                        break;
                    case '\t':
                        stringBuilder.Append(@"\t");
                        break;
                    case '\n' when escapeNewlines:
                        stringBuilder.Append(@"\n");
                        break;
                    case '\f':
                        stringBuilder.Append(@"\f");
                        break;
                    case '\r' when escapeNewlines:
                        stringBuilder.Append(@"\r");
                        break;
                    case '\\':
                        stringBuilder.Append(@"\");
                        break;
                    case '\"':
                        stringBuilder.Append(@"\""");
                        break;
                    default:
                        if (TomlSyntax.ShouldBeEscaped(c) || TOML.ForceASCII && c > sbyte.MaxValue)
                        {
                            if (char.IsSurrogatePair(txt, i))
                                stringBuilder.Append("\\U").Append(char.ConvertToUtf32(txt, i++).ToString("X8"));
                            else
                                stringBuilder.Append("\\u").Append(((ushort) c).ToString("X4"));
                        }
                        else
                        {
                            stringBuilder.Append(c);
                        }

                        break;
                }
            }

            return stringBuilder.ToString();
        }

        public static string Unescape(this string txt)
        {
            if (string.IsNullOrEmpty(txt)) return txt;
            var stringBuilder = new StringBuilder(txt.Length);
            for (var i = 0; i < txt.Length;)
            {
                var num = txt.IndexOf('\\', i);
                var next = num + 1;
                if (num < 0 || num == txt.Length - 1) num = txt.Length;
                stringBuilder.Append(txt, i, num - i);
                if (num >= txt.Length) break;
                var c = txt[next];
                switch (c)
                {
                    case 'b':
                        stringBuilder.Append('\b');
                        break;
                    case 't':
                        stringBuilder.Append('\t');
                        break;
                    case 'n':
                        stringBuilder.Append('\n');
                        break;
                    case 'f':
                        stringBuilder.Append('\f');
                        break;
                    case 'r':
                        stringBuilder.Append('\r');
                        break;
                    case '\'':
                        stringBuilder.Append('\'');
                        break;
                    case '\"':
                        stringBuilder.Append('\"');
                        break;
                    case '\\':
                        stringBuilder.Append('\\');
                        break;
                    case 'u':
                        if (next + 4 >= txt.Length) throw new Exception("Undefined escape sequence!");
                        stringBuilder.Append(char.ConvertFromUtf32(Convert.ToInt32(txt.Substring(next + 1, 4), 16)));
                        num += 4;
                        break;
                    case 'U':
                        if (next + 8 >= txt.Length) throw new Exception("Undefined escape sequence!");
                        stringBuilder.Append(char.ConvertFromUtf32(Convert.ToInt32(txt.Substring(next + 1, 8), 16)));
                        num += 8;
                        break;
                    default:
                        throw new Exception("Undefined escape sequence!");
                }

                i = num + 2;
            }

            return stringBuilder.ToString();
        }
    }

    #endregion
}