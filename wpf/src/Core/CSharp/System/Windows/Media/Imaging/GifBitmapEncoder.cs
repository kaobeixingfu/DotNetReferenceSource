//------------------------------------------------------------------------------
//  Microsoft Avalon
//  Copyright (c) Microsoft Corporation, All Rights Reserved
//
//  File: GifBitmapEncoder.cs
//
//------------------------------------------------------------------------------

using System;
using System.Security;
using System.Security.Permissions;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using MS.Internal;
using MS.Win32.PresentationCore;
using System.Diagnostics;
using System.Windows.Media;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace System.Windows.Media.Imaging
{
    #region GifBitmapEncoder

    /// <summary>
    /// Built-in Encoder for Gif files.
    /// </summary>
    public sealed class GifBitmapEncoder : BitmapEncoder
    {
        #region Constructors

        /// <summary>
        /// Constructor for GifBitmapEncoder
        /// </summary>
        /// <SecurityNote>
        /// Critical - will eventuall create unmanaged resources
        /// PublicOK - all inputs are verified
        /// </SecurityNote>
        [SecurityCritical ]
        public GifBitmapEncoder() :
            base(true)
        {
            _supportsPreview = false;
            _supportsGlobalThumbnail = false;
            _supportsGlobalMetadata = false;
            _supportsFrameThumbnails = false;
            _supportsMultipleFrames = true;
            _supportsFrameMetadata = false;
        }

        #endregion

        #region Internal Properties / Methods

        /// <summary>
        /// Returns the container format for this encoder
        /// </summary>
        /// <SecurityNote>
        /// Critical - uses guid to create unmanaged resources
        /// </SecurityNote>
        internal override Guid ContainerFormat
        {
            [SecurityCritical]
            get
            {
                return _containerFormat;
            }
        }

        /// <summary>
        /// Setups the encoder and other properties before encoding each frame
        /// </summary>
        [SecurityCritical]
        internal override void SetupFrame(SafeMILHandle frameEncodeHandle, SafeMILHandle encoderOptions)
        {
            HRESULT.Check(UnsafeNativeMethods.WICBitmapFrameEncode.Initialize(
                frameEncodeHandle,
                encoderOptions
                ));
        }

        #endregion

        #region Internal Abstract

        /// Need to implement this to derive from the "sealed" object
        internal override void SealObject()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Data Members

        /// <SecurityNote>
        /// Critical - CLSID used for creation of critical resources
        /// </SecurityNote>
        [SecurityCritical]
        private Guid _containerFormat = MILGuidData.GUID_ContainerFormatGif;

        #endregion
    }

    #endregion // GifBitmapEncoder
}
