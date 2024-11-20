using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace library.Controllers
{
    public class ReaderController : Controller
    {
        librarySystemEntities library = new librarySystemEntities();
        // GET: Reader
        public ActionResult Index(FormCollection searchdata)
        {
            var readers = library.reader.ToList();
            if (!string.IsNullOrEmpty(searchdata["Search"]))
            {
                string searchKeyword = searchdata["Search"].ToLower();
                readers = readers.Where(x => x.fullname.ToLower().Contains(searchKeyword)).ToList();
            }
            return View(readers);
        }

        public ActionResult Details(int id)
        {
            var reader = library.reader.Find(id); // Assuming db is your database context
            if (reader == null)
            {
                return HttpNotFound();
            }
            return View(reader); // Pass the single reader object to the view
        }

        // GET: Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(reader newReader)
        {
            if (ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                // Thêm đối tượng reader mới vào DbSet và lưu vào cơ sở dữ liệu
                library.reader.Add(newReader);
                library.SaveChanges();

                return RedirectToAction("Details", new { id = newReader.idReader }); // Chuyển hướng về trang chi tiết của reader mới tạo
            }

            return View(newReader); // Nếu model không hợp lệ, trả về view để tạo lại
        }


        // GET: Edit
        public ActionResult Edit(int id)
        {
            var reader = library.reader.Find(id); // Tìm kiếm reader theo id
            if (reader == null)
            {
                return HttpNotFound();
            }
            return View(reader); // Trả về view để chỉnh sửa thông tin của reader
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(reader updatedReader)
        {
            if (ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                var existingReader = library.reader.Find(updatedReader.idReader); // Tìm đối tượng reader hiện tại
                if (existingReader == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật các thuộc tính của đối tượng reader
                existingReader.firstName = updatedReader.firstName;
                existingReader.lastName = updatedReader.lastName;
                existingReader.dob = updatedReader.dob;
                existingReader.contactAddress = updatedReader.contactAddress;
                existingReader.gender = updatedReader.gender;
                existingReader.phoneNumber = updatedReader.phoneNumber;
                existingReader.email = updatedReader.email;
                existingReader.userType = updatedReader.userType;

                library.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("Details", new { id = existingReader.idReader }); // Chuyển hướng về trang chi tiết
            }

            return View(updatedReader); // Nếu model không hợp lệ, trả về view để chỉnh sửa lại
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
            var reader = library.reader.Find(id);
            if (reader != null)
            {
                library.reader.Remove(reader);
                library.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult PrintReader(int id)
        {
            var reader = library.reader
                .Where(r => r.idReader == id)
                .FirstOrDefault(); // Get the first matching reader

            if (reader == null)
            {
                return HttpNotFound();
            }

            using (var ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A6, 20f, 20f, 20f, 20f); // A6 size for library card
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Header with Library Information
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(100, 150, 200)); // Light blue-gray
                var subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, new BaseColor(120, 120, 120)); // Light gray

                document.Add(new Paragraph("Library PT", headerFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("Library Address: 123 abc, TP.HCM", subHeaderFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("Contact: 123-456-7890", subHeaderFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("\n")); // Adding space

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, new BaseColor(150, 150, 150)); // Soft gray
                document.Add(new Paragraph("Library Card", titleFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("\n")); // Adding space

                // General Information Table
                PdfPTable generalInfoTable = new PdfPTable(2);
                generalInfoTable.WidthPercentage = 100;
                generalInfoTable.SetWidths(new float[] { 1f, 2f });

                // Adding headers with light pastel color backgrounds
                var headerCellFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                var infoFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);

                PdfPCell headerCell = new PdfPCell { BackgroundColor = new BaseColor(180, 200, 210) }; // Light pastel blue
                headerCell.Phrase = new Phrase("Reader ID:", headerCellFont);
                generalInfoTable.AddCell(headerCell);
                generalInfoTable.AddCell(new Phrase(reader.idReader.ToString(), infoFont));

                headerCell.Phrase = new Phrase("Full Name:", headerCellFont);
                generalInfoTable.AddCell(headerCell);
                generalInfoTable.AddCell(new Phrase(reader.fullname, infoFont));

                headerCell.Phrase = new Phrase("Date of Birth:", headerCellFont);
                generalInfoTable.AddCell(headerCell);
                generalInfoTable.AddCell(new Phrase(reader.dob.HasValue ? reader.dob.Value.ToString("dd/MM/yyyy") : "n/a", infoFont));

                headerCell.Phrase = new Phrase("Phone Number:", headerCellFont);
                generalInfoTable.AddCell(headerCell);
                generalInfoTable.AddCell(new Phrase(reader.phoneNumber, infoFont));

                document.Add(generalInfoTable); // Add the table to the document
                document.Add(new Paragraph("\n")); // Adding space

                // Additional Information with light pastel text color
                var additionalInfoFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, new BaseColor(120, 120, 180)); // Light blue-gray
                document.Add(new Paragraph("Email: " + reader.email, additionalInfoFont));
                document.Add(new Paragraph("User Type: " + reader.userType, additionalInfoFont));
                document.Add(new Paragraph("\n")); // Adding space

                // Footer with additional information in soft gray italic font
                var footerFont = new Font(Font.FontFamily.HELVETICA, 10, Font.ITALIC, new BaseColor(100, 100, 100)); // Soft gray
                document.Add(new Paragraph("Thank you for being a valued member of our library!", footerFont) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph("Please reach out to us for any inquiries.", footerFont) { Alignment = Element.ALIGN_CENTER });

                // Closing the document
                document.Close();

                return File(ms.ToArray(), "application/pdf", $"Reader_{id}.pdf");
            }
        }



    }
}