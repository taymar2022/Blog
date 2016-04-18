using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Blog.Models;
using BlogSpring16.Models;
using PagedList;
using PagedList.Mvc;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.AspNet.Identity;

namespace Blog.Controllers
{


}
//[RequireHttps]
public class BlogPostsController : Controller
{
    private ApplicationDbContext db = new ApplicationDbContext();

    public static class ImageUpLoadValidator
    {
        public static bool IsWebFriendlyImage(HttpPostedFileBase file)
        {
            //check for actual object
            if (file == null)
                return false;
            // check size - file must be less than 2 MB and greater than 1 KB
            if (file.ContentLength > 2 * 1024 || file.ContentLength < 1024)
                return false;

            try
            {
                using (var img = Image.FromStream(file.InputStream))
                {
                    return ImageFormat.Jpeg.Equals(img.RawFormat) ||
                           ImageFormat.Png.Equals(img.RawFormat) ||
                           ImageFormat.Gif.Equals(img.RawFormat);
                }
            }
            catch
            {
                return false;
            }


        }
    }



    // GET: BlogPosts
    public ActionResult Index(int? page, string query)
    {
        int pageSize = 5;//the number of posts you want to display per page
        int pageNumber = (page ?? 1);
        ViewBag.Query = query;
        var qposts = db.Posts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))

        { qposts = qposts.Where(p => p.Title.Contains(query) || p.Body.Contains(query) || p.Comments.Any(c => c.Body.Contains(query) || c.Author.DisplayName.Contains(query))); }

        var posts = qposts.OrderByDescending(p => p.Created).ToPagedList(pageNumber, pageSize);
        //return View(db.Posts.OrderByDescending(p => p.Created).ToPagedList(pageNumber, pageSize));
        //}
        
        return View(posts);
    }
    [RequireHttps]
    [Authorize(Roles = "Admin")]
    public ActionResult Admin()
    {
        return View();
    }

    // GET: BlogPosts/Details/5
    public ActionResult Details(string slug)
    {
        if (String.IsNullOrWhiteSpace(slug))
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        BlogPost blogPost = db.Posts.FirstOrDefault(p => p.Slug == slug);
        if (blogPost == null)
        {
            return HttpNotFound();
        }
        return View(blogPost);
    }

    public ActionResult Contact()
    {
        ViewBag.Message = "https://localhost:44300/";
        return View();
    }

    // GET: BlogPosts/Create
    //    [HttpPost]
    //[ValidateAntiForgeryToken]
    //[Authorize(Roles = "Admin")]  // Admin Users only

    //    public ActionResult Create(BlogPost BlogPost, HttpPostedFileBase image)
    //{
    //    BlogPost.Created = new DateTimeOffset(DateTime.Now);
    //    if (ModelState.IsValid)
    //    {
    //        // restricting the valid file formats to images only

    //        BlogPost.Created = new DateTimeOffset(DateTime.Now);
    //        DBConcurrencyException.Posts.Add(BlogPost);
    //        DBConcurrencyException.SaveChanges();
    //        return RedirectToAction("Index");
    //    }

    //}
    //if (BlogPost.MediaURL ! = null)
    //    {
    //    <img class="img-responsive" src="MediaUrl.Content(post.MediaUrl)" alt="">
    //    }

    // POST: BlogPosts/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public ActionResult Create([Bind(Include = "Id,Created,Updated,Title,Slug,Body,MediaUrl,Published")] BlogPost blogPost, HttpPostedFileBase image)
    {
        if (ModelState.IsValid)
        {
            var Slug = StringUtilities.URLFriendly(blogPost.Title);
            if (String.IsNullOrWhiteSpace(Slug))
            {
                ModelState.AddModelError("Title", "Invalid title.");
                return View(blogPost);
            }
            if (db.Posts.Any(p => p.Slug == Slug))
            {
                ModelState.AddModelError("Title", "The title must be unique.");
                return View(blogPost);
            }


            if (ImageUpLoadValidator.IsWebFriendlyImage(image))
            {
                var fileName = Path.GetFileName(image.FileName);
                image.SaveAs(Path.Combine(Server.MapPath("~/img/blog/"), fileName));
                blogPost.MediaUrl = "~/img/blog/" + fileName;
            }

            blogPost.Slug = Slug;
            blogPost.Created = System.DateTimeOffset.Now;
            db.Posts.Add(blogPost);
            db.SaveChanges();
        }
        return RedirectToAction("Index");

        //return View(blogPost);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public ActionResult CreateComment(Comment comment)
    {
        if (ModelState.IsValid)
        {
            comment.AuthorId = User.Identity.GetUserId();
            comment.Created = System.DateTimeOffset.Now;
        
            db.Comments.Add(comment);
            db.SaveChanges();
        }
        var post = db.Posts.Find(comment.PostId);
        return RedirectToAction("Details", new { slug = post.Slug });
       

    }

    // GET: BlogPosts/Edit/5
    public ActionResult Edit(int? id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        BlogPost blogPost = db.Posts.Find(id);
        if (blogPost == null)
        {
            return HttpNotFound();
        }
        return View(blogPost);
    }

    // POST: BlogPosts/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [Authorize (Roles="admin, Moderator")]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "Id,Created,Updated,Title,Slug,Body,MediaUrl,Published")] BlogPost blogPost)
    {
        if (ModelState.IsValid)
        {
            db.Entry(blogPost).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(blogPost);
    }
    [HttpPost]
    [Authorize(Roles = "Admin, Moderator")]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "Id,Created,Updated,Title,Slug,Body,MediaUrl,Published")] Comment Comment)
    {
        if (ModelState.IsValid)
        {
            db.Comments.Add(Comment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(Comment);
    }

    // GET: BlogPosts/Delete/5
    //public ActionResult Delete(int? id)
    //{
    //    if (id == null)
    //    {
    //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    //    }
    //    BlogPost blogPost = db.Posts.Find(id);
    //    if (blogPost == null)
    //    {
    //        return HttpNotFound();
    //    }
    //    return View(blogPost);
    //}

    // POST: BlogPosts/Delete/5
    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Admin, Moderator")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        BlogPost blogPost = db.Posts.Find(id);
        db.Posts.Remove(blogPost);
        db.SaveChanges();
        return RedirectToAction("Index");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            db.Dispose();
        }
        base.Dispose(disposing);
    }

}



