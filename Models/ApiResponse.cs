using System;
using System.Net;

namespace RedMango_API.Models
{
	public class ApiResponse
	{
		public HttpStatusCode StatusCode { get; set; }
		public bool IsSuccess { get; set; }
		public List<string> ErrorMessages { get; set; }

		public object result { get; set; }
	}
}

