using System;
using AutoMapper;
using RedMango_API.DTO;
using RedMango_API.Models;

namespace RedMango_API
{
	public class MappingConfig:Profile
	{
		public MappingConfig()
		{
			CreateMap<MenuItem, MenuItemCreateDTO>().ReverseMap();
            CreateMap<MenuItem, MenuItemUpdateDTO>().ReverseMap();
        }
	}
}

