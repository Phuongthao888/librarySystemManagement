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
namespace librarySystem.Controllers
{
    public class TransactionBorrowController : Controller
    {
        librarySystemEntities library = new librarySystemEntities();
        public ActionResult Index(FormCollection searchdata)
        {
            var transactions = library.transactionBorrow
                .Include(t => t.reader)  // Include related reader data
                .Include(t => t.staff)   // Include related staff data
                .Include(t => t.book)    // Include related book data
                .ToList();
            if (!string.IsNullOrEmpty(searchdata["Search"]))
            {
                string searchKeyword = searchdata["Search"].ToLower();
                transactions = transactions.Where(x => x.reader.fullname.ToLower().Contains(searchKeyword)).ToList();
            }
            return View(transactions);
        }
        // GET: Create
        public ActionResult Create()
        {
            var books = library.book.ToList();
            var readers = library.reader.ToList();
            var staff = library.staff.ToList();

            ViewBag.fk_idBook = new SelectList(books, "idBook", "titleBook");
            ViewBag.fk_idReader = new SelectList(readers, "idReader", "fullName");
            ViewBag.fk_idStaff = new SelectList(staff, "idStaff", "nameStaff");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(transactionBorrow newTransaction)
        {
            if (ModelState.IsValid)
            {
                // Check the count of borrowed books for the reader
                var borrowedCount = library.transactionBorrow
                    .Count(t => t.fk_idReader == newTransaction.fk_idReader);

                if (borrowedCount < 3)
                {
                    // Check book availability
                    var book = library.book.Find(newTransaction.fk_idBook);
                    if (book != null && book.quantity >= newTransaction.quantity)
                    {
                        // Update quantity and calculate price
                        book.quantity -= newTransaction.quantity;
                        newTransaction.price = book.price * newTransaction.quantity;

                        // Add transaction and save changes
                        library.transactionBorrow.Add(newTransaction);
                        library.SaveChanges();

                        // Clear the form fields for a new transaction
                        newTransaction = new transactionBorrow
                        {
                            fk_idReader = newTransaction.fk_idReader,
                            fk_idStaff = newTransaction.fk_idStaff,
                            borrowDate = newTransaction.borrowDate,
                            dueDate = newTransaction.dueDate
                        };
                    }
                    else
                    {
                        ModelState.AddModelError("quantity", "Not enough books available to borrow.");
                    }
                }
                else
                {
                    ModelState.AddModelError("fk_idBook", "This reader has already borrowed the maximum number of books.");
                }
            }

            // Repopulate the select lists for the dropdowns
            ViewBag.fk_idBook = new SelectList(library.book, "idBook", "titleBook", newTransaction.fk_idBook);
            ViewBag.fk_idReader = new SelectList(library.reader, "idReader", "fullName", newTransaction.fk_idReader);
            ViewBag.fk_idStaff = new SelectList(library.staff, "idStaff", "nameStaff", newTransaction.fk_idStaff);

            return View(newTransaction); // Stay on the same page with a cleared form
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
            var borrow = library.transactionBorrow.Find(id);
            if (borrow != null)
            {
                // Tìm sách theo idBook từ transaction bị xóa
                var book = library.book.Find(borrow.fk_idBook);

                if (book != null)
                {
                    // Cộng lại số lượng sách đã mượn vào quantity của sách
                    book.quantity += borrow.quantity;
                }

                // Xóa transaction
                library.transactionBorrow.Remove(borrow);
                library.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        //public ActionResult DeleteAllByBorrowId(int idBorrow)
        //{
        //    var transactions = library.transactionBorrow
        //        .Where(t => t.idBorrow == idBorrow)
        //        .ToList();

        //    if (transactions.Any())
        //    {
        //        foreach (var transaction in transactions)
        //        {
        //            // Find the book related to the transaction
        //            var book = library.book.Find(transaction.fk_idBook);
        //            if (book != null)
        //            {
        //                // Restore the quantity back to the book
        //                book.quantity += transaction.quantity;
        //            }

        //            // Remove each transaction
        //            library.transactionBorrow.Remove(transaction);
        //        }

        //        // Save all changes in one operation
        //        library.SaveChanges();
        //    }

        //    return RedirectToAction("Index");
        //}


        public ActionResult PrintTransaction(int id)
        {
            var transaction = library.transactionBorrow
                .Where(t => t.idBorrow == id)
                .ToList();

            if (transaction == null || !transaction.Any())
            {
                return HttpNotFound();
            }

            using (var ms = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Header
                document.Add(new Paragraph("Library Name", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20)) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("Library Address", FontFactory.GetFont(FontFactory.HELVETICA, 12)) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("Contact: 123-456-7890", FontFactory.GetFont(FontFactory.HELVETICA, 12)) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("\n")); // Adding space

                // Title
                document.Add(new Paragraph("Transaction Invoice", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24)) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("\n")); // Adding space

                // General Information
                var firstRecord = transaction.First();
                PdfPTable generalInfoTable = new PdfPTable(2);
                generalInfoTable.WidthPercentage = 100;
                generalInfoTable.SetWidths(new float[] { 1f, 2f });

                generalInfoTable.AddCell(new Phrase("Borrow ID:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                generalInfoTable.AddCell(firstRecord.idBorrow.ToString());
                generalInfoTable.AddCell(new Phrase("Reader Name:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                generalInfoTable.AddCell(firstRecord.reader.fullname);
                generalInfoTable.AddCell(new Phrase("Staff Name:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                generalInfoTable.AddCell(firstRecord.staff.nameStaff);
                generalInfoTable.AddCell(new Phrase("Borrow Date:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                generalInfoTable.AddCell(firstRecord.borrowDate.ToString());
                generalInfoTable.AddCell(new Phrase("Due Date:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                generalInfoTable.AddCell(firstRecord.dueDate.ToString());

                document.Add(generalInfoTable); // Add the table to the document
                document.Add(new Paragraph("\n")); // Adding space

                // Create a table for book details
                PdfPTable bookTable = new PdfPTable(4); // 4 columns
                bookTable.WidthPercentage = 100; // Set width to 100% of page
                bookTable.SetWidths(new float[] { 2f, 1f, 1f, 2f }); // Adjust column widths

                // Table headers
                bookTable.AddCell(new Phrase("Book Title", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                bookTable.AddCell(new Phrase("Price", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                bookTable.AddCell(new Phrase("Quantity", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                bookTable.AddCell(new Phrase("Note", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));

                // Total price calculation
                decimal totalPrice = 0;

                // Populate the table with transaction items
                foreach (var item in transaction)
                {
                    // Use null-coalescing operator to handle nullable decimals
                    decimal price = item.price ?? 0;  // Default to 0 if price is null
                    int quantity = item.quantity ?? 0; // Default to 0 if quantity is null

                    bookTable.AddCell(item.book.titleBook);
                    bookTable.AddCell(price.ToString("C")); // Format as currency
                    bookTable.AddCell(quantity.ToString());
                    bookTable.AddCell(item.note);

                    // Calculate total price for each item
                    totalPrice += price * quantity;
                }

                document.Add(bookTable); // Add the book table to the document

                // Add Total Price to the document
                document.Add(new Paragraph("\n")); // Adding space
                document.Add(new Paragraph($"Total Price: {totalPrice:C}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)) { Alignment = Element.ALIGN_RIGHT });

                // Footer with payment instructions
                document.Add(new Paragraph("\nThank you for your business!", new Font(Font.FontFamily.HELVETICA, 12, Font.ITALIC)) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("Please make sure to return any borrowed books on time.", new Font(Font.FontFamily.HELVETICA, 12, Font.ITALIC)) { Alignment = Element.ALIGN_CENTER });

                // Closing the document
                document.Close();

                return File(ms.ToArray(), "application/pdf", $"Transaction_{id}.pdf");
            }
        }





        public ActionResult DeleteAllByBorrowId(int idBorrow)
        {
            // Get all transactions related to the idBorrow
            var transactions = library.transactionBorrow
                .Where(t => t.idBorrow == idBorrow)
                .ToList();

            // Initialize fine amount
            decimal fineAmount = 0;

            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    // Find the book related to the transaction
                    var book = library.book.Find(transaction.fk_idBook);
                    if (book != null)
                    {
                        // Restore the quantity back to the book
                        book.quantity += transaction.quantity;
                    }

                    // Calculate overdue days
                    var currentDate = DateTime.Now; // Current date

                    // Ensure that dueDate is not null before calculating
                    if (transaction.dueDate.HasValue) // Check if dueDate is not null
                    {
                        var dueDate = transaction.dueDate.Value; // Get the non-nullable DateTime
                        int overdueDays = (int)(currentDate - dueDate).TotalDays; // Calculate overdue days

                        // Determine the fine amount based on overdue days
                        if (overdueDays > 0) // If overdue
                        {
                            // Reset fineAmount for this transaction
                            fineAmount = 0;
                            fineAmount = (int)overdueDays * 10; // Calculate fine as 10 per overdue day

                            // Log the computed values for debugging
                            Console.WriteLine($"Calculated Fine Amount for Overdue Days: {overdueDays}, Fine Amount: {fineAmount}");
                        }


                        // Create a new transactionReturn record
                        var transactionReturn = new transactionReturn
                        {
                            fk_transactionID = transaction.transactionID,
                            overDueDate = currentDate, // Set to current date
                            overDueNumber = (int)overdueDays, // Total days overdue (cast to int)
                            fineAmount = fineAmount, // Total fine amount
                        };

                        // Add transactionReturn to the context
                        library.transactionReturn.Add(transactionReturn);
                    }

                    // Update the note field to 'đã trả sách' instead of removing the transaction
                    transaction.note = "Đã trả sách";
                }

                // Save all changes in one operation
                library.SaveChanges();
            }

            return RedirectToAction("Index");
        }



    }
}