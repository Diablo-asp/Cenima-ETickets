﻿using System.Linq.Expressions;
using System.Threading.Tasks;
using Cinema_ETickets.Data;
using Cinema_ETickets.Models;
using Cinema_ETickets.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema_ETickets.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin},{SD.Employe},{SD.Company}")]

    public class CinemaController : Controller
    {
        private ApplicationDbContext _context = new();
        private ICinemaRepository _cinemarepository;

        public CinemaController(ICinemaRepository cinemaRepository)
        {
            _cinemarepository = cinemaRepository;
        }

        #region Index
        public async Task<IActionResult> Index()
        {
            var cinemas = await _cinemarepository.GetAsync();
            return View(cinemas);
        }
        #endregion
                
        #region Create
        [HttpGet]
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]

        public IActionResult Create()
        {
            return View(new Cenima());
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Create(string Name, string Description, string Address, IFormFile CenimaLogo)
        {
            if (!ModelState.IsValid)
            {
                return View(new Cenima());
            }
            if (CenimaLogo != null && CenimaLogo.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(CenimaLogo.FileName);
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Cinemas", fileName);

                using var stream = new FileStream(savePath, FileMode.Create);
                await CenimaLogo.CopyToAsync(stream);

                var cinema = new Cenima
                {
                    Name = Name,
                    Description = Description,
                    Address = Address,
                    CenimaLogo = fileName
                };

                await _cinemarepository.CreateAsync(cinema);
                TempData["SuccessMessage"] = "🎉 Cinema created successfully!";

                return RedirectToAction("Index");
            }
            return View(new Cenima());
        }
        #endregion

        #region movie by cinema
        public async Task<IActionResult> MoviesByCinema(int id)
        {
            var cinema = await _cinemarepository.GetCinemaWithMoviesAsync(id);

            if (cinema == null)
                return NotFound();

            return View(cinema);
        }

        #endregion 

        #region Edit
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Edit(int id)
        {
            var cinema = await _cinemarepository.GetOneAsync(c => c.Id == id);
            if (cinema is null) return NotFound();

            return View(cinema);
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Edit(int id, Cenima model, IFormFile? CenimaLogo)
        {
            ModelState.Remove("CenimaLogo");
            ModelState.Remove("movies");
            if (ModelState.IsValid)
            {
                var cinemaInDb = await _cinemarepository.GetOneAsync(c => c.Id == id);
                if (cinemaInDb is null) return NotFound();

                // لو تم رفع صورة جديدة
                if (CenimaLogo != null && CenimaLogo.Length > 0)
                {
                    var newFileName = Guid.NewGuid().ToString() + Path.GetExtension(CenimaLogo.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Cinemas", newFileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await CenimaLogo.CopyToAsync(stream);
                    }

                    // احذف الصورة القديمة
                    var oldPath = Path.Combine("wwwroot/images/Cinemas", cinemaInDb.CenimaLogo);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    cinemaInDb.CenimaLogo = newFileName;
                }

                // تحديث البيانات
                cinemaInDb.Name = model.Name;
                cinemaInDb.Description = model.Description;
                cinemaInDb.Address = model.Address;

                await _cinemarepository.UpdateAsync(cinemaInDb);

                TempData["SuccessMessage"] = "🎉 Cinema Edit successfully!";

                return RedirectToAction("Index");
            }
            var cinema = await _cinemarepository.GetOneAsync(c => c.Id == id);
            if (cinema is null) return NotFound();

            return View(cinema);
        }
        #endregion

        #region Delete
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cinema = await _cinemarepository.GetOneAsync(c => c.Id == id);
            if (cinema is null) return NotFound();

            // حذف الصورة من المجلد
            var imagePath = Path.Combine("wwwroot/images/Cinemas", cinema.CenimaLogo);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);

            await _cinemarepository.DeleteAsync(cinema);
            TempData["SuccessMessage"] = "🎉 Cinema Delete successfully!";

            return RedirectToAction("Index");
        }
        #endregion

    }
}
