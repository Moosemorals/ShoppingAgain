using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShoppingAgain.Models;
using ShoppingAgain.Services;

namespace ShoppingAgain.Controllers
{
    [Route("api")]
    [ApiController]
    public class APIController : ControllerBase
    {

        private ShoppingService _db;

        public APIController(ShoppingService db)
        {
            _db = db;
        }

        [Route(""), HttpPost]
        [Produces(typeof(ShoppingList))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<ShoppingList> Create(string name)
        {
            if (_db.ExistsByName(name))
            {
                return BadRequest("That shopping list already exists");
            }

            ShoppingList list = new ShoppingList
            {
                Name = name,
            };

            list = _db.Add(list);

            return CreatedAtAction("GetById", new { id = list.ID });
        }

        [Route("{id:long}", Name = "GetById")]
        [Produces(typeof(ShoppingList))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ShoppingList> GetById(long id)
        {
            ShoppingList list = _db.Get(id);
            if (list != null)
            {
                return list;
            }
            return NotFound();
        }

        [Route("{name}", Name = "GetByName")]
        [Produces(typeof(ShoppingList))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Get(string name)
        {
            ShoppingList list = _db.Get(name);

            if (list != null)
            {
                return Ok(list);
            }
            return NotFound();
        }

    }
}