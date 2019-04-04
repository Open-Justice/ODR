using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using ClickNClaim.Business;
using ClickNClaim.Common;
using ClickNClaim.WebPortal.Helpers;
using ClickNClaim.WebPortal.Extensions;

namespace ClickNClaim.WebPortal.Controllers
{
    public class UploadController : Controller
    {
        // GET: Upload
        public ActionResult Index()
        {
            return View();
        }

        public ContentResult UploadFile()
        {

            var conflictId = int.Parse(Request.Form["conflictId"]);
            var eventId = Request.Form["EventId"];
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
                if (hpf.ContentLength == 0)
                    continue;

                var blockBlob = AzureFileHelper.AddFile(conflictId, hpf);

                var f = new ProofFile();
                f.FilePath = blockBlob.Uri.AbsoluteUri;
                f.IdConflict = conflictId;
                f.Name = blockBlob.Name;

                if (eventId == "0")
                {
                    var resultFile = BLLConflicts.AddFile(f);

                    return new ContentResult() { Content = JsonHelper.GetJsonString(resultFile), ContentType = "application/json" };
                }
                else
                {
                    var resultFile = BLLConflicts.AddFile(f, int.Parse(eventId));

                    return new ContentResult() { Content = JsonHelper.GetJsonString(resultFile), ContentType = "application/json" };
                }

            }
            return new ContentResult();
        }

        public ContentResult UploadFileFromDefault()
        {
            var conflictId = int.Parse(Request.Form["conflictId"]);
            string uid = Request.Form["uid"];
            List<ProofFile> fileIds = new List<ProofFile>();
            foreach (string file in Request.Files)
            {
                HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
                if (hpf.ContentLength == 0)
                    continue;

                var blockBlob = AzureFileHelper.AddFile(conflictId, hpf);




                var f = new ProofFile();
                f.FilePath = blockBlob.Uri.AbsoluteUri;
                f.IdConflict = conflictId;
                f.Name = blockBlob.Name;
                f.uid = uid;
                f = BLLConflicts.AddFile(f);
                fileIds.Add(f);

            }
            return new ContentResult() { Content = JsonHelper.GetJsonString(fileIds), ContentType = "application/json" };
        }

        //public ContentResult UploadFileFromDefault()
        //{
        //    var conflictId = int.Parse(Request.Form["conflictId"]);
        //    var eventId = Request.Form["EventId"];
        //    var defaultEventId = Request.Form["DefaultEventId"];
        //    DateTime date;
        //   var hasDate = DateTime.TryParse(Request.Form["Date"], out date);
        //    var description = Request.Form["Description"];

        //    DefaultEvent defaultEvent = null;
        //    if (!String.IsNullOrWhiteSpace(defaultEventId))
        //    {
        //        defaultEvent = BLLRefs.GetDefaultEvent(int.Parse(defaultEventId));
        //    }

        //    Event evt = null;
        //    if (eventId == "0")
        //    {
        //        evt = BLLConflicts.AddEvent(new Event()
        //        {
        //            DateBegin = hasDate ? date : DateTime.Now,
        //            Description = description,
        //            IdConflict = conflictId,
        //            IdDefaultEvent = defaultEvent.Id,
        //            IdUser = User.Identity.GetId(),
        //            Name = defaultEvent.Name,
        //            Type = defaultEvent.Type
        //        });
        //    }
        //    else
        //    {
        //        evt = BLLConflicts.GetEvent(int.Parse(eventId));
        //    }


        //    foreach (string file in Request.Files)
        //    {
        //        HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
        //        if (hpf.ContentLength == 0)
        //            continue;

        //        var blockBlob = AzureFileHelper.AddFile(conflictId, hpf);




        //        var f = new ProofFile();
        //        f.FilePath = blockBlob.Uri.AbsoluteUri;
        //        f.IdConflict = conflictId;
        //        f.Name = blockBlob.Name;


        //        var resultFile = BLLConflicts.AddFile(f, evt.Id);
        //        if (evt.ProofFiles == null)
        //            evt.ProofFiles = new List<ProofFile>();

        //        evt.ProofFiles.Add(resultFile);


        //    }
        //    return new ContentResult() { Content = JsonHelper.GetJsonString(evt), ContentType = "application/json" };
        //}



        public static ContentResult DeleteFile(int conflictId, string fileName)
        {

            AzureFileHelper.DeleteFile(conflictId, fileName);
            return new ContentResult() { Content = "OK" };
        }


    }
}