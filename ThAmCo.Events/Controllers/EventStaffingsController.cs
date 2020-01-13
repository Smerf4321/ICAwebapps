using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Events.Data;

namespace ThAmCo.Events.Controllers
{
    public class EventStaffingsController : Controller
    {
        private readonly EventsDbContext _context;

        public EventStaffingsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: EventStaffings
        public async Task<IActionResult> Index()
        {
            var eventsDbContext = _context.EventStaffing.Include(e => e.Event).Include(e => e.Staff);
            return View(await eventsDbContext.ToListAsync());
        }

        public async Task<IActionResult> EventIndex(int id)
        {
            ViewData["EventId"] = id;
            ViewData["EventName"] = _context.Events.FirstOrDefault(e => e.Id == id).Title;

            var eventsDbContext = _context.EventStaffing.Include(e => e.Staff).Include(e => e.Event).Where(e => e.EventId == id);

            var eve = await _context.Events.Include(e => e.Bookings).FirstOrDefaultAsync(e => e.Id == id);

            if (eventsDbContext.Select(s => s.Staff.FirstAider).Count() == 0)
            {
                ModelState.AddModelError("", "No first aider");
            }
            if (!((eventsDbContext.Select(s => s.Staff).Count() * 10) >= eve.Bookings.Count()))
            {
                ModelState.AddModelError("", "Not enough staff assigned");
            }

            return View("Index", await eventsDbContext.ToListAsync());
        }

        // GET: EventStaffings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventStaffing = await _context.EventStaffing
                .Include(e => e.Event)
                .Include(e => e.Staff)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (eventStaffing == null)
            {
                return NotFound();
            }

            return View(eventStaffing);
        }

        // GET: EventStaffings/Create
        public IActionResult Create(int? eventID)
        {
            ViewData["EventId"] = new SelectList(_context.Events.Where(e => e.Id == eventID), "Id", "Title");
            ViewData["StaffId"] = new SelectList(_context.Staff.Where(c => !_context.EventStaffing.Where(g => g.EventId == eventID).Any(g => g.StaffId == c.Id)), "Id", "Email");
            return View();
        }

        // POST: EventStaffings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StaffId,EventId")] EventStaffing eventStaffing)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eventStaffing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(EventIndex) + "/" + eventStaffing.EventId);
            }
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", eventStaffing.EventId);
            ViewData["StaffId"] = new SelectList(_context.Staff, "Id", "Email", eventStaffing.StaffId);
            return RedirectToAction(nameof(EventIndex) + "/" + eventStaffing.EventId);
        }

        // GET: EventStaffings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventStaffing = await _context.EventStaffing.FindAsync(id);
            if (eventStaffing == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", eventStaffing.EventId);
            ViewData["StaffId"] = new SelectList(_context.Staff, "Id", "Email", eventStaffing.StaffId);
            return View(eventStaffing);
        }

        // POST: EventStaffings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StaffId,EventId")] EventStaffing eventStaffing)
        {
            if (id != eventStaffing.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventStaffing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventStaffingExists(eventStaffing.Id))
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
            ViewData["EventId"] = new SelectList(_context.Events, "Id", "Title", eventStaffing.EventId);
            ViewData["StaffId"] = new SelectList(_context.Staff, "Id", "Email", eventStaffing.StaffId);
            return View(eventStaffing);
        }

        // GET: EventStaffings/Delete/5
        public async Task<IActionResult> Delete(int eventid, int staffid)
        {
            var eventStaffing = await _context.EventStaffing
                .Include(e => e.Event)
                .Include(e => e.Staff)
                .FirstOrDefaultAsync(m => m.StaffId == staffid && m.EventId == eventid);
            if (eventStaffing == null)
            {
                return NotFound();
            }

            return View(eventStaffing);
        }

        // POST: EventStaffings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int eventid, int staffid)
        {
            var eventStaffing = await _context.EventStaffing.FirstOrDefaultAsync(m => m.StaffId == staffid && m.EventId == eventid);
            _context.EventStaffing.Remove(eventStaffing);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(EventIndex), new { id = eventid });
        }

        private bool EventStaffingExists(int id)
        {
            return _context.EventStaffing.Any(e => e.Id == id);
        }
    }
}
