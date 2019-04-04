var ViewModel = null;

function viewModel(data) {
    var self = this;
    self.data = ko.mapping.fromJS(data.Conflict);
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