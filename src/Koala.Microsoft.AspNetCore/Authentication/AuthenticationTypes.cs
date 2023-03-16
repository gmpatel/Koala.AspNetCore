using Newtonsoft.Json.DataExtensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Authentication
{
    public enum AuthTypes
    {
        Google
    }
    
    public class AuthToken
    {
        public AuthTypes? AuthenticationType { get; set; }

        public string Email { get; set; }

        public bool? EmailVerified { get; set; }

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PictureLink { get; set; }

        public IList<string> Claims { get; set; }

        public IList<string> AppClaims { get; set; }

        public IList<string> ApiClaims { get; set; }

        [JsonIgnore]
        public JObject UnderlyingObject { get; set; }
    }
    
    public static class AuthExtensions
    {
        public static string GetAuthTokenEncrypted(this string oAuthJwtTokenBase64, IList<string> claims = default, IList<string> appClaims = default, IList<string> apiClaims = default)
        {
            return oAuthJwtTokenBase64.GetAuthToken(claims, appClaims, apiClaims)
                .Json(compact: true)
                .Encrypt()
                .Base64Encode();
        }

        public static string GetAuthTokenBase64(this string oAuthJwtTokenBase64, IList<string> claims = default, IList<string> appClaims = default, IList<string> apiClaims = default)
        {
            return oAuthJwtTokenBase64.GetAuthToken(claims, appClaims, apiClaims)
                .Json(compact: true)
                .Base64Encode();
        }

        public static AuthToken GetAuthToken(this string oAuthJwtTokenBase64, IList<string> claims = default, IList<string> appClaims = default, IList<string> apiClaims = default)
        {
            var oAuthJwtToken = oAuthJwtTokenBase64
                .Base64Decode()
                .Get<JObject>();

            var authenticationType = oAuthJwtToken.GetAuthenticationType();

            switch (authenticationType)
            {
                case AuthTypes.Google:
                    {
                        return oAuthJwtToken.GetAuthenticationTokenForGoogleAuthenticationType(authenticationType);
                    }
            }

            return oAuthJwtToken.GetAuthenticationTokenForUnknownAuthenticationType(authenticationType);
        }

        private static AuthTypes? GetAuthenticationType(this JObject data)
        {
            if (data.TryGetValue("iss", StringComparison.InvariantCultureIgnoreCase, out var issValue) && issValue.Contains("google"))
            {
                return AuthTypes.Google;
            }

            return default;
        }

        private static AuthToken GetAuthenticationTokenForGoogleAuthenticationType(this JObject data, AuthTypes? authenticationType, IList<string> claims = default, IList<string> appClaims = default, IList<string> apiClaims = default)
        {
            return new AuthToken
            {
                AuthenticationType = authenticationType,
                Email = data.GetProperty<string>("email", string.Empty).Trim().ToLower(),
                EmailVerified = data.GetProperty<bool?>("email_verified"),
                FirstName = data.GetProperty<string>("given_name"),
                LastName = data.GetProperty<string>("family_name"),
                Name = data.GetProperty<string>("name"),
                PictureLink = data.GetProperty<string>("picture"),
                Claims = claims,
                AppClaims = appClaims,
                ApiClaims = apiClaims,
                UnderlyingObject = data
            };
        }

        private static AuthToken GetAuthenticationTokenForUnknownAuthenticationType(this JObject data, AuthTypes? authenticationType, IList<string> claims = default, IList<string> appClaims = default, IList<string> apiClaims = default)
        {
            if (string.IsNullOrWhiteSpace(data.GetProperty<string>("email")))
            {
                throw new InvalidDataException(@"Invalid authentication data provided; unable to retrieve email address of the user signed in");
            }

            return new AuthToken
            {
                AuthenticationType = authenticationType,
                Email = data.GetProperty<string>("email"),
                EmailVerified = data.GetProperty<bool?>("email_verified"),
                FirstName = data.GetProperty<string>("given_name"),
                LastName = data.GetProperty<string>("family_name"),
                Name = data.GetProperty<string>("name"),
                PictureLink = data.GetProperty<string>("picture"),
                Claims = claims,
                AppClaims = appClaims,
                ApiClaims = apiClaims,
                UnderlyingObject = data
            };
        }

        private static T GetProperty<T>(this JObject metadata, string propertyName, T defaultValue = default(T))
        {
            var t = typeof(T);

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                t = t.GetGenericArguments()[0];

            return !metadata.ContainsKey(propertyName)
                ? default(T)
                : (T)Convert.ChangeType(metadata[propertyName], t);
        }
    }
}