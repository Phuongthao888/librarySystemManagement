using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using OfficeOpenXml;
using System.IO;

namespace library.Controllers
{
    public class BookController : Controller
    {
        librarySystemEntities library = new librarySystemEntities();
        public ActionResult Index(FormCollection searchdata)
        {
            var books = library.book
                .Include(b => b.category)
                .Include(b => b.bookshelf)
                .ToList();

            if (!string.IsNullOrEmpty(searchdata["Search"]))
            {
                string searchKeyword = searchdata["Search"].ToLower();
                books = books.Where(x => x.titleBook.ToLower().Contains(searchKeyword)).ToList();
            }

            return View(books);
        }

        public ActionResult Details(int id)
        {
            var book = library.book
                .Include(b => b.category)
                .Include(b => b.bookshelf)
                .FirstOrDefault(b => b.idBook == id); 

            if (book == null)
            {
                return HttpNotFound(); // Return 404 if the book is not found
            }

            return View(book); // Pass the single book object to the view
        }

        // GET: Create
        public ActionResult Create()
        {
            // Get the list of categories to populate the dropdown
            var categories = library.category.ToList();
            var bookshelf = library.bookshelf.ToList();
            ViewBag.fk_idCategory = new SelectList(categories, "idCategory", "nameCategory");
            ViewBag.fk_idBookshelf = new SelectList(bookshelf, "idBookshelf", "idBookshelf");
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(book newBook)
        {
            if (ModelState.IsValid) // Check if the model is valid
            {
                library.book.Add(newBook); // Add the new book to the context
                library.SaveChanges(); // Save changes to the database

                return RedirectToAction("Details", new { id = newBook.idBook }); // Redirect to the Details page of the newly created book
            }

            var categories = library.category.ToList();
            var bookshelf = library.bookshelf.ToList();
            ViewBag.fk_idCategory = new SelectList(categories, "idCategory", "nameCategory");
            ViewBag.fk_idBookshelf = new SelectList(bookshelf, "idBookshelf", "nameBookshelfRow");
            return View(newBook); // Return the view with the invalid model
        }
        // GET: Edit
        public ActionResult Edit(int id)
        {
            // Find the book by id
            var book = library.book
                .Include(b => b.category)
                .Include(b => b.bookshelf)
                .FirstOrDefault(b => b.idBook == id); // Find the book by id

            if (book == null)
            {
                return HttpNotFound(); // Return 404 if the book is not found
            }

            var categories = library.category.ToList();
            var bookshelf = library.bookshelf.ToList();
            ViewBag.fk_idCategory = new SelectList(categories, "idCategory", "nameCategory");
            ViewBag.fk_idBookshelf = new SelectList(bookshelf, "idBookshelf", "nameBookshelfRow");
            return View(book); // Pass the book object to the view
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(book updatedBook)
        {
            if (ModelState.IsValid) // Check if the model is valid
            {
                // Attach the updatedBook to the context
                library.Entry(updatedBook).State = EntityState.Modified;
                library.SaveChanges(); // Save changes to the database

                return RedirectToAction("Details", new { id = updatedBook.idBook }); // Redirect to the Details page of the updated book
            }

            var categories = library.category.ToList();
            var bookshelf = library.bookshelf.ToList();
            ViewBag.fk_idCategory = new SelectList(categories, "idCategory", "nameCategory");
            ViewBag.fk_idBookshelf = new SelectList(bookshelf, "idBookshelf", "nameBookshelfRow");
            return View(updatedBook); // Return the view with the invalid model
        }
        public ActionResult Delete(int id)
        {
            // Chuyển hướng trực tiếp đến DeleteConfirmed
            return RedirectToAction("DeleteConfirmed", new { id = id });
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var book = library.book.Find(id);
            if (book != null)
            {
                library.book.Remove(book);
                library.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        public ActionResult GetPreviewData()
        {
            var books = library.book.Select(b => new
            {
                b.idBook,
                b.quantity,
                b.quantityTotal,
                category = new { b.category.nameCategory },
                bookshelf = new { b.bookshelf.nameBookshelfRow, b.bookshelf.shelfPosition },
                b.titleBook,
                b.authorName,
                b.publisher,
                b.pubDate,
                b.reprintDate,
                b.numberOfPages,
                b.bookLanguage,
                b.price
            }).ToList();

            return Json(books, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportToExcel()
        {
            var books = library.book.ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Books");

                // Add Header Row
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Quantity";
                worksheet.Cells[1, 3].Value = "Quantity Total";
                worksheet.Cells[1, 4].Value = "Category";
                worksheet.Cells[1, 5].Value = "Bookshelf";
                worksheet.Cells[1, 6].Value = "Title";
                worksheet.Cells[1, 7].Value = "Author";
                worksheet.Cells[1, 8].Value = "Publisher";
                worksheet.Cells[1, 9].Value = "Publication Date";
                worksheet.Cells[1, 10].Value = "Reprint Date";
                worksheet.Cells[1, 11].Value = "Pages";
                worksheet.Cells[1, 12].Value = "Language";
                worksheet.Cells[1, 13].Value = "Price";

                // Populate Data Rows
                for (int i = 0; i < books.Count; i++)
                {
                    var book = books[i];
                    worksheet.Cells[i + 2, 1].Value = book.idBook;
                    worksheet.Cells[i + 2, 2].Value = book.quantity;
                    worksheet.Cells[i + 2, 3].Value = book.quantityTotal;
                    worksheet.Cells[i + 2, 4].Value = book.category.nameCategory;
                    worksheet.Cells[i + 2, 5].Value = $"{book.bookshelf.nameBookshelfRow} - {book.bookshelf.shelfPosition}";
                    worksheet.Cells[i + 2, 6].Value = book.titleBook;
                    worksheet.Cells[i + 2, 7].Value = book.authorName;
                    worksheet.Cells[i + 2, 8].Value = book.publisher;
                    worksheet.Cells[i + 2, 9].Value = book.pubDate?.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheet.Cells[i + 2, 10].Value = book.reprintDate?.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheet.Cells[i + 2, 11].Value = book.numberOfPages;
                    worksheet.Cells[i + 2, 12].Value = book.bookLanguage;
                    worksheet.Cells[i + 2, 13].Value = book.price;
                }

                // Set the column width for better readability
                worksheet.Cells.AutoFitColumns();

                // Convert the Excel package to a byte array and return as a file download
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = "BooksList.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        public ActionResult ExportOutdatedBooks()
        {
            int outdatedYearThreshold = DateTime.Now.Year - 5; // For 2024, this is 2019
            var outdatedBooks = library.book
                .Where(b => b.pubDate.HasValue && b.pubDate.Value.Year <= outdatedYearThreshold)
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Outdated Books");

                // Add Header Row
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Quantity";
                worksheet.Cells[1, 3].Value = "Quantity Total";
                worksheet.Cells[1, 4].Value = "Category";
                worksheet.Cells[1, 5].Value = "Bookshelf";
                worksheet.Cells[1, 6].Value = "Title";
                worksheet.Cells[1, 7].Value = "Author";
                worksheet.Cells[1, 8].Value = "Publisher";
                worksheet.Cells[1, 9].Value = "Publication Date";
                worksheet.Cells[1, 10].Value = "Pages";
                worksheet.Cells[1, 11].Value = "Language";
                worksheet.Cells[1, 12].Value = "Price";

                // Populate Data Rows
                for (int i = 0; i < outdatedBooks.Count; i++)
                {
                    var book = outdatedBooks[i];
                    worksheet.Cells[i + 2, 1].Value = book.idBook;
                    worksheet.Cells[i + 2, 2].Value = book.quantity;
                    worksheet.Cells[i + 2, 3].Value = book.quantityTotal;
                    worksheet.Cells[i + 2, 4].Value = book.category.nameCategory;
                    worksheet.Cells[i + 2, 5].Value = $"{book.bookshelf.nameBookshelfRow} - {book.bookshelf.shelfPosition}";
                    worksheet.Cells[i + 2, 6].Value = book.titleBook;
                    worksheet.Cells[i + 2, 7].Value = book.authorName;
                    worksheet.Cells[i + 2, 8].Value = book.publisher;
                    worksheet.Cells[i + 2, 9].Value = book.pubDate?.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheet.Cells[i + 2, 10].Value = book.numberOfPages;
                    worksheet.Cells[i + 2, 11].Value = book.bookLanguage;
                    worksheet.Cells[i + 2, 12].Value = book.price;
                }

                // Set the column width for better readability
                worksheet.Cells.AutoFitColumns();

                // Convert the Excel package to a byte array and return as a file download
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = "OutdatedBooksList.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

    }
}