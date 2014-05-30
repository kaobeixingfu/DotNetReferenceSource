//---------------------------------------------------------------------------
//
// <copyright file="CacheModeConverter.cs" company="Microsoft">
//    Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// This file was generated, please do not edit it directly.
//
// Please see http://wiki/default.aspx/Microsoft.Projects.Avalon/MilCodeGen.html for more information.
//
//---------------------------------------------------------------------------

using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.Collections;
using MS.Internal.PresentationCore;
using MS.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel.Design.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Windows.Media.Converters;
using System.Security;
using System.Security.Permissions;
using SR=MS.Internal.PresentationCore.SR;
using SRID=MS.Internal.PresentationCore.SRID;
#pragma warning disable 1634, 1691  // suppressing PreSharp warnings

namespace System.Windows.Media
{
    /// <summary>
    /// CacheModeConverter - Converter class for converting instances of other types to and from CacheMode instances
    /// </summary>
    public sealed class CacheModeConverter : TypeConverter
    {
        /// <summary>
        /// Returns true if this type converter can convert from a given type.
        /// </summary>
        /// <returns>
        /// bool - True if this converter can convert from the provided type, false if not.
        /// </returns>
        /// <param name="context"> The ITypeDescriptorContext for this call. </param>
        /// <param name="sourceType"> The Type being queried for support. </param>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Returns true if this type converter can convert to the given type.
        /// </summary>
        /// <returns>
        /// bool - True if this converter can convert to the provided type, false if not.
        /// </returns>
        /// <param name="context"> The ITypeDescriptorContext for this call. </param>
        /// <param name="destinationType"> The Type being queried for support. </param>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {

                // When invoked by the serialization engine we can convert to string only for some instances
                if (context != null && context.Instance != null)
                {
                    if (!(context.Instance is CacheMode))
                    {
                        throw new ArgumentException(SR.Get(SRID.General_Expected_Type, "CacheMode"), "context");
                    }

                    CacheMode value = (CacheMode)context.Instance;

                    #pragma warning suppress 6506 // value is obviously not null
                    return value.CanSerializeToString();
                }

                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        /// Attempts to convert to a CacheMode from the given object.
        /// </summary>
        /// <returns>
        /// The CacheMode which was constructed.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// A NotSupportedException is thrown if the example object is null or is not a valid type
        /// which can be converted to a CacheMode.
        /// </exception>
        /// <param name="context"> The ITypeDescriptorContext for this call. </param>
        /// <param name="culture"> The requested CultureInfo.  Note that conversion uses "en-US" rather than this parameter. </param>
        /// <param name="value"> The object to convert to an instance of CacheMode. </param>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                throw GetConvertFromException(value);
            }

            String source = value as string;

            if (source != null)
            {
                return CacheMode.Parse(source);
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// ConvertTo - Attempt to convert an instance of CacheMode to the given type
        /// </summary>
        /// <returns>
        /// The object which was constructoed.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// A NotSupportedException is thrown if "value" is null or not an instance of CacheMode,
        /// or if the destinationType isn't one of the valid destination types.
        /// </exception>
        /// <param name="context"> The ITypeDescriptorContext for this call. </param>
        /// <param name="culture"> The CultureInfo which is respected when converting. </param>
        /// <param name="value"> The object to convert to an instance of "destinationType". </param>
        /// <param name="destinationType"> The type to which this will convert the CacheMode instance. </param>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != null && value is CacheMode)
            {
                CacheMode instance = (CacheMode)value;

                if (destinationType == typeof(string))
                {

                    // When invoked by the serialization engine we can convert to string only for some instances
                    if (context != null && context.Instance != null)
                    {
                        #pragma warning suppress 6506 // instance is obviously not null
                        if (!instance.CanSerializeToString())
                        {
                            throw new NotSupportedException(SR.Get(SRID.Converter_ConvertToNotSupported));
                        }
                    }

                    // Delegate to the formatting/culture-aware ConvertToString method.

                    #pragma warning suppress 6506 // instance is obviously not null
                    return instance.ConvertToString(null, culture);
                }
            }

            // Pass unhandled cases to base class (which will throw exceptions for null value or destinationType.)
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

}
