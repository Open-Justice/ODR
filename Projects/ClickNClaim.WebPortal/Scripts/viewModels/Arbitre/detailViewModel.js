var ViewModel = null;

function viewModel(data) {
    var self = this;
    self.selectedConflictId = ko.observable();
    self.skills = ko.observableArray(data.ArbiterSkills);
    self.skillToAdd = ko.observable('');
    self.addSkill = function (data, event) {
        self.skills.push({ Skill: { Name: self.skillToAdd() } });
        self.skillToAdd('');
    }
    self.removeSkill = function (data, event) {
        self.skills.remove(data);
    }
    ViewModel = self;
}


function openModal(modal, id) {
    if (id != undefined && id != null)
        ViewModel.selectedConflictId(id);
    ViewModel.selectedConflictId(id);
    //$("input[type=hidden]#conflitIdHidden").val(id);
    $(modal).openModal();

}

