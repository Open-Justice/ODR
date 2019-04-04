var ViewModel = null;

function viewModel(data) {
    var self = this;
    self.data = ko.mapping.fromJS(data.Conflict);
    self.clause = data.Clause == null ? null : ko.mapping.fromJS(data.Clause);
    self.updateClause = function (data, event) {
        $("#FileClause").click();
        $('input[type=file]').change(function (e) {
            self.shouldShowNewFile(true);
        });
    };
      
    self.shouldShowNewFile = ko.observable(false);

    self.toggleClause = function (data, event) {
        var thumb = $(event.target).closest(".mainSwitchDiv").children("#notifThumb");
        if (thumb.css("marginLeft").indexOf('0') == 0) {
            thumb.animate({ 'margin-left': thumb.width() }, 100);
            ViewModel.data.HasArbitralClause(false);
        }
        else {
            thumb.animate({ 'margin-left': '0px' }, 100);
            ViewModel.data.HasArbitralClause(true);
            $("select").material_select();
        }
    }

    ViewModel = self;
}