var ViewModel = null;

function viewModel(data) {
    var self = this;
    self.emailList = ko.observableArray(['']);
    self.addTier = function (data, event) {
        self.emailList.push('');
    }
    self.currentConflictForModal = ko.observable();
    self.openModal = function (data, event) {
        self.currentConflictForModal($(event.target).data("value"));
        $("#invitationBox").openModal();
    }
    self.sendInvitations = function (data, event) {
        $.ajax(
            {
                url: rootDir + "Conflict/SendInvitations",
                data: { emails: ko.unwrap(self.emailList), conflictId: self.currentConflictForModal()},
                complete: function (d, e) {
                    location.reload(true);
                }
            })
    }
    ViewModel = self;
}