using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerce_Backend_Task1.Helpers
{
    public static class HelperMethods
    {

        public static ClaimsIdentity VerifyJwtToken(string token)
        {
            try
            {

                if (string.IsNullOrEmpty(token))
                    return null;

                var tokenHandler1 = new JwtSecurityTokenHandler();



                var jwtToken = tokenHandler1.ReadToken(token) as JwtSecurityToken;

                var validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Startup.StaticConfig["JWTSecert"]))
                };

                SecurityToken securityToken;
                var principal = tokenHandler1.ValidateToken(token, validationParameters, out securityToken);

                var identity = principal.Identity as ClaimsIdentity;

                if (identity == null)
                    return null;

                if (!identity.IsAuthenticated)
                    return null;

                return identity;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
