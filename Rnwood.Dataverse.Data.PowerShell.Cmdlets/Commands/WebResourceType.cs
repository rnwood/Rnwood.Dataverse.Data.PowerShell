using System;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Specifies the type of web resource in Dataverse.
    /// </summary>
    public enum WebResourceType
    {
        /// <summary>
        /// HTML web page (.htm, .html)
        /// </summary>
        HTML = 1,

        /// <summary>
        /// Cascading Style Sheet (.css)
        /// </summary>
        CSS = 2,

        /// <summary>
        /// JavaScript (.js)
        /// </summary>
        JavaScript = 3,

        /// <summary>
        /// XML data (.xml)
        /// </summary>
        XML = 4,

        /// <summary>
        /// PNG image (.png)
        /// </summary>
        PNG = 5,

        /// <summary>
        /// JPEG image (.jpg, .jpeg)
        /// </summary>
        JPG = 6,

        /// <summary>
        /// GIF image (.gif)
        /// </summary>
        GIF = 7,

        /// <summary>
        /// Silverlight application (.xap)
        /// </summary>
        XAP = 8,

        /// <summary>
        /// XSL Stylesheet (.xsl, .xslt)
        /// </summary>
        XSL = 9,

        /// <summary>
        /// ICO icon (.ico)
        /// </summary>
        ICO = 10,

        /// <summary>
        /// SVG image (.svg)
        /// </summary>
        SVG = 11,

        /// <summary>
        /// RESX resource file (.resx)
        /// </summary>
        RESX = 12
    }
}
