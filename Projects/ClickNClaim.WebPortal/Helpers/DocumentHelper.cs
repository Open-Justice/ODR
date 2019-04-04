using ClickNClaim.Business;
using ClickNClaim.Common;
using Novacode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ClickNClaim.WebPortal.Helpers
{
    public class DocumentHelper
    {
        public static Stream GenerateMissionAct(int conflictId)
        {
            Conflict conflict = BLLConflicts.GetConflictForArbitration(conflictId);
            var demandeur = conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser);
            if (demandeur.IsLawyer != null && demandeur.IsLawyer.Value)
            {
                demandeur = conflict.UsersInConflicts.First(c => c.IdLawyer == conflict.IdCreationUser);
            }
            var defendeur = conflict.UsersInConflicts.First(c => (c.IsLawyer == null || !c.IsLawyer.Value) && c.IdUser != demandeur.IdUser);

            var stream = File.OpenWrite(HttpContext.Current.Server.MapPath("~/Templates/C1.Acte De Mission.docx"));

            using (DocX document = DocX.Load(stream))
            {
                document.ReplaceText("[N° du Litige]", conflict.Id.ToString());
                document.ReplaceText("[N° RCS Demandeur]", demandeur.UserCompany != null && demandeur.UserCompany.Company != null ? demandeur.UserCompany.Company.RCS : "RCS Inconnu");
                document.ReplaceText("[N° RCS Défendeur]", defendeur.UserCompany != null && defendeur.UserCompany.Company != null ? defendeur.UserCompany.Company.RCS : "RCS Inconnu");
                document.ReplaceText("[Demandeur]", demandeur.User.DisplayName);
                document.ReplaceText("[Défendeur]", defendeur.User.DisplayName);
                document.ReplaceText("[Arbitre]", conflict.Arbiter.DisplayName);
                document.ReplaceText("[Date]", DateTime.Now.ToShortDateString());
            }

            return stream;
        }

        public static Stream GenerateIndependencyAndAcceptanceAct(int conflictId)
        {
            Conflict conflict = BLLConflicts.GetConflictForArbitration(conflictId);

            var stream = File.OpenWrite(HttpContext.Current.Server.MapPath("~/Templates/C2. Déclaration d'acceptation et d'indépendance_V1.docx"));

            using (DocX document = DocX.Load(stream))
            {
                document.ReplaceText("[N° du Litige]", conflict.Id.ToString());
                document.ReplaceText("[Demandeur]", conflict.CreatedBy.DisplayName);
                document.ReplaceText("[Défendeur]", conflict.UsersInConflicts.First(c => c.IdUser != conflict.IdCreationUser).User.DisplayName);
                document.ReplaceText("[Arbitre]", conflict.Arbiter.DisplayName);
                document.ReplaceText("[Date]", DateTime.Now.ToShortDateString());
            }

            return stream;
        }

        public static Stream GenerateSentence(Conflict conflict)
        {
            var stream = File.OpenWrite(HttpContext.Current.Server.MapPath("~/Templates/C4. Sentence_V2.doc"));
            var demandeur = conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser);
            if (demandeur.IsLawyer != null && demandeur.IsLawyer.Value)
            {
                demandeur = conflict.UsersInConflicts.First(c => c.IdLawyer == conflict.IdCreationUser);
            }
            var defendeur = conflict.UsersInConflicts.First(c => (c.IsLawyer == null || !c.IsLawyer.Value) && c.IdUser != demandeur.IdUser);
            var demandeurCompany = demandeur.UserCompany.Company;
            var defendeurCompany = defendeur.UserCompany.Company;

            using (DocX document = DocX.Load(stream))
            {
                document.ReplaceText("[N° du Litige]", conflict.Id.ToString());
                document.ReplaceText("[Demandeur]", conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser).UserCompany.Company.Name);
                document.ReplaceText("[Défendeur]", conflict.UsersInConflicts.First(c => c.IdUser != conflict.IdCreationUser && (c.IsLawyer != null || !c.IsLawyer.Value)).UserCompany.Company.Name);

                document.ReplaceText("[forme de la société 1]", demandeurCompany.Forme);
                document.ReplaceText("[nationalité de la société 1]", demandeurCompany.Country);
                document.ReplaceText("[capital 1]", demandeurCompany.Capital);
                document.ReplaceText("[numéro 1]", demandeurCompany.Siret);
                document.ReplaceText("[ville 1]", demandeurCompany.City);
                document.ReplaceText("[adresse de la société 1]", String.Format("{0} {1} {2} {3} {4}, {5}", demandeurCompany.Address1,
                    String.IsNullOrWhiteSpace(demandeurCompany.Address2) ? "" : ", " + demandeurCompany.Address2,
                     String.IsNullOrWhiteSpace(demandeurCompany.Address3) ? "" : ", " + demandeurCompany.Address3,
                     demandeurCompany.PostalCode, demandeurCompany.City,
                     demandeurCompany.Country
                    ));

                if (demandeur.IsRepresented)
                {
                    var avocatDemandeur = conflict.UsersInConflicts.Where(c => c.IdUser == demandeur.IdLawyer).First();
                    document.ReplaceText("[avocat 1]", avocatDemandeur.User.DisplayName);
                    document.ReplaceText("[ville a1]", avocatDemandeur.User.BusinessFunction);
                }

                document.ReplaceText("[forme de la société 2]", defendeurCompany.Forme);
                document.ReplaceText("[nationalité de la société 2]", defendeurCompany.Country);
                document.ReplaceText("[capital 2]", defendeurCompany.Capital);
                document.ReplaceText("[numéro 2]", defendeurCompany.Siret);
                document.ReplaceText("[ville 2]", defendeurCompany.City);
                document.ReplaceText("[adresse de la société 2]", String.Format("{0} {1} {2} {3} {4}, {5}", defendeurCompany.Address1,
                    String.IsNullOrWhiteSpace(defendeurCompany.Address2) ? "" : ", " + defendeurCompany.Address2,
                     String.IsNullOrWhiteSpace(defendeurCompany.Address3) ? "" : ", " + defendeurCompany.Address3,
                     defendeurCompany.PostalCode, defendeurCompany.City,
                     defendeurCompany.Country
                    ));

                if (defendeur.IsRepresented)
                {
                    var avocatDefendeur = conflict.UsersInConflicts.Where(c => c.IdUser == defendeur.IdLawyer).First();
                    document.ReplaceText("[avocat 2]", avocatDefendeur.User.DisplayName);
                    document.ReplaceText("[ville a2]", avocatDefendeur.User.BusinessFunction);
                }


                //document.ReplaceText("[date_acte_mission]", "");
              //  document.ReplaceText("[date_acceptation]")



                document.ReplaceText("[Arbitre]", conflict.Arbiter.DisplayName);
                document.ReplaceText("[Date]", DateTime.Now.ToShortDateString());
            }

            return stream;
        }
    }
}