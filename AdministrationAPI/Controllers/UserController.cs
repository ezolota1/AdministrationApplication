﻿using AdministrationAPI.Contracts.Requests;
using AdministrationAPI.Contracts.Responses;
using AdministrationAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace AdministrationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUser([FromQuery] string id)
        {
            try
            {
                var userFetchResult = await _userService.GetUser(id);

                return Ok(userFetchResult);
            }
            catch (DataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtility.Logger.LogException(ex, "UserController.UserById");
                return StatusCode(500, ex.Message);
            }
        }

        // [HttpGet("2fa-qrcode")]
        // public Task<IActionResult> Get2FAQRCode([FromQuery] string id)
        // {
        //     try
        //     {
        //         var userFetchResult = _userService.GetTwoFactorQRCode(id);

        //         return Ok(userFetchResult);
        //     }
        //     catch (DataException ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        //     catch (Exception ex)
        //     {
        //         LoggerUtility.Logger.LogException(ex, "UserController.UserById");
        //         return StatusCode(500, ex.Message);
        //     }
        // }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var authenticationResult = await _userService.Login(loginRequest);

                if (authenticationResult.TwoFactorEnabled)
                    return Ok(authenticationResult);

                if (authenticationResult.Success)
                    return Ok(_mapper.Map<AuthenticationResult, AuthSuccessResponse>(authenticationResult));
                else
                    return BadRequest(_mapper.Map<AuthenticationResult, AuthFailResponse>(authenticationResult));
            }
            catch (DataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtility.Logger.LogException(ex, "UserController.Login");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("login2FA")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithCode([FromBody] Login2FARequest loginRequest)
        {
            try
            {                
                var authenticationResult = await _userService.Login2FA(loginRequest);

                if (authenticationResult.Success)
                    return Ok(_mapper.Map<AuthenticationResult, AuthSuccessResponse>(authenticationResult));
                else
                    return BadRequest(_mapper.Map<AuthenticationResult, AuthFailResponse>(authenticationResult));
            }
            catch (DataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                LoggerUtility.Logger.LogException(ex, "UserController.Login2FA");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
