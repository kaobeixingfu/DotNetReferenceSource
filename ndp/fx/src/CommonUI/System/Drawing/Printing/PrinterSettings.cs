//------------------------------------------------------------------------------
// <copyright file="PrinterSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Drawing.Printing {
    using System.Runtime.Serialization.Formatters;
    using System.Configuration.Assemblies;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;    
    using System;    
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Drawing;
    using System.Drawing.Internal;
    using System.Drawing.Imaging;
    using System.ComponentModel;
    using Microsoft.Win32;
    using System.Globalization;
    using System.Runtime.Versioning;

    /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings"]/*' />
    /// <devdoc>
    ///    Information about how a document should be printed, including which printer
    ///    to print it on.
    /// </devdoc>
    [Serializable]
    public class PrinterSettings : ICloneable {
        // All read/write data is stored in managed code, and whenever we need to call Win32,
        // we create new DEVMODE and DEVNAMES structures.  We don't store device capabilities,
        // though.
        //
        // Also, all properties have hidden tri-state logic -- yes/no/default
        private const int PADDING_IA64 = 4;

        private string printerName; // default printer.
        private string driverName = "";
        private string outputPort = "";
        private bool printToFile;

        // Whether the PrintDialog has been shown (not whether it's currently shown).  This is how we enforce SafePrinting.
        private bool printDialogDisplayed;

        private short extrabytes;
        private byte[] extrainfo;

        private short copies = -1;
        private Duplex duplex = System.Drawing.Printing.Duplex.Default;
        private TriState collate = TriState.Default;
        private PageSettings defaultPageSettings;
        private int fromPage;
        private int toPage;
        private int maxPage = 9999;
        private int minPage;
        private PrintRange printRange;

        private short  devmodebytes;
        private byte[] cachedDevmode;

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterSettings"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Drawing.Printing.PrinterSettings'/> class.
        ///    </para>
        /// </devdoc>
        public PrinterSettings() {
            defaultPageSettings = new PageSettings(this);
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.CanDuplex"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the printer supports duplex (double-sided) printing.
        ///    </para>
        /// </devdoc>
        public bool CanDuplex {
            get { return DeviceCapabilities(SafeNativeMethods.DC_DUPLEX, IntPtr.Zero, 0) == 1;}
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.Copies"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the number of copies to print.
        ///    </para>
        /// </devdoc>
        public short Copies {
            get {
                if (copies != -1)
                    return copies;
                else
                    return GetModeField(ModeField.Copies, 1);
            }
            set {
                if (value < 0)
                    throw new ArgumentException(SR.GetString(SR.InvalidLowBoundArgumentEx,
                                                             "value", value.ToString(CultureInfo.CurrentCulture), 
                                                             (0).ToString(CultureInfo.CurrentCulture)));
                /*
                We shouldnt allow copies to be set since the copies can be a large number 
                and can be reflected in PrintDialog. So for the Copies property,
                we prefer that for SafePrinting, copied cannot be set programmatically 
                but through the print dialog. 
                Any lower security could set copies to anything. Vs Whidbey 93475*/
                IntSecurity.SafePrinting.Demand();
                copies = value;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.Collate"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       a value indicating whether the print out is collated.
        ///    </para>
        /// </devdoc>
        public bool Collate {
            get {
                if (!collate.IsDefault)
                    return(bool) collate;
                else
                    return GetModeField(ModeField.Collate, SafeNativeMethods.DMCOLLATE_FALSE) == SafeNativeMethods.DMCOLLATE_TRUE;
            }
            set { collate = value;}
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.DefaultPageSettings"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the default page settings for this printer.
        ///    </para>
        /// </devdoc>
        public PageSettings DefaultPageSettings {
            get { return defaultPageSettings;}
        }

        // As far as I can tell, Windows no longer pays attention to driver names and output ports.
        // But I'm leaving this code in place in case I'm wrong.
        internal string DriverName {
            get { return driverName;}
            // set { driverName = value;}
        }

        /* // No point in having a driver version if you can't get the driver name
        /// <summary>
        ///    <para>
        ///       Gets the printer driver version number.
        ///    </para>
        /// </summary>
        /// <value>
        ///    <para>
        ///       The printer driver version number.
        ///    </para>
        /// </value>
        public int DriverVersion {
            get { return DeviceCapabilities(SafeNativeMethods.DC_DRIVER, 0, -1);}
        }
        */

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.Duplex"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the printer's duplex setting.
        ///    </para>
        /// </devdoc>
        public Duplex Duplex {
            get {
                if (duplex != Duplex.Default)
                    return duplex;
                else
                    return(Duplex) GetModeField(ModeField.Duplex, SafeNativeMethods.DMDUP_SIMPLEX);
            }            
            set { 
                //valid values are 0xffffffff to 0x3
                if (!ClientUtils.IsEnumValid(value, unchecked((int)value), unchecked((int)Duplex.Default), unchecked((int)Duplex.Horizontal)))
                {
                    throw new InvalidEnumArgumentException("value", unchecked((int)value), typeof(Duplex));
                }
                duplex = value;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.FromPage"]/*' />
        /// <devdoc>
        ///    <para>Gets or sets the first page to print.</para>
        /// </devdoc>
        public int FromPage {
            get { return fromPage;}
            set {
                if (value < 0)
                    throw new ArgumentException(SR.GetString(SR.InvalidLowBoundArgumentEx,
                                                             "value", value.ToString(CultureInfo.CurrentCulture), 
                                                             (0).ToString(CultureInfo.CurrentCulture)));
                fromPage = value;
            }
        }

        

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.InstalledPrinters"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the names of all printers installed on the machine.
        ///    </para>
        /// </devdoc>
        public static StringCollection InstalledPrinters {
            get {
                IntSecurity.AllPrinting.Demand();

                int returnCode;
                int bufferSize;
                int count;
                int level, sizeofstruct;
                // Note: Level 5 doesn't seem to work properly on NT platforms
                // (atleast the call to get the size of the buffer reqd.),
                // and Level 4 doesn't work on Win9x.
                //
                if (Environment.OSVersion.Platform == System.PlatformID.Win32NT) {
                    level = 4;
                    // PRINTER_INFO_4 are 12 bytes in size
                    if (IntPtr.Size == 8) {
                        sizeofstruct = (IntPtr.Size * 2) + (Marshal.SizeOf(typeof(int)) * 1) + PADDING_IA64;
                    }
                    else {
                        sizeofstruct = (IntPtr.Size * 2) + (Marshal.SizeOf(typeof(int)) * 1);
                    }
                }
                else {
                    level = 5;
                    // PRINTER_INFO_5 are 20 bytes in size
                    sizeofstruct = (IntPtr.Size * 2) + (Marshal.SizeOf(typeof(int)) * 3);
                }
                string[] array;

                IntSecurity.UnmanagedCode.Assert();
                try {
                    SafeNativeMethods.EnumPrinters(SafeNativeMethods.PRINTER_ENUM_LOCAL | SafeNativeMethods.PRINTER_ENUM_CONNECTIONS, null, level, IntPtr.Zero, 0, out bufferSize, out count);

                    IntPtr buffer = Marshal.AllocCoTaskMem(bufferSize);
                    returnCode = SafeNativeMethods.EnumPrinters(SafeNativeMethods.PRINTER_ENUM_LOCAL | SafeNativeMethods.PRINTER_ENUM_CONNECTIONS,
                                                            null, level, buffer,
                                                            bufferSize, out bufferSize, out count);
                    array = new string[count];
                    
                    if (returnCode == 0) {
                        Marshal.FreeCoTaskMem(buffer);
                        throw new Win32Exception();
                    }

                    for (int i = 0; i < count; i++) {
                        // The printer name is at offset 0
                        //
                        IntPtr namePointer = (IntPtr) Marshal.ReadIntPtr((IntPtr)(checked((long)buffer + i * sizeofstruct)));
                        array[i] = Marshal.PtrToStringAuto(namePointer);
                    }

                    Marshal.FreeCoTaskMem(buffer);
                }
                finally {
                    CodeAccessPermission.RevertAssert();
                }

                return new StringCollection(array);
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.IsDefaultPrinter"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the <see cref='System.Drawing.Printing.PrinterSettings.PrinterName'/>
        ///       property designates the default printer.
        ///    </para>
        /// </devdoc>
        public bool IsDefaultPrinter {
            get {
                return (printerName == null || printerName == GetDefaultPrinterName());
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.IsPlotter"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the printer is a plotter, as opposed to a raster printer.
        ///    </para>
        /// </devdoc>
        public bool IsPlotter {
            get {
                return GetDeviceCaps(SafeNativeMethods.TECHNOLOGY, SafeNativeMethods.DT_RASPRINTER) == SafeNativeMethods.DT_PLOTTER;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.IsValid"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the <see cref='System.Drawing.Printing.PrinterSettings.PrinterName'/>
        ///       property designates a valid printer.
        ///    </para>
        /// </devdoc>
        public bool IsValid {
            get {
                return DeviceCapabilities(SafeNativeMethods.DC_COPIES, IntPtr.Zero, -1) != -1;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.LandscapeAngle"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the angle, in degrees, which the portrait orientation is rotated
        ///       to produce the landscape orientation.
        ///    </para>
        /// </devdoc>
        public int LandscapeAngle {
            get { return DeviceCapabilities(SafeNativeMethods.DC_ORIENTATION, IntPtr.Zero, 0);}
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.MaximumCopies"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the maximum number of copies allowed by the printer.
        ///    </para>
        /// </devdoc>
        public int MaximumCopies {
            get { return DeviceCapabilities(SafeNativeMethods.DC_COPIES, IntPtr.Zero, 1);}
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.MaximumPage"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the highest <see cref='System.Drawing.Printing.PrinterSettings.FromPage'/> or <see cref='System.Drawing.Printing.PrinterSettings.ToPage'/>
        ///       which may be selected in a print dialog box.
        ///    </para>
        /// </devdoc>
        public int MaximumPage {
            get { return maxPage;}
            set {
                if (value < 0)
                    throw new ArgumentException(SR.GetString(SR.InvalidLowBoundArgumentEx,
                                                             "value", value.ToString(CultureInfo.CurrentCulture), 
                                                             (0).ToString(CultureInfo.CurrentCulture)));
                maxPage = value;
            }

        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.MinimumPage"]/*' />
        /// <devdoc>
        /// <para>Gets or sets the lowest <see cref='System.Drawing.Printing.PrinterSettings.FromPage'/> or <see cref='System.Drawing.Printing.PrinterSettings.ToPage'/>
        /// which may be selected in a print dialog box.</para>
        /// </devdoc>
        public int MinimumPage {
            get { return minPage;}
            set {
                if (value < 0)
                    throw new ArgumentException(SR.GetString(SR.InvalidLowBoundArgumentEx,
                                                             "value", value.ToString(CultureInfo.CurrentCulture), 
                                                             (0).ToString(CultureInfo.CurrentCulture)));
                minPage = value;
            }
        }

        internal string OutputPort {
            get { 
                return outputPort;
                }
            set { 
                outputPort = value;
                }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrintFileName"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Indicates the name of the printerfile.
        ///    </para>
        /// </devdoc>
        public string PrintFileName {
            get {
                string printFileName = OutputPort;
                if (!string.IsNullOrEmpty(printFileName))
                {
                    IntSecurity.DemandReadFileIO(printFileName);
                }
                return printFileName;
            }
            set {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(value);
                }
                IntSecurity.DemandWriteFileIO(value);
                OutputPort = value;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSizes"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the paper sizes supported by this printer.
        ///    </para>
        /// </devdoc>
        public PaperSizeCollection PaperSizes {
            get { return new PaperSizeCollection(Get_PaperSizes());}
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSources"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the paper sources available on this printer.
        ///    </para>
        /// </devdoc>
        public PaperSourceCollection PaperSources {
            get { return new PaperSourceCollection(Get_PaperSources());}
        }

        /// <devdoc>
        ///    <para>
        ///        Whether the print dialog has been displayed.  In SafePrinting mode,
        ///        a print dialog is required to print.  After printing,
        ///        this property is set to false if the program does not have AllPrinting;
        ///        this guarantees a document is only printed once each time the print dialog is shown.
        ///    </para>
        /// </devdoc>
        internal bool PrintDialogDisplayed {
            // 


            get {
                // no security check

                return printDialogDisplayed;
            }

            set {
                IntSecurity.AllPrinting.Demand();
                printDialogDisplayed = value;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrintRange"]/*' />
        /// <devdoc>
        ///    <para> 
        ///       Gets or sets the pages the user has asked to print.</para>
        /// </devdoc>
        public PrintRange PrintRange {
            get { return printRange;}            
            [SuppressMessage("Microsoft.Performance", "CA1803:AvoidCostlyCallsWherePossible")]
            set { 
                if (!Enum.IsDefined(typeof(PrintRange), value))
                    throw new InvalidEnumArgumentException("value", unchecked((int)value), typeof(PrintRange));

                printRange = value;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrintToFile"]/*' />
        /// <devdoc>
        ///       Indicates whether to print to a file instead of a port.
        /// </devdoc>
        public bool PrintToFile {
            get {
                return printToFile;
            }
            set {
                printToFile = value;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterName"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the name of the printer.
        ///    </para>
        /// </devdoc>
        public string PrinterName {
            get { 
                IntSecurity.AllPrinting.Demand();
                return PrinterNameInternal;
            }

            set {
                IntSecurity.AllPrinting.Demand();
                PrinterNameInternal = value;
            }
        }

        private string PrinterNameInternal {
            get {
                if (printerName == null)
                    return GetDefaultPrinterName();
                else
                    return printerName;
            }
            set {
                // Reset the DevMode and Extrabytes...
                cachedDevmode = null;
                extrainfo = null;
                printerName = value;
                // VsWhidbey : 235920: PrinterName can be set through a fulltrusted assembly without using  the PrintDialog. 
                // So dont set this variable here.
                //PrintDialogDisplayed = true;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterResolutions"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets the resolutions supported by this printer.
        ///    </para>
        /// </devdoc>
        public PrinterResolutionCollection PrinterResolutions {
            get { return new PrinterResolutionCollection(Get_PrinterResolutions());}
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.IsDirectPrintingSupported"]/*' />
        /// <devdoc>
        ///     If the image is a JPEG or a PNG (Image.RawFormat) and the printer returns true
        ///     from ExtEscape(CHECKJPEGFORMAT) or ExtEscape(CHECKPNGFORMAT) then this function returns true.
        /// </devdoc>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        public bool IsDirectPrintingSupported(ImageFormat imageFormat) {
            bool isDirectPrintingSupported = false;
            if (imageFormat.Equals(ImageFormat.Jpeg) || imageFormat.Equals(ImageFormat.Png)) {
               int nEscape = imageFormat.Equals(ImageFormat.Jpeg) ? SafeNativeMethods.CHECKJPEGFORMAT : SafeNativeMethods.CHECKPNGFORMAT;
               int outData = 0;
               DeviceContext dc = CreateInformationContext(DefaultPageSettings);
               HandleRef hdc = new HandleRef(dc, dc.Hdc);
               try {
                isDirectPrintingSupported = SafeNativeMethods.ExtEscape(hdc, SafeNativeMethods.QUERYESCSUPPORT, Marshal.SizeOf(typeof(int)), ref nEscape, 0, out outData) > 0;
               }
               finally {
                dc.Dispose();
               }
            }
            return isDirectPrintingSupported;
        
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.IsDirectPrintingSupported"]/*' />
        /// <devdoc>
        ///    <para>
        /// This method utilizes the CHECKJPEGFORMAT/CHECKPNGFORMAT printer escape functions
        /// to determine whether the printer can handle a JPEG image.
        /// See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/prntspol_51ys.asp
        /// for more information on this printer escape function.
        ///
        /// If the image is a JPEG or a PNG (Image.RawFormat) and the printer returns true
        /// from ExtEscape(CHECKJPEGFORMAT) or ExtEscape(CHECKPNGFORMAT) then this function returns true.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        public bool IsDirectPrintingSupported(Image image) {
            bool isDirectPrintingSupported = false;
            if (image.RawFormat.Equals(ImageFormat.Jpeg) || image.RawFormat.Equals(ImageFormat.Png)) {
                MemoryStream stream = new MemoryStream();
                try {
                    image.Save(stream, image.RawFormat);
                    stream.Position = 0;
                    using (BufferedStream inStream = new BufferedStream(stream)) {
                        int pvImageLen = (int)inStream.Length;
                        byte[] pvImage = new byte[pvImageLen];

                        int nRead = inStream.Read(pvImage, 0, (int)pvImageLen);

                        int nEscape = image.RawFormat.Equals(ImageFormat.Jpeg) ? SafeNativeMethods.CHECKJPEGFORMAT : SafeNativeMethods.CHECKPNGFORMAT;
                        int outData = 0;

                        DeviceContext dc = CreateInformationContext(DefaultPageSettings);
                        HandleRef hdc = new HandleRef(dc, dc.Hdc);
                        try {
                            bool querySupported = SafeNativeMethods.ExtEscape(hdc, SafeNativeMethods.QUERYESCSUPPORT, Marshal.SizeOf(typeof(int)), ref nEscape, 0, out outData) > 0;
                            if (querySupported) {
                                isDirectPrintingSupported = (SafeNativeMethods.ExtEscape(hdc, nEscape, pvImageLen, pvImage, Marshal.SizeOf(typeof(int)), out outData) > 0)
                                                            && (outData == 1);
                            }
                        }
                        finally {
                            dc.Dispose();
                        }
                    }
                }
                finally {
                    stream.Close();
                }
            }
            return isDirectPrintingSupported;
        }
        
        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.SupportsColor"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets a
        ///       value indicating whether the printer supports color printing.
        ///    </para>
        /// </devdoc>
        public bool SupportsColor {
            get {
                // 

                return GetDeviceCaps(SafeNativeMethods.BITSPIXEL, 1) > 1;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.ToPage"]/*' />
        /// <devdoc>
        ///    Gets or sets the last page to print.
        /// </devdoc>
        public int ToPage {
            get { return toPage;}
            set {
                if (value < 0)
                    throw new ArgumentException(SR.GetString(SR.InvalidLowBoundArgumentEx,
                                                             "value", value.ToString(CultureInfo.CurrentCulture), 
                                                             (0).ToString(CultureInfo.CurrentCulture)));
                toPage = value;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.Clone"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Creates an identical copy of this object.
        ///    </para>
        /// </devdoc>
        public object Clone() {
            PrinterSettings clone = (PrinterSettings) MemberwiseClone();
            clone.printDialogDisplayed = false;
            return clone;
        }
        
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts")] // what is done in copytohdevmode cannot give unwanted access AllPrinting permission
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        internal DeviceContext CreateDeviceContext(PageSettings pageSettings) {
            IntPtr modeHandle = GetHdevmodeInternal(); 
            DeviceContext dc  = null;

            try {
                //Copy the PageSettings to the DEVMODE...
                //Assert permission as CopyToHdevmode() demands...
                IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                try 
                {
                    pageSettings.CopyToHdevmode(modeHandle);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                dc = CreateDeviceContext(modeHandle);
            }
            finally {
                SafeNativeMethods.GlobalFree(new HandleRef(null, modeHandle));
            }
            return dc;
        }

        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        internal DeviceContext CreateDeviceContext(IntPtr hdevmode) {
            IntPtr modePointer = SafeNativeMethods.GlobalLock(new HandleRef(null, hdevmode));
            DeviceContext dc = DeviceContext.CreateDC(DriverName, PrinterNameInternal, (string) null, new HandleRef(null, modePointer));
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, hdevmode));
            return dc;
        }

        // A read-only DC, which is faster than CreateHdc        
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts")] // what is done in copytohdevmode cannot give unwanted access AllPrinting permission
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        internal DeviceContext CreateInformationContext(PageSettings pageSettings) {
            IntPtr modeHandle = GetHdevmodeInternal();
            DeviceContext dc;

            try {
                //Copy the PageSettings to the DEVMODE...
                //Assert permission as CopyToHdevmode() demands...
                IntSecurity.AllPrintingAndUnmanagedCode.Assert();
                try 
                {
                    pageSettings.CopyToHdevmode(modeHandle);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                dc = CreateInformationContext(modeHandle);
            }
            finally {
                SafeNativeMethods.GlobalFree(new HandleRef(null, modeHandle));
            }
            return dc;
        }

        // A read-only DC, which is faster than CreateHdc
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        internal DeviceContext CreateInformationContext(IntPtr hdevmode) {
            IntPtr modePointer = SafeNativeMethods.GlobalLock(new HandleRef(null, hdevmode));
            DeviceContext dc = DeviceContext.CreateIC(DriverName, PrinterNameInternal, (string) null, new HandleRef(null, modePointer));
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, hdevmode));
            return dc;
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.CreateMeasurementGraphics"]/*' />
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public Graphics CreateMeasurementGraphics() {
            return CreateMeasurementGraphics(DefaultPageSettings);
        }
        
        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.CreateMeasurementGraphics2"]/*' />
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts")] //whatever the call stack calling HardMarginX and HardMarginY here is safe
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public Graphics CreateMeasurementGraphics(bool honorOriginAtMargins) {
            Graphics g = CreateMeasurementGraphics();
            if (g != null && honorOriginAtMargins) {
                IntSecurity.AllPrintingAndUnmanagedCode.Assert(); 
                try
                {
                    g.TranslateTransform(-defaultPageSettings.HardMarginX, -defaultPageSettings.HardMarginY);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                g.TranslateTransform(defaultPageSettings.Margins.Left, defaultPageSettings.Margins.Top);
            }
            return g;
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.CreateMeasurementGraphics3"]/*' />
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public Graphics CreateMeasurementGraphics(PageSettings pageSettings) {
            // returns the Graphics object for the printer
            DeviceContext dc = CreateDeviceContext(pageSettings);
            Graphics g = Graphics.FromHdcInternal(dc.Hdc);
            g.PrintingHelper = dc; // Graphics will dispose of the DeviceContext.
            return g;
        }
        
        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.CreateMeasurementGraphics4"]/*' />
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts")] //whatever the call stack calling HardMarginX and HardMarginY here is safe
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public Graphics CreateMeasurementGraphics(PageSettings pageSettings, bool honorOriginAtMargins) {
            Graphics g = CreateMeasurementGraphics();
            if (g != null && honorOriginAtMargins) {
                IntSecurity.AllPrintingAndUnmanagedCode.Assert(); 
                try
                {
                    g.TranslateTransform(-pageSettings.HardMarginX, -pageSettings.HardMarginY);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                g.TranslateTransform(pageSettings.Margins.Left, pageSettings.Margins.Top);
            }
            return g;
        }
            
                        
        // Create a PRINTDLG with a few useful defaults.
        // Try to keep this consistent with PrintDialog.CreatePRINTDLG.
        private static SafeNativeMethods.PRINTDLGX86 CreatePRINTDLGX86() {
                SafeNativeMethods.PRINTDLGX86 data = new SafeNativeMethods.PRINTDLGX86();
                data.lStructSize = Marshal.SizeOf(typeof(SafeNativeMethods.PRINTDLGX86));
                data.hwndOwner = IntPtr.Zero;
                data.hDevMode = IntPtr.Zero;
                data.hDevNames = IntPtr.Zero;
                data.Flags = 0;
                data.hwndOwner = IntPtr.Zero;
                data.hDC = IntPtr.Zero;
                data.nFromPage = 1;
                data.nToPage = 1;
                data.nMinPage = 0;
                data.nMaxPage = 9999;
                data.nCopies = 1;
                data.hInstance = IntPtr.Zero;
                data.lCustData = IntPtr.Zero;
                data.lpfnPrintHook = IntPtr.Zero;
                data.lpfnSetupHook = IntPtr.Zero;
                data.lpPrintTemplateName = null;
                data.lpSetupTemplateName = null;
                data.hPrintTemplate = IntPtr.Zero;
                data.hSetupTemplate = IntPtr.Zero;
                return data;
        }


        // Create a PRINTDLG with a few useful defaults.
        // Try to keep this consistent with PrintDialog.CreatePRINTDLG.
        private static SafeNativeMethods.PRINTDLG CreatePRINTDLG() {
                SafeNativeMethods.PRINTDLG data = new SafeNativeMethods.PRINTDLG();
                data.lStructSize = Marshal.SizeOf(typeof(SafeNativeMethods.PRINTDLG));
                data.hwndOwner = IntPtr.Zero;
                data.hDevMode = IntPtr.Zero;
                data.hDevNames = IntPtr.Zero;
                data.Flags = 0;
                data.hwndOwner = IntPtr.Zero;
                data.hDC = IntPtr.Zero;
                data.nFromPage = 1;
                data.nToPage = 1;
                data.nMinPage = 0;
                data.nMaxPage = 9999;
                data.nCopies = 1;
                data.hInstance = IntPtr.Zero;
                data.lCustData = IntPtr.Zero;
                data.lpfnPrintHook = IntPtr.Zero;
                data.lpfnSetupHook = IntPtr.Zero;
                data.lpPrintTemplateName = null;
                data.lpSetupTemplateName = null;
                data.hPrintTemplate = IntPtr.Zero;
                data.hSetupTemplate = IntPtr.Zero;
                return data;
        }

        //  Use FastDeviceCapabilities where possible -- computing PrinterName is quite slow
        private int DeviceCapabilities(short capability, IntPtr pointerToBuffer, int defaultValue) {
            IntSecurity.AllPrinting.Assert();
            string printerName = PrinterName;
            CodeAccessPermission.RevertAssert();

            IntSecurity.UnmanagedCode.Assert();

            return FastDeviceCapabilities(capability, pointerToBuffer, defaultValue, printerName);
        }

        // We pass PrinterName in as a parameter rather than computing it ourselves because it's expensive to compute.
        // We need to pass IntPtr.Zero since passing HDevMode is non-performant.
        private static int FastDeviceCapabilities(short capability, IntPtr pointerToBuffer, int defaultValue, string printerName) {
            int result = SafeNativeMethods.DeviceCapabilities(printerName, GetOutputPort(),
                                                          capability, pointerToBuffer, IntPtr.Zero);
            if (result == -1)
                return defaultValue;
            return result;
        }

        // Called by get_PrinterName
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        private static string GetDefaultPrinterName() {
            IntSecurity.UnmanagedCode.Assert();
            if (IntPtr.Size == 8)
            {
                SafeNativeMethods.PRINTDLG data = CreatePRINTDLG();
                data.Flags = SafeNativeMethods.PD_RETURNDEFAULT;
                bool status = SafeNativeMethods.PrintDlg(data);

                if (!status)
                    return SR.GetString(SR.NoDefaultPrinter);

                IntPtr handle = data.hDevNames;
                IntPtr names = SafeNativeMethods.GlobalLock(new HandleRef(data, handle));
                if (names == IntPtr.Zero)
                    throw new Win32Exception();

                string name = ReadOneDEVNAME(names, 1);
                SafeNativeMethods.GlobalUnlock(new HandleRef(data, handle));
                names = IntPtr.Zero;

                // Windows allocates them, but we have to free them
                SafeNativeMethods.GlobalFree(new HandleRef(data, data.hDevNames));
                SafeNativeMethods.GlobalFree(new HandleRef(data, data.hDevMode));

                return name;
            }
            else {
                SafeNativeMethods.PRINTDLGX86 data = CreatePRINTDLGX86();
                data.Flags = SafeNativeMethods.PD_RETURNDEFAULT;
                bool status = SafeNativeMethods.PrintDlg(data);

                if (!status)
                return SR.GetString(SR.NoDefaultPrinter);
                
                IntPtr handle = data.hDevNames;
                IntPtr names = SafeNativeMethods.GlobalLock(new HandleRef(data, handle));
                if (names == IntPtr.Zero)
                    throw new Win32Exception();

                string name = ReadOneDEVNAME(names, 1);
                SafeNativeMethods.GlobalUnlock(new HandleRef(data, handle));
                names = IntPtr.Zero;

                // Windows allocates them, but we have to free them
                SafeNativeMethods.GlobalFree(new HandleRef(data, data.hDevNames));
                SafeNativeMethods.GlobalFree(new HandleRef(data, data.hDevMode));

                return name;
            }
        }


        // Called by get_OutputPort
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        private static string GetOutputPort() {
            IntSecurity.UnmanagedCode.Assert();

            if (IntPtr.Size == 8)
            {
                SafeNativeMethods.PRINTDLG data = CreatePRINTDLG();
                data.Flags = SafeNativeMethods.PD_RETURNDEFAULT;
                bool status = SafeNativeMethods.PrintDlg(data);
                if (!status)
                    return SR.GetString(SR.NoDefaultPrinter);

                IntPtr handle = data.hDevNames;
                IntPtr names = SafeNativeMethods.GlobalLock(new HandleRef(data, handle));
                if (names == IntPtr.Zero)
                    throw new Win32Exception();

                string name = ReadOneDEVNAME(names, 2);
                
                SafeNativeMethods.GlobalUnlock(new HandleRef(data, handle));
                names = IntPtr.Zero;

                // Windows allocates them, but we have to free them
                SafeNativeMethods.GlobalFree(new HandleRef(data, data.hDevNames));
                SafeNativeMethods.GlobalFree(new HandleRef(data, data.hDevMode));

                return name;
            }
            else {
                SafeNativeMethods.PRINTDLGX86 data = CreatePRINTDLGX86();
                data.Flags = SafeNativeMethods.PD_RETURNDEFAULT;
                bool status = SafeNativeMethods.PrintDlg(data);

                if (!status)
                    return SR.GetString(SR.NoDefaultPrinter);

                IntPtr handle = data.hDevNames;
                IntPtr names = SafeNativeMethods.GlobalLock(new HandleRef(data, handle));
                if (names == IntPtr.Zero)
                    throw new Win32Exception();

                string name = ReadOneDEVNAME(names, 2);
                
                SafeNativeMethods.GlobalUnlock(new HandleRef(data, handle));
                names = IntPtr.Zero;

                // Windows allocates them, but we have to free them
                SafeNativeMethods.GlobalFree(new HandleRef(data, data.hDevNames));
                SafeNativeMethods.GlobalFree(new HandleRef(data, data.hDevMode));

                return name;
            }
            
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        private int GetDeviceCaps(int capability, int defaultValue) {
            DeviceContext dc = CreateInformationContext(DefaultPageSettings);
            int result = defaultValue;

            try {
                result = UnsafeNativeMethods.GetDeviceCaps(new HandleRef(dc, dc.Hdc), capability);
            }
            catch (InvalidPrinterException) {
                // do nothing, will return defaultValue.
            }
            finally{
                dc.Dispose();
            }

            return result;
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.GetHdevmode"]/*' />
        /// <devdoc>
        ///    <para>Creates a handle to a DEVMODE structure which correspond too the printer settings.
        ///       When you are done with the handle, you must deallocate it yourself:
        ///       Windows.GlobalFree(handle);
        ///       Where "handle" is the return value from this method.</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public IntPtr GetHdevmode() {
            IntSecurity.AllPrintingAndUnmanagedCode.Demand();
            // Don't assert unmanaged code -- anyone using handles should have unmanaged code permission
            IntPtr modeHandle = GetHdevmodeInternal();
            defaultPageSettings.CopyToHdevmode(modeHandle);
            return modeHandle;
        }

        internal IntPtr GetHdevmodeInternal() {
            // getting the printer name is quite expensive if PrinterName is left default,
            // because it needs to figure out what the default printer is
            return GetHdevmodeInternal(PrinterNameInternal);
        }

        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        private IntPtr GetHdevmodeInternal(string printer) {
            // Create DEVMODE
            int modeSize = SafeNativeMethods.DocumentProperties(NativeMethods.NullHandleRef, NativeMethods.NullHandleRef, printer, IntPtr.Zero, NativeMethods.NullHandleRef, 0);
            if (modeSize < 1) {
                throw new InvalidPrinterException(this);
            }
            IntPtr handle = SafeNativeMethods.GlobalAlloc(SafeNativeMethods.GMEM_MOVEABLE, (uint)modeSize); // cannot be <0 anyway
            IntPtr pointer = SafeNativeMethods.GlobalLock(new HandleRef(null, handle));
            
            //Get the DevMode only if its not cached....
            if (cachedDevmode != null) {
                Marshal.Copy(cachedDevmode, 0, pointer, devmodebytes);
            }
            else  {
                int returnCode = SafeNativeMethods.DocumentProperties(NativeMethods.NullHandleRef, NativeMethods.NullHandleRef, printer, pointer, NativeMethods.NullHandleRef, SafeNativeMethods.DM_OUT_BUFFER);
                if (returnCode < 0) {
                    throw new Win32Exception();
                }
            }

            SafeNativeMethods.DEVMODE mode = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(pointer, typeof(SafeNativeMethods.DEVMODE));


            if (extrainfo != null) {
                // guard against buffer overrun attacks (since design allows client to set a new printer name without updating the devmode)
                // by checking for a large enough buffer size before copying the extrainfo buffer
                if (extrabytes <= mode.dmDriverExtra)  {
                    IntPtr pointeroffset = (IntPtr)(checked((long)pointer + (long)mode.dmSize));
                    Marshal.Copy(extrainfo, 0, pointeroffset, extrabytes);
                }
            }
            if ((mode.dmFields & SafeNativeMethods.DM_COPIES) == SafeNativeMethods.DM_COPIES)
            {
                if (copies != -1) mode.dmCopies = copies;
            }

            if ((mode.dmFields & SafeNativeMethods.DM_DUPLEX) == SafeNativeMethods.DM_DUPLEX)
            {
                if (unchecked((int)duplex) != -1) mode.dmDuplex = unchecked((short) duplex);
            }
            
            if ((mode.dmFields & SafeNativeMethods.DM_COLLATE) == SafeNativeMethods.DM_COLLATE)
            {
                if (collate.IsNotDefault)
                    mode.dmCollate = (short) (((bool) collate) ? SafeNativeMethods.DMCOLLATE_TRUE : SafeNativeMethods.DMCOLLATE_FALSE);
            }

            Marshal.StructureToPtr(mode, pointer, false);
        
            int retCode = SafeNativeMethods.DocumentProperties(NativeMethods.NullHandleRef, NativeMethods.NullHandleRef, printer, pointer, pointer, SafeNativeMethods.DM_IN_BUFFER | SafeNativeMethods.DM_OUT_BUFFER);
            if (retCode < 0) {
                SafeNativeMethods.GlobalFree(new HandleRef(null, handle));
                SafeNativeMethods.GlobalUnlock(new HandleRef(null, handle));
                return IntPtr.Zero;
            }
            
            
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, handle));
            return handle;
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.GetHdevmode1"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Creates a handle to a DEVMODE structure which correspond to the printer
        ///       and page settings.
        ///       When you are done with the handle, you must deallocate it yourself:
        ///       Windows.GlobalFree(handle);
        ///       Where "handle" is the return value from this method.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public IntPtr GetHdevmode(PageSettings pageSettings) {
            IntSecurity.AllPrintingAndUnmanagedCode.Demand();
            // Don't assert unmanaged code -- anyone using handles should have unmanaged code permission
            IntPtr modeHandle = GetHdevmodeInternal();
            pageSettings.CopyToHdevmode(modeHandle);

            return modeHandle;
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.GetHdevnames"]/*' />
        /// <devdoc>
        ///    Creates a handle to a DEVNAMES structure which correspond to the printer settings.
        ///    When you are done with the handle, you must deallocate it yourself:
        ///    Windows.GlobalFree(handle);
        ///    Where "handle" is the return value from this method.
        /// </devdoc>
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        public IntPtr GetHdevnames() {
            IntSecurity.AllPrintingAndUnmanagedCode.Demand();
            // Don't assert unmanaged code -- anyone using handles should have unmanaged code permission

            string printerName = PrinterName; // the PrinterName property is slow when using the default printer
            string driver = DriverName;  // make sure we are writing out exactly the same string as we got the length of
            string outPort = OutputPort;

            // Create DEVNAMES structure
            // +4 for null terminator
            int namesCharacters = checked(4 + printerName.Length + driver.Length + outPort.Length);

            // 8 = size of fixed portion of DEVNAMES
            short offset = (short) (8 / Marshal.SystemDefaultCharSize); // Offsets are in characters, not bytes
            uint namesSize = (uint)checked(Marshal.SystemDefaultCharSize * (offset + namesCharacters)); // always >0
            IntPtr handle = SafeNativeMethods.GlobalAlloc(SafeNativeMethods.GMEM_MOVEABLE | SafeNativeMethods.GMEM_ZEROINIT, namesSize);
            IntPtr namesPointer = SafeNativeMethods.GlobalLock(new HandleRef(null, handle));

            Marshal.WriteInt16(namesPointer, offset); // wDriverOffset
            offset += WriteOneDEVNAME(driver, namesPointer, offset);
            Marshal.WriteInt16((IntPtr)(checked((long)namesPointer + 2)), offset); // wDeviceOffset
            offset += WriteOneDEVNAME(printerName, namesPointer, offset);
            Marshal.WriteInt16((IntPtr)(checked((long)namesPointer + 4)), offset); // wOutputOffset
            offset += WriteOneDEVNAME(outPort, namesPointer, offset);
            Marshal.WriteInt16((IntPtr)(checked((long)namesPointer + 6)), offset); // wDefault

            SafeNativeMethods.GlobalUnlock(new HandleRef(null, handle));
            return handle;
        }

        // Handles creating then disposing a default DEVMODE
        internal short GetModeField(ModeField field, short defaultValue) {
            return GetModeField(field, defaultValue, IntPtr.Zero);
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        internal short GetModeField(ModeField field, short defaultValue, IntPtr modeHandle) {
            bool ownHandle = false;
            short result;
            try {
            
                if (modeHandle == IntPtr.Zero) {
                    try {
                        modeHandle = GetHdevmodeInternal();
                        ownHandle = true;
                    }
                    catch (InvalidPrinterException) {
                        return defaultValue;
                    }
                }
    
                IntPtr modePointer = SafeNativeMethods.GlobalLock(new HandleRef(this, modeHandle));
                SafeNativeMethods.DEVMODE mode = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(modePointer, typeof(SafeNativeMethods.DEVMODE));
                switch (field) {
                    case ModeField.Orientation: result = mode.dmOrientation; break;
                    case ModeField.PaperSize: result = mode.dmPaperSize; break;
                    case ModeField.PaperLength: result = mode.dmPaperLength; break;
                    case ModeField.PaperWidth: result = mode.dmPaperWidth; break;
                    case ModeField.Copies: result = mode.dmCopies; break;
                    case ModeField.DefaultSource: result = mode.dmDefaultSource; break;
                    case ModeField.PrintQuality: result = mode.dmPrintQuality; break;
                    case ModeField.Color: result = mode.dmColor; break;
                    case ModeField.Duplex: result = mode.dmDuplex; break;
                    case ModeField.YResolution: result = mode.dmYResolution; break;
                    case ModeField.TTOption: result = mode.dmTTOption; break;
                    case ModeField.Collate: result = mode.dmCollate; break;
                    default:
                        Debug.Fail("Invalid field in GetModeField");
                        result = defaultValue;
                        break;
                }
                SafeNativeMethods.GlobalUnlock(new HandleRef(this, modeHandle));
            }
            finally
            {
                if (ownHandle) {
                    SafeNativeMethods.GlobalFree(new HandleRef(this, modeHandle));
                }
                
            }
            return result;
        }

        internal PaperSize[] Get_PaperSizes() {
            IntSecurity.AllPrintingAndUnmanagedCode.Assert();

            string printerName = PrinterName; //  this is quite expensive if PrinterName is left default

            int count = FastDeviceCapabilities(SafeNativeMethods.DC_PAPERNAMES, IntPtr.Zero, -1, printerName);
            if (count == -1)
                return new PaperSize[0];
            int stringSize = Marshal.SystemDefaultCharSize * 64;
            IntPtr namesBuffer = Marshal.AllocCoTaskMem(checked(stringSize * count));
            FastDeviceCapabilities(SafeNativeMethods.DC_PAPERNAMES, namesBuffer, -1, printerName);

            Debug.Assert(FastDeviceCapabilities(SafeNativeMethods.DC_PAPERS, IntPtr.Zero, -1, printerName) == count,
                         "Not the same number of paper kinds as paper names?");
            IntPtr kindsBuffer = Marshal.AllocCoTaskMem(2 * count);
            FastDeviceCapabilities(SafeNativeMethods.DC_PAPERS, kindsBuffer, -1, printerName);

            Debug.Assert(FastDeviceCapabilities(SafeNativeMethods.DC_PAPERSIZE, IntPtr.Zero, -1, printerName) == count,
                         "Not the same number of paper kinds as paper names?");
            IntPtr dimensionsBuffer = Marshal.AllocCoTaskMem(8 * count);
            FastDeviceCapabilities(SafeNativeMethods.DC_PAPERSIZE, dimensionsBuffer, -1, printerName);

            PaperSize[] result = new PaperSize[count];
            for (int i = 0; i < count; i++) {
                string name = Marshal.PtrToStringAuto((IntPtr)(checked((long)namesBuffer + stringSize * i)), 64);
                int index = name.IndexOf('\0');
                if (index > -1) {
                    name = name.Substring(0, index);
                }
                short kind = Marshal.ReadInt16((IntPtr)(checked((long)kindsBuffer + i*2)));
                int width = Marshal.ReadInt32((IntPtr)(checked((long)dimensionsBuffer + i * 8)));
                int height = Marshal.ReadInt32((IntPtr)(checked((long)dimensionsBuffer + i * 8 + 4)));
                result[i] = new PaperSize((PaperKind) kind, name, 
                                          PrinterUnitConvert.Convert(width, PrinterUnit.TenthsOfAMillimeter, PrinterUnit.Display),
                                          PrinterUnitConvert.Convert(height, PrinterUnit.TenthsOfAMillimeter, PrinterUnit.Display));
            }

            Marshal.FreeCoTaskMem(namesBuffer);
            Marshal.FreeCoTaskMem(kindsBuffer);
            Marshal.FreeCoTaskMem(dimensionsBuffer);
            return result;
        }

        internal PaperSource[] Get_PaperSources() {
            IntSecurity.AllPrintingAndUnmanagedCode.Assert();

            string printerName = PrinterName; //  this is quite expensive if PrinterName is left default

            int count = FastDeviceCapabilities(SafeNativeMethods.DC_BINNAMES, IntPtr.Zero, -1, printerName);
            if (count == -1)
                return new PaperSource[0];

            // Contrary to documentation, DeviceCapabilities returns char[count, 24],
            // not char[count][24]
            int stringSize = Marshal.SystemDefaultCharSize * 24;
            IntPtr namesBuffer = Marshal.AllocCoTaskMem(checked(stringSize * count));
            FastDeviceCapabilities(SafeNativeMethods.DC_BINNAMES, namesBuffer, -1, printerName);

            Debug.Assert(FastDeviceCapabilities(SafeNativeMethods.DC_BINS, IntPtr.Zero, -1, printerName) == count,
                         "Not the same number of bin kinds as bin names?");
            IntPtr kindsBuffer = Marshal.AllocCoTaskMem(2 * count);
            FastDeviceCapabilities(SafeNativeMethods.DC_BINS, kindsBuffer, -1, printerName);

            PaperSource[] result = new PaperSource[count];
            for (int i = 0; i < count; i++) {
                string name = Marshal.PtrToStringAuto((IntPtr)(checked((long)namesBuffer + stringSize * i)), 24);
                int index = name.IndexOf('\0');
                if (index > -1) {
                    name = name.Substring(0, index);
                }

                short kind = Marshal.ReadInt16((IntPtr)(checked((long)kindsBuffer + 2*i)));
                result[i] = new PaperSource((PaperSourceKind) kind, name);
            }

            Marshal.FreeCoTaskMem(namesBuffer);
            Marshal.FreeCoTaskMem(kindsBuffer);
            return result;
        }

        internal PrinterResolution[] Get_PrinterResolutions() {
            IntSecurity.AllPrintingAndUnmanagedCode.Assert();

            string printerName = PrinterName; //  this is quite expensive if PrinterName is left default
            PrinterResolution[] result;

            int count = FastDeviceCapabilities(SafeNativeMethods.DC_ENUMRESOLUTIONS, IntPtr.Zero, -1, printerName);
            if (count == -1) {
                //Just return the standrard values if custom resolutions absemt ....
                result = new PrinterResolution[4];
                result[0] = new PrinterResolution(PrinterResolutionKind.High, -4, -1);
                result[1] = new PrinterResolution(PrinterResolutionKind.Medium, -3, -1);
                result[2] = new PrinterResolution(PrinterResolutionKind.Low, -2, -1);
                result[3] = new PrinterResolution(PrinterResolutionKind.Draft, -1, -1);
                
                return result;
            }
            
            result = new PrinterResolution[count + 4];
            result[0] = new PrinterResolution(PrinterResolutionKind.High, -4, -1);
            result[1] = new PrinterResolution(PrinterResolutionKind.Medium, -3, -1);
            result[2] = new PrinterResolution(PrinterResolutionKind.Low, -2, -1);
            result[3] = new PrinterResolution(PrinterResolutionKind.Draft, -1, -1);

            IntPtr buffer = Marshal.AllocCoTaskMem(checked(8 * count));
            FastDeviceCapabilities(SafeNativeMethods.DC_ENUMRESOLUTIONS, buffer, -1, printerName);
            
            for (int i = 0; i < count; i++) {
                int x = Marshal.ReadInt32((IntPtr)(checked((long)buffer + i*8)));
                int y = Marshal.ReadInt32((IntPtr)(checked((long)buffer + i*8 + 4)));
                result[i + 4] = new PrinterResolution(PrinterResolutionKind.Custom, x, y);
            }

            Marshal.FreeCoTaskMem(buffer);
            return result;
        }

        // names is pointer to DEVNAMES
        private static String ReadOneDEVNAME(IntPtr pDevnames, int slot) {
            int offset = checked(Marshal.SystemDefaultCharSize * Marshal.ReadInt16((IntPtr)(checked((long)pDevnames + slot * 2))));
            string result = Marshal.PtrToStringAuto((IntPtr)(checked((long)pDevnames + offset)));
            return result;
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.SetHdevmode"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Copies the relevant information out of the handle and into the PrinterSettings.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        public void SetHdevmode(IntPtr hdevmode) {
            IntSecurity.AllPrintingAndUnmanagedCode.Demand();
            // Don't assert unmanaged code -- anyone using handles should have unmanaged code permission

            if (hdevmode == IntPtr.Zero)
                throw new ArgumentException(SR.GetString(SR.InvalidPrinterHandle, hdevmode));

            IntPtr pointer = SafeNativeMethods.GlobalLock(new HandleRef(null, hdevmode));
            SafeNativeMethods.DEVMODE mode = (SafeNativeMethods.DEVMODE) UnsafeNativeMethods.PtrToStructure(pointer, typeof(SafeNativeMethods.DEVMODE));

            //Copy entire public devmode as a byte array...
            devmodebytes = mode.dmSize;
            if (devmodebytes > 0)  {
                cachedDevmode = new byte[devmodebytes];
                Marshal.Copy(pointer, cachedDevmode, 0, devmodebytes);
            }

            //Copy private devmode as a byte array..
            extrabytes = mode.dmDriverExtra;
            if (extrabytes > 0)  {
                extrainfo = new byte[extrabytes];
                Marshal.Copy((IntPtr)(checked((long)pointer + (long)mode.dmSize)), extrainfo, 0, extrabytes);
            }

            if ((mode.dmFields & SafeNativeMethods.DM_COPIES) == SafeNativeMethods.DM_COPIES) {
                copies = mode.dmCopies;
            }

            if ((mode.dmFields & SafeNativeMethods.DM_DUPLEX) == SafeNativeMethods.DM_DUPLEX ) {
                duplex = (Duplex) mode.dmDuplex;
            }
            
            if ((mode.dmFields & SafeNativeMethods.DM_COLLATE) == SafeNativeMethods.DM_COLLATE) {
                collate = (mode.dmCollate == SafeNativeMethods.DMCOLLATE_TRUE);
            }

            SafeNativeMethods.GlobalUnlock(new HandleRef(null, hdevmode));
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.SetHdevnames"]/*' />
        /// <devdoc>
        ///    <para>Copies the relevant information out of the handle and into the PrinterSettings.</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        public void SetHdevnames(IntPtr hdevnames) {
            IntSecurity.AllPrintingAndUnmanagedCode.Demand();

            if (hdevnames == IntPtr.Zero)
                throw new ArgumentException(SR.GetString(SR.InvalidPrinterHandle, hdevnames));

            IntPtr namesPointer = SafeNativeMethods.GlobalLock(new HandleRef(null, hdevnames));

            driverName = ReadOneDEVNAME(namesPointer, 0);
            printerName = ReadOneDEVNAME(namesPointer, 1);
            outputPort = ReadOneDEVNAME(namesPointer, 2);

            PrintDialogDisplayed = true;
            
            SafeNativeMethods.GlobalUnlock(new HandleRef(null, hdevnames));
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.ToString"]/*' />
        /// <internalonly/>
        /// <devdoc>
        ///    <para>
        ///       Provides some interesting information about the PrinterSettings in
        ///       String form.
        ///    </para>
        /// </devdoc>
        public override string ToString() {
            string printerName = (IntSecurity.HasPermission(IntSecurity.AllPrinting)) ? PrinterName : "<printer name unavailable>";
            return "[PrinterSettings " 
            + printerName
            + " Copies=" + Copies.ToString(CultureInfo.InvariantCulture)
            + " Collate=" + Collate.ToString(CultureInfo.InvariantCulture)
            //            + " DriverName=" + DriverName.ToString(CultureInfo.InvariantCulture)
            //            + " DriverVersion=" + DriverVersion.ToString(CultureInfo.InvariantCulture)
            + " Duplex=" + TypeDescriptor.GetConverter(typeof(Duplex)).ConvertToString(unchecked((int) Duplex))
            + " FromPage=" + FromPage.ToString(CultureInfo.InvariantCulture)
            + " LandscapeAngle=" + LandscapeAngle.ToString(CultureInfo.InvariantCulture)
            + " MaximumCopies=" + MaximumCopies.ToString(CultureInfo.InvariantCulture)
            + " OutputPort=" + OutputPort.ToString(CultureInfo.InvariantCulture)
            + " ToPage=" + ToPage.ToString(CultureInfo.InvariantCulture)
            + "]";
        }

        // Write null terminated string, return length of string in characters (including null)
        private short WriteOneDEVNAME(string str, IntPtr bufferStart, int index) {
            if (str == null) str = "";
            IntPtr address = (IntPtr)(checked((long)bufferStart + index * Marshal.SystemDefaultCharSize));
            
            if (Marshal.SystemDefaultCharSize == 1) {
                byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
                Marshal.Copy(bytes, 0, address, bytes.Length);
                Marshal.WriteByte(checked((IntPtr)((long)address + bytes.Length)), 0);
            }
            else {
                char[] data = str.ToCharArray();
                Marshal.Copy(data, 0, address, data.Length);
                Marshal.WriteInt16((IntPtr)(checked((long)address + data.Length*2)), 0);
            }
            
            return checked((short) (str.Length + 1));
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSizeCollection"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Collection of PaperSize's...
        ///    </para>
        /// </devdoc>
        public class PaperSizeCollection : ICollection {
            private PaperSize[] array;

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSizeCollection.PaperSizeCollection"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Initializes a new instance of the <see cref='System.Drawing.Printing.PrinterSettings.PaperSizeCollection'/> class.
            ///    </para>
            /// </devdoc>
            public PaperSizeCollection(PaperSize[] array) {
                this.array = array;
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSizeCollection.Count"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Gets a value indicating the number of paper sizes.
            ///    </para>
            /// </devdoc>
            public int Count {
                get {
                    return array.Length;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSizeCollection.this"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Retrieves the PaperSize with the specified index.
            ///    </para>
            /// </devdoc>
            public virtual PaperSize this[int index] {
                get {
                    return array[index];
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSizeCollection.GetEnumerator"]/*' />
            /// <devdoc>
            /// </devdoc>
            public IEnumerator GetEnumerator() {
                return new ArrayEnumerator(array, 0, Count);
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSizeCollection.ICollection.Count"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            int ICollection.Count {
                get {
                    return this.Count;
                }
            }


            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSizeCollection.ICollection.IsSynchronized"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            bool ICollection.IsSynchronized {
                get {
                    return false;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSizeCollection.ICollection.SyncRoot"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            object ICollection.SyncRoot {
                get {
                    return this;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PaperSizeCollection.ICollection.CopyTo"]/*' />
            /// <devdoc>        
            /// ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            void ICollection.CopyTo(Array array, int index) {
                Array.Copy(this.array, index, array, 0, this.array.Length);
            }

            public void CopyTo(PaperSize[] paperSizes, int index) {
                Array.Copy(this.array, index, paperSizes, 0, this.array.Length);
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSizeCollection.IEnumerable.GetEnumerator"]/*' />
            /// <devdoc>        
            ///    IEnumerable private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }         

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSizeCollection.Add"]/*' />
            /// <devdoc>        
            /// Empty implementation required for serialization of PrinterSettings object.
            /// </devdoc>
            /// <internalonly/>
            [
                EditorBrowsable(EditorBrowsableState.Never)
            ]
            public Int32 Add(PaperSize paperSize)
            {
                PaperSize[] newArray = new PaperSize[this.Count + 1];
                ((ICollection) this).CopyTo(newArray, 0);
                newArray[this.Count] = paperSize;
                this.array = newArray;
                return this.Count;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSourceCollection"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Collection of PaperSource's...
        ///    </para>
        /// </devdoc>
        public class PaperSourceCollection : ICollection {
            private PaperSource[] array;

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSourceCollection.PaperSourceCollection"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Initializes a new instance of the <see cref='System.Drawing.Printing.PrinterSettings.PaperSourceCollection'/> class.
            ///    </para>
            /// </devdoc>
            public PaperSourceCollection(PaperSource[] array) {
                this.array = array;
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSourceCollection.Count"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Gets a value indicating the number of paper sources.
            ///    </para>
            /// </devdoc>
            public int Count {
                get {
                    return array.Length;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSourceCollection.this"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Gets the PaperSource with the specified index.
            ///    </para>
            /// </devdoc>
            public virtual PaperSource this[int index] {
                get {
                    return array[index];
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSourceCollection.GetEnumerator"]/*' />
            /// <devdoc>
            /// </devdoc>
            /// <devdoc>
            /// </devdoc>
            /// <devdoc>
            /// </devdoc>
            public IEnumerator GetEnumerator() {
                return new ArrayEnumerator(array, 0, Count);
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSourceCollection.ICollection.Count"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            int ICollection.Count {
                get {
                    return this.Count;
                }
            }


            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSourceCollection.ICollection.IsSynchronized"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            bool ICollection.IsSynchronized {
                get {
                    return false;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSourceCollection.ICollection.SyncRoot"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            object ICollection.SyncRoot {
                get {
                    return this;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PaperSourceCollection.ICollection.CopyTo"]/*' />
            /// <devdoc>        
            /// ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            void ICollection.CopyTo(Array array, int index) {
                Array.Copy(this.array, index, array, 0, this.array.Length);
            }

            public void CopyTo(PaperSource[] paperSources, int index) {
                Array.Copy(this.array, index, paperSources, 0, this.array.Length);
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSourceCollection.IEnumerable.GetEnumerator"]/*' />
            /// <devdoc>        
            ///    IEnumerable private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PaperSourceCollection.Add"]/*' />
            /// <devdoc>        
            /// Empty implementation required for serialization of PrinterSettings object.
            /// </devdoc>
            /// <internalonly/>
            [
                EditorBrowsable(EditorBrowsableState.Never)
            ]
            public Int32 Add(PaperSource paperSource)
            {
                PaperSource[] newArray = new PaperSource[this.Count + 1];
                ((ICollection) this).CopyTo(newArray, 0);
                newArray[this.Count] = paperSource;
                this.array = newArray;
                return this.Count;
            }
        }

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterResolutionCollection"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Collection of PrinterResolution's...
        ///    </para>
        /// </devdoc>
        public class PrinterResolutionCollection : ICollection {
            private PrinterResolution[] array;

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterResolutionCollection.PrinterResolutionCollection"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Initializes a new instance of the <see cref='System.Drawing.Printing.PrinterSettings.PrinterResolutionCollection'/> class.
            ///    </para>
            /// </devdoc>
            public PrinterResolutionCollection(PrinterResolution[] array) {
                this.array = array;
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterResolutionCollection.Count"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Gets a
            ///       value indicating the number of available printer resolutions.
            ///    </para>
            /// </devdoc>
            public int Count {
                get {
                    return array.Length;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterResolutionCollection.this"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Retrieves the PrinterResolution with the specified index.
            ///    </para>
            /// </devdoc>
            public virtual PrinterResolution this[int index] {
                get {
                    return array[index];
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterResolutionCollection.GetEnumerator"]/*' />
            /// <devdoc>
            /// </devdoc>
            /// <devdoc>
            /// </devdoc>
            /// <devdoc>
            /// </devdoc>
            public IEnumerator GetEnumerator() {
                return new ArrayEnumerator(array, 0, Count);
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterResolutionCollection.ICollection.Count"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            int ICollection.Count {
                get {
                    return this.Count;
                }
            }


            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterResolutionCollection.ICollection.IsSynchronized"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            bool ICollection.IsSynchronized {
                get {
                    return false;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterResolutionCollection.ICollection.SyncRoot"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            object ICollection.SyncRoot {
                get {
                    return this;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterResolutionCollection.ICollection.CopyTo"]/*' />
            /// <devdoc>        
            /// ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            void ICollection.CopyTo(Array array, int index) {
                Array.Copy(this.array, index, array, 0, this.array.Length);
            }

            public void CopyTo(PrinterResolution[] printerResolutions, int index) {
                Array.Copy(this.array, index, printerResolutions, 0, this.array.Length);
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterResolutionCollection.IEnumerable.GetEnumerator"]/*' />
            /// <devdoc>        
            ///    IEnumerable private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.PrinterResolutionCollection.Add"]/*' />
            /// <devdoc>        
            /// Empty implementation required for serialization of PrinterSettings object.
            /// </devdoc>
            /// <internalonly/>
            [
                EditorBrowsable(EditorBrowsableState.Never)
            ]
            public Int32 Add(PrinterResolution printerResolution)
            {
                PrinterResolution[] newArray = new PrinterResolution[this.Count + 1];
                ((ICollection) this).CopyTo(newArray, 0);
                newArray[this.Count] = printerResolution;
                this.array = newArray;
                return this.Count;
            }
        }        

        /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.StringCollection"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Collection of String's...
        ///    </para>
        /// </devdoc>
        /// <internalonly/>
        public class StringCollection : ICollection {
            private String[] array;

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.StringCollection.StringCollection"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Initializes a new instance of the <see cref='System.Drawing.Printing.PrinterSettings.StringCollection'/> class.
            ///    </para>
            /// </devdoc>
            public StringCollection(String[] array) {
                this.array = array;
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.StringCollection.Count"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Gets a value indicating the number of strings.
            ///    </para>
            /// </devdoc>
            public int Count {
                get {
                    return array.Length;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.StringCollection.this"]/*' />
            /// <devdoc>
            ///    <para>
            ///       Gets the string with the specified index.
            ///    </para>
            /// </devdoc>
            public virtual String this[int index] {
                get {
                    return array[index];
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.StringCollection.GetEnumerator"]/*' />
            /// <devdoc>
            /// </devdoc>
            /// <devdoc>
            /// </devdoc>
            /// <devdoc>
            /// </devdoc>
            public IEnumerator GetEnumerator() {
                return new ArrayEnumerator(array, 0, Count);
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.StringCollection.ICollection.Count"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            int ICollection.Count {
                get {
                    return this.Count;
                }
            }


            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.StringCollection.ICollection.IsSynchronized"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            bool ICollection.IsSynchronized {
                get {
                    return false;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="PrinterSettings.StringCollection.ICollection.SyncRoot"]/*' />
            /// <devdoc>        
            ///    ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            object ICollection.SyncRoot {
                get {
                    return this;
                }
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="StringCollection.ICollection.CopyTo"]/*' />
            /// <devdoc>        
            /// ICollection private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            void ICollection.CopyTo(Array array, int index) {
                Array.Copy(this.array, index, array, 0, this.array.Length);
            }

            
            public void CopyTo(string[] strings, int index) {
                Array.Copy(this.array, index, strings, 0, this.array.Length);
            }

            

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="StringCollection.IEnumerable.GetEnumerator"]/*' />
            /// <devdoc>        
            /// IEnumerable private interface implementation.        
            /// </devdoc>
            /// <internalonly/>
            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            /// <include file='doc\PrinterSettings.uex' path='docs/doc[@for="StringCollection.Add"]/*' />
            /// <devdoc>        
            /// Empty implementation required for serialization of PrinterSettings object.
            /// </devdoc>
            /// <internalonly/>
            [
                EditorBrowsable(EditorBrowsableState.Never)
            ]
            public Int32 Add(String value)
            {
                String[] newArray = new String[this.Count + 1];
                ((ICollection) this).CopyTo(newArray, 0);
                newArray[this.Count] = value;
                this.array = newArray;
                return this.Count;
            }
        }

        private class ArrayEnumerator : IEnumerator {
            private object[] array;
            private object item;
            private int index;
            private int startIndex;
            private int endIndex;

            public ArrayEnumerator(object[] array, int startIndex, int count) {
                this.array = array;
                this.startIndex = startIndex;
                endIndex = index + count;

                index = this.startIndex;
            }

            public object Current {
                get {
                    return item;
                }                    
            }


            public bool MoveNext() {
                if (index >= endIndex) return false;
                item = array[index++];
                return true;
            }

            public void Reset() {

                // Position enumerator before first item 

                index = startIndex;
                item = null;
            }
        }
    }
}


