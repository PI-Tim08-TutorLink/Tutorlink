using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutorLinkApp.Models;

namespace TutorLinkApp.Controllers
{
    public class TutorController : Controller
    {
        private readonly TutorLinkContext _context;
        public TutorController(TutorLinkContext context)
        {
            _context = context;
        }

        // GET: TutorController
        public async Task<IActionResult> Index()
        {
            var tutors = await _context.Tutors.Include(t => t.User)
                .ToListAsync();

            return View(tutors);
        }

        // GET: TutorController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TutorController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TutorController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TutorController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TutorController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TutorController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TutorController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
