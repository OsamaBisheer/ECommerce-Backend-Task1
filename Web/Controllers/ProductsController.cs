using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ECommerce_Backend_Task1.View_Models.Product;
using AutoMapper;
using Infrastructure.UnitOfWork;
using Core.Entities;
using Core.Interfaces;
using ECommerce_Backend_Task1.Filters;
using Microsoft.Extensions.Configuration;

namespace ECommerce_Backend_Task1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public ProductsController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        /// <remarks>
        /// Sample request:
        ///
        /// GET: api/Products
        /// </remarks>
        /// <response code="200">Returns items</response>
        // GET: api/Products
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Product>> GetProducts()
        {
            return  _unitOfWork.Product.Get();
        }

        /// <remarks>
        /// Sample request:
        ///
        ///     GET: api/Products/SearchForProducts?searchType=Color&amp;searchValue=0
        /// </remarks>
        /// <response code="200">Returns items</response>
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Product>> SearchForProducts(string searchType, string searchValue)
        {
            switch(searchType)
            {
                case "Title":
                    {
                        return _unitOfWork.Product.Get(p=>p.Title == searchValue);
                    }
                case "Color":
                    {
                        int colorNum = 0;
                        if(int.TryParse(searchValue,out colorNum)) return _unitOfWork.Product.Get(p => (int)p.Color == colorNum);
                        
                        break;
                    }
                case "Quantity":
                    {
                        int quantity = 0;
                        if (int.TryParse(searchValue, out quantity)) return _unitOfWork.Product.Get(p => p.Quantity == quantity);

                        break;
                    }
            }
            return _unitOfWork.Product.Get();
        }

        // GET: api/Products/ac0b7046-bf00-4fea-9144-1f8887344dfd
        /// <remarks>
        /// Sample request:
        ///
        ///     GET: api/Products/ac0b7046-bf00-4fea-9144-1f8887344dfd
        /// </remarks>
        /// <response code="200">Returns item</response>
        /// <response code="400">Invalid Id</response>
        /// <response code="404">Id is not provided / Id not matched any item</response> 
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Product> GetProduct(Guid id)
        {
            var product = _unitOfWork.Product.GetById(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT: api/Products
        ///     {
        ///        "Id":"565c2b7a-9c9c-4a99-a11f-f492721c5ba2",
        ///        	"Color":1,
        ///        "Title":"skmj"
        ///     }
        ///     
        /// header: x-auth-token : jwt  valid token
        ///
        /// </remarks>
        /// <response code="201">Returns the newly updated item</response>
        /// <response code="400">Invalid attrbuites</response>
        /// <response code="404">Item is null / Item id is not provided / Item id not matched any item</response> 
        /// <response code="401">Token not provided / Invalid token</response>
        /// <response code="403">Role not have permmision</response> 
        [HttpPut]
        [AuthFilter("Seller")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult PutProduct(EditProduct productVM)
        {
            var oldProduct = productVM?.Id == default ? null : _unitOfWork.Product.GetById(productVM.Id);

            if (oldProduct == null)
            {
                return NotFound();
            }

            var iMapper = Mapping(new MapperConfiguration(cfg => {
                cfg.CreateMap<EditProduct, Product>()
                .ForMember(destination => destination.Title,
               opts => opts.MapFrom(source => source.Title ?? oldProduct.Title));
            }));

            
            var product = iMapper.Map<EditProduct, Product>(productVM);

            _unitOfWork.Product.Update(product,oldProduct);

            _unitOfWork.Save();
            
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // POST: api/Products
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        /// <remarks>
        /// Sample request:
        ///
        ///     POST: api/Products
        ///      {
	    ///            	"Color":0,
        ///             "Title":"skmj",
	    ///              "Quantity":3
        ///        }
        ///        
        ///     header: x-auth-token : jwt  valid token
        /// </remarks>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">Invalid attrbuites</response>
        /// <response code="401">Token not provided / Invalid token</response>
        /// <response code="403">Role not have permmision</response> 
        [HttpPost]
        [AuthFilter("Seller")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<Product> PostProduct(CreateProduct productVM)
        {
            var iMapper = Mapping(new MapperConfiguration(cfg => {
                cfg.CreateMap<CreateProduct, Product>();
            }));

            var product = iMapper.Map<CreateProduct, Product>(productVM);

            _unitOfWork.Product.Insert(product);
            _unitOfWork.Save();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/565c2b7a-9c9c-4a99-a11f-f492721c5ba2
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE: api/Products/565c2b7a-9c9c-4a99-a11f-f492721c5ba2
        ///        
        ///    header: x-auth-token : jwt  valid token
        /// </remarks>
        /// <response code="200">Returns the softed deleted item</response>
        /// <response code="400">Id not provided in the route / Invalid Id</response>
        /// <response code="401">Token not provided / Invalid token</response>
        /// <response code="403">Role not have permmision</response> 
        /// <response code="404">Item id not matched any item</response> 
        [HttpDelete("{id}")]
        [AuthFilter("Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Product> SoftDeleteProduct(Guid id)
        {
            var oldProduct =  _unitOfWork.Product.GetById(id);
            if (oldProduct == null)
            {
                return NotFound();
            }

            var product = oldProduct;
            product.SoftDeleted = true;

            _unitOfWork.Product.Update(product, oldProduct);
            _unitOfWork.Save();

            return product;
        }

        private bool ProductExists(Guid id)
        {
            return _unitOfWork.Product.IsExists(id);
        }

        private IMapper Mapping(MapperConfiguration mc)
        {
            var config = mc;

            return config.CreateMapper();
        }
    }
}
