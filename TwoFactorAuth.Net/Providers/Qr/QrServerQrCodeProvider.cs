﻿using System;
using System.Drawing;
using System.Net.Security;

namespace TwoFactorAuthNet.Providers.Qr
{
    //TODO: implement charset-source / charset-target?

    /// <summary>
    /// Provides QR codes generated by GoQR.Me (qrserver.com).
    /// </summary>
    /// <seealso href="http://goqr.me/api/doc/create-qr-code/"/>.
    public class QrServerQrCodeProvider : BaseHttpQrCodeProvider, IQrCodeProvider
    {
        /// <summary>
        /// Represents the filetype to be returned.
        /// </summary>
        public enum QrServerImageFormat
        {
            /// <summary>PNG</summary>
            Png,
            /// <summary>GIF</summary>
            Gif,
            /// <summary>JPEG</summary>
            Jpeg,
            /// <summary>SVG</summary>
            Svg,
            /// <summary>EPS</summary>
            Eps
        }

        /// <summary>
        /// Gets the <see cref="ErrorCorrectionLevel"/> for the QR code.
        /// </summary>
        public ErrorCorrectionLevel ErrorCorrectionLevel { get; private set; }

        /// <summary>
        /// Gets the background color to be used for the QR code.
        /// </summary>
        public Color BackgroundColor { get; private set; }

        /// <summary>
        /// Gets the foreground color to be used for the QR code.
        /// </summary>
        public Color ForegroundColor { get; private set; }

        /// <summary>
        /// Gets the thickness of a margin in pixels.
        /// </summary>
        /// <remarks>
        /// The margin will always have the same color as the background (you can configure this via 
        /// <see cref="BackgroundColor"/>). It will not be added to the width of the image set by size, therefore it
        /// has to be smaller than at least one third of the size value. The margin will be drawn in addition to an
        /// optionally set <see cref="QuietZone"/> value. The margin parameter will be ignored if 
        /// <see cref="QrServerImageFormat.Svg"/> or <see cref="QrServerImageFormat.Eps"/> is used as QR code format 
        /// (e.g. the QR code output is a vector graphic).
        /// </remarks>
        public int Margin { get; private set; }

        /// <summary>
        /// Gets the thickness of a "quiet zone", an area without disturbing elements to help readers locating the QR
        /// code, in modules as measuring unit.
        /// </summary>
        /// <remarks>
        /// Measuring unit means a value of 1 leads to a drawn margin around the QR code which is as thick as a data
        /// pixel/module of the QR code. The quiet zone will always have the same color as the background (you can
        /// configure this via <see cref="BackgroundColor"/>). The quiet zone will be drawn in addition to an
        /// optionally set margin value.
        /// </remarks>
        public int QuietZone { get; private set; }

        /// <summary>
        /// Gets the <see cref="QrServerImageFormat"/> of the QR code.
        /// </summary>
        public QrServerImageFormat ImageFormat { get; private set; }

        /// <summary>
        /// <see cref="BaseHttpQrCodeProvider.BaseUri"/> for this QR code provider.
        /// </summary>
        private static readonly Uri baseuri = new Uri("https://api.qrserver.com/v1/create-qr-code/");

