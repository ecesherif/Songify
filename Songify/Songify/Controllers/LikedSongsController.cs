using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Songify.Data;
using Songify.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Songify.Models.AlbumModels;
using Songify.Models.LikedSongsModels;

namespace Songify.Controllers
{
    public class LikedSongsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<SongifyUser> userManager;
        public LikedSongsController(ApplicationDbContext context, UserManager<SongifyUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
        public ActionResult Index()
        {
            var userId = userManager.GetUserId(User);
            var likedSongs = context.LikedSongs
                .AsNoTracking()
                .Include(l => l.Song)
                .Where(l => l.UserId == userId)
                .ToList();

            return View(likedSongs);
        }
        public IActionResult All(string searchString)
        {
            List<LikedSongsAllViewModel> likedsongs = context.LikedSongs
                .Select(lsFromDb => new LikedSongsAllViewModel
                {
                    Id = lsFromDb.Id.ToString(),
                    SongId = lsFromDb.SongId.ToString(),
                    SongTitle = lsFromDb.Song.Title

                })
                .ToList();
            if (!String.IsNullOrEmpty(searchString))
            {
                likedsongs = likedsongs.Where(s => s.UserId.Contains(searchString)).ToList();
            }

            return this.View(likedsongs);
        }
        public ActionResult Add()
        {
            ViewBag.SongId = new SelectList(context.Songs, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(LikedSongAddBindingModel model)
        {
            if (ModelState.IsValid)
            {
                var likedSong = new LikedSong
                {
                    UserId = userManager.GetUserId(User), // Convert UserId to string if needed
                    SongId = model.SongId
                };

                context.LikedSongs.Add(likedSong);
                context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.SongId = new SelectList(context.Songs, "Id", "Title", model.SongId);
            return View(model);
        }

        public IActionResult Remove(int id)
        {
            var likedSong = context.LikedSongs
                .Include(ls => ls.Song)
                .FirstOrDefault(ls => ls.Id == id);
            if (likedSong == null)
            {
                return NotFound();
            }
            var model = new LikedSongRemoveViewModel
            {
                Id = likedSong.Id,
                SongTitle = likedSong.Song.Title
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveConfirmed(int id)
        {
            var likedSong = context.LikedSongs.Find(id);

            if (likedSong == null)
            {
                return NotFound();
            }

            context.LikedSongs.Remove(likedSong);
            context.SaveChanges();

            return RedirectToAction(nameof(All));
        }
    }
}
