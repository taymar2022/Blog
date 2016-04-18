using FirstTest_MVC.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Blog.Controllers
    
{
    [Authorize]
    [RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        //    public ActionResult Abc()
        //    {
        //        ViewBag.Pqr = "This is the Abc page!!!";
        //        return View();

        //            }
        //    public ActionResult Cup()
        //    {
        //        ViewBag.Sheetz = "My coffee cup!";
        //        return View();

        //    }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
  
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<ActionResult> Contact([Bind(Include = "Id,Name,Email,Message,Phone,MessageSent")] Contact contact)
{
    contact.MessageSent = DateTime.Now;

    var svc = new Blog.EmailService();
    var msg = new IdentityMessage();
    msg.Subject = "Contact From Web Site";
    msg.Body = contact.Message;
    await svc.SendAsync(msg);

    return View(contact);
}
                }
        }

