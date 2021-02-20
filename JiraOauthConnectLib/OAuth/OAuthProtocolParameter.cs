namespace JiraOAuthConnectLib
{
    /// <summary>

    /// Enumerator for OAuth parameters

    /// </summary> 

    internal enum OAuthProtocolParameter

    {

        [EnumStringValueAttribute("oauth_consumer_key")]

        ConsumerKey,

        [EnumStringValueAttribute("oauth_signature_method")]

        SignatureMethod,

        [EnumStringValueAttribute("oauth_signature")]

        Signature,

        [EnumStringValueAttribute("oauth_timestamp")]

        Timestamp,

        [EnumStringValueAttribute("oauth_nonce")]

        Nounce,

        [EnumStringValueAttribute("oauth_version")]

        Version,

        [EnumStringValueAttribute("oauth_callback")]

        Callback,

        [EnumStringValueAttribute("oauth_verifier")]

        Verifier,

        [EnumStringValueAttribute("oauth_token")]

        Token,

        [EnumStringValueAttribute("oauth_token_secret")]

        TokenSecret,

        [EnumStringValueAttribute("oauth_body_hash")]

        BodHash

    }


}
