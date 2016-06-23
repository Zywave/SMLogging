using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SMRequestLogging
{
    /// <summary>
    /// <see cref="System.String"/> helper and extension methods.
    /// </summary>
    public static class StringHelpers
    {
        #region Public Methods

        /// <summary>
        /// Concatenates the members of a collection, using the specified separator between each member.
        /// </summary>
        /// <typeparam name="T">The type of the members of <paramref name="values"/>.</typeparam>
        /// <param name="separator">The string to use as a separator.</param>
        /// <param name="values">A collection that contains the objects to concatenate.</param>
        /// <returns>
        /// A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/> 
        /// string. If <paramref name="values"/> has no members, the method returns <see cref="System.String.Empty"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="values"/> is <see langword="null"/>.</exception>
        public static string Join<T>(string separator, IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            if (separator == null)
            {
                separator = string.Empty;
            }

            using (var enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return string.Empty;
                }

                var sb = new StringBuilder();
                if (enumerator.Current != null)
                {
                    var str = enumerator.Current.ToString();
                    if (str != null)
                    {
                        sb.Append(str);
                    }
                }
                while (enumerator.MoveNext())
                {
                    sb.Append(separator);
                    if (enumerator.Current != null)
                    {
                        var str = enumerator.Current.ToString();
                        if (str != null)
                        {
                            sb.Append(str);
                        }
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Replaces the format items in a specified <see cref="System.String"/> with the text equivalent of the values of a specified 
        /// object array. The format specification is enhanced to support max length, substring and conditional suffixes, prefixes or 
        /// replacement content.
        /// </summary>
        /// <remarks>
        /// The syntax of a format item is as follows: 
        /// {index[*joinConnector[,joinTwoPartConnector[;joinLastConnector]]][,minLength][;maxLength][:formatString][|startIndex[,length]][?conditionalPrefix,conditionalSuffix[:conditionalContent]]}
        /// </remarks>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        /// <returns>
        /// A copy of <paramref name="format"/> in which the format items have been replaced by the <see cref="System.String"/> equivalent of the 
        /// corresponding objects in <paramref name="args"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="format"/> or <paramref name="args"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.FormatException">Throw if the <paramref name="format"/> is invalid.</exception>
        public static string Format(string format, params object[] args)
        {
            return Format(null, format, args);
        }

        /// <summary>
        /// Replaces the format items in a specified <see cref="System.String"/> with the text equivalent of the values of a specified 
        /// object array. The format specification is enhanced to support max length, substring and conditional suffixes, prefixes or 
        /// replacement content.  Uses <see cref="System.Globalization.CultureInfo.InvariantCulture"/> as a format provider.
        /// </summary>
        /// <remarks>
        /// The syntax of a format item is as follows: 
        /// {index[*joinConnector[,joinTwoPartConnector[;joinLastConnector]]][,minLength][;maxLength][:formatString][|startIndex[,length]][?conditionalPrefix,conditionalSuffix[:conditionalContent]]}
        /// </remarks>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        /// <returns>
        /// A copy of <paramref name="format"/> in which the format items have been replaced by the <see cref="System.String"/> equivalent of the 
        /// corresponding objects in <paramref name="args"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="format"/> or <paramref name="args"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.FormatException">Throw if the <paramref name="format"/> is invalid.</exception>
        public static string FormatInvariant(string format, params object[] args)
        {
            return Format(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Replaces the format items in a specified <see cref="System.String"/> with the text equivalent of the values of a specified 
        /// object array. The format specification is enhanced to support max length, substring and conditional suffixes, prefixes or 
        /// replacement content.
        /// </summary>
        /// <remarks>
        /// The syntax of a format item is as follows: 
        /// {index[*joinConnector[,joinTwoPartConnector[;joinLastConnector]]][,minLength][;maxLength][:formatString][|startIndex[,length]][?conditionalPrefix,conditionalSuffix[:conditionalContent]]}
        /// </remarks>
        /// <param name="provider">An <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        /// <returns>
        /// A copy of <paramref name="format"/> in which the format items have been replaced by the <see cref="System.String"/> equivalent of the 
        /// corresponding objects in <paramref name="args"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="format"/> or <paramref name="args"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.FormatException">Throw if the <paramref name="format"/> is invalid.</exception>
        public static string Format(IFormatProvider provider, string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("format");
            if (args == null)
                throw new ArgumentNullException("args");

            return _formatArgumentPattern.Replace(
                format, 
                delegate(Match match)
                    {
                        int argIndex;
                        if (!Int32.TryParse(match.Groups["Key"].Value, out argIndex))
                            throw new FormatException();
                        if (argIndex < 0 || argIndex >= args.Length)
                            throw new FormatException();
                        var arg = args[argIndex];
                        return FormatArgument(provider, match, arg);
                    });
        }

        /// <summary>
        /// Replaces the named format items in a specified <see cref="System.String"/> with the text equivalent of the values of a specified 
        /// object array. The format specification is enhanced to support max length, substring and conditional suffixes, prefixes or 
        /// replacement content.
        /// </summary>
        /// <remarks>
        /// The syntax of a format item is as follows: 
        /// {name[*joinConnector[,joinTwoPartConnector[;joinLastConnector]]][,minLength][;maxLength][:formatString][|startIndex[,length]][?conditionalPrefix,conditionalSuffix[:conditionalContent]]}
        /// </remarks>
        /// <param name="format">A composite format string.</param>
        /// <param name="namedArgs">An object whose properties are to represent the named objects to format.</param>
        /// <returns>
        /// A copy of <paramref name="format"/> in which the named format items have been replaced by the <see cref="System.String"/> equivalent of the 
        /// corresponding objects in <paramref name="namedArgs"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="format"/> or <paramref name="namedArgs"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.FormatException">Throw if the <paramref name="format"/> is invalid.</exception>
        public static string NamedFormat(string format, object namedArgs)
        {
            return NamedFormat(null, format, namedArgs);
        }

        /// <summary>
        /// Replaces the named format items in a specified <see cref="System.String"/> with the text equivalent of the values of a specified 
        /// object array. The format specification is enhanced to support max length, substring and conditional suffixes, prefixes or 
        /// replacement content.  Uses <see cref="System.Globalization.CultureInfo.InvariantCulture"/> as a format provider.
        /// </summary>
        /// <remarks>
        /// The syntax of a format item is as follows: 
        /// {name[*joinConnector[,joinTwoPartConnector[;joinLastConnector]]][,minLength][;maxLength][:formatString][|startIndex[,length]][?conditionalPrefix,conditionalSuffix[:conditionalContent]]}
        /// </remarks>
        /// <param name="format">A composite format string.</param>
        /// <param name="namedArgs">An object whose properties are to represent the named objects to format.</param>
        /// <returns>
        /// A copy of <paramref name="format"/> in which the named format items have been replaced by the <see cref="System.String"/> equivalent of the 
        /// corresponding objects in <paramref name="namedArgs"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="format"/> or <paramref name="namedArgs"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.FormatException">Throw if the <paramref name="format"/> is invalid.</exception>
        public static string NamedFormatInvariant(string format, object namedArgs)
        {
            return NamedFormat(CultureInfo.InvariantCulture, format, namedArgs);
        }

        /// <summary>
        /// Replaces the named format items in a specified <see cref="System.String"/> with the text equivalent of the values of a specified 
        /// object array. The format specification is enhanced to support max length, substring and conditional suffixes, prefixes or 
        /// replacement content.
        /// </summary>
        /// <remarks>
        /// The syntax of a format item is as follows: 
        /// {name[*joinConnector[,joinTwoPartConnector[;joinLastConnector]]][,minLength][;maxLength][:formatString][|startIndex[,length]][?conditionalPrefix,conditionalSuffix[:conditionalContent]]}
        /// </remarks>
        /// <param name="provider">An <see cref="System.IFormatProvider"/> that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="namedArgs">An object whose properties are to represent the named objects to format.</param>
        /// <returns>
        /// A copy of <paramref name="format"/> in which the named format items have been replaced by the <see cref="System.String"/> equivalent of the 
        /// corresponding objects in <paramref name="namedArgs"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="format"/> or <paramref name="namedArgs"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.FormatException">Throw if the <paramref name="format"/> is invalid.</exception>
        public static string NamedFormat(IFormatProvider provider, string format, object namedArgs)
        {
            if (format == null)
                throw new ArgumentNullException("format");
            if (namedArgs == null)
                throw new ArgumentNullException("namedArgs");

            IDictionary<string, object> args = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(namedArgs))
            {
                args.Add(descriptor.Name, descriptor.GetValue(namedArgs));
            }

            return NamedFormat(provider, format, args);
        }

        /// <summary>
        /// Replaces the named format items in a specified <see cref="System.String"/> with the text equivalent of the values of a specified 
        /// object array. The format specification is enhanced to support max length, substring and conditional suffixes, prefixes or 
        /// replacement content.
        /// </summary>
        /// <remarks>
        /// The syntax of a format item is as follows: 
        /// {name[*joinConnector[,joinTwoPartConnector[;joinLastConnector]]][,minLength][;maxLength][:formatString][|startIndex[,length]][?conditionalPrefix,conditionalSuffix[:conditionalContent]]}
        /// </remarks>
        /// <param name="format">A composite format string.</param>
        /// <param name="namedArgs">A dictionary containing the objects to format, the key representing the name.</param>
        /// <returns>
        /// A copy of <paramref name="format"/> in which the named format items have been replaced by the <see cref="System.String"/> equivalent of the 
        /// corresponding objects in <paramref name="namedArgs"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="format"/> or <paramref name="namedArgs"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.FormatException">Throw if the <paramref name="format"/> is invalid.</exception>
        public static string NamedFormat(string format, IDictionary<string, object> namedArgs)
        {
            return NamedFormat(null, format, namedArgs);
        }

        /// <summary>
        /// Replaces the named format items in a specified <see cref="System.String" /> with the text equivalent of the values of a specified
        /// object array. The format specification is enhanced to support max length, substring and conditional suffixes, prefixes or
        /// replacement content.
        /// </summary>
        /// <param name="provider">An <see cref="System.IFormatProvider" /> that supplies culture-specific formatting information.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="namedArgs">A dictionary containing the objects to format, the key representing the name.</param>
        /// <param name="ignoreMissingArgs">Ignore missing arguments if set to <c>true</c> .</param>
        /// <returns>
        /// A copy of <paramref name="format" /> in which the named format items have been replaced by the <see cref="System.String" /> equivalent of the
        /// corresponding objects in <paramref name="namedArgs" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="format" /> or <paramref name="namedArgs" /> is <see langword="null" />.</exception>
        /// <exception cref="System.FormatException">Throw if the <paramref name="format" /> is invalid.</exception>
        /// <remarks>
        /// The syntax of a format item is as follows:
        /// {name[*joinConnector[,joinTwoPartConnector[;joinLastConnector]]][,minLength][;maxLength][:formatString][|startIndex[,length]][?conditionalPrefix,conditionalSuffix[:conditionalContent]]}
        /// </remarks>
        public static string NamedFormat(IFormatProvider provider, string format, IDictionary<string, object> namedArgs, bool ignoreMissingArgs = false)
        {
            if (format == null)
                throw new ArgumentNullException("format");
            if (namedArgs == null)
                throw new ArgumentNullException("namedArgs");

            return _formatArgumentPattern.Replace(
                format, 
                delegate(Match match)
                    {
                        var argName = match.Groups["Key"].Value;
                        if (!namedArgs.ContainsKey(argName))
                        {
                            if (ignoreMissingArgs)
                            {
                                return match.Value;
                            }
                            throw new FormatException();
                        }
                        var arg = namedArgs[argName];
                        return FormatArgument(provider, match, arg);
                    });
        }
        
        /// <summary>
        /// Determines whether this string starts with any of the specified <see cref="System.String"/> objects.
        /// </summary>
        /// <param name="target">The target string.</param>
        /// <param name="values">The <see cref="System.String"/> objects to compare.</param>
        /// <returns>A value indicating whether this string starts with any of the <paramref name="values"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="target"/> or <paramref name="values"/> is <see langword="null"/>.</exception>
        public static bool StartsWithAny(this string target, params string[] values)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (values == null)
                throw new ArgumentNullException("values");

            return values.Any(arg => target.StartsWith(arg, false, null));
        }
       
        #endregion

        #region Private Methods

        private static string FormatArgument(IFormatProvider provider, Match match, object arg)
        {
            string result = null;

            var formatter = provider != null ? (ICustomFormatter)provider.GetFormat(typeof(ICustomFormatter)) : null;

            if (match.Groups["Join"].Success)
            {
                var enumerableArg = arg as IEnumerable;
                if (enumerableArg != null)
                {
                    var connector = String.Empty;
                    if (match.Groups["Connector"].Success)
                    {
                        connector = match.Groups["Connector"].Value;
                    }
                    var twoPartConnector = connector;
                    if (match.Groups["TwoPartConnector"].Success)
                    {
                        twoPartConnector = match.Groups["TwoPartConnector"].Value;
                    }
                    var lastConnector = twoPartConnector;
                    if (match.Groups["LastConnector"].Success)
                    {
                        lastConnector = match.Groups["LastConnector"].Value;
                    }
                    
                    result = String.Empty;
                    var currentIndex = 0;
                    var enumerator = enumerableArg.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        while (true)
                        {
                            var argText = FormatToString(enumerator.Current, match.Groups["Format"].Value, provider, formatter);
                            var next = enumerator.MoveNext();
                            if (!string.IsNullOrWhiteSpace(argText))
                            {
                                if (currentIndex == 0)
                                {
                                    result = String.Concat(result, argText);
                                }
                                else if (currentIndex > 0 && next)
                                {
                                    result = String.Concat(result, connector, argText);
                                }
                                else if (!next && currentIndex == 1)
                                {
                                    result = String.Concat(result, twoPartConnector, argText);
                                }
                                else
                                {
                                    result = String.Concat(result, lastConnector, argText);
                                }
                            }

                            if (!next)
                            {
                                break;
                            }

                            currentIndex++;
                        }
                    }
                }
            }

            if (result == null)
            {
                result = FormatToString(arg, match.Groups["Format"].Value, provider, formatter);
            }

            if (match.Groups["MinLength"].Success)
            {
                var argMinLength = Int32.Parse(match.Groups["MinLength"].Value, CultureInfo.InvariantCulture);
                if (argMinLength > 0)
                {
                    result = result.PadRight(argMinLength);
                }
                else if (argMinLength < 0)
                {
                    result = result.PadLeft(Math.Abs(argMinLength));
                }
            }

            if (match.Groups["MaxLength"].Success)
            {
                var argMaxLength = Int32.Parse(match.Groups["MaxLength"].Value, CultureInfo.InvariantCulture);
                if (argMaxLength > 0 && argMaxLength <= result.Length)
                {
                    result = result.Substring(0, argMaxLength);
                }
                else if (argMaxLength < 0 && Math.Abs(argMaxLength) <= result.Length)
                {
                    result = result.Substring(result.Length + argMaxLength);
                }
            }

            if (match.Groups["Substring"].Success)
            {
                var argStartIndex = Int32.Parse(match.Groups["StartIndex"].Value, CultureInfo.InvariantCulture);
                var argLength = -1;
                if (match.Groups["Length"].Success)
                {
                    argLength = Int32.Parse(match.Groups["Length"].Value, CultureInfo.InvariantCulture);
                }
                if (argStartIndex >= 0 && argStartIndex < result.Length)
                {
                    if (argLength >= 0 && argStartIndex + argLength < result.Length)
                    {
                        result = result.Substring(argStartIndex, argLength);
                    }
                    else
                    {
                        result = result.Substring(argStartIndex);
                    }
                }
                else
                {
                    result = String.Empty;
                }
            }

            if (match.Groups["Conditional"].Success)
            {
                var prefix = String.Empty;
                if (match.Groups["Prefix"].Success)
                {
                    prefix = match.Groups["Prefix"].Value;
                }
                var suffix = String.Empty;
                if (match.Groups["Suffix"].Success)
                {
                    suffix = match.Groups["Suffix"].Value;
                }
                var content = String.Empty;
                if (match.Groups["Content"].Success)
                {
                    content = match.Groups["Content"].Value;
                }
                if (String.IsNullOrEmpty(result))
                {
                    result = content;
                }
                else
                {
                    result = prefix + result + suffix;
                }
            }
            
            return result;
        }

        private static string FormatToString(object value, string format, IFormatProvider provider, ICustomFormatter formatter)
        {
            string result = null;

            if (!String.IsNullOrWhiteSpace(format))
            {
                if (formatter != null)
                {
                    result = formatter.Format(format, value, provider);
                }
                if (result == null)
                {
                    var formattableArg = value as IFormattable;
                    if (formattableArg != null)
                    {
                        result = formattableArg.ToString(format, provider);
                    }
                }
            }
            if (result == null && value != null)
            {
                result = value.ToString();
            }

            return result ?? String.Empty;
        }

        #endregion

        #region Private Constants

        private static readonly Regex _formatArgumentPattern =
            new Regex(
                @"{(?<Key>\w+)(?<Join>\*(?:""""|""(?<Connector>(?:.|\\"")*?[^\\])"")?(?:,(?:""""|""(?<TwoPartConnector>(?:.|\\"")*?[^\\])""))?(?:;(?:""""|""(?<LastConnector>(?:.|\\"")*?[^\\])""))?)?(?:,(?<MinLength>-?\d+))?(?:;(?<MaxLength>-?\d+))?(?:\:(?<Format>.+?))?(?<Substring>\|(?<StartIndex>\d+)(?:,(?<Length>\d+))?)?(?<Conditional>\?(?:""""|(?:""(?<Prefix>(?:.|\\"")*?[^\\])""))?(?:,(?:""""|""(?<Suffix>(?:.|\\"")*?[^\\])""))?(?:\:(?:""""|""(?<Content>(?:.|\\"")*?[^\\])""))?)?}", 
                RegexOptions.Compiled);

        #endregion
    }
}