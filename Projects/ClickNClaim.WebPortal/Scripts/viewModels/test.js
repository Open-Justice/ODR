
var audio_context;
var recorder;
var recording = false;



function startUserMedia(stream) {
    var input = audio_context.createMediaStreamSource(stream);

    // Uncomment if you want the audio to feedback directly
    //input.connect(audio_context.destination);
    //__log('Input connected to audio context destination.');

    recorder = new Recorder(input);
}

function toggleRecording(button) {
    if (!recording) {
        startRecording(button);
    }
    else {
        stopRecording(button);
    }
    recording = !recording;
}




function createDownloadLink() {
    recorder && recorder.exportWAV(function(blob) {
        var url = URL.createObjectURL(blob);
        var xhr = new XMLHttpRequest;
        xhr.responseType = 'blob';
        xhr.onload = function(){
            var recoveredBlob = xhr.response;
            var reader = new FileReader;
            reader.onload = function(){
                var blobAsDataUrl = reader.result; // LZString.compressToUTF16(reader.result);

                $.ajax({
                    url : rootDir + "Recordings/SaveRecord",
                    data : {
                        url : blobAsDataUrl,
                        conflictId : conflictId,
                        eventId : eventId
                    },
                    method: 'POST',
                    type:'POST',
                    complete : function(data, status, xhr){
                        location.reload();
                    }
                
                })
          
            };
            reader.readAsDataURL(recoveredBlob);
            
        }
        xhr.open('GET', url);
        xhr.send();
        //$.ajax({
        //    url : rootDir + "Recordings/SaveRecord",
        //    data : {
        //        url : url,
        //        conflictId : conflictId,
        //        eventId : eventId
        //    },
        //    method : 'POST',
        //    complete : function(data, status, xhr){
        //        location.reload();
        //    }
                
        //})
    });
}

function startRecording(button) {
    recorder && recorder.record();
    //  button.disabled = true;
    //button.nextElementSibling.disabled = false;
    //   __log('Recording...');
}

function stopRecording(button) {
    recorder && recorder.stop();
    //   button.disabled = true;
    // button.previousElementSibling.disabled = false;
    //  __log('Stopped recording.');

    // create WAV download link using audio data blob
    createDownloadLink();




    recorder.clear();
}

$(document).ready(function () {
    try {

        ko.applyBindings(null, $("#wrapper")[0]);

        // webkit shim
        window.AudioContext = window.AudioContext || window.webkitAudioContext;
        navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia;
        window.URL = window.URL || window.webkitURL;

        audio_context = new AudioContext;
        if (!navigator.getUserMedia) {
            var i = $("div.recorder-content i");
            i.removeClass("fa-microphone");
            i.addClass("fa fa-microphone-slash");
        }

        navigator.getUserMedia({ audio: true }, startUserMedia, function (e) {
        });

    } catch (e) {
        var i = $("div.recorder-content i");
        i.removeClass("fa-microphone");
        i.addClass("fa fa-microphone-slash");
    }
})
