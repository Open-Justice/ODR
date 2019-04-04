var ViewModel = null;

function viewModel(data) {
    var self = this;
    self.data = data;
    self.disagreementModel = data.DisagreementModel;

    self.events = ko.mapping.fromJS(data.Conflict.Events);
    self.newEvent = ko.observable(new eventElt(data.Conflict.Id));
    self.updateIdx = null;
    self.newFreeComment = ko.observable();
    self.addNewEvent = function (data, event) {
        var hasErrors = false;
        if (self.newEvent().DateBegin() == '') {
            $("#newEventBeginDate").addClass("invalid");
            hasErrors = true;
        }
        if (self.newEvent().Name() == '') {
            $("#newEventName").addClass("invalid");
            hasErrors = true;
        }

        if (hasErrors) {
            return false;
        }

        var isToUpdate = false;
        var isAdded = false;
        if (self.updateIdx != null) {
            isToUpdate = true;
        }
        var previous = self.newEvent().DateBegin();
        self.newEvent().DateBegin(moment(self.newEvent().DateBegin()).format('L'));



        $.ajax({
            url: rootDir + "Conflict/AddNewEvent",
            data: { e: ko.mapping.toJS(self.newEvent), conflictId: self.data.Conflict.Id },
            method: "POST",
            complete: function (data, status, xhr) {
                data.responseJSON.DateBegin = moment(data.responseJSON.DateBegin).format("MM/DD/YYYY");
                if (isToUpdate) {
                    ViewModel.events.remove(function (item) {
                        return item.Id() == data.responseJSON.Id;
                    });
                }
                for (var i = 0; i < self.events().length; i++) {
                    if (moment(data.responseJSON.DateBegin).isBefore(moment(self.events()[i].DateBegin()))) {
                        self.events.splice(i, 0, ko.mapping.fromJS(data.responseJSON));
                        isAdded = true;
                        break;
                    }
                }
                if (!isAdded) {
                    self.events.push(ko.mapping.fromJS(data.responseJSON));
                }
                $('.tooltipped').tooltip({ delay: 50 });
                $(".read-more").unbind("click");
                $(".read-more").click(function (event) {
                    var target = $(this).data("target");
                    var accordion = $(this).closest('.row').siblings("div.accordion#" + target);
                    if (accordion.is(":visible")) {
                        accordion.slideUp();
                    }
                    else {
                        accordion.slideDown();
                    }
                })
            }
        })
        $("#addNewEventModal").closeModal();

        self.newEvent(new eventElt(ViewModel.data.Conflict.Id));
    }
    self.editEvent = function (data, event) {
        self.updateIdx = ViewModel.events.indexOf(data);
        self.newEvent(data);
        $("#addNewEventModal").openModal({
            complete: function () {
                self.updateIdx = null;
               
            },
            ready: function () {
                makeDownloadAvailable();
            }

        });
        $("#newEventBeginDate").val(moment(data.DateBegin()).format('L'));
        $('input.date').bootstrapMaterialDatePicker({ format: 'DD/MM/YYYY', lang: 'fr', weekStart: 1, cancelText: 'ANNULER', time: false });


    }
    self.clean = function (data, event) {
        self.updateIdx = null;
        self.newComment('');
        self.selectedEvent(null);
        self.debateEvt(null);
        self.newFreeComment('');
        self.newDisagreement(ko.mapping.fromJS(jQuery.extend({}, self.disagreementModel)));
        $("#addNewEventModal").closeModal();
    }

    self.addNewFreeComment = function (data, event) {
        $.ajax({
            url: rootDir + 'Conflict/AddFreeComment',
            data:
                {
                    conflictId: ViewModel.data.Conflict.Id,
                    comment: ViewModel.newFreeComment(),
                },
            complete: function (data, status, xhr) {
                self.events.push(ko.mapping.fromJS(data.responseJSON));
            }
        })
    }

    self.isSelected = function (pieceName) {
        var pieces = self.newDisagreement().ConcurrentPieces().split(',');
        if (pieces.indexOf(pieceName) >= 0) {
            return pieceName;
        }
    }
    self.deleteEventHandler = function (data, event) {
        Layout.showConfirm({
            header: "Etes-vous sur de vouloir supprimer cet évènement ?",
            text: "supprimer cet évènement supprimera aussi les fichiers qui sont liés à cet évènement et ne pourra pas être pris en compte lors de la résolution de votre litige",
            confirmYes: function () {
                self.deleteEvent(data, event);
            },
            confirmNo: function () { }
        })
    }
    self.deleteFileHandler = function (data, event) {
        Layout.showConfirm({
            header: "Etes-vous sur de vouloir supprimer ce fichier ?",
            text: "Ce fichier ne pourra pas être pris en compte lors de la résolution de votre litige",
            confirmYes: function () {
                self.deleteFile(data, event);
            },
            confirmNo: function () { }
        })
    }
    self.deleteEvent = function (data, event) {
        if (data.Id() == 0) {
            self.events.remove(data);
        }
        else {
            var callback = function () {
                self.events.remove(data);
            };
            $.ajax({
                url: rootDir + 'Conflict/RemoveEvent',
                data: { id: data.Id(), conflictId : self.data.Conflict.Id },
                complete: function (data, event) {
                    callback();
                }
            })
        }
    }
    self.deleteFile = function (data, event) {
        var callback = function () {
            event.ProofFiles.remove(data);
        };
        $.ajax({
            url: rootDir + 'Conflict/RemoveFile',
            data: { id: data.Id(), conflictId: ViewModel.data.Conflict.Id, fileName: data.Name() },
            complete: function (data, event) {
                callback();
            }
        })
    }
    self.uploadFile = function (data, event) {
        $(event.target).closest(".timeline-elt-edit").siblings(".drop-area-div").find("input[type=file]").click();
    }
    self.uploadFileToNewEvent = function (data, event) {
        $(event.target).closest(".modal-content").children(".drop-area-div").find("input[type=file]").click();
    }

    self.commentEvent = function (data, event) {
        $("#commentEventModal").openModal({
            dismissible: false
        });
        self.selectedEvent(data);
    }
    self.newComment = ko.observable();
    self.selectedEvent = ko.observable();
    self.addNewComment = function (data, event) {
        $.ajax({
            url: rootDir + 'Conflict/AddNewComment',
            data: {
                Text: self.newComment(),
                IdEvent: self.selectedEvent().Id(),
                conflictId : self.data.Conflict.Id
            },
            complete: function (data, event) {
                self.selectedEvent().Comments.push(data.responseJSON);
            }
        })
    }

    self.countDonwloadableItems = function (items) {
        var j = 0;
        for (var i = 0; i < items.length; i++) {
            j += items[i].Id() != 0 ? 1 : 0;
        }
        return j;
    }
    self.next = function (data, event) {
        window.location = rootDir + "Conflict/" + ViewModel.data.Conflict.Id + "/" + "Resolution";

    }
    self.setDownloadable = function (data, event) {
        makeDownloadAvailable();
    }

    self.newDisagreement = ko.observable(ko.mapping.fromJS(jQuery.extend({}, self.disagreementModel)));
    self.startDebate = function (data, event) {
        self.debateEvt(data);
        self.newDisagreement().ConcurrentDate(data.DateBegin());
        if (data.Disagreements().length > 0) {
            self.newDisagreement(data.Disagreements()[0]);
        }
        $("#debatEventModal").openModal();
        $('input.date').bootstrapMaterialDatePicker({ format: 'DD/MM/YYYY', lang: 'fr', weekStart: 1, cancelText: 'ANNULER', time: false });

    }

    self.debateEvt = ko.observable(null);
    self.setDisagreement = function (data, event) {
        var disagreement = ko.mapping.toJS(self.newDisagreement);
        disagreement.IdEvent = self.debateEvt().Id();
        if (disagreement.DisagreementOnPiece) {
            disagreement.ConcurrentPieces = $("input[name=group2]:checked").map(function () { return $(this).val() }).get().join();
        }
        if (disagreement.DisagreementOnDate) {
            disagreement.ConcurrentDate = $("#disagreementDate").val();
        }

        $.ajax({
            url: rootDir + "Action/AddAction",
            data: disagreement,
            method: 'POST',
            complete: function (d, e) {
                var event = ko.utils.arrayFirst(self.events(), function (item) {
                    return item.Id() == self.debateEvt().Id();
                });
                event.Disagreements.removeAll();
                event.Disagreements.push(ko.mapping.fromJS(d.responseJSON));
                self.clean();
            }
        })
    }
    self.eventTemplateSelector = function (item) {
        var type = ko.unwrap(item.Type);
        if (type == 0) {
            return 'classic-event-template';
        }
        else if (type == 1) {
            return 'classic-event-template';
        }
        else if (type == 2) {
            return 'classic-event-template';
        }
        else if (type == 3) {
            return 'classic-event-template';
        }
        else if (type == 4) {
            return 'classic-event-template';
        }
        else if (type == 5) {
            return 'classic-event-template';
        }
        else if (type == 6) {
            return 'classic-event-template';
        }
        else if (type == 8) {
            return 'classic-event-template';
        }

    }

    self.allowDownload = function (elt) {
        makeDownloadAvailable();
    }

    ViewModel = self;
}

