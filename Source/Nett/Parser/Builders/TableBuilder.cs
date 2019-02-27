using System;
using System.Collections.Generic;
using System.Linq;
using Nett.LinqExtensions;
using Nett.Parser.Nodes;

namespace Nett.Parser.Builders
{
    internal static class TableBuilder
    {
        public static TomlTable Build(StartNode node, TomlSettings settings)
        {
            TomlTable.RootTable rootTable = new TomlTable.RootTable(settings);
            TomlTable current = rootTable;

            if (node == null) { return rootTable; }

            System.Collections.Generic.IEnumerable<ExpressionNode> expressions = node.Expressions();

            foreach (ExpressionNode e in expressions)
            {
                switch (e)
                {
                    case KeyValueExpressionNode kvn:
                        var val = ToTomlValue(rootTable, kvn.Value.SyntaxNode());
                        val.AddComments(kvn);

                        if (kvn.Key.SyntaxNode().IsDottedKey())
                        {
                            var chain = KeyChain.FromSegments(kvn.Key.SyntaxNode().GetSegments());
                            var owner = FindOrCreateOwnerTableForTable(current, chain, out var last, TomlTable.TableTypes.Dotted);
                            owner.AddRow(last, val);
                        }
                        else
                        {
                            current.AddRow(kvn.Key.SyntaxNode().ExpressionKey(), val);
                        }

                        break;
                    case TableNode tn:
                        current = CreateTableOrArrayOfTables(rootTable, tn);
                        break;
                    case CommentExpressionNode cen:
                        var loc = rootTable.Rows.Any() ? CommentLocation.Append : CommentLocation.Prepend;
                        foreach (var c in cen.Comments) { rootTable.AddComment(c.Value, loc); }
                        break;
                    default:
                        throw new InvalidOperationException($"Encountered unexpected expression of type '{e.GetType()}'.");
                }
            }

            return rootTable;
        }

        private static TomlTable CreateTableOrArrayOfTables(TomlTable.RootTable root, TableNode table)
        {
            Node sn = table.Table.SyntaxNode();
            switch (sn)
            {
                case StandardTableNode stn: return CreateStandardTable(root, stn, table);
                case TableArrayNode an: return CreateTableArray(root, an, table);
                default:
                    throw new InvalidOperationException(
                    $"Encountered unexpected table node of type '{sn?.GetType().ToString() ?? "null"}'.");
            }
        }

        private static TomlTable CreateStandardTable(TomlTable.RootTable root, StandardTableNode table, IHasComments comments)
        {
            var keySegments = table.Key.SyntaxNode().GetSegments();
            KeyChain chain = KeyChain.FromSegments(keySegments);

            if (chain.IsEmpty) { throw new InvalidOperationException("Empty TOML key is not allowed."); }

            TomlTable owner = FindOrCreateOwnerTableForTable(root, chain, out TomlKey last);
            TomlObject existing = owner.TryGetValue(last);
            if (existing != null)
            {
                throw new InvalidOperationException($"Cannot define table with key '{chain}' as the owner already " +
                    $"contains a row for key '{last}' of type '{existing.ReadableTypeName}'.");
            }
            else
            {
                TomlTable newTable = new TomlTable(root);
                owner.AddRow(last, newTable);
                newTable.AddComments(comments);
                return newTable;
            }
        }

        private static TomlTable CreateTableArray(TomlTable.RootTable root, TableArrayNode tableArray, IHasComments comments)
        {
            System.Collections.Generic.IEnumerable<TerminalNode> keySegments = tableArray.Key.SyntaxNode().GetSegments();
            KeyChain chain = KeyChain.FromSegments(keySegments);

            if (chain.IsEmpty) { throw new InvalidOperationException("Empty TOML key is not allowed."); }

            TomlTable owner = FindOrCreateOwnerTableForTableArray(root, chain, out TomlKey last);
            TomlObject existing = owner.TryGetValue(last);
            if (existing != null && existing is TomlTableArray existingArray)
            {
                TomlTable newTable = new TomlTable(root);
                existingArray.Add(newTable);
                newTable.AddComments(comments);
                return newTable;
            }
            else if (existing == null)
            {
                TomlTableArray newTableArray = new TomlTableArray(root);
                owner.AddRow(last, newTableArray);
                TomlTable newTable = new TomlTable(root);
                newTable.AddComments(comments);
                newTableArray.Add(newTable);
                return newTable;
            }
            else
            {
                throw new InvalidOperationException($"Cannot define table array '{last}' as an object of type "
                    + $"'{existing.ReadableTypeName}' exists already.");
            }
        }

        private static TomlTable FindOrCreateOwnerTableForTable(
            TomlTable current, KeyChain keys, out TomlKey last, TomlTable.TableTypes tableType = TomlTable.TableTypes.Default)
        {
            if (keys.IsLastSegment)
            {
                last = keys.Key;
                return current;
            }

            TomlObject row = current.TryGetValue(keys.Key);

            if (row != null)
            {
                if (row is TomlTable tbl)
                {
                    return FindOrCreateOwnerTableForTable(tbl, keys.Next, out last, tableType);
                }
                else if (row is TomlTableArray ta)
                {
                    return FindOrCreateOwnerTableForTable(ta.Last(), keys.Next, out last, tableType);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Key '{keys}' corresponds to a TOML object hat is not of type TOML table.");
                }
            }
            else
            {
                TomlTable t = new TomlTable(current.Root, tableType);
                current.AddRow(keys.Key, t);
                return FindOrCreateOwnerTableForTable(t, keys.Next, out last, tableType);
            }
        }

