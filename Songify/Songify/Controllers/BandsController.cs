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
        // Constructor to initialize the context
        public BandsController(ApplicationDbContext context)
        {
            this.context = context;
        }
        // Action to display all bands with optional search functionality
        public IActionResult All(string searchString)
        {
            ViewData["Controller"] = "Bands";
            ViewData["Action"] = "All";
            List<BandAllViewModel> bands = context.Bands
                .Select(bandFromDb => new BandAllViewModel
                {
                    Id = bandFromDb.Id.ToString(),
                    Name = bandFromDb.Name,
                    FormYear = bandFromDb.FormYear.ToString(),
                    Country = bandFromDb.Country
                })
                .ToList();
            if (!string.IsNullOrEmpty(searchString))
            {
                bands = bands.Where(b => b.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return this.View(bands);
        }
        // Action to display the band creation form (only accessible by Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return this.View();
        }
        // POST action to handle the creation of a new band (only accessible by Admin)
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
        // Action to display the edit form for an existing band (only accessible by Admin)
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
        // POST action to handle the editing of a band (only accessible by Admin)
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
        // Action to display the deletion confirmation for a band (only accessible by Admin)
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
        // POST action to handle the actual deletion of a band (only accessible by Admin)
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
        // Action to return the index view (default page for the Bands controller)
        public IActionResult Index()
        {
            return View();
        }
    }
}