function setPickADate(date) {
    $('#newEventBeginDate').pickadate('set').set('select', new Date(date));
}

function eventElt(idConflict) {
    var self = this;
    self.Id = ko.observable(0);
    self.Name = ko.observable('');
    self.DateBegin = ko.observable();
    self.DateEnd = ko.observable('');
    self.IdConflict = ko.observable(idConflict);
    self.Description = ko.observable('');
    self.ProofFiles = ko.observableArray([]);
    self.isValid = function () {
        if (self.Name() != '' && self.IdConflict() != null) {
            return true;
        }
        else {
            return false;
        }
    }
    self.IsDownloading = ko.observable(false);
    self.Percent = ko.observable(0);

}



function loadComponents() {

    var timelineBlocks = $('.cd-timeline-block'),
		offset = 0.8;

    //hide timeline blocks which are outside the viewport
    hideBlocks(timelineBlocks, offset);

    //on scolling, show/animate timeline blocks when enter the viewport
    $(window).on('scroll', function () {
        (!window.requestAnimationFrame)
			? setTimeout(function () { showBlocks(timelineBlocks, offset); }, 100)
			: window.requestAnimationFrame(function () { showBlocks(timelineBlocks, offset); });
    });

    function hideBlocks(blocks, offset) {
        blocks.each(function () {
            ($(this).offset().top > $(window).scrollTop() + $(window).height() * offset) && $(this).find('.cd-timeline-img, .cd-timeline-content').addClass('is-hidden');
        });
    }

    function showBlocks(blocks, offset) {
        blocks.each(function () {
            ($(this).offset().top <= $(window).scrollTop() + $(window).height() * offset && $(this).find('.cd-timeline-img').hasClass('is-hidden')) && $(this).find('.cd-timeline-img, .cd-timeline-content').removeClass('is-hidden').addClass('bounce-in');
        });
    }


    $(".read-more").click(function (event) {
        var target = $(this).data("target");
        var accordion = $(this).closest('.row').siblings("div.accordion#" + target);
        if (accordion.is(":visible")) {
            accordion.slideUp();
        }
        else {
            accordion.slideDown();
        }
    })

    $('#timeline-nav').addClass('active');



    $('.modal-trigger').leanModal({
        ready: function () {
            ViewModel.newEvent(new eventElt(ViewModel.data.Conflict.Id));
            $('input.date').bootstrapMaterialDatePicker({ format: 'DD/MM/YYYY', lang: 'fr', weekStart: 1, cancelText: 'ANNULER', time: false });

            makeDownloadAvailable();
        }
    });
    makeDownloadAvailable();
}

