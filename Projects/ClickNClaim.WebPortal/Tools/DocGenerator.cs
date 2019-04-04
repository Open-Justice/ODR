using ClickNClaim.Business;
using ClickNClaim.Common;
using PdfSharp;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using ClickNClaim.WebPortal.Extensions;
using ClickNClaim.BodaccReader;
using ClickNClaim.Common.Enums;

namespace ClickNClaim.WebPortal.Tools
{
    public class DocGenerator
    {

        public static FileStreamed GenerateAcceptanceDeclaration(Conflict conflict, string arbiterName)
        {
            var filename = "Declaration_d_acceptation_et_d_independance_" + conflict.Id + ".pdf";

            var demandeur = conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser);
            if (demandeur.IsLawyer != null && demandeur.IsLawyer.Value)
            {
                demandeur = conflict.UsersInConflicts.First(c => c.IdLawyer == conflict.IdCreationUser);
            }
            var defendeur = conflict.UsersInConflicts.First(c => (c.IsLawyer == null || !c.IsLawyer.Value) && c.IdUser != demandeur.IdUser);


            MemoryStream ms = new MemoryStream();
            StringWriter wr = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(wr))
            {
                StringWriter wwr = new StringWriter();
                using (HtmlTextWriter titleWriter = new HtmlTextWriter(wwr))
                {
                    titleWriter.AddTag("p", "DECLARATION D'ACCEPTATION ET D'INDEPENDANCE", HtmlTextWriterDefaults.CenteredTitle);
                    titleWriter.AddTag("p", "Litige n° " + conflict.Id, HtmlTextWriterDefaults.CenteredSubTitleInfo);
                    titleWriter.AddTag("p", (demandeur.CompanyName ?? demandeur.User.DisplayName) + " c/ " + (defendeur.CompanyName ?? defendeur.User.DisplayName), HtmlTextWriterDefaults.CenteredSubTitleInfo);
                    titleWriter.AddTag("p", " ");
                    writer.TitleBox(wwr.ToString());
                }
                wwr = null;

                writer.AddTag("p", "Je soussigné,", HtmlTextWriterDefaults.DefaultText);
                writer.AddTag("b", arbiterName, HtmlTextWriterDefaults.DefaultText);

                writer.AddTag("p", "Déclare personnellement accepter la mission d’arbitre selon le Règlement d’arbitrage de l’Idam dans la présente affaire. En conséquence,");

                writer.RenderBeginTag("ul");
                writer.AddTag("li", "Je déclare avoir pris connaissance des exigences du Règlement d’arbitrage de l’Idam et de la Charte éthique ;");
                writer.AddTag("li", "Je déclare avoir les aptitudes requises pour exercer la mission d’arbitre conformément à toutes les dispositions de ce Règlement ;");
                writer.AddTag("li", "Je déclare avoir la disponibilité requise pour mener à bien la mission qui m’est confiée ;");
                writer.AddTag("li", "Je suis indépendant de chacune des parties en cause et entend le rester ;");
                writer.AddTag("li", "A ma connaissance, il n’existe aucun fait ou circonstance passé ou présent qui nécessite d’être révélé parce qu’il pourrait être de nature à mettre en cause mon indépendance dans l’esprit de l’une quelconque des parties ;");
                writer.AddTag("li", "Je déclare que rien ne s’oppose à ce que je tranche le Litige opposant les Parties en qualité d’arbitre");
                writer.RenderEndTag();

                writer.AddTag("p", "Fait à Paris, le " + DateTime.Now.ToShortDateString());
                writer.AddTag("p", arbiterName);

                using (PdfDocument pdf = PdfGenerator.GeneratePdf(wr.ToString(), PageSize.A4))
                {
                    pdf.Save(ms, false);
                }
            }
            return new FileStreamed(filename, ms);
        }

        public static FileStreamed GeneratePreSentenceReport(Conflict conflict)
        {
            var filename = "Pré-sentence_Dossier_N°_" + conflict.Id + ".docx";
            var fileStream = new MemoryStream();

            var demandeur = conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser);
            if (demandeur.IsLawyer != null && demandeur.IsLawyer.Value)
            {
                demandeur = conflict.UsersInConflicts.First(c => c.IdLawyer == conflict.IdCreationUser);
            }
            var defendeur = conflict.UsersInConflicts.First(c => (c.IsLawyer == null || !c.IsLawyer.Value) && c.IdUser != demandeur.IdUser);


            MemoryStream ms = new MemoryStream();
            StringWriter wr = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(wr))
            {
                StringWriter wwr = new StringWriter();
                using (HtmlTextWriter titleWriter = new HtmlTextWriter(wwr))
                {
                    titleWriter.AddTag("p", "PRE-SENTENCE ARBITRALE", HtmlTextWriterDefaults.CenteredTitle);
                    titleWriter.AddTag("p", "Litige n° " + conflict.Id, HtmlTextWriterDefaults.CenteredSubTitleInfo);
                    titleWriter.AddTag("p", (demandeur.CompanyName ?? demandeur.User.DisplayName) + " c/ " + (defendeur.CompanyName ?? defendeur.User.DisplayName), HtmlTextWriterDefaults.CenteredSubTitleInfo);
                    titleWriter.AddTag("p", " ");
                    writer.TitleBox(wwr.ToString());
                }
                wwr = null;

                writer.AddTag("p", "PARTIES", HtmlTextWriterDefaults.H2_Centered);

                writer.AddTag("p", "Entre :", HtmlTextWriterDefaults.DefaulText_Bold);
                var demandeurCompany = demandeur.UserCompany.Company;
                writer.AddTag("p", demandeurCompany.Name + ", " + demandeurCompany.Forme + " de droit " + demandeurCompany.Country +
                    ", au capital de " + demandeurCompany.Capital + " euros, immatriculée sous le numéro " + demandeurCompany.Siret + " au RCS de " +
                    demandeurCompany.City + ", dont le siège social est " + String.Format("{0} {1} {2} {3} {4}, {5}", demandeurCompany.Address1,
                    String.IsNullOrWhiteSpace(demandeurCompany.Address2) ? "" : ", " + demandeurCompany.Address2,
                     String.IsNullOrWhiteSpace(demandeurCompany.Address3) ? "" : ", " + demandeurCompany.Address3,
                     demandeurCompany.PostalCode, demandeurCompany.City,
                     demandeurCompany.Country
                    ) + ", demandeur,");

                if (demandeur.IsRepresented)
                {
                    var avocatDemandeur = conflict.UsersInConflicts.Where(c => c.IdUser == demandeur.IdLawyer).First();
                    writer.AddTag("p", "Assisté par Me " + avocatDemandeur.User.DisplayName + ", Avocat au barreau de " + avocatDemandeur.User.BusinessFunction + ".");
                }

                writer.AddTag("p", "Et :", HtmlTextWriterDefaults.DefaulText_Bold);
                var defendeurCompany = defendeur.UserCompany.Company;
                writer.AddTag("p", defendeurCompany.Name + ", " + defendeurCompany.Forme + " de droit " + defendeurCompany.Country +
                    ", au capital de " + defendeurCompany.Capital + " euros, immatriculée sous le numéro " + defendeurCompany.Siret + " au RCS de " +
                    defendeurCompany.City + ", dont le siège social est " + String.Format("{0} {1} {2} {3} {4}, {5}", defendeurCompany.Address1,
                    String.IsNullOrWhiteSpace(defendeurCompany.Address2) ? "" : ", " + defendeurCompany.Address2,
                     String.IsNullOrWhiteSpace(defendeurCompany.Address3) ? "" : ", " + defendeurCompany.Address3,
                     defendeurCompany.PostalCode, defendeurCompany.City,
                     defendeurCompany.Country
                    ) + ", défendeur,");

                writer.BreakLine();
                writer.BreakLine();
                writer.BreakLine();

                writer.AddTag("p", "TABLE DES MATIERES", HtmlTextWriterDefaults.H2_Centered);

                writer.AddTag("p", "I.   Faits et introduction de l'arbitrage", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "II.  Mise en œuvre de l'arbitrage et procédure", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "III. Position des parties", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "IV.  Motivation", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "V.   Décision", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.BreakLine();
                writer.BreakLine();
                writer.BreakLine();

                if (conflict.HasArbitralClause)
                {
                    writer.AddTag("p", "Vu la clause compromissoire,", HtmlTextWriterDefaults.DefaultText_Italic);
                }
                else
                {
                    writer.AddTag("p", " Vu le compromis d'arbitrage,", HtmlTextWriterDefaults.DefaultText_Italic);
                }
                writer.AddTag("p", "Vu le règlement du Centre d’arbitrage,", HtmlTextWriterDefaults.DefaultText_Italic);

                var missionOrder = BLLLegalDocuments.GetLastDocumentForType(conflict.Id, (int)LegalDocumentTypeEnum.MissionAct);
                if (missionOrder != null && missionOrder.CreateDate != null && missionOrder.CreateDate.Value != null)
                {
                    writer.AddTag("p", "Vu l’acte de mission du "+ missionOrder.CreateDate.Value.ToShortDateString() +",", HtmlTextWriterDefaults.DefaultText_Italic);
                }
                writer.AddTag("p", "Vu la déclaration d’acceptation et d’indépendance du " + conflict.Events.First(c => c.Name == "Déclaration d'acceptation et d'indépendance de l'arbitre").DateBegin.ToShortDateString() + ",", HtmlTextWriterDefaults.DefaultText_Italic);

                var ordonnance = BLLLegalDocuments.GetDocumentsForType(conflict.Id, (int)LegalDocumentTypeEnum.OrdonnanceProcedure);
                if (ordonnance != null && ordonnance.Count > 0)
                {
                    foreach (var item in ordonnance)
                    {
                        writer.AddTag("p", "Vu l’ordonnance de procédure du "+ item.CreateDate.Value.ToShortDateString() +",", HtmlTextWriterDefaults.DefaultText_Italic);
                    }
                }

                writer.AddTag("p", "Vu les arguments et pièces des parties tels qu’exposés sur la plateforme en ligne,", HtmlTextWriterDefaults.DefaultText_Italic);

                writer.AddTag("p", "Après avoir laissé le temps nécessaire aux parties pour exposer en fait et en droit leurs arguments respectifs, la juridiction arbitrale décide :");

                writer.AddTag("p", "I.   Faits et introduction de l'arbitrage", HtmlTextWriterDefaults.DefaulText_Bold);


                writer.AddTag("p", "II.  Mise en œuvre de l'arbitrage et procédure", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.AddTag("p", "a)	L'arbitre", HtmlTextWriterDefaults.DefaultText_WithLeftPadding);
                writer.AddTag("p", "Conformément au Règlement d’arbitrage, " + conflict.Arbiter.DisplayName + " a été désigné en qualité d’arbitre, lequel a déclaré ne pas se trouver en situation de conflit d’intérêts et a accepté sa mission.");
                writer.AddTag("p", "Le Centre d’arbitrage a ensuite notifié aux parties la désignation de l'arbitre et fixé la provision pour les frais administratifs et honoraires dus à l'arbitre.");
                writer.AddTag("p", "Cette provision a été réglée comme indiqué en annexe.");

                writer.AddTag("p", "b)	La procédure", HtmlTextWriterDefaults.DefaultText_WithLeftPadding);
                writer.AddTag("p", "L'arbitrage a lieu à Paris, sur la plateforme en ligne de l’Idam. La langue de l'arbitrage est le français. L'arbitrage est soumis au droit français. La procédure arbitrale est réglée par les arbitres conformément au Règlement d’arbitrage. L’arbitrage ne donne pas lieu à une audience. La sentence est exécutoire par provision. Le point de départ de l'arbitrage est fixé à la de signature de l'acte de mission.");
                writer.AddTag("p", "Les parties ont exposé leurs arguments en fait et en droit sur la plateforme. Conformément au Règlement d’arbitrage, l’arbitrage s’est déroulé sans audience.");
                writer.AddTag("p", "Aucune difficulté ni aucune exception ou contestation relative aux écritures et pièces n’a été soulevée.");

                writer.AddTag("p", "III. Position des parties", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.AddTag("p", "Il résulte des discussions sur la plateforme au jour de la présente sentence qu’il n’existe pas de contestation des parties concernant les points suivants :");
                foreach (var item in conflict.Events.Where(c => c.Disagreements == null || c.Disagreements.Count == 0 && (c.Type == (int)EventTypeEnum.Event || c.Type == (int)EventTypeEnum.Resolution)))
                {
                    writer.AddTag("p", item.Name, HtmlTextWriterDefaults.DefaulText_Bold);
                    writer.AddTag("p", item.Description);
                }

                writer.AddTag("p", "En revanche, les parties sont en désaccord concernant les points suivants : ");
                foreach (var item in conflict.Events.Where(c => c.Disagreements != null && c.Disagreements.Count > 0 && (c.Type == (int)EventTypeEnum.Event || c.Type == (int)EventTypeEnum.Resolution)))
                {
                    writer.AddTag("p", item.Name, HtmlTextWriterDefaults.DefaulText_Bold);
                    writer.AddTag("p", item.Description);
                    if (item.Disagreements.First().DisagreementOnDate)
                    {
                        writer.AddTag("p", "Désaccord sur la date :", HtmlTextWriterDefaults.DefaulText_Bold);
                        writer.AddTag("p", item.Disagreements.First().ConcurrentDate.Value.ToShortDateString() + " au lieu de " + item.DateBegin.ToShortDateString());
                    }
                    if (item.Disagreements.First().DisagreementOnDescription)
                    {
                        writer.AddTag("p", "Désaccord sur le fait lui-même :", HtmlTextWriterDefaults.DefaulText_Bold);
                        writer.AddTag("p", item.Disagreements.First().ConcurrentDescription);
                    }
                    if (item.Disagreements.First().DisagreementOnPiece)
                    {
                        writer.AddTag("p", "Désaccord sur une ou plusieurs pièces :", HtmlTextWriterDefaults.DefaulText_Bold);
                        writer.AddTag("p", item.Disagreements.First().ConcurrentPieces);
                    }

                }
                writer.BreakLine();

                writer.AddTag("p", "IV. Motivation", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.AddTag("p", "Demandes de la société 1 : ", HtmlTextWriterDefaults.DefaultText_Underlined);
                foreach (var item in conflict.Resolutions.Where(c => c.IdUser == conflict.IdCreationUser))
                {
                    writer.AddTag("p", item.Name, HtmlTextWriterDefaults.DefaulText_Bold);
                    writer.AddTag("p", item.ResolutionComment);
                    if (item.ProofFiles != null && item.ProofFiles.Count > 0)
                    {
                        writer.AddTag("p", "Elle a pour cela fourni le ou les documents suivant :");
                        foreach (var file in item.ProofFiles)
                        {
                            writer.AddLink(file.Name, file.FilePath);
                        }
                    }
                }

                if (conflict.Resolutions.Any(c => c.IdUser != conflict.IdCreationUser))
                {
                    writer.AddTag("p", "Demande reconventionnelle de la société 2 :", HtmlTextWriterDefaults.DefaultText_Underlined);
                    foreach (var item in conflict.Resolutions.Where(c => c.IdUser != conflict.IdCreationUser))
                    {
                        writer.AddTag("p", item.Name, HtmlTextWriterDefaults.DefaulText_Bold);
                        writer.AddTag("p", item.ResolutionComment);
                        if (item.ProofFiles != null && item.ProofFiles.Count > 0)
                        {
                            writer.AddTag("p", "Elle a pour cela fourni le ou les documents suivant :");
                            foreach (var file in item.ProofFiles)
                            {
                                writer.AddLink(file.Name, file.FilePath);
                            }
                        }
                    }
                }

                writer.AddTag("p", "Frais de l’arbitrage et autres demandes accessoires :", HtmlTextWriterDefaults.DefaultText_Underlined);
                writer.AddTag("p", "Une consignation a été versée pour couvrir les frais d'arbitrage comprenant les frais administratifs et les honoraires de l'arbitre, d'un montant de " + ConflictCost.GetConflictCost(conflict.ConflictType) + " euros HT.");

                writer.AddTag("p", "Chaque partie a sollicité la condamnation de l'autre à payer le total des frais du présent arbitrage. Le règlement d'arbitrage prévoit que l'arbitre liquide les frais d'arbitrage et en répartit la charge.");



                writer.AddTODO("[Motivation de la répartition des frais d'arbitrage].");


                writer.AddTag("p", "Demandes au titre des honoraires d'avocat et débours des parties :", HtmlTextWriterDefaults.DefaultText_Underlined);

                writer.AddTODO("La société 1 évalue ses frais à la somme de [montant] euros.");
                writer.AddTODO("La société 2 évalue ses frais à la somme de [montant] euros.");
                writer.AddTODO("[Motivation de la répartition des honoraires d'avocat et débours des parties].");


                writer.AddTag("p", "V. Décision", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "Pour l'ensemble des motifs développés ci-avant, par sentence arbitrale contradictoire prononcée en dernier ressort :");
                writer.AddTODO("[Décision de l arbitre].");

                writer.BreakLine();
                writer.BreakLine();
                writer.AddTag("p", "Les parties s'engagent à exécuter fidèlement et intégralement la sentence. La partie qui refuserait de l'exécuter prendra à sa charge tous les frais et droits auxquels la poursuite et l'exécution de ladite sentence donnera lieu.");
                writer.BreakLine();
                writer.BreakLine();


                writer.AddTag("p", "Fait à Paris, le " + DateTime.Now.ToShortDateString());
                writer.AddTag("p", conflict.Arbiter.DisplayName);

                ClickNClaim.WebPortal.Tools.HtmlToDocx.SaveDOCX(fileStream, wr.ToString(), true);
                fileStream.Seek(0, SeekOrigin.Begin);

            }
            return new FileStreamed(filename, fileStream);
        }

        internal static FileStreamed GenerateOrdonnanceProcedure(Conflict conflict, Debate debat)
        {
            var filename = "Ordonnance_Procedure_" + debat.Id + "_Dossier_N°_" + conflict.Id + ".docx";
            var fileStream = new MemoryStream();

            var demandeur = conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser);
            if (demandeur.IsLawyer != null && demandeur.IsLawyer.Value)
            {
                demandeur = conflict.UsersInConflicts.First(c => c.IdLawyer == conflict.IdCreationUser);
            }
            var defendeur = conflict.UsersInConflicts.First(c => (c.IsLawyer == null || !c.IsLawyer.Value) && c.IdUser != demandeur.IdUser);


            MemoryStream ms = new MemoryStream();
            StringWriter wr = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(wr))
            {
                StringWriter wwr = new StringWriter();
                using (HtmlTextWriter titleWriter = new HtmlTextWriter(wwr))
                {
                    titleWriter.AddTag("p", "ORDONNANCE DE PROCEDURE", HtmlTextWriterDefaults.CenteredTitle);
                    titleWriter.AddTag("p", "Litige n° " + conflict.Id, HtmlTextWriterDefaults.CenteredSubTitleInfo);
                    titleWriter.AddTag("p", (demandeur.CompanyName ?? demandeur.User.DisplayName) + " c/ " + (defendeur.CompanyName ?? defendeur.User.DisplayName), HtmlTextWriterDefaults.CenteredSubTitleInfo);
                    titleWriter.AddTag("p", " ");
                    writer.TitleBox(wwr.ToString());
                }
                wwr = null;

                writer.AddTag("p", "PARTIES", HtmlTextWriterDefaults.H2_Centered);

                writer.AddTag("p", "Entre :", HtmlTextWriterDefaults.DefaulText_Bold);
                var demandeurCompany = demandeur.UserCompany.Company;
                writer.AddTag("p", demandeurCompany.Name + ", " + demandeurCompany.Forme + " de droit " + demandeurCompany.Country +
                    ", au capital de " + demandeurCompany.Capital + " euros, immatriculée sous le numéro " + demandeurCompany.Siret + " au RCS de " +
                    demandeurCompany.City + ", dont le siège social est " + String.Format("{0} {1} {2} {3} {4}, {5}", demandeurCompany.Address1,
                    String.IsNullOrWhiteSpace(demandeurCompany.Address2) ? "" : ", " + demandeurCompany.Address2,
                     String.IsNullOrWhiteSpace(demandeurCompany.Address3) ? "" : ", " + demandeurCompany.Address3,
                     demandeurCompany.PostalCode, demandeurCompany.City,
                     demandeurCompany.Country
                    ) + ", demandeur,");

                if (demandeur.IsRepresented)
                {
                    var avocatDemandeur = conflict.UsersInConflicts.Where(c => c.IdUser == demandeur.IdLawyer).First();
                    writer.AddTag("p", "Assisté par Me " + avocatDemandeur.User.DisplayName + ", Avocat au barreau de " + avocatDemandeur.User.BusinessFunction + ".");
                }

                writer.AddTag("p", "Et :", HtmlTextWriterDefaults.DefaulText_Bold);
                var defendeurCompany = defendeur.UserCompany.Company;
                writer.AddTag("p", defendeurCompany.Name + ", " + defendeurCompany.Forme + " de droit " + defendeurCompany.Country +
                    ", au capital de " + defendeurCompany.Capital + " euros, immatriculée sous le numéro " + defendeurCompany.Siret + " au RCS de " +
                    defendeurCompany.City + ", dont le siège social est " + String.Format("{0} {1} {2} {3} {4}, {5}", defendeurCompany.Address1,
                    String.IsNullOrWhiteSpace(defendeurCompany.Address2) ? "" : ", " + defendeurCompany.Address2,
                     String.IsNullOrWhiteSpace(defendeurCompany.Address3) ? "" : ", " + defendeurCompany.Address3,
                     defendeurCompany.PostalCode, defendeurCompany.City,
                     defendeurCompany.Country
                    ) + ", défendeur,");

                writer.BreakLine();
                writer.BreakLine();
                writer.BreakLine();

                writer.AddTag("p", "TABLE DES MATIERES", HtmlTextWriterDefaults.H2_Centered);

                writer.AddTag("p", "I.	Objet de l’ordonnance de procédure", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "II.	Position des parties", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "III.Motivation", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "IV.	Décision", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.BreakLine();
                writer.BreakLine();
                writer.BreakLine();

                if (conflict.HasArbitralClause)
                {
                    writer.AddTag("p", "Vu la clause compromissoire,", HtmlTextWriterDefaults.DefaultText_Italic);
                }
                else
                {
                    writer.AddTag("p", " Vu le compromis d'arbitrage,", HtmlTextWriterDefaults.DefaultText_Italic);
                }
                writer.AddTag("p", "Vu le règlement du Centre d’arbitrage,", HtmlTextWriterDefaults.DefaultText_Italic);
                writer.AddTag("p", "Vu l’acte de mission du[date],", HtmlTextWriterDefaults.DefaultText_Italic);
                writer.AddTag("p", "Vu la déclaration d’acceptation et d’indépendance du " + conflict.Events.First(c => c.Name == "Déclaration d'acceptation et d'indépendance de l'arbitre").DateBegin.ToShortDateString() + ",", HtmlTextWriterDefaults.DefaultText_Italic);
                writer.AddTag("p", "Vu l’ordonnance de procédure du " + DateTime.Now + ",", HtmlTextWriterDefaults.DefaultText_Italic);
                writer.AddTag("p", "Vu les arguments et pièces des parties tels qu’exposés sur la plateforme en ligne,", HtmlTextWriterDefaults.DefaultText_Italic);

                writer.AddTag("p", "Après avoir laissé le temps nécessaire aux parties pour exposer en fait et en droit leurs arguments respectifs, la juridiction arbitrale décide :");

                writer.AddTag("p", "I.	Objet de l’ordonnance de procédure", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.AddTag("p", debat.Title);
                writer.AddTag("p", debat.Description);


                writer.AddTag("p", "II. Position des parties", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.AddTag("p", "Il résulte des discussions sur la plateforme au jour de la présente sentence qu’il n’existe pas de contestation des parties concernant les points suivants :");
                writer.AddTODO("[Exposer ici les points sur lesquels il n’existe pas de désaccord entre les parties]");
                writer.AddTag("p", "En revanche, les parties sont en désaccord concernant les points suivants :");
                writer.AddTODO("[Exposer ici les points sur lesquels il existe des désaccords entre les parties, en mentionnant prétentions et arguments de chaque partie]");

                writer.AddTag("p", "III. Motivation", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.AddTag("p", "Demandes 1 :", HtmlTextWriterDefaults.DefaultText_Underlined);
                writer.BreakLine();
                writer.AddTag("p", "Demandes 2 :", HtmlTextWriterDefaults.DefaultText_Underlined);

                writer.AddTag("p", " IV.Décision", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "Pour l'ensemble des motifs développés ci-avant, par ordonnance de procédure contradictoire prononcée en dernier ressort :");
                writer.AddTODO("- Disons que -------------, sous astreinte de 500 € par jour de retard passer un délai de 5 jours à compter de la mise à disposition sur la plateforme de la présente sentence arbitrale ;");
                writer.AddTag("p", "- Réservons les frais et honoraires d'arbitrage ;");
                writer.AddTag("p", "- Disons que la présente sentence arbitrale est exécutoire par provision ;");
                writer.AddTag("p", "- Déboutons les parties de leurs demandes plus amples ou contraires.");

                writer.BreakLine();
                writer.AddTag("p", "Les parties s'engagent à exécuter fidèlement et intégralement l’ordonnance de procédure. La partie qui refuserait de l'exécuter prendra à sa charge tous les frais et droits auxquels la poursuite et l'exécution de ladite sentence donnera lieu.");

                writer.AddTag("p", "Fait à Paris, le " + DateTime.Now.ToShortDateString());
                writer.AddTag("p", conflict.Arbiter.DisplayName);

                ClickNClaim.WebPortal.Tools.HtmlToDocx.SaveDOCX(fileStream, wr.ToString(), true);
                fileStream.Seek(0, SeekOrigin.Begin);
            }
            return new FileStreamed(filename, fileStream);
        }

        public static FileStreamed GenerateSentenceReport(Conflict conflict)
        {
            var filename = "Sentence_Dossier_N°_" + conflict.Id + ".docx";
            var fileStream = new MemoryStream();

            var demandeur = conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser);
            if (demandeur.IsLawyer != null && demandeur.IsLawyer.Value)
            {
                demandeur = conflict.UsersInConflicts.First(c => c.IdLawyer == conflict.IdCreationUser);
            }
            var defendeur = conflict.UsersInConflicts.First(c => (c.IsLawyer == null || !c.IsLawyer.Value) && c.IdUser != demandeur.IdUser);


            MemoryStream ms = new MemoryStream();
            StringWriter wr = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(wr))
            {
                StringWriter wwr = new StringWriter();
                using (HtmlTextWriter titleWriter = new HtmlTextWriter(wwr))
                {
                    titleWriter.AddTag("p", "SENTENCE ARBITRALE", HtmlTextWriterDefaults.CenteredTitle);
                    titleWriter.AddTag("p", "Litige n° " + conflict.Id, HtmlTextWriterDefaults.CenteredSubTitleInfo);
                    titleWriter.AddTag("p", (demandeur.CompanyName ?? demandeur.User.DisplayName) + " c/ " + (defendeur.CompanyName ?? defendeur.User.DisplayName), HtmlTextWriterDefaults.CenteredSubTitleInfo);
                    titleWriter.AddTag("p", " ");
                    writer.TitleBox(wwr.ToString());
                }
                wwr = null;

                writer.AddTag("p", "PARTIES", HtmlTextWriterDefaults.H2_Centered);

                writer.AddTag("p", "Entre :", HtmlTextWriterDefaults.DefaulText_Bold);
                var demandeurCompany = demandeur.UserCompany.Company;
                writer.AddTag("p", demandeurCompany.Name + ", " + demandeurCompany.Forme + " de droit " + demandeurCompany.Country +
                    ", au capital de " + demandeurCompany.Capital + " euros, immatriculée sous le numéro " + demandeurCompany.Siret + " au RCS de " +
                    demandeurCompany.City + ", dont le siège social est " + String.Format("{0} {1} {2} {3} {4}, {5}", demandeurCompany.Address1,
                    String.IsNullOrWhiteSpace(demandeurCompany.Address2) ? "" : ", " + demandeurCompany.Address2,
                     String.IsNullOrWhiteSpace(demandeurCompany.Address3) ? "" : ", " + demandeurCompany.Address3,
                     demandeurCompany.PostalCode, demandeurCompany.City,
                     demandeurCompany.Country
                    ) + ", demandeur,");

                if (demandeur.IsRepresented)
                {
                    var avocatDemandeur = conflict.UsersInConflicts.Where(c => c.IdUser == demandeur.IdLawyer).First();
                    writer.AddTag("p", "Assisté par Me " + avocatDemandeur.User.DisplayName + ", Avocat au barreau de " + avocatDemandeur.User.BusinessFunction + ".");
                }

                writer.AddTag("p", "Et :", HtmlTextWriterDefaults.DefaulText_Bold);
                var defendeurCompany = defendeur.UserCompany.Company;
                writer.AddTag("p", defendeurCompany.Name + ", " + defendeurCompany.Forme + " de droit " + defendeurCompany.Country +
                    ", au capital de " + defendeurCompany.Capital + " euros, immatriculée sous le numéro " + defendeurCompany.Siret + " au RCS de " +
                    defendeurCompany.City + ", dont le siège social est " + String.Format("{0} {1} {2} {3} {4}, {5}", defendeurCompany.Address1,
                    String.IsNullOrWhiteSpace(defendeurCompany.Address2) ? "" : ", " + defendeurCompany.Address2,
                     String.IsNullOrWhiteSpace(defendeurCompany.Address3) ? "" : ", " + defendeurCompany.Address3,
                     defendeurCompany.PostalCode, defendeurCompany.City,
                     defendeurCompany.Country
                    ) + ", défendeur,");

                writer.BreakLine();
                writer.BreakLine();
                writer.BreakLine();

                writer.AddTag("p", "TABLE DES MATIERES", HtmlTextWriterDefaults.H2_Centered);

                writer.AddTag("p", "I.   Faits et introduction de l'arbitrage", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "II.  Mise en œuvre de l'arbitrage et procédure", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "III. Position des parties", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "IV.  Motivation", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "V.   Décision", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.BreakLine();
                writer.BreakLine();
                writer.BreakLine();

                if (conflict.HasArbitralClause)
                {
                    writer.AddTag("p", "Vu la clause compromissoire,", HtmlTextWriterDefaults.DefaultText_Italic);
                }
                else
                {
                    writer.AddTag("p", " Vu le compromis d'arbitrage,", HtmlTextWriterDefaults.DefaultText_Italic);
                }
                writer.AddTag("p", "Vu le règlement du Centre d’arbitrage,", HtmlTextWriterDefaults.DefaultText_Italic);
                var missionOrder = BLLLegalDocuments.GetLastDocumentForType(conflict.Id, (int)LegalDocumentTypeEnum.MissionAct);
                if (missionOrder != null && missionOrder.CreateDate != null && missionOrder.CreateDate.Value != null)
                {
                    writer.AddTag("p", "Vu l’acte de mission du " + missionOrder.CreateDate.Value.ToShortDateString() + ",", HtmlTextWriterDefaults.DefaultText_Italic);
                }
                writer.AddTag("p", "Vu la déclaration d’acceptation et d’indépendance du " + conflict.Events.First(c => c.Name == "Déclaration d'acceptation et d'indépendance de l'arbitre").DateBegin.ToShortDateString() + ",", HtmlTextWriterDefaults.DefaultText_Italic);
                var ordonnance = BLLLegalDocuments.GetDocumentsForType(conflict.Id, (int)LegalDocumentTypeEnum.OrdonnanceProcedure);
                if (ordonnance != null && ordonnance.Count > 0)
                {
                    foreach (var item in ordonnance)
                    {
                        writer.AddTag("p", "Vu l’ordonnance de procédure du " + item.CreateDate.Value.ToShortDateString() + ",", HtmlTextWriterDefaults.DefaultText_Italic);
                    }
                }
                writer.AddTag("p", "Vu les arguments et pièces des parties tels qu’exposés sur la plateforme en ligne,", HtmlTextWriterDefaults.DefaultText_Italic);

                writer.AddTag("p", "Après avoir laissé le temps nécessaire aux parties pour exposer en fait et en droit leurs arguments respectifs, la juridiction arbitrale décide :");

                writer.AddTag("p", "I.   Faits et introduction de l'arbitrage", HtmlTextWriterDefaults.DefaulText_Bold);


                writer.AddTag("p", "II.  Mise en œuvre de l'arbitrage et procédure", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.AddTag("p", "a)	L'arbitre", HtmlTextWriterDefaults.DefaultText_WithLeftPadding);
                writer.AddTag("p", "Conformément au Règlement d’arbitrage, " + conflict.Arbiter.DisplayName + " a été désigné en qualité d’arbitre, lequel a déclaré ne pas se trouver en situation de conflit d’intérêts et a accepté sa mission.");
                writer.AddTag("p", "Le Centre d’arbitrage a ensuite notifié aux parties la désignation de l'arbitre et fixé la provision pour les frais administratifs et honoraires dus à l'arbitre.");
                writer.AddTag("p", "Cette provision a été réglée comme indiqué en annexe.");

                writer.AddTag("p", "b)	La procédure", HtmlTextWriterDefaults.DefaultText_WithLeftPadding);
                writer.AddTag("p", "L'arbitrage a lieu à Paris, sur la plateforme en ligne de l’Idam. La langue de l'arbitrage est le français. L'arbitrage est soumis au droit français. La procédure arbitrale est réglée par les arbitres conformément au Règlement d’arbitrage. L’arbitrage ne donne pas lieu à une audience. La sentence est exécutoire par provision. Le point de départ de l'arbitrage est fixé à la de signature de l'acte de mission.");
                writer.AddTag("p", "Les parties ont exposé leurs arguments en fait et en droit sur la plateforme. Conformément au Règlement d’arbitrage, l’arbitrage s’est déroulé sans audience.");
                writer.AddTag("p", "Aucune difficulté ni aucune exception ou contestation relative aux écritures et pièces n’a été soulevée.");

                writer.AddTag("p", "III. Position des parties", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.AddTag("p", "Il résulte des discussions sur la plateforme au jour de la présente sentence qu’il n’existe pas de contestation des parties concernant les points suivants :");
                foreach (var item in conflict.Events.Where(c => c.Disagreements == null || c.Disagreements.Count == 0 && (c.Type == (int)EventTypeEnum.Event || c.Type == (int)EventTypeEnum.Resolution)))
                {
                    writer.AddTag("p", item.Name, HtmlTextWriterDefaults.DefaulText_Bold);
                    writer.AddTag("p", item.Description);
                }

                writer.AddTag("p", "En revanche, les parties sont en désaccord concernant les points suivants : ");
                foreach (var item in conflict.Events.Where(c => c.Disagreements != null && c.Disagreements.Count > 0 && (c.Type == (int)EventTypeEnum.Event || c.Type == (int)EventTypeEnum.Resolution)))
                {
                    writer.AddTag("p", item.Name, HtmlTextWriterDefaults.DefaulText_Bold);
                    writer.AddTag("p", item.Description);
                    if (item.Disagreements.First().DisagreementOnDate)
                    {
                        writer.AddTag("p", "Désaccord sur la date :", HtmlTextWriterDefaults.DefaulText_Bold);
                        writer.AddTag("p", item.Disagreements.First().ConcurrentDate.Value.ToShortDateString() + " au lieu de " + item.DateBegin.ToShortDateString());
                    }
                    if (item.Disagreements.First().DisagreementOnDescription)
                    {
                        writer.AddTag("p", "Désaccord sur le fait lui-même :", HtmlTextWriterDefaults.DefaulText_Bold);
                        writer.AddTag("p", item.Disagreements.First().ConcurrentDescription);
                    }
                    if (item.Disagreements.First().DisagreementOnPiece)
                    {
                        writer.AddTag("p", "Désaccord sur une ou plusieurs pièces :", HtmlTextWriterDefaults.DefaulText_Bold);
                        writer.AddTag("p", item.Disagreements.First().ConcurrentPieces);
                    }

                }
                writer.BreakLine();

                writer.AddTag("p", "IV. Motivation", HtmlTextWriterDefaults.DefaulText_Bold);

                writer.AddTag("p", "Demandes de la société 1 : ", HtmlTextWriterDefaults.DefaultText_Underlined);
                foreach (var item in conflict.Resolutions.Where(c => c.IdUser == conflict.IdCreationUser))
                {
                    writer.AddTag("p", item.Name, HtmlTextWriterDefaults.DefaulText_Bold);
                    writer.AddTag("p", item.ResolutionComment);
                    if (item.ProofFiles != null && item.ProofFiles.Count > 0)
                    {
                        writer.AddTag("p", "Elle a pour cela fourni le ou les documents suivant :");
                        foreach (var file in item.ProofFiles)
                        {
                            writer.AddLink(file.Name, file.FilePath);
                        }
                    }
                }

                if (conflict.Resolutions.Any(c => c.IdUser != conflict.IdCreationUser))
                {
                    writer.AddTag("p", "Demande reconventionnelle de la société 2 :", HtmlTextWriterDefaults.DefaultText_Underlined);
                    foreach (var item in conflict.Resolutions.Where(c => c.IdUser != conflict.IdCreationUser))
                    {
                        writer.AddTag("p", item.Name, HtmlTextWriterDefaults.DefaulText_Bold);
                        writer.AddTag("p", item.ResolutionComment);
                        if (item.ProofFiles != null && item.ProofFiles.Count > 0)
                        {
                            writer.AddTag("p", "Elle a pour cela fourni le ou les documents suivant :");
                            foreach (var file in item.ProofFiles)
                            {
                                writer.AddLink(file.Name, file.FilePath);
                            }
                        }
                    }
                }

                writer.AddTag("p", "Frais de l’arbitrage et autres demandes accessoires :", HtmlTextWriterDefaults.DefaultText_Underlined);
                writer.AddTag("p", "Une consignation a été versée pour couvrir les frais d'arbitrage comprenant les frais administratifs et les honoraires de l'arbitre, d'un montant de " + ConflictCost.GetConflictCost(conflict.ConflictType) + " euros HT.");

                writer.AddTag("p", "Chaque partie a sollicité la condamnation de l'autre à payer le total des frais du présent arbitrage. Le règlement d'arbitrage prévoit que l'arbitre liquide les frais d'arbitrage et en répartit la charge.");



                writer.AddTODO("[Motivation de la répartition des frais d'arbitrage].");


                writer.AddTag("p", "Demandes au titre des honoraires d'avocat et débours des parties :", HtmlTextWriterDefaults.DefaultText_Underlined);

                writer.AddTODO("La société 1 évalue ses frais à la somme de [montant] euros.");
                writer.AddTODO("La société 2 évalue ses frais à la somme de [montant] euros.");
                writer.AddTODO("[Motivation de la répartition des honoraires d'avocat et débours des parties].");


                writer.AddTag("p", "V. Décision", HtmlTextWriterDefaults.DefaulText_Bold);
                writer.AddTag("p", "Pour l'ensemble des motifs développés ci-avant, par sentence arbitrale contradictoire prononcée en dernier ressort :");
                writer.AddTODO("[Décision de l arbitre].");

                writer.BreakLine();
                writer.BreakLine();
                writer.AddTag("p", "Les parties s'engagent à exécuter fidèlement et intégralement la sentence. La partie qui refuserait de l'exécuter prendra à sa charge tous les frais et droits auxquels la poursuite et l'exécution de ladite sentence donnera lieu.");
                writer.BreakLine();
                writer.BreakLine();


                writer.AddTag("p", "Fait à Paris, le " + DateTime.Now.ToShortDateString());
                writer.AddTag("p", conflict.Arbiter.DisplayName);

                ClickNClaim.WebPortal.Tools.HtmlToDocx.SaveDOCX(fileStream, wr.ToString(), true);
                fileStream.Seek(0, SeekOrigin.Begin);

            }
            return new FileStreamed(filename, fileStream);
        }


        public static FileStreamed GenerateMissionOrder(int conflictId)
        {
            var conflict = BLLConflicts.GetConflictWithUsers(conflictId);

            var filename = "Acte_de_mission_" + conflict.Id + ".pdf";


            var fileStream = new MemoryStream();

            var demandeur = conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser);
            if (demandeur.IsLawyer != null && demandeur.IsLawyer.Value)
            {
                demandeur = conflict.UsersInConflicts.First(c => c.IdLawyer == conflict.IdCreationUser);
            }
            var defendeur = conflict.UsersInConflicts.First(c => (c.IsLawyer == null || !c.IsLawyer.Value) && c.IdUser != demandeur.IdUser);


            MemoryStream ms = new MemoryStream();
            StringWriter wr = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(wr))
            {
                StringWriter wwr = new StringWriter();
                using (HtmlTextWriter titleWriter = new HtmlTextWriter(wwr))
                {
                    titleWriter.AddTag("p", "ACTE DE MISSION", HtmlTextWriterDefaults.CenteredTitle);
                    titleWriter.AddTag("p", "Litige n° " + conflict.Id, HtmlTextWriterDefaults.CenteredSubTitleInfo);
                    titleWriter.AddTag("p", (demandeur.CompanyName ?? demandeur.User.DisplayName) + " c/ " + (defendeur.CompanyName ?? defendeur.User.DisplayName), HtmlTextWriterDefaults.CenteredSubTitleInfo);
                    titleWriter.AddTag("p", " ");
                    writer.TitleBox(wwr.ToString());
                }
                wwr = null;

                writer.AddTag("p","ARTICLE1. PARTIE ", HtmlTextWriterDefaults.MissionActTitle);

                writer.AddTag("p", demandeur.User.DisplayName);
                writer.AddTag("p", "RCS : " + (demandeur.UserCompany != null && demandeur.UserCompany.Company != null ? (demandeur.UserCompany.Company.Siret ?? "Inconnu") : "Inconnu"));

                writer.AddTag("p", "c/");

                writer.AddTag("p", defendeur.User.DisplayName);
                writer.AddTag("p", "RCS : " + (defendeur.UserCompany != null && defendeur.UserCompany.Company != null ? (defendeur.UserCompany.Company.Siret ?? "Inconnu") : "Inconnu"));

                writer.AddTag("p", "LITIGE", HtmlTextWriterDefaults.MissionActTitle);
                writer.AddTag("p", "Le Litige est celui désigné sur la Plateforme sous le n° " + conflictId + ".");

                writer.AddTag("p", "TRIBUNAL ARBITRAL", HtmlTextWriterDefaults.MissionActTitle);
                writer.AddTag("p", "Le Centre désigne l’Arbitre ci-après pour trancher le Litige entre les Parties conformément au Règlement d’arbitrage de l’Idam :  ");

                writer.AddTag("p", conflict.Arbiter.DisplayName, HtmlTextWriterDefaults.DefaulText_Bold);

                writer.BreakLine();
                writer.AddTag("p", "Fait à Paris, le " + DateTime.Now.ToShortDateString());
                writer.AddTag("p", "Emmanuel Mouclier");
                writer.AddTag("p", "Président de l’Institut digital d’arbitrage et de médiation. ");

                using (PdfDocument pdf = PdfGenerator.GeneratePdf(wr.ToString(), PageSize.A4))
                {
                    pdf.Save(ms, false);
                }
            }
            return new FileStreamed(filename, ms);
        }




        public static FileStreamed GenerateBodaccReport(string siren)
        {
            BodaccReader.BodaccReader br = new BodaccReader.BodaccReader();
            var elts = br.GetBodacc(siren.Replace(" ", ""));
            if (elts != null)
            {
                var filename = "Rapport_BODACC_SIREN_" + siren + ".pdf";
                MemoryStream ms = new MemoryStream();
                StringWriter wr = new StringWriter();
                using (HtmlTextWriter writer = new HtmlTextWriter(wr))
                {
                    writer.AddTag("h1", "Rapport des annonces BODACC", HtmlTextWriterDefaults.H1);
                    writer.BreakLine();

                    foreach (notice item in elts)
                    {
                        writer.AddTag("h3", "Annonce N°" + item.content.numeroAnnonce, HtmlTextWriterDefaults.H3);
                        writer.AddTag("i", "Date de publication : " + item.publication_date, HtmlTextWriterDefaults.DefaultText);
                        writer.BreakLine();
                        writer.AddTag("i", "Tribunal : " + item.content.tribunal, HtmlTextWriterDefaults.DefaultText);
                        if (!String.IsNullOrWhiteSpace(item.content.denomination))
                            writer.AddTag("p", "Dénomination : " + item.content.denomination, new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "15px", FontWeight = "bold" });

                        if (!String.IsNullOrWhiteSpace(item.content.formeJuridique))
                        {
                            writer.AddTag("i", "forme juridique : " + item.content.formeJuridique, HtmlTextWriterDefaults.DefaultText);
                        }


                        if (item.content.depot != null)
                        {
                            writer.AddTag("p", String.Format("Dépôt : {0} (cloture le {1})", item.content.depot.typeDepot, item.content.depot.dateCloture), HtmlTextWriterDefaults.DefaultText);
                            if (!String.IsNullOrWhiteSpace(item.content.depot.descriptif))
                                writer.AddTag("p", item.content.depot.descriptif, HtmlTextWriterDefaults.DefaultText);
                        }

                        if (item.content.numeroImmatriculation != null)
                        {
                            writer.AddTag("p", String.Format("{0} {1}",
                                  item.content.numeroImmatriculation.codeRCS,
                                  item.content.numeroImmatriculation.numeroIdentificationRCS), HtmlTextWriterDefaults.DefaultText);
                            writer.AddTag("p", String.Format("Greffe : {0}", item.content.numeroImmatriculation.nomGreffeImmat), HtmlTextWriterDefaults.DefaultText);

                        }

                        if (item.content.personnes != null)
                        {
                            #region SIEGE SOCIAL
                            if (item.content.personnes.personne.siegeSocial != null)
                            {
                                writer.BreakLine();
                                writer.AddTag("p", "Siège social", HtmlTextWriterDefaults.H3);
                                if (item.content.personnes.personne.siegeSocial.france != null)
                                {
                                    writer.AddTag("p", String.Format("{0}, {1} {2}",
                                        item.content.personnes.personne.siegeSocial.france.numeroVoie,
                                        item.content.personnes.personne.siegeSocial.france.typeVoie,
                                        item.content.personnes.personne.siegeSocial.france.nomVoie), HtmlTextWriterDefaults.DefaultText);
                                    writer.AddTag("p", String.Format("{0} {1}",
                                        item.content.personnes.personne.siegeSocial.france.codePostal,
                                        item.content.personnes.personne.siegeSocial.france.ville), HtmlTextWriterDefaults.DefaultText);
                                }
                                else if (item.content.personnes.personne.siegeSocial.etranger != null)
                                {
                                    writer.AddTag("p", item.content.personnes.personne.siegeSocial.etranger.adresse, HtmlTextWriterDefaults.DefaultText);
                                    writer.AddTag("p", item.content.personnes.personne.siegeSocial.etranger.pays, HtmlTextWriterDefaults.DefaultText);
                                }
                            }
                            #endregion

                            #region PERSONNE MORALE
                            if (item.content.personnes.personne.personneMorale != null)
                            {
                                writer.AddTag("p", "PERSONNE MORALE", HtmlTextWriterDefaults.DefaultText);
                                writer.AddTag("p", item.content.personnes.personne.personneMorale.denomination, HtmlTextWriterDefaults.H2);
                                writer.AddTag("p", item.content.personnes.personne.personneMorale.formeJuridique, HtmlTextWriterDefaults.DefaultText);
                                writer.AddTag("p", item.content.personnes.personne.personneMorale.administration, HtmlTextWriterDefaults.DefaultText);

                                if (item.content.personnes.personne.personneMorale.capital != null)
                                {
                                    writer.AddTag("p", "Au capital de " + item.content.personnes.personne.personneMorale.capital.montantCapital
                                                                        + " " + item.content.personnes.personne.personneMorale.capital.devise,
                                                                        HtmlTextWriterDefaults.DefaultText);
                                }

                                if (item.content.personnes.personne.personneMorale.numeroImmatriculation != null)
                                {
                                    writer.AddTag("p", String.Format("{0} {1}",
                                        item.content.personnes.personne.personneMorale.numeroImmatriculation.codeRCS,
                                        item.content.personnes.personne.personneMorale.numeroImmatriculation.numeroIdentificationRCS), HtmlTextWriterDefaults.DefaultText);
                                    writer.AddTag("p", String.Format("Greffe : {0}", item.content.personnes.personne.personneMorale.numeroImmatriculation.nomGreffeImmat), HtmlTextWriterDefaults.DefaultText);
                                }

                            }
                            #endregion

                            #region PERSONNE PHYSIQUE
                            if (item.content.personnes.personne.personnePhysique != null)
                            {
                                writer.AddTag("p", "PERSONNE PHYSIQUE", HtmlTextWriterDefaults.DefaultText);
                                writer.AddTag("p", "prénom : " + item.content.personnes.personne.personnePhysique.prenom, HtmlTextWriterDefaults.DefaultText);
                                writer.AddTag("p", "nom : " + item.content.personnes.personne.personnePhysique.nom, HtmlTextWriterDefaults.DefaultText);
                                if (!string.IsNullOrWhiteSpace(item.content.personnes.personne.personnePhysique.nomCommercial))
                                    writer.AddTag("p", "nom commercial : " + item.content.personnes.personne.personnePhysique.nomCommercial, HtmlTextWriterDefaults.DefaultText);
                                if (!string.IsNullOrWhiteSpace(item.content.personnes.personne.personnePhysique.pseudonyme))
                                    writer.AddTag("p", "pseudonyme : " + item.content.personnes.personne.personnePhysique.pseudonyme, HtmlTextWriterDefaults.DefaultText);
                                if (!string.IsNullOrWhiteSpace(item.content.personnes.personne.personnePhysique.nationalite))
                                    writer.AddTag("p", "nationalité : " + item.content.personnes.personne.personnePhysique.nationalite, HtmlTextWriterDefaults.DefaultText);
                                if (item.content.personnes.personne.personnePhysique.numeroImmatriculation != null)
                                {
                                    writer.AddTag("p", String.Format("{0} {1}",
                                        item.content.personnes.personne.personnePhysique.numeroImmatriculation.codeRCS,
                                        item.content.personnes.personne.personnePhysique.numeroImmatriculation.numeroIdentificationRCS), HtmlTextWriterDefaults.DefaultText);
                                    writer.AddTag("p", String.Format("Greffe : {0}", item.content.personnes.personne.personnePhysique.numeroImmatriculation.nomGreffeImmat), HtmlTextWriterDefaults.DefaultText);
                                }

                            }
                            #endregion

                            if (item.content.personnes.personne.numeroImmatriculation != null)
                            {
                                writer.AddTag("p", String.Format("{0} {1}",
                                      item.content.personnes.personne.numeroImmatriculation.codeRCS,
                                      item.content.personnes.personne.numeroImmatriculation.numeroIdentificationRCS), HtmlTextWriterDefaults.DefaultText);
                                writer.AddTag("p", String.Format("Greffe : {0}", item.content.personnes.personne.numeroImmatriculation.nomGreffeImmat), HtmlTextWriterDefaults.DefaultText);

                            }

                        }

                        writer.HLine();
                    }
                    using (PdfDocument pdf = PdfGenerator.GeneratePdf(wr.ToString(), PageSize.A4))
                    {
                        pdf.Save(ms, false);
                    }

                }

                //  return wr.ToString();
                return new FileStreamed(filename, ms);
            }
            return null;
        }

        public static FileStreamed GenerateBodaccReport(List<notice> notices)
        {
            var filename = "Rapport_BODACC_{0}.pdf";
            string naming = null;
            string rcs = null;
            MemoryStream ms = new MemoryStream();
            StringWriter wr = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(wr))
            {
                writer.AddTag("h1", "Rapport des annonces BODACC", HtmlTextWriterDefaults.H1);
                writer.BreakLine();

                foreach (notice item in notices)
                {
                    writer.AddTag("h3", "Annonce N°" + item.content.numeroAnnonce, HtmlTextWriterDefaults.H3);
                    writer.AddTag("i", "Date de publication : " + item.publication_date, HtmlTextWriterDefaults.DefaultText);
                    writer.BreakLine();
                    writer.AddTag("i", "Tribunal : " + item.content.tribunal, HtmlTextWriterDefaults.DefaultText);
                    if (!String.IsNullOrWhiteSpace(item.content.denomination))
                    {
                        writer.AddTag("p", "Dénomination : " + item.content.denomination, new HtmlTextWriterOptions() { FontFamily = "Arial", FontSize = "15px", FontWeight = "bold" });
                        naming = item.content.denomination;
                    }
                    if (!String.IsNullOrWhiteSpace(item.content.formeJuridique))
                    {
                        writer.AddTag("i", "forme juridique : " + item.content.formeJuridique, HtmlTextWriterDefaults.DefaultText);
                    }


                    if (item.content.depot != null)
                    {
                        writer.AddTag("p", String.Format("Dépôt : {0} (cloture le {1})", item.content.depot.typeDepot, item.content.depot.dateCloture), HtmlTextWriterDefaults.DefaultText);
                        if (!String.IsNullOrWhiteSpace(item.content.depot.descriptif))
                            writer.AddTag("p", item.content.depot.descriptif, HtmlTextWriterDefaults.DefaultText);
                    }

                    if (item.content.numeroImmatriculation != null)
                    {
                        writer.AddTag("p", String.Format("{0} {1}",
                              item.content.numeroImmatriculation.codeRCS,
                              item.content.numeroImmatriculation.numeroIdentificationRCS), HtmlTextWriterDefaults.DefaultText);
                        writer.AddTag("p", String.Format("Greffe : {0}", item.content.numeroImmatriculation.nomGreffeImmat), HtmlTextWriterDefaults.DefaultText);
                        rcs = item.content.numeroImmatriculation.numeroIdentificationRCS;
                    }

                    if (item.content.personnes != null)
                    {
                        #region SIEGE SOCIAL
                        if (item.content.personnes.personne.siegeSocial != null)
                        {
                            writer.BreakLine();
                            writer.AddTag("p", "Siège social", HtmlTextWriterDefaults.H3);
                            if (item.content.personnes.personne.siegeSocial.france != null)
                            {
                                writer.AddTag("p", String.Format("{0}, {1} {2}",
                                    item.content.personnes.personne.siegeSocial.france.numeroVoie,
                                    item.content.personnes.personne.siegeSocial.france.typeVoie,
                                    item.content.personnes.personne.siegeSocial.france.nomVoie), HtmlTextWriterDefaults.DefaultText);
                                writer.AddTag("p", String.Format("{0} {1}",
                                    item.content.personnes.personne.siegeSocial.france.codePostal,
                                    item.content.personnes.personne.siegeSocial.france.ville), HtmlTextWriterDefaults.DefaultText);
                            }
                            else if (item.content.personnes.personne.siegeSocial.etranger != null)
                            {
                                writer.AddTag("p", item.content.personnes.personne.siegeSocial.etranger.adresse, HtmlTextWriterDefaults.DefaultText);
                                writer.AddTag("p", item.content.personnes.personne.siegeSocial.etranger.pays, HtmlTextWriterDefaults.DefaultText);
                            }
                        }
                        #endregion

                        #region PERSONNE MORALE
                        if (item.content.personnes.personne.personneMorale != null)
                        {
                            writer.AddTag("p", "PERSONNE MORALE", HtmlTextWriterDefaults.DefaultText);
                            writer.AddTag("p", item.content.personnes.personne.personneMorale.denomination, HtmlTextWriterDefaults.H2);

                            naming = naming ?? item.content.personnes.personne.personneMorale.denomination;
                            writer.AddTag("p", item.content.personnes.personne.personneMorale.formeJuridique, HtmlTextWriterDefaults.DefaultText);
                            writer.AddTag("p", item.content.personnes.personne.personneMorale.administration, HtmlTextWriterDefaults.DefaultText);

                            if (item.content.personnes.personne.personneMorale.capital != null)
                            {
                                writer.AddTag("p", "Au capital de " + item.content.personnes.personne.personneMorale.capital.montantCapital
                                                                    + " " + item.content.personnes.personne.personneMorale.capital.devise,
                                                                    HtmlTextWriterDefaults.DefaultText);
                            }

                            if (item.content.personnes.personne.personneMorale.numeroImmatriculation != null)
                            {
                                writer.AddTag("p", String.Format("{0} {1}",
                                    item.content.personnes.personne.personneMorale.numeroImmatriculation.codeRCS,
                                    item.content.personnes.personne.personneMorale.numeroImmatriculation.numeroIdentificationRCS), HtmlTextWriterDefaults.DefaultText);
                                rcs = rcs ?? item.content.personnes.personne.personneMorale.numeroImmatriculation.numeroIdentificationRCS;
                                writer.AddTag("p", String.Format("Greffe : {0}", item.content.personnes.personne.personneMorale.numeroImmatriculation.nomGreffeImmat), HtmlTextWriterDefaults.DefaultText);
                            }

                        }
                        #endregion

                        #region PERSONNE PHYSIQUE
                        if (item.content.personnes.personne.personnePhysique != null)
                        {
                            writer.AddTag("p", "PERSONNE PHYSIQUE", HtmlTextWriterDefaults.DefaultText);
                            writer.AddTag("p", "prénom : " + item.content.personnes.personne.personnePhysique.prenom, HtmlTextWriterDefaults.DefaultText);
                            writer.AddTag("p", "nom : " + item.content.personnes.personne.personnePhysique.nom, HtmlTextWriterDefaults.DefaultText);
                            if (!string.IsNullOrWhiteSpace(item.content.personnes.personne.personnePhysique.nomCommercial))
                            {
                                writer.AddTag("p", "nom commercial : " + item.content.personnes.personne.personnePhysique.nomCommercial, HtmlTextWriterDefaults.DefaultText);
                                naming = naming ?? item.content.personnes.personne.personnePhysique.nomCommercial;
                            }
                            if (!string.IsNullOrWhiteSpace(item.content.personnes.personne.personnePhysique.pseudonyme))
                                writer.AddTag("p", "pseudonyme : " + item.content.personnes.personne.personnePhysique.pseudonyme, HtmlTextWriterDefaults.DefaultText);
                            if (!string.IsNullOrWhiteSpace(item.content.personnes.personne.personnePhysique.nationalite))
                                writer.AddTag("p", "nationalité : " + item.content.personnes.personne.personnePhysique.nationalite, HtmlTextWriterDefaults.DefaultText);
                            if (item.content.personnes.personne.personnePhysique.numeroImmatriculation != null)
                            {
                                writer.AddTag("p", String.Format("{0} {1}",
                                    item.content.personnes.personne.personnePhysique.numeroImmatriculation.codeRCS,
                                    item.content.personnes.personne.personnePhysique.numeroImmatriculation.numeroIdentificationRCS), HtmlTextWriterDefaults.DefaultText);
                                writer.AddTag("p", String.Format("Greffe : {0}", item.content.personnes.personne.personnePhysique.numeroImmatriculation.nomGreffeImmat), HtmlTextWriterDefaults.DefaultText);
                                rcs = rcs ?? item.content.personnes.personne.personnePhysique.numeroImmatriculation.numeroIdentificationRCS;
                            }

                        }
                        #endregion

                        if (item.content.personnes.personne.numeroImmatriculation != null)
                        {
                            writer.AddTag("p", String.Format("{0} {1}",
                                  item.content.personnes.personne.numeroImmatriculation.codeRCS,
                                  item.content.personnes.personne.numeroImmatriculation.numeroIdentificationRCS), HtmlTextWriterDefaults.DefaultText);
                            writer.AddTag("p", String.Format("Greffe : {0}", item.content.personnes.personne.numeroImmatriculation.nomGreffeImmat), HtmlTextWriterDefaults.DefaultText);
                            rcs = rcs ?? item.content.personnes.personne.numeroImmatriculation.numeroIdentificationRCS;
                        }

                    }

                    writer.HLine();
                }
                using (PdfDocument pdf = PdfGenerator.GeneratePdf(wr.ToString(), PageSize.A4))
                {
                    pdf.Save(ms, false);
                }

            }

            //  return wr.ToString();
            return new FileStreamed(filename, ms);
        }
    }
}