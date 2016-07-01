using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace SMLogging
{
    /// <summary>
    /// Represents a <see cref="System.ComponentModel.TypeConverter"/> that converts an <see cref="System.Diagnostics.TraceSource"/> to and from a string value.
    /// </summary>
    public class TraceSourceConverter : TypeConverter
    {
        #region Public Methods

        /// <summary>
        /// Indicates whether this converter can convert an object of the given type to the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">
        /// An <see cref="System.ComponentModel.ITypeDescriptorContext"/> object that provides a format context that can
        /// be used to extract additional information about the environment from which this converter is invoked. This parameter or 
        /// properties of this parameter can be null.
        /// </param>
        /// <param name="sourceType">The <see cref="System.Type"/> to convert.</param>
        /// <returns>A value indicating whether this converter can perform the conversion.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Converts an <see cref="System.Diagnostics.TraceSource"/> from a string.
        /// </summary>
        /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">A <see cref="System.Globalization.CultureInfo"/> object. If null, the current culture is used.</param>
        /// <param name="value">The value to convert.</param>
        /// <returns>A <see cref="System.Diagnostics.TraceSource"/>.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null || !CanConvertFrom(value.GetType()))
                throw GetConvertFromException(value);

            TraceSource source = null;

            if (value != null)
            {
                source = new TraceSource((string)value);
            }

            return source;
        }

        /// <summary>
        /// Converts a <see cref="System.Diagnostics.TraceSource"/> to a string.
        /// </summary>
        /// <param name="context">An <see cref="System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">A <see cref="System.Globalization.CultureInfo"/> object. If null, the current culture is used.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="destinationType">The <see cref="System.Type"/> to convert to.</param>
        /// <returns>A string.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!CanConvertTo(context, destinationType))
                throw GetConvertToException(value, destinationType);

            var result = String.Empty;

            var source = value as TraceSource;
            if (source != null)
            {
                result = source.Name;
            }

            return result;
        }

        #endregion
    }
}
