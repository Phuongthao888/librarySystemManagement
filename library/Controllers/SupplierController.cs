using library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace librarySystem.Controllers
{
    public class SupplierController : Controller
    {
        librarySystemEntities library = new librarySystemEntities();
        // GET: Supplier
        public ActionResult Index(FormCollection searchdata)
        {
            var supplier = library.supplier.ToList();
            if (!string.IsNullOrEmpty(searchdata["Search"]))
            {
                string searchKeyword = searchdata["Search"].ToLower();
                supplier = supplier.Where(x => x.nameSupplier.ToLower().Contains(searchKeyword)).ToList();
            }
            return View(supplier);
        }

        public ActionResult Details(int id)
        {
            var supplier = library.supplier.Find(id); // Assuming db is your database context
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier); // Pass the single reader object to the view
        }

        // GET: Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(supplier newSupplier)
        {
            if (ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                library.supplier.Add(newSupplier);
                library.SaveChanges();

                return RedirectToAction("Details", new { id = newSupplier.idSupplier }); // Chuyển hướng về trang chi tiết của reader mới tạo
            }

            return View(newSupplier); // Nếu model không hợp lệ, trả về view để tạo lại
        }

        // GET: Edit
        public ActionResult Edit(int id)
        {
            var supplier = library.supplier.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(supplier updatedSupplier)
        {
            if (ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                var existingSupplier = library.supplier.Find(updatedSupplier.idSupplier);
                if (existingSupplier == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật các thuộc tính của đối tượng reader
                existingSupplier.nameSupplier = updatedSupplier.nameSupplier;

                library.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("Details", new { id = existingSupplier.idSupplier }); // Chuyển hướng về trang chi tiết
            }

            return View(updatedSupplier); // Nếu model không hợp lệ, trả về view để chỉnh sửa lại
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
            var supplier = library.supplier.Find(id);
            if (supplier != null)
            {
                library.supplier.Remove(supplier);
                library.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}