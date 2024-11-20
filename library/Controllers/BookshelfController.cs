using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace library.Controllers
{
    public class BookshelfController : Controller
    {
        librarySystemEntities library = new librarySystemEntities();
        // GET: Supplier
        public ActionResult Index(FormCollection searchdata)
        {
            var bookshelf = library.bookshelf.ToList();
            if (!string.IsNullOrEmpty(searchdata["Search"]))
            {
                string searchKeyword = searchdata["Search"].ToLower();
                bookshelf = bookshelf.Where(x => x.nameBookshelfRow.ToLower().Contains(searchKeyword)).ToList();
            }
            return View(bookshelf);
        }
        public ActionResult Details(int id)
        {
            var bookshelf = library.bookshelf.Find(id); 
            if (bookshelf == null)
            {
                return HttpNotFound();
            }
            return View(bookshelf); 
        }
        // GET: Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(bookshelf newbookshelf)
        {
            if (ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                library.bookshelf.Add(newbookshelf);
                library.SaveChanges();

                return RedirectToAction("Details", new { id = newbookshelf.idBookshelf }); 
            }

            return View(newbookshelf); // Nếu model không hợp lệ, trả về view để tạo lại
        }
        // GET: Edit
        public ActionResult Edit(int id)
        {
            var bookshelf = library.bookshelf.Find(id);
            if (bookshelf == null)
            {
                return HttpNotFound();
            }
            return View(bookshelf);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(bookshelf updatedbookshelf)
        {
            if (ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                var existingBookshelf = library.bookshelf.Find(updatedbookshelf.idBookshelf);
                if (existingBookshelf == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật các thuộc tính của đối tượng reader
                existingBookshelf.nameBookshelfRow = updatedbookshelf.nameBookshelfRow;

                library.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("Details", new { id = existingBookshelf.idBookshelf }); 
            }

            return View(updatedbookshelf); // Nếu model không hợp lệ, trả về view để chỉnh sửa lại
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
            var bookshelf = library.bookshelf.Find(id);
            if (bookshelf != null)
            {
                library.bookshelf.Remove(bookshelf);
                library.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}