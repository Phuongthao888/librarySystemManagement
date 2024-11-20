using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using library;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace library.Controllers
{
    public class importRepositoriesController : Controller
    {
        librarySystemEntities library = new librarySystemEntities();
        public ActionResult Index(FormCollection searchdata)
        {
            var import = library.importRepository
                .Include(i => i.supplier)  // Include related reader data
                .Include(i => i.staff)   // Include related staff data
                .Include(i => i.book)    // Include related book data
                .ToList();

            if (!string.IsNullOrEmpty(searchdata["Search"]))
            {
                string searchKeyword = searchdata["Search"].ToLower();
                import = import.Where(x => x.staff.nameStaff.ToLower().Contains(searchKeyword)).ToList();
            }
            return View(import);
        }
        // GET: Create
        public ActionResult Create()
        {
            var supplier = library.supplier.ToList();
            var staff = library.staff.ToList();
            var books = library.book.ToList();

            ViewBag.fk_idSupplier = new SelectList(supplier, "idSupplier", "nameSupplier");
            ViewBag.fk_idStaff = new SelectList(staff, "idStaff", "nameStaff");
            ViewBag.fk_idBook = new SelectList(books, "idBook", "titleBook");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(importRepository newImport)
        {
            if (ModelState.IsValid)
            {
                // Find the book being importRepositories
                var book = library.book.Find(newImport.fk_idBook);
                if (book != null && book.quantity >= newImport.quantity) // Check if enough books are available
                {
                    // Update the book's quantity
                    book.quantityTotal += newImport.quantity;
                    book.quantity += newImport.quantity;

                    // Calculate the total price
                    newImport.price = book.price * newImport.quantity;

                    // Add the transaction
                    library.importRepository.Add(newImport);
                    library.SaveChanges();

                // Clear the editable fields for the next transaction
                newImport = new importRepository
                {
                        fk_idBook = newImport.fk_idBook,
                        quantity = newImport.quantity,
                    };
                    return View(newImport); // Stay on the same page
                }
            }

            var supplier = library.supplier.ToList();
            var staff = library.staff.ToList();
            var books = library.book.ToList();

            ViewBag.fk_idSupplier = new SelectList(supplier, "idSupplier", "nameSupplier", newImport.fk_idSupplier);
            ViewBag.fk_idStaff = new SelectList(staff, "idStaff", "nameStaff", newImport.fk_idStaff);
            ViewBag.fk_idBook = new SelectList(books, "idBook", "titleBook", newImport.fk_idBook);
            return View(newImport);
        }

        public ActionResult Delete(int id)
        {
            // Redirect to DeleteConfirmed
            return RedirectToAction("DeleteConfirmed", new { id = id });
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int idImport)
        {
            // Find all import records that match the specified idImport
            var imports = library.importRepository.Where(i => i.idImport == idImport).ToList();

            if (imports.Any())
            {
                // Check if all importDates are today's date
                if (imports.All(i => i.importDate == DateTime.Now.Date))
                {
                    // Loop through each import and update the corresponding book's quantities
                    foreach (var import in imports)
                    {
                        var book = library.book.Find(import.fk_idBook);
                        if (book != null)
                        {
                            book.quantityTotal -= import.quantity;
                            book.quantity -= import.quantity;
                        }

                        // Remove each import record
                        library.importRepository.Remove(import);
                    }

                    // Save all changes to the database
                    library.SaveChanges();

                    return RedirectToAction("Index");
                }
                else
                {
                    // Display an error message if any importDate does not match today's date
                    ModelState.AddModelError("", "Cannot delete these records because one or more import dates do not match today's date.");
                }
            }

            // Return to the Index view with an error message if no records found
            return RedirectToAction("Index");
        }
        public ActionResult PrintImportTransaction(int id)
        {
            var import = library.importRepository
                .Where(i => i.idImport == id)
                .ToList();

            if (import == null || !import.Any())
            {
                return HttpNotFound();
            }

            using (var ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 50, 50, 25, 25); // Set margins
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Title (simulate a bill header)
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var regularFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

                // Add a header
                document.Add(new Paragraph("Warehouse Import Bill", titleFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("Library PT", boldFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("123 TanBinh, 123 456 789, pt@gmai.com", regularFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("\n")); // Spacer

                // General Information
                var firstRecord = import.First();
                document.Add(new Paragraph($"Import ID: {firstRecord.idImport}", regularFont));
                document.Add(new Paragraph($"Supplier Name: {firstRecord.supplier.nameSupplier}", regularFont));
                document.Add(new Paragraph($"Staff Name: {firstRecord.staff.nameStaff}", regularFont));
                document.Add(new Paragraph($"Import Date: {firstRecord.importDate:dd/MM/yyyy}", regularFont));
                document.Add(new Paragraph("\n")); // Spacer

                // Create a table for book details
                PdfPTable table = new PdfPTable(4); // 4 columns
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 3f, 1f, 1f, 2f }); // Column widths for better layout

                // Table headers with background color
                PdfPCell cell;
                cell = new PdfPCell(new Phrase("Book Title", boldFont));
                cell.BackgroundColor = new BaseColor(230, 230, 250); // Light lavender background
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Price", boldFont));
                cell.BackgroundColor = new BaseColor(230, 230, 250);
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Quantity", boldFont));
                cell.BackgroundColor = new BaseColor(230, 230, 250);
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Total Price", boldFont));
                cell.BackgroundColor = new BaseColor(230, 230, 250);
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                table.AddCell(cell);

                // Total price calculation
                decimal grandTotalPrice = 0;

                // Populate the table with import items
                foreach (var item in import)
                {
                    decimal price = item.price ?? 0;
                    int quantity = item.quantity ?? 0;
                    decimal totalPrice = price * quantity;

                    table.AddCell(new Phrase(item.book.titleBook, regularFont));

                    cell = new PdfPCell(new Phrase(price.ToString("C"), regularFont));
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(quantity.ToString(), regularFont));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);

                    cell = new PdfPCell(new Phrase(totalPrice.ToString("C"), regularFont));
                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    table.AddCell(cell);

                    grandTotalPrice += totalPrice;
                }

                document.Add(table); // Add the table to the document

                // Add Grand Total Price to the document
                document.Add(new Paragraph("\n")); // Adding space
                Paragraph grandTotal = new Paragraph($"Grand Total: {grandTotalPrice:C}", boldFont);
                grandTotal.Alignment = Element.ALIGN_RIGHT;
                document.Add(grandTotal);

                // Footer
                document.Add(new Paragraph("\nThank you for your business!", regularFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("Please retain this document for your records.", regularFont) { Alignment = Element.ALIGN_CENTER });

                document.Close();

                return File(ms.ToArray(), "application/pdf", $"ImportTransaction_{id}.pdf");
            }
        }


    }
}