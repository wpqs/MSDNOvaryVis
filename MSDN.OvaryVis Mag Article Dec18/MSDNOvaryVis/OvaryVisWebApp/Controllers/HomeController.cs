using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using OvaryVisWebApp.Data;
using OvaryVisWebApp.Models;

namespace OvaryVisWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Id,D1mm,D2mm,D3mm")] OvaryVis form)
        {
            OvaryVis record = new OvaryVis
            {
                D1mm = form.D1mm,
                D2mm = form.D2mm,
                D3mm = form.D3mm,
                JobSubmitted = DateTime.UtcNow,
                ResultVis = -1,
                StatusMsg = String.Format("Job created at {0}", DateTime.UtcNow.ToString("dd-MM-yy, HH:mm:ss"))
            };

            _context.Add(record);
            await _context.SaveChangesAsync();

            var message = new Message(Encoding.UTF8.GetBytes(record.Id));
            await Startup.GetQueueClient().SendAsync(message);

            return RedirectToAction("Result", new { id = record.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Result(string Id)
        {
            var record = await _context.OvaryVis.SingleOrDefaultAsync(a => a.Id == Id);
            if (record == null)
                record = new OvaryVis();

            return View(record);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
