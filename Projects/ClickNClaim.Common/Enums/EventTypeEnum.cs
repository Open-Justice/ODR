using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common.Enums
{
    public enum EventTypeEnum
    {
        [Description("Evènement")]
        Event = 0,
        [Description("Création de la société")]
        CompanyCreation = 1,
        [Description("Fermeture de la société")]
        CompanyClosure = 2,
        [Description("Fichier")]
        FileOrientedEvent = 3,
        [Description("Résolution")]
        Resolution = 4,
        [Description("Visio-conférence")]
        Visio = 5,
        [Description("Pré-conclusion")]
        PreConclusion = 6,
        Conclusion = 7,
        FreeComment = 8,
        OfficialDocuments = 9,
        MissionAct = 10

    }
}
