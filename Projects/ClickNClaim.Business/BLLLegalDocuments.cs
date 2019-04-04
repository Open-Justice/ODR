using ClickNClaim.Common;
using ClickNClaim.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Business
{
    public class BLLLegalDocuments
    {
        public static LegalDocument AddLegalDocument(LegalDocument document)
        {
            using (var repo = new CRUDRepository<LegalDocument>())
            {
                document.CreateDate = DateTime.Now;
                return repo.Add(document);
            }
        }

        public static LegalDocument Update(LegalDocument document)
        {
            using (var repo = new CRUDRepository<LegalDocument>())
            {
                var doc = repo.GetQuery<LegalDocument>(x => x.Id == document.Id).FirstOrDefault();
                if( doc != null)
                {
                    doc.Filename = document.Filename;
                    doc.Url = document.Url;

                    repo.Update(doc);
                    return doc;
                }
                return null;
            }
        }

        public static LegalDocument GetDocumentByName(int conflictId, string documentName)
        {
            using (var repo = new CRUDRepository<LegalDocument>())
            {
                return repo.GetQuery<LegalDocument>(c => c.IdConflict == conflictId && c.Filename == documentName)
                    .OrderByDescending(c => c.Id)
                    .FirstOrDefault();
            }
        }

        public static LegalDocument GetClauseCompromissoire(int conflictId)
        {
            return GetDocumentByName(conflictId, "Clause compromissoire");
        }

        public static void Delete(int id)
        {
            using (var repo = new CRUDRepository<LegalDocument>())
            {
                var doc = repo.GetQuery<LegalDocument>(c => c.Id == id).FirstOrDefault();
                if (doc != null)
                {
                    repo.Delete(doc);
                }
            }
        }

        public static LegalDocument GetLastDocumentForType(int idConflict, int type)
        {
            using (var repo = new CRUDRepository<LegalDocument>())
            {
                var files = repo.GetQuery<LegalDocument>(c => c.IdConflict == idConflict && c.Type == type);
                if (files.Any())
                {
                    if (files.Where(c => c.CreateDate != null).Count() > 0)
                    {
                        return files.OrderByDescending(c => c.CreateDate).First();
                    }
                    else
                    {
                        return files.OrderByDescending(c => c.Id).First();
                    }
                }
                return null;
            }
        }

        public static List<LegalDocument> GetDocumentsForType(int conflictId, int type)
        {
            using (var repo = new CRUDRepository<LegalDocument>())
            {
                var files = repo.GetQuery<LegalDocument>(c => c.IdConflict == conflictId && c.Type == type);
                if (files.Any())
                {
                    if (files.Where(c => c.CreateDate != null).Count() > 0)
                    {
                        return files.OrderBy(c => c.CreateDate).ToList();
                    }
                    else
                    {
                        return files.OrderBy(c => c.Id).ToList();
                    }
                }
                return null;
            }
        }
    }
}
