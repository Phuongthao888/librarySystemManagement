using library.Models;
using System.Linq;
using System.Web.Mvc;

namespace library.Controllers
{
    public class DashboardController : Controller
    {
        // Instantiate the database context
        private librarySystemEntities library = new librarySystemEntities();

        public ActionResult Index()
        {
            var model = new DashboardViewModel
            {
                TotalReaders = library.reader.Count(),
                TotalStaff = library.staff.Count(),
                TotalSuppliers = library.supplier.Count(),
                TotalBooks = library.book.Count(),
                TotalBookshelf = library.bookshelf.Count()
            };

            return View(model);
        }

        // Dispose of the context after use
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                library.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
