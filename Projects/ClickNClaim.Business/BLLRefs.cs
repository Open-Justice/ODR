using ClickNClaim.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClickNClaim.Data;
using ClickNClaim.Data.Repositories;

namespace ClickNClaim.Business
{
    public static class BLLRefs
    {
        public static List<ConflictType> ListConflictsType(int? category =null)
        {
            using (var repo = new CRUDRepository<ConflictType>())
            {
                var elts = repo.GetQuery<ConflictType>().Where(c => c.IsMapped);
                
                if (category != null)
                {
                    elts =  elts.Where(c => c.Code.StartsWith(category.Value.ToString()));
                }
                
                return elts.ToList();
            }
        }

        public static List<DefaultEvent> ListDefaultEventsForConflictType(int conflictTypeId)
        {
            using (var repo= new CRUDRepository<DefaultEvent>())
            {
                return repo.GetQuery<DefaultEvent>(c => c.IdConflictType == conflictTypeId).OrderBy(c => c.Type).ToList();
            }
        }

        public static DefaultEvent GetDefaultEvent(int id)
        {
            using (var repo = new CRUDRepository<DefaultEvent>())
            {
                return repo.GetQuery<DefaultEvent>(c => c.Id == id).FirstOrDefault();
            }

        }


        public static ConflictType AddConflictType(ConflictType conflictType)
        {
            using ( var repo = new CRUDRepository<ConflictType>())
            {
                conflictType.IsMapped = false;
                return repo.Add(conflictType);
            }
        }

        public static List<ResolutionType> ListResolutionTypes()
        {
            using (var repo = new CRUDRepository<ResolutionType>())
            {
                return repo.GetQuery<ResolutionType>().ToList();
            }
        }

        public static List<ResolutionType> ListResolutionTypesForConflict(int conflictTypeId)
        {
            using (var repo = new CRUDRepository<ResolutionType>())
            {
                var resolutionTypes = repo.GetQuery<ResolutionType>(c => c.IdConflictType == conflictTypeId).ToList();
                foreach (var item in resolutionTypes)
                {
                    item.ProofFiles = new List<ProofFile>();
                }
                return resolutionTypes;
            }
        }

        public static ResolutionType GetResolutionType(int id)
        {
            using (var repo = new CRUDRepository<ResolutionType>())
            {
                return repo.GetQuery<ResolutionType>(c => c.Id == id).FirstOrDefault();
            }
        }
        
    }
}
