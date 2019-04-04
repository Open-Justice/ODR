var ViewModel = null;

function toStr(obj) {
    if (obj == null)
        return '';
    else
        return obj.toString();
}

function viewModel(data) {
    var self = this;
   
    self.conflict = ko.mapping.fromJS(data.Conflict);
    self.lawyer = ko.mapping.fromJS(data.Lawyer);
    self.me = ko.mapping.fromJS(data.Me);
   
    ViewModel = self;
}

function loadComponents() {
    $('#identification-nav').addClass('active');
}