        /// <summary>
        /// Initializes a new instance of a <see cref="QrServerQrCodeProvider"/> with the specified
        /// <see cref="ErrorCorrectionLevel"/>, <see cref="Margin"/>, <see cref="QuietZone"/>, 
        /// <see cref="BackgroundColor"/>, <see cref="ForegroundColor"/>,
        /// <see cref="QrServerImageFormat">ImageFormat</see> and <see cref="RemoteCertificateValidationCallback"/>.
        /// </summary>
        /// <param name="errorCorrectionLevel">The <see cref="ErrorCorrectionLevel"/> to use when generating QR codes.</param>
        /// <param name="margin">The <see cref="Margin"/> to be used for the QR code.</param>
        /// <param name="quietZone">The <see cref="QuietZone"/> to be used for the QR code.</param>
        /// <param name="backgroundColor">The background color to be used for the QR code.</param>
        /// <param name="foregroundColor">The foreground color to be used for the QR code.</param>
        /// <param name="imageFormat">The <see cref="QrServerImageFormat"/> of the QR code.</param>
        /// <param name="remoteCertificateValidationCallback">
        /// The <see cref="RemoteCertificateValidationCallback"/> to use when generating QR codes.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when an invalid <see cref="ErrorCorrectionLevel"/> or <see cref="QrServerImageFormat"/> is specified,
        /// <paramref name="margin"/> is less than 0 or more than 50 or <paramref name="quietZone"/> is less than 0 or
        /// more than 100.
        /// </exception>
        public QrServerQrCodeProvider(
            ErrorCorrectionLevel errorCorrectionLevel = ErrorCorrectionLevel.Low, 
            int margin = 4, 
            int quietZone = 1, 
            Color? backgroundColor = null, 
            Color? foregroundColor = null, 
            QrServerImageFormat imageFormat = QrServerImageFormat.Png, 
            RemoteCertificateValidationCallback remoteCertificateValidationCallback = null
        )
            : base(baseuri, remoteCertificateValidationCallback)
        {
            if (!Enum.IsDefined(typeof(ErrorCorrectionLevel), errorCorrectionLevel))
                throw new ArgumentOutOfRangeException(nameof(errorCorrectionLevel));
            ErrorCorrectionLevel = errorCorrectionLevel;

            if (margin < 0 || margin > 50)
                throw new ArgumentOutOfRangeException(nameof(margin));
            Margin = margin;

            if (quietZone < 0 || quietZone > 100)
                throw new ArgumentOutOfRangeException(nameof(quietZone));
            QuietZone = quietZone;

            BackgroundColor = backgroundColor ?? Color.White;
            ForegroundColor = foregroundColor ?? Color.Black;

            if (!Enum.IsDefined(typeof(QrServerImageFormat), imageFormat))
                throw new ArgumentOutOfRangeException(nameof(imageFormat));
            ImageFormat = imageFormat;

        }

        /// <summary>
        /// Gets the MIME type of the image.
        /// </summary>
        /// <returns>Returns the MIME type of the image.</returns>
        /// <seealso cref="IQrCodeProvider"/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an unknown <see cref="QrServerImageFormat"/> is used.
        /// </exception>
        public string GetMimeType()
        {
            switch (ImageFormat)
            {
                case QrServerImageFormat.Png:
                    return "image/png";
                case QrServerImageFormat.Gif:
                    return "image/gif";
                case QrServerImageFormat.Jpeg:
                    return "image/jpeg";
                case QrServerImageFormat.Svg:
                    return "image/svg+xml";
                case QrServerImageFormat.Eps:
                    return "application/postscript";
            }
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            throw new InvalidOperationException("Unknown imageformat");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        }

        /// <summary>
        /// Downloads / retrieves / generates a QR code as image.
        /// </summary>
        /// <param name="text">The text to encode in the QR code.</param>
        /// <param name="size">The desired size (width and height equal) for the image.</param>
        /// <returns>Returns the binary representation of the image.</returns>
        /// <seealso cref="IQrCodeProvider"/>
        public byte[] GetQrCodeImage(string text, int size)
        {
            return DownloadData(GetUri(text, size));
        }

        /// <summary>
        /// Builds an <see cref="Uri"/> based on the instance's <see cref="BaseHttpQrCodeProvider.BaseUri"/>.
        /// </summary>
        /// <param name="qrText">The text to encode in the QR code.</param>
        /// <param name="size">The desired size of the QR code.</param>
        /// <returns>A <see cref="Uri"/> to the QR code.</returns>
        private Uri GetUri(string qrText, int size)
        {
            return new Uri(BaseUri,
                "?size=" + size + "x" + size
                + "&ecc=" + Char.ToUpperInvariant(((char)ErrorCorrectionLevel))
                + "&margin=" + Margin
                + "&qzone=" + QuietZone
                + "&bgcolor=" + Color2Hex(BackgroundColor)
                + "&color=" + Color2Hex(ForegroundColor)
#pragma warning disable CA1308 // Normalize strings to uppercase
                + "&format=" + Enum.GetName(typeof(QrServerImageFormat), ImageFormat).ToLowerInvariant()
#pragma warning restore CA1308 // Normalize strings to uppercase
                + "&data=" + Uri.EscapeDataString(qrText)
            );
        }
    }
}
