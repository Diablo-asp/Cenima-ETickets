using Cenima_ETickets.Data;
using Cenima_ETickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;

namespace Cenima_ETickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorController : Controller
    {
        private AppcationDbContext _context = new();
        #region Index
        public IActionResult Index()
        {
            var actors = _context.actors;
            return View(actors.ToList());
        }
        #endregion

        #region movie by actor
        public IActionResult MoviesByActor(int id)
        {
            var actor = _context.actors
                .Include(a => a.ActorMovies)
                .ThenInclude(am => am.movie)
                .FirstOrDefault(a => a.Id == id);

            if (actor == null)
                return NotFound();

            return View(actor);
        }
        #endregion

        #region Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string FirstName, string LastName, string Bio, string News, IFormFile ProfilePic)
        {
            if(ModelState.IsValid) {
                if (ProfilePic != null && ProfilePic.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePic.FileName);
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cast", fileName);

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await ProfilePic.CopyToAsync(stream);
                    }

                    var newActor = new Actor
                    {
                        FirstName = FirstName,
                        LastName = LastName,
                        Bio = Bio,
                        News = News,
                        ProfilePic = fileName
                    };

                    _context.actors.Add(newActor);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "🎉 Actor Create successfully!";

                    return RedirectToAction("Index");
                }
                return View();
            }
            return View();
        }
        #endregion

        #region Edit
        public IActionResult Edit(int id)
        {
            var actor = _context.actors.Find(id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        
        [HttpPost]
        public async Task<IActionResult> Edit(int Id,Actor actor, IFormFile? NewProfilePic)
        {
            ModelState.Remove("movies");
            ModelState.Remove("ActorMovies");
            ModelState.Remove("ProfilePic");
            
            if (ModelState.IsValid)
            {
                var actorInDb = _context.actors.FirstOrDefault(a => a.Id == actor.Id);
                if (actorInDb == null) return NotFound();

                if (NewProfilePic != null && NewProfilePic.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(NewProfilePic.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cast", fileName);

                    using (var stream = System.IO.File.Create(path))
                    {
                        await NewProfilePic.CopyToAsync(stream);
                    }

                    // حذف الصورة القديمة
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cast", actorInDb.ProfilePic ?? "");
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    actorInDb.ProfilePic = fileName;
                }

                actorInDb.FirstName = actor.FirstName;
                actorInDb.LastName = actor.LastName;
                actorInDb.Bio = actor.Bio;
                actorInDb.News = actor.News;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "🎉 Actor Edit successfully!";

                return RedirectToAction(nameof(Index));
            }
            var actor1 = _context.actors.Find(Id);
            if (actor1 == null) return NotFound();

            return View(actor1);
        }
        #endregion

        #region Delete
        public IActionResult Delete(int id)
        {
            var actor = _context.actors.Find(id);
            if (actor == null) return NotFound();

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cast", actor.ProfilePic ?? "");
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _context.actors.Remove(actor);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "🎉 Actor Delete successfully!";

            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
