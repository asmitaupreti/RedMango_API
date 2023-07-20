using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.DTO;
using AutoMapper;

namespace RedMango_API.Controllers
{
    [Route("api/MenuItem")]
    [ApiController]
	public class MenuItemController:ControllerBase
	{
		private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
		protected ApiResponse _response;
		public MenuItemController(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
            _mapper = mapper;
			this._response = new();
		}


		[HttpGet]
		public async Task<ActionResult<ApiResponse>> GetMenuItems()
		{
			try
			{
				_response.result = _db.MenuItems;
				_response.StatusCode = HttpStatusCode.OK;
				_response.IsSuccess = true;
				
			}
			catch (Exception ex)
			{
               
                _response.StatusCode = HttpStatusCode.BadRequest;
				_response.ErrorMessages = new() { ex.ToString()};
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
			return Ok(_response);
		}

        [HttpGet("{id:int}", Name ="GetMenuItem")]
        public async Task<ActionResult<ApiResponse>> GetMenuItem(int id)
        {
            try
            {
                if(id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new() { "Id cannot be 0" };
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                MenuItem menuItem =  _db.MenuItems.Where(u => u.Id == id).FirstOrDefault();
                if (menuItem==null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new() { "Cannot find the item" };
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                _response.result = menuItem;
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
            }
            catch (Exception ex)
            {

                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages = new() { ex.ToString() };
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            return Ok(_response);

        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromBody]MenuItemCreateDTO menuItemCreateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    
                        MenuItem menuItem = _mapper.Map<MenuItem>(menuItemCreateDTO);
                        _db.MenuItems.Add(menuItem);
                         await _db.SaveChangesAsync();

                         _response.result = menuItem;
                         _response.StatusCode = HttpStatusCode.Created;
                         _response.IsSuccess = true;
                    return CreatedAtRoute("GetMenuItem", new { id = menuItem.Id }, _response);

                }
                else
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
               
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages = new() { ex.ToString() };
                _response.IsSuccess = false;
                return BadRequest(_response);

            }
           

        }



        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int id,[FromBody] MenuItemUpdateDTO menuItemUpdateDTO)
        {
            try
            {

               
                if (ModelState.IsValid)
                {
                    if (menuItemUpdateDTO == null || id != menuItemUpdateDTO.Id)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        return BadRequest(_response);
                    }

                    MenuItem menuItemId = await _db.MenuItems.FindAsync(id);
                    if (menuItemId == null)
                    {
                        _response.IsSuccess = false;
                        _response.ErrorMessages = new() { "Cannot find the item to delete" };
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        return BadRequest(_response);
                    }

                    MenuItem menuItem = _mapper.Map<MenuItem>(menuItemUpdateDTO);
                    _db.MenuItems.Update(menuItem);
                    await _db.SaveChangesAsync();

                    _response.result = menuItem;
                    _response.StatusCode = HttpStatusCode.Created;
                    _response.IsSuccess = true;
                    return Ok(_response);

                }
                else
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages = new() { ex.ToString() };
                _response.IsSuccess = false;
                return BadRequest(_response);

            }
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
        {
            if (id == 0 )
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new() { "Id cannot be 0" };
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            MenuItem menuItem = await _db.MenuItems.FindAsync(id);
            if(menuItem == null)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new() { "Cannot find the item to delete" };
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            _db.MenuItems.Remove(menuItem);
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }

    }
}

