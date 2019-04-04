var ViewModel = null;

ko.validation.rules.pattern.message = 'Invalid.';
ko.validation.rules["validObject"] = {
    validator: function (obj, bool) {
        if (!obj || typeof obj !== "object") {
            throw "[validObject] Parameter must be an object";
        }
        return bool === (ko.validation.group(obj)().length === 0);
    },
    message: "Every property of the object must validate to '{0}'"
};

ko.validation.init({
    registerExtenders: true,
    messagesOnModified: true,
    insertMessages: false,
    parseInputAttributes: true,
    messageTemplate: null,
    grouping: {
        deep: true,
        observable: true,
        live: true
    },
    decorateElement: true,
    decorateInputElement: true,
    errorElementClass: 'invalid',
    errorClass : 'invalid'

}, true);



function user() {
    var u = this;
    u.id = ko.observable(0);
    u.firstName = ko.observable().extend({ required: true });
    u.lastName = ko.observable().extend({ required: true });
    u.status = ko.observable('');
    u.businessFunction = ko.observable('');
    u.email = ko.observable().extend({ required: true });
    u.isLegalRepresentative = ko.observable(false);
    u.displayName = function () {
        var tmp = '';
        if (this.firstName() != '' && this.firstName() != undefined) {
            tmp += this.firstName();
        }
        if (this.lastName() != '' && this.lastName() != undefined) {
            tmp += ' ' + this.lastName()[0] + '.';
        }
        return tmp;
    }
    u.isInModification = ko.observable(true);
    u.isValid = ko.observable(false);
    u.validateUser = function () {
        var isOk = true;
        if (u.firstName() == '') {
            isOk = false;
        }


        if (u.firsName() != '' && u.lastName() != '' && u.email() != '')
            return true;
        else {

        }
        return isOk;
    }
}

function toStr(obj) {
    objj = ko.unwrap(obj);
    if (objj == null)
        return '';
    else
        return objj.toString();
}

function viewModel(data) {
    var self = this;
   
    self.conflict = ko.mapping.fromJS(data.Conflict);
    self.lawyer = ko.mapping.fromJS(data.Lawyer);
    self.userRepresented = ko.mapping.fromJS(data.UserRepresented);
    self.creationUser = self.conflict.CreatedBy;

    self.currentUserUIC = ko.utils.arrayFirst(self.conflict.UsersInConflicts(), function (item) {
        if (item.IdUser() == ui) {
            item.IsPhysical(item.IsPhysical().toString());
            item.IsRepresented(item.IsRepresented().toString());
            item.IsLawyer(item.IsLawyer() != null ? item.IsLawyer().toString() : 'false');
            return item;
        }
    });

    self.createdByUIC = ko.utils.arrayFirst(self.conflict.UsersInConflicts(), function (item) {
        if (item.IdUser() == self.conflict.IdCreationUser()) {
           
            return item;
        }
    });

    self.deleteUser = function (data, event) {
        $("#resume_uic_" + data.IdUser()).remove();
        self.conflict.UsersInConflicts.remove(data);

        if (data.IdUser() != null && data.IdUser() != "") {
            $.ajax({
                url: '/Conflict/RemoveInvitation',
                data: { conflictId: self.conflict.Id(), id: data.IdUser() }
            });
        }
    }

    self.addOpponent = function (data, event) {
        self.conflict.UsersInConflicts.push(ko.mapping.fromJS({
            IdConflict: self.conflict.Id(),
            IdUser: '',
            IsPhysical: true,
            CompanyName: '',
            User: {
                FirstName: '',
                LastName: '',
                Email : '',
            },
            IsRepresented :false
        }));
    }

    self.opponents = ko.computed(function () {

        var demandeur = "";
        var demandeursLawyer = "";
        if (self.conflict.UsersInConflicts()[0].IsLawyer() == "true")  {
            demandeur = self.conflict.UsersInConflicts()[1];
          
            demandeursLawyer = self.conflict.UsersInConflicts()[0];
        }
        else {
            demandeur = self.conflict.UsersInConflicts()[0];
           
            if (self.conflict.UsersInConflicts()[0].IsRepresented() == "true") {
                demandeursLawyer = ko.utils.arrayFirst(self.conflict.UsersInConflicts(), function (item) {
                    if (item.IdUser() == self.conflict.UsersInConflicts()[0].IdLawyer())
                        return item;
                });
            }
            //if (self.conflict.UsersInConflicts().length > 2) {
            //    self.conflict.UsersInConflicts.remove(self.conflict.UsersInConflicts()[1]);
            //}
        }

        var ops = ko.utils.arrayFilter(self.conflict.UsersInConflicts(), function (item) {
            return item.IdUser() != self.conflict.IdCreationUser() && !item.IsRepresented() && (demandeursLawyer == "" || demandeursLawyer == null || item.IdUser() != demandeursLawyer.IdUser());
        })
        if (ops == null || ops.length <= 0) {
            self.addOpponent();
        }
        return ko.utils.arrayFilter(self.conflict.UsersInConflicts(), function (item) {
            return item.IdUser() != self.conflict.IdCreationUser() && !item.IsRepresented() && (demandeursLawyer == "" || demandeursLawyer == null || item.IdUser() != demandeursLawyer.IdUser());
        })
    });

    self.validateForm = function (data, event) {
        if (self.opponents().length <= 0) {
            $("#opponent-error").removeClass('hidden');
            return false;
        }
        else {
            $("#opponent-error").addClass('hidden');
        }
        var mails =[];
         ko.utils.arrayForEach(self.opponents(), function (value, idx) {
             mails.push(value.User.Email());
         })
         mails.push(self.creationUser.Email());
        

         if (hasDuplicates(mails)) {
             $("#emails-duplicate-error").removeClass('hidden');
             return false;
         }
         else {
             $("#emails-duplicate-error").addClass('hidden');
         }

        return true;
    }


    self.eltFadeOut = function (element, index, data) {
        $(element).fadeOut(400, function () {
            $(element).remove();
        });
        
    }
    ViewModel = self;
}

function loadComponents() {
    $('#identification-nav').addClass('active');
}

function hasDuplicates(array) {
    var valuesSoFar = Object.create(null);
    for (var i = 0; i < array.length; ++i) {
        var value = array[i];
        if (value in valuesSoFar) {
            return true;
        }
        valuesSoFar[value] = true;
    }
    return false;
}