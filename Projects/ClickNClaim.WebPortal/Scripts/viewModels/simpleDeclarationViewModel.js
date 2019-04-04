var ViewModel = null;

function viewModel(data) {
    var self = this;
    self.conflict = ko.mapping.fromJS(data);
    self.toggleClause = function (data, event) {
        var thumb = $(event.target).closest(".mainSwitchDiv").children("#notifThumb");
        if (thumb.css("marginLeft").indexOf('0') == 0) {
            thumb.animate({ 'margin-left': thumb.width() }, 100);
            ViewModel.conflict.HasArbitralClause(false);
        }
        else {
            thumb.animate({ 'margin-left': '0px' }, 100);
            ViewModel.conflict.HasArbitralClause(true);
            //   $(event.target).find("#hiddenValue").val(true);
        }
    }

    ViewModel = self;
}