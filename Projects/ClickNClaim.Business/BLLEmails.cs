using ClickNClaim.Common;
using ClickNClaim.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Business
{
    public static class BLLEmails
    {
        public static Email AddEmail(string to, string subject, string body, Guid guid)
        {
            using (var repo = new CRUDRepository<Email>())
            {
                Email email = new Email();
                email.Id = guid;
                email.to = to;
                email.subject = subject;
                email.body = body;

                email = repo.Add(email);
                return email;
            }
        }

        public static Email GetEmail(Guid id)
        {
            using (var repo = new CRUDRepository<Email>())
            {
                var email = repo.GetQuery<Email>(c => c.Id == id).FirstOrDefault();

                return email;

            }
        }
    }
}
