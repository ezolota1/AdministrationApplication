﻿using AdministrationAPI.Contracts.Requests;
using AdministrationAPI.Contracts.Responses;
using AdministrationAPI.Models;
using AdministrationAPI.Services.Interfaces;
using Google.Authenticator;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Data;

namespace AdministrationAPI.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public UserService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<UserDT> GetUser(string id)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) {
                throw new DataException("User with the provided id does not exist!");
            }

            return new UserDT
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                IsTwoFactorEnabled = user.TwoFactorEnabled,
                AuthenticatorKey = user.AuthenticatorKey
            };
        }

        public async Task<AuthenticationResult> Login(LoginRequest loginRequest)
        {
            User user = new User();

            if (loginRequest.Email != null)
                user = await _userManager.FindByEmailAsync(loginRequest.Email);
            else
                user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == loginRequest.Phone);

            if (user == null)
                return new AuthenticationResult
                {
                    Errors = new[] { "User not found!" }
                };



            if (!await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "Email/Phone/Password combination mismatch!" }
                };
            }

            if (user.TwoFactorEnabled && user.AuthenticatorKey != null)
                return new AuthenticationResult
                {
                    TwoFactorEnabled = true,
                    Mail = user.Email
                };

            var authClaims = await GetAuthClaimsAsync(user);

            var token = CreateToken(authClaims);

            return new AuthenticationResult
            {
                Success = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserId = user.Id
            };
        }

        public async Task<AuthenticationResult> Login2FA(Login2FARequest loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);

            if (user == null)
                return new AuthenticationResult
                {
                    Errors = new[] { "Invalid user!" }
                };

            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            string key = Encoding.UTF8.GetString(Convert.FromBase64String(user.AuthenticatorKey));
            bool result = tfa.ValidateTwoFactorPIN(key, loginRequest.Code);

            if (result)
            {
                var authClaims = await GetAuthClaimsAsync(user);
                var token = CreateToken(authClaims);

                return new AuthenticationResult
                {
                    Success = true,
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    UserId = user.Id
                };
            }

            return new AuthenticationResult
            {
                Errors = new[] { "Invalid code!" }
            };
        }

        public async Task<QRCodeResponse> GetTwoFactorQRCode(string id)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Id == id);

            if (user == null) 
                throw new DataException("User with the provided id does not exist!");
            
            string key = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            string encodedKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(key));;

            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            SetupCode setupInfo = tfa.GenerateSetupCode("Administration App", user.Email, key, false);
            string qrCodeUrl = setupInfo.QrCodeSetupImageUrl;
            string manualEntryCode = setupInfo.ManualEntryKey;
            
            user.AuthenticatorKey = encodedKey;
            await _userManager.UpdateAsync(user);

            return new QRCodeResponse
            {
                Url = qrCodeUrl,
                ManualString = manualEntryCode
            };
        }

        public async Task<bool> Toggle2FA(string id)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Id == id);

            if (user == null) 
                throw new DataException("User with the provided id does not exist!");

            if(user.TwoFactorEnabled)
                user.AuthenticatorKey = null;
        
            user.TwoFactorEnabled = !user.TwoFactorEnabled;

            var result = await _userManager.UpdateAsync(user);

            return user.TwoFactorEnabled;
        }

        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Token:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Token:ValidIssuer"],
                audience: _configuration["Token:ValidAudience"],
                expires: DateTime.Now.AddMinutes(30),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        private async Task<List<Claim>> GetAuthClaimsAsync(User user)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            return authClaims;
        }

        public TokenVerificationResult VerifyToken(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            var userNameClaim =  token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            var roleClaims = token.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

            var roleValues = roleClaims.Select(c => c.Value).ToList();

            if (userNameClaim == null)
            {
                return new TokenVerificationResult
                {
                    Errors = new[] { "User not found!" }
                };

            }
            return new TokenVerificationResult 
            { 
                Username = userNameClaim, 
                Roles = roleValues
            };
        }
    }
}
