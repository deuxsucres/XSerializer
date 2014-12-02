using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace deuxsucres.XSerializer
{
    /// <summary>
    /// Simple XDocument serializer/deserializer
    /// </summary>
    public class XDocSerializer
    {
        #region Private fields
        private CultureInfo _Culture;
        #endregion

        #region Ctors & Dests

        /// <summary>
        /// Create a new serializer
        /// </summary>
        public XDocSerializer()
        {
            _Culture = CultureInfo.InvariantCulture;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the culture used when reading XML. Defaults to <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public virtual CultureInfo Culture
        {
            get { return _Culture ?? CultureInfo.InvariantCulture; }
            set { _Culture = value; }
        }

        #endregion
    }
}
