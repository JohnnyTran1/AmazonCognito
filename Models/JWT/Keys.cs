using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace AmazonCognito.Models.JWT
{
    public class Keys
    {
        public string alg { get; set; }
        public string e { get; set; }

        public string kid { get; set; }

        public string kty { get; set; }

        public string n { get; set; }
        public string use { get; set; }



        public static List<Keys> DecodePublicKeys(string json)
        {
            List<Keys> list = new List<Keys>();
            dynamic j = JsonConvert.DeserializeObject<dynamic>(json);

            var keys = j["keys"];

            foreach (var endpoint in keys)
            {
                Keys key = new Keys
                {
                    alg = endpoint["alg"],
                    e = endpoint["e"],
                    kid = endpoint["kid"],
                    kty = endpoint["kty"],
                    n = endpoint["n"],
                    use = endpoint["use"]
                };

                list.Add(key);
            }
            return list;
        }

        public static object GetAccessTokenHeaderValue(string token, string key)
        {
            try
            {
                object value = string.Empty;
                var handler = new JwtSecurityTokenHandler();
                var read = handler.ReadJwtToken(token);

                read.Header.TryGetValue(key, out value);

                return value;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}