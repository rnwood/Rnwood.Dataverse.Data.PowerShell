using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnwood.Dataverse.Data.PowerShell.Cmdlets.Commands.Model
{
    internal class ZipFileNameEncoding : UTF8Encoding
    {
        public ZipFileNameEncoding() : base(true)
        {

        }
        public override byte[] GetBytes(string s)
        {
            s = s.Replace("\\", "/");
            return base.GetBytes(s);
        }
    }


}
