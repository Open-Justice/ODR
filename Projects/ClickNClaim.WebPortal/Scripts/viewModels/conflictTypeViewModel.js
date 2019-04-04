var ViewModel = null;

function viewModel(data) {
    var self = this;
    self.data = data.Conflict;
    self.needSave = false;

    self.updateCategories = function (newValue, cb) {
        $.ajax({
            url: rootDir + "Conflict/Types?category=" + newValue + "&conflictId=" + self.data.Id,
            method: 'GET',
            complete: function (data, status, xhr) {
                self.availableTypes.removeAll();
                self.availableTypes.pushAll(data.responseJSON);
                if (cb != null) {
                    cb();
                }
               
            }
        })
    }
    self.dropText = ko.observable('Choisir le type de votre litige :');
    self.selectedType = ko.observable(self.data.ConflictType != null ? self.data.ConflictType.Code[0] : '');
    if (self.data.ConflictType != null) {
        self.updateCategories(self.data.ConflictType.Code[0],
            function () {
                self.typologySelected(self.data.IdConflictType);
                self.dropText(self.data.ConflictType.Name)
                $.ajax({
                    url: rootDir + "Conflict/DefaultEvents",
                    data: { conflictTypeId: self.data.IdConflictType, conflictId : self.data.Id },
                    method: 'GET',
                    complete: function (data, status, xhr) {
                        self.defaultEvents.removeAll();
                        var eltArray = [];
                        for (var i = 0; i < data.responseJSON.length; i++) {
                            var elt = ko.mapping.fromJS(data.responseJSON[i]);
                            elt.Files = ko.observableArray(elt.Files());
                            eltArray.push(elt);
                        }
                        self.defaultEvents.pushAll(eltArray);
                        self.defaultEvents.notifySubscribers();
                    }
                });
            });
    }
    self.typologySelected = ko.observable(self.data.IdConflictType);

    self.availableTypes = ko.observableArray([]);

    self.loadActionsForElement = function (data, event) {
        if (ViewModel != null) {
            makeDownloadAvailable();
            $('input.date').bootstrapMaterialDatePicker({ format: 'DD/MM/YYYY', lang: 'fr', weekStart: 1, cancelText: 'ANNULER', time: false });

        }
    }

    self.defaultEvents = ko.observableArray(data.DefaultEvents);
    self.cat1 = ko.computed(function () {
        return ko.utils.arrayFilter(self.defaultEvents(), function (item) {
            var t = ko.unwrap(item.Type);
            return t == 1;
        })
    });
    self.cat2 = ko.computed(function () {
        return ko.utils.arrayFilter(self.defaultEvents(), function (item) {
            var t = ko.unwrap(item.Type);
            return t == 2;
        })
    });
    self.cat3 = ko.computed(function () {
        return ko.utils.arrayFilter(self.defaultEvents(), function (item) {
            var t = ko.unwrap(item.Type);
            return t == 3;
        })
    });
    self.existingEvents = ko.mapping.fromJS(ko.utils.arrayMap(ko.utils.arrayFilter(self.data.Events, function (item) {
        return item.IdDefaultEvent != null;
    }), function (item) {
        if (item.IdDefaultEvent != null) {

            var defItem = ko.utils.arrayFirst(self.defaultEvents(), function (ee) {
                return item.IdDefaultEvent == ee.Id;
            })
            if (defItem != null) {
                return {
                    Id: item.IdDefaultEvent,
                    Name: item.Name,
                    Date: moment(item.DateBegin).format("DD/MM/YYYY"),
                    Description: item.Description,
                    IdEvent: item.Id,
                    Info: '',
                    Placeholder: defItem.Placeholder,
                    Files: item.ProofFiles,
                    IsDownloading: false,
                    Percent: 0,
                    uid: guid(),
                    Type: item.Type,
                    isSaved: ko.observable(true),
                    SubAccordTitle: defItem.SubAccordTitle,
                    SubAccordInfo: defItem.SubAccordInfo,
                    SubAccordPlaceholder: defItem.SubAccordPlaceholder,
                    SubPbTitle: defItem.SubPbTitle,
                    SubPbInfo: defItem.SubPbInfo,
                    SubPbPlaceholder: defItem.SubPbPlaceholder,
                    SubName : item.SubName

                }
            }



        }
    }));

    self.firstOfKind = function (id, index) {
        for (var i = 0; i < index; i++) {
            if (self.existingCat2Events()[i].Id() == id)
                return false;
        }
        return true;
    }

    self.lastOfKind = function (id, index) {
        for (var i = index+1; i < self.existingCat2Events().length; i++) {
            if (self.existingCat2Events()[i].Id() == id)
                return false;
        }
        return true;
    }

  

    self.addToExistingElement = function (data, event) {
        var toAdd = $.extend({}, ko.mapping.toJS(data));
        toAdd = ko.mapping.fromJS(toAdd);
        toAdd.Date('');
        toAdd.isSaved = ko.observable(false);
        toAdd.uid = ko.observable(guid());
        self.existingEvents.push(toAdd);
        self.needSave = true;
        self.loadActionsForElement();
        //document.getElementById("event_" + (self.existingEvents().length - 1)).scrollIntoView(true);
    }

    self.addAnother = function (data, event) {
        var elt = ko.utils.arrayFirst(self.defaultEvents(), function (item) {
            return item.Id() == data.Id();
        })
        var toAdd = $.extend({}, ko.mapping.toJS(elt));
        toAdd = ko.mapping.fromJS(toAdd);
        toAdd.Date('');
        toAdd.isSaved = ko.observable(false);
        toAdd.uid = ko.observable(guid());
        self.existingEvents.push(toAdd);
        self.needSave = true;
        self.loadActionsForElement();
    }

    self.existingCat1Events = ko.computed(function () {
        var tmp = ko.utils.arrayFirst(self.existingEvents(), function (item) {
                var type = ko.unwrap(item.Type);
                return type == 1;
        })
        if (tmp != null && tmp.length > 0) {
            return tmp;
        }
        else {
            if (self.cat1 != null && self.cat1().length > 0) {
                self.addToExistingElement(self.cat1()[0]);
            }
            var tmp = ko.utils.arrayFirst(self.existingEvents(), function (item) {
                var type = ko.unwrap(item.Type);
                return type == 1;
            })
            return tmp;
        }
    });
    self.existingCat2Events = ko.computed(function () {
        return ko.utils.arrayFilter(self.existingEvents(), function (item) {
            var type = ko.unwrap(item.Type);
            return type == 2;

        })
    });
    self.existingCat3Events = ko.computed(function () {
        return ko.utils.arrayFilter(self.existingEvents(), function (item) {
            var type = ko.unwrap(item.Type);
            return type == 3;

        })
    });

    self.conflictTypeTemplateSelector = function (item) {
        var type = ko.unwrap(item.Type);
        if (type == 1 || type == 4) {
            return 'base-accord-template';
        }
        else if (type == 2) {
            return 'accord-issue-template';
        }
        else if (type == 3) {
            return 'resolution-issue-template';
        }
    }
    self.saveEvent = function (data, event) {
        var form = $(event.target).closest('form');
        if (form.isValid()) {
            data.isSaved(true);
            $.post(form.attr('action'), form.serialize());
        }
    }

    self.selectType = function (data, event) {
        self.dropText(data.Name);
        self.existingEvents.removeAll();
        self.typologySelected(data.Id);
        self.existingEvents.removeAll();
        self.error('');
    
    }
    self.uploadFile = function (data, event) {
        $(event.target).closest(".card").find(".drop-area-div").find("input[type=file]").click();
    }
    self.deleteElt = function (data, event) {
        if (data.IdEvent() != 0)
            Layout.showConfirm({
                header: "Êtes-vous sur de vouloir supprimer cet évènement et ses documents ?",
                text: "Vous êtes sur le point de supprimer un évènement que vous avez renseigné",
                confirmNo: function () { },
                confirmYes: function () {
                    self.existingEvents.remove(data);
                    if (data.IdEvent != null && data.IdEvent() > 0) {
                        $.post(rootDir + "Conflict/RemoveEvent", {
                            id: data.IdEvent(),
                            conflictId : self.data.Id
                        })
                    }
                }
            })
        else {
            self.existingEvents.remove(data);
        }
    }
  
    self.deleteFileHandler = function (data, parent) {
        Layout.showConfirm({
            header: "Etes-vous sur de vouloir supprimer ce fichier ?",
            text: "Ce fichier ne pourra pas être pris en compte lors de la résolution de votre litige",
            confirmYes: function () {
                self.deleteFile(data, parent);
            },
            confirmNo: function () { }
        })
    }
    self.deleteFile = function (data, parent) {
        var callback = function () {
            parent.Files.remove(data);
        };
        $.ajax({
            url: rootDir + 'Conflict/RemoveFile',
            data: { id: data.Id, conflictId: ViewModel.data.Id, fileName: data.Name },
            complete: function (data, event) {
                callback();
            }
        })
    }


    self.selectedType.subscribe(function (newValue) {
        if (newValue != '') {
            self.updateCategories(newValue);
        }
        self.dropText('Choisir le type de votre litige :');
        self.typologySelected('');
    });
    self.typologySelected.subscribe(function (newValue) {
        if (newValue != '') {
            $.ajax({
                url: rootDir + "Conflict/DefaultEvents",
                data: { conflictTypeId: newValue, conflictId : self.data.Id },
                method: 'GET',
                complete: function (data, status, xhr) {
                    self.defaultEvents.removeAll();
                    var eltArray = [];
                    for (var i = 0; i < data.responseJSON.length; i++) {
                        var elt = ko.mapping.fromJS(data.responseJSON[i]);
                        elt.Files = ko.observableArray(elt.Files());
                        eltArray.push(elt);
                    }
                    self.defaultEvents.pushAll(eltArray);
                    self.existingEvents.removeAll();
                    self.loadActionsForElement();
                    $('.tooltipped').tooltip({ delay: 50 });
                }
            })
        }
    });
    self.error = ko.observable('');
    self.formsToPost = 0;
    self.updateDates = function (shouldSubmitMainForm) {
        //for (var i = 0; i < self.existingEvents().length; i++) {
        //    if (self.existingEvents()[i].Date() != '') {
        //        var splits = self.existingEvents()[i].Date().split('/')
        //        self.existingEvents()[i].Date(splits[1] + "/" + splits[0] + "/" + splits[2]);
        //    }
        //}

        if (self.typologySelected() == '') {
            self.error("Merci de définir en quoi concerne votre litige.");
            return false;
        }

        if (self.needSave) {
            if (ko.utils.arrayFilter(self.existingEvents(), function (item) {
                return item.isSaved() == false;
            }).length > 0) {
                Layout.showConfirm({
                    header: "Sauvegarder mes modifications ?",
                    text: "Vous n'avez pas enregistré tous les évènements que vous avez déclarés. Souhaitez-vous les enregistrer maintenant ?",
                    confirmNo: function () { return true; },
                    confirmYes: function () {
                        var forms = $('form.event-form');
                        ViewModel.formsToPost = forms.length;
                        for (var i = 0; i < forms.length; i++) {
                            if ($(forms[i]).isValid()) {
                                $.post($(forms[i]).attr('action'), $(forms[i]).serialize(), function (data, status, xhr) {
                                    ViewModel.formsToPost--;

                                    if (ViewModel.formsToPost == 0) {
                                        ViewModel.needSave = false;
                                        if (shouldSubmitMainForm != false) {
                                            $("form.main-form").submit();
                                        }
                                    }
                                });
                            }
                            else {
                                Layout.showInfo({ infoHeader: 'Impossible de sauvegarder - Champs requis', 'infoText': 'Certains champs obligatoire n\' pas été rempli. Merci de bien vérifier que vous avez renseigné une date pour chaque évènement.' });
                                break;
                            }
                        }

                    }
                });
                return false;
            }
        }

        return true;
    }


    self.save = function (data, event) {
        self.updateDates(false);
    }


    

    ViewModel = self;
    self.loadActionsForElement();
}


