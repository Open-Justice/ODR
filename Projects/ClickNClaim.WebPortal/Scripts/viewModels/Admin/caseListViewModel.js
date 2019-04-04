var ViewModel = null;
function viewModel()
{
    var self = this;

    self.arbitersLoading = ko.observable(false);
    self.arbiters = ko.observableArray([]);
    self.selectedConflictId = ko.observable();

    ViewModel = self;
}


function openModal(modal, id) {
    ViewModel.selectedConflictId(id);
    if (modal == "#arbiter-selection" && ViewModel.arbiters.length == 0) {
        ViewModel.arbitersLoading(true);
        $.ajax({
            url: rootDir + "Arbitre/GetArbiterList",
            complete: function (data, status, xhr) {
                ViewModel.arbiters(data.responseJSON);
                ViewModel.arbitersLoading(false);
            }
        })
    }
    $(modal).openModal();
    $("input[type=hidden]#conflitId").val(id);
}

