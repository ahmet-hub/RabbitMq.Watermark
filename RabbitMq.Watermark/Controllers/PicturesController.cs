using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RabbitMq.Watermark.Models;
using RabbitMq.Watermark.Services;

namespace RabbitMq.Watermark.Controllers
{
    public class PicturesController : Controller
    {
        private readonly Models.AppContext _context;
        private readonly RabbitMqPublisher _rabbitMqPublisher;

        public PicturesController(Models.AppContext context, RabbitMqPublisher rabbitMqPublisher)
        {
            _context = context;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        // GET: Pictures
        public async Task<IActionResult> Index()
        {
            return View(await _context.Picture.ToListAsync());
        }

        // GET: Pictures/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var picture = await _context.Picture
                .FirstOrDefaultAsync(m => m.Id == id);
            if (picture == null)
            {
                return NotFound();
            }

            return View(picture);
        }

        // GET: Pictures/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pictures/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Text,FileName")] Picture picture,IFormFile ImageFile)
        {
            if (!ModelState.IsValid) return View(picture);

            if (ImageFile is { Length: > 0 })
            {
                var imageName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", imageName);

                await using FileStream stream = new(path, FileMode.Create);

                await ImageFile.CopyToAsync(stream);

                _rabbitMqPublisher.Publish(new PictureCreatedEvent() { ImageName = imageName });
                picture.FileName = imageName;
            }

            _context.Add(picture);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Pictures/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var picture = await _context.Picture.FindAsync(id);
            if (picture == null)
            {
                return NotFound();
            }
            return View(picture);
        }

        // POST: Pictures/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Text,FileName")] Picture picture)
        {
            if (id != picture.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(picture);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PictureExists(picture.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(picture);
        }

        // GET: Pictures/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var picture = await _context.Picture
                .FirstOrDefaultAsync(m => m.Id == id);
            if (picture == null)
            {
                return NotFound();
            }

            return View(picture);
        }

        // POST: Pictures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var picture = await _context.Picture.FindAsync(id);
            _context.Picture.Remove(picture);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PictureExists(string id)
        {
            return _context.Picture.Any(e => e.Id == id);
        }
    }
}