        private static TomlTable FindOrCreateOwnerTableForTableArray(TomlTable current, KeyChain keys, out TomlKey last)
        {
            if (keys.IsLastSegment)
            {
                last = keys.Key;
                return current;
            }

            TomlObject row = current.TryGetValue(keys.Key);

            if (row != null)
            {
                if (row is TomlTable tbl)
                {
                    return FindOrCreateOwnerTableForTable(tbl, keys.Next, out last);
                }
                else if (row is TomlTableArray ta)
                {
                    return FindOrCreateOwnerTableForTableArray(ta.Items.Last(), keys.Next, out last);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Key '{keys}' corresponds to a TOML object hat is not of type TOML table or TOML table array.");
                }
            }
            else
            {
                TomlTableArray a = new TomlTableArray(current.Root);
                TomlTable t = new TomlTable(current.Root);
                a.Add(t);
                current.AddRow(keys.Key, a);
                return FindOrCreateOwnerTableForTableArray(t, keys.Next, out last);
            }
        }

        private static TomlObject ToTomlValue(ITomlRoot root, ValueNode node)
        {
            switch (node.Value.SyntaxNode())
            {
                case TerminalNode tn: return CreateValue(tn);
                case ArrayNode an: return CreateArrayOrTableArray(an);
                case InlineTableNode it: return CreateInlineTable(it);
                default: throw new Exception($"Cannot create TomlValue from node with type '{node.GetType()}'.");
            }

            TomlValue CreateValue(TerminalNode tn)
            {
                var val = CreateValueFromTerminal(tn.Terminal);
                if (node.Unit.HasNode)
                {
                    val.Unit = node.Unit.SyntaxNodeOrDefault().Terminal.Value;
                }

                return val;
            }

            TomlValue CreateValueFromTerminal(Token terminal)
            {
                switch (terminal.Type)
                {
                    case TokenType.Integer: return new TomlInt(root, Convert.ToInt64(Cleanup(terminal.Value, 0)), TomlInt.IntTypes.Decimal);
                    case TokenType.HexInteger: return new TomlInt(root, Convert.ToInt64(Cleanup(terminal.Value, 2), 16), TomlInt.IntTypes.Hex);
                    case TokenType.BinaryInteger: return new TomlInt(root, Convert.ToInt64(Cleanup(terminal.Value, 2), 2), TomlInt.IntTypes.Binary);
                    case TokenType.OctalInteger: return new TomlInt(root, Convert.ToInt64(Cleanup(terminal.Value, 2), 8), TomlInt.IntTypes.Octal);
                    case TokenType.Bool: return new TomlBool(root, Convert.ToBoolean(terminal.Value));
                    case TokenType.String: return new TomlString(root, terminal.Value.Unescape(terminal));
                    case TokenType.LiteralString: return new TomlString(root, terminal.Value, TomlString.TypeOfString.Literal);
                    case TokenType.MultilineLiteralString: return new TomlString(root, terminal.Value, TomlString.TypeOfString.MultilineLiteral);
                    case TokenType.MultilineString: return new TomlString(root, terminal.Value, TomlString.TypeOfString.Multiline);
                    case TokenType.Float: return TomlFloat.FromTerminal(root, terminal);
                    case TokenType.OffsetDateTime: return TomlOffsetDateTime.Parse(root, terminal.Value);
                    case TokenType.LocalTime: return TomlLocalTime.Parse(root, terminal.Value);
                    case TokenType.Duration: return TomlDuration.Parse(root, terminal.Value);
                    case TokenType.LocalDate: return TomlLocalDate.Parse(root, terminal.Value);
                    case TokenType.LocalDateTime: return TomlLocalDateTime.Parse(root, terminal.Value);
                    default: throw new NotSupportedException();
                }

                string Cleanup(string s, int sub)
                    => s.Substring(sub).Replace("_", string.Empty);
            }

            TomlObject CreateArrayOrTableArray(ArrayNode array)
            {
                var values = CreateValues(array.GetValues()).ToList();
                var tables = values.OfType<TomlTable>().ToList();

                if (tables.Count > 0 && tables.Count == values.Count)
                {
                    var ta = new TomlTableArray(root, tables);
                    ta.AddComments(array);
                    return ta;
                }
                else if (tables.Count == 0)
                {
                    var arr = new TomlArray(root, values.Cast<TomlValue>().ToArray());
                    arr.AddComments(array);
                    return arr;
                }
                else
                {
                    throw new InvalidOperationException("Array is a mixture of a value array and a TOML table array.");
                }

                IEnumerable<TomlObject> CreateValues(IEnumerable<ValueNode> nodes)
                {
                    var linked = nodes.Select(n => Tuple.Create(n, ToTomlValue(root, n))).ToList();
                    var expectedType = linked.DistinctBy(n => n.Item2.GetType()).FirstOrDefault();
                    var wrongType = linked.DistinctBy(n => n.Item2.GetType()).Skip(1).FirstOrDefault();

                    if (wrongType != null)
                    {
                        string msg = $"Expected array value of type '{expectedType.Item2.ReadableTypeName}' " +
                            $"but value of type '{wrongType.Item2.ReadableTypeName}' was found.'";

                        throw ParseException.MessageForNode(wrongType.Item1, msg);
                    }

                    return linked.Select(l => l.Item2);
                }
            }

            TomlTable CreateInlineTable(InlineTableNode it)
            {
                TomlTable table = new TomlTable(root, TomlTable.TableTypes.Inline);
                table.AddComments(it);

                System.Collections.Generic.IEnumerable<KeyValueExpressionNode> expressions = it.GetExpressions();

                foreach (KeyValueExpressionNode e in expressions)
                {
                    table.AddRow(e.Key.SyntaxNode().ExpressionKey(), ToTomlValue(root, e.Value.SyntaxNode()));
                }

                return table;
            }
        }
    }
}
