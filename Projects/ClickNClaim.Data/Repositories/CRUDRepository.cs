using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data.EntityFramework;
using System.Data.Entity.Infrastructure;

namespace ClickNClaim.Data.Repositories
{
    public class CRUDRepository<T> : GenericRepository
        where T : class
    {

        public CRUDRepository()
            : base(new ClickNClaim.Data.ClickNClaimEntities())
        {
            this._context.Configuration.LazyLoadingEnabled = false;
            
        }

        public T Add(T t)
        {
            this.Add<T>(t);
            this.UnitOfWork.SaveChanges();
            return t;
        }

        public void Delete(T t)
        {
            this.Attach<T>(t);
            this.Delete<T>(t);
            this.UnitOfWork.SaveChanges();
        }

        public void Update(T t)
        {
            try
            {
                this.Update<T>(t);
                this.UnitOfWork.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ex.Entries.Single().Reload();
            }
        }

        public void UpdateAll(IEnumerable<T> t)
        {
            foreach (var item in t)
            {
                var entry = _context.Entry(item);
                entry.State = System.Data.Entity.EntityState.Modified;
            }
            _context.SaveChanges();
         
        }


        public List<T> ListAll()
        {
            return GetAll<T>().ToList();
        }

      


    }
}
