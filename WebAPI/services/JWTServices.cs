using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

using webapi_csharp.Models;

namespace webapi_csharp.services
{
    public class JWTServices
    {
        private string GenerateToken(Claim[] claims, string privateKey, TimeSpan expirationTime)
        {
            var key = new SymmetricSecurityKey(Convert.FromBase64String(privateKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.Add(expirationTime),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<(string accessToken, string refreshToken)> generateTokens(User user)
        {
            try
            {
                var payload = new Claim[] {
                    new Claim("_id", user.Id),
                    new Claim("roles", user.Role)
                };

                var accessToken = GenerateToken(
                    payload,
                    "AUTH.JWT.ACCESS_TOKEN.PRIVATE_KEY",
                    TimeSpan.FromMinutes(15)
                ); 

                var refreshToken = GenerateToken(
                    payload,
                    "AUTH.JWT.REFRESH_TOKEN_PRIVATE_KEY",
                    TimeSpan.FromDays(7)
                );

                return await Task.FromResult((accessToken, refreshToken));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool isValid, string message)> verifyRefreshToken(string refreshToken)
        {
            try
            {
                var privateKey = "AUTH.JWT.REFRESH_TOKEN.PRIVATE_KEY"; 
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(privateKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var handler = new JwtSecurityTokenHandler();
                SecurityToken token;

                var principal = handler.ValidateToken(refreshToken, tokenValidationParameters, out token);

                return await Task.FromResult((true, "Valid refresh token"));
            }
            catch (SecurityTokenException)
            {
                return await Task.FromResult((false, "Invalid refresh token"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}