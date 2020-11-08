using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using ECommerce_Backend_Task1.Filters;
using ECommerce_Backend_Task1.View_Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ECommerce_Backend_Task1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        IUnitOfWork _unitOfWork;

        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Users
        /// <remarks>
        /// Sample request:
        ///
        ///     GET: api/Users
        /// </remarks>
        /// <response code="200">Returns items</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<FrontUser>> GetUsers()
        {
            List<FrontUser> frontUsers = new List<FrontUser>();

            var iMapperFrontUser = Mapping(GetFrontUserMapperConfiguration());

            foreach(var user in _unitOfWork.User.GetAll())
            {
                frontUsers.Add(iMapperFrontUser.Map<User, FrontUser>(user));
            }

            return frontUsers;
        }

        // GET: api/Users/5
        /// <remarks>
        /// Sample request:
        ///
        ///     GET: api/Users/565c2b7a-9c9c-4a99-a11f-f492721c5ba2
        /// </remarks>
        /// <response code="200">Returns item</response>
        /// <response code="400">Invalid Id</response>
        /// <response code="404">Id is not provided / Id not matched any item</response> 
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<FrontUser> GetUser(Guid id)
        {
            var user = _unitOfWork.User.GetById(id);

            if (user == null)
            {
                return NotFound();
            }

            var iMapperFrontUser = Mapping(GetFrontUserMapperConfiguration());

            var frontUser = iMapperFrontUser.Map<User, FrontUser>(user);

            return frontUser;
        }

        /// <remarks>
        /// Sample request:
        ///
        ///     PUT: api/Users
        ///      {
	    ///            "id": "57789be1-60e5-469d-9919-073b34fb2ede",
        ///          "roleId":"e4ca18e4-bcb8-4dd1-ba30-9cf3c6c279aa"
	    ///
        ///        }
        ///        
        /// header: x-auth-token : jwt  valid token
        /// 
        /// </remarks>
        /// <response code="200">Returns the newly updated item</response>
        /// <response code="400">Invalid attrbuites</response>
        /// <response code="404">Item is null / Item id is not provided / Item id not matched any item</response> 
        /// <response code="401">Token not provided / Invalid token</response>
        /// <response code="403">Role not have permmision</response>
        // PUT: api/Users
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut]
        [AuthFilter("",true)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<FrontUser> PutUser(EditUser userVM)
        {
            var oldUser = userVM.Id == default ? null : _unitOfWork.User.GetById(userVM.Id);

            if (oldUser == null)
            {
                return NotFound();
            }

            if (userVM.RoleId != default && _unitOfWork.Role.GetById(userVM.RoleId) == null) return StatusCode(400, new { msg = "Please enter valid role" });

            var iMapper = Mapping(new MapperConfiguration(cfg => {
                cfg.CreateMap<EditUser, User>()
                .ForMember(destination => destination.FirstName,
               opts => opts.MapFrom(source => source.FirstName ?? oldUser.FirstName))
                .ForMember(destination => destination.LastName,
               opts => opts.MapFrom(source => source.LastName ?? oldUser.LastName))
                .ForMember(destination => destination.Password,
               opts => opts.MapFrom(source => source.Password != null? BCrypt.Net.BCrypt.HashPassword(source.Password) : oldUser.Password))
                .ForMember(destination => destination.FullName,
               opts => opts.MapFrom(source => source.FullName ?? oldUser.FullName))
                .ForMember(destination => destination.RoleId,
               opts => opts.MapFrom(source => source.RoleId == default? oldUser.RoleId : source.RoleId))
                .ForMember(destination => destination.Email,
               opts => opts.MapFrom(source => oldUser.Email))
                .ForMember(destination => destination.EmailConfirmed,
               opts => opts.MapFrom(source => oldUser.EmailConfirmed));
            }));


            var user = iMapper.Map<EditUser, User>(userVM);

            _unitOfWork.User.Update(user, oldUser);
            _unitOfWork.Save();

            var iMapperFrontUser = Mapping(GetFrontUserMapperConfiguration());

            var frontUser = iMapperFrontUser.Map<User, FrontUser>(user);

            return frontUser;
        }


        // DELETE: api/Users/01e691c7-bd1b-43ba-b39b-7eb0f1febddb
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE: api/Users/01e691c7-bd1b-43ba-b39b-7eb0f1febddb     
        ///   header: x-auth-token : jwt  valid token
        ///
        /// </remarks>
        /// <response code="200">Returns the softed deleted item</response>
        /// <response code="400">Id not provided in the route / Invalid Id</response>
        /// <response code="401">Token not provided / Invalid token</response>
        /// <response code="403">Role not have permmision</response> 
        /// <response code="404">Item id not matched any item</response> 
        [HttpDelete("{id}")]
        [AuthFilter("", true, "query")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<FrontUser> DeleteUser(Guid id)
        {
            var user = _unitOfWork.User.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            _unitOfWork.User.Delete(user);
            _unitOfWork.Save();

            var iMapperFrontUser = Mapping(GetFrontUserMapperConfiguration());

            var frontUser = iMapperFrontUser.Map<User, FrontUser>(user);

            return frontUser;
        }

        private bool UserExists(Guid id)
        {
            return _unitOfWork.User.IsExists(id);
        }

        private IMapper Mapping(MapperConfiguration mc)
        {
            var config = mc;

            return config.CreateMapper();
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
    }
}
