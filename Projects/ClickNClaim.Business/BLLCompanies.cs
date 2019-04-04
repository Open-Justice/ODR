using ClickNClaim.Common;
using ClickNClaim.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Business
{
    public class BLLCompanies
    {
        public static Company AddOrUpdateCompany(Company c, string userId)
        {
            using (var repo = new CRUDRepository<Company>())
            {
                var previous = repo.GetQuery<Company>(u => u.Id == c.Id).FirstOrDefault();
                if (previous == null)
                {
                   var company =  repo.Add(c);
                    repo.Add<UserCompany>(new UserCompany() { IdCompany = company.Id, IdUser = userId });
                    repo.UnitOfWork.SaveChanges();
                    return company;
                }
                else
                {
                    if(!String.IsNullOrWhiteSpace(c.Name))
                    previous.Name = c.Name;
                    if (!String.IsNullOrWhiteSpace(c.PostalCode))
                        previous.PostalCode = c.PostalCode;
                    if (!String.IsNullOrWhiteSpace(c.Address1))
                        previous.Address1 = c.Address1;
                    if (!String.IsNullOrWhiteSpace(c.Address2))
                        previous.Address2 = c.Address2;
                    if (!String.IsNullOrWhiteSpace(c.Address3))
                        previous.Address3 = c.Address3;
                    if (!String.IsNullOrWhiteSpace(c.City))
                        previous.City = c.City;
                    if (!String.IsNullOrWhiteSpace(c.Fonction))
                        previous.Fonction = c.Fonction;
                    if (!String.IsNullOrWhiteSpace(c.RCS))
                        previous.RCS = c.RCS;
                    if (!String.IsNullOrWhiteSpace(c.Siret))
                        previous.Siret = c.Siret;
                    if (!String.IsNullOrWhiteSpace(c.TelCompany))
                        previous.TelCompany = c.TelCompany;
                    repo.Update(previous);
                    return previous;
                }
            }
        }

        public static void AddOrUpdateCompanyForUserInConflict(int companyId, string userId, int conflictId)
        {
            using (var repo=  new CRUDRepository<UsersInConflict>())
            {
                var userCompany = repo.GetQuery<UserCompany>(c => c.IdUser == userId && c.IdCompany == companyId).First();

                var company = repo.GetQuery<Company>(c => c.Id == companyId).FirstOrDefault();

                var previous = repo.GetQuery<UsersInConflict>(c => c.IdConflict == conflictId && c.IdUser == userId).FirstOrDefault();
                if( previous != null)
                {
                    previous.IdUserCompany = userCompany.Id;
                    previous.CompanyName = company.Name;
                    repo.Update(previous);
                }
               
            }
        }

        public static Company GetCompany(int id)
        {
            using (var repo = new CRUDRepository<Company>())
            {
                return repo.GetQuery<Company>(c => c.Id == id).FirstOrDefault();
            }
        }

        public static Company GetCompany(string siret)
        {
            using (var repo = new CRUDRepository<Company>())
            {
                return repo.GetQuery<Company>(c => c.Siret == siret).FirstOrDefault();
            }
        }

        public static List<Company> GetUserCompanies(string idUser) {
            using (var repo = new CRUDRepository<Company>())
            {
                return repo.GetQuery<Company>().Where(c => c.UserCompanies.Any(d => d.IdUser == idUser)).ToList();
            }
}
      
    }
}
