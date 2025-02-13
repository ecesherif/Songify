using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Songify.Data;
using Songify.Entities;
using Songify.Models.AlbumModels;
using System.Security.Claims;

namespace Songify.Controllers
{
    public class AlbumsController : Controller
    {
        private readonly ApplicationDbContext context;

        public AlbumsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult All(string searchString)
        {
            List<AlbumAllViewModel> albums = context.Albums
                .Select(albumFromDb => new AlbumAllViewModel
                {
                    Id = albumFromDb.Id.ToString(),
                    Title = albumFromDb.Title,
                    ReleaseYear = albumFromDb.ReleaseYear.ToString()
                })
                .ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                albums = albums.Where(s => s.Title.Contains(searchString)).ToList();
            }

            return this.View(albums);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(AlbumCreateBindingModel bindingModel)
        {
            if (this.ModelState.IsValid)
            {
                string currentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                Album albumFromDb = new Album
                {
                    Title = bindingModel.Title,
                    ReleaseYear = bindingModel.ReleaseYear
                };

                context.Albums.Add(albumFromDb);
                context.SaveChanges();

                return this.RedirectToAction("All");
            }

            return this.View();
        }
        [Authorize]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var album = context.Albums.Find(id);
            if (album == null)
            {
                return NotFound();
            }

            var model = new AlbumEditBindingModel
            {
                Id = album.Id,
                Title = album.Title,
                ReleaseYear = album.ReleaseYear
            };

            return View(model);
        }
        [Authorize]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(AlbumEditBindingModel model)
        {
            var album = context.Albums.Find(model.Id);
            if (album == null)
            {
                return NotFound();
            }

            album.Title = model.Title;
            album.ReleaseYear = model.ReleaseYear;

            context.Update(album);
            context.SaveChanges();

            return RedirectToAction("All");
        }
        [Authorize]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var album = context.Albums.Find(id);
            if (album == null)
            {
                return NotFound();
            }

            var model = new AlbumDeleteViewModel
            {
                Id = album.Id,
                Title = album.Title,
                ReleaseYear = album.ReleaseYear
            };

            return View(model);
        }
        [Authorize]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var album = context.Albums.Find(id);
            if (album == null)
            {
                return NotFound();
            }

            context.Albums.Remove(album);
            context.SaveChanges();

            return RedirectToAction("All");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
