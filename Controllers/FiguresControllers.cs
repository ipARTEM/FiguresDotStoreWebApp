using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FiguresDotStoreWebApp.Models;


namespace FiguresDotStoreWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FiguresControllers : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

		private readonly ILogger<FiguresControllers> _logger;
		private readonly IOrderStorage _orderStorage;

		public FiguresControllers(ILogger<FiguresControllers> logger, IOrderStorage orderStorage)
		{
			_logger = logger;
			_orderStorage = orderStorage;
		}

		// хотим оформить заказ и получить в ответе его стоимость
		[HttpPost]
		public async Task<ActionResult> Order(Cart cart)
		{
			foreach (var position in cart.Positions)
			{
				if (!FiguresStorage.CheckIfAvailable(position.Type, position.Count))
				{
					return new BadRequestResult();
				}
			}

			var order = new Order
			{
				Positions = cart.Positions.Select(p =>
				{
					Figure figure = p.Type switch
					{
						"Circle" => new Circle(),
						"Triangle" => new Triangle(),
						"Square" => new Square()
					};
					figure.SideA = p.SideA;
					figure.SideB = p.SideB;
					figure.SideC = p.SideC;
					figure.Validate();
					return figure;
				}).ToList()
			};

			foreach (var position in cart.Positions)
			{
				FiguresStorage.Reserve(position.Type, position.Count);
			}

			var result = _orderStorage.Save(order);

			return new OkObjectResult(result.Result);
		}
	}
}
