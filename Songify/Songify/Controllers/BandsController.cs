using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Songify.Data;
using Songify.Entities;
using Songify.Models;
using Songify.Models.BandModels;
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
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var band = context.Bands.Find(id);
            if (band == null)
            {
                return NotFound();
            }

            var model = new BandEditBindingModel
            {
                Id = band.Id,
                Name = band.Name,
                FormYear = band.FormYear,
                Country = band.Country
            };

            return View(model);
        }
        [Authorize]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(BandEditBindingModel model)
        {
            var band = context.Bands.Find(model.Id);
            if (band == null)
            {
                return NotFound();
            }

            band.Name = model.Name;
            band.FormYear = model.FormYear;
            band.Country = model.Country;
           
            context.Update(band);
            context.SaveChanges();

            return RedirectToAction("All");
        }
        [Authorize]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var band = context.Bands.Find(id);
            if (band == null)
            {
                return NotFound();
            }

            var model = new BandDeleteViewModel
            {
                Id = band.Id,
                Name = band.Name,
                FormYear = band.FormYear,
                Country = band.Country
            };

            return View(model);
        }
        [Authorize]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var band = context.Bands.Find(id);
            if (band == null)
            {
                return NotFound();
            }

            context.Bands.Remove(band);
            context.SaveChanges();

            return RedirectToAction("All");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
