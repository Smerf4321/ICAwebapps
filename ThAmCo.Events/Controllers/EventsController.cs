using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Events.Data;
using ThAmCo.Events.Models;
using ThAmCo.Events.ViewModels;

namespace ThAmCo.Events.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventsDbContext _context;

        public EventsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.Include(e => e.Staffing).ThenInclude(e => e.Staff).Include(e => e.Bookings).ToListAsync());
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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Date,Duration,TypeId")] EventCreateVM eventVm)
        {
            if (ModelState.IsValid)
            {
                var addEvent = new Event() {
                    Title = eventVm.Title,
                    Date = eventVm.Date,
                    Duration = eventVm.Duration,
                    TypeId = eventVm.TypeId
                };

                _context.Add(addEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eventVm);
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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Date,Duration,TypeId")] Event @event)
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

            if (@event.venueRef != null)
            {
                HttpClient venueClient = new HttpClient();

                var builder = new UriBuilder("http://localhost");
                builder.Port = 23652;
                builder.Path = "api/Reservations/" + @event.venueRef;

                string url = builder.ToString();

                venueClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");

                var venuesResponse = await venueClient.DeleteAsync(url);

                if (!venuesResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Service unavailable");
                    return RedirectToAction(nameof(ReserveVenue), @event.Id);
                }

                @event.venueRef = null;
                _context.Update(@event);
                await _context.SaveChangesAsync();
            }

            var evetStaffing = await _context.EventStaffing.FindAsync(id);
            _context.EventStaffing.RemoveRange(_context.EventStaffing.Where(s => s.EventId == id));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> ReserveVenue(int id)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return NotFound();
            }

            HttpClient client = new HttpClient();

            var Builder = new UriBuilder("http://localhost");
            Builder.Port = 23652;
            Builder.Path = "api/Availability";
            var query = HttpUtility.ParseQueryString(Builder.Query);
            query["eventType"] = @event.TypeId;
            query["beginDate"] = @event.Date.ToString("yyyy/MM/dd HH:mm:ss");
            query["endDate"] = @event.Date.Add(@event.Duration.Value).ToString("yyyy/MM/dd HH:mm:ss");

            Builder.Query = query.ToString();
            string url = Builder.ToString();

            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var venueList = await response.Content.ReadAsAsync<IEnumerable<Venue>>();
                if (venueList.Count() == 0)
                {
                    ModelState.AddModelError("", "No suitable venues found");
                }

                ViewData["venues"] = new SelectList(venueList, "Code", "Name");
                return View();
            }
            else
            {
                ModelState.AddModelError("", "Service Unavailable");
            }
            return View();
        }

        public async Task<IActionResult> Reserve(int id, string venueRef)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                return BadRequest();
            }

            if (@event.venueRef != null)
            {
                HttpClient venueClient = new HttpClient();

                var builder = new UriBuilder("http://localhost");
                builder.Port = 23652;
                builder.Path = "api/Reservations/" + @event.venueRef;

                string url = builder.ToString();

                venueClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");

                var venuesResponse = await venueClient.DeleteAsync(url);

                if (!venuesResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Service unavailable");
                    return RedirectToAction(nameof(ReserveVenue), @event.Id);
                }

                @event.venueRef = null;
                _context.Update(@event);
                await _context.SaveChangesAsync();
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new System.Uri("http://localhost:23652");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            var request = new ReservationPostDto
            {
                EventDate = @event.Date,
                VenueCode = venueRef,
                StaffId = "TODO"
            };

            var response = await client.PostAsJsonAsync("api/Reservations", request);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Service unavailable");
                return RedirectToAction(nameof(ReserveVenue), @event.Id);
            }

            var reservationInfo = await response.Content.ReadAsAsync<ReservationGetDto>();

            @event.venueRef = reservationInfo.Reference;
            _context.Update(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
