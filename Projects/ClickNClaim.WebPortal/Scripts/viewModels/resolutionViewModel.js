var ViewModel = null;
function viewModel(data, declarantResolutionEvent) {
    var self = this;
    self.conflict = data.Conflict;
    self.declarantResolutionEvent = ko.mapping.fromJS(declarantResolutionEvent);
    self.disagreementText = declarantResolutionEvent != null ? ko.observable(declarantResolutionEvent.Disagreements != null && declarantResolutionEvent.Disagreements.length > 0 ? declarantResolutionEvent.Disagreements[0].Comment : '') : '';
    self.errors = ko.observableArray([]);
    self.resolutions = ko.mapping.fromJS(data.ResolutionTypes);
    self.myResolution = ko.mapping.fromJS(data.MyResolutions);
    self.emailList = ko.observableArray(['']);
    self.addTier = function (data, event) {
        self.emailList.push('');
    }

    self.finalize = function (data, event) {
        var res = self.validateForm(data, event);
        if (res) {
            $('#emitBox').openModal();
        }
    }

    self.selectResolution = function (data, event) {
    }
    self.toggleResolution = function (data, event) {
        data.IsSelected(!data.IsSelected());
        makeDownloadAvailable();
    }

    self.noBubbling = function (data, event) {
        event.stopPropagation();
    }

    self.sendData = function (data, event) {

        if ($("#validation-form").isValid()) {

            var d = ko.utils.arrayFilter(ko.mapping.toJS(self.resolutions), function (item) {
                return item.IsSelected;
            });

            $.ajax({
                url: rootDir + "Conflict/SaveResolutions",
                data: {
                    conflictId: self.conflict.Id,
                    myResolutions: d
                },
                method: 'POST',
                complete: function (data, status, xhr) {
                    window.location = rootDir + "Conflict/"+self.conflict.Id+"/Clause";
                }
            })
        }
    }

    self.validateForm = function (data, event) {
        self.errors.removeAll();
        if (self.myResolution.IdResolutionType() == 0) {
            self.errors.push("Vous devez choisir la résolution que vous souhaitée");
            return false;
        }
        else if (self.myResolution.ResolutionComment() == null || self.myResolution.ResolutionComment() == '') {
            self.errors.push("Merci d'expliquer pourquoi vous souhaitez ce type de résolution");
            return false;
        }
        else if (self.myResolution.IdResolutionType() == 1 && (self.myResolution.ResolutionValue() == null || self.myResolution.ResolutionValue() == '')) {
            self.errors.push("Merci de préciser un montant");
            return false;
        }
        return true;
    }

    self.setDisagreement = function (data, event) {
        if (self.disagreementText() != '') {
            var disagreementId = 0;
            if (self.declarantResolutionEvent.Disagreements() != null && self.declarantResolutionEvent.Disagreements().length > 0) {
                disagreementId = self.declarantResolutionEvent.Disagreements()[0].Id();
            }
            $("#disagreement").removeClass("invalid");
            $.ajax({
                url: rootDir + "Action/AddDemandDisagreement",
                data: { disagreement: self.disagreementText(), idEvent: self.declarantResolutionEvent.Id, idDisagreement: disagreementId },
                complete: function (d, e) {
                    self.declarantResolutionEvent.Disagreements.removeAll();
                    self.declarantResolutionEvent.Disagreements.push(ko.mapping.fromJS(d.responseJSON));
                }
            })
        }
        else {
            $("#disagreement").addClass("invalid");
        }
    }

    self.startDebate = function (data, event) {
        $("#debatEventModal").openModal();
    }

    self.uploadFileToNewEvent = function (data, event) {
        $(event.target).closest(".table-cell").children(".drop-area-div").find("input[type=file]").click();
    }

    ViewModel = self;
}


function makeDownloadAvailable() {
    $(".drop-area-div").dmUploader({
        url: rootDir + 'Upload/UploadFileFromDefault',
        extraData: {
            'conflictId': ViewModel.conflict.Id
        },
        onBeforeUpload: function (id) {

            var uid = $(this).children("input[type=hidden].uid").val();
            var event = ko.utils.arrayFirst(ViewModel.resolutions(), function (item) {
                return item.Id() == uid;
            });

            event.IsDownloading(true);

            $(this).data('dmUploader').settings.extraData = {
                'conflictId': ViewModel.conflict.Id,
                'uid': uid
            }

        },
        onUploadSuccess: function (id, data) {
            var lookingFor = $(this).children('input[type=hidden].uid').val();
            var event = ko.utils.arrayFirst(ViewModel.resolutions(), function (item) {
                return item.Id() == lookingFor;
            });

            for (var i = 0; i < data.length; i++) {
                event.ProofFiles.push(data[i]);
            }
            //event.IdEvent(data.Id);
            //event.Id(data.IdDefaultEvent);
            event.IsDownloading(false);
        },
        onUploadProgress: function (id, percent) {
            var uid = $(this).children("input[type=hidden].uid").val();

            var event = ko.utils.arrayFirst(ViewModel.resolutions(), function (item) {
                return item.Id() == uid;
            });

            event.Percent(percent + "%");
        }
    });

}
