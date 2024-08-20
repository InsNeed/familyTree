using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FamilyTree.Data;
using FamilyTree.Models;
using FamilyTree.Models.ViewModel;

namespace FamilyTree.Controllers
{
    public class EventController : Controller
    {
        private readonly FamilyTreeContext _context;

        public EventController(FamilyTreeContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            // 初始化PersonEventView
            var personEventView = new PersonEventView();

            personEventView.Events = await _context.Events.ToListAsync();
            personEventView.Persons = await _context.Persons.ToListAsync();

            // 返回视图，并传递模型数据
            return View(personEventView);
        }


        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Location,Activitie")] Event @event,
            string name)
        {
            if (ModelState.IsValid)
            {
                Func<string, bool> isNumeric = str => int.TryParse(str, out _);
                if (isNumeric(name))
                {
                    int id = Convert.ToInt32(name);
                    Person person = _context.Persons
                        .Where(n => n.ID == id)
                        .AsNoTracking()
                        .FirstOrDefault();
                    if(person == null)
                    {
                        ModelState.AddModelError("", "未找到此人");
                        return View();
                    }
                }
                else
                {
                    List<Person> PersonList = await _context.Persons
                        .Where(n => n.LastName + n.FirstName == name)
                        .AsNoTracking()
                        .ToListAsync();
                    if (PersonList.Count > 1)
                    {
                        ModelState.AddModelError("", "有重名，输入ID");
                    }
                    else if (PersonList.Count == 0)
                    {
                        ModelState.AddModelError("", "未找到此人");
                    }
                }
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PersonId,Date,Location,Activitie")] Event @event)
        {
            if (id != @event.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.Id))
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
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
