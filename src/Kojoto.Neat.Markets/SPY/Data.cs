using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Kojoto.Markets.SPY
{

    public class Data
    {
        NameValueCollection _AppSettings = ConfigurationManager.AppSettings;

        public string Filename
        {
            get
            {
                return _AppSettings["SPY_DATA_FILE"];
            }
        }
    }
}
