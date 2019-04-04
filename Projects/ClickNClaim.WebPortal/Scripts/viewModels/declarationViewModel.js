var ViewModel = null;

function viewModel(data) {
    var self = this;
    self.data = data;
    self.conflict = ko.mapping.fromJS(data.Conflict);
    self.description = ko.observable(data.UserInConflict.UserDescriptionOfTheConflict);

    ViewModel = self;
}


function loadComponents() {
    $('#declaration-nav').addClass('active');
    $("#declaration-nav").prev().css('display', 'inline-block');
}