using AmazonCognito.Models.Authenticate;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AmazonCognito.Models
{
    public static class Validation
    {
        //Validates for a valid phone number 
        public static bool ValidatePhoneNumber(string PhoneNumber)
        {
            Regex regexPhoneNumber = new Regex(@"^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$");
            return (regexPhoneNumber.IsMatch(PhoneNumber));
        }

        //Check for a box to have a value
        public static bool IsBlank(string FormString)
        {
            return (string.IsNullOrEmpty(FormString) || string.IsNullOrWhiteSpace(FormString));
        }

        //Validate a 5 digit zipcode
        public static bool ValidateZipCode(string ZipCode)
        {
            Regex regexZipCode = new Regex(@"^(\d{5}-\d{4}|\d{5}|\d{9})$|^([a-zA-Z]\d[a-zA-Z] \d[a-zA-Z]\d)$");
            return (regexZipCode.IsMatch(ZipCode));
        }

        //Validate for a non-negative number
        public static bool ValidateNonNegativeNumber(string nonNegativeNumber)
        {
            Regex NonNegativeNumber = new Regex(@"^\d+$");
            return (NonNegativeNumber.IsMatch(nonNegativeNumber));
        }

        //validate for currency
        public static bool ValidateCurrency(string validCurrency)
        {
            Regex ValidCurrency = new Regex(@"^\d*\.?\d*$");
            return (ValidCurrency.IsMatch(validCurrency));
        }

        //Validate date
        public static bool ValidateDate(string date)
        {
            DateTime res;
            return (DateTime.TryParse(date, out res));
        }

        // Validate that it is a potentially valid email address
        public static bool ValidateAnyEmail(string Email)
        {
            Regex regexEmail = new Regex("^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$");
            return regexEmail.IsMatch(Email);
        }

        //Validate a 9 digit TUid
        public static bool ValidateTUID(string TUID)
        {
            Regex regexTUID = new Regex(@"^\d{9}$");
            return (regexTUID.IsMatch(TUID));
        }

        //Validate for a 4 digit academic year
        public static bool ValidateAcademicYear(string academicYear)
        {
            Regex regex = new Regex(@"^\d{4}[-]\d{4}$");
            return (regex.IsMatch(academicYear));
        }

        //Validate length of string in a text box
        public static bool ValidateLength(string tbox, int length)
        {
            return tbox.Length == length;
        }

        //Validate word count in a text field
        public static bool ValidateWordCount(string tbox, int count)
        {
            return tbox.Length <= (count * 10);
        }

        public static bool ValidateNumberValue(string num, double maxNum)
        {
            if (!string.IsNullOrEmpty(num))
            {
                return long.Parse(num) <= maxNum;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if JWT token is expired or not
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <returns>Boolean</returns>
        public static bool IsTokenExpired(string token)
        {
            JwtSecurityToken jwtSecurityToken;
            try
            {
                jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            }
            catch (Exception)
            {
                return false;
            }

            return jwtSecurityToken.ValidTo > DateTime.UtcNow;
        }

        /// <summary>
        /// Check if JWT token is expired or not
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <returns>Boolean</returns>
        public static bool IsTokenExpired(Token token)
        {
            if(token == null)
            {
                return false;
            }


            JwtSecurityToken jwtSecurityToken;
            try
            {
                jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(token.Access_Token);
            }
            catch (Exception)
            {
                return false;
            }

            return jwtSecurityToken.ValidTo > DateTime.UtcNow;
        }

        /// <summary>
        /// Validate user's AWS access token's signature with AWS user pools public key 
        /// </summary>
        /// <param name="userAccessToken"></param>
        /// <param name="keys">Collection of Keys</param>
        /// <returns>Boolean</returns>
        public static bool ValidateSignatureOfAccessToken(string userAccessToken, ICollection<JWT.Keys> keys)
        {
            try
            {
                var userAccessTokenKid = JWT.Keys.GetAccessTokenHeaderValue(userAccessToken, "kid");
                JWT.Keys awsPublicKey = keys.Where(x => x.kid.Equals(userAccessTokenKid)).FirstOrDefault();

                if (awsPublicKey != null)
                {
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.ImportParameters(
                      new RSAParameters()
                      {
                          Modulus = Base64UrlDecode(awsPublicKey.n),
                          Exponent = Base64UrlDecode(awsPublicKey.e)
                      });

                    var validationParameters = new TokenValidationParameters
                    {
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = new RsaSecurityKey(rsa)
                    };

                    SecurityToken validatedSecurityToken = null;
                    var handler = new JwtSecurityTokenHandler();
                    handler.ValidateToken(userAccessToken, validationParameters, out validatedSecurityToken);
                    JwtSecurityToken validatedJwt = validatedSecurityToken as JwtSecurityToken;

                    return true;
                }
                else
                    return false;
            }
            catch(Exception)
            {
                return false;
            }
        }


        // from JWT spec
        private static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 1: output += "==="; break; // Three pad chars
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }

           
    }
}