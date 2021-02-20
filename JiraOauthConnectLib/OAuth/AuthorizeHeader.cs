using System.Text;

namespace JiraOAuthConnectLib
{
    /// <summary>

    /// The Authorization header class. 

    /// The Header class .ToString() method will return composit value of OAuth details which need to append along with REST request header.

    /// </summary>

    public class AuthorizeHeader

    {

        public string Realm { get; private set; }

        public string ConsumerKey { get; private set; }

        public string SignatureMethod { get; private set; }

        public string Signature { get; private set; }

        public string Timestamp { get; private set; }

        public string Nounce { get; private set; }

        public string Version { get; private set; }

        public string BodyHash { get; private set; }

        public string Callback { get; private set; }

        public string Token { get; private set; }

        public string Verifier { get; private set; }

        /// <summary>

        /// Constructor to initialize the OAUTH data.

        /// </summary>

        /// <param name="realm">RELM value.(Optional) This value is not required in REST using GET method.</param>

        /// <param name="consumerKey">Customer ID required for accessing the REST service. Mandetory string value </param>

        /// <param name="signatureMethod">The REST service call signature method. In this case its "RSA-SHA"</param>

        /// <param name="signature">The encrypted signature value which is calculated based on OAuth parameters, Query string parameters and RSA cryptography. 

        /// This signature value is cross checked on REST service provider end to validate the REST request</param>

        /// <param name="timestamp">Timestamp value indicating when the request is raised</param>

        /// <param name="nounce">Some unique value calculated runtime based on timestamp</param>

        /// <param name="version">OAUTH version value "1.0"</param>

        public AuthorizeHeader(string realm, string consumerKey, string signatureMethod, string signature, string timestamp, string nounce, string version, string callback, string verificationCode, string requestToken)

        {

            Realm = realm;

            ConsumerKey = consumerKey;

            SignatureMethod = signatureMethod;

            Signature = signature;

            Timestamp = timestamp;

            Nounce = nounce;

            Version = version;

            // This field is not applicable in HTTP GET request. So keep it null

            BodyHash = null;

            Callback = callback;

            Verifier = verificationCode;

            Token = requestToken;

        }



        public override string ToString()

        {

            // Construct the OAuth parameters to set at Request Header Authorization part.

            var sb = new StringBuilder();

            sb.Append("OAuth ");

            sb.AppendFormat("realm=\"{0}\", ", Realm);

            sb.AppendFormat("{0}=\"{1}\", ", OAuthProtocolParameter.ConsumerKey.GetStringValue(), ConsumerKey); // Mandetory Input

            sb.AppendFormat("{0}=\"{1}\", ", OAuthProtocolParameter.Nounce.GetStringValue(), Nounce); // Mandetory Input

            sb.AppendFormat("{0}=\"{1}\", ", OAuthProtocolParameter.Timestamp.GetStringValue(), Timestamp); // Mandetory Input

            sb.AppendFormat("{0}=\"{1}\", ", OAuthProtocolParameter.Version.GetStringValue(), Version);

            // If you are using "GET" method then BodyHash parameter will cause serious exception. So Always skip it in signature.

            //////sb.AppendFormat("{0}=\"{1}\", ", OAuthProtocolParameter.BodHash.GetStringValue(), BodyHash); 

            sb.AppendFormat("{0}=\"{1}\", ", OAuthProtocolParameter.SignatureMethod.GetStringValue(), SignatureMethod); // Mandetory Input

            sb.AppendFormat("{0}=\"{1}\", ", OAuthProtocolParameter.Signature.GetStringValue(), Signature);

            sb.AppendFormat("{0}=\"{1}\", ", OAuthProtocolParameter.Callback.GetStringValue(), Callback);


            if (!string.IsNullOrEmpty(Verifier))
                sb.AppendFormat("{0}=\"{1}\", ", OAuthProtocolParameter.Verifier.GetStringValue(), Verifier);

            if (!string.IsNullOrEmpty(Token))
                sb.AppendFormat("{0}=\"{1}\", ", OAuthProtocolParameter.Token.GetStringValue(), Token);

            sb = sb.Remove(sb.Length - 2, 2);

            return sb.ToString();

        }

    }


}
