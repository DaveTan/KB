using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace KB.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Accounts()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Login");
            }
            if (!checkAdmin())
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Accounts(FormCollection input)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Login");
            }
            if (!checkAdmin())
            {
                return RedirectToAction("Index");
            }
            if (!string.IsNullOrEmpty(input["Register"]))
            {
                if (input["username"] != "" & input["password"] != "" & (input["password"] == input["password2"]) & input["email"] != "")
                {
                    using (var ctx = new KBEntities())
                    {
                        ctx.sp_AddAccount(input["username"], input["password"], input["email"]);
                    }
                }
            }
            else
            {
                using (var ctx = new KBEntities())
                {
                    ctx.sp_EditAccount(Convert.ToInt32(input["Id"]), input["Username"], input["Password"], input["Email"], Convert.ToInt32(input["Role"]));
                }
            }
            return View();
        }
        public JsonResult SearchAccount(int offset, int limit, string search, string sort, string order)
        {
            if (search == null)
            {
                search = "";
            }
            using (var ctx = new KBEntities())
            {
                List<sp_GetAccounts_Result> searchResult = new List<sp_GetAccounts_Result>();
                var courses = ctx.sp_GetAccounts(search);
                foreach (var c in courses)
                {
                    searchResult.Add(c);
                }
                var model = new
                {
                    total = searchResult.Count(),
                    rows = searchResult.Skip((offset / limit) * limit).Take(limit),
                };
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DeleteAccount(int id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Login");
            }
            if (!checkAdmin())
            {
                return RedirectToAction("Index");
            }
            using (var ctx = new KBEntities())
            {
                ctx.sp_DeleteAccount(id);
            }
            return RedirectToAction("Accounts");
        }
        public ActionResult Categories()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Login");
            }
            if (!checkAdmin())
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [HttpPost]
        public ActionResult Categories(FormCollection input)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Login");
            }
            if (!checkAdmin())
            {
                return RedirectToAction("Index");
            }
            if (!string.IsNullOrEmpty(input["Category"]))
            {
                using (var ctx = new KBEntities())
                {
                    ctx.sp_AddCategory(input["CatName"]);
                }
            }
            else
            {
                using (var ctx = new KBEntities())
                {
                    ctx.sp_EditCategory(Convert.ToInt32(input["Id"]), input["Name"]);
                }
            }
            return View();
        }
        public JsonResult SearchCategory(int offset, int limit, string search, string sort, string order)
        {
            if (search == null)
            {
                search = "";
            }
            using (var ctx = new KBEntities())
            {
                List<sp_GetCategories_Result> searchResult = new List<sp_GetCategories_Result>();
                var courses = ctx.sp_GetCategories(search);
                foreach (var c in courses)
                {
                    searchResult.Add(c);
                }
                var model = new
                {
                    total = searchResult.Count(),
                    rows = searchResult.Skip((offset / limit) * limit).Take(limit),
                };
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DeleteCategory(int id)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Login");
            }
            if (!checkAdmin())
            {
                return RedirectToAction("Index");
            }
            using (var ctx = new KBEntities())
            {
                ctx.sp_DeleteCategory(id);
            }
            return RedirectToAction("Categories");
        }
        public ActionResult Login()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoServerCaching();
            Response.Cache.SetNoStore();

            Session["Id"] = null;
            Session["Role"] = null;
            Session["Name"] = null;
            return View();
        }
        public Boolean checkLogin()
        {
            if (Session["Id"] == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public Boolean checkAdmin()
        {
            if (Convert.ToInt32(Session["Role"]) == 18)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        [HttpPost]
        public ActionResult Login(FormCollection input)
        {
            loadData();
            if (!string.IsNullOrEmpty(input["Register"]))
            {
                if (input["username"] != "" & input["password"] != "" & (input["password"] == input["password2"]) & input["email"]!="")
                {
                    using (var ctx = new KBEntities())
                    {
                        ctx.sp_AddAccount(input["username"], input["password"], input["email"]);
                    }
                }
            }
            if (!string.IsNullOrEmpty(input["Login"]))
            {
                using (var ctx = new KBEntities())
                {
                    var result = ctx.sp_ValidateLogin(input["username"], input["password"]);
                    foreach (sp_ValidateLogin_Result c in result)
                    {
                        Session["Id"] = c.Id;
                        Session["Role"] = c.RoleId;
                        Session["Name"] = c.Username;
                        return Redirect("Index");
                    }
                }
            }
            return View();
        }
        public FileResult Download(String fileDownloadPath)
        {
            string file = "../"+fileDownloadPath;
            var retval = File(file, Path.GetFileName(file));
            retval.FileDownloadName = Path.GetFileName(retval.FileName);
            return retval;
        }
        public void loadData()
        {
            ViewBag.CategoryHTML = "";
            ViewBag.AccountHTML = "";
            String temp = "";
            String Datalist = "";
            String AccountHTML = "";
            using (var ctx = new KBEntities())
            {
                var topics = ctx.sp_GetCategories("");
                foreach (sp_GetCategories_Result c in topics)
                {
                    temp = temp + "<option value='" + c.Id + "'>" + c.Name + "</option>";
                    Datalist = Datalist + "<option value='" + c.Name + "'>";
                }
            }
            ViewBag.AccountHTML = AccountHTML;
            ViewBag.Datalist = Datalist;
            ViewBag.CategoryHTML = temp;
        }
        public ActionResult Index()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Login");
            }
            loadData();
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Index(IEnumerable<HttpPostedFileBase> fileInput, FormCollection input)
        {
            if (!checkLogin())
            {
                return RedirectToAction("Login");
            }
            if (!string.IsNullOrEmpty(input["Download"]))
            {
                return RedirectToAction("Download", new { fileDownloadPath = input["Download"] });
            }
            if (!string.IsNullOrEmpty(input["Edit"]))
            {
                using (var ctx = new KBEntities())
                {
                    ctx.sp_UpdateKb(Convert.ToInt32(input["Id"]), input["Title2"], input["content2"], Convert.ToInt32(input["Category2"]));
                }
                if (fileInput != null)
                {
                    foreach (var file in fileInput)
                    {
                        if (file != null)
                        {
                            if (file.ContentLength > 0)
                            {
                                var fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                                file.SaveAs(path);
                                using (var ctx = new KBEntities())
                                {
                                    ctx.sp_AddAttachment("/App_Data/uploads/" + fileName, Convert.ToInt32(input["Id"]));
                                }
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(input["Delete"]))
            {
                using(var ctx = new KBEntities())
                {
                    ctx.sp_DeleteAccount(Convert.ToInt32(input["Id"]));
                }
            }
            if (!string.IsNullOrEmpty(input["Add"]))
            {
                int lastId = 0;
                if (!string.IsNullOrEmpty(input["Title"]))
                {
                    using (var ctx = new KBEntities())
                    {
                    var Id = Session["Id"];
                    var k = Id;
                    var result = ctx.sp_AddKb(input["Title"], input["content"], Convert.ToInt32(input["Category"]), Convert.ToInt32(Id));
                    lastId = Convert.ToInt32(result.ElementAt(0));
                    }
                }
                if (fileInput != null)
                {
                    foreach (var file in fileInput)
                    {
                        if (file != null)
                        {
                            if (file.ContentLength > 0)
                            {
                                var fileName = Path.GetFileName(file.FileName);
                                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                                file.SaveAs(path);
                                using (var ctx = new KBEntities())
                                {
                                    ctx.sp_AddAttachment("/App_Data/uploads/" + fileName, lastId);
                                }
                            }
                        }
                    }
                }
            }
            loadData();
            return View("Index");
        }
        [HttpGet]
        public JsonResult GetRecord(int id)
        {
            Session["currId"] = id;
            using (var ctx = new KBEntities())
            {
                var topics = ctx.sp_GetKbDetail(id).ToList();
                var retval = Json(topics, JsonRequestBehavior.AllowGet);
                retval.MaxJsonLength = int.MaxValue;
                return retval;
            }
        }
        public ActionResult Delete()
        {
            if (!checkLogin())
            {
                return RedirectToAction("Login");
            }
            var temp = Session["currId"];
            using (var ctx = new KBEntities())
            {
                ctx.sp_DeleteKb(Convert.ToInt32(temp));
            }
            return Redirect("../Index");
        }
        public JsonResult SearchRecord(int offset, int limit, string search, string sort, string order)
        {
            if (search == null)
            {
                search="";
            }
            using (var ctx = new KBEntities())
            {
                List<sp_GetSideBarTopics_Result> searchResult = new List<sp_GetSideBarTopics_Result>();
                var courses = ctx.sp_GetSideBarTopics(search);
                foreach (sp_GetSideBarTopics_Result c in courses)
                {
                    c.Desc = ScrubHtml(c.Desc);
                    searchResult.Add(c);
                }
                var model = new
                {
                    total = searchResult.Count(),
                    rows = searchResult.Skip((offset / limit) * limit).Take(limit),
                };
                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }
        public static string ScrubHtml(string value)
        {
            var step1 = System.Text.RegularExpressions.Regex.Replace(value, @"<[^>]+>|&nbsp;", "").Trim();
            var step2 = System.Text.RegularExpressions.Regex.Replace(step1, @"\s{2,}", " ");
            var step3 = step2.Substring(0, Math.Min(50, step2.Length));
            return step3;
        }
    }
}