$(document).ready(function () {
    $("input[type=checkbox]").on('change', function () {
        if ($("input[type=hidden]#RememberMe").val() == "false") {
            $("input[type=hidden]#RememberMe").val("true");
        }
        else {
            $("input[type=hidden]#RememberMe").val("false");
        }
    });

    $("#Email").blur(function () {
        $("#Email").val($("#Email").val().split(' ').join(''));
        resetValidation();
    })


    function resetValidation() {
        $('.field-validation-valid').html("");
        $(".field-validation-error").html("");
    }

})