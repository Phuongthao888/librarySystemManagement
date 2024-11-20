using library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace librarySystem.Controllers
{
    public class CategoryController : Controller
    {
        librarySystemEntities library = new librarySystemEntities();
        // GET: Category
        public ActionResult Index(FormCollection searchdata)
        {
            var category = library.category.ToList();
            if (!string.IsNullOrEmpty(searchdata["Search"]))
            {
                string searchKeyword = searchdata["Search"].ToLower();
                category = category.Where(x => x.nameCategory.ToLower().Contains(searchKeyword)).ToList();
            }
            return View(category);
        }

        public ActionResult Details(int id)
        {
            var category = library.category.Find(id); // Assuming db is your database context
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category); // Pass the single reader object to the view
        }

        // GET: Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(category newCategory)
        {
            if (ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                library.category.Add(newCategory);
                library.SaveChanges();

                return RedirectToAction("Details", new { id = newCategory.idCategory }); // Chuyển hướng về trang chi tiết của reader mới tạo
            }

            return View(newCategory); // Nếu model không hợp lệ, trả về view để tạo lại
        }

        // GET: Edit
        public ActionResult Edit(int id)
        {
            var category = library.category.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(category updatedCategory)
        {
            if (ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                var existingCategogy = library.category.Find(updatedCategory.idCategory);
                if (existingCategogy == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật các thuộc tính của đối tượng reader
                existingCategogy.nameCategory = updatedCategory.nameCategory;
                existingCategogy.ageLimit = updatedCategory.ageLimit;

                library.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("Details", new { id = existingCategogy.idCategory }); // Chuyển hướng về trang chi tiết
            }

            return View(updatedCategory); // Nếu model không hợp lệ, trả về view để chỉnh sửa lại
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
            var category = library.category.Find(id);
            if (category != null)
            {
                library.category.Remove(category);
                library.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}