using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Core.Entities;
using ECommerce_Backend_Task1.View_Models.User;
using Core.Interfaces;
using AutoMapper;
using ECommerce_Backend_Task1.View_Models.Login;
using MimeKit;
using MailKit.Net.Smtp;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using ECommerce_Backend_Task1.Helpers;

namespace ECommerce_Backend_Task1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/Auth/Register
        ///      {
	    ///           "Email":"osamamohammed9694@gmail.com",
        ///           "FullName":"debhwgf",
        ///           "LastName":"hiijiu",
        ///          "Password":"~987OPer"
	    ///           "FirstName":"ijijiij",
        ///           "RoleId":"caae3218-a7b2-4068-9ee4-53126776a574"
        ///        }
        /// </remarks>
        /// <response code="200">Returns the newly created item</response>
        /// <response code="400">Invalid attrbuites / RoleId not provided</response>
        /// <response code="300">Already added user with this email</response>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status300MultipleChoices)]
        public ActionResult<User> Register(CreateUser userVM)
        {

            if (userVM.RoleId == default || _unitOfWork.Role.GetById(userVM.RoleId) == null) return StatusCode(400, new { msg = "Please enter valid role" });

            if (IsAmbgious(userVM.Email))
                return StatusCode(300, new { msg = "Already added" });

            var iMapper = Mapping(new MapperConfiguration(cfg => {
                cfg.CreateMap<CreateUser, User>()
                .ForMember(destination => destination.Password,
               opts => opts.MapFrom(source => BCrypt.Net.BCrypt.HashPassword(source.Password)));
            }));

            var user = iMapper.Map<CreateUser, User>(userVM);

            _unitOfWork.User.Insert(user);
            _unitOfWork.Save();

            SendVerificationEmail(user);

            var iMapperFrontUser = Mapping(GetFrontUserMapperConfiguration());

            var frontUser = iMapperFrontUser.Map<User, FrontUser>(user);

            return Ok(frontUser);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <remarks>
        /// Sample request:
        ///
        ///     POST api/Auth/Register
        ///      {
	    ///            "Email":"osamamohammed9694@gmail.com",
        ///          "Password":"~987OPer"
	    ///
        ///        }
        /// </remarks>
        /// <response code="200">Returns the item</response>
        /// <response code="400">Invalid attrbuites / Invalid Email or Password</response>
        /// <response code="405">Email not confirmed</response>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
        public ActionResult<User> Login(Login loginVM)
        {
            User user = _unitOfWork.User.Get(u=>u.Email==loginVM.Email).FirstOrDefault();

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginVM.Password, user.Password))
            {
                return StatusCode(400, "Invalid Email or Password");
            }
            else if (user.EmailConfirmed == false)
            {
                return StatusCode(405,"Please confirm your email");
            }

            var iMapperFrontUser = Mapping(GetFrontUserMapperConfiguration());

            var frontUser = iMapperFrontUser.Map<User, FrontUser>(user);

            Response.Headers.Add("X-Auth-Token", GenerateTokenUsingJwt<FrontUser>(frontUser));

            return Ok(frontUser);
        }

        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/Auth/VerifyEmail/jwt  valid token
        /// </remarks>
        /// <response code="200">Success confirm email</response>
        /// <response code="400">Token not provided / Invalid token</response>
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult VerifyEmail(string emailToken)
        {
            if (emailToken == null)
                return BadRequest("Invalid Token");

            var identity = HelperMethods.VerifyJwtToken(emailToken);

            if (identity == null)
                return BadRequest("Invalid Token");

            var userId = identity.Claims.FirstOrDefault((c) => c.Type == "Id");

            var tokenTime = identity.Claims.FirstOrDefault((c) => c.Type == "Time");

            if (tokenTime == null || userId == null)
                return BadRequest("Invalid Token");

            TimeSpan timeDiffernce = DateTime.ParseExact(tokenTime.Value, "yyyyMMddHHmmssffff",
                                CultureInfo.InvariantCulture).Subtract(DateTime.Now);

            var timeFlag = timeDiffernce.CompareTo(new TimeSpan(0, 0, 0));

            if (timeFlag == -1)
                return BadRequest("Invalid Token");

            _unitOfWork.User.GetById(new Guid(userId.Value)).EmailConfirmed = true;

            _unitOfWork.Save();

            return Ok();

        }

        private MapperConfiguration GetFrontUserMapperConfiguration()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, FrontUser>()
                .ForMember(destination => destination.Phone,
               opts => opts.MapFrom(source => source.Phone ?? ""))
                .ForMember(destination => destination.RoleName,
               opts => opts.MapFrom(source => _unitOfWork.Role.GetById(source.RoleId).Name));
            });
        }

        private string GenerateTokenUsingJwt<K>(K data)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var now = DateTime.Now;

            List<Claim> claims = new List<Claim>();

            foreach (var prop in data.GetType().GetProperties())
            {
                claims.Add(new Claim(prop.Name, prop.GetValue(data).ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),

                Expires = now.AddDays(30),

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Convert.FromBase64String(_configuration["JWTSecert"])),
                    SecurityAlgorithms.HmacSha256Signature)
            };


            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);

            return token;
        }

        private bool IsAmbgious(string email)
        {
            return _unitOfWork.User.Get(r => r.Email == email).Count > 0;
        }

        private IMapper Mapping(MapperConfiguration mc)
        {
            var config = mc;

            return config.CreateMapper();
        }

        
        private async void SendVerificationEmail(User user)
        {
            string emailToken = GenerateTokenUsingJwt(new { Id = user.Id.ToString(), Time = DateTime.Now.AddDays(1).ToString("yyyyMMddHHmmssffff") });
            MimeMessage message = CreateEmail(emailToken, user);
            await SendEmail(message);
        }

        private MimeMessage CreateEmail(string emailToken, User user)
        {
            MimeMessage message = new MimeMessage();

            MailboxAddress from = new MailboxAddress("ECommerceTask",
            "osamamohammed9694@gmail.com");
            message.From.Add(from);

            MailboxAddress to = new MailboxAddress($"{user.FirstName} {user.LastName}",
            user.Email);
            message.To.Add(to);

            message.Subject = "Verify your email";

            string verficiationLink = $"https://localhost:44326/api/Auth/VerifyEmail?emailToken={emailToken}";

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $"<p>Dear;</p><br/><p>Thank you for register in ECommerce Task , Please confirm your email using below link so you can login and use our services.</p><br/><a href={verficiationLink}>Click here</a>";

            //bodyBuilder.Attachments.Add(env.WebRootPath + "\\file.png");

            message.Body = bodyBuilder.ToMessageBody();

            return message;
        }

        private async Task SendEmail(MimeMessage message)
        {
            SmtpClient client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 465, true);
            await client.AuthenticateAsync("osamamohammed9694@gmail.com", _configuration["PasswordOfEmail"]);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            client.Dispose(); 
        }
    }
}
