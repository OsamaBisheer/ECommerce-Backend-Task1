using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using ECommerce_Backend_Task1.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_Backend_Task1.Controllers
{
    /// <response code="401">Token not provided / Invalid token</response>
    /// <response code="403">Role not have permmision</response> 
    [Route("api/[controller]")]
    [ApiController]
    [AuthFilter("Master")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class RolesController : ControllerBase
    {
        IUnitOfWork _unitOfWork;

        public RolesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Roles
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/Roles
        ///     
        /// header: x-auth-token : jwt  valid token
        /// </remarks>
        /// <response code="200">Returns item</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Role>> GetRoles()
        {
            return _unitOfWork.Role.GetAll();
        }

        // GET: api/Roles/01e691c7-bd1b-43ba-b39b-7eb0f1febddb
        /// <remarks>
        /// Sample request:
        ///
        /// GET api/Roles/01e691c7-bd1b-43ba-b39b-7eb0f1febddb     
        ///     
        /// header: x-auth-token : jwt  valid token
        /// </remarks>
        /// <response code="200">Returns item</response>
        /// <response code="400">Invalid Id</response>
        /// <response code="404">Id is not provided / Id not matched any item</response> 
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Role> GetRole(Guid id)
        {
            var role = _unitOfWork.Role.GetById(id);

            if (role == null)
            {
                return NotFound();
            }

            return role;
        }

        /// <remarks>
        /// Sample request:
        ///
        ///     PUT: api/Roles
        ///      {
	    ///            	"id":"e4ca18e4-bcb8-4dd1-ba30-9cf3c6c279aa",
        ///          "Name":"Admin"
	    ///
        ///        }
        ///        
        /// header: x-auth-token : jwt  valid token
        /// </remarks>
        /// <response code="201">Returns the newly updated item</response>
        /// <response code="400">Invalid attrbuites</response>
        /// <response code="404">Item is null / Item id is not provided / Item id not matched any item</response> 
        /// <response code="300">Already added role with this name</response> 
        // PUT: api/Roles
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status300MultipleChoices)]
        public IActionResult PutRole(Role role)
        {
            var oldRole = role?.Id == default ? null : _unitOfWork.Role.GetById(role.Id);

            if (oldRole == null)
            {
                return NotFound();
            }

            if (IsAmbgious(role))
                return StatusCode(300, new { msg = "Already added" });

            _unitOfWork.Role.Update(role, oldRole);
            _unitOfWork.Save();

            return CreatedAtAction("GetRole", new { id = role.Id }, role);
        }

        /// <remarks>
        /// Sample request:
        ///
        ///     POST: api/Roles
        ///      {
	    ///           "Name":"Admin"
	    ///
        ///        }
        ///        
        /// header: x-auth-token : jwt  valid token
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">Invalid attrbuites</response>
        /// <response code="300">Already added role with this name</response> 
        // POST: api/Roles
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status300MultipleChoices)]
        public ActionResult<Role> PostRole(Role role)
        {
            if (IsAmbgious(role))
                return StatusCode(300, new { msg = "Already added" });

            _unitOfWork.Role.Insert(role);
            _unitOfWork.Save();

            return CreatedAtAction("GetRole", new { id = role.Id }, role);
        }

        // DELETE: api/Roles/e4ca18e4-bcb8-4dd1-ba30-9cf3c6c279aa
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE: api/Roles/e4ca18e4-bcb8-4dd1-ba30-9cf3c6c279aa
        ///     
        /// header: x-auth-token : jwt  valid token
        /// </remarks>
        /// <response code="200">Returns deleted item</response>
        /// <response code="400">Id not provided in the route / Invalid Id</response>
        /// <response code="404">Item id not matched any item</response> 
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Role> DeleteRole(Guid id)
        {
            var role = _unitOfWork.Role.GetById(id);
            if (role == null)
            {
                return NotFound();
            }

            _unitOfWork.Role.Delete(role);
            _unitOfWork.Save();

            return role;
        }

        private bool IsAmbgious(Role role)
        {
            return _unitOfWork.Role.Get(r => r.Name == role.Name).Count > 0;
        }

        private bool RoleExists(Guid id)
        {
            return _unitOfWork.Role.IsExists(id);
        }
    }
}