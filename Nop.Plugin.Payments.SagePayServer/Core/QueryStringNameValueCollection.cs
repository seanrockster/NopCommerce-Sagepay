using System.Text;
using System.Collections.Specialized;
using System.Web;

namespace Nop.Plugin.Payments.SagePayServer.Core
{
    public sealed class QueryStringNameValueCollection : NameValueCollection
    {
        private const string Ampersand = "&";
        private const string EQUALS = "=";
        private static readonly char[] AmpersandCharArray = Ampersand.ToCharArray();
        private static readonly char[] EQUALSCharArray = EQUALS.ToCharArray();

        /// <summary>
        /// Returns the built NVP string of all name/value pairs in the Hashtable
        /// </summary>
        /// <returns></returns>
        public string Encode()
        {
            var sb = new StringBuilder();
            var firstPair = true;
            foreach (var kv in AllKeys)
            {
                var name = UrlEncode(kv);
                var value = UrlEncode(this[kv]);
                if (!firstPair)
                {
                    sb.Append(Ampersand);
                }
                sb.Append(name).Append(EQUALS).Append(value);
                firstPair = false;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Decoding the string
        /// </summary>
        /// <param name="nvpstring"></param>
        public void Decode(string nvpstring)
        {
            Clear();
            foreach (var nvp in nvpstring.Split(AmpersandCharArray))
            {
                var tokens = nvp.Split(EQUALSCharArray);
                if (tokens.Length >= 2)
                {
                    var name = UrlDecode(tokens[0]);
                    var value = UrlDecode(tokens[1]);
                    Add(name, value);
                }
            }
        }

        private static string UrlDecode(string s) { return HttpUtility.UrlDecode(s); }
        private static string UrlEncode(string s) { return HttpUtility.UrlEncode(s); }

    }
}
