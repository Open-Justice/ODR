var ViewModel = null;
function viewModel(data) {
    var self = this;


    var visios = ko.utils.arrayFilter(data.Events, function (item) {
        return item.Type == 5;
    })
    for (var j = 0; j < visios.length; j++) {
        for (var k = 0; k < visios[j].MeetingDebates[0].MeetingPropositions.length; k++) {
            for (var i = 0; i < data.UsersInConflicts.length; i++) {
                if (data.UsersInConflicts[i].User.MeetingPropositionAnswers == null) {
                    data.UsersInConflicts[i].User.MeetingPropositionAnswers = [];
                    data.UsersInConflicts[i].User.MeetingPropositionAnswers.push({

                        IdMeetingProposition: visios[j].MeetingDebates[0].MeetingPropositions[k].Id,
                        IdUser: data.UsersInConflicts[i].IdUser,
                        Response: null,
                    });
                }

                if (data.UsersInConflicts[i].User.MeetingPropositionAnswers != null && ko.utils.arrayFirst(data.UsersInConflicts[i].User.MeetingPropositionAnswers, function (item) {
                    return item.IdMeetingProposition == visios[j].MeetingDebates[0].MeetingPropositions[k].Id;
                }) == null) {
                    data.UsersInConflicts[i].User.MeetingPropositionAnswers.push({

                        IdMeetingProposition: visios[j].MeetingDebates[0].MeetingPropositions[k].Id,
                        IdUser: data.UsersInConflicts[i].IdUser,
                        Response: null,
                    });
                }

            }
        }
    }

    self.data = ko.mapping.fromJS(data);
    self.events = ko.observableArray(self.data.Events());
    self.countDonwloadableItems = function (items) {
        var j = 0;
        for (var i = 0; i < items.length; i++) {
            j += items[i].Id != 0 ? 1 : 0;
        }
        return j;
    }
    self.emailList = ko.observableArray(['']);
    self.addTier = function (data, event) {
        self.emailList.push('');
    }
    self.typeOfDebat = ko.observable('');
    self.hasUnusedInvitations = function () {
        var res = true;
        for (var i = 0; i < self.data.Invitations.length; i++) {
            if (self.data.Invitations[i].ReadyForArbitration == null || self.data.Invitations[i].ReadyForArbitration == false) {
                res = false;
                break;
            }
        }
        return res;
    }
    self.partyName = ko.observable();
    self.setParty = function (partyName) {
        $("#partyName").val(partyName);
        self.partyName(partyName);
    }

    self.selectedEvent = ko.observable();
    self.startDebat = function (data, event) {
        self.selectedEvent(data.Id());
        $("#startdebateModal").openModal();
    }

    self.startDisagreement = function (data, event) {
        self.selectedEvent(data.Id());
        $("#official-disagreement").openModal();
    }

    self.openPropositionModal = function (data, event) {
        self.datePropositions.removeAll();
        self.datePropositions.push('');
        $("#planVisio").openModal();
        $('.rdv').bootstrapMaterialDatePicker({ format: 'DD/MM/YYYY HH:mm', lang: 'fr', weekStart: 1, cancelText: 'ANNULER' });
    }
    self.datePropositions = ko.observableArray(['']);
    self.addDateProposistion = function (data, event) {
        self.datePropositions.push('');
        $('.rdv').bootstrapMaterialDatePicker({ format: 'DD/MM/YYYY HH:mm', lang: 'fr', weekStart: 1, cancelText: 'ANNULER' });
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
            return 'visio-event-template';
        }
        else if (type == 6) {
            return 'classic-event-template';
        }
        else if (type == 8) {
            return 'classic-event-template';
        }
        else if (type == 9) {
            return 'official-event-template';
        }
    }
    self.saveEvent = function (data, event) {

        var answers = [];

        for (var i = 0; i < data.MeetingDebates()[0].MeetingPropositions().length; i++) {
            answers.push({
                IdMeetingProposition: data.MeetingDebates()[0].MeetingPropositions()[i].Id(),
                IdUser: userId,
                Response: $("#" + data.MeetingDebates()[0].MeetingPropositions()[i].Id() + "_" + userId).is(":checked")
            })
        }


        $.ajax({
            url: rootDir + "Conflict/SavePropositionAnswers",
            data: { eventId: data.Id(), answers: answers, conflictId : self.data.Id() },
            method: 'POST',
            complete: function (data, status, xhr) {
                location.reload();
            }
        })
    }
    self.addNewDateToEvent = function (data, event) {
        self.selectedEvent(data);
        $("#newDateModal").openModal();
    }
    self.setDate = function (data, event) {
        var divButton = null;
        if (event.target.nodeName == "P") {
            divButton = $(event.target).closest("div");
        }
        else {
            divButton = $(event.target);
        }

        Layout.showConfirm({
            header: "Êtes-vous sûr de vouloir fixer la date de la visio-conférence ?",
            text: "Vous êtes sur le point de fixer la date de la visio-conférence au " + divButton.find("p")[0].innerText + " à " + divButton.find("p")[1].innerText,
            confirmYes: function () {
                $.ajax({
                    url: rootDir + "Conflict/SetDateForVisio",
                    data: { idMeetingDebate: this.obj.IdMeetingDebate(), idMeetingProposition: this.obj.Id(), conflictId : self.data.Id() },
                    complete: function (data, status, xhr) {

                    }
                })
            },
            object: data,
            confirmNo: function () {

            }
        })
    }
    self.show = function (data) {
        if (data == "mail") {
            if (!$("div.mail").is(":visible")) {
                $("div.mail").removeClass("hidden");
                $("div.visio").addClass("hidden");
            }

        }
        else {
            if (!$("div.visio").is("visible")) {
                $("div.visio").removeClass("hidden");
                $("div.mail").addClass("hidden");
            }
        }
    }
    self.DownloadAndCreateAudio = function (data, event) {
        $.ajax({
            url: data.FilePath(),
            method: 'GET',
            type: 'GET',
            complete: function (data, status, xhr) {
                var audioFile = $(data.responseText)[0].data;
                var audio = $('<audio />', {
                    autoPlay: 'autoplay',
                    controls: 'controls'
                });
                audio.attr('src', audioFile);
                $(event.target).closest("div").append(audio);
            }
        })
    }

    self.updateStateId = ko.observable();
    self.updateStateName = ko.observable();
    self.needsCountDown = ko.observable(false);
    self.countDown = ko.observable('');

    if (data.ConflictStateHistorics.length > 0 && data.ConflictStateHistorics[data.ConflictStateHistorics.length - 1].CountDown != null) {
        setInterval(function () {
            var begin = moment().unix();
            var end = moment(data.ConflictStateHistorics[data.ConflictStateHistorics.length - 1].CreateDate).add(data.ConflictStateHistorics[data.ConflictStateHistorics.length - 1].CountDown, 'days').unix();
            var duration = moment.duration((end - begin) , 'seconds');
           
            self.countDown(duration.days()+'j ' + duration.hours() +':'+duration.minutes() +':'+ duration.seconds());
        }, 1000);
    }


    self.updateState = function (id, name, hasCountdown) {
        self.updateStateId(id);
        self.updateStateName(name);
        self.needsCountDown(hasCountdown);
        $("#updateStateModal").openModal();
    }


    self.newEvent = ko.observable(new eventElt(data.Id));
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
            url: rootDir + "Viewer/AddNewEvent",
            data: ko.mapping.toJS(self.newEvent),
            method: "POST",
            complete: function (data, status, xhr) {
                data.responseJSON.DateBegin = moment(data.responseJSON.DateBegin).format("MM/DD/YYYY");
                if (isToUpdate) {
                    ViewModel.data.events.remove(function (item) {
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

        self.newEvent(new eventElt(ViewModel.data.Id));
    }
    self.clean = function (data, event) {
        self.updateIdx = null;
        self.selectedEvent(null);
        $("#addNewEventModal").closeModal();
    }
    self.uploadFileToNewEvent = function (data, event) {
        $(event.target).closest(".modal-content").children(".drop-area-div").find("input[type=file]").click();
    }

    self.newDisagreement = ko.observable(ko.mapping.fromJS(new disagreementElt()));
    self.startDebate = function (data, event) {
        self.debateEvt(data);
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

        $.ajax({
            url: rootDir + "Action/AddAction",
            data: disagreement,
            method: 'POST',
            complete: function (d, e) {
                var event = ko.utils.arrayFirst(self.events(), function (item) {
                    return item.Id() == self.debateEvt().Id();
                });
                event.Disagreements.removeAll();
                //    d.responseJSON.ConcurrentDate = moment(d.responseJSON.ConcurrentDate).format('L');
                event.Disagreements.push(ko.mapping.fromJS(d.responseJSON));
                self.clean();
            }
        })
    }

    self.updatableCompany = ko.observable();
    self.openUpdateCompany = function (idUser) {
        var uic = ko.utils.arrayFirst(self.data.UsersInConflicts(), function (item) {
            return item.IdUser() == idUser;
        })
        if (ko.isObservable(uic.UserCompany) || !uic.UserCompany) {
            uic.UserCompany =ko.mapping.fromJS({ Company: { Id :0, Name: '', Address1: '', Address2: '', Address3: '', PostalCode: '', City: '', TelCompany: '', Siret: '', RCS: '', Capital : '', Forme : '' } });
        }
        if (uic.UserCompany.Company.Name().length <= 0) {
            uic.UserCompany.Company.Name(uic.CompanyName());
        }
        self.updatableCompany(uic.UserCompany.Company);
        $("#companyUpdateUserId").val(idUser);
        $("#companyInfoModal").openModal();
    }

    self.isIFrameLoading = ko.observable(false);
    self.showDoc = function (filePath) {
        self.isIFrameLoading(true);
        var modal = $("#ViewDocument #iFrameContainer");
        modal.empty();
        var newDoc = "<iframe style='width:100%;height:100%;' src='https://docs.google.com/gview?url=" + filePath() + "&embedded=true' onload='disableLoading()'  />";
        modal.append(newDoc);
        $("#ViewDocument").openModal();
    }

    ViewModel = self;
}

function disableLoading() {
    ViewModel.isIFrameLoading(false);
}

function smoothScroll(name) {
    var target = $("#" + name);
    if (target.length) {
        $('html, body').animate({
            scrollTop: target.offset().top - (($(window).height() - 64) / 2)
        }, 1000);
        return false;
    }
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

function disagreementElt() {
    var self = this;
    self.Comment = ko.observable('');
    self.ConcurrentDate = ko.observable();
    self.ConcurrentDescription = ko.observable('');
    self.ConcurrentPieces = ko.observable('');
    self.CreatedBy = ko.observable();
    self.DisagreementOnDate = ko.observable(false);
    self.DisagreementOnDescription = ko.observable(false);
    self.DisagreementOnPiece = ko.observable(false);
    self.Event = ko.observable();
    self.Id = ko.observable(0);
    self.IdEvent = ko.observable(0);
    self.IdUser = ko.observable();
    self.IsResolved = ko.observable(false);
}



function isInMeetingDebate(debate, elt) {
    var ids = ko.utils.arrayMap(debate.MeetingPropositions(), function (item) {
        return item.Id();
    })
    if (ids.indexOf(elt) >= 0) {
        return true;
    }
    else
        return false;
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

    $('.rdv').bootstrapMaterialDatePicker({ format: 'DD/MM/YYYY HH:mm', lang: 'fr', weekStart: 1, cancelText: 'ANNULER' });
    $('select').material_select();
    $('.modal-trigger').leanModal({
        ready: function () {
            ViewModel.newEvent(new eventElt(ViewModel.data.Id()));
            $('input.date').bootstrapMaterialDatePicker({ format: 'DD/MM/YYYY', lang: 'fr', weekStart: 1, cancelText: 'ANNULER', time: false });

            makeDownloadAvailable();
        }
    });
}

function makeDownloadAvailable() {
    $(".drop-area-div").dmUploader({
        url: rootDir + 'Upload/UploadFile',
        extraData: {
            'conflictId': ViewModel.data.Id()
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
                'conflictId': ViewModel.data.Id(),
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