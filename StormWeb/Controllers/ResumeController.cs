using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StormWeb.Models;
using StormWeb.Models.ModelHelper;
using StormWeb.Helper;
using System.IO;

namespace StormWeb.Controllers
{
    public class ResumeController : Controller
    {
        private StormDBEntities db = new StormDBEntities();
        DocumentController doc = new DocumentController();

        #region Listing
        public ViewResult Index()
        {
            return View(db.Resumes.ToList());
        }

        public static string GetFileName(int resumeId)
        {
            StormDBEntities db = new StormDBEntities();
            string fileName = "";
            var resume = (from res in db.Resumes
                          from rec in db.Resume_File
                          where res.Resume_Id == resumeId && rec.Resume_Id == res.Resume_Id
                          select rec);
            if (resume.Count() > 0)
                fileName = resume.FirstOrDefault().Path + '/' + resume.FirstOrDefault().FileName;

            return fileName;
        }
        public static int GetFileId(int resumeId)
        {
            StormDBEntities db = new StormDBEntities();
            int fileName = 0;
            var resume = (from res in db.Resumes
                          from rec in db.Resume_File
                          where res.Resume_Id == resumeId && rec.Resume_Id == res.Resume_Id
                          select rec);
            if (resume.Count() > 0)
                fileName = resume.FirstOrDefault().ResumeFile_Id;
            return fileName;
        }

        public void DownloadResume(int id)
        {
            Resume_File resume = db.Resume_File.SingleOrDefault(x => x.ResumeFile_Id == id);
            if (resume != null)
                doc.downloadAWS(resume.ResumeFile_Id, resume.Path, resume.FileName, this.HttpContext);
        }
        #endregion

        public ViewResult Details(int id)
        {
            Resume resume = db.Resumes.Single(r => r.Resume_Id == id);
            return View(resume);
        }

        #region Create

        public static string RESUME_UPLOADS_PATH = "Resume/";

        [Authorize(Roles = "Super,BranchManager,HR")]
        public ActionResult Create()
        {
            ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName");
            return View();
        }


        [HttpPost]
        public ActionResult Create(Resume resume, FormCollection fc, HttpPostedFileBase Resume_File)
        {
            db.Resumes.AddObject(resume);
            db.SaveChanges();
            Resume_File resumeFile = new Resume_File();
            if (Resume_File != null)
            {
                //string name = StormWeb.Helper.Utilities.getName(CookieHelper.Username);
                string name = resume.GivenName + " " + resume.LastName;
                string fileToCreate = RESUME_UPLOADS_PATH + resume.Resume_Id + "_" + name + '/' + Path.GetFileNameWithoutExtension(Resume_File.FileName) + "_Resume_" + resume.Resume_Id + Path.GetExtension(Resume_File.FileName);
                resumeFile.Resume_Id = resume.Resume_Id;
                resumeFile.FileName = Path.GetFileNameWithoutExtension(Resume_File.FileName) + "_Resume_" + resume.Resume_Id + Path.GetExtension(Resume_File.FileName);
                resumeFile.Path = RESUME_UPLOADS_PATH + resume.Resume_Id + "_" + name;
                resumeFile.UploadedBy = CookieHelper.Username;
                resumeFile.UploadedOn = DateTime.Now;
                DocumentController doc = new DocumentController();
                doc.uploadAWS(fileToCreate, Resume_File);
                db.Resume_File.AddObject(resumeFile);
                db.SaveChanges();
            }

            return RedirectToAction("Index");

            ViewBag.CountryList = new SelectList(CountryHelper.GetCountries(), "CountryCode", "CountryName");

            return View(resume);
        }

        //[HttpPost]
        public ActionResult UploadResume(int id, HttpPostedFileBase Resume_File)
        {
            Resume resume = db.Resumes.Single(x => x.Resume_Id == id);
            Resume_File resumeFile = new Resume_File();
            string name = resume.GivenName + " " + resume.LastName;
            string fileToCreate = RESUME_UPLOADS_PATH + resume.Resume_Id + "_" + name + '/' + Path.GetFileNameWithoutExtension(Resume_File.FileName) + "_Resume_" + resume.Resume_Id + Path.GetExtension(Resume_File.FileName);
            resumeFile.Resume_Id = resume.Resume_Id;
            resumeFile.FileName = Path.GetFileNameWithoutExtension(Resume_File.FileName) + "_Resume_" + resume.Resume_Id + Path.GetExtension(Resume_File.FileName);
            resumeFile.Path = RESUME_UPLOADS_PATH + resume.Resume_Id + "_" + name;
            resumeFile.UploadedBy = CookieHelper.Username;
            resumeFile.UploadedOn = DateTime.Now;
            DocumentController doc = new DocumentController();
            doc.uploadAWS(fileToCreate, Resume_File);
            db.Resume_File.AddObject(resumeFile);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        #endregion



        #region Delete

        public ActionResult Delete(int id)
        {
            Resume resume = db.Resumes.Single(r => r.Resume_Id == id);
            return View(resume);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var fileexists = db.Resume_File.Where(x => x.Resume_Id == id);
            if (fileexists.Count() > 0)
            {
                Resume_File resume_file = db.Resume_File.Single(r => r.Resume_Id == id);
                db.Resume_File.DeleteObject(resume_file);
                db.SaveChanges();
                doc.DeletingAWS(resume_file.Path + '/' + resume_file.FileName);
            }
            Resume resume = db.Resumes.Single(r => r.Resume_Id == id);
            db.Resumes.DeleteObject(resume);
            db.SaveChanges();


            return RedirectToAction("Index");
        }
        public ActionResult DeleteResumeFile(int id)
        {
            Resume_File resume_file = db.Resume_File.Single(r => r.Resume_Id == id);
            db.Resume_File.DeleteObject(resume_file);
            db.SaveChanges();
            doc.DeletingAWS(resume_file.Path + '/' + resume_file.FileName);
            return RedirectToAction("Index");
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}