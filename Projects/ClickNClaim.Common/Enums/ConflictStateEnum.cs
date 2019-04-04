using ClickNClaim.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common
{
    public enum ConflictState
    {
        [EnumDisplayName("Dossier créé, nous avons besoin d'information complémentaire pour un éventuel arbitrage.")]
        [Description("Votre dossier vient d'être créé. Vous devez maintenant le renseigner ainsi que les personnes avec qui vous avez un litige avant que celles-ci ne reçoivent une invitation à venir résoudre ce litige sur la plateforme")]
        Created = 0,
        [EnumDisplayName("Le demandeur de ce dossier a terminé de remplir sa déclaration. Le défendeur peut maintenant contester la déclaration du demandeur et apporter ses propres faits et pièces.")]
        [Description("L'instigateur de ce dossier a bien rempli sa déclaration du litige. Le dossier est maintenant en attente de la déclaration des parties adverses.")]
        Pending = 1,
        [EnumDisplayName("Dossier complet, vous n’avez plus qu’à demander un arbitrage")]
        [Description("Chaque partie a maintenant rempli sa déclaration. Vous pouvez rentrer en phase d'arbitrage dès accord entre chaque partie.")]
        Open = 20,
        [EnumDisplayName("Demande de devis effectué")]
        [Description("Le conseil d'administration de FastArbitre va étudier votre dossier et vous transmettra un devis spécifique pour la résolution de votre litige.")]
        Quotation = 25,
        [EnumDisplayName("Demande d'arbitrage proposée. En attente de la validation des parties adverses")]
        [Description("Une partie au litige a demandé à ce que celui-ci passe maintenant en arbitrage. Une fois que chaque partie aura validé cette demande, Le dossier sera transmis au tribunal arbitral")]
        ArbitrationAsked = 30,
        [EnumDisplayName("Demande d'arbitrage effectuée. Le centre va maintenant se saisir de votre dossier. Vous recevrez un email de confirmation lorsque celui-ci aura été accepté.")]
        [Description("Les parties ont demandé à résoudre ce litige par arbitrage. En attente de validation par la plateforme de votre dossier.")]
        ArbitrationReady = 40,
        [EnumDisplayName("Dossier refusé par la plateforme")]
        [Description("Malheureusement, ce dossier ne peut être actuellement traité sur la plateforme. Si vous le souhaitez, vous pouvez à la demande de la plateforme compléter votre dossier, ou bien lancer une procédure hors plateforme.")]
        RefusedByPlateform = 41,
        [EnumDisplayName("Conflit refusé par l'arbitre en charge")]
        [Description("Un arbitre a refusé votre dossier. La plateforme se charge de vous proposer un nouvel arbitre dès que possible.")]
        RefusedByArbiter = 42,
        [EnumDisplayName("Arbitre désigné et dossier en relecture. En attente de sa validation par l'arbitre")]
        [Description("Un arbitre a été désigné. Celui-ci relit votre dossier, confirme qu'il n'existe à sa connaissance aucun conflit d'intérêt entre lui et vous-même et confirme être légitime pour la résolution de votre dossier.")]
        ArbiterAssigned = 45,
        [EnumDisplayName("Examen des demandes par l'arbitre")]
        [Description("Votre dossier est maintenant en phase d'arbitrage. L'arbitre traitant votre dossier est chargé d'animer les débats lui permettant de statuer sur votre dossier. Vous avez une première période de 15 jours qui a démarré, pendant laquelle vous devrez transmettre toutes les informations que l'arbitre vous demandera. A la fin de cette période, une pré-décision vous sera transmise et vous aurez alors 7 jours de plus pour la contester ou apporter un complément d'information à votre dossier.")]
        ArbitrationStarted = 50,
        [EnumDisplayName("Libres échanges")]
        [Description("Phase ouverte par l'arbitre pour un temps qu'il détermine, où les parties peuvent solliciter une visio-conférence, fournir des pièces complémentaires et évènements nouveaux")]
        FreeHandsArbitration = 53,
        [EnumDisplayName("Clôture des échanges")]
        [Description("Fin des échanges entre les parties et l'arbitre.")]
        ExchangeClosure = 56,
        [EnumDisplayName("Délibéré")]
        [Description("L'arbitre est en cours de rédaction de sa pré-sentence.")]
        Deliberation = 59,
        [EnumDisplayName("Pré-sentence rédigée")]
        [Description("La pré-sentence de votre dossier est maintenant rédigée. Vous entrez dans la dernière période de 7 jours pendant laquelle vous pouvez encore contester la pré-sentence et transmettre à l'arbitre toute pièce qui vous semblerait prouver vos dires.")]
        PreConcluded = 60,
        [EnumDisplayName("Discussion de la pré-sentence")]
        [Description("Si vous considérez qu'une pièce a été mal interprêté ou que l'arbitre n'a pas tenu compte de l'un de vos propos, il s'agit d'en débattre maintenant.")]
        PreConclusionDebate = 63,
        [EnumDisplayName("Clôture des débats")]
        [Description("La période de débat de la pré-décision est maintenant terminée. Il n'est plus possible de renseigner une quelconque information ou de télécharger une nouvelle pièce. L'arbitre va maintenant procéder à l'écriture de la décision finale sur votre dossier.")]
        EndOfDebates = 65,
        [EnumDisplayName("Délibéré définitif")]
        [Description("L'arbitre rédige actuellement la décision finale de votre affaire.")]
        FinalDeliberation = 69,
        [EnumDisplayName("Sentence rédigée")]
        [Description("La sentence finale de votre dossier vous a maintenant été transmise. Cette sentence vaut jugement. Les parties ont maintenant 15 jours pour respecter celle-ci. Une fois passé ce délai, l'exequatur peut être demandé et un huissier peut être désigné afin de faire appliquer la sentence.")]
        Concluded = 70,
        [EnumDisplayName("Dossier clos")]
        [Description("Votre dossier est maintenant clos. La décision a été rendue et appliquée.")]
        Close = 100
    }
}
