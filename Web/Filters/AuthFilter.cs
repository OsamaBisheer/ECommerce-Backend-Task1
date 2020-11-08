using ECommerce_Backend_Task1.Helpers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Entities;
using Newtonsoft.Json;

namespace ECommerce_Backend_Task1.Filters
{
    //[AttributeUsage(AttributeTargets.Method)]
    public class AuthFilter : Attribute, IAuthorizationFilter
    {
        readonly string _role;
        readonly bool _checkUserId;
        readonly string _paramPosition;
        Guid paramId;

        public AuthFilter()
        {
            _role = "";
        }

        public AuthFilter(string role , bool checkUserId = false , string paramPosition = "body")
        {
            _role = role;

            _checkUserId = checkUserId;

            _paramPosition = paramPosition;
        }

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null) 
            { 
                HandleNullableContext(filterContext);
                return;
            }

            if (_checkUserId &&_paramPosition == "body")
            {
                GetBody(filterContext);
            }
            else if (_checkUserId && _paramPosition == "query")
            {
                GetQuery(filterContext);
            }

            var authToken = GetAuthTokenHeader(filterContext);

            if(string.IsNullOrEmpty(authToken))
            {
                SetResultUnauthorized(filterContext);
                return;
            }

            var claimsIdentity = HelperMethods.VerifyJwtToken(authToken);

            if (authToken == null || claimsIdentity == null  )
            {
                SetResultUnauthorized(filterContext);
            }
            else if(!string.IsNullOrEmpty(_role) && claimsIdentity.Claims.FirstOrDefault(c => c.Type == "RoleName").Value != _role
                    || (_checkUserId && claimsIdentity.Claims.FirstOrDefault(c => c.Type == "Id").Value != paramId.ToString()))
            {
                SetResultForbbiden(filterContext);
            }
        }

        private void HandleNullableContext(AuthorizationFilterContext filterContext)
        {
            filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
            filterContext.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Please Provide authToken";
            filterContext.Result = new JsonResult("Please Provide authToken")
            {
                Value = new
                {
                    Status = "Error",
                    Message = "Please Provide authToken"
                },
            };
        }

        private async void GetBody(AuthorizationFilterContext filterContext)
        {
            var req = filterContext.HttpContext.Request;
            req.EnableBuffering();
            var stream = new StreamReader(filterContext.HttpContext.Request.Body);
            var bodyObj = await stream.ReadToEndAsync();
            paramId = JsonConvert.DeserializeObject<Base>(bodyObj).Id;
            req.Body.Position = 0;
        }

        private void GetQuery(AuthorizationFilterContext filterContext)
        {
            var req = filterContext.HttpContext.Request;
            var splits = req.Path.Value.Split("/");
            paramId = new Guid(splits[splits.Length - 1]);
        }

        private string GetAuthTokenHeader(AuthorizationFilterContext filterContext)
        {
            Microsoft.Extensions.Primitives.StringValues authTokens;
            filterContext.HttpContext.Request.Headers.TryGetValue("x-auth-token", out authTokens);

            var _token = authTokens.FirstOrDefault();

            return _token;
        }

        private void SetResultUnauthorized(AuthorizationFilterContext filterContext)
        {
            filterContext.HttpContext.Response.StatusCode = 401;
            filterContext.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "NotAuthorized";
            filterContext.Result = new JsonResult("NotAuthorized")
            {
                Value = new
                {
                    Status = "Error",
                    Message = "Invalid Token"
                },
            };
        }

        private void SetResultForbbiden(AuthorizationFilterContext filterContext)
        {
            filterContext.HttpContext.Response.StatusCode = 403;
            filterContext.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Forbbiden";
            filterContext.Result = new JsonResult("Forbbiden")
            {
                Value = new
                {
                    Status = "Error",
                    Message = "Invalid Token"
                },
            };
        }
    }
}



