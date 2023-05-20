/*
* Copyright (c) 2013 Calvin Rien
*
* Based on the JSON parser by Patrick van Bergen
* http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
*
* Simplified it so that it doesn't throw exceptions
* and can be used in Unity iPhone with maximum code stripping.
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be
* included in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MultiplayerARPG.MMO
{
    namespace MiniJSON
    {
        // Example usage:
        //
        //  using UnityEngine;
        //  using System.Collections;
        //  using System.Collections.Generic;
        //  using MiniJSON;
        //
        //  public class MiniJSONTest : MonoBehaviour {
        //      void Start () {
        //          var jsonString = "{ \"array\": [1.44,2,3], " +
        //                          "\"object\": {\"key1\":\"value1\", \"key2\":256}, " +
        //                          "\"string\": \"The quick brown fox \\\"jumps\\\" over the lazy dog \", " +
        //                          "\"unicode\": \"\\u3041 Men\u00fa sesi\u00f3n\", " +
        //                          "\"int\": 65536, " +
        //                          "\"float\": 3.1415926, " +
        //                          "\"bool\": true, " +
        //                          "\"null\": null }";
        //
        //          var dict = Json.Deserialize(jsonString) as Dictionary<string,object>;
        //
        //          Debug.Log("deserialized: " + dict.GetType());
        //          Debug.Log("dict['array'][0]: " + ((List<object>) dict["array"])[0]);
        //          Debug.Log("dict['string']: " + (string) dict["string"]);
        //          Debug.Log("dict['float']: " + (double) dict["float"]); // floats come out as doubles
        //          Debug.Log("dict['int']: " + (long) dict["int"]); // ints come out as longs
        //          Debug.Log("dict['unicode']: " + (string) dict["unicode"]);
        //
        //          var str = Json.Serialize(dict);
        //
        //          Debug.Log("serialized: " + str);
        //      }
        //  }

        /// <summary>
        /// This class encodes and decodes JSON strings.
        /// Spec. details, see http://www.json.org/
        ///
        /// JSON uses Arrays and Objects. These correspond here to the datatypes IList and IDictionary.
        /// All numbers are parsed to doubles.
        /// </summary>
        public static class Json
        {
            /// <summary>
            /// Parses the string json into a value
            /// </summary>
            /// <param name="json">A JSON string.</param>
            /// <returns>An List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false</returns>
            public static object Deserialize(string json)
            {
                // save the string for debug information
                if (json == null)
                {
                    return null;
                }

                return Parser.Parse(json);
            }

            sealed class Parser : IDisposable
            {
                const string WORD_BREAK = "{}[],:\"";

                public static bool IsWordBreak(char c)
                {
                    return Char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
                }

                enum TOKEN
                {
                    NONE,
                    CURLY_OPEN,
                    CURLY_CLOSE,
                    SQUARED_OPEN,
                    SQUARED_CLOSE,
                    COLON,
                    COMMA,
                    STRING,
                    NUMBER,
                    TRUE,
                    FALSE,
                    NULL
                };

                StringReader json;

                Parser(string jsonString)
                {
                    json = new StringReader(jsonString);
                }

                public static object Parse(string jsonString)
                {
                    using (var instance = new Parser(jsonString))
                    {
                        return instance.ParseValue();
                    }
                }

                public void Dispose()
                {
                    json.Dispose();
                    json = null;
                }

                Dictionary<string, object> ParseObject()
                {
                    var table = new Dictionary<string, object>();

                    // ditch opening brace
                    json.Read();

                    // {
                    while (true)
                    {
                        switch (NextToken)
                        {
                            case TOKEN.NONE:
                                return null;
                            case TOKEN.COMMA:
                                continue;
                            case TOKEN.CURLY_CLOSE:
                                return table;
                            default:
                                // name
                                var name = ParseString();
                                if (name == null)
                                {
                                    return null;
                                }

                                // :
                                if (NextToken != TOKEN.COLON)
                                {
                                    return null;
                                }
                                // ditch the colon
                                json.Read();

                                // value
                                table[name] = ParseValue();
                                break;
                        }
                    }
                }

                List<object> ParseArray()
                {
                    var array = new List<object>();

                    // ditch opening bracket
                    json.Read();

                    // [
                    var parsing = true;
                    while (parsing)
                    {
                        var nextToken = NextToken;

                        switch (nextToken)
                        {
                            case TOKEN.NONE:
                                return null;
                            case TOKEN.COMMA:
                                continue;
                            case TOKEN.SQUARED_CLOSE:
                                parsing = false;
                                break;
                            default:
                                var value = ParseByToken(nextToken);

                                array.Add(value);
                                break;
                        }
                    }

                    return array;
                }

                object ParseValue()
                {
                    var nextToken = NextToken;
                    return ParseByToken(nextToken);
                }

                object ParseByToken(TOKEN token)
                {
                    switch (token)
                    {
                        case TOKEN.STRING:
                            return ParseString();
                        case TOKEN.NUMBER:
                            return ParseNumber();
                        case TOKEN.CURLY_OPEN:
                            return ParseObject();
                        case TOKEN.SQUARED_OPEN:
                            return ParseArray();
                        case TOKEN.TRUE:
                            return true;
                        case TOKEN.FALSE:
                            return false;
                        case TOKEN.NULL:
                            return null;
                        default:
                            return null;
                    }
                }

                string ParseString()
                {
                    var s = new StringBuilder();
                    char c;

                    // ditch opening quote
                    json.Read();

                    var parsing = true;
                    while (parsing)
                    {

                        if (json.Peek() == -1)
                        {
                            break;
                        }

                        c = NextChar;
                        switch (c)
                        {
                            case '"':
                                parsing = false;
                                break;
                            case '\\':
                                if (json.Peek() == -1)
                                {
                                    parsing = false;
                                    break;
                                }

                                c = NextChar;
                                switch (c)
                                {
                                    case '"':
                                    case '\\':
                                    case '/':
                                        s.Append(c);
                                        break;
                                    case 'b':
                                        s.Append('\b');
                                        break;
                                    case 'f':
                                        s.Append('\f');
                                        break;
                                    case 'n':
                                        s.Append('\n');
                                        break;
                                    case 'r':
                                        s.Append('\r');
                                        break;
                                    case 't':
                                        s.Append('\t');
                                        break;
                                    case 'u':
                                        var hex = new char[4];

                                        for (var i = 0; i < 4; i++)
                                        {
                                            hex[i] = NextChar;
                                        }

                                        s.Append((char)Convert.ToInt32(new string(hex), 16));
                                        break;
                                }
                                break;
                            default:
                                s.Append(c);
                                break;
                        }
                    }

                    return s.ToString();
                }

                object ParseNumber()
                {
                    var number = NextWord;

                    if (number.IndexOf('.') == -1 && number.IndexOf('e') == -1 && number.IndexOf('E') == -1)
                    {
                        Int64.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedInt);
                        return parsedInt;
                    }

                    Double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedDouble);
                    return parsedDouble;
                }

                void EatWhitespace()
                {
                    while (Char.IsWhiteSpace(PeekChar))
                    {
                        json.Read();

                        if (json.Peek() == -1)
                        {
                            break;
                        }
                    }
                }

                char PeekChar => Convert.ToChar(json.Peek());

                char NextChar => Convert.ToChar(json.Read());

                string NextWord
                {
                    get
                    {
                        var word = new StringBuilder();

                        while (!IsWordBreak(PeekChar))
                        {
                            word.Append(NextChar);

                            if (json.Peek() == -1)
                            {
                                break;
                            }
                        }

                        return word.ToString();
                    }
                }

                TOKEN NextToken
                {
                    get
                    {
                        EatWhitespace();

                        if (json.Peek() == -1)
                        {
                            return TOKEN.NONE;
                        }

                        switch (PeekChar)
                        {
                            case '{':
                                return TOKEN.CURLY_OPEN;
                            case '}':
                                json.Read();
                                return TOKEN.CURLY_CLOSE;
                            case '[':
                                return TOKEN.SQUARED_OPEN;
                            case ']':
                                json.Read();
                                return TOKEN.SQUARED_CLOSE;
                            case ',':
                                json.Read();
                                return TOKEN.COMMA;
                            case '"':
                                return TOKEN.STRING;
                            case ':':
                                return TOKEN.COLON;
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                            case '-':
                                return TOKEN.NUMBER;
                        }

                        switch (NextWord)
                        {
                            case "false":
                                return TOKEN.FALSE;
                            case "true":
                                return TOKEN.TRUE;
                            case "null":
                                return TOKEN.NULL;
                        }

                        return TOKEN.NONE;
                    }
                }
            }

            /// <summary>
            /// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
            /// </summary>
            /// <param name="obj">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
            /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
            public static string Serialize(object obj)
            {
                return Serializer.Serialize(obj);
            }

            sealed class Serializer
            {
                readonly StringBuilder builder;

                Serializer()
                {
                    builder = new StringBuilder();
                }

                public static string Serialize(object obj)
                {
                    var instance = new Serializer();

                    instance.SerializeValue(obj);

                    return instance.builder.ToString();
                }

                void SerializeValue(object value)
                {
                    IList asList;
                    IDictionary asDict;
                    string asStr;

                    if (value == null)
                    {
                        builder.Append("null");
                    }
                    else if ((asStr = value as string) != null)
                    {
                        SerializeString(asStr);
                    }
                    else if (value is bool)
                    {
                        builder.Append((bool)value ? "true" : "false");
                    }
                    else if ((asList = value as IList) != null)
                    {
                        SerializeArray(asList);
                    }
                    else if ((asDict = value as IDictionary) != null)
                    {
                        SerializeObject(asDict);
                    }
                    else if (value is char)
                    {
                        SerializeString(new string((char)value, 1));
                    }
                    else
                    {
                        SerializeOther(value);
                    }
                }

                void SerializeObject(IDictionary obj)
                {
                    var first = true;

                    builder.Append('{');

                    foreach (var e in obj.Keys)
                    {
                        if (!first)
                        {
                            builder.Append(',');
                        }

                        SerializeString(e.ToString());
                        builder.Append(':');

                        SerializeValue(obj[e]);

                        first = false;
                    }

                    builder.Append('}');
                }

                void SerializeArray(IList anArray)
                {
                    builder.Append('[');

                    var first = true;

                    foreach (var obj in anArray)
                    {
                        if (!first)
                        {
                            builder.Append(',');
                        }

                        SerializeValue(obj);

                        first = false;
                    }

                    builder.Append(']');
                }

                void SerializeString(string str)
                {
                    builder.Append('\"');

                    var charArray = str.ToCharArray();
                    foreach (var c in charArray)
                    {
                        switch (c)
                        {
                            case '"':
                                builder.Append("\\\"");
                                break;
                            case '\\':
                                builder.Append("\\\\");
                                break;
                            case '\b':
                                builder.Append("\\b");
                                break;
                            case '\f':
                                builder.Append("\\f");
                                break;
                            case '\n':
                                builder.Append("\\n");
                                break;
                            case '\r':
                                builder.Append("\\r");
                                break;
                            case '\t':
                                builder.Append("\\t");
                                break;
                            default:
                                var codepoint = Convert.ToInt32(c);
                                if ((codepoint >= 32) && (codepoint <= 126))
                                {
                                    builder.Append(c);
                                }
                                else
                                {
                                    builder.Append("\\u");
                                    builder.Append(codepoint.ToString("x4"));
                                }
                                break;
                        }
                    }

                    builder.Append('\"');
                }

                void SerializeOther(object value)
                {
                    // Ensure we serialize Numbers using decimal (.) characters; avoid using comma (,) for
                    // decimal separator. Use CultureInfo.InvariantCulture.

                    // NOTE: decimals lose precision during serialization.
                    // They always have, I'm just letting you know.
                    // Previously floats and doubles lost precision too.
                    if (value is float)
                    {
                        builder.Append(((float)value).ToString("R", CultureInfo.InvariantCulture));
                    }
                    else if (value is int
                               || value is uint
                               || value is long
                               || value is sbyte
                               || value is byte
                               || value is short
                               || value is ushort
                               || value is ulong)
                    {
                        builder.Append(value);
                    }
                    else if (value is double
                               || value is decimal)
                    {
                        builder.Append(Convert.ToDouble(value).ToString("R", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        SerializeString(value.ToString());
                    }
                }
            }
        }

        // By Unity
        #region Extension methods

        /// <summary>
        /// Extension class for MiniJson to access values in JSON format.
        /// </summary>
        public static class MiniJsonExtensions
        {
            /// <summary>
            /// Get the HashDictionary of a key in JSON dictionary.
            /// </summary>
            /// <param name="dic">The JSON in dictionary representations.</param>
            /// <param name="key">The Key to get the HashDictionary from in the JSON dictionary.</param>
            /// <returns>The HashDictionary found in the JSON</returns>
            public static Dictionary<string, object> GetHash(this Dictionary<string, object> dic, string key)
            {
                return (Dictionary<string, object>)dic[key];
            }

            /// <summary>
            /// Get the casted enum in the JSON dictionary.
            /// </summary>
            /// <param name="dic">The JSON in dictionary representations.</param>
            /// <param name="key">The Key to get the casted enum from in the JSON dictionary.</param>
            /// <typeparam name="T">The class to cast the enum.</typeparam>
            /// <returns>The casted enum or will return T if the key was not found in the JSON dictionary.</returns>
            public static T GetEnum<T>(this Dictionary<string, object> dic, string key)
            {
                if (dic.ContainsKey(key))
                {
                    return (T)Enum.Parse(typeof(T), dic[key].ToString(), true);
                }

                return default;
            }

            /// <summary>
            /// Get the string in the JSON dictionary.
            /// </summary>
            /// <param name="dic">The JSON in dictionary representations.</param>
            /// <param name="key">The Key to get the string from in the JSON dictionary.</param>
            /// <param name="defaultValue">The default value to send back if the JSON dictionary doesn't contains the key.</param>
            /// <returns>The string from the JSON dictionary or the default value if there is none</returns>
            public static string GetString(this Dictionary<string, object> dic, string key, string defaultValue = "")
            {
                if (dic.ContainsKey(key))
                {
                    return dic[key].ToString();
                }

                return defaultValue;
            }

            /// <summary>
            /// Get the long in the JSON dictionary.
            /// </summary>
            /// <param name="dic">The JSON in dictionary representations.</param>
            /// <param name="key">The Key to get the long from in the JSON dictionary.</param>
            /// <returns>The long from the JSON dictionary or 0 if the key was not found in the JSON dictionary</returns>
            public static long GetLong(this Dictionary<string, object> dic, string key)
            {
                if (dic.ContainsKey(key))
                {
                    return long.Parse(dic[key].ToString());
                }

                return 0;
            }

            /// <summary>
            /// Get the list of strings in the JSON dictionary.
            /// </summary>
            /// <param name="dic">The JSON in dictionary representations.</param>
            /// <param name="key">The Key to get the list of strings from in the JSON dictionary.</param>
            /// <returns>The list of strings from the JSON dictionary or an empty list of strings if the key was not found in the JSON dictionary</returns>
            public static List<string> GetStringList(this Dictionary<string, object> dic, string key)
            {
                if (dic.ContainsKey(key))
                {
                    var result = new List<string>();
                    var objs = (List<object>)dic[key];
                    foreach (var v in objs)
                    {
                        result.Add(v.ToString());
                    }

                    return result;
                }

                return new List<string>();
            }

            /// <summary>
            /// Get the bool in the JSON dictionary.
            /// </summary>
            /// <param name="dic">The JSON in dictionary representations.</param>
            /// <param name="key">The Key to get the bool from in the JSON dictionary.</param>
            /// <returns>The bool from the JSON dictionary or false if the key was not found in the JSON dictionary</returns>
            public static bool GetBool(this Dictionary<string, object> dic, string key)
            {
                if (dic.ContainsKey(key))
                {
                    return bool.Parse(dic[key].ToString());
                }

                return false;
            }

            /// <summary>
            /// Get the casted object in the JSON dictionary.
            /// </summary>
            /// <param name="dic">The JSON in dictionary representations.</param>
            /// <param name="key">The Key to get the casted object from in the JSON dictionary.</param>
            /// <typeparam name="T">The class to cast the object.</typeparam>
            /// <returns>The casted object or will return T if the key was not found in the JSON dictionary.</returns>
            public static T Get<T>(this Dictionary<string, object> dic, string key)
            {
                if (dic.ContainsKey(key))
                {
                    return (T)dic[key];
                }

                return default;
            }

            /// <summary>
            /// Convert a Dictionary to JSON.
            /// </summary>
            /// <param name="obj">The dictionary to convert to JSON.</param>
            /// <returns>The converted dictionary in JSON string format.</returns>
            public static string toJson(this Dictionary<string, object> obj)
            {
                return MiniJson.JsonEncode(obj);
            }

            /// <summary>
            /// Convert a Dictionary to JSON.
            /// </summary>
            /// <param name="obj">The dictionary to convert to JSON.</param>
            /// <returns>The converted dictionary in JSON string format.</returns>
            public static string toJson(this Dictionary<string, string> obj)
            {
                return MiniJson.JsonEncode(obj);
            }

            /// <summary>
            /// Convert a string array to JSON.
            /// </summary>
            /// <param name="array">The string array to convert to JSON.</param>
            /// <returns>The converted dictionary in JSON string format.</returns>
            public static string toJson(this string[] array)
            {
                var list = new List<object>();
                foreach (var s in array)
                {
                    list.Add(s);
                }

                return MiniJson.JsonEncode(list);
            }

            /// <summary>
            /// Convert string JSON into List of Objects.
            /// </summary>
            /// <param name="json">String JSON to convert.</param>
            /// <returns>List of Objects converted from string json.</returns>
            public static List<object> ArrayListFromJson(this string json)
            {
                return MiniJson.JsonDecode(json) as List<object>;
            }

            /// <summary>
            /// Convert string JSON into Dictionary.
            /// </summary>
            /// <param name="json">String JSON to convert.</param>
            /// <returns>Dictionary converted from string json.</returns>
            public static Dictionary<string, object> HashtableFromJson(this string json)
            {
                return MiniJson.JsonDecode(json) as Dictionary<string, object>;
            }
        }

        #endregion
    }

    /// <summary>
    /// Extension class for MiniJson to Encode and Decode JSON.
    /// </summary>
    public class MiniJson
    {
        /// <summary>
        /// Converts an object into a JSON string
        /// </summary>
        /// <param name="json">Object to convert to JSON string</param>
        /// <returns>JSON string</returns>
        public static string JsonEncode(object json)
        {
            return MiniJSON.Json.Serialize(json);
        }

        /// <summary>
        /// Converts an string into a JSON object
        /// </summary>
        /// <param name="json">String to convert to JSON object</param>
        /// <returns>JSON object</returns>
        public static object JsonDecode(string json)
        {
            return MiniJSON.Json.Deserialize(json);
        }
    }
}
