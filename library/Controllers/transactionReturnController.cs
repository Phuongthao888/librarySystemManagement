using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

namespace library.Controllers
{
    public class transactionReturnController : Controller
    {
        librarySystemEntities library = new librarySystemEntities();
        public ActionResult Index()
        {
            var transReturn = library.transactionReturn
                .Include(t => t.transactionBorrow) 
                .ToList();
            return View(transReturn);
        }

        public ActionResult Details(int id)
        {
            var transReturn = library.transactionReturn
               .Include(t => t.transactionBorrow)
                .FirstOrDefault(b => b.idReturn == id);

            if (transReturn == null)
            {
                return HttpNotFound(); // Return 404 if the book is not found
            }

            return View(transReturn); // Pass the single book object to the view
        }

        public ActionResult PrintTransactionReturn(int id)
        {
            // Fetch the transaction return record using the provided id
            var transactionReturn = library.transactionReturn
                .Include(t => t.transactionBorrow) // Include related transactionBorrow for details
                .FirstOrDefault(t => t.idReturn == id);

            if (transactionReturn == null)
            {
                return HttpNotFound();
            }

            using (var ms = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Header
                document.Add(new Paragraph("Library PT", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20)) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("Contact: 123-456-7890", FontFactory.GetFont(FontFactory.HELVETICA, 12)) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("\n")); // Adding space

                // Title
                document.Add(new Paragraph("Transaction Return Invoice", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24)) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("\n")); // Adding space

                // Create a table for the general information
                PdfPTable generalInfoTable = new PdfPTable(2);
                generalInfoTable.WidthPercentage = 100;
                generalInfoTable.SetWidths(new float[] { 1f, 2f });

                // General Information
                generalInfoTable.AddCell(new Phrase("Return ID:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                generalInfoTable.AddCell(transactionReturn.idReturn.ToString());
                generalInfoTable.AddCell(new Phrase("Overdue Date:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                generalInfoTable.AddCell(transactionReturn.overDueDate?.ToString("dd/MM/yyyy") ?? "N/A");
                generalInfoTable.AddCell(new Phrase("Overdue Number:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                generalInfoTable.AddCell((transactionReturn.overDueNumber ?? 0).ToString());
                generalInfoTable.AddCell(new Phrase("Fine Amount:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                generalInfoTable.AddCell(transactionReturn.fineAmount.Value.ToString("C"));

                document.Add(generalInfoTable); // Add the table to the document
                document.Add(new Paragraph("\n")); // Adding space

                // If there are associated transactionBorrow details
                if (transactionReturn.transactionBorrow != null)
                {
                    var borrow = transactionReturn.transactionBorrow;
                    document.Add(new Paragraph("Borrower Information", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18)));

                    // Create a table for the borrower information
                    PdfPTable borrowerInfoTable = new PdfPTable(2);
                    borrowerInfoTable.WidthPercentage = 100;
                    borrowerInfoTable.SetWidths(new float[] { 1f, 2f });

                    borrowerInfoTable.AddCell(new Phrase("Borrow ID:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    borrowerInfoTable.AddCell(borrow.idBorrow.ToString());
                    borrowerInfoTable.AddCell(new Phrase("Reader Name:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    borrowerInfoTable.AddCell(borrow.reader.fullname);
                    borrowerInfoTable.AddCell(new Phrase("Staff Name:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    borrowerInfoTable.AddCell(borrow.staff.nameStaff);
                    borrowerInfoTable.AddCell(new Phrase("Borrow Date:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    borrowerInfoTable.AddCell(borrow.borrowDate.ToString());
                    borrowerInfoTable.AddCell(new Phrase("Due Date:", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    borrowerInfoTable.AddCell(borrow.dueDate.ToString());

                    document.Add(borrowerInfoTable); // Add the borrower table to the document
                    document.Add(new Paragraph("\n")); // Adding space
                }

                // Footer with payment instructions
                document.Add(new Paragraph("Thank you for your business!", new Font(Font.FontFamily.HELVETICA, 12, Font.ITALIC)) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("Please make sure to settle any outstanding fines promptly.", new Font(Font.FontFamily.HELVETICA, 12, Font.ITALIC)) { Alignment = Element.ALIGN_CENTER });

                // Closing the document
                document.Close();

                return File(ms.ToArray(), "application/pdf", $"TransactionReturn_{id}.pdf");
            }
        }



    }
}