using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using library;
using System.Data;


namespace librarySystem.Controllers
{
    public class InventoriesController : Controller
    {
        librarySystemEntities library = new librarySystemEntities();
        public ActionResult Index(FormCollection searchdata)
        {
            var inventories = library.inventory
                .Include(i => i.book)
                .ToList();
            if (!string.IsNullOrEmpty(searchdata["Search"]))
            {
                string searchKeyword = searchdata["Search"].ToLower();
                inventories = inventories.Where(x => x.book.titleBook.ToLower().Contains(searchKeyword)).ToList();
            }

            return View(inventories);
        }

        public ActionResult Details(int id)
        {
            var inventories = library.inventory
                .Include(i => i.book)
                .FirstOrDefault(i => i.idInventory == id);

            if (inventories == null)
            {
                return HttpNotFound(); // Return 404 if the book is not found
            }

            return View(inventories); // Pass the single book object to the view
        }

        // GET: Create
        public ActionResult Create()
        {
            var books = library.book.ToList();
            ViewBag.fk_idBook = new SelectList(books, "idBook", "titleBook");
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(inventory newInventory)
        {
            if (ModelState.IsValid)
            {
                library.inventory.Add(newInventory);
                library.SaveChanges();
                return RedirectToAction("Details", new { id = newInventory.idInventory });
            }

            var books = library.book.ToList();
            ViewBag.fk_idBook = new SelectList(books, "idBook", "titleBook", newInventory.fk_idBook);
            return View(newInventory);
        }
        // GET: Edit
        public ActionResult Edit(int id)
        {
            // Tìm inventory cần chỉnh sửa theo ID
            var inventory = library.inventory
                .Include(i => i.book)
                .FirstOrDefault(i => i.idInventory == id);

            if (inventory == null)
            {
                return HttpNotFound(); // Trả về 404 nếu không tìm thấy inventory
            }

            // Lấy danh sách các sách để hiển thị trong dropdown
            var books = library.book.ToList();
            ViewBag.fk_idBook = new SelectList(books, "idBook", "titleBook", inventory.fk_idBook);

            return View(inventory);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(inventory updatedInventory)
        {
            if (ModelState.IsValid)
            {
                // Cập nhật thông tin inventory
                library.Entry(updatedInventory).State = EntityState.Modified;
                library.SaveChanges();

                return RedirectToAction("Details", new { id = updatedInventory.idInventory });
            }

            // Nếu không hợp lệ, hiển thị lại form cùng với dữ liệu cũ
            var books = library.book.ToList();
            ViewBag.fk_idBook = new SelectList(books, "idBook", "titleBook", updatedInventory.fk_idBook);

            return View(updatedInventory);
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
            var inventory = library.inventory.Find(id);
            if (inventory != null)
            {
                library.inventory.Remove(inventory);
                library.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}