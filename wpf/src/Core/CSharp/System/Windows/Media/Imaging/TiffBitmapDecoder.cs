//------------------------------------------------------------------------------
//  Microsoft Avalon
//  Copyright (c) Microsoft Corporation, All Rights Reserved
//
//  File: TiffBitmapDecoder.cs
//
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using Microsoft.Win32.SafeHandles;
using MS.Internal;
using System.Diagnostics;
using System.Windows.Media;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Media.Imaging
{
    #region TiffBitmapDecoder

    /// <summary>
    /// The built-in Microsoft Tiff (Bitmap) Decoder.
    /// </summary>
    public sealed class TiffBitmapDecoder : BitmapDecoder
    {
        /// <summary>
        /// Don't allow construction of a decoder with no params
        /// </summary>
        private TiffBitmapDecoder()
        {
        }

        /// <summary>
        /// Create a TiffBitmapDecoder given the Uri
        /// </summary>
        /// <param name="bitmapUri">Uri to decode</param>
        /// <param name="createOptions">Bitmap Create Options</param>
        /// <param name="cacheOption">Bitmap Caching Option</param>
        /// <SecurityNote>
        /// Critical - access critical resource
        /// PublicOK - inputs verified or safe
        /// </SecurityNote>
        [SecurityCritical]
        public TiffBitmapDecoder(
            Uri bitmapUri,
            BitmapCreateOptions createOptions,
            BitmapCacheOption cacheOption
            ) : base(bitmapUri, createOptions, cacheOption, MILGuidData.GUID_ContainerFormatTiff)
        {
        }

        /// <summary>
        /// If this decoder cannot handle the bitmap stream, it will throw an exception.
        /// </summary>
        /// <param name="bitmapStream">Stream to decode</param>
        /// <param name="createOptions">Bitmap Create Options</param>
        /// <param name="cacheOption">Bitmap Caching Option</param>
        /// <SecurityNote>
        /// Critical - access critical resource
        /// PublicOK - inputs verified or safe
        /// </SecurityNote>
        [SecurityCritical ]
        public TiffBitmapDecoder(
            Stream bitmapStream,
            BitmapCreateOptions createOptions,
            BitmapCacheOption cacheOption
            ) : base(bitmapStream, createOptions, cacheOption, MILGuidData.GUID_ContainerFormatTiff)
        {
        }

        /// <summary>
        /// Internal Constructor
        /// </summary>
        /// <SecurityNote>
        /// Critical: Uses a SafeFileHandle, which is a SecurityCritical type (in v4).
        ///     Calls SecurityCritical base class constructor.
        /// </SecurityNote>
        [SecurityCritical]
        internal TiffBitmapDecoder(
            SafeMILHandle decoderHandle,
            BitmapDecoder decoder,
            Uri baseUri,
            Uri uri,
            Stream stream,
            BitmapCreateOptions createOptions,
            BitmapCacheOption cacheOption,
            bool insertInDecoderCache,
            bool originalWritable,
            Stream uriStream,
            UnmanagedMemoryStream unmanagedMemoryStream,
            SafeFileHandle safeFilehandle
            ) : base(decoderHandle, decoder, baseUri, uri, stream, createOptions, cacheOption, insertInDecoderCache, originalWritable, uriStream, unmanagedMemoryStream, safeFilehandle)
        {
        }

        /// <summary>
        /// Returns whether metadata is fixed size or not.
        /// </summary>
        internal override bool IsMetadataFixedSize
        {
            get
            {
                return true;
            }
        }

        #region Internal Abstract

        /// Need to implement this to derive from the "sealed" object
        internal override void SealObject()
        {
            throw new NotImplementedException();
        }

        #endregion

    }

    #endregion
}

