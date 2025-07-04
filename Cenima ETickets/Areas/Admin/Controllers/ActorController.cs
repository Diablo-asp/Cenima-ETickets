using Cinema_ETickets.Data;
using Cinema_ETickets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;

namespace Cinema_ETickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorController : Controller
    {
        private IActorRepository _actorRepository;
        private ApplicationDbContext _context = new();

        public ActorController(IActorRepository actorRepository)
        {
            _actorRepository = actorRepository;
        }

        #region Index
        public async Task<IActionResult> Index()
        {
            var actors = await _actorRepository.GetAsync();
            return View(actors);
        }
        #endregion

        #region movie by actor
        public async Task<IActionResult> MoviesByActor(int id)
        {
            var actor = await _actorRepository.GetActorWithMoviesAsync(id);

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

                    await _actorRepository.CreateAsync(newActor);
                    TempData["SuccessMessage"] = "🎉 Actor Create successfully!";

                    return RedirectToAction("Index");
                }
                return View();
            }
            return View();
        }
        #endregion

        #region Edit
        public async Task<IActionResult> Edit(int id)
        {
            var actor = await _actorRepository.GetOneAsync(e => e.Id == id);
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
                var actorInDb = await _actorRepository.GetOneAsync(a => a.Id == actor.Id);
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
                await _actorRepository.UpdateAsync(actorInDb);
                TempData["SuccessMessage"] = "🎉 Actor Edit successfully!";

                return RedirectToAction(nameof(Index));
            }
            var actor1 = await _actorRepository.GetOneAsync(e => e.Id == Id);
            if (actor1 == null) return NotFound();

            return View(actor1);
        }
        #endregion

        #region Delete
        public async Task<IActionResult> Delete(int id)
        {
            var actor = await _actorRepository.GetOneAsync(e => e.Id == id);
            if (actor == null) return NotFound();

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/cast", actor.ProfilePic ?? "");
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            await _actorRepository.DeleteAsync(actor);
            TempData["SuccessMessage"] = "🎉 Actor Delete successfully!";

            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
