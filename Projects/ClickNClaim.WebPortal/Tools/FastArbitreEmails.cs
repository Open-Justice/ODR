using ClickNClaim.Business;
using ClickNClaim.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickNClaim.WebPortal.Tools
{
    public class FastArbitreEmails
    {

        public static void CaseRefused(string to, string text, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            var user = BLLUsers.GetUserByEmail(to);

            replacements.Add("|*REASON*|", text);
            replacements.Add("|*EMAILLINK*|",  emailUrl);
            replacements.Add("|*TO*|", user != null ? user.DisplayName : to);

            MailSender.SendMessage(to, "[Fast Arbitre] Refus de demande d'arbitrage", MailSender.GetHtmlAndReplaceData("~/Emails/Dossier.Refus.html", replacements),guid);
        }

        public static void ArbitreRefuseAssignation(string text, int conflictId, string arbiterName, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

         

            replacements.Add("|*REASON*|", text);
            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*ARBITRE*|", arbiterName);
            replacements.Add("|*REF*|", conflictId.ToString());

            MailSender.SendMessage("contact@fast-arbitre.com", "[Fast Arbitre] Refus par l'arbitre de l'assignation", MailSender.GetHtmlAndReplaceData("~/Emails/Assignation.Refus.html", replacements), guid);

        }

        public static void NewMissionOrder(string to, string fileUri, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();
         
            replacements.Add("|*EMAILLINK*|",emailUrl);
            replacements.Add("|*TITLE*|", "Nouvel ordre de mission");
            replacements.Add("|*TEXT*|", "Bonjour,<br/> Fast Arbitre vous a attribué un nouveau dossier. Vous trouverez "+"<a href='" + fileUri + "'>l'ordre de mission s’y référant "+
                "ici</a>. Afin de valider votre ordre de mission, merci de vous connecter sur la plateforme FAST ARBITRE. "+
                "Dans la liste de \"Vos Dossiers\", veuillez cliquez sur \"Accepter\" si vous acceptez cette mission.");

            MailSender.SendMessage(to, "[Fast Arbitre] Nouvel ordre de mission", MailSender.GetHtmlAndReplaceData("~/Emails/Simple.EMMO.html", replacements), guid);

        }

        public static void YouHaveMail(string to, Debate debate, string debateUri, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();
            
            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Vous avez reçu un nouveau mail au sujet de : " + debate.Title);
            replacements.Add("|*TEXT*|", "Bonjour,<br/>vous retrouverez"+ " <a href='" + debateUri + "' >" + "le nouveau mail au sujet de " + debate.Title +
                @" ici </a>");

            MailSender.SendMessage(to, "[Fast Arbitre] Vous avez reçu un nouveau mail au sujet de : " + debate.Title,
                MailSender.GetHtmlAndReplaceData("~/Emails/Template.Simple.html", replacements), guid);

        }

        public static void VisioConferencePlanned(string to, DateTime? rdv, string visioTitle, 
            string visioDescription, string visioLink, string login, string password, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();
          
            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "[FastArbitre] Visio conférence le " + rdv.Value.ToShortDateString() + " à " + rdv.Value.ToShortTimeString());
            replacements.Add("|*TEXT*|", "Bonjour,<br/>vous avez une visio-conférence prévue le " + rdv.Value.ToShortDateString() + " à " + 
                rdv.Value.ToShortTimeString() + " organisée par votre arbitre.<br/> Cette visio-conférence portera sur " + visioTitle + 
                ".<br/>Description:" + visioDescription + "<br/>Pour vous connecter à la visio-conférence, veuillez cliquer sur ce <a href='" + visioLink 
                + "'>lien</a> et utiliser les loginv et mot de passe suivants :" +
                "<br/>Login : " + login +
                "<br/>Mot de passe : " + password);


            MailSender.SendMessage(to, "[FastArbitre] Visio conférence le " + rdv.Value.ToShortDateString() + " à " + rdv.Value.ToShortTimeString(),
                MailSender.GetHtmlAndReplaceData("~/Emails/Template.Simple.html", replacements), guid);
        }


        //public static void AccountCreated(string to, string name, string pwd, string emailUrl, Guid guid)
        //{
        //    var replacements = new Dictionary<string, string>();
          
        //    replacements.Add("|*EMAILLINK*|", emailUrl);
        //    replacements.Add("|*TITLE*|", "Bienvenu sur  Fast-Arbitre.com");

        //    var str = "<p> Cher(e) *|FNAME|*, <br />Votre mot de passe est le suivant : *|PWD|*<br />Vous pouvez le ré-initialiser à n'importe quel "+
        //        "moment en cliquant sur le lien ci-dessous : </p><a href = \"http://clicknclaim.azurewebsites.net/Account/ResetPassword\" > "+
        //        "Ré-initialiser mon mot de passe </a>".Replace("*|FNAME|*", name).Replace("*|PWD|*", pwd);
        //    replacements.Add("|*TEXT*|", str);

        //    Tools.MailSender.SendMessage(to,
        //                "[FastArbitre] Bienvenue chez FastArbitre",
        //                MailSender.GetHtmlAndReplaceData("~/Emails/welcome.html", replacements), guid);


        //}

        public static void AccountCreatedForUserByLawyer(string to, string password, string url, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();
           
            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Bienvenue chez FastArbitre!");
            replacements.Add("|*TEXT*|", "Bonjour,<br/>Votre compte FastArbitre a bien été créé. Vous pouvez d'ores et déjà vous connecter sur <a href='" + 
                url + "'>la plateforme FastArbitre</a> et suivre l'avancement de votre conflit." +
                       "Pour vous connecter, veuillez utiliser l'identifiant suivant : <br/>" +
                       "Login: " + to + "<br/>" +
                       "Mot de passe: " + password + "<br/>");

            MailSender.SendMessage(to, "[FastArbitre] Bienvenue chez FastArbitre!", MailSender.GetHtmlAndReplaceData("~/Emails/Template.Simple.html", replacements), guid);
        }

        public static  void LawyerStartedCase(string to, string lawyerName, int conflictId, string url, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            var user = BLLUsers.GetUserByEmail(to);
            var conflict = BLLConflicts.GetConflict(conflictId);

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TO*|", user != null ? user.DisplayName : to);
            replacements.Add("|*AVOCAT*|", lawyerName);
            replacements.Add("|*REFDOSSIER*|", conflictId.ToString());
            replacements.Add("|*ADVERSAIRE*|", conflict.UsersInConflicts.First(c => c.IdUser == c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*CLIENT*|", conflict.UsersInConflicts.First(c => c.IdUser != c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*URL*|",url);


            MailSender.SendMessage(to, "[FastArbitre] Me " + lawyerName + " a engagé une procédure en votre nom",
                MailSender.GetHtmlAndReplaceData("~/Emails/Proc.By.Lawyer.html", replacements), guid);

        }

        public static void LawyerCalledOnCase(string to, string lawyerName, int conflictId, string clientName, string url, string login, string password, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            var user = BLLUsers.GetUserByEmail(to);
            var conflict = BLLConflicts.GetConflict(conflictId);

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*LAWYER*|", lawyerName);
            replacements.Add("|*FROM*|", user != null ? user.DisplayName : to);
            replacements.Add("|*REFDOSSIER*|", conflictId.ToString());
            replacements.Add("|*ADVERSAIRE*|", conflict.UsersInConflicts.First(c => c.IdUser == c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*CLIENT*|", conflict.UsersInConflicts.First(c => c.IdUser != c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*LOGIN*|", login);
            replacements.Add("|*PWD*|", password);

            MailSender.SendMessage(to, "[FastArbitre] Un client vous a désigné comme avocat", MailSender.GetHtmlAndReplaceData("~/Emails/Template.Simple.html", replacements), guid);
        }

        public static void InvitationToJoinConflict(Invitation invit, Conflict conflict, string url, string name, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();
           
            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*ADVERSAIRE*|", name);
            replacements.Add("|*CLIENT*|", invit.FirstName + " " + invit.LastName);
            replacements.Add("|*REFDOSSIER*|", conflict.Id.ToString());
            replacements.Add("|*URL*|", url);
            replacements.Add("|*DECLARATION*|", conflict.UsersInConflicts.First(c => c.IdUser == conflict.IdCreationUser).UserDescriptionOfTheConflict);
            string demandes = "";
            for (int i = 0; i < conflict.Resolutions.Count; i++)
            {
                if( i > 0)
                {
                    demandes += ", ";
                }
                demandes += conflict.Resolutions.ElementAt(i).ResolutionType.Name + " (" + conflict.Resolutions.ElementAt(i).ResolutionComment + ")";
            }
            replacements.Add("|*DEMANDES*|", conflict.Resolutions != null && conflict.Resolutions.Count > 0 ? demandes : "Aucune demande exprimée");        

            MailSender.SendMessage(invit.Email, "[FastArbitre] " + name + " a formulé des demandes à votre encontre", MailSender.GetHtmlAndReplaceData("~/Emails/Invitation.html", replacements), guid);
        }

        public static void VisioProgrammed(string to,int conflictId, string url, string confTitle, MeetingProposition meetingProp, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            var conflict = BLLConflicts.GetConflict(conflictId);
            var user = BLLUsers.GetUserByEmail(to);

            replacements.Add("|*EMAILLINK*|",emailUrl);
            replacements.Add("|*REFDOSSIER*|", conflictId.ToString());
            replacements.Add("|*CLIENT*|", conflict.UsersInConflicts.First(c => c.IdUser != c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*ADVERSAIRE*|", conflict.UsersInConflicts.First(c => c.IdUser == c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*DATE*|", meetingProp.DateTimeProposition.ToLongDateString() + " à " + meetingProp.DateTimeProposition.Hour + ":" + meetingProp.DateTimeProposition.Minute);
            replacements.Add("|*URL*|", url);
            replacements.Add("|*LOGIN*|", user.FirstName.Replace(" ", ".").ToLower() + "." + user.LastName.Replace(" ", ".").ToLower());
            replacements.Add("|*PWD*|", to);

            MailSender.SendMessage(to, "[FastArbitre] Notez la date de votre visioconférence",MailSender.GetHtmlAndReplaceData("~/Emails/Visio.Dated.html", replacements), guid);

        }

        public static void VisioProgrammedForArbiter(string to, string confTitle, int conflictId, string url, MeetingProposition meetingProp, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();
     
            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Visio conférence programmée");
            replacements.Add("|*TEXT*|", "Bonjour,<br/> la date pour la visio conférence " + confTitle +
                        " du conflit <a href='"+url+ "'>N°" + conflictId + "</a> est fixée au " + meetingProp.DateTimeProposition.ToLongDateString() + 
                        " à " + meetingProp.DateTimeProposition.ToShortTimeString());

            MailSender.SendMessage(to, "[FastArbitre] Visio conférence programmée", MailSender.GetHtmlAndReplaceData("~/Emails/Template.Simple.html", replacements), guid);

        }

        public static void VisioNotProgrammable(string to, string confTitle, int conflictId, string url, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            var conflict = BLLConflicts.GetConflict(conflictId);

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*REFDOSSIER*|", conflictId.ToString());
            replacements.Add("|*CLIENT*|", conflict.UsersInConflicts.First(c => c.IdUser != c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*ADVERSAIRE*|", conflict.UsersInConflicts.First(c => c.IdUser == c.Conflict.IdCreationUser).User.DisplayName);

            replacements.Add("|*URL*|", url);

            MailSender.SendMessage(to, "[FastArbitre] Impossible de fixer une date",MailSender.GetHtmlAndReplaceData("~/Emails/Visio.NoDate.html", replacements), guid);
        }

        public static void VisioMultipleDateChoice(string to, string confTitle, int conflictId, string url, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            var conflict = BLLConflicts.GetConflict(conflictId);
            var user = BLLUsers.GetUserByEmail(to);


            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Choix de la date");
            replacements.Add("|*TO*|", user.DisplayName);
            replacements.Add("|*CLIENT*|", conflict.UsersInConflicts.First(c => c.IdUser != c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*REFDOSSIER*|", conflictId.ToString());
            replacements.Add("|*ADVERSAIRE*|", conflict.UsersInConflicts.First(c => c.IdUser == c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*URL*|", url);

            MailSender.SendMessage(to, "[FastArbitre] Choix de la date", MailSender.GetHtmlAndReplaceData("~/Emails/Visio.MultipleDate.html", replacements), guid);

        }

        public static void PreConclusion(string to,string userName, int conflictId, string url, string fileUrl, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();
            
            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Approuvez ou contestez la pré-sentence");
            replacements.Add("|*TEXT*|", "Bonjour "+userName+ ",<br/>L'arbitre en charge de votre dossier a rendu une pré-sentence. Vous en trouverez une copie ci-joint. Il s'agit d'une décision provisoire pouvant encore faire l'objet d'une contestation de votre part."+
                "<div style=\"width:100%;text-align:center;font-size:20px;font-weight:bold;\"><a href='" + url + "'>APPROUVEZ OU CONTESTEZ LA PRE-SENTENCE</a></div>");

            MailSender.SendMessage(to, "[FastArbitre]La pré-sentence de votre dossier N°" + conflictId + " est tombé!", MailSender.GetHtmlAndReplaceData("~/Emails/Simple.EMMO.html", replacements), guid);

        }

        public static void Sentence(string to, int conflictId, string url, string fileUrl, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            var user = BLLUsers.GetUserByEmail(to);
            var conflict = BLLConflicts.GetConflict(conflictId);

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TO*|", user != null ? user.DisplayName : to);
            replacements.Add("|*REFDOSSIER*|", conflictId.ToString());
            replacements.Add("|*ADVERSAIRE*|", conflict.UsersInConflicts.First(c => c.IdUser == c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*CLIENT*|", conflict.UsersInConflicts.First(c => c.IdUser != c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*URL*|", url);
            MailSender.SendMessage(to, "[FastArbitre] Mise à disposition de la sentence arbitrale", MailSender.GetHtmlAndReplaceData("~/Emails/Sentence.html", replacements), guid);

        }

        public static void ArbitrationAsked(string to, int conflictId, string url, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            var user = BLLUsers.GetUserByEmail(to);
            var conflict = BLLConflicts.GetConflict(conflictId);
            replacements.Add("|*REFDOSSIER*|", conflictId.ToString());
            replacements.Add("|*ADVERSAIRE*|", conflict.UsersInConflicts.First(c => c.IdUser == c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*CLIENT*|", conflict.UsersInConflicts.First(c => c.IdUser != c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*URL*|", url);
            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TO*|", user != null ?  user.DisplayName : to);


            MailSender.SendMessage(to, "[FastArbitre] Demande d'arbitrage", MailSender.GetHtmlAndReplaceData("~/Emails/ArbitrationAsked.html", replacements), guid);

        }

        public static void ArbitrationSentToCentre(string to, int conflictId, string url, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            var user = BLLUsers.GetUserByEmail(to);
            var conflict = BLLConflicts.GetConflict(conflictId);
            replacements.Add("|*REFDOSSIER*|", conflictId.ToString());
            replacements.Add("|*ADVERSAIRE*|", conflict.UsersInConflicts.First(c => c.IdUser == c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*CLIENT*|", conflict.UsersInConflicts.First(c => c.IdUser != c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*URL*|", url);
            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TO*|", user != null ? user.DisplayName : to);


            MailSender.SendMessage(to, "[FastArbitre] Dossier transmis au centre", MailSender.GetHtmlAndReplaceData("~/Emails/ArbitrationSentToCentre.html", replacements), guid);

        }

        public static void VisioAsked(string to,string displayName, int conflictId, string reason, string url, string refuse, string emailUrl, Guid guid)
        {

            var replacements = new Dictionary<string, string>();

            var conflict = BLLConflicts.GetConflict(conflictId);

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*REFDOSSIER*|", conflictId.ToString());
            replacements.Add("|*ADVERSAIRE*|", conflict.UsersInConflicts.First(c => c.IdUser == c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*CLIENT*|", conflict.UsersInConflicts.First(c => c.IdUser != c.Conflict.IdCreationUser).User.DisplayName);
            replacements.Add("|*PARTIE*|", displayName);
            replacements.Add("|*RAISON*|", reason);
            replacements.Add("|*ACCEPTURL*|", url);
            replacements.Add("|*REFUSURL*|", refuse);

         
            MailSender.SendMessage(to, "[FastArbitre] Demande de visioconférence d'une partie", MailSender.GetHtmlAndReplaceData("~/Emails/Visio.Asked.html", replacements), guid);
          
        }

        public static void ConfirmAccount(string to, string displayName, string confirmationUrl, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Confirmation de compte");
            replacements.Add("|*TEXT*|", "Bonjour " + displayName + ",<br/> Votre compte FastArbitre a été créé avec succès. Vous devez désormais l'activer : "+
                "<div style=\"width:100%;text-align:center;font-size:20px;font-weight:bold;\"><a href='" + confirmationUrl + "'>ACTIVER MON COMPTE</a>.</div>");


            MailSender.SendMessage(to, "[FastArbitre] Confirmation de compte", MailSender.GetHtmlAndReplaceData("~/Emails/Template.Simple.html", replacements), guid);

        }

        public static void ReinitiatingPassword(string to, string callbackUrl, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Réinitialisation de votre mot de passe");
            replacements.Add("|*TEXT*|", "Vous avez demandé à réinitialiser votre mot de passe.<br/> Merci de <a href='" + callbackUrl + "'>cliquer " +
                "ici</a> afin de commencer la procédure.");


            MailSender.SendMessage(to, "[FastArbitre] Réinitialisation de votre mot de passe", MailSender.GetHtmlAndReplaceData("~/Emails/Template.Simple.html", replacements), guid);

        }

        public static void ConfirmSendToOpponents(string to, string url, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Transmission aux parties adverses");
            replacements.Add("|*TEXT*|", "Bonjour,<br/>vous avez rempli votre dossier sur FastArbitre et nous vous en remercions.<br/>Votre dossier a maintenant été transmis aux parties adverses. Vous ne pouvez plus modifier votre déclaration jusqu'à l'engagement d'un arbitrage sur la plateforme. Vous pouvez cependant revoir le détail de votre dossier, ainsi que l'avancement de la déclaration des parties adverses en vous rendant ici :"+
                "<div style=\"width:100%;text-align:center;font-size:20px;font-weight:bold;\"><a href='" + url+ "'>Voir le résumé de votre litige</a>.</div>");


            MailSender.SendMessage(to, "[FastArbitre] Transmission aux parties adverses", MailSender.GetHtmlAndReplaceData("~/Emails/Template.Simple.html", replacements), guid);

        }

        public static void ConfirmContestationFilledIn(string to, string url, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Votre contestation est bien enregistrée");
            replacements.Add("|*TEXT*|", "Bonjour,<br/>vous avez terminé de remplir votre contestation et nous vous en remercions.<br/>Le dossier de l affaire est maintenant complet et peut être transmis au centre d'arbitrage à votre demande à tout moment." +
                "Vous ne pouvez plus modifier votre contestation jusqu'à ce que vous ayez démarré l'arbitrage de votre dossier. Vous pouvez cependant revoir le détail de votre dossier en vous rendant ici :" +
                "<div style=\"width:100%;text-align:center;font-size:20px;font-weight:bold;\"><a href='" + url + "'>Voir le résumé de votre litige</a>.</div>");


            MailSender.SendMessage(to, "[FastArbitre] Votre contestation est bien enregistrée", MailSender.GetHtmlAndReplaceData("~/Emails/Template.Simple.html", replacements), guid);

        }

        public static void EndFreePhase(string to, string displayName, string url, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Fin de la phase amiable");
            replacements.Add("|*TEXT*|", "Bonjour "+displayName+ ",<br/>Vous et votre adversaire avez exposé votre litige sur la plateforme. Vous pouvez désormais consulter votre profil, afin d’identifier les désaccords entre vous :" +
                "<div style=\"width:100%;text-align:center;font-size:20px;font-weight:bold;\"><a href='" + url + "'>Voir le litige</a></div>");


            MailSender.SendMessage(to, "[FastArbitre] Fin de la phase amiable", MailSender.GetHtmlAndReplaceData("~/Emails/Simple.EMMO.html", replacements), guid);

        }

        public static void SendMessageToArbiter(string to, string displayName, int conflictId, string url, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Message de " + displayName );
            replacements.Add("|*TEXT*|", "Bonjour,<br/>"+displayName+" vous a envoyé un message. Pour en prendre connaissance et y répondre, veuillez cliquer sur le lien suivant :" +
                "<div style=\"width:100%;text-align:center;font-size:20px;font-weight:bold;\"><a href='" + url + "'>Voir le message</a></div>");


            MailSender.SendMessage(to, "[FastArbitre] Message de " + displayName + " dans le dossier N° " +conflictId, MailSender.GetHtmlAndReplaceData("~/Emails/Simple.EMMO.html", replacements), guid);
        }

        public static void SendMessageToParty(string to, string displayName, int conflictId, string url, string emailUrl, Guid guid)
        {
            var replacements = new Dictionary<string, string>();

            replacements.Add("|*EMAILLINK*|", emailUrl);
            replacements.Add("|*TITLE*|", "Message pour " + displayName);
            replacements.Add("|*TEXT*|", "Bonjour, <br/> l'arbitre en charge de votre dossier N°"+ conflictId+" a envoyé un message pour "+displayName+". Pour en prendre connaissance et y répondre, veuillez cliquer sur le lien suivant :" +
                "<div style=\"width:100%;text-align:center;font-size:20px;font-weight:bold;\"><a href='" + url + "'>Voir le message</a></div>");


            MailSender.SendMessage(to, "[FastArbitre] Message pour " + displayName + " dans le dossier N° " + conflictId, MailSender.GetHtmlAndReplaceData("~/Emails/Simple.EMMO.html", replacements), guid);

        }

    }
}