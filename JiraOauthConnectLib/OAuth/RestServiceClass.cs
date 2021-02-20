using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace JiraOAuthConnectLib
{
    public partial class RestServiceClass
    {
        #region Private global variables

        private static Random random = new Random();
        WebProxy myProxy;

        #endregion




        /// <summary>
        /// This is the starting method of this class. This method will construct the URL, give call to REST service and collect response
        /// </summary>
        /// <param name="url">URL os the REST service location. Example : "https://www.RESTserviceProvider.com/v1/atm?querystring1&querystring2";</param>
        /// <param name="method">For Request token and Access token method should be POST. Example : "GET, POST";</param>
        /// <param name="verificationCode"> Verification code receieved after authorizing the request token. Copy this from the browser. For request token, pass string.empty Example : "'asdb'";</param>
        /// <param name="token">To get access token, pass the request token, to get the request token pass string.empty Example : "https://www.RESTserviceProvider.com/v1/atm?querystring1&querystring2";</param>
        /// <returns>Returns the Response XML in string form received from  service.</returns>
        public string SignUrlAndSendRequest(string url, string method, string verificationCode, string token, string data = "")
        {
            var strOriginalUrl = url;

            try
            {
                //remove all spaces from URL string and encode the query string values.
                string strFormattedUrl = FormatCorrectUrl(strOriginalUrl);

                //Encryption Signature method. In our case its "RSA-SHA1"
                string signatureMethod = "RSA-SHA1";

                //Request HTTP method.=GET
                string strHttpMethod = method;

                // Consumer Id key used for access. This id is set while setting contract up contract with REST service provider.
                // This ID is always in encrypted format. for example : _rt_EbYznlcNxc5Z8uslIVNFrtSE3d45SDry-bh83hsgus73
                string strConsumerKey = "OauthKey-test";

                //1. Construct the Signature base string and Oauth signature.
                AuthorizeHeader authorizationHeader = GetRequestTokenAuthorizationHeader(strFormattedUrl, signatureMethod, strHttpMethod, strConsumerKey, "", verificationCode, token);

                //2. Make a REST service call and collect resposne.
                var result = MakeRESTRequest(strFormattedUrl, method, authorizationHeader, data);
                return result;
            }

            catch (Exception ex)
            {
                throw ex;

            }

        }

        #region Private methods Responsible for OAuth Generation and REST call

        /// <summary>
        /// Give a REST based Service call to REST service provider and collect response in string form
        /// </summary>
        /// <param name="url">URL with end point and query string parameters.</param>
        /// <param name="authorizationHeader">Authorization class object which hold all OAuth parameter properties.</param>
        /// <returns>REST Service response in string form </returns>
        private string MakeRESTRequest(string strUrl, string method, AuthorizeHeader objAuthorizationHeader, string data = "")
        {

            string strResponseResult = string.Empty;
            try
            {
                HttpWebRequest requestObject = CreateWebRequestObject(strUrl, method, objAuthorizationHeader);

                // Create POST data and convert it to a byte array.
                //This is used only in case of POST and creation of Task.
                if (!string.IsNullOrEmpty(data))
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(data);
                    // Set the ContentType property of the WebRequest.
                    requestObject.ContentType = "application/json";
                    // Set the ContentLength property of the WebRequest.
                    requestObject.ContentLength = byteArray.Length;

                    using (var stream = requestObject.GetRequestStream())
                    {
                        // Write the data to the request stream.
                        stream.Write(byteArray, 0, byteArray.Length);
                    }
                    // Get the request stream.
                }

                return SendHttpRequest(ref strResponseResult, requestObject);
            }
            catch (WebException e)
            {
                throw e;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private static string SendHttpRequest(ref string strResponseResult, HttpWebRequest requestObject)
        {
            // Raise the HTTP REST request and collect the response using stream object
            using (Stream objStream = requestObject.GetResponse().GetResponseStream())
            {

                // Read the Stream object using stream reader.
                using (StreamReader objReader = new StreamReader(objStream))
                {
                    string strResLine = string.Empty;

                    int i = 0;

                    // Parse the response to gain Complete xml response in string form
                    while (strResLine != null)
                    {
                        i++;
                        strResLine = objReader.ReadLine();
                        if (strResLine != null) { strResponseResult += strResLine; }
                    }
                }
            }

            // return the success response.
            return strResponseResult;
        }

        private HttpWebRequest CreateWebRequestObject(string strUrl, string method, AuthorizeHeader objAuthorizationHeader)
        {
            // Get the URL details before making REST call.
            string strNormalizedEndpoint = NormalizeUrl(strUrl);

            HttpWebRequest requestObject = (HttpWebRequest)WebRequest.Create(strNormalizedEndpoint);

            // Retrieve the Authorization Header string
            string strAuthorizationHeader = string.Empty;
            if (objAuthorizationHeader != null)
            {
                strAuthorizationHeader = objAuthorizationHeader.ToString();
            }
            requestObject.Method = method;
            // Note : The REST service call always expect a Authorization signature part along with the request header.
            // So, Add the oauth signature in request header under name "Authorization"
            requestObject.Headers.Add("Authorization", strAuthorizationHeader);

            //// Assign the proxy setting to request.
            requestObject.Proxy = myProxy;
            // Setup the request timeout period
            // Wait for 2 mins. to get back response. 
            requestObject.Timeout = 12000;
            //// IF proxy details required then Put Proxy details in request header.

            if (WebRequest.DefaultWebProxy.GetProxy(new Uri(strUrl)).ToString() != strUrl)
            {
                IWebProxy iProxy = WebRequest.DefaultWebProxy;
                myProxy = new WebProxy(iProxy.GetProxy(new Uri(strUrl)));
                myProxy.UseDefaultCredentials = true;
            }
            return requestObject;
        }

        public bool PostAttachments(string restUrl, IEnumerable<FileInfo> filePaths, string accessToken)
        {
            try
            {
                string boundary;
                MemoryStream content;

                CreateMemoryStreamFromFiles(filePaths, out boundary, out content);

                string strResponseResult = string.Empty;

                // Get the URL details before making REST call.
                string strNormalizedEndpoint = NormalizeUrl(restUrl);
                string method = "POST";
                //remove all spaces from URL string and encode the query string values.
                string strFormattedUrl = FormatCorrectUrl(restUrl);
                //1. Construct the Signature base string and Oauth signature.
                AuthorizeHeader authorizationHeader = GetRequestTokenAuthorizationHeader(strFormattedUrl, "RSA-SHA1", "POST", "OauthKey-test", "", "", accessToken);
                HttpWebRequest requestObject = CreateWebRequestObject(restUrl, method, authorizationHeader);

                requestObject.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
                requestObject.Headers.Add("X-Atlassian-Token", "nocheck");
                requestObject.ContentLength = content.Length;

                //Add the data to the request object's stream
                using (Stream requestStream = requestObject.GetRequestStream())
                {
                    content.WriteTo(requestStream);
                    requestStream.Close();
                }



                SendHttpRequest(ref strResponseResult, requestObject);

                // return the success response.
                return true;
            }

            catch (WebException e)
            {
                throw e;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {

            }

        }

        private void CreateMemoryStreamFromFiles(IEnumerable<FileInfo> filePaths, out string boundary, out MemoryStream content)
        {
            boundary = string.Format("----------{0:N}", Guid.NewGuid());
            content = new MemoryStream();
            var writer = new StreamWriter(content);

            foreach (var filePath in filePaths)
            {
                var fs = new FileStream(filePath.FullName, FileMode.Open, FileAccess.Read);
                var data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();

                writer.WriteLine("--{0}", boundary);
                writer.WriteLine("Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"", filePath.Name);
                writer.WriteLine("Content-Type: application/octet-stream");
                writer.WriteLine();
                writer.Flush();

                content.Write(data, 0, data.Length);

                writer.WriteLine();
            }

            writer.WriteLine("--" + boundary + "--");
            writer.Flush();
            content.Seek(0, SeekOrigin.Begin);
        }



        /// <summary>
        /// Control the flow of OAuth and Signature base string generation.
        /// </summary>
        /// <param name="strUrl">The string URL where system will make REST call. The URL is combination of URL endpoint and query string parameters.</param>
        /// <param name="signatureMethod">The REST service call signature method. In this case its RSA-SHA</param>
        /// <param name="httpMethod">The httpmethod call. The REST work only with 'GET' method</param>
        /// <param name="consumerKey">The client ID or user ID value for respective REST based service provider</param>
        /// <param name="strRealm">Ther RELM value. This is blank in this case.</param>
        /// <returns></returns>
        public AuthorizeHeader GetRequestTokenAuthorizationHeader(string strUrl, string signatureMethod, string httpMethod, string consumerKey, string strRealm, string verificationCode, string requestToken)
        {
            try
            {
                /// NOTE :- The final REST service URL should contain the query string parameters as well as OAUTH parameters sorted in ascending order
                ///         So first create a list query string parameters in sorted order then create a list of OAuth parameters.
                ///         Then combine both these list into master list.

                //------------------------------------------------------------------------------------------------
                // 1. Rearrange the list of query string parameters in ascending order
                //------------------------------------------------------------------------------------------------

                // Get the list of query string(location search) parameters
                List<QueryParameter> searchParameters = ExtractQueryStrings(strUrl);


                //------------------------------------------------------------------------------------------------
                // 2. Get all oauth parameters and then Rearrange the list in ascending order
                //------------------------------------------------------------------------------------------------
                // Get the Current time stamp of Machine.
                string strTimeStamp = GenerateTimeStamp();

                // Generate the nounce based on timestamp
                string strNounce = GenerateNonce(strTimeStamp);



                // Arrange the list of Parameters and save in key-value pair format
                List<QueryParameter> oauthParameters = new List<QueryParameter>();
                oauthParameters.Add(new QueryParameter(OAuthProtocolParameter.ConsumerKey.GetStringValue(), consumerKey));
                oauthParameters.Add(new QueryParameter(OAuthProtocolParameter.SignatureMethod.GetStringValue(), signatureMethod));
                oauthParameters.Add(new QueryParameter(OAuthProtocolParameter.Timestamp.GetStringValue(), strTimeStamp));
                oauthParameters.Add(new QueryParameter(OAuthProtocolParameter.Nounce.GetStringValue(), strNounce));
                oauthParameters.Add(new QueryParameter(OAuthProtocolParameter.Version.GetStringValue(), "1.0"));
                oauthParameters.Add(new QueryParameter(OAuthProtocolParameter.Callback.GetStringValue(), "oob"));
                if (!string.IsNullOrEmpty(verificationCode))
                    oauthParameters.Add(new QueryParameter(OAuthProtocolParameter.Verifier.GetStringValue(), verificationCode));

                if (!string.IsNullOrEmpty(requestToken))
                    oauthParameters.Add(new QueryParameter(OAuthProtocolParameter.Token.GetStringValue(), requestToken));
                // Sort the OAuth string parameters in ascending order.[This is a standard in REST service call]
                oauthParameters.Sort(new LexicographicComparer());


                // Master list will contain the sorted query string parameters followed by OAuth parameters.
                List<QueryParameter> MasterParameterList = new List<QueryParameter>();

                //Create the master list of Parameters. Search Parameters followed by Oauth parameters
                MasterParameterList.AddRange(searchParameters);
                MasterParameterList.AddRange(oauthParameters);

                MasterParameterList.Sort(new LexicographicComparer());

                //------------------------------------------------------------------------------------------------
                // 3. All parameters are arranged so now generate the Signature base string.
                //------------------------------------------------------------------------------------------------

                // Createte the Signature Base string which can be encrypted by RSA to form OAuth_signature.
                string strSignatureBaseString = GenerateSignatureBaseString(strUrl, httpMethod, MasterParameterList);

                //------------------------------------------------------------------------------------------------
                // 4. Based on the signature base string, generate the encrypted version of signature using RSA cryptography
                //------------------------------------------------------------------------------------------------

                // Create the Oauth_signature using Private Key, signature base string and RSA cryptograph.                
                string strOauth_Signature = GenerateSignature(strSignatureBaseString);

                // Generate the Authorizantion Section and return header.
                return new AuthorizeHeader(strRealm, consumerKey, signatureMethod, strOauth_Signature, strTimeStamp, strNounce, "1.0", "oob", verificationCode, requestToken);
            }

            catch (CryptographicException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This will form the URL with Querystring and OAuth parameters and restructure the complete URL before making REST service call.
        /// This function is called before making REST service call and returns the URL with endpoint followed by Querystring parameters.
        /// </summary>
        /// <param name="strUrl">String URL</param>
        /// <returns>Returns the URL with endpoint followed by Querystring parameters.</returns>
        private string NormalizeUrl(string strUrl)
        {
            StringBuilder result = null;
            try
            {
                //Check the index of '?'
                int questionIndex = strUrl.IndexOf('?');
                if (questionIndex == -1)
                {
                    return strUrl;
                }
                var parameters = strUrl.Substring(questionIndex + 1);
                result = new StringBuilder();
                result.Append(strUrl.Substring(0, questionIndex + 1));

                bool hasQueryParameters = false;
                if (!String.IsNullOrEmpty(parameters))
                {
                    string[] parts = parameters.Split('&');
                    hasQueryParameters = parts.Length > 0;
                    foreach (var part in parts)
                    {
                        var nameValue = part.Split('=');
                        result.Append(nameValue[0] + "=");
                        if (nameValue.Length == 2)
                        {
                            result.Append(nameValue[1]);
                        }
                        result.Append("&");
                    }
                    if (hasQueryParameters)
                    {
                        result = result.Remove(result.Length - 1, 1);
                    }
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// This function removes all empty spaces and url encode the values.
        /// This function is called soon after receiving the URL from REST base class to url encode the fields like Address, State which may contain the values 
        /// with spaces.
        /// </summary>
        /// <param name="strUrl">String URL with quesry string values having spaces.</param>
        /// <returns>Returns the URL with endpoint followed by URL encoded Querystring parameters.</returns>
        private string FormatCorrectUrl(string strUrl)
        {
            StringBuilder result = null;
            try
            {
                //Check the index of '?'
                int questionIndex = strUrl.IndexOf('?');
                if (questionIndex == -1)
                {
                    return strUrl;
                }
                var parameters = strUrl.Substring(questionIndex + 1);
                result = new StringBuilder();
                result.Append(strUrl.Substring(0, questionIndex + 1));

                bool hasQueryParameters = false;
                if (!String.IsNullOrEmpty(parameters))
                {
                    string[] parts = parameters.Split('&');
                    hasQueryParameters = parts.Length > 0;

                    foreach (var part in parts)
                    {
                        var nameValue = part.Split('=');
                        if (!nameValue[0].Equals(string.Empty))
                        {
                            // Append querystring field
                            result.Append(nameValue[0] + "=");

                        }
                        if (nameValue.Length == 2)
                        {
                            // Append querystring field value
                            result.Append(UrlEncode(nameValue[1]));
                            result.Append("&");
                        }
                    }
                    if (hasQueryParameters)
                    {
                        result = result.Remove(result.Length - 1, 1);
                    }
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Generate the Timestamp value in milliseconds form base 1970
        /// </summary>
        /// <returns>Timestamp value in seconds string form</returns>
        private string GenerateTimeStamp()
        {
            try
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1);
                return Math.Truncate(ts.TotalSeconds).ToString();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Generate the Random number between 0 - 100000000 and append after timestamp string value
        /// </summary>
        /// <param name="timestamp">Timestamp value in string</param>
        /// <returns>nonce value as Timestamp<appended by>random number</appended></returns>
        private string GenerateNonce(string strTimeStamp)
        {
            try
            {
                Random random = new Random();
                Int64 randomNumber = random.Next(0, 100000000);
                return strTimeStamp.ToString() + randomNumber.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Extract the Key-Value data for query string from List object and 
        /// construct a single string of query string parameters seperated by '&' sign
        /// </summary>
        /// <param name="parameters">List object containing query parameters in key-value form</param>
        /// <returns>String of quesry string parameters in key=value form appended by &</returns>
        private string NormalizeProtocolParameters(IList<QueryParameter> parameters)
        {
            try
            {
                StringBuilder sbResult = new StringBuilder();
                QueryParameter p = null;
                for (int i = 0; i < parameters.Count; i++)
                {
                    p = parameters[i];
                    //sb.AppendFormat("{0}={1}", p.Name, UrlEncode(p.Value));
                    sbResult.AppendFormat("{0}={1}", p.Name, p.Value);
                    if (i < parameters.Count - 1)
                    {
                        sbResult.Append("&");
                    }
                }
                return sbResult.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This class extract the query string search parameters and add then in QueryParameter array list with Key-Value pair.        
        /// </summary>
        /// <param name="strUrl">URL containing the endpoints followed by the query string parameters</param>
        /// <returns>List object containing query string parameters in key-value form.</returns>
        private List<QueryParameter> ExtractQueryStrings(string strUrl)
        {
            try
            {
                int questionIndex = strUrl.IndexOf('?');
                if (questionIndex == -1)
                    return new List<QueryParameter>();

                string strParameters = strUrl.Substring(questionIndex + 1);
                var result = new List<QueryParameter>();

                if (!String.IsNullOrEmpty(strParameters))
                {
                    string[] parts = strParameters.Split('&');
                    foreach (string part in parts)
                    {
                        if (!string.IsNullOrEmpty(part) && !part.StartsWith("oauth_"))
                        {
                            if (part.IndexOf('=') != -1)
                            {
                                string[] nameValue = part.Split('=');
                                result.Add(new QueryParameter(nameValue[0], nameValue[1]));
                            }
                            else
                                result.Add(new QueryParameter(part, String.Empty));
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// The Method generates the Base string by appending HTTP Method, URL and Querys string parameters.
        /// </summary>
        /// <param name="url">Service url</param>
        /// <param name="httpMethod">GET</param>
        /// <param name="protocolParameters">Query string parameters</param>
        /// <returns>Signature base string. </returns>
        private string GenerateSignatureBaseString(string strUrl, string strHttpMethod, List<QueryParameter> protocolParameters)
        {
            StringBuilder sbSignatureBase = new StringBuilder();
            try
            {
                Uri uri = new Uri(strUrl);
                string strNormalizedUrl = string.Format("{0}://{1}", uri.Scheme, uri.Host);

                /**
                 * Calculates the normalized request url, 
                 * This removes the querystring from the url and the port (if it is the standard http or https port).              
                 **/

                if (!((uri.Scheme == "http" && uri.Port == 80) || (uri.Scheme == "https" && uri.Port == 443)))
                    strNormalizedUrl += ":" + uri.Port;
                strNormalizedUrl += uri.AbsolutePath;

                // Get the query string parameter string seperated by '&' sign
                string strNormalizedRequestParameters = NormalizeProtocolParameters(protocolParameters);

                sbSignatureBase.AppendFormat("{0}&", strHttpMethod);
                // Always URL Encode the string
                sbSignatureBase.AppendFormat("{0}&", UrlEncode(strNormalizedUrl));
                sbSignatureBase.AppendFormat("{0}", UrlEncode(strNormalizedRequestParameters));


                return sbSignatureBase.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        /// <summary>
        /// Encrypt the Signature base string using RSA-SHA1 algorithm and return Base64 & UTF encoded string.
        /// </summary>
        /// <param name="signatureBaseString">Signature base string</param>
        /// <returns>Base 64 and encoded string</returns>
        public string GenerateSignature(string strSignatureBaseString)
        {
            /// ------------------------------------------------------------------------------------
            // This is perfect code to generate the Encrypted string from signature base string.
            // This is very sensative code.Do not make any new change in this code
            /// ------------------------------------------------------------------------------------
            SHA1Managed shaHASHObject = null;
            try
            {

                // Read the .P12 file to read Private/Public key Certificate

                string certFilePath = @"C:\Users\Admin\Desktop\Jira Integration\jira_publickey\jira_privatekey.pem";

                var privateKey = PemReaderB.GetRSAProviderFromPemFile(certFilePath);


                // Retrieve the Private key from Certificate.
                RSACryptoServiceProvider RSAcrypt = privateKey;


                // Create a RSA-SHA1 Hash object           
                shaHASHObject = new SHA1Managed();

                // Create Byte Array of Signature base string
                byte[] data = System.Text.Encoding.ASCII.GetBytes(strSignatureBaseString);

                // Create Hashmap of Signature base string
                byte[] hash = shaHASHObject.ComputeHash(data);

                // Create Sign Hash of base string 
                // NOTE -  'SignHash' gives correct data. Don't use SignData method
                byte[] rsaSignature = RSAcrypt.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));

                // Convert to Base64 string
                string base64string = Convert.ToBase64String(rsaSignature);

                // Return the Encoded UTF8 string
                return UrlEncode(base64string);
            }
            catch (CryptographicException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // clear the memory allocation               
                if (shaHASHObject != null)
                {
                    shaHASHObject.Dispose();
                }
            }
        }


        /// <summary>
        /// This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
        /// </summary>
        /// <param name="strValue">The value to Url encode</param>
        /// <returns>Returns a Url encoded string</returns>
        private string UrlEncode(string strValue)
        {
            var encodedUrl = HttpUtility.UrlEncode(strValue);
            // list of reserved character string which need to encode
            string reservedCharacters = " !*'();:@&=+$,/?%#[]";

            try
            {
                if (String.IsNullOrEmpty(strValue))
                    return String.Empty;

                StringBuilder sbResult = new StringBuilder();

                foreach (char @char in strValue)
                {
                    if (reservedCharacters.IndexOf(@char) == -1)
                        sbResult.Append(@char.ToString());
                    else
                    {
                        sbResult.AppendFormat("%{0:X2}", (int)@char);
                    }
                }
                return sbResult.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Private methods

    }
}
