using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.OpenFireConnector
{
    public class chatRoom
    {
        public string roomName { get; set; }
        public string naturalName { get; set; }
        public string description { get; set; }
        public string subject { get; set; }
        public DateTime creationDate { get; set; }
        public DateTime modificationDate { get; set; }
        public int maxUsers { get; set; }
        public bool persistent { get; set; }
        public bool publicRoom { get; set; }
        public bool registrationEnabled { get; set; }
        public bool canAnyoneDiscoverJID { get; set; }
        public bool canOccupantsChangeSubject { get; set; }
        public bool canChangeNickname { get; set; }
        public bool logEnabled { get; set; }
        public bool loginRestrictedToNickname { get; set; }
        public bool membersOnly { get; set; }
        public bool moderated { get; set; }
        public List<broadcastPresenceRole> broadcastPresenceRoles { get; set; }
        public List<owner> owners { get; set; }
        public List<admin> admins { get; set; }
        public List<member> members { get; set; }
        public List<outcast> outcasts { get; set; }

        public chatRoom()
        {
            broadcastPresenceRoles = new List<broadcastPresenceRole>();
            owners = new List<owner>();
            admins = new List<admin>();
            members = new List<member>();
            outcasts = new List<outcast>();
        }

        public chatRoom(string name, string description, DateTime creationDate)
            :this()
        {
            this.description = description;
            this.logEnabled = true;
            this.loginRestrictedToNickname = false;
            this.membersOnly = true;
            this.persistent = true;
            this.creationDate = creationDate;
            this.maxUsers = 10;
            this.membersOnly = true;
            this.naturalName = name.ToLower();
            this.publicRoom = false;
            this.registrationEnabled = true;
            this.roomName = name.ToLower();
            this.subject = name;
        }
    }
}

