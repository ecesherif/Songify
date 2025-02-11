using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Songify.Data;
using Songify.Entities;
using Songify.Models;
using Songify.Models.BandModels;
using Songify.Models.SongModels;
using System.Security.Claims;

namespace Songify.Controllers
{
    public class BandsController : Controller
    {
        private readonly ApplicationDbContext context;

        public BandsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult All(string searchString)
        {
            List<BandAllViewModel> bands = context.Bands
                .Select(bandFromDb => new BandAllViewModel
                {
                    Id = bandFromDb.Id.ToString(),
                    Name = bandFromDb.Name,
                    FormYear = bandFromDb.FormYear.ToString(),
                    Country = bandFromDb.Country
                })
                .ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                bands = bands.Where(s => s.Name.Contains(searchString)).ToList();
            }

            return this.View(bands);
        }

        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]

        public IActionResult Create(BandCreateBindingModel bindingModel)
        {
            if (this.ModelState.IsValid)
            {
                string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                Band bandFromDb = new Band
                {
                    Name = bindingModel.Name,
                    FormYear = bindingModel.FormYear,
                    Country = bindingModel.Country
                };

                context.Bands.Add(bandFromDb);
                context.SaveChanges();

                return this.RedirectToAction("All");
            }

            return this.View();
        }
        [Authorize]
        
        public IActionResult Index()
        {
            return View();
        }
    }
}
