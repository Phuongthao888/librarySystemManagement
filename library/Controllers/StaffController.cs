using library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace librarySystem.Controllers
{
    public class StaffController : Controller
    {
        librarySystemEntities library = new librarySystemEntities();

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string userName, string pass)
        {
            // Check if the credentials match any staff in the database
            var staffMember = library.staff.FirstOrDefault(s => s.userName == userName && s.pass == pass);

            if (staffMember != null)
            {
                // Create a session or authentication token here
                Session["StaffUser"] = staffMember; // Example: Store staff member in session
                return RedirectToAction("Index", "Dashboard"); // Redirect to the Index action of Dashboard after successful login
            }

            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng."); // Invalid credentials message
            return View(); // Return to the login view if login fails
        }


        // GET: Logout
        public ActionResult Logout()
        {
            Session.Abandon(); // Clear the session
            return RedirectToAction("Login"); // Redirect to the login page
        }


        public ActionResult Index(FormCollection searchdata)
        {
            var staff = library.staff
                .OrderByDescending(s => s.role == "Admin") // Đưa role "Admin" lên đầu
                .ThenBy(s => s.nameStaff) // Sắp xếp theo tên nhân viên nếu có nhiều "Admin"
                .ToList();
            if (!string.IsNullOrEmpty(searchdata["Search"]))
            {
                string searchKeyword = searchdata["Search"].ToLower();
                staff = staff.Where(x => x.nameStaff.ToLower().Contains(searchKeyword)).ToList();
            }
            return View(staff);
        }


        public ActionResult Details(int id)
        {
            var staff = library.staff.Find(id); // Assuming db is your database context
            if (staff == null)
            {
                return HttpNotFound();
            }
            return View(staff);
        }

        // GET: Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(staff newStaff)
        {
            // Check if there's already an Admin in the database
            var existingAdmin = library.staff.FirstOrDefault(s => s.role == "Admin");

            // Check if the new role contains "Admin"
            if (newStaff.role.Contains("Admin") && existingAdmin != null)
            {
                // If an Admin already exists, add a model error
                ModelState.AddModelError("role", "Admin chỉ có 1 tài khoản duy nhất, không được tạo thêm.");

                // Return the view with the validation message
                return View(newStaff);
            }

            if (ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                library.staff.Add(newStaff);
                library.SaveChanges();

                return RedirectToAction("Details", new { id = newStaff.idStaff }); // Chuyển hướng về trang chi tiết 
            }

            return View(newStaff); // Nếu model không hợp lệ, trả về view để tạo lại
        }


        // GET: Edit
        public ActionResult Edit(int id)
        {
            var staff = library.staff.Find(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            return View(staff);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(staff updatedStaff)
        {
            if (ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                var existingStaff = library.staff.Find(updatedStaff.idStaff);
                if (existingStaff == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra nếu người dùng cố gắng chỉnh sửa role thành "Admin"
                if (updatedStaff.role.Contains("Admin"))
                {
                    var existingAdmin = library.staff.FirstOrDefault(s => s.role == "Admin" && s.idStaff != updatedStaff.idStaff);

                    if (existingAdmin != null)
                    {
                        // Nếu đã tồn tại tài khoản Admin khác, hiển thị thông báo lỗi
                        ModelState.AddModelError("role", "Admin chỉ có duy nhất 1 tài khoản.");
                        return View(updatedStaff);
                    }
                }

                // Cập nhật các thuộc tính của đối tượng 
                existingStaff.nameStaff = updatedStaff.nameStaff;
                existingStaff.userName = updatedStaff.userName;
                existingStaff.pass = updatedStaff.pass;
                existingStaff.role = updatedStaff.role;

                library.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                return RedirectToAction("Details", new { id = existingStaff.idStaff }); // Chuyển hướng về trang chi tiết
            }

            return View(updatedStaff); // Nếu model không hợp lệ, trả về view để chỉnh sửa lại
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
            var staff = library.staff.Find(id);
            if (staff != null)
            {
                if (staff.role == "Admin")
                {
                    // Hiển thị thông báo lỗi nếu là Admin
                    TempData["ErrorMessage"] = "Tài khoản Admin không thể xóa";
                    return RedirectToAction("Index");
                }

                library.staff.Remove(staff);
                library.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}