function loadComponents() {
    ViewModel.loadActionsForElement();
    $('#timeline-nav').addClass('active');
    $("#timeline-nav").prev().css('display', 'inline-block');
}

function makeDownloadAvailable() {
    $(".drop-area-div").dmUploader({
        url: rootDir + 'Upload/UploadFileFromDefault',
        extraData: {
            'conflictId': ViewModel.data.Id
        },
        auto : true,
        onBeforeUpload: function (id) {
            var uid = $(this).children("input[type=hidden].uid").val();
            var event = ko.utils.arrayFirst(ViewModel.existingEvents(), function (item) {
                return item.uid() == uid;
            });

            event.IsDownloading(true);

            $(this).data('dmUploader').settings.extraData = {
                'conflictId': ViewModel.data.Id,
                'uid': uid
            }

        },
        onUploadSuccess: function (id, data) {
            var lookingFor = $(this).children('input[type=hidden].uid').val();
            var event = ko.utils.arrayFirst(ViewModel.existingEvents(), function (item) {
                return item.uid() == lookingFor;
            });

            for (var i = 0; i < data.length; i++) {
                event.Files.push(data[i]);
            }
            //event.IdEvent(data.Id);
            //event.Id(data.IdDefaultEvent);
            event.IsDownloading(false);
        },
        onUploadProgress: function (id, percent) {
            var uid = $(this).children("input[type=hidden].uid").val();

            var event = ko.utils.arrayFirst(ViewModel.existingEvents(), function (item) {
                return item.uid() == uid;
            });

            event.Percent(percent + "%");
        },
        onInit: function(){
        },
        onNewFile: function (id, file) {
        },
        onUploadError: function (id, xhr, status, error) {
        }

    });

}
function guid() {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
          .toString(16)
          .substring(1);
    }
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
      s4() + '-' + s4() + s4() + s4();
}