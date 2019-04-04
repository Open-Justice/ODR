var ViewModel = null;


function viewModel(data) {
    var self = this;
 
    self.company = ko.mapping.fromJS(data.Company);
    self.conflict = ko.mapping.fromJS(data.Conflict);

    self.SaveOfficialCompanyInfo = function (data, event) {
        if ($("#data-validation-form").isValid(null, null, true)) {
            var form = $("#data-validation-form");
            $.post(form.attr('action'),
                {conflictId : self.conflict.Id(), 'company.Siret' : self.company.Siret(), 'company.Name' : self.company.Name() }, function (data, status, xhr) {
                    $("#validation").removeClass("disable-content");
                });
              

            
        }
    }


    ViewModel = self;
}