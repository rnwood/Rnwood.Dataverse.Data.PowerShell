using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Model
{
    /// <summary>
    /// A hashtable that serializes to PowerShell syntax in its ToString() method.
    /// </summary>
    public class PSSerializableHashtable : Hashtable
    {
        /// <summary>
        /// Returns a string representation of the hashtable in PowerShell syntax.
        /// </summary>
        /// <returns>A PowerShell hashtable literal like @{key1 = "value1"; key2 = "value2"}</returns>
        public override string ToString()
        {
            if (Count == 0)
            {
                return "@{}";
            }

            var sb = new StringBuilder();
            sb.AppendLine("@{");
            foreach (var key in Keys.Cast<object>())
            {
                sb.AppendLine($"    {FormatValue(key)} = {FormatValue(this[key])}");
            }
            sb.Append("}");
            return sb.ToString();
        }

        private static string FormatValue(object value)
        {
            if (value == null)
            {
                return "$null";
            }

            if (value is string str)
            {
                // Escape quotes and format as string literal
                return $"\"{str.Replace("\"", "`\"")}\"";
            }

            if (value is bool b)
            {
                return b ? "$true" : "$false";
            }

            if (value is int || value is long || value is double || value is float || value is decimal)
            {
                return value.ToString();
            }

            if (value is PSSerializableHashtable psHashtable)
            {
                return psHashtable.ToString();
            }

            if (value is Hashtable hashtable)
            {
                // Convert regular hashtable to PS serializable format
                var result = new PSSerializableHashtable();
                foreach (DictionaryEntry entry in hashtable)
                {
                    result[entry.Key] = entry.Value;
                }
                return result.ToString();
            }

            if (value is Array array)
            {
                if (array.Length == 0)
                {
                    return "@()";
                }
                var sb = new StringBuilder();
                sb.AppendLine("@(");
                foreach (var element in array)
                {
                    sb.AppendLine($"    {FormatValue(element)},");
                }
                sb.Append(")");
                return sb.ToString();
            }

            // For other types, use the default string representation
            return value.ToString();
        }
    }
}