function makeDownloadAvailable() {
    $(".drop-area-div").dmUploader({
        url: rootDir + 'Upload/UploadFile',
        extraData: {
            'conflictId': ViewModel.data.Conflict.Id
        },
        onBeforeUpload: function (id) {

            var eventId = $(this).children("input[type=hidden]").val();

            if (eventId == "0") {
                ViewModel.newEvent().IsDownloading(true);
            }
            else {

                var event = ko.utils.arrayFirst(ViewModel.events(), function (item) {
                    return item.Id() == eventId;
                });

                event.IsDownloading(true);

            }
            $(this).data('dmUploader').settings.extraData = {
                'conflictId': ViewModel.data.Conflict.Id,
                'EventId': eventId,
            }

        },
        onUploadSuccess: function (id, data) {
            var eventId = $(this).children("input[type=hidden]").val();

            if (eventId == "0") {
                ViewModel.newEvent().IsDownloading(false);
                ViewModel.newEvent().ProofFiles.push(ko.mapping.fromJS(data));
            }
            else {

                var event = ko.utils.arrayFirst(ViewModel.events(), function (item) {
                    return item.Id() == eventId;
                });
                event.ProofFiles.push(ko.mapping.fromJS(data));
                event.IsDownloading(false);

            }

            $(this).closest(".modal-content").scrollTop(5000);


        },
        onUploadProgress: function (id, percent) {
            var eventId = $(this).children("input[type=hidden]").val();

            if (eventId == "0") {
                ViewModel.newEvent().Percent(percent + "%");
            }
            else {
                var event = ko.utils.arrayFirst(ViewModel.events(), function (item) {
                    return item.Id() == eventId;
                });

                event.Percent(percent + "%");
            }
        }
    });